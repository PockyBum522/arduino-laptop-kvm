using System.Windows.Interop;

namespace WindowsKeyboardMouseServer.UI.WindowResources.MainWindowResources;

///<summary>
///Interaction logic for MainWindow.xaml
///</summary>
public partial class MainWindow
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private HwndSource _source;

    /// <summary>
    /// Main window constructor
    /// </summary>
    public MainWindow(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
        
        InitializeComponent();
    }
}