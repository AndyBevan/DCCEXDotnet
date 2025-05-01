using DCCEXDotnet;
using Shared;

namespace DCCEXProtocol_Loco_Control;
// C# version of the MyDelegate class
public class MyDelegate : ConsoleDelegate
{
    public override void ReceivedServerVersion(int major, int minor, int patch)
    {
        Console.WriteLine($"\n\nReceived version: {major}.{minor}.{patch}");
    }

    public override void ReceivedTrackPower(TrackPower state)
    {
        Console.WriteLine($"\n\nReceived Track Power: {state}\n\n");
    }

    // Use for roster Locos (LocoSource.LocoSourceRoster)
    public override void ReceivedLocoUpdate(Loco loco)
    {
        Console.WriteLine($"Received Loco update for DCC address: {loco.GetAddress()}");
    }

    // Use for locally created Locos (LocoSource.LocoSourceEntry)
    public override void ReceivedLocoBroadcast(int address, int speed, Direction direction, int functionMap)
    {
        Console.Write("\n\nReceived Loco broadcast: address|speed|direction|functionMap: ");
        Console.Write($"{address}|{speed}|");

        if (direction == Direction.Forward)
        {
            Console.Write("Fwd");
        }
        else
        {
            Console.Write("Rev");
        }

        Console.Write("|");
        Console.WriteLine(functionMap);
    }
}

