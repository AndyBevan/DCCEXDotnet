using DCCEXDotnet.Tests.Mocks;

namespace DCCEXDotnet.Tests.General
{

    public class VersionTests
    {
        private readonly DCCEXProtocol _dccexProtocol;
        private readonly TestStream _stream;
        private readonly MockDelegate _delegate;

        public VersionTests()
        {
            _stream = new TestStream();
            _delegate = new MockDelegate();
            _dccexProtocol = new DCCEXProtocol();
            _dccexProtocol.SetDelegate(_delegate);
            _dccexProtocol.Connect(_stream);
        }

        [Fact]
        public void Request_ShouldSendRequestServerVersionCommand()
        {
            _dccexProtocol.RequestServerVersion();
            Assert.Equal("<s>\r\n", _stream.GetBuffer());
            _stream.ClearBuffer();
        }

        [Fact]
        public void VersionJustZeros_ShouldParseVersionCorrectly()
        {
            Assert.False(_dccexProtocol.ReceivedVersion());
            _stream.LoadString("<iDCCEX V-0.0.0 / MEGA / STANDARD_MOTOR_SHIELD / 7>");
            _dccexProtocol.Check();
            Assert.True(_delegate.ReceivedServerVersionCalledWith(0, 0, 0));
            Assert.True(_dccexProtocol.ReceivedVersion());
            Assert.Equal(0, _dccexProtocol.GetMajorVersion());
            Assert.Equal(0, _dccexProtocol.GetMinorVersion());
            Assert.Equal(0, _dccexProtocol.GetPatchVersion());
        }

        [Fact]
        public void VersionSingleDigits_ShouldParseVersionCorrectly()
        {
            Assert.False(_dccexProtocol.ReceivedVersion());
            _stream.LoadString("<iDCCEX V-1.2.3 / MEGA / STANDARD_MOTOR_SHIELD / 7>");
            _dccexProtocol.Check();
            Assert.True(_delegate.ReceivedServerVersionCalledWith(1, 2, 3));
            Assert.True(_dccexProtocol.ReceivedVersion());
            Assert.Equal(1, _dccexProtocol.GetMajorVersion());
            Assert.Equal(2, _dccexProtocol.GetMinorVersion());
            Assert.Equal(3, _dccexProtocol.GetPatchVersion());
        }

        [Fact]
        public void VersionMultipleDigits_ShouldParseVersionCorrectly()
        {
            Assert.False(_dccexProtocol.ReceivedVersion());
            _stream.LoadString("<iDCCEX V-92.210.10 / MEGA / STANDARD_MOTOR_SHIELD / 7>");
            _dccexProtocol.Check();
            Assert.True(_delegate.ReceivedServerVersionCalledWith(92, 210, 10));
            Assert.True(_dccexProtocol.ReceivedVersion());
            Assert.Equal(92, _dccexProtocol.GetMajorVersion());
            Assert.Equal(210, _dccexProtocol.GetMinorVersion());
            Assert.Equal(10, _dccexProtocol.GetPatchVersion());
        }

        [Fact]
        public void VersionIgnoreLabels_ShouldStillParseVersionCorrectly()
        {
            Assert.False(_dccexProtocol.ReceivedVersion());
            _stream.LoadString("<iDCCEX V-1.2.3-smartass / MEGA / STANDARD_MOTOR_SHIELD / 7>");
            _dccexProtocol.Check();
            Assert.True(_delegate.ReceivedServerVersionCalledWith(1, 2, 3));
            Assert.True(_dccexProtocol.ReceivedVersion());
            Assert.Equal(1, _dccexProtocol.GetMajorVersion());
            Assert.Equal(2, _dccexProtocol.GetMinorVersion());
            Assert.Equal(3, _dccexProtocol.GetPatchVersion());
        }
    }
}
