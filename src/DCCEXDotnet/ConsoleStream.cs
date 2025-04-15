using System.Text;

namespace DCCEXDotnet;

/// <summary>
/// Console implementation of IStream interface
/// </summary>
public class ConsoleStream : IStream
{
    private StringBuilder _inputBuffer = new StringBuilder();
    private int _position = 0;

    public ConsoleStream()
    {
        // Pre-populate input buffer with any console input that was already available
        while (Console.KeyAvailable)
        {
            _inputBuffer.Append((char)Console.Read());
        }
    }

    public int Available()
    {
        // Check if we have characters in our buffer
        if (_position < _inputBuffer.Length)
        {
            return _inputBuffer.Length - _position;
        }

        // Check if there are any keys available in the console
        if (Console.KeyAvailable)
        {
            // Read all available input
            while (Console.KeyAvailable)
            {
                _inputBuffer.Append((char)Console.Read());
            }
            return _inputBuffer.Length - _position;
        }

        return 0;
    }

    public int Read()
    {
        // If we have characters in our buffer, return the next one
        if (_position < _inputBuffer.Length)
        {
            return _inputBuffer[_position++];
        }

        // If no characters in buffer but keys are available, read them
        if (Console.KeyAvailable)
        {
            _inputBuffer.Clear();
            _position = 0;

            // Read until Enter is pressed
            while (true)
            {
                int key = Console.Read();
                _inputBuffer.Append((char)key);

                // Break if Enter key is pressed (CR or LF)
                if (key == 13 || key == 10)
                {
                    break;
                }
            }

            return _inputBuffer[_position++];
        }

        return -1; // No data available
    }

    public void WriteLine(string text)
    {
        Console.WriteLine(text);
    }

    public void Print(string text)
    {
        Console.Write(text);
    }

    public void Println(string text)
    {
        Console.WriteLine(text);
    }
}
