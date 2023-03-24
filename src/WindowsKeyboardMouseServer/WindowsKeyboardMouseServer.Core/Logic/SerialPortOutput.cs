﻿using System.IO.Ports;
using System.Text;

namespace WindowsKeyboardMouseServer.Core.Logic;

/// <summary>
/// Handles setting up a serial port connection, keeping the connection open, and sending data over serial port
/// </summary>
public class SerialPortOutput
{
    private SerialPort _serialPort;

    /// <summary>
    /// Sets up the serial port and opens the connection when SerialPortOutput is created
    /// </summary>
    /// <param name="portName">Port name, formatted like: COM7</param>
    /// <param name="baudRate">Baud rate, defaults to 115200</param>
    public SerialPortOutput(string portName, int baudRate = 115200)
    {
        _serialPort ??= new SerialPort(portName, baudRate);
        
        _serialPort.Open();
        
        _serialPort.Encoding = Encoding.ASCII;
    }
    
    /// <summary>
    /// Sends data over the serial port 
    /// </summary>
    /// <param name="data">String to send over the serial port</param>
    public void SendDataOverSerialPort(string data)
    {
        _serialPort.Write(data);
    }

    /// <summary>
    /// Closes the serial port connection
    /// </summary>
    public void CloseSerialPort()
    {
        _serialPort.Close();
    }
}