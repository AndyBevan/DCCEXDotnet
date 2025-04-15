using DCCEXDotnet.Tests.Mocks;
using Moq;

namespace DCCEXDotnet.Tests.General;
public class TrackPowerControlTests
{
    private TestStream _stream;
    private DCCEXProtocol _protocol;

    public TrackPowerControlTests()
    {
        _stream = new TestStream();
        var testLogStream = new TestStream();
        _protocol = new DCCEXProtocol();
        var delegateMock = new Mock<IDCCEXProtocolDelegate>();

        _protocol.SetDelegate(delegateMock.Object);
        _protocol.SetLogStream(testLogStream);
        _protocol.Connect(_stream);
    }

    [Fact]
    public void PowerAllOn_SendsCorrectCommand()
    {
        _protocol.PowerOn();
        Assert.Equal("<1>\r\n", _stream.GetBuffer());
    }

    [Fact]
    public void PowerAllOff_SendsCorrectCommand()
    {
        _protocol.PowerOff();
        Assert.Equal("<0>\r\n", _stream.GetBuffer());
    }

    [Fact]
    public void PowerMainOn_SendsCorrectCommand()
    {
        _protocol.PowerMainOn();
        Assert.Equal("<1 MAIN>\r\n", _stream.GetBuffer());
    }

    [Fact]
    public void PowerMainOff_SendsCorrectCommand()
    {
        _protocol.PowerMainOff();
        Assert.Equal("<0 MAIN>\r\n", _stream.GetBuffer());
    }

    [Fact]
    public void PowerProgOn_SendsCorrectCommand()
    {
        _protocol.PowerProgOn();
        Assert.Equal("<1 PROG>\r\n", _stream.GetBuffer());
    }

    [Fact]
    public void PowerProgOff_SendsCorrectCommand()
    {
        _protocol.PowerProgOff();
        Assert.Equal("<0 PROG>\r\n", _stream.GetBuffer());
    }

    [Fact]
    public void JoinProg_SendsCorrectCommand()
    {
        _protocol.JoinProg();
        Assert.Equal("<1 JOIN>\r\n", _stream.GetBuffer());
    }
}
