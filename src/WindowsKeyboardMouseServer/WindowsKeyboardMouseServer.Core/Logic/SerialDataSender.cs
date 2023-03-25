using System.Windows.Input;

namespace WindowsKeyboardMouseServer.Core.Logic;

/// <summary>
/// Handles setting up data in the formats that the microcontroller expects to see and sending it over a provided
/// SerialPortOutput
/// </summary>
public class SerialDataSender
{
    private readonly SerialPortOutput _serialPortOutput;

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="serialPortOutput">Injected SerialPortOutput to use and send data on</param>
    public SerialDataSender(SerialPortOutput serialPortOutput)
    {
        _serialPortOutput = serialPortOutput;
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

    /// <summary>
    /// Sets up and sends keys
    /// </summary>
    /// <param name="key">Key code to send</param>
    /// <param name="released">True if the key should be released, false if should be pressed</param>
    public void SendKeyEvent(int key, bool released)
    {
        var serialString = $"k:{key.ToString()},";
        
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