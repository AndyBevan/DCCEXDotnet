using System.Text;

namespace DCCEXDotnet.Tests.General
{
    public class DCCEXInboundTests
    {
        [Fact]
        public void CanParseMessage()
        {
            var message = @"<m ""Hello World"">";

            DCCEXInbound.Setup(50);
            var isParsed = DCCEXInbound.Parse(message);
            var foo = DCCEXInbound.GetTextParameter(0);

            DCCEXInboundNewExperiment.Setup(50);
            var isParsed2 = DCCEXInboundNewExperiment.Parse(message);
        }

        [Fact]
        public void Parse_LowercaseKeyword_ShouldBeParsedAsHashedParameter()
        {
            // Arrange
            DCCEXInbound.Setup(5);

            string input = "<m hello>";

            // Act
            bool result = DCCEXInbound.Parse(input);

            // Assert parse success
            Assert.True(result);

            // It should have 1 parameter
            Assert.Equal(1, DCCEXInbound.GetParameterCount());

            // Compute expected hash for "HELLO"
            int expected = 0;
            foreach (char c in "HELLO")
                expected = (expected << 5) + expected ^ c;

            Assert.Equal(expected, DCCEXInbound.GetNumber(0));

            var textParameter = DCCEXInbound.GetTextParameter(1);
        }

        [Fact]
        public void Parse_QuotedTextParameter_ShouldReturnExactString()
        {
            // Arrange
            DCCEXInbound.Setup(5);
            string input = "<m \"Hello World\">";

            // Act
            bool result = DCCEXInbound.Parse(input);

            // Assert
            Assert.True(result);
            Assert.Equal(1, DCCEXInbound.GetParameterCount());
            Assert.True(DCCEXInbound.IsTextParameter(0));

            string text = DCCEXInbound.GetTextParameter(0);
            Assert.Equal("Hello World", text);
        }

        public class DumpStreamMock : IStream
        {
            private readonly StringBuilder _log = new();

            public string Output => _log.ToString();

            public int Available() => 0;
            public int Read() => -1;

            public void WriteLine(string text) => _log.AppendLine(text);
            public void Print(string text) => _log.Append(text);
            public void Println(string text) => _log.AppendLine(text);
        }

        public class DumpTests
        {
            [Fact]
            public void Dump_WithTextAndNumberParameters_ShouldWriteExpectedOutput()
            {
                // Arrange
                var stream = new DumpStreamMock();
                DCCEXInbound.Setup(5);
                DCCEXInbound.Parse("<m \"TestString\" 123>");

                // Act
                DCCEXInbound.Dump(stream);
                var output = stream.Output;

                // Assert
                Assert.Contains("Opcode='m'", output);
                Assert.Contains("GetTextParameter(0)=\"TestString\"", output);
                Assert.Contains("GetNumber(1)=123", output);
            }

            [Fact]
            public void Dump_WithOnlyOpcode_ShouldStillOutputOpcodeLine()
            {
                var stream = new DumpStreamMock();
                DCCEXInbound.Setup(5);
                DCCEXInbound.Parse("<x>");

                DCCEXInbound.Dump(stream);
                var output = stream.Output;

                Assert.Contains("Opcode='x'", output);
            }

            [Fact]
            public void Dump_WithNoParameters_ShouldOnlyOutputOpcode()
            {
                var stream = new DumpStreamMock();
                DCCEXInbound.Setup(5);
                DCCEXInbound.Parse("<x>");

                DCCEXInbound.Dump(stream);
                var output = stream.Output;

                Assert.DoesNotContain("GetTextParameter", output);
                Assert.DoesNotContain("GetNumber", output);
            }
        }

        #region Generated Tests
        [Fact]
        public void Parse_SimpleCommandWithText_ShouldReturnTextParameter()
        {
            DCCEXInbound.Setup(5);
            string input = "<m \"Hello World\">";
            bool result = DCCEXInbound.Parse(input);

            Assert.True(result);
            Assert.Equal(1, DCCEXInbound.GetParameterCount());
            Assert.True(DCCEXInbound.IsTextParameter(0));
            Assert.Equal("Hello World", DCCEXInbound.GetTextParameter(0));
        }

        [Fact]
        public void Parse_CommandWithNumber_ShouldReturnCorrectNumber()
        {
            DCCEXInbound.Setup(5);
            string input = "<m 42>";
            bool result = DCCEXInbound.Parse(input);

            Assert.True(result);
            Assert.Equal(1, DCCEXInbound.GetParameterCount());
            Assert.False(DCCEXInbound.IsTextParameter(0));
            Assert.Equal(42, DCCEXInbound.GetNumber(0));
        }

        [Fact]
        public void Parse_CommandWithKeyword_ShouldReturnHashedValue()
        {
            DCCEXInbound.Setup(5);
            string input = "<m HELLO>";
            bool result = DCCEXInbound.Parse(input);

            Assert.True(result);
            Assert.Equal(1, DCCEXInbound.GetParameterCount());

            int expected = 0;
            foreach (char c in "HELLO")
                expected = (expected << 5) + expected ^ c;

            Assert.Equal(expected, DCCEXInbound.GetNumber(0));
        }

        [Fact]
        public void Parse_CommandWithMixedParameters_ShouldParseAllCorrectly()
        {
            DCCEXInbound.Setup(5);
            string input = "<m \"Text\" 100 ABC>";
            bool result = DCCEXInbound.Parse(input);

            Assert.True(result);
            Assert.Equal(3, DCCEXInbound.GetParameterCount());

            Assert.True(DCCEXInbound.IsTextParameter(0));
            Assert.Equal("Text", DCCEXInbound.GetTextParameter(0));

            Assert.False(DCCEXInbound.IsTextParameter(1));
            Assert.Equal(100, DCCEXInbound.GetNumber(1));

            Assert.False(DCCEXInbound.IsTextParameter(2));
            int expected = 0;
            foreach (char c in "ABC")
                expected = (expected << 5) + expected ^ c;
            Assert.Equal(expected, DCCEXInbound.GetNumber(2));
        }

        [Fact]
        public void Parse_CommandWithNoGreaterThan_ShouldReturnFalse()
        {
            DCCEXInbound.Setup(5);
            string input = "<m 123";
            bool result = DCCEXInbound.Parse(input);
            Assert.False(result);
        }

        [Fact]
        public void Parse_iCommand_ShouldStoreWholeBufferAsText()
        {
            DCCEXInbound.Setup(5);
            string input = "<iHelloDcc>";
            bool result = DCCEXInbound.Parse(input);

            Assert.True(result);
            Assert.Equal(1, DCCEXInbound.GetParameterCount());
            Assert.True(DCCEXInbound.IsTextParameter(0));
            Assert.Equal("HelloDcc", DCCEXInbound.GetTextParameter(0)); // Everything after 'i'
        }
        #endregion Generated Tests
    }
}
