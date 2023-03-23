using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using WindowsKeyboardMouseServer.Core.Logic;

namespace WindowsKeyboardMouseServer.UI.WindowResources.MainWindow;

/// <summary>
/// The ViewModel for MainWindow
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly ILogger _logger;
    private readonly SerialPortOutput _serialPortOutput;
    private readonly SerialDataConverters _serialDataConverters;
    
    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="logger">Injected ILogger to use</param>
    public MainWindowViewModel(
        ILogger logger)
    {
        _logger = logger;
        
        _serialPortOutput = new SerialPortOutput("COM3", 115200);

        _serialDataConverters = new SerialDataConverters(_serialPortOutput);
    }

    [RelayCommand]
    private void StartMainSetupProcess()
    {
        _logger.Information("Running {ThisName}", System.Reflection.MethodBase.GetCurrentMethod()?.Name);
    }

    [RelayCommand]
    private void SendHello()
    {
        _serialDataConverters.SendKey('H');
        _serialDataConverters.SendKey('e');
        _serialDataConverters.SendKey('l');
        _serialDataConverters.SendKey('l');
        _serialDataConverters.SendKey('o');
        _serialDataConverters.SendKey('.');
    }
    
    [RelayCommand]
    private void SendMouseMove()
    {
        MouseSquare(200);
    }

    private void MouseSquare(int amountToMove)
    {
        for (var i = 0; i < amountToMove; i++)
        {
            _serialDataConverters.SendMouseMove(-1, 0);

            Thread.Sleep(1);
        }


        for (var i = 0; i < amountToMove; i++)
        {
            _serialDataConverters.SendMouseMove(0, 1);

            Thread.Sleep(1);
        }


        for (var i = 0; i < amountToMove; i++)
        {
            _serialDataConverters.SendMouseMove(1, 0);

            Thread.Sleep(1);
        }

        for (var i = 0; i < amountToMove; i++)
        {
            _serialDataConverters.SendMouseMove(0, -1);

            Thread.Sleep(1);
        }
    }

    [RelayCommand]
    private void SendMouseMoveSmall()
    {
        MouseSquare(20);
    }

    [RelayCommand]
    private void SendMouseLeftClick()
    {
        _serialDataConverters.SendMouseClick(MouseButton.Left);
    }
    
    [RelayCommand]
    private void SendMouseRightClick()
    {
        _serialDataConverters.SendMouseClick(MouseButton.Right);
    }

    [RelayCommand]
    private void MoveMouseUp()
    {
        _serialDataConverters.SendMouseMove(0, -1);
    }

    [RelayCommand]
    private void MoveMouseRight()
    {
        _serialDataConverters.SendMouseMove(1, 0);
    }

    [RelayCommand]
    private void MoveMouseDown()
    {
        _serialDataConverters.SendMouseMove(0, 1);
    }

    [RelayCommand]
    private void MoveMouseLeft()
    {
        _serialDataConverters.SendMouseMove(-1, 0);
    }
}