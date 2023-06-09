﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using WindowsKeyboardMouseServer.Core.Logic;
using WindowsKeyboardMouseServer.Core.Models;
using WindowsKeyboardMouseServer.UI.WindowResources.SettingsWindowResources;

namespace WindowsKeyboardMouseServer.UI.WindowResources.MainWindowResources;

/// <summary>
/// The ViewModel for MainWindow
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly ILogger _logger;
    private readonly ISettingsApplicationLocal _settingsApplicationLocal;
    private readonly SettingsWindow _settingsWindow;
    private readonly SettingsWindowViewModel _settingsWindowViewModel;
    private SerialDataSender? _serialDataSender;

    private bool IsTrapEnabled { get; set; }
    private bool _artificialMovement;
    
    private Vector _movement;
    private Point _cursorResetPosition;

    private Button? _mouseTrapButton;
    private SerialPortOutput? _serialPortOutput;
    private readonly KeyboardListener _keyboardListener;
    
    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="logger">Injected ILogger to use</param>
    /// <param name="settingsApplicationLocal">Application settings handled by config.net</param>
    /// <param name="settingsWindow">Settings window</param>
    /// <param name="settingsWindowViewModel">Settings window view model</param>
    public MainWindowViewModel(
        ILogger logger,
        ISettingsApplicationLocal settingsApplicationLocal, 
        SettingsWindow settingsWindow, 
        SettingsWindowViewModel settingsWindowViewModel)
    {
        _logger = logger;
        _settingsApplicationLocal = settingsApplicationLocal;
        _settingsWindow = settingsWindow;
        _settingsWindowViewModel = settingsWindowViewModel;
        
        _settingsWindow.DataContext = _settingsWindowViewModel;
        
        if (!LastSelectedPortIsValid())
        {
            PromptUserForNewPort();
        }
        
        _keyboardListener = new KeyboardListener();
        
        _keyboardListener.KeyDown += KeyboardHookKeyDown;
        _keyboardListener.KeyUp += KeyboardHookKeyUp;
    }

    private void PromptUserForNewPort()
    {
        var portName = "";
        
        if (_serialPortOutput is not null && _serialPortOutput.IsOpen)
            _serialPortOutput.CloseSerialPort();
        
        MessageBox.Show(
            $@"""Couldn't open serial port: {portName}

                        Please select a new port and press save.""");
         
        _settingsWindow.ShowDialog();

        if (!_settingsWindowViewModel.UserCancelled)
            portName = _settingsWindowViewModel.SelectedPort;
        else
        {
            MessageBox.Show(
                $@"""User cancelled with no valid port selected. Application will now close.""");
                
            Environment.Exit(0);
        }
        
        try
        {
            _serialPortOutput = new SerialPortOutput(_settingsApplicationLocal.LastSelectedComPort);
        
            _serialDataSender = new SerialDataSender(_serialPortOutput);
        }
        catch (FileNotFoundException)
        {
            MessageBox.Show(
                $@"""Couldn't open new serial port: {portName}. Application will now close.""");
            
            Environment.Exit(0);
        }
    }

    private bool LastSelectedPortIsValid()
    {
        try
        {
            if (_serialPortOutput?.IsOpen ?? false)
                _serialPortOutput.CloseSerialPort();

            _serialPortOutput = new SerialPortOutput(_settingsApplicationLocal.LastSelectedComPort);

            _serialDataSender = new SerialDataSender(_serialPortOutput);    
            
            return true;
        }
        catch
        {
            return false;    
        }
    }

    [RelayCommand]
    private void StartMainSetupProcess()
    {
        _logger.Information("Running {ThisName}", System.Reflection.MethodBase.GetCurrentMethod()?.Name);
    }
    
    /// <summary>
    /// Starts trapping mouse/keyboard events
    /// </summary>
    private void SetTrapState(bool isEnabled)
    {
        IsTrapEnabled = isEnabled;
        
        if (isEnabled)
            _keyboardListener.StartHandlingKeypresses();
        else
            _keyboardListener.StopHandlingKeypresses();
        
        Mouse.OverrideCursor = Cursors.None;
    }
    
    [RelayCommand]
    private void ButtonLoaded(object source)
    {
        var routedEventArgsSource = ((RoutedEventArgs)source).Source;

        if (routedEventArgsSource is null) return;
        
        _mouseTrapButton ??= (Button)routedEventArgsSource;
    }
    
    [RelayCommand]
    private void MouseUp(MouseButtonEventArgs e)
    {
        if (!IsTrapEnabled) return;
        
        _serialDataSender?.SendMouseEvent(e, true);
        e.Handled = true;
    }

    [RelayCommand]
    private void MouseDown(MouseButtonEventArgs e)
    {
        if (IsTrapEnabled)
        {
            _serialDataSender?.SendMouseEvent(e, false);
            e.Handled = true;
            return;
        }
        
        MoveMouseToCenter(_mouseTrapButton);

        SetTrapState(true);
    }
    
    [RelayCommand]
    private void MouseMove(MouseEventArgs e)
    {
        if (!IsTrapEnabled || _artificialMovement) return;
        
        MeasurePositionDelta(e);

        MoveMouseToCenter(_mouseTrapButton);
    }
    
    private void KeyboardHookKeyDown(object sender, RawKeyEventArgs keyArgs)
    {
        _serialDataSender?
            .DebugWriteKeyEvent(keyArgs.Key, true);
        
        if (!IsTrapEnabled) return;
        
        if (keyArgs.Key == Key.Escape)
        {
            IsTrapEnabled = false;
            Environment.Exit(0);
            return;
        }
        
        _serialDataSender?.SendKeyEvent(keyArgs.Key, false);
    }
    
    private void KeyboardHookKeyUp(object sender, RawKeyEventArgs keyArgs)
    {
        _serialDataSender?
            .DebugWriteKeyEvent(keyArgs.Key, true); 
        
        if (!IsTrapEnabled) return;
        
        _serialDataSender?.SendKeyEvent(keyArgs.Key, true);
    }
    
    [RelayCommand]
    private void OpenSettingsWindow()
    {
        

    }

    private MainWindow GetParentWindowFromButton(FrameworkElement buttonToGetParentOf)
    {
        var currentElement = buttonToGetParentOf;

        while (currentElement is not MainWindow)
        {
            currentElement = (FrameworkElement)currentElement.Parent;
        }

        return (MainWindow)currentElement;
    }

    private void MeasurePositionDelta(MouseEventArgs e)
    {
        if (_mouseTrapButton is null) return;
        
        var newMousePosition = _mouseTrapButton.PointToScreen(e.GetPosition(_mouseTrapButton));
        
        _movement = newMousePosition - _cursorResetPosition;

        int integerMovementX;
        int integerMovementY;

        if (_movement.X >= 0)
        {
            integerMovementX = (int)Math.Ceiling(_movement.X);
        }
        else
        {
            integerMovementX = (int)Math.Floor(_movement.X);
        }
        
        if (_movement.Y >= 0)
        {
            integerMovementY = (int)Math.Ceiling(_movement.Y + 0.5);
        }
        else
        {
            integerMovementY = (int)Math.Floor(_movement.Y + 0.5);
        }

        //Debug.WriteLine($"sending: {integerMovementX}, {integerMovementY}");
        
        _serialDataSender?.SendMouseMove(integerMovementX, integerMovementY);
    }

    private void MoveMouseToCenter(object? sender)
    {
        if (sender is null || _mouseTrapButton is null) return;

        _artificialMovement = true;
        
        var parentWindow = GetParentWindowFromButton((Button)sender);
        
        _cursorResetPosition = 
            new Point( parentWindow.Left + _mouseTrapButton.ActualWidth / 2,
                parentWindow.Top + _mouseTrapButton.ActualHeight / 2);
        
        //Debug.WriteLine($"cursorResetPosition: {_cursorResetPosition}");
        
        SetCursorPos((int)_cursorResetPosition.X, (int)_cursorResetPosition.Y);
        
        // Reset for next mouse movement
        _movement = new Vector(0, 0);
        
        _artificialMovement = false;
    }

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);
}