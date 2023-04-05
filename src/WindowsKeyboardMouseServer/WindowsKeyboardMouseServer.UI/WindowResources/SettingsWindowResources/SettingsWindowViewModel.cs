using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using WindowsKeyboardMouseServer.Core.Models;

namespace WindowsKeyboardMouseServer.UI.WindowResources.SettingsWindowResources;

/// <summary>
/// ViewModel for the SettingsWindow.
/// </summary>
public partial class SettingsWindowViewModel : ObservableObject
{
    private readonly ISettingsApplicationLocal _settingsApplicationLocal;

    /// <summary>
    /// What port the user last selected
    /// </summary>
    public string SelectedPort
    {
        get { return _selectedPort;}
        set
        {
            _settingsApplicationLocal.LastSelectedComPort = value;

            _selectedPort = value;
        }
    }
    
    [ObservableProperty]
    private ObservableCollection<string> _availablePorts;
   
    [ObservableProperty]
    private bool _isCtrlChecked;

    [ObservableProperty]
    private bool _isAltChecked;

    [ObservableProperty]
    private bool _isSuperChecked;

    [ObservableProperty]
    private bool _isShiftChecked;

    [ObservableProperty]
    private bool _userCancelled = true;

    private string _selectedPort;

    /// <summary>
    /// Initializes a new instance of the SettingsViewModel class.
    /// </summary>
    public SettingsWindowViewModel(ISettingsApplicationLocal settingsApplicationLocal)
    {
        _settingsApplicationLocal = settingsApplicationLocal;
        
        // Initialize available ports and other properties as needed
        _availablePorts = new ObservableCollection<string>();

        foreach (var portName in GetAvailableComPorts())
        {
            _availablePorts.Add(portName);
        }

        _selectedPort = "";
    }

    /// <summary>
    /// Executes the logic to save the settings.
    /// </summary>
    [RelayCommand]
    private void Save(object parameter)
    {
        UserCancelled = false;
        
        if (parameter is Window window)
        {
            window.Close();
        }
    }

    /// <summary>
    /// Executes the logic to cancel the settings changes.
    /// </summary>
    [RelayCommand]
    private void Cancel(object parameter)
    {
        UserCancelled = true;
        
        if (parameter is Window window)
        {
            window.Close();
        }
    }
    
    private ObservableCollection<string> GetAvailableComPorts()
    {
        var availablePorts = new ObservableCollection<string>();
        string[] portNames = SerialPort.GetPortNames();

        // Extract port numbers and sort them
        var sortedPortNumbers = portNames
            .Where(name => name.StartsWith("COM"))
            .Select(name => int.TryParse(name.AsSpan(3), out var portNumber) ? portNumber : (int?)null)
            .Where(portNumber => portNumber.HasValue)
            .OrderBy(portNumber => portNumber)
            .ToArray();

        // Add sorted port numbers to the ObservableCollection
        foreach (var portNumber in sortedPortNumbers)
        {
            availablePorts.Add($"COM{portNumber}");
        }

        return availablePorts;
    }
}