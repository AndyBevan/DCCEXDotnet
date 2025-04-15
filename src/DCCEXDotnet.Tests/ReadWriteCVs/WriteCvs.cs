using DCCEXDotnet.Tests.Mocks;
using Moq;

namespace DCCEXDotnet.Tests.ReadWriteCVs
{
    public class WriteCvs
    {
        private DCCEXProtocol _protocol;
        private TestStream _stream;
        private Mock<IDCCEXProtocolDelegate> _mockDelegate;

        public WriteCvs()
        {
            _protocol = new DCCEXProtocol();
            _stream = new TestStream();
            _mockDelegate = new Mock<IDCCEXProtocolDelegate>();
            _protocol.SetDelegate(_mockDelegate.Object);
            _protocol.Connect(_stream);
        }


        [Fact]
        public void WriteLocoAddress_ShouldGenerateCorrectCommand()
        {
            _protocol.WriteLocoAddress(1234);
            Assert.Equal("<W 1234>\r\n", _stream.GetBuffer());
        }

        [Fact]
        public void WriteCV_ShouldGenerateCorrectCommand()
        {
            _protocol.WriteCV(1, 3);
            Assert.Equal("<W 1 3>\r\n", _stream.GetBuffer());
        }

        [Fact]
        public void WriteCVBit_ShouldGenerateCorrectCommand()
        {
            _protocol.WriteCVBit(19, 4, 1);
            Assert.Equal("<B 19 4 1>\r\n", _stream.GetBuffer());
        }

        [Fact]
        public void WriteCVOnMain_ShouldGenerateCorrectCommand()
        {
            _protocol.WriteCVOnMain(3, 8, 4);
            Assert.Equal("<w 3 8 4>\r\n", _stream.GetBuffer());
        }

        [Fact]
        public void WriteCVBitOnMain_ShouldGenerateCorrectCommand()
        {
            _protocol.WriteCVBitOnMain(3, 19, 4, 1);
            Assert.Equal("<b 3 19 4 1>\r\n", _stream.GetBuffer());
        }

        [Fact]
        public void WriteLocoAddressResponse_ShouldCallReceivedWriteLoco()
        {
            _stream.LoadString("<w 1234>");
            _mockDelegate.Setup(d => d.ReceivedWriteLoco(1234)).Verifiable();
            _protocol.Check();
            _mockDelegate.Verify();
        }

        [Fact]
        public void WriteCVResponse_ShouldCallReceivedWriteCV()
        {
            _stream.LoadString("<r 1 3>");
            _mockDelegate.Setup(d => d.ReceivedWriteCV(1, 3)).Verifiable();
            _protocol.Check();
            _mockDelegate.Verify();
        }
    }
}
