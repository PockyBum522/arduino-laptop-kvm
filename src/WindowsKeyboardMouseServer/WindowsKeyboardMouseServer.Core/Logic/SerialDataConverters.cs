using System;

namespace WindowsKeyboardMouseServer.Core.Logic;

public class SerialDataConverters
{
    private readonly SerialPortOutput _serialPortOutput;

    public SerialDataConverters(SerialPortOutput serialPortOutput)
    {
        _serialPortOutput = serialPortOutput;
    }
    
    public void SendMouseMove(int x, int y)
    {
        var convertedX = x + 50;
        var convertedY = y + 50;
        
        _serialPortOutput.SendDataOverSerialPort($"m:x{convertedX},y{convertedY};");
    }
    
    public void SendMouseClick(MouseButton buttonToClick)
    {
        switch (buttonToClick)
        {
            case MouseButton.Left:
                _serialPortOutput.SendDataOverSerialPort("c:0;");
                break;
            
            case MouseButton.Middle:
                _serialPortOutput.SendDataOverSerialPort("c:1;");
                break;
            
            case MouseButton.Right:
                _serialPortOutput.SendDataOverSerialPort("c:2;");
                break;
        }
    }
    
    public void SendKey(char key)
    {
        _serialPortOutput.SendDataOverSerialPort($"k:x{key};");
    }
}

public enum MouseButton
{
    Uninitialized,
    Left,
    Middle,
    Right
}