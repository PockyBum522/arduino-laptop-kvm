using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;

namespace WindowsKeyboardMouseServer.Core.Logic;

/// <summary>
/// Handles setting up data in the formats that the microcontroller expects to see and sending it over a provided
/// SerialPortOutput
/// </summary>
public class SerialDataSender
{
    private readonly SerialPortOutput _serialPortOutput;
    private readonly KeyConverter _keyConverter;

    private readonly Key[] _keysWithNoAsciiCode;

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="serialPortOutput">Injected SerialPortOutput to use and send data on</param>
    public SerialDataSender(SerialPortOutput serialPortOutput)
    {
        _serialPortOutput = serialPortOutput;
        
        _keyConverter = new();

        _keysWithNoAsciiCode = GetKeysWithNoAsciiCode();
    }

    /// <summary>
    /// Sets up and sends mouse button clicks
    /// </summary>
    /// <param name="e">MouseButtonEventArgs from the event, so we know what button did something</param>
    /// <param name="released">True if the button should be released, false if should be pressed</param>
    public void SendMouseEvent(MouseButtonEventArgs e, bool released)
    {
        //Debug.WriteLine("STATE: " + e.ButtonState);

        var serialFormattedString = "";
        
        switch (e.ChangedButton)
        {
            case MouseButton.Left:
                serialFormattedString += "c:1,";
                break;
            
            case MouseButton.Right:
                serialFormattedString += "c:2,";
                break;
        }

        if (released)
            serialFormattedString += $"1;";
        else
            serialFormattedString += $"0;";
        
        _serialPortOutput.SendDataOverSerialPort(serialFormattedString);
    }

    
    [DllImport("user32.dll")]
    public static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)] StringBuilder pwszBuff, int cchBuff, uint wFlags);
    
    private int? KeyToAscii(Key key)
    {
        if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
        {
            key = KeyInterop.KeyFromVirtualKey(KeyInterop.VirtualKeyFromKey(key) | 0x0100);
        }

        var virtualKey = KeyInterop.VirtualKeyFromKey(key);
        var keyboardState = new byte[256];
        
        var buffer = new StringBuilder();
        var result = ToUnicode((uint)virtualKey, 0, keyboardState, buffer, 2, 0);

        if (result == 1 || result == 2)
        {
            return buffer[0];
        }

        return null;
    }
    
    /// <summary>
    /// Sets up and sends keys
    /// </summary>
    /// <param name="key">Key code to send</param>
    /// <param name="released">True if the key should be released, false if should be pressed</param>
    public void SendKeyEvent(Key key, bool released)
    {
        var serialString = "";
        
        var keyCode = KeyToAscii(key);

        if (keyCode is > 64 and < 91)
            keyCode += 32;  // Convert to lowercase
        
        if (IsKeyWithNoAsciiCode(key))
        {
            serialString = $"s:{keyCode.ToString()},";
        }
        else
        {
            serialString = $"k:{keyCode.ToString()},";
        }
        
        if (released)
            serialString += $"1;";
        else
            serialString += $"0;";

        DebugWriteKeyEvent(key, released);
        
        _serialPortOutput.SendDataOverSerialPort(serialString);
    }

    /// <summary>
    /// Writes the key event to debug with helpful information
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="released">Whether or not it's being released</param>
    public void DebugWriteKeyEvent(Key key, bool released)
    {
        var keyCode = KeyToAscii(key);
        
        Debug.WriteLine($"Virtual KeyCode: {keyCode} | Down: {!released} | Friendly: {key.ToString()}");
    }
    
    /// <summary>
    /// Sets up and sends mouse movements
    /// </summary>
    public void SendMouseMove(int xAmount, int yAmount)
    {
        var convertedX = xAmount + 50;
        var convertedY = yAmount + 50;

        var serialString = $"m:x{convertedX.ToString()},y{convertedY.ToString()};";
        
        //Debug.WriteLine($"Sending over serial: {serialString}");
        
        _serialPortOutput.SendDataOverSerialPort(serialString);
    }
    
    private Key[] GetKeysWithNoAsciiCode()
    {
        // These correspond to what's in key_handlers.ino in the switch statement in sendKeyboardKeyWithNoAsciiCode()
        
        return new[]
        {
            Key.F1,
            Key.F2,
            Key.F3,
            Key.F4,
            Key.F5,
            Key.F6,
            Key.F7,
            Key.F8,
            Key.F9,
            Key.F10,
            Key.F11,
            Key.F12,
            Key.Up,
            Key.Down,
            Key.Left,
            Key.Right,
            Key.LeftCtrl,
            Key.RightCtrl,
            Key.LeftShift,
            Key.RightShift,
            Key.LeftAlt,
            Key.RightAlt,
            Key.CapsLock,
            Key.Apps,
            Key.Delete,
            Key.Tab,
            Key.Return,
            Key.LWin,
            Key.RWin
        };
    }

    private bool IsKeyWithNoAsciiCode(Key key)
    {
        return _keysWithNoAsciiCode.Contains(key);
    }
}