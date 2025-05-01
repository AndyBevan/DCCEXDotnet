using DCCEXDotnet;
using Shared;
using System.Net.Sockets;

namespace DCCEXProtocol_Roster_etc;

public class Program
{
    // Delegate class
    class MyDelegate : ConsoleDelegate
    {
        public override void ReceivedServerVersion(int major, int minor, int patch)
        {
            Console.Write("\n\nReceived version: ");
            Console.Write($"{major}.{minor}.{patch}");
            Console.WriteLine();
        }

        public override void ReceivedTrackPower(TrackPower state)
        {
            Console.Write("\n\nReceived Track Power: ");
            Console.WriteLine(state);
            Console.WriteLine("\n\n");
        }

        public override void ReceivedRosterList()
        {
            Console.WriteLine("\n\nReceived Roster");
            PrintRoster();
        }

        public override void ReceivedTurnoutList()
        {
            Console.Write("\n\nReceived Turnouts/Points list");
            PrintTurnouts();
            Console.WriteLine("\n\n");
        }

        public override void ReceivedRouteList()
        {
            Console.Write("\n\nReceived Routes List");
            PrintRoutes();
            Console.WriteLine("\n\n");
        }

        public override void ReceivedTurntableList()
        {
            Console.Write("\n\nReceived Turntables list");
            PrintTurntables();
            Console.WriteLine("\n\n");
        }
    }

    static long lastTime = 0;
    static bool done = false;

    // Global objects
    static DCCEXProtocol dccexProtocol;
    static MyDelegate myDelegate;
    static CancellationTokenSource cancellationTokenSource;

    static void PrintRoster()
    {
        for (Loco loco = dccexProtocol.Roster?.GetFirstInstance(); loco != null; loco = loco.GetNext())
        {
            int id = loco.GetAddress();
            string name = loco.GetName();
            Console.Write(id);
            Console.Write(" ~");
            Console.Write(name);
            Console.WriteLine("~");
            for (int i = 0; i < 32; i++)
            {
                string fName = loco.GetFunctionName(i);
                if (fName != null)
                {
                    Console.Write("loadFunctionLabels() ");
                    Console.Write(fName);
                    if (loco.IsFunctionMomentary(i))
                    {
                        Console.Write(" - Momentary");
                    }
                    Console.WriteLine();
                }
            }
        }
        Console.WriteLine("\n");
    }

    static void PrintTurnouts()
    {
        for (Turnout turnout = dccexProtocol?.Turnouts?.GetFirstInstance(); turnout != null; turnout = turnout.GetNext())
        {
            int id = turnout.GetId();
            string name = turnout.GetName();
            Console.Write(id);
            Console.Write(" ~");
            Console.Write(name);
            Console.WriteLine("~");
        }
        Console.WriteLine("\n");
    }

    static void PrintRoutes()
    {
        for (Route route = dccexProtocol?.Routes?.GetFirstInstance(); route != null; route = route.GetNext())
        {
            int id = route.GetId();
            string name = route.GetName();
            Console.Write(id);
            Console.Write(" ~");
            Console.Write(name);
            Console.WriteLine("~");
        }
        Console.WriteLine("\n");
    }

    static void PrintTurntables()
    {
        for (Turntable turntable = dccexProtocol?.Turntables?.GetFirstInstance(); turntable != null; turntable = turntable.GetNext())
        {
            int id = turntable.GetId();
            string name = turntable.GetName();
            Console.Write(id);
            Console.Write(" ~");
            Console.Write(name);
            Console.WriteLine("~");

            int j = 0;
            for (TurntableIndex turntableIndex = turntable.GetFirstIndex(); turntableIndex != null;
                turntableIndex = turntableIndex.GetNextIndex())
            {
                string indexName = turntableIndex.GetName();
                Console.Write("  index");
                Console.Write(j);
                Console.Write(" ~");
                Console.Write(indexName);
                Console.WriteLine("~");
                j++;
            }
        }
        Console.WriteLine("\n");
    }

    static async Task Main(string[] args)
    {
        Console.WriteLine("DCCEXProtocol Roster and Objects Demo");
        Console.WriteLine();

        // Create objects
        dccexProtocol = new DCCEXProtocol();
        myDelegate = new MyDelegate();
        cancellationTokenSource = new CancellationTokenSource();

        // Connect to the server
        Console.WriteLine("Connecting to the server...");
        try
        {
            var stream = Utils.GetSerialPortStream();

            // Enable logging on Console
            dccexProtocol.SetLogStream(new ConsoleStream());

            // Pass the delegate instance to dccexProtocol
            dccexProtocol.SetDelegate(myDelegate);

            // Pass the communication to dccexProtocol
            dccexProtocol.Connect(stream);
            Console.WriteLine("DCC-EX connected");

            dccexProtocol.RequestServerVersion();
            dccexProtocol.PowerOn();

            // Start the loop as a background task
            var loopTask = RunLoop(cancellationTokenSource.Token);

            // Keep the application running until user presses a key
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);

            // Cancel the loop
            cancellationTokenSource.Cancel();

            // Wait for the loop to finish
            await loopTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
        }
        finally
        {
            dccexProtocol.Disconnect();
        }
    }

    static async Task RunLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Parse incoming messages
                dccexProtocol.Check();

                // Sequentially request and get the required lists. To avoid overloading the buffer
                // GetLists(bool rosterRequired, bool turnoutListRequired, bool routeListRequired, bool turntableListRequired)
                dccexProtocol.GetLists(true, true, true, true);

                // Add a delay between iterations
                await Task.Delay(100, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // This is expected when cancellation is requested
            Console.WriteLine("Loop canceled.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in loop: {ex.Message}");
        }
    }
}
