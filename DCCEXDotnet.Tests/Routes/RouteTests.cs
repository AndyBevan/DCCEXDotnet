using DCCEXDotnet.Tests.Mocks;
using Moq;
using System.IO;

namespace DCCEXDotnet.Tests.Routes;

public class RouteTests
{
    private DCCEXProtocol protocol;
    private TestStream stream;
    private Mock<IDCCEXProtocolDelegate> mockDelegate;

    public RouteTests()
    {
        Route.ClearRouteList();
        protocol = new DCCEXProtocol();
        stream = new TestStream();
        mockDelegate = new Mock<IDCCEXProtocolDelegate>();
        protocol.Connect(stream);
        protocol.SetDelegate(mockDelegate.Object);
    }
    [Fact]
    public void CreateSingleRoute_ShouldHaveCorrectProperties()
    {
        var route200 = new Route(200);
        route200.SetName("Route 200");
        route200.SetType(RouteType.RouteTypeRoute);

        Assert.Equal(200, route200.GetId());
        Assert.Equal("Route 200", route200.GetName());
        Assert.Equal(RouteType.RouteTypeRoute, route200.GetType());

        Assert.Equal(route200, Route.GetFirst());
        Assert.Null(route200.GetNext());
    }

    [Fact]
    public void CreateThreeRoutes_ShouldBeRetrievableById()
    {
        var route200 = new Route(200);
        route200.SetName("Route 200");
        route200.SetType(RouteType.RouteTypeRoute);

        var route300 = new Route(300);
        route300.SetName("Automation 300");
        route300.SetType(RouteType.RouteTypeAutomation);

        var route400 = new Route(400);
        route400.SetName("");
        route400.SetType(RouteType.RouteTypeRoute);

        Assert.Equal(route200, protocol.Routes.GetByIdNonStatic(200));
        Assert.Equal(route200, protocol.Routes.GetByIdNonStatic(200));
        Assert.Equal(route300, protocol.Routes.GetByIdNonStatic(300));
        Assert.Equal(route400, protocol.Routes.GetByIdNonStatic(400));

        Assert.Equal(200, route200.GetId());
        Assert.Equal("Route 200", route200.GetName());
        Assert.Equal(RouteType.RouteTypeRoute, route200.GetType());

        Assert.Equal(300, route300.GetId());
        Assert.Equal("Automation 300", route300.GetName());
        Assert.Equal(RouteType.RouteTypeAutomation, route300.GetType());

        Assert.Equal(400, route400.GetId());
        Assert.Equal("", route400.GetName());
        Assert.Equal(RouteType.RouteTypeRoute, route400.GetType());
    }

    [Fact]
    public void AutomationHandOff_ShouldGenerateExpectedCommand()
    {
        var automation100 = new Route(100);
        automation100.SetType(RouteType.RouteTypeAutomation);

        protocol.HandOffLoco(1234, 100);
        Assert.Equal("</ START 1234 100>\r\n", stream.GetBuffer());
    }
}
