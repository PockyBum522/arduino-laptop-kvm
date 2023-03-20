using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using WindowsKeyboardMouseServer.Core.Logic.MainProcessExecutors;
using WindowsKeyboardMouseServer.Core.Logic.MainWindowLoaders;
using WindowsKeyboardMouseServer.Core.Models;
using WindowsKeyboardMouseServer.Core.Models.Enums;
using WindowsKeyboardMouseServer.UI.WindowResources.InstallsEditor;
using WindowsKeyboardMouseServer.UI.WpfHelpers;

namespace WindowsKeyboardMouseServer.UI.WindowResources.MainWindow;

/// <summary>
/// The ViewModel for MainWindow
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly ILogger _logger;
    
    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="logger">Injected ILogger to use</param>
    public MainWindowViewModel(
        ILogger logger)
    {
        _logger = logger;
    }

    [RelayCommand]
    private async Task StartMainSetupProcess()
    {
        _logger.Information("Running {ThisName}", System.Reflection.MethodBase.GetCurrentMethod()?.Name);
           
    }
    
    [RelayCommand] 
    private void ReloadInstallerList() => _availableApplicationsJsonLoader.LoadAvailableInstallersFromJsonFile();
}