using System.IO.Ports;

namespace DCCEXDotnet;

/// <summary>
/// Serial port implementation of IStream interface
/// </summary>
public class SerialPortStream : IStream, IDisposable
{
    private SerialPort _serialPort;

    public SerialPortStream(string portName, int baudRate = 115200)
    {
        //TODO - set timeouts?
        _serialPort = new SerialPort(portName, baudRate);
        _serialPort.Open();
    }

    public int Available()
    {
        return _serialPort.BytesToRead;
    }

    public int Read()
    {
        if (_serialPort.BytesToRead > 0)
        {
            return _serialPort.ReadByte();
        }
        return -1;
    }

    public void WriteLine(string text)
    {
        _serialPort.WriteLine(text);
    }

    public void Print(string text)
    {
        _serialPort.Write(text);
    }

    public void Println(string text)
    {
        _serialPort.WriteLine(text);
    }

    public void Dispose()
    {
        if (_serialPort != null)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
            _serialPort.Dispose();
            _serialPort = null;
        }
    }
}