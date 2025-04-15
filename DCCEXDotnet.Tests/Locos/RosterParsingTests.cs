using DCCEXDotnet.Tests.Mocks;
using Moq;

namespace DCCEXDotnet.Tests.Locos
{
    public class RosterParsingTests
    {
        [Fact]
        public void ParseEmptyRoster()
        {
            var protocol = new DCCEXProtocol();
            var stream = new TestStream();
            var mockDelegate = new Mock<IDCCEXProtocolDelegate>();
            protocol.Connect(stream);
            protocol.SetDelegate(mockDelegate.Object);

            Assert.False(protocol.ReceivedRoster());

            protocol.GetLists(true, false, false, false);
            Assert.Equal("<JR>\r\n", stream.GetBuffer());
            stream.ClearBuffer();

            stream.LoadString("<jR>");
            protocol.Check();

            Assert.True(protocol.ReceivedRoster());
        }

        [Fact]
        public void ParseRosterWithThreeIDs()
        {
            var protocol = new DCCEXProtocol();
            var stream = new TestStream();
            var mockDelegate = new Mock<IDCCEXProtocolDelegate>();
            protocol.Connect(stream);
            protocol.SetDelegate(mockDelegate.Object);

            Assert.False(protocol.ReceivedRoster());

            protocol.GetLists(true, false, false, false);
            Assert.Equal("<JR>\r\n", stream.GetBuffer());
            stream.ClearBuffer();

            stream.LoadString("<jR 42 9 120>");
            protocol.Check();

            Assert.False(protocol.ReceivedRoster());

            stream.LoadString("<jR 42 \"Loco42\" \"Func42\">");
            protocol.Check();

            stream.LoadString("<jR 9 \"Loco9\" \"Func9\">");
            protocol.Check();

            stream.LoadString("<jR 120 \"Loco120\" \"Func120\">");
            mockDelegate.Setup(d => d.ReceivedRosterList()).Verifiable();
            protocol.Check();

            Assert.True(protocol.ReceivedRoster());
            mockDelegate.Verify();
        }
    }
}
