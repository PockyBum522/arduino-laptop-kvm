using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Xaml.Behaviors;
using WindowsKeyboardMouseServer.Core.Logic;
using Point = System.Windows.Point;

namespace WindowsKeyboardMouseServer.UI.WindowResources;

/// <summary>
/// Handles starting mouse/keyboard trap, and sending mouse events/movement, and keyboard events over serial
/// </summary>
[ObservableObject]
public partial class MouseTrapButtonBehavior : Behavior<Button>
{
    private readonly SerialDataConverters _serialDataConverters;
    
    private bool _isTrapEnabled;
    private bool _artificialMovement;
    
    private Vector _movement;
    private Point _cursorResetPosition;

    private KeyCodeToCharConverter _keyCodeToCharConverter;

    /// <summary>
    /// Handles starting mouse/keyboard trap
    /// </summary>
    public MouseTrapButtonBehavior()
    {
        var serialPortOutput = new SerialPortOutput("COM3");

        _serialDataConverters = new SerialDataConverters(serialPortOutput);

        _keyCodeToCharConverter = new();
    }
    
    /// <summary>
    /// Starts trapping mouse/keyboard events
    /// </summary>
    public void ActivateTrap(bool isEnabled)
    {
        _isTrapEnabled = isEnabled;
        
        Mouse.OverrideCursor = Cursors.None;
    }

    [RelayCommand]
    private void MouseUp(MouseButtonEventArgs e)
    {
        if (!_isTrapEnabled) return;
        
        _serialDataConverters.SendMouseEvent(e, true);
        e.Handled = true;
    }

    [RelayCommand]
    private void MouseDown(MouseButtonEventArgs e)
    {
        if (_isTrapEnabled)
        {
            _serialDataConverters.SendMouseEvent(e, false);
            e.Handled = true;
            return;
        }
        
        MoveMouseToCenter(AssociatedObject);

        ActivateTrap(true);
    }
    
    [RelayCommand]
    private void MouseMove(MouseEventArgs e)
    {
        if (!_isTrapEnabled || _artificialMovement) return;
        
        MeasurePositionDelta(e);

        MoveMouseToCenter(AssociatedObject);
    }
    
    [RelayCommand]
    private void KeyDown(KeyEventArgs keyEventArgs)
    {
        if (keyEventArgs.Key != Key.Escape) return;

        _isTrapEnabled = false;
        
        Mouse.OverrideCursor = Cursors.Arrow;
    }
    
    [RelayCommand]
    private void KeyUp(KeyEventArgs keyEventArgs)
    {
        var charToSend = _keyCodeToCharConverter.KeyCodeToUnicode(keyEventArgs.Key);
        
        Debug.WriteLine($"CHAR: {charToSend}");
        
        //_serialDataConverters.SendKeyEvent(keyEventArgs.);
    }


    private MainWindow.MainWindow GetParentWindowFromButton(FrameworkElement buttonToGetParentOf)
    {
        var currentElement = buttonToGetParentOf;

        while (currentElement is not MainWindow.MainWindow)
        {
            currentElement = (FrameworkElement)currentElement.Parent;
        }

        return (MainWindow.MainWindow)currentElement;
    }

    private void MeasurePositionDelta(MouseEventArgs e)
    {
        var newMousePosition = AssociatedObject.PointToScreen(e.GetPosition(AssociatedObject));
        
        _movement = newMousePosition - _cursorResetPosition;

        //Debug.WriteLine($"Movement: {_movement.X}, {_movement.Y}");
        
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
        _serialDataConverters.SendMouseMove(integerMovementX, integerMovementY);
    }

    private void MoveMouseToCenter(object sender)
    {
        _artificialMovement = true;
        
        var parentWindow = GetParentWindowFromButton((Button)sender);
        
        _cursorResetPosition = 
            new Point( parentWindow.Left + AssociatedObject.ActualWidth / 2,
                parentWindow.Top + AssociatedObject.ActualHeight / 2);
        
        //Debug.WriteLine($"cursorResetPosition: {_cursorResetPosition}");
        
        SetCursorPos((int)_cursorResetPosition.X, (int)_cursorResetPosition.Y);
        
        // Reset for next mouse movement
        _movement = new Vector(0, 0);
        
        _artificialMovement = false;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);
}