namespace DCCEXDotnet.Integration.Tests;

public class SandboxTests
{
    private const string COM_PORT = "COM6";
    private const int BAUD_RATE = 115200;

    private DCCEXProtocol InitializeProtocol()
    {
        var serialPortStream = new SerialPortStream(COM_PORT, BAUD_RATE);
        var protocol = new DCCEXProtocol(1024, 10);
        protocol.Connect(serialPortStream);

        protocol.SetDelegate(new TestDelegate());

        return protocol;
    }
    [Fact]
    public void TestPowerOnOff()
    {
        // Arrange
        var protocol = InitializeProtocol();

        // Act
        protocol.PowerOn();

        Task.Delay(3000).Wait();

        protocol.PowerOff();

        // Assert
        // Add assertions to verify the expected behavior
    }

    [Fact]
    public void TestPowerOff()
    {
        // Arrange
        var protocol = InitializeProtocol();

        // Act
        protocol.PowerOff();
    }

    [Fact]
    public void TestSendTrain()
    {
        // Arrange
        var protocol = InitializeProtocol();

        protocol.PowerOn();

        protocol.GetLists(false, false, true, false);

        Thread.Sleep(2 * 1000); //Wait for a response from DCC-EX

        protocol.Check(); //
        Thread.Sleep(2 * 1000); //Wait for a response from DCC-EX
        protocol.Check(); //I think it needs a 2nd one - the first seems to go into _ProcessRouteList which calls RequestRouteEntry
        Assert.NotNull(protocol.Routes);

        var locoId = 145;
        var automationId = 1;
        var loco = new Loco(locoId, LocoSource.LocoSourceEntry);
        var locoAddress = loco.GetAddress();
        Console.WriteLine("Added loco: " + locoAddress);

        // Act
        //protocol.PowerOn();
        protocol.HandOffLoco(locoId, automationId);

        // Assert
        // Add assertions to verify the expected behavior
    }

    [Fact]
    public void TestSendCommand()
    {
        // Arrange
        var protocol = InitializeProtocol();

        protocol.SendCommand("/START 3 13");

        // Assert
        // Add assertions to verify the expected behavior
    }

    //[Fact]
    //public void TestSendTrainOnAutomation4()
    //{
    //    // Arrange
    //    var serialPortStream = new SerialPortStream("COM3", 9600, Parity.None, 8, StopBits.One);
    //    var protocol = new DCCEXProtocol(serialPortStream);

    //    // Act
    //    protocol.SendAutomationCommand(4);

    //    // Assert
    //    // Add assertions as needed to verify the expected behavior
    //}

    //[Fact]
    //public void TestSendTrainOnAutomation4()
    //{
    //    // Arrange
    //    var serialPortStream = new SerialPortStream(COM_PORT, BAUD_RATE);
    //    var protocol = new DCCEXProtocol(1024, 10);
    //    protocol.Connect(serialPortStream);

    //    // Act
    //    protocol.HandOffLoco(1234, 4); // Assuming loco address 1234

    //    // Assert
    //    // Add assertions as needed to verify the expected behavior
    //}
}
