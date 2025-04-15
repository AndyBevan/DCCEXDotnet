namespace DCCEXDotnet.Tests.Mocks
{
    public class MockDelegate : IDCCEXProtocolDelegate
    {
        public List<(int, int, int)> ServerVersions = new();
        public List<string> Messages = new();
        public List<(int Screen, int Row, string Message)> ScreenUpdates = new();
        public List<Loco> LocoUpdates = new();
        public List<(int Address, int Speed, Direction Direction, int FunctionMap)> LocoBroadcasts = new();
        public bool RosterListReceived = false;
        public bool TurnoutListReceived = false;
        public List<(int Id, bool Thrown)> TurnoutActions = new();
        public bool RouteListReceived = false;
        public bool TurntableListReceived = false;
        public List<(int Id, int NewIndex, bool Moving)> TurntableActions = new();
        public List<TrackPower> TrackPowerStates = new();
        public List<(TrackPower State, int Track)> IndividualTrackPowers = new();
        public List<(char Track, TrackManagerMode Mode, int Address)> TrackTypes = new();
        public List<int> ReadLocos = new();
        public List<int> WriteLocos = new();
        public List<(int CV, int Value)> WriteCVs = new();
        public List<(int CV, int Value)> ValidateCVs = new();
        public List<(int CV, int Bit, int Value)> ValidateCVBits = new();

        public void ReceivedServerVersion(int major, int minor, int patch) => ServerVersions.Add((major, minor, patch));
        public void ReceivedMessage(string message) => Messages.Add(message);
        public void ReceivedScreenUpdate(int screen, int row, string message) => ScreenUpdates.Add((screen, row, message));
        public void ReceivedLocoUpdate(Loco loco) => LocoUpdates.Add(loco);
        public void ReceivedLocoBroadcast(int address, int speed, Direction direction, int functionMap) => LocoBroadcasts.Add((address, speed, direction, functionMap));
        public void ReceivedRosterList() => RosterListReceived = true;
        public void ReceivedTurnoutList() => TurnoutListReceived = true;
        public void ReceivedTurnoutAction(int id, bool thrown) => TurnoutActions.Add((id, thrown));
        public void ReceivedRouteList() => RouteListReceived = true;
        public void ReceivedTurntableList() => TurntableListReceived = true;
        public void ReceivedTurntableAction(int id, int newIndex, bool moving) => TurntableActions.Add((id, newIndex, moving));
        public void ReceivedTrackPower(TrackPower state) => TrackPowerStates.Add(state);
        public void ReceivedIndividualTrackPower(TrackPower state, int track) => IndividualTrackPowers.Add((state, track));
        public void ReceivedTrackType(char track, TrackManagerMode trackType, int address) => TrackTypes.Add((track, trackType, address));
        public void ReceivedReadLoco(int address) => ReadLocos.Add(address);
        public void ReceivedWriteLoco(int value) => WriteLocos.Add(value);
        public void ReceivedWriteCV(int cv, int value) => WriteCVs.Add((cv, value));
        public void ReceivedValidateCV(int cv, int value) => ValidateCVs.Add((cv, value));
        public void ReceivedValidateCVBit(int cv, int bit, int value) => ValidateCVBits.Add((cv, bit, value));

        public bool ReceivedServerVersionCalledWith(int major, int minor, int patch) => ServerVersions.Contains((major, minor, patch));
        public bool ReceivedMessageCalledWith(string message)
        {
            return Messages.Contains(message);
        }

        public bool ReceivedTrackPowerCalledWith(TrackPower power)
        {
            return TrackPowerStates.Contains(power);
        }
    }
}
