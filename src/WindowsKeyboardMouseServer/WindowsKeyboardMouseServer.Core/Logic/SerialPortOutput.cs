using System.IO.Ports;
using System.Text;

namespace WindowsKeyboardMouseServer.Core.Logic;

public class SerialPortOutput
{
    private SerialPort _serialPort;

    public SerialPortOutput(string portName, int baudRate)
    {
        _serialPort ??= new SerialPort(portName, baudRate);
        
        _serialPort.Open();
        
        _serialPort.Encoding = Encoding.ASCII;
    }
    
    public void SendDataOverSerialPort(string data)
    {
        _serialPort.WriteLine(data);
    }

    public void CloseSerialPort()
    {
        _serialPort.Close();
    }
}