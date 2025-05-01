using DCCEXDotnet;

namespace Shared;

public class Utils
{
    private const string COM_PORT = "COM6";
    private const int BAUD_RATE = 115200;

    public static SerialPortStream GetSerialPortStream() => new SerialPortStream(COM_PORT, BAUD_RATE);
}
