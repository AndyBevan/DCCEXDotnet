namespace DCCEXProtocol_Basic;

using DCCEXDotnet;
using Shared;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

// DCCEXProtocol library: Basic example in C#
//
// Shows how to create an instance of DCCEXProtocol client
// and how to connect to a DCC-EX Native protocol server using static IP
// Converted from Arduino/ESP32 code to C#
//
// Original authors: Peter Akers (Flash62au), Peter Cole (PeteGSX) and Chris Harlow (UKBloke), 2023
// C# conversion created on April 29, 2025


public class Program
{
    private static DCCEXProtocol dccexProtocol;
    private static CancellationTokenSource cancellationTokenSource;

    static async Task Main(string[] args)
    {
        Console.WriteLine("DCCEXProtocol Basic Demo");
        Console.WriteLine();

        // Create objects
        dccexProtocol = new DCCEXProtocol();
        cancellationTokenSource = new CancellationTokenSource();

        // Setup logging
        dccexProtocol.SetLogStream(new ConsoleStream());

        // Connect to the server
        Console.WriteLine("Connecting to the server...");
        try
        {
            var stream = Utils.GetSerialPortStream();

            // Enable heartbeat
            dccexProtocol.EnableHeartbeat(10000);//Default heartbeat is 60 seconds which is too long for testing

            // Pass the communication to DCCEXProtocol
            dccexProtocol.Connect(stream);
            Console.WriteLine("DCC-EX connected");

            // Start continuous message checking
            await CheckMessagesLoop(cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
            return;
        }
    }

    private static async Task CheckMessagesLoop(CancellationToken cancellationToken)
    {
        // Keep checking for incoming messages until cancelled
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Parse incoming messages
                //Console.WriteLine("Checking for messages...");
                dccexProtocol.Check();
                await Task.Delay(1000, cancellationToken); // Small delay between checks
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in message loop: {ex.Message}");
        }
        finally
        {
            dccexProtocol.Disconnect();
        }
    }
}


