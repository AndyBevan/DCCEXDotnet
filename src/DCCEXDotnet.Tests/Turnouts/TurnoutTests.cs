namespace DCCEXDotnet.Tests.Turnouts;

public class TurnoutTests
{
    public TurnoutTests()
    {
        Turnout.ClearTurnoutList();
    }

    [Fact]
    public void CreateSingleTurnout_ShouldHaveCorrectProperties()
    {
        var protocol = new DCCEXProtocol();
        var turnout100 = new Turnout(100, false);
        turnout100.SetName("Turnout 100");

        Assert.Equal(100, turnout100.GetId());
        Assert.Equal("Turnout 100", turnout100.GetName());
        Assert.False(turnout100.GetThrown());

        Assert.Equal(turnout100, protocol.Turnouts.GetByIdNonStatic(100));
    }

    [Fact]
    public void CreateTurnoutList_ShouldContainAllTurnouts()
    {
        var protocol = new DCCEXProtocol();

        var turnout100 = new Turnout(100, false);
        turnout100.SetName("Turnout 100");

        var turnout101 = new Turnout(101, true);
        turnout101.SetName("Turnout 101");

        var turnout102 = new Turnout(102, false);
        turnout102.SetName("");

        Assert.Equal(turnout100, protocol.Turnouts.GetByIdNonStatic(100));
        Assert.Equal(turnout101, protocol.Turnouts.GetByIdNonStatic(101));
        Assert.Equal(turnout102, protocol.Turnouts.GetByIdNonStatic(102));

        Assert.Equal(100, turnout100.GetId());
        Assert.Equal("Turnout 100", turnout100.GetName());
        Assert.False(turnout100.GetThrown());

        Assert.Equal(101, turnout101.GetId());
        Assert.Equal("Turnout 101", turnout101.GetName());
        Assert.True(turnout101.GetThrown());

        Assert.Equal(102, turnout102.GetId());
        Assert.Equal("", turnout102.GetName());
        Assert.False(turnout102.GetThrown());
    }

    [Fact]
    public void OperateTurnout_ShouldToggleStateCorrectly()
    {
        var turnout100 = new Turnout(100, false);
        turnout100.SetName("Turnout 100");

        turnout100.SetThrown(false);
        Assert.False(turnout100.GetThrown());

        turnout100.SetThrown(true);
        Assert.True(turnout100.GetThrown());

        turnout100.SetThrown(false);
        Assert.False(turnout100.GetThrown());
    }
}



