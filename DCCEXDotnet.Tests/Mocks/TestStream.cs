using System.Text;

namespace DCCEXDotnet.Tests.Mocks
{
    public class TestStream : IStream
    {
        private readonly Queue<int> _buffer = new();
        //public List<string> WrittenLines { get; } = new();
        private readonly StringBuilder _writeBuffer = new();

        public void LoadBytes(IEnumerable<char> data)
        {
            foreach (var ch in data)
                _buffer.Enqueue(ch);
        }

        public void LoadString(string data)
        {
            if (data == null) return;
            foreach (var ch in data)
                _buffer.Enqueue(ch);
        }

        public int Available() => _buffer.Count;

        public int Read()
        {
            return _buffer.Count > 0 ? _buffer.Dequeue() : -1;
        }

        public void WriteLine(string text)
        {
            //WrittenLines.Add(text);
            _writeBuffer.AppendLine(text);
        }

        public void Print(string text)
        {
            //WrittenLines.Add(text); // Treat Print like WriteLine for testing
            _writeBuffer.Append(text);
        }

        public void Println(string text)
        {
            //WrittenLines.Add(text);
            _writeBuffer.AppendLine(text);
        }

        public string GetBuffer() => _writeBuffer.ToString();

        public string GetByteAsString()
        {
            return new string(_buffer.ToArray().Select(c => (char)c).ToArray());
        }

        public void ClearBuffer()
        {
            _buffer.Clear();
        }
    }
}
