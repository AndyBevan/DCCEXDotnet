using DCCEXDotnet.Tests.Mocks;
using Moq;

namespace DCCEXDotnet.Tests.General
{
    public class TrackPowerParsingTests
    {
        private TestStream _stream;
        private DCCEXProtocol _protocol;
        private Mock<IDCCEXProtocolDelegate> _delegateMock;

        public TrackPowerParsingTests()
        {
            _stream = new TestStream();
            _delegateMock = new Mock<IDCCEXProtocolDelegate>();
            _protocol = new DCCEXProtocol();
            _protocol.SetDelegate(_delegateMock.Object);
            _protocol.SetLogStream(new TestStream());
            _protocol.Connect(_stream);
        }

        [Fact]
        public void AllTracksOff_ShouldTriggerDelegate()
        {
            _stream.LoadString("<p0>");
            _delegateMock.Setup(d => d.ReceivedTrackPower(TrackPower.PowerOff)).Verifiable();
            _protocol.Check();
            _delegateMock.Verify(d => d.ReceivedTrackPower(TrackPower.PowerOff), Times.Once);
        }

        [Fact]
        public void AllTracksOn_ShouldTriggerDelegate()
        {
            _stream.LoadString("<p1>");
            _delegateMock.Setup(d => d.ReceivedTrackPower(TrackPower.PowerOn)).Verifiable();
            _protocol.Check();
            _delegateMock.Verify(d => d.ReceivedTrackPower(TrackPower.PowerOn), Times.Once);
        }

        [Fact]
        public void MainTrackOn_ShouldTriggerDelegate()
        {
            _stream.LoadString("<p1 MAIN>");
            _delegateMock.Setup(d => d.ReceivedTrackPower(TrackPower.PowerOn)).Verifiable();
            _protocol.Check();
            _delegateMock.Verify(d => d.ReceivedTrackPower(TrackPower.PowerOn), Times.Once);
        }

        [Fact]
        public void MainTrackOff_ShouldTriggerDelegate()
        {
            _stream.LoadString("<p0 MAIN>");
            _delegateMock.Setup(d => d.ReceivedTrackPower(TrackPower.PowerOff)).Verifiable();
            _protocol.Check();
            _delegateMock.Verify(d => d.ReceivedTrackPower(TrackPower.PowerOff), Times.Once);
        }
    }
}
