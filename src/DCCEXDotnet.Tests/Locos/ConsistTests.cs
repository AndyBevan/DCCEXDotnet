namespace DCCEXDotnet.Tests.Locos
{
    public class ConsistTests
    {
        [Fact]
        public void CreateConsistByLoco()
        {
            string functionList = "Lights/*Horn";

            var loco10 = new Loco(10, LocoSource.LocoSourceRoster);
            loco10.SetName("Loco 10");
            loco10.SetupFunctions(functionList);

            var loco2 = new Loco(2, LocoSource.LocoSourceRoster);
            loco2.SetName("Loco 2");
            loco2.SetupFunctions(functionList);

            var loco10000 = new Loco(10000, LocoSource.LocoSourceRoster);
            loco10000.SetName("Loco 10000");
            loco10000.SetupFunctions(functionList);

            var consist = new Consist();
            consist.SetName("Test Legacy Consist");
            consist.AddLoco(loco10, Facing.FacingForward);
            consist.AddLoco(loco2, Facing.FacingReversed);
            consist.AddLoco(loco10000, Facing.FacingForward);

            Assert.Equal("Test Legacy Consist", consist.GetName());
            Assert.Equal(3, consist.GetLocoCount());
            Assert.True(consist.InConsist(loco10));
            Assert.True(consist.InConsist(loco2));
            Assert.True(consist.InConsist(loco10000));
            Assert.True(consist.InConsist(10));
            Assert.True(consist.InConsist(2));
            Assert.True(consist.InConsist(10000));

            Assert.Equal(loco10, consist.GetFirst().GetLoco());

            Assert.Equal(0, consist.GetSpeed());
            Assert.Equal(Direction.Forward, consist.GetDirection());
            loco2.SetSpeed(35);
            loco10000.SetDirection(Direction.Reverse);
            Assert.Equal(0, consist.GetSpeed());
            Assert.Equal(Direction.Forward, consist.GetDirection());
            loco10.SetSpeed(21);
            loco10.SetDirection(Direction.Reverse);
            Assert.Equal(21, consist.GetSpeed());
            Assert.Equal(Direction.Reverse, consist.GetDirection());

            consist.RemoveLoco(loco2);
            Assert.Equal(2, consist.GetLocoCount());
            Assert.Equal(loco10, consist.GetFirst().GetLoco());
            Assert.Equal(21, consist.GetSpeed());
            Assert.Equal(Direction.Reverse, consist.GetDirection());

            consist.RemoveLoco(loco10);
            Assert.Equal(1, consist.GetLocoCount());
            Assert.Equal(loco10000, consist.GetFirst().GetLoco());
            Assert.Equal(0, consist.GetSpeed());
            Assert.Equal(Direction.Reverse, consist.GetDirection());

            consist.RemoveAllLocos();
            Assert.Equal(0, consist.GetLocoCount());
            Assert.Null(consist.GetFirst());
            Assert.Equal(0, consist.GetSpeed());
            Assert.Equal(Direction.Forward, consist.GetDirection());
        }

        [Fact]
        public void CreateConsistByAddress()
        {
            var consist = new Consist();
            consist.AddLoco(10, Facing.FacingForward);
            consist.AddLoco(2, Facing.FacingReversed);
            consist.AddLoco(10000, Facing.FacingForward);

            Assert.Equal("10", consist.GetName());
            Assert.Equal(3, consist.GetLocoCount());
            Assert.True(consist.InConsist(10));
            Assert.True(consist.InConsist(2));
            Assert.True(consist.InConsist(10000));

            var loco10 = consist.GetByAddress(10)?.GetLoco();
            Assert.NotNull(loco10);
            Assert.Equal(10, loco10.GetAddress());

            var loco2 = consist.GetByAddress(2)?.GetLoco();
            Assert.NotNull(loco2);
            Assert.Equal(2, loco2.GetAddress());

            var loco10000 = consist.GetByAddress(10000)?.GetLoco();
            Assert.NotNull(loco10000);
            Assert.Equal(10000, loco10000.GetAddress());

            Assert.Equal(10, consist.GetFirst().GetLoco().GetAddress());

            Assert.Equal(0, consist.GetSpeed());
            Assert.Equal(Direction.Forward, consist.GetDirection());
            loco2.SetSpeed(35);
            loco10000.SetDirection(Direction.Reverse);
            Assert.Equal(0, consist.GetSpeed());
            Assert.Equal(Direction.Forward, consist.GetDirection());
            loco10.SetSpeed(21);
            loco10.SetDirection(Direction.Reverse);
            Assert.Equal(21, consist.GetSpeed());
            Assert.Equal(Direction.Reverse, consist.GetDirection());

            consist.RemoveLoco(loco2);
            Assert.Equal(2, consist.GetLocoCount());
            Assert.Equal(loco10, consist.GetFirst().GetLoco());
            Assert.Equal(21, consist.GetSpeed());
            Assert.Equal(Direction.Reverse, consist.GetDirection());

            consist.RemoveLoco(loco10);
            Assert.Equal(1, consist.GetLocoCount());
            Assert.Equal(loco10000, consist.GetFirst().GetLoco());
            Assert.Equal(0, consist.GetSpeed());
            Assert.Equal(Direction.Reverse, consist.GetDirection());

            consist.RemoveAllLocos();
            Assert.Equal(0, consist.GetLocoCount());
            Assert.Null(consist.GetFirst());
            Assert.Equal(0, consist.GetSpeed());
            Assert.Equal(Direction.Forward, consist.GetDirection());
        }
    }
}
