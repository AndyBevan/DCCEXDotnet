using DCCEXDotnet.Tests.Mocks;
using Moq;

namespace DCCEXDotnet.Tests.Routes;

public class RouteParsingTests
{
    private DCCEXProtocol protocol;
    private TestStream stream;
    private Mock<IDCCEXProtocolDelegate> mockDelegate;

    public RouteParsingTests()
    {
        Route.ClearRouteList();
        protocol = new DCCEXProtocol();
        stream = new TestStream();
        mockDelegate = new Mock<IDCCEXProtocolDelegate>();
        protocol.Connect(stream);
        protocol.SetDelegate(mockDelegate.Object);
    }

    [Fact]
    public void ParseEmptyRouteList_ShouldSetReceivedFlagTrue()
    {
        Assert.False(protocol.ReceivedRouteList());

        protocol.GetLists(false, false, true, false);
        Assert.Equal("<JA>\r\n", stream.GetBuffer());
        stream.ClearBuffer();

        stream.LoadString("<jA>");
        protocol.Check();

        Assert.True(protocol.ReceivedRouteList());
    }

    [Fact]
    public void ParseThreeRoutes_ShouldSetReceivedFlagAfterAllDetails()
    {
        Assert.False(protocol.ReceivedRouteList());

        protocol.GetLists(false, false, true, false);
        Assert.Equal("<JA>\r\n", stream.GetBuffer());
        stream.ClearBuffer();

        stream.LoadString("<jA 21 121 221>");
        protocol.Check();

        Assert.False(protocol.ReceivedRouteList());

        stream.LoadString("<jA 21 R \"Route 21\">");
        protocol.Check();

        stream.LoadString("<jA 121 A \"Automation 121\">");
        protocol.Check();

        stream.LoadString("<jA 221 R \"\">");
        mockDelegate.Setup(d => d.ReceivedRouteList()).Verifiable();
        protocol.Check();

        Assert.True(protocol.ReceivedRouteList());
        mockDelegate.Verify();
    }
}
