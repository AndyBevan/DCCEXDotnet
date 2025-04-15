namespace DCCEXDotnet.Tests.Turntables;
public class TurntableTests
{
    public TurntableTests()
    {
        Turntable.ClearTurntableList();
    }
    [Fact]
    public void CreateTurntableIndex_ShouldHaveCorrectAttributes()
    {
        var index = new TurntableIndex(1, 0, 900, "Home");

        Assert.NotNull(index);
        Assert.Equal(1, index.GetTtId());
        Assert.Equal(0, index.GetId());
        Assert.Equal(900, index.GetAngle());
        Assert.Equal("Home", index.GetName());
        Assert.Null(index.GetNextIndex());
    }

    [Fact]
    public void CreateEXTurntable_ShouldStoreAllIndexes()
    {
        var turntable1 = new Turntable(1);

        turntable1.SetType(TurntableType.TurntableTypeEXTT);
        turntable1.SetIndex(0);
        turntable1.SetNumberOfIndexes(5);
        turntable1.SetName("Test EX-Turntable");

        turntable1.AddIndex(new TurntableIndex(1, 0, 900, "Home"));
        turntable1.AddIndex(new TurntableIndex(1, 1, 450, "EX-Turntable Index 1"));
        turntable1.AddIndex(new TurntableIndex(1, 2, 1800, "EX-Turntable Index 2"));
        turntable1.AddIndex(new TurntableIndex(1, 3, 2700, "EX-Turntable Index 3"));
        turntable1.AddIndex(new TurntableIndex(1, 4, 3000, "EX-Turntable Index 4"));

        Assert.Equal(TurntableType.TurntableTypeEXTT, turntable1.GetType());
        Assert.Equal(0, turntable1.GetIndex());
        Assert.Equal(5, turntable1.GetNumberOfIndexes());
        Assert.Equal("Test EX-Turntable", turntable1.GetName());
        Assert.Null(turntable1.GetNext());
        Assert.Equal(5, turntable1.GetIndexCount());
        Assert.Equal("EX-Turntable Index 3", turntable1.GetIndexById(3)?.GetName());
    }

    [Fact]
    public void CreateDCCTurntable_ShouldStoreAllIndexes()
    {
        var turntable2 = new Turntable(2);

        turntable2.SetType(TurntableType.TurntableTypeDCC);
        turntable2.SetIndex(3);
        turntable2.SetNumberOfIndexes(6);
        turntable2.SetName("Test DCC Turntable");

        turntable2.AddIndex(new TurntableIndex(2, 0, 0, "Home"));
        turntable2.AddIndex(new TurntableIndex(2, 1, 450, "DCC Turntable Index 1"));
        turntable2.AddIndex(new TurntableIndex(2, 2, 1800, "DCC Turntable Index 2"));
        turntable2.AddIndex(new TurntableIndex(2, 3, 2700, "DCC Turntable Index 3"));
        turntable2.AddIndex(new TurntableIndex(2, 4, 3000, "DCC Turntable Index 4"));
        turntable2.AddIndex(new TurntableIndex(2, 5, 3300, "DCC Turntable Index 5"));

        Assert.Equal(TurntableType.TurntableTypeDCC, turntable2.GetType());
        Assert.Equal(3, turntable2.GetIndex());
        Assert.Equal(6, turntable2.GetNumberOfIndexes());
        Assert.Equal("Test DCC Turntable", turntable2.GetName());
        Assert.Equal(6, turntable2.GetIndexCount());
        Assert.Equal("DCC Turntable Index 4", turntable2.GetIndexById(4)?.GetName());
    }

    [Fact]
    public void CreateTurntableList_ShouldLinkProperly()
    {
        var t1 = new Turntable(1) { Type = TurntableType.TurntableTypeEXTT, Index = 0, Name = "Test EX-Turntable" };
        var t2 = new Turntable(2) { Type = TurntableType.TurntableTypeDCC, Index = 3, Name = "Test DCC Turntable" };
        var t3 = new Turntable(3) { Type = TurntableType.TurntableTypeEXTT, Index = 0, Name = "Test EX-Turntable" };

        Assert.NotNull(t1);
        Assert.NotNull(t2);
        Assert.NotNull(t3);
        Assert.Equal(t1, Turntable.GetFirst());
        Assert.Equal(t2, t1.GetNext());
        Assert.Null(t3.GetNext()); // Not linked
    }

    [Fact]
    public void OperateTurntable_ShouldUpdateIndexAndMovingState()
    {
        var tt = new Turntable(1)
        {
            Type = TurntableType.TurntableTypeEXTT,
            Index = 0,
            NumberOfIndexes = 5,
            Name = "Test EX-Turntable"
        };       

        tt.AddIndex(new TurntableIndex(1, 0, 900, "Home"));
        tt.AddIndex(new TurntableIndex(1, 1, 450, "EX-Turntable Index 1"));
        tt.AddIndex(new TurntableIndex(1, 2, 1800, "EX-Turntable Index 2"));
        tt.AddIndex(new TurntableIndex(1, 3, 2700, "EX-Turntable Index 3"));
        tt.AddIndex(new TurntableIndex(1, 4, 3000, "EX-Turntable Index 4"));

        Assert.Equal(0, tt.GetIndex());
        Assert.False(tt.IsMoving());
        Assert.Equal("Home", tt.GetIndexById(tt.GetIndex())?.GetName());

        tt.SetIndex(3);
        tt.SetMoving(true);

        Assert.Equal(3, tt.GetIndex());
        Assert.True(tt.IsMoving());
        Assert.Equal("EX-Turntable Index 3", tt.GetIndexById(tt.GetIndex())?.GetName());

        tt.SetMoving(false);

        Assert.False(tt.IsMoving());
        Assert.Equal("EX-Turntable Index 3", tt.GetIndexById(tt.GetIndex())?.GetName());
    }
}

