using System;
using System.Diagnostics;
using System.Windows.Input;

namespace WindowsKeyboardMouseServer.Core.Logic;

/// <summary>
/// Handles setting up data in the formats that the microcontroller expects to see and sending it over a provided
/// SerialPortOutput
/// </summary>
public class SerialDataConverters
{
    private readonly SerialPortOutput _serialPortOutput;

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="serialPortOutput">Injected SerialPortOutput to use and send data on</param>
    public SerialDataConverters(SerialPortOutput serialPortOutput)
    {
        _serialPortOutput = serialPortOutput;
    }

    /// <summary>
    /// Sets up and sends mouse button clicks
    /// </summary>
    /// <param name="e">MouseButtonEventArgs from the event, so we know what button did something</param>
    /// <param name="buttonState">If we should send that the button was pressed or released</param>
    public void SendMouseEvent(MouseButtonEventArgs e, bool released)
    {
        //Debug.WriteLine("STATE: " + e.ButtonState);

        string serialFormattedString = "";
        
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
    
    /// <summary>
    /// Sets up and sends keys
    /// </summary>
    public void SendKeyEvent(char key, bool released)
    {
        var serialString = $"k:{key},";
        
        if (released)
            serialString += $"1;";
        else
            serialString += $"0;";
        
        _serialPortOutput.SendDataOverSerialPort(serialString);
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
}