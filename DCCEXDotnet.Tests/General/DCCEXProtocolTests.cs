using DCCEXDotnet.Tests.Mocks;
using Moq;

namespace DCCEXDotnet.Tests.General;



public class DCCEXProtocolTests
{
    [Fact]
    public void ClearBufferWhenFull_ShouldIgnoreGarbageAndProcessNextCommand()
    {
        // Arrange
        var testStream = new TestStream();
        var testLogStream = new TestStream();
        var protocol = new DCCEXProtocol();
        var delegateMock = new Mock<IDCCEXProtocolDelegate>();

        protocol.SetDelegate(delegateMock.Object);
        protocol.SetLogStream(testLogStream); // IStream adapter
        protocol.Connect(testStream);

        // Fill buffer with garbage
        var rand = new Random();
        for (int i = 0; i < 500; i++)
        {
            char ch = (char)('A' + rand.Next(0, 26));
            testStream.LoadBytes(new[] { ch });
        }

        // Then load a valid message
        string message = "<m \"Hello World\">"; // valid inbound command
        testStream.LoadBytes(message.ToCharArray());
        //testStream.LoadBytes("<m \"Hello World\">".ToCharArray());

        var foo = testStream.GetByteAsString();

        // Expect the delegate to be called once with the clean message
        delegateMock.Setup(d => d.ReceivedMessage("Hello World")).Verifiable();

        // Act
        protocol.Check();

        // Assert
        delegateMock.Verify(d => d.ReceivedMessage("Hello World"), Times.Once);
    }

    [Fact]
    public void Parse_UnderscoreOnlyKeyword_ShouldHashUnderscore()
    {
        // Arrange
        DCCEXInbound.Setup(5);
        string input = "<m _>";

        // Act
        bool result = DCCEXInbound.Parse(input);

        // Assert parse success
        Assert.True(result);

        // It should have 1 parameter
        Assert.Equal(1, DCCEXInbound.GetParameterCount());

        // Expected: hash of "_"
        int expected = (0 << 5) + 0 ^ '_'; // just '^' because first char

        Assert.Equal(expected, DCCEXInbound.GetNumber(0));
    }

    //[Fact]
    //public void BroadcastHelloWorld_ShouldInvokeReceivedMessage()
    //{
    //    // Arrange
    //    var testStream = new TestStream();
    //    var testLogStream = new TestStream();
    //    var protocol = new DCCEXProtocol();
    //    var delegateMock = new Mock<IDCCEXProtocolDelegate>();

    //    protocol.SetDelegate(delegateMock.Object);
    //    protocol.SetLogStream(testLogStream);
    //    protocol.Connect(testStream);

    //    // Load valid message into stream
    //    //testStream.LoadBytes("<m \"Hello World\">".ToCharArray());
    //    //testStream.LoadBytes(@"<m ""Hello World"">".ToCharArray());
    //    var message = @"<m ""Hello World"">"; // valid inbound command
    //    testStream.LoadBytes(message.ToCharArray());

    //    delegateMock.Setup(d => d.ReceivedMessage("Hello World")).Verifiable();

    //    // Act
    //    protocol.Check();

    //    // Assert
    //    delegateMock.Verify(d => d.ReceivedMessage("Hello World"), Times.Once);
    //}

}
