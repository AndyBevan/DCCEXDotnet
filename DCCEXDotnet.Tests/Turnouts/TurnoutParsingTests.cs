using DCCEXDotnet.Tests.Mocks;
using Moq;

namespace DCCEXDotnet.Tests.Turnouts;

public class TurnoutParsingTests
{
    [Fact]
    public void ParseEmptyTurnoutList_ShouldSetReceivedFlagTrue()
    {
        var protocol = new DCCEXProtocol();
        var stream = new TestStream();
        var mockDelegate = new Mock<IDCCEXProtocolDelegate>();
        protocol.Connect(stream);
        protocol.SetDelegate(mockDelegate.Object);

        Assert.False(protocol.ReceivedTurnoutList());

        protocol.GetLists(false, true, false, false);
        Assert.Equal("<JT>\r\n", stream.GetBuffer());
        stream.ClearBuffer();

        stream.LoadString("<jT>");
        protocol.Check();

        Assert.True(protocol.ReceivedTurnoutList());
    }

    [Fact]
    public void ParseThreeTurnouts_ShouldSetReceivedFlagAfterDetails()
    {
        var protocol = new DCCEXProtocol();
        var stream = new TestStream();
        var mockDelegate = new Mock<IDCCEXProtocolDelegate>();
        protocol.Connect(stream);
        protocol.SetDelegate(mockDelegate.Object);

        Assert.False(protocol.ReceivedTurnoutList());

        protocol.GetLists(false, true, false, false);
        Assert.Equal("<JT>\r\n", stream.GetBuffer());
        stream.ClearBuffer();

        stream.LoadString("<jT 100 101 102>");
        protocol.Check();

        Assert.False(protocol.ReceivedTurnoutList());

        stream.LoadString("<jT 100 C \"Turnout 100\">");
        protocol.Check();

        stream.LoadString("<jT 101 T \"Turnout 101\">");
        protocol.Check();

        stream.LoadString("<jT 102 C \"\">");
        mockDelegate.Setup(d => d.ReceivedTurnoutList()).Verifiable();
        protocol.Check();

        Assert.True(protocol.ReceivedTurnoutList());
        mockDelegate.Verify();
    }
}
