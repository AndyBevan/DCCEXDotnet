using DCCEXDotnet.Tests.Mocks;
using Moq;

namespace DCCEXDotnet.Tests.ReadWriteCVs
{
    public class ReadCVs
    {
        [Fact]
        public void ReadAddressCVResponse_ShouldCallReceivedReadLoco()
        {
            var protocol = new DCCEXProtocol();
            var stream = new TestStream();
            var mockDelegate = new Mock<IDCCEXProtocolDelegate>();
            protocol.Connect(stream);
            protocol.SetDelegate(mockDelegate.Object);

            stream.LoadString("<r 1234>");
            mockDelegate.Setup(d => d.ReceivedReadLoco(1234)).Verifiable();
            protocol.Check();

            mockDelegate.Verify();
        }

        [Fact]
        public void ValidateCVResponse_ShouldCallReceivedValidateCV()
        {
            var protocol = new DCCEXProtocol();
            var stream = new TestStream();
            var mockDelegate = new Mock<IDCCEXProtocolDelegate>();
            protocol.Connect(stream);
            protocol.SetDelegate(mockDelegate.Object);

            stream.LoadString("<v 1 3>");
            mockDelegate.Setup(d => d.ReceivedValidateCV(1, 3)).Verifiable();
            protocol.Check();

            mockDelegate.Verify();
        }

        [Fact]
        public void ValidateCVBitResponse_ShouldCallReceivedValidateCVBit()
        {
            var protocol = new DCCEXProtocol();
            var stream = new TestStream();
            var mockDelegate = new Mock<IDCCEXProtocolDelegate>();
            protocol.Connect(stream);
            protocol.SetDelegate(mockDelegate.Object);

            stream.LoadString("<v 1 3 1>");
            mockDelegate.Setup(d => d.ReceivedValidateCVBit(1, 3, 1)).Verifiable();
            protocol.Check();

            mockDelegate.Verify();
        }
    }
}
