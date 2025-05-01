using DCCEXDotnet;

namespace Shared;

public class ConsoleDelegate : IDCCEXProtocolDelegate
{
    public virtual void ReceivedServerVersion(int major, int minor, int patch)
    {
        Console.WriteLine($"Server Version: {major}.{minor}.{patch}");
    }

    public virtual void ReceivedMessage(string message)
    {
        Console.WriteLine($"Message: {message}");
    }

    public virtual void ReceivedScreenUpdate(int screen, int row, string message)
    {
        Console.WriteLine($"Screen Update - Screen: {screen}, Row: {row}, Message: {message}");
    }

    public virtual void ReceivedLocoUpdate(Loco loco)
    {
        Console.WriteLine($"Loco Update - Address: {loco.GetAddress()}, Speed: {loco.GetSpeed()}, Direction: {loco.GetDirection()}");
    }

    public virtual void ReceivedLocoBroadcast(int address, int speed, Direction direction, int functionMap)
    {
        Console.WriteLine($"Loco Broadcast - Address: {address}, Speed: {speed}, Direction: {direction}, Function Map: {functionMap}");
    }

    public virtual void ReceivedRosterList()
    {
        Console.WriteLine("Roster List Received");
    }

    public virtual void ReceivedTurnoutList()
    {
        Console.WriteLine("Turnout List Received");
    }

    public virtual void ReceivedTurnoutAction(int id, bool thrown)
    {
        Console.WriteLine($"Turnout Action - ID: {id}, Thrown: {thrown}");
    }

    public virtual void ReceivedRouteList()
    {
        Console.WriteLine("Route List Received");
    }

    public virtual void ReceivedTurntableList()
    {
        Console.WriteLine("Turntable List Received");
    }

    public virtual void ReceivedTurntableAction(int id, int newIndex, bool moving)
    {
        Console.WriteLine($"Turntable Action - ID: {id}, New Index: {newIndex}, Moving: {moving}");
    }

    public virtual void ReceivedTrackPower(TrackPower state)
    {
        Console.WriteLine($"Track Power: {state}");
    }

    public virtual void ReceivedIndividualTrackPower(TrackPower state, int track)
    {
        Console.WriteLine($"Individual Track Power - Track: {track}, State: {state}");
    }
    public virtual void ReceivedTrackType(char track, TrackManagerMode trackType, int address)
    {
        Console.WriteLine($"Track Type - Track: {track}, Type: {trackType}, Address: {address}");
    }

    public virtual void ReceivedReadLoco(int address)
    {
        Console.WriteLine($"Read Loco - Address: {address}");
    }

    public virtual void ReceivedWriteLoco(int value)
    {
        Console.WriteLine($"Write Loco - Value: {value}");
    }

    public virtual void ReceivedWriteCV(int cv, int value)
    {
        Console.WriteLine($"Write CV - CV: {cv}, Value: {value}");
    }

    public virtual void ReceivedValidateCV(int cv, int value)
    {
        Console.WriteLine($"Validate CV - CV: {cv}, Value: {value}");
    }

    public virtual void ReceivedValidateCVBit(int cv, int bit, int value)
    {
        Console.WriteLine($"Validate CV Bit - CV: {cv}, Bit: {bit}, Value: {value}");
    }
}
