using DCCEXDotnet.Tests.Mocks;
using Moq;

namespace DCCEXDotnet.Tests.Locos
{
    public class LocoUpdate
    {
        [Fact]
        public void ReceiveRosterLocoUpdate()
        {
            //Temp to make the test pass
            Loco.ClearRoster();

            var protocol = new DCCEXProtocol();
            var stream = new TestStream();
            var mockDelegate = new Mock<IDCCEXProtocolDelegate>();
            protocol.Connect(stream);
            protocol.SetDelegate(mockDelegate.Object);

            // Ensure roster starts empty
            //TODO - this is more like how the CPP was
            //Assert.Null(protocol.Roster.GetFirst());
            Assert.Null(Loco.GetFirst());

            // Add locos to the roster
            var loco42 = new Loco(42, LocoSource.LocoSourceRoster);
            loco42.SetName("Loco42");
            var loco120 = new Loco(120, LocoSource.LocoSourceRoster);
            loco120.SetName("Loco120");

            // Verify roster not empty
            //Assert.NotNull(protocol.Roster.GetFirst());
            Assert.NotNull(Loco.GetFirst());

            // Simulate update for loco 42: forward, speed 21, function 0 on
            stream.LoadString("<l 42 0 150 1>");

            mockDelegate.Setup(d => d.ReceivedLocoUpdate(loco42)).Verifiable();
            mockDelegate.Setup(d => d.ReceivedLocoBroadcast(42, 21, Direction.Forward, 1)).Verifiable();
            protocol.Check();

            Assert.Equal(21, loco42.GetSpeed());
            Assert.Equal(Direction.Forward, loco42.GetDirection());
            Assert.Equal(1, loco42.GetFunctionStates());

            // Simulate update for loco 120: reverse, speed 11, function 1 on
            stream.LoadString("<l 120 0 12 2>");

            mockDelegate.Setup(d => d.ReceivedLocoUpdate(loco120)).Verifiable();
            mockDelegate.Setup(d => d.ReceivedLocoBroadcast(120, 11, Direction.Reverse, 2)).Verifiable();
            protocol.Check();

            Assert.Equal(11, loco120.GetSpeed());
            Assert.Equal(Direction.Reverse, loco120.GetDirection());
            Assert.Equal(2, loco120.GetFunctionStates());

            mockDelegate.Verify();
        }

        [Fact]
        public void ReceiveNonRosterLocoUpdate()
        {
            //TODO - Temp
            Loco.ClearRoster();

            var protocol = new DCCEXProtocol();
            var stream = new TestStream();
            var mockDelegate = new Mock<IDCCEXProtocolDelegate>();
            protocol.Connect(stream);
            protocol.SetDelegate(mockDelegate.Object);

            // Simulate unknown loco update: 355, forward, speed 31, functions off
            stream.LoadString("<l 355 0 160 0>");
            mockDelegate.Setup(d => d.ReceivedLocoBroadcast(355, 31, Direction.Forward, 0)).Verifiable();
            mockDelegate.Setup(d => d.ReceivedLocoUpdate(It.IsAny<Loco>())).Throws(new Exception("Should not be called"));
            protocol.Check();

            // Simulate unknown loco update: 42, reverse, speed 11, function 1 on
            stream.LoadString("<l 42 0 12 2>");
            mockDelegate.Setup(d => d.ReceivedLocoBroadcast(42, 11, Direction.Reverse, 2)).Verifiable();
            protocol.Check();

            mockDelegate.Verify();
        }

    }
}
