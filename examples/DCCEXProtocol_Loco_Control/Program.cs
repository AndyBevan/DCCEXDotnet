namespace DCCEXProtocol_Loco_Control;

using DCCEXDotnet;
using Shared;

// DCCEXProtocol library: Loco control example in C#
//
// Shows how to control a single loco
// Converted from Arduino/ESP32 code to C#
//
// Original authors: Peter Akers (Flash62au), Peter Cole (PeteGSX) and Chris Harlow (UKBloke), 2023
// C# conversion created on April 29, 2025

using System;
using System.Threading;
using System.Threading.Tasks;

public class Program
{
    // Configuration properties that would be in config.h
    // Global objects

    private static DCCEXProtocol dccexProtocol;
    private static MyDelegate myDelegate;
    private static CancellationTokenSource cancellationTokenSource;

    // For random speed changes
    private static int speed = 50;
    private static int up = 1;
    private static DateTime lastTime;

    // Define our loco object
    private static Loco loco = null;

    static async Task Main(string[] args)
    {
        Console.WriteLine("DCCEXProtocol Loco Control Demo");
        Console.WriteLine();

        dccexProtocol = new DCCEXProtocol();
        myDelegate = new MyDelegate();
        cancellationTokenSource = new CancellationTokenSource();

        // Setup logging
        dccexProtocol.SetLogStream(new ConsoleStream());

        // Pass the delegate instance to DCCEXProtocol
        dccexProtocol.SetDelegate(myDelegate);

        // Connect to the server
        //Console.WriteLine("Connecting to the server...");
        try
        {
            var stream = Utils.GetSerialPortStream();

            // Pass the communication to DCCEXProtocol
            dccexProtocol.Connect(stream);
            Console.WriteLine("DCC-EX connected");

            // Request server version
            dccexProtocol.RequestServerVersion();

            // Set initial time
            lastTime = DateTime.Now;

            // Start the main loop
            await MainLoop(cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
            return;
        }
    }

    private static async Task MainLoop(CancellationToken cancellationToken)
    {
        Random random = new Random();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Parse incoming messages
                dccexProtocol.Check();

                if (loco == null)
                {
                    // Add a loco with DCC address 11 - LocoSourceEntry means it's not from the roster
                    loco = new Loco(145, LocoSource.LocoSourceEntry);
                    Console.WriteLine($"Added loco: {loco.GetAddress()}");

                    // Turn track power on or the loco won't move
                    dccexProtocol.PowerOn();
                }

                if (loco != null)
                {
                    // Every 10 seconds change speed and set a random function on or off
                    TimeSpan elapsed = DateTime.Now - lastTime;
                    if (elapsed.TotalMilliseconds >= 10000)
                    {
                        if (speed >= 100)
                            up = -1;
                        if (speed <= 0)
                            up = 1;

                        speed = speed + up;
                        dccexProtocol.SetThrottle(loco, speed, Direction.Forward);

                        int fn = random.Next(0, 27);
                        int fns = random.Next(0, 100);
                        bool fnState = fns < 50 ? false : true;

                        if (fnState)
                        {
                            dccexProtocol.FunctionOn(loco, fn);
                        }
                        else
                        {
                            dccexProtocol.FunctionOff(loco, fn);
                        }

                        lastTime = DateTime.Now;
                    }
                }

                await Task.Delay(10, cancellationToken); // Small delay between checks
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in main loop: {ex.Message}");
        }
        finally
        {
            dccexProtocol.Disconnect();
            // Clean up
            //client?.Close();
            //stream?.Dispose();
        }
    }
}
