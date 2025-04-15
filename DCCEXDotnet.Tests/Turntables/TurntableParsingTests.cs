using DCCEXDotnet.Tests.Mocks;
using Moq;

namespace DCCEXDotnet.Tests.Turntables;

public class TurntableParsingTests
{
    private DCCEXProtocol protocol;
    private TestStream stream;
    private Mock<IDCCEXProtocolDelegate> mockDelegate;

    public TurntableParsingTests()
    {
        Turntable.ClearTurntableList();
        protocol = new DCCEXProtocol();
        stream = new TestStream();
        mockDelegate = new Mock<IDCCEXProtocolDelegate>();
        protocol.Connect(stream);
        protocol.SetDelegate(mockDelegate.Object);
    }
    [Fact]
    public void ParseEmptyTurntableList_ShouldSetReceivedFlagTrue()
    {


        Assert.False(protocol.ReceivedTurntableList());

        protocol.GetLists(false, false, false, true);
        Assert.Equal("<JO>\r\n", stream.GetBuffer());
        stream.ClearBuffer();

        stream.LoadString("<jO>");
        protocol.Check();

        Assert.True(protocol.ReceivedTurntableList());
    }

    [Fact]
    public void ParseTwoTurntables_ShouldSetReceivedFlagAfterDetails()
    {
        Assert.False(protocol.ReceivedTurntableList());

        protocol.GetLists(false, false, false, true);
        Assert.Equal("<JO>\r\n", stream.GetBuffer());
        stream.ClearBuffer();

        stream.LoadString("<jO 1 2>");
        protocol.Check();

        Assert.False(protocol.ReceivedTurntableList());

        stream.LoadString("<jO 1 1 0 5 \"EX-Turntable\">");
        stream.LoadString("<jO 2 0 3 6 \"DCC Turntable\">");

        stream.LoadString("<jP 1 0 900 \"Home\">"); protocol.Check();
        stream.LoadString("<jP 1 1 450 \"Position 1\">"); protocol.Check();
        stream.LoadString("<jP 1 2 1800 \"Position 2\">"); protocol.Check();
        stream.LoadString("<jP 1 3 2700 \"Position 3\">"); protocol.Check();
        stream.LoadString("<jP 1 4 3000 \"Position 4\">"); protocol.Check();

        stream.LoadString("<jP 2 0 0 \"Home\">"); protocol.Check();
        stream.LoadString("<jP 2 1 450 \"Position 1\">"); protocol.Check();
        stream.LoadString("<jP 2 2 1800 \"Position 2\">"); protocol.Check();
        stream.LoadString("<jP 2 3 2700 \"Position 3\">"); protocol.Check();
        stream.LoadString("<jP 2 4 3000 \"Position 4\">"); protocol.Check();
        stream.LoadString("<jP 2 5 3300 \"Position 5\">");

        mockDelegate.Setup(d => d.ReceivedTurntableList()).Verifiable();
        protocol.Check();

        Assert.True(protocol.ReceivedTurntableList());
        mockDelegate.Verify();
    }
}
