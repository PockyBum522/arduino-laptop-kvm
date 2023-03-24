using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

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
    private void StartMainSetupProcess()
    {
        _logger.Information("Running {ThisName}", System.Reflection.MethodBase.GetCurrentMethod()?.Name);
    }
}