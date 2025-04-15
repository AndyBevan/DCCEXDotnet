namespace DCCEXDotnet.Tests.Locos;

public class LocoTests
{
    [Fact]
    public void CreateSingleLoco()
    {
        //Added to make the test pass
        Loco.ClearRoster();

        // Arrange
        var loco1 = new Loco(1, LocoSource.LocoSourceEntry);
        loco1.SetName("Loco 1");
        loco1.SetupFunctions("Lights/*Horn/Bell///Function 5");

        // Assert address, name, and source
        Assert.Equal(1, loco1.GetAddress());
        Assert.Equal("Loco 1", loco1.GetName());
        Assert.Equal(LocoSource.LocoSourceEntry, loco1.GetSource());

        // Assert function momentary and names
        Assert.False(loco1.IsFunctionMomentary(0));
        Assert.True(loco1.IsFunctionMomentary(1));
        Assert.Equal("Bell", loco1.GetFunctionName(2));
        Assert.Equal("Function 5", loco1.GetFunctionName(5));

        // Assert speed and direction
        Assert.Equal(0, loco1.GetSpeed());
        Assert.Equal(Direction.Forward, loco1.GetDirection());
        loco1.SetSpeed(13);
        loco1.SetDirection(Direction.Reverse);
        Assert.Equal(13, loco1.GetSpeed());
        Assert.Equal(Direction.Reverse, loco1.GetDirection());

        // Assert it's not in the roster
        //Assert.Null(Loco.First);
        Assert.Null(Loco.GetFirst());
        Assert.Null(Loco.GetByAddress(1));

        // Assert next is null
        Assert.Null(loco1.Next);
    }

    [Fact]
    public void CreateRosterOfLocos()
    {
        //Added to make the test pass
        Loco.ClearRoster();

        // Arrange
        Assert.Null(Loco.First);

        var loco42 = new Loco(42, LocoSource.LocoSourceRoster);
        loco42.SetName("Loco42");
        var loco9 = new Loco(9, LocoSource.LocoSourceRoster);
        loco9.SetName("Loco9");
        var loco120 = new Loco(120, LocoSource.LocoSourceRoster);
        loco120.SetName("Loco120");

        // Assert roster first loco
        var firstLoco = Loco.First;
        Assert.NotNull(firstLoco);
        Assert.Equal(42, firstLoco.GetAddress());
        Assert.Equal("Loco42", firstLoco.GetName());
        Assert.Equal(LocoSource.LocoSourceRoster, firstLoco.GetSource());

        // Assert second loco
        var secondLoco = firstLoco.Next;
        Assert.NotNull(secondLoco);
        Assert.Equal(9, secondLoco.GetAddress());
        Assert.Equal("Loco9", secondLoco.GetName());
        Assert.Equal(LocoSource.LocoSourceRoster, secondLoco.GetSource());

        // Assert third loco
        var thirdLoco = secondLoco.Next;
        Assert.NotNull(thirdLoco);
        Assert.Equal(120, thirdLoco.GetAddress());
        Assert.Equal("Loco120", thirdLoco.GetName());
        Assert.Equal(LocoSource.LocoSourceRoster, thirdLoco.GetSource());

        // Assert no fourth loco
        Assert.Null(thirdLoco.Next);
    }
}
