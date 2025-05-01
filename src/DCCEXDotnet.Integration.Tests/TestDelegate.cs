namespace DCCEXDotnet.Integration.Tests
{
    public class TestDelegate : IDCCEXProtocolDelegate
    {
        public void ReceivedServerVersion(int major, int minor, int patch)
        {
            Console.WriteLine($"Server Version: {major}.{minor}.{patch}");
        }

        public void ReceivedMessage(string message)
        {
            Console.WriteLine($"Message: {message}");
        }

        public void ReceivedScreenUpdate(int screen, int row, string message)
        {
            Console.WriteLine($"Screen Update - Screen: {screen}, Row: {row}, Message: {message}");
        }

        public void ReceivedLocoUpdate(Loco loco)
        {
            Console.WriteLine($"Loco Update - Address: {loco.GetAddress()}, Speed: {loco.GetSpeed()}, Direction: {loco.GetDirection()}");
        }

        public void ReceivedLocoBroadcast(int address, int speed, Direction direction, int functionMap)
        {
            Console.WriteLine($"Loco Broadcast - Address: {address}, Speed: {speed}, Direction: {direction}, Function Map: {functionMap}");
        }

        public void ReceivedRosterList()
        {
            Console.WriteLine("Roster List Received");
        }

        public void ReceivedTurnoutList()
        {
            Console.WriteLine("Turnout List Received");
        }

        public void ReceivedTurnoutAction(int id, bool thrown)
        {
            Console.WriteLine($"Turnout Action - ID: {id}, Thrown: {thrown}");
        }

        public void ReceivedRouteList()
        {
            Console.WriteLine("Route List Received");
        }

        public void ReceivedTurntableList()
        {
            Console.WriteLine("Turntable List Received");
        }

        public void ReceivedTurntableAction(int id, int newIndex, bool moving)
        {
            Console.WriteLine($"Turntable Action - ID: {id}, New Index: {newIndex}, Moving: {moving}");
        }

        public void ReceivedTrackPower(TrackPower state)
        {
            Console.WriteLine($"Track Power: {state}");
        }

        public void ReceivedIndividualTrackPower(TrackPower state, int track)
        {
            Console.WriteLine($"Individual Track Power - Track: {track}, State: {state}");
        }
        public void ReceivedTrackType(char track, TrackManagerMode trackType, int address)
        {
            Console.WriteLine($"Track Type - Track: {track}, Type: {trackType}, Address: {address}");
        }

        public void ReceivedReadLoco(int address)
        {
            Console.WriteLine($"Read Loco - Address: {address}");
        }

        public void ReceivedWriteLoco(int value)
        {
            Console.WriteLine($"Write Loco - Value: {value}");
        }

        public void ReceivedWriteCV(int cv, int value)
        {
            Console.WriteLine($"Write CV - CV: {cv}, Value: {value}");
        }

        public void ReceivedValidateCV(int cv, int value)
        {
            Console.WriteLine($"Validate CV - CV: {cv}, Value: {value}");
        }

        public void ReceivedValidateCVBit(int cv, int bit, int value)
        {
            Console.WriteLine($"Validate CV Bit - CV: {cv}, Bit: {bit}, Value: {value}");
        }
    }
}
