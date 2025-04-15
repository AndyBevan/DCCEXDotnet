/*
 * DCCEXProtocol
 *
 * This package implements a DCCEX native protocol connection,
 * allow a device to communicate with a DCC-EX EX-CommandStation.
 *
 * Copyright © 2025 Andy Bevan
 *
 * This work is licensed under the Creative Commons Attribution-ShareAlike
 * 4.0 International License. To view a copy of this license, visit
 * http://creativecommons.org/licenses/by-sa/4.0/ or send a letter to
 * Creative Commons, PO Box 1866, Mountain View, CA 94042, USA.
 *
 * Attribution — You must give appropriate credit, provide a link to the
 * license, and indicate if changes were made. You may do so in any
 * reasonable manner, but not in any way that suggests the licensor
 * endorses you or your use.
 *
 * ShareAlike — If you remix, transform, or build upon the material, you
 * must distribute your contributions under the same license as the
 * original.
 *
 * All other rights reserved.
 */

/*
Conventions

Function/method prefixes
- Received... = notification to the client app (_delegate)
- Send... = sends a command to the CS
- Process... - process a response from the CS
- Init... - initialize an object
- Set... = Sets internal variables/values. May subsequently call a 'send'
- Get... - Gets internal variables/values. May subsequently call a 'send'
*/

namespace DCCEXDotnet;

public class DCCEXProtocol
{
    private const int MIN_SPEED = 0;
    private const int MAX_SPEED = 126;

    // Stream handling
    private IStream _stream;
    private IStream _console;
    private NullStream _nullStream = new NullStream();

    // Command buffer
    private string _cmdBuffer;
    private int _maxCmdBuffer;
    private int _bufflen;
    private string _outboundCommand;
    private char[] _inputBuffer = new char[1024]; // Adjust size as needed
    private int _nextChar;

    // Delegate
    private IDCCEXProtocolDelegate _delegate;

    // Heartbeat
    private bool _enableHeartbeat;
    private long _heartbeatDelay;
    private long _lastHeartbeat;

    // Server info
    private int[] _version = new int[3];
    private bool _receivedVersion;
    private long _lastServerResponseTime;

    // List tracking
    private bool _receivedLists;
    private bool _rosterRequested;
    private bool _receivedRoster;
    private bool _turnoutListRequested;
    private bool _receivedTurnoutList;
    private bool _routeListRequested;
    private bool _receivedRouteList;
    private bool _turntableListRequested;
    private bool _receivedTurntableList;

    // Object counts
    private int _rosterCount;
    private int _turnoutCount;
    private int _routeCount;
    private int _turntableCount;

    // Object collections - using static properties in the classes instead
    public Loco Roster => Loco.GetFirst();
    public Turnout Turnouts => Turnout.GetFirst();
    public Route Routes => Route.GetFirst();
    public Turntable turntables => Turntable.GetFirst();

    public DCCEXProtocol(int maxCmdBuffer = 500, int maxCommandParams = 50)
    {
        // Init streams
        _stream = _nullStream;
        _console = _nullStream;

        // Allocate memory for command buffer
        _cmdBuffer = "";
        _maxCmdBuffer = maxCmdBuffer;

        // Setup command parser
        DCCEXInbound.Setup(maxCommandParams);
        _bufflen = 0;

        // Set heartbeat defaults
        _enableHeartbeat = false;
        _heartbeatDelay = 0;
        _lastHeartbeat = 0;
    }

    ~DCCEXProtocol()
    {
        // Cleanup command parser
        DCCEXInbound.Cleanup();
    }

    // Set the delegate instance for callbacks
    public void SetDelegate(IDCCEXProtocolDelegate delegateInstance)
    {
        _delegate = delegateInstance;
    }

    // Set the Stream used for logging
    public void SetLogStream(IStream console)
    {
        _console = console;
    }

    public void EnableHeartbeat(long heartbeatDelay)
    {
        _enableHeartbeat = true;
        _heartbeatDelay = heartbeatDelay;
    }

    public void Connect(IStream stream)
    {
        _Init();
        _stream = stream;
    }

    public void Disconnect()
    {
        _outboundCommand = "<U DISCONNECT>";
        _SendCommand();
        _stream = null;
    }

    public void Check()
    {
        if (_stream != null)
        {
            while (_stream.Available() > 0)
            {
                // Read from our stream
                int r = _stream.Read();
                if (r == -1)
                    continue;

                if (_bufflen < _maxCmdBuffer - 1)
                {
                    _cmdBuffer += (char)r;
                    _bufflen++;
                }
                else
                {
                    // Clear buffer if full
                    _cmdBuffer = "";
                    _bufflen = 0;
                }

                if (r == '>')
                {
                    if (DCCEXInbound.Parse(_cmdBuffer))
                    {
                        // Process stuff here
                        _console.Print("<== ");
                        _console.Println(_cmdBuffer);
                        _ProcessCommand();
                    }
                    // Clear buffer after use
                    _cmdBuffer = "";
                    _bufflen = 0;
                }
            }
            if (_enableHeartbeat)
            {
                _SendHeartbeat();
            }
        }
    }

    public void SendCommand(string cmd)
    {
        _outboundCommand = $"<{cmd}>";
        _SendCommand();
    }

    // Sequentially request and get the required lists to avoid overloading the buffer
    public void GetLists(bool rosterRequired, bool turnoutListRequired, bool routeListRequired, bool turntableListRequired)
    {
        if (!_receivedLists)
        {
            if (rosterRequired && !_rosterRequested)
            {
                _GetRoster();
            }
            else if (!rosterRequired || _receivedRoster)
            {
                if (turnoutListRequired && !_turnoutListRequested)
                {
                    _GetTurnouts();
                }
                else if (!turnoutListRequired || _receivedTurnoutList)
                {
                    if (routeListRequired && !_routeListRequested)
                    {
                        _GetRoutes();
                    }
                    else if (!routeListRequired || _receivedRouteList)
                    {
                        if (turntableListRequired && !_turntableListRequested)
                        {
                            _GetTurntables();
                        }
                        else if (!turntableListRequired || _receivedTurntableList)
                        {
                            _receivedLists = true;
                        }
                    }
                }
            }
        }
    }

    public bool ReceivedLists() { return _receivedLists; }

    public void RequestServerVersion()
    {
        if (_delegate != null)
        {
            _outboundCommand = "<s>";
            _SendCommand();
        }
    }

    public bool ReceivedVersion() { return _receivedVersion; }
    public int GetMajorVersion() { return _version[0]; }
    public int GetMinorVersion() { return _version[1]; }
    public int GetPatchVersion() { return _version[2]; }
    public long GetLastServerResponseTime() { return _lastServerResponseTime; }

    public void ClearAllLists()
    {
        ClearRoster();
        ClearTurnoutList();
        ClearTurntableList();
        ClearRouteList();
    }

    public void RefreshAllLists()
    {
        RefreshRoster();
        RefreshTurnoutList();
        RefreshTurntableList();
        RefreshRouteList();
    }

    // Consist/loco methods
    public void SetThrottle(Loco loco, int speed, Direction direction)
    {
        if (_delegate != null)
        {
            int address = loco.GetAddress();
            _SetLoco(address, speed, direction);
        }
    }

    public void SetThrottle(Consist consist, int speed, Direction direction)
    {
        if (_delegate != null)
        {
            for (ConsistLoco cl = consist.GetFirst(); cl != null; cl = cl.GetNext())
            {
                int address = cl.GetLoco().GetAddress();
                Direction dir = direction;
                if (cl.GetFacing() == Facing.FacingReversed)
                {
                    dir = direction == Direction.Forward ? Direction.Reverse : Direction.Forward;
                }
                _SetLoco(address, speed, dir);
            }
        }
    }

    public void FunctionOn(Loco loco, int function)
    {
        if (_delegate != null)
        {
            int address = loco.GetAddress();
            if (address >= 0)
            {
                _outboundCommand = $"<F {address} {function} 1>";
                _SendCommand();
            }
        }
    }

    public void FunctionOff(Loco loco, int function)
    {
        if (_delegate != null)
        {
            int address = loco.GetAddress();
            if (address >= 0)
            {
                _outboundCommand = $"<F {address} {function} 0>";
                _SendCommand();
            }
        }
    }

    public bool IsFunctionOn(Loco loco, int function)
    {
        if (_delegate != null)
        {
            return loco.IsFunctionOn(function);
        }
        return false;
    }

    public void FunctionOn(Consist consist, int function)
    {
        if (_delegate != null)
        {
            for (ConsistLoco cl = consist.GetFirst(); cl != null; cl = cl.GetNext())
            {
                FunctionOn(cl.GetLoco(), function);
            }
        }
    }

    public void FunctionOff(Consist consist, int function)
    {
        if (_delegate != null)
        {
            for (ConsistLoco cl = consist.GetFirst(); cl != null; cl = cl.GetNext())
            {
                FunctionOff(cl.GetLoco(), function);
            }
        }
    }

    public bool IsFunctionOn(Consist consist, int function)
    {
        if (_delegate != null)
        {
            ConsistLoco firstCL = consist.GetFirst();
            return firstCL.GetLoco().IsFunctionOn(function);
        }
        return false;
    }

    public void RequestLocoUpdate(int address)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<t {address}>";
            _SendCommand();
        }
    }

    public void ReadLoco()
    {
        if (_delegate != null)
        {
            _outboundCommand = "<R>";
            _SendCommand();
        }
    }

    public void EmergencyStop()
    {
        if (_delegate != null)
        {
            _outboundCommand = "<!>";
            _SendCommand();
        }
    }

    // Roster methods
    public int GetRosterCount() { return _rosterCount; }
    public bool ReceivedRoster() { return _receivedRoster; }

    public Loco FindLocoInRoster(int address)
    {
        for (Loco r = Roster; r != null; r = r.GetNext())
        {
            if (r.GetAddress() == address)
            {
                return r;
            }
        }
        return null;
    }

    public void ClearRoster()
    {
        Loco.ClearRoster();
        _rosterCount = 0;
    }

    public void RefreshRoster()
    {
        ClearRoster();
        _receivedLists = false;
        _receivedRoster = false;
        _rosterRequested = false;
    }

    // Turnout methods
    public int GetTurnoutCount() { return _turnoutCount; }
    public bool ReceivedTurnoutList() { return _receivedTurnoutList; }

    // Find the turnout/point in the turnout list by id. Return a pointer or null if not found
    public Turnout GetTurnoutById(int turnoutId)
    {
        for (Turnout turnout = Turnouts; turnout != null; turnout = turnout.GetNext())
        {
            if (turnout.GetId() == turnoutId)
            {
                return turnout;
            }
        }
        return null;
    }

    public void CloseTurnout(int turnoutId)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<T {turnoutId} 0>";
            _SendCommand();
        }
    }

    public void ThrowTurnout(int turnoutId)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<T {turnoutId} 1>";
            _SendCommand();
        }
    }

    public void ToggleTurnout(int turnoutId)
    {
        for (Turnout t = Turnouts; t != null; t = t.GetNext())
        {
            if (t.GetId() == turnoutId)
            {
                bool thrown = !t.GetThrown();
                _outboundCommand = $"<T {turnoutId} {(thrown ? 1 : 0)}>";
                _SendCommand();
            }
        }
    }

    public void ClearTurnoutList()
    {
        Turnout.ClearTurnoutList();
        _turnoutCount = 0;
    }

    public void RefreshTurnoutList()
    {
        ClearTurnoutList();
        _receivedLists = false;
        _receivedTurnoutList = false;
        _turnoutListRequested = false;
    }

    // Route methods
    public int GetRouteCount() { return _routeCount; }
    public bool ReceivedRouteList() { return _receivedRouteList; }

    public void StartRoute(int routeId)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"</ START {routeId}>";
            _SendCommand();
        }
    }

    public void HandOffLoco(int locoAddress, int automationId)
    {
        if (_delegate != null)
        {
            Route automation = Routes != null ? Route.GetById(automationId) : null;
            if (automation == null || automation.GetType() != RouteType.RouteTypeAutomation)
                return;
            _outboundCommand = $"</ START {locoAddress} {automationId}>";
            _SendCommand();
        }
    }

    public void PauseRoutes()
    {
        if (_delegate != null)
        {
            _outboundCommand = "</PAUSE>";
            _SendCommand();
        }
    }

    public void ResumeRoutes()
    {
        if (_delegate != null)
        {
            _outboundCommand = "</RESUME>";
            _SendCommand();
        }
    }

    public void ClearRouteList()
    {
        Route.ClearRouteList();
        _routeCount = 0;
    }

public void RefreshRouteList()
    {
        ClearRouteList();
        _receivedLists = false;
        _receivedRouteList = false;
        _routeListRequested = false;
    }

    // Turntable methods
    public int GetTurntableCount() { return _turntableCount; }
    public bool ReceivedTurntableList() { return _receivedTurntableList; }

    public Turntable GetTurntableById(int turntableId)
    {
        for (Turntable tt = turntables; tt != null; tt = tt.GetNext())
        {
            if (tt.GetId() == turntableId)
            {
                return tt;
            }
        }
        return null;
    }

    public void RotateTurntable(int turntableId, int position, int activity)
    {
        if (_delegate != null)
        {
            Turntable tt = turntables != null ? Turntable.GetById(turntableId) : null;
            if (tt != null)
            {
                if (tt.GetType() == TurntableType.TurntableTypeEXTT)
                {
                    if (position == 0)
                    {
                        activity = 2;
                    }
                    _outboundCommand = $"<I {turntableId} {position} {activity}>";
                }
                else
                {
                    _outboundCommand = $"<I {turntableId} {position}>";
                }
            }
            _SendCommand();
        }
    }

    public void ClearTurntableList()
    {
        Turntable.ClearTurntableList();
        _turntableCount = 0;
    }

    public void RefreshTurntableList()
    {
        ClearTurntableList();
        _receivedLists = false;
        _receivedTurntableList = false;
        _turntableListRequested = false;
    }

    // Track management methods
    public void PowerOn()
    {
        if (_delegate != null)
        {
            _outboundCommand = "<1>";
            _SendCommand();
        }
    }

    public void PowerOff()
    {
        if (_delegate != null)
        {
            _outboundCommand = "<0>";
            _SendCommand();
        }
    }

    public void PowerMainOn()
    {
        if (_delegate != null)
        {
            _outboundCommand = "<1 MAIN>";
            _SendCommand();
        }
    }

    public void PowerMainOff()
    {
        if (_delegate != null)
        {
            _outboundCommand = "<0 MAIN>";
            _SendCommand();
        }
    }

    public void PowerProgOn()
    {
        if (_delegate != null)
        {
            _outboundCommand = "<1 PROG>";
            _SendCommand();
        }
    }

    public void PowerProgOff()
    {
        if (_delegate != null)
        {
            _outboundCommand = "<0 PROG>";
            _SendCommand();
        }
    }

    public void JoinProg()
    {
        if (_delegate != null)
        {
            _outboundCommand = "<1 JOIN>";
            _SendCommand();
        }
    }

    public void PowerTrackOn(char track)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<1 {track}>";
            _SendCommand();
        }
    }

    public void PowerTrackOff(char track)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<0 {track}>";
            _SendCommand();
        }
    }

    public void SetTrackType(char track, TrackManagerMode type, int address)
    {
        if (_delegate != null)
        {
            switch (type)
            {
                case TrackManagerMode.MAIN:
                    _outboundCommand = $"<= {track} MAIN>";
                    break;
                case TrackManagerMode.PROG:
                    _outboundCommand = $"<= {track} PROG>";
                    break;
                case TrackManagerMode.DC:
                    _outboundCommand = $"<= {track} DC {address}>";
                    break;
                case TrackManagerMode.DCX:
                    _outboundCommand = $"<= {track} DCX {address}>";
                    break;
                case TrackManagerMode.NONE:
                    _outboundCommand = $"<= {track} NONE>";
                    break;
                default:
                    return;
            }
            _SendCommand();
        }
    }

    // DCC accessory methods
    public void ActivateAccessory(int accessoryAddress, int accessorySubAddr)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<a {accessoryAddress} {accessorySubAddr} 1>";
            _SendCommand();
        }
    }

    public void DeactivateAccessory(int accessoryAddress, int accessorySubAddr)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<a {accessoryAddress} {accessorySubAddr} 0>";
            _SendCommand();
        }
    }

    public void ActivateLinearAccessory(int linearAddress)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<a {linearAddress} 1>";
            _SendCommand();
        }
    }

    public void DeactivateLinearAccessory(int linearAddress)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<a {linearAddress} 0>";
            _SendCommand();
        }
    }

    public void GetNumberSupportedLocos()
    {
        if (_delegate != null)
        {
            _outboundCommand = "<#>";
            _SendCommand();
        }
    }

    // CV programming methods
    public void ReadCV(int cv)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<R {cv}>";
            _SendCommand();
        }
    }

    public void ValidateCV(int cv, int value)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<V {cv} {value}>";
            _SendCommand();
        }
    }

    public void ValidateCVBit(int cv, int bit, int value)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<V {cv} {bit} {value}>";
            _SendCommand();
        }
    }

    public void WriteLocoAddress(int address)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<W {address}>";
            _SendCommand();
        }
    }

    public void WriteCV(int cv, int value)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<W {cv} {value}>";
            _SendCommand();
        }
    }

    public void WriteCVBit(int cv, int bit, int value)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<B {cv} {bit} {value}>";
            _SendCommand();
        }
    }

    public void WriteCVOnMain(int address, int cv, int value)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<w {address} {cv} {value}>";
            _SendCommand();
        }
    }

    public void WriteCVBitOnMain(int address, int cv, int bit, int value)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<b {address} {cv} {bit} {value}>";
            _SendCommand();
        }
    }

    // Private methods
    // Protocol and server methods

    // Init the DCCEXProtocol instance after connection to the server
    private void _Init()
    {
        // Initialize input buffer and position variable
        _inputBuffer = new char[1024];
        _nextChar = 0;
        // Set last response time
        _lastServerResponseTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }

    private void _SendCommand()
    {
        if (_stream != null)
        {
            _stream.WriteLine(_outboundCommand);
            _console.Print("==> ");
            _console.Println(_outboundCommand);
            _outboundCommand = "";  // Clear it once it has been sent
            _lastHeartbeat = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond; // If we sent a command, a heartbeat isn't necessary
        }
    }

    private void _ProcessCommand()
    {
        if (_delegate != null)
        {
            // Update last response time
            _lastServerResponseTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            switch (DCCEXInbound.GetOpcode())
            {
                case '@': // Screen update
                    if (DCCEXInbound.IsTextParameter(2) && DCCEXInbound.GetParameterCount() == 3)
                    {
                        _ProcessScreenUpdate();
                    }
                    break;

                case 'i': // iDCC-EX server info
                    if (DCCEXInbound.IsTextParameter(0))
                    {
                        _ProcessServerDescription();
                    }
                    break;

                case 'm': // Broadcast message
                    if (DCCEXInbound.IsTextParameter(0))
                    {
                        _ProcessMessage();
                    }
                    break;

                case 'I': // Turntable broadcast
                    if (DCCEXInbound.GetParameterCount() == 3)
                    {
                        _ProcessTurntableBroadcast();
                    }
                    break;

                case 'p': // Power broadcast
                    if (DCCEXInbound.IsTextParameter(0) || DCCEXInbound.GetParameterCount() > 2)
                        break;
                    _ProcessTrackPower();
                    break;

                case '=': // Track type broadcast
                    if (DCCEXInbound.GetParameterCount() < 2)
                        break;
                    _ProcessTrackType();
                    break;

                case 'l': // Loco/cab broadcast
                    if (DCCEXInbound.IsTextParameter(0) || DCCEXInbound.GetParameterCount() != 4)
                        break;
                    _ProcessLocoBroadcast();
                    break;

                case 'j': // Throttle list response jA|O|P|R|T
                    if (DCCEXInbound.IsTextParameter(0))
                        break;
                    if (DCCEXInbound.GetNumber(0) == 'A')
                    {   // Receive route/automation info
                        if (DCCEXInbound.GetParameterCount() == 0)
                        {
                            // Empty list, no routes/automations
                            _receivedRouteList = true;
                        }
                        else if (DCCEXInbound.GetParameterCount() == 4 && DCCEXInbound.IsTextParameter(3))
                        {
                            // Receive route entry
                            _ProcessRouteEntry();
                        }
                        else
                        {
                            // Receive route/automation list
                            _ProcessRouteList();
                        }
                    }
                    else if (DCCEXInbound.GetNumber(0) == 'O')
                    {   // Receive turntable info
                        if (DCCEXInbound.GetParameterCount() == 0)
                        {
                            // Empty turntable list
                            _receivedTurntableList = true;
                        }
                        else if (DCCEXInbound.GetParameterCount() == 6 && DCCEXInbound.IsTextParameter(5))
                        {
                            // Turntable entry
                            _ProcessTurntableEntry();
                        }
                        else
                        {
                            // Turntable list
                            _ProcessTurntableList();
                        }
                    }
                    else if (DCCEXInbound.GetNumber(0) == 'P')
                    {   // Receive turntable position info
                        if (DCCEXInbound.GetParameterCount() == 5 && DCCEXInbound.IsTextParameter(4))
                        {
                            // Turntable position index entry
                            _ProcessTurntableIndexEntry();
                        }
                    }
                    else if (DCCEXInbound.GetNumber(0) == 'R')
                    {   // Receive roster info
                        if (DCCEXInbound.GetParameterCount() == 1)
                        {
                            // Empty list, no roster
                            _receivedRoster = true;
                        }
                        else if (DCCEXInbound.GetParameterCount() == 4 && DCCEXInbound.IsTextParameter(2) &&
                                 DCCEXInbound.IsTextParameter(3))
                        {
                            // Roster entry <jR id "desc" "func1/func2/func3/...">
                            _ProcessRosterEntry();
                        }
                        else
                        {
                            // Roster list <jR id1 id2 id3 ...>
                            _ProcessRosterList();
                        }
                    }
                    else if (DCCEXInbound.GetNumber(0) == 'T')
                    {   // Receive turnout info
                        if (DCCEXInbound.GetParameterCount() == 1)
                        {
                            // Empty list, no turnouts defined
                            _receivedTurnoutList = true;
                        }
                        else if (DCCEXInbound.GetParameterCount() == 4 && DCCEXInbound.IsTextParameter(3))
                        {
                            // Turnout entry <jT id state "desc">
                            _ProcessTurnoutEntry();
                        }
                        else
                        {
                            // Turnout list <jT id1 id2 id3 ...>
                            _ProcessTurnoutList();
                        }
                    }
                    break;

                case 'H': // Turnout broadcast
                    if (DCCEXInbound.IsTextParameter(0))
                        break;
                    _ProcessTurnoutBroadcast();
                    break;

                case 'r': // Read loco response
                    if (DCCEXInbound.IsTextParameter(0))
                        break;
                    if (DCCEXInbound.GetParameterCount() == 1)
                    {
                        _ProcessReadResponse();
                    }
                    else if (DCCEXInbound.GetParameterCount() == 2)
                    {
                        _ProcessWriteCVResponse();
                    }
                    break;

                case 'w': // Write loco response
                    if (DCCEXInbound.IsTextParameter(0))
                        break;
                    _ProcessWriteLocoResponse();
                    break;

                case 'v': // Validate CV response
                    if (DCCEXInbound.IsTextParameter(0))
                        break;
                    if (DCCEXInbound.GetParameterCount() == 2)
                    {
                        _ProcessValidateCVResponse();
                    }
                    else if (DCCEXInbound.GetParameterCount() == 3)
                    {
                        _ProcessValidateCVBitResponse();
                    }
                    break;

                default:
                    break;
            }
        }
    }

    private void _ProcessServerDescription()
    {
        if (_delegate == null) return;

        // Assume the first parameter contains the full description string like: "DCCEX V-1.2.3 / ..."
        string description = DCCEXInbound.GetTextParameter(0);

        if (string.IsNullOrEmpty(description) || description.Length < 7)
            return;

        string versionSection = description.Substring(7); // Skip "DCCEX "

        int[] parsedVersion = new int[3];
        int parsedCount = 0;

        for (int i = 0; i < versionSection.Length && parsedCount < 3; i++)
        {
            char ch = versionSection[i];

            if (ch != '-' && ch != '.')
                continue;

            int startIndex = i + 1;
            if (startIndex >= versionSection.Length || !char.IsDigit(versionSection[startIndex]))
                continue;

            // Read integer value from that point
            int endIndex = startIndex;
            while (endIndex < versionSection.Length && char.IsDigit(versionSection[endIndex]))
                endIndex++;

            string numberText = versionSection.Substring(startIndex, endIndex - startIndex);
            if (int.TryParse(numberText, out int value) && value >= 0 && value < 1000)
            {
                parsedVersion[parsedCount++] = value;
                i = endIndex - 1; // move to the last digit parsed
            }
            else
            {
                return; // Invalid version format
            }
        }

        if (parsedCount == 3)
        {
            _version = parsedVersion;
            _receivedVersion = true;
            _delegate.ReceivedServerVersion(_version[0], _version[1], _version[2]);
        }
    }

    private void _ProcessMessage()
    {
        _delegate.ReceivedMessage(DCCEXInbound.GetTextParameter(0));
    }

    private void _ProcessScreenUpdate()
    {
        _delegate.ReceivedScreenUpdate(DCCEXInbound.GetNumber(0), DCCEXInbound.GetNumber(1),
                                        DCCEXInbound.GetTextParameter(2));
    }

    private void _SendHeartbeat()
    {
        long currentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        if (currentTime - _lastHeartbeat > _heartbeatDelay)
        {
            _lastHeartbeat = currentTime;
            _outboundCommand = "<#>";
            _SendCommand();
        }
    }

    // Consist/loco methods
    private void _ProcessLocoBroadcast()
    {
        int address = DCCEXInbound.GetNumber(0);
        int speedByte = DCCEXInbound.GetNumber(2);
        int functMap = _GetValidFunctionMap(DCCEXInbound.GetNumber(3));
        int speed = _GetSpeedFromSpeedByte(speedByte);
        Direction dir = _GetDirectionFromSpeedByte(speedByte);

        // Set a known Loco with the received info and call the delegate
        for (Loco l = Loco.GetFirst(); l != null; l = l.GetNext())
        {
            if (l.GetAddress() == address)
            {
                l.SetSpeed(speed);
                l.SetDirection(dir);
                l.SetFunctionStates(functMap);
                _delegate.ReceivedLocoUpdate(l);
            }
        }

        // Send a broadcast for unknown as well in case it's a local Loco not in the roster
        _delegate.ReceivedLocoBroadcast(address, speed, dir, functMap);
    }

    private int _GetValidFunctionMap(int functionMap)
    {
        // Mask off anything above 28 bits/28 functions
        if (functionMap > 0xFFFFFFF)
        {
            functionMap &= 0xFFFFFFF;
        }
        return functionMap;
    }

    private int _GetSpeedFromSpeedByte(int speedByte)
    {
        int speed = speedByte;
        if (speed >= 128)
        {
            speed = speed - 128;
        }
        if (speed > 1)
        {
            speed = speed - 1; // get around the idiotic design of the speed command
        }
        else
        {
            speed = 0;
        }
        return speed;
    }

    private Direction _GetDirectionFromSpeedByte(int speedByte)
    {
        return speedByte >= 128 ? Direction.Forward : Direction.Reverse;
    }

    private void _SetLoco(int address, int speed, Direction direction)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<t {address} {speed} {(int)direction}>";
            _SendCommand();
        }
    }

    private void _ProcessReadResponse()
    {
        int address = DCCEXInbound.GetNumber(0);
        _delegate.ReceivedReadLoco(address);
    }

    // Roster methods
    private void _GetRoster()
    {
        if (_delegate != null)
        {
            _outboundCommand = "<JR>";
            _SendCommand();
            _rosterRequested = true;
        }
    }

    private bool _RequestedRoster()
    {
        return _rosterRequested;
    }

    private void _ProcessRosterList()
    {
        if (Roster != null)
        {
            // Already have a roster so this is an update
            return;
        }
        if (DCCEXInbound.GetParameterCount() == 1)
        {
            // Roster empty
            _receivedRoster = true;
            return;
        }
        for (int i = 1; i < DCCEXInbound.GetParameterCount(); i++)
        {
            int address = DCCEXInbound.GetNumber(i);
            new Loco(address, LocoSource.LocoSourceRoster);
        }
        _RequestRosterEntry(Loco.GetFirst().GetAddress());
        _rosterCount = DCCEXInbound.GetParameterCount() - 1;
    }

    private void _RequestRosterEntry(int address)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<JR {address}>";
            _SendCommand();
        }
    }

    private void _ProcessRosterEntry()
    {
        int address = DCCEXInbound.GetNumber(1);
        string name = DCCEXInbound.CopyTextParameter(2);
        string funcs = DCCEXInbound.CopyTextParameter(3);
        bool missingRosters = false;

        Loco loco = Loco.GetByAddress(address);
        if (loco != null)
        {
            loco.SetName(name);
            loco.SetupFunctions(funcs);
            if (loco.GetNext() != null && loco.GetNext().GetName() == null)
            {
                missingRosters = true;
                _RequestRosterEntry(loco.GetNext().GetAddress());
            }
        }

        if (!missingRosters)
        {
            _receivedRoster = true;
            _delegate.ReceivedRosterList();
        }
    }

    // Turnout methods
    private void _GetTurnouts()
    {
        if (_delegate != null)
        {
            _outboundCommand = "<JT>";
            _SendCommand();
            _turnoutListRequested = true;
        }
    }

    private bool _RequestedTurnouts()
    {
        return _turnoutListRequested;
    }

    private void _ProcessTurnoutList()
    {
        if (Turnouts != null)
        {
            return;
        }
        if (DCCEXInbound.GetParameterCount() == 1)
        {
            // Turnout list is empty
            _receivedTurnoutList = true;
            return;
        }
        for (int i = 1; i < DCCEXInbound.GetParameterCount(); i++)
        {
            int id = DCCEXInbound.GetNumber(i);
            new Turnout(id, false);
        }
        _RequestTurnoutEntry(Turnout.GetFirst().GetId());
        _turnoutCount = DCCEXInbound.GetParameterCount() - 1;
    }

    private void _RequestTurnoutEntry(int id)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<JT {id}>";
            _SendCommand();
        }
    }

    private void _ProcessTurnoutEntry()
    {
        if (DCCEXInbound.GetParameterCount() != 4)
            return;

        int id = DCCEXInbound.GetNumber(1);
        bool thrown = DCCEXInbound.GetNumber(2) == 'T';
        string name = DCCEXInbound.CopyTextParameter(3);
        bool missingTurnouts = false;

        Turnout t = Turnout.GetById(id);
        if (t != null)
        {
            t.SetName(name);
            t.SetThrown(thrown);
            if (t.GetNext() != null && t.GetNext().GetName() == null)
            {
                missingTurnouts = true;
                _RequestTurnoutEntry(t.GetNext().GetId());
            }
        }

        if (!missingTurnouts)
        {
            _receivedTurnoutList = true;
            _delegate.ReceivedTurnoutList();
        }
    }

    private void _ProcessTurnoutBroadcast()
    {
        if (DCCEXInbound.GetParameterCount() != 2)
            return;
            
        int id = DCCEXInbound.GetNumber(0);
        bool thrown = Convert.ToBoolean(DCCEXInbound.GetNumber(1));
        
        for (Turnout t = Turnout.GetFirst(); t != null; t = t.GetNext())
        {
            if (t.GetId() == id)
            {
                t.SetThrown(thrown);
                _delegate.ReceivedTurnoutAction(id, thrown);
            }
        }
    }

    // Route methods
    private void _GetRoutes()
    {
        if (_delegate != null)
        {
            _outboundCommand = "<JA>";
            _SendCommand();
            _routeListRequested = true;
        }
    }

    private bool _RequestedRoutes()
    {
        return _routeListRequested;
    }

    private void _ProcessRouteList()
    {
        if (Routes != null)
        {
            return;
        }
        if (DCCEXInbound.GetParameterCount() == 1)
        {
            // Route list is empty
            _receivedRouteList = true;
            return;
        }
        for (int i = 1; i < DCCEXInbound.GetParameterCount(); i++)
        {
            int id = DCCEXInbound.GetNumber(i);
            new Route(id);
        }
        _RequestRouteEntry(Route.GetFirst().GetId());
        _routeCount = DCCEXInbound.GetParameterCount() - 1;
    }

    private void _RequestRouteEntry(int id)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<JA {id}>";
            _SendCommand();
        }
    }

    private void _ProcessRouteEntry()
    {
        int id = DCCEXInbound.GetNumber(1);
        RouteType type = (RouteType)DCCEXInbound.GetNumber(2);
        string name = DCCEXInbound.CopyTextParameter(3);
        bool missingRoutes = false;

        Route r = Route.GetById(id);
        if (r != null)
        {
            r.SetType(type);
            r.SetName(name);
            if (r.GetNext() != null && r.GetNext().GetName() == null)
            {
                missingRoutes = true;
                _RequestRouteEntry(r.GetNext().GetId());
            }
        }

        if (!missingRoutes)
        {
            _receivedRouteList = true;
            _delegate.ReceivedRouteList();
        }
    }

    // Turntable methods
    private void _GetTurntables()
    {
        if (_delegate != null)
        {
            _outboundCommand = "<JO>";
            _SendCommand();
            _turntableListRequested = true;
        }
    }

    private bool _RequestedTurntables()
    {
        return _turntableListRequested;
    }

    private void _ProcessTurntableList()
    {
        if (turntables != null)
        {
            return;
        }
        if (DCCEXInbound.GetParameterCount() == 1)
        {
            // List is empty so we have received it
            _receivedTurntableList = true;
            return;
        }
        for (int i = 1; i < DCCEXInbound.GetParameterCount(); i++)
        {
            int id = DCCEXInbound.GetNumber(i);
            new Turntable(id);
        }
        _RequestTurntableEntry(Turntable.GetFirst().GetId());
        _turntableCount = DCCEXInbound.GetParameterCount() - 1;
    }

    private void _RequestTurntableEntry(int id)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<JO {id}>";
            _SendCommand();
        }
    }

    private void _ProcessTurntableEntry()
    {
        int id = DCCEXInbound.GetNumber(1);
        TurntableType ttType = (TurntableType)DCCEXInbound.GetNumber(2);
        int index = DCCEXInbound.GetNumber(3);
        int indexCount = DCCEXInbound.GetNumber(4);
        string name = DCCEXInbound.CopyTextParameter(5);

        Turntable tt = Turntable.GetById(id);
        if (tt != null)
        {
            tt.SetType(ttType);
            tt.SetIndex(index);
            tt.SetNumberOfIndexes(indexCount);
            tt.SetName(name);
            _RequestTurntableIndexEntry(id);
            if (tt.GetNext() != null && tt.GetNext().GetName() == null)
            {
                _RequestTurntableEntry(tt.GetNext().GetId());
            }
        }
    }

    private void _RequestTurntableIndexEntry(int id)
    {
        if (_delegate != null)
        {
            _outboundCommand = $"<JP {id}>";
            _SendCommand();
        }
    }

    private void _ProcessTurntableIndexEntry()
    {
        if (DCCEXInbound.GetParameterCount() == 5)
        {
            int ttId = DCCEXInbound.GetNumber(1);
            int index = DCCEXInbound.GetNumber(2);
            int angle = DCCEXInbound.GetNumber(3);
            string name = DCCEXInbound.CopyTextParameter(4);
            
            if (index == 0)
            {
                // Index 0 is always home and never has a label, so set one
                name = "Home";
            }

            Turntable tt = GetTurntableById(ttId);
            if (tt == null)
                return;

            int numIndexes = tt.GetNumberOfIndexes();
            int idxCount = tt.GetIndexCount();

            if (numIndexes != idxCount)
            {
                TurntableIndex newIndex = new TurntableIndex(ttId, index, angle, name);
                tt.AddIndex(newIndex);
            }

            bool receivedAll = true;

            for (Turntable t = turntables; t != null; t = t.GetNext())
            {
                int turntableNumIndexes = t.GetNumberOfIndexes();
                int indexCount = t.GetIndexCount();
                if (t.GetName() == null || turntableNumIndexes != indexCount)
                    receivedAll = false;
            }

            if (receivedAll)
            {
                _receivedTurntableList = true;
                _delegate.ReceivedTurntableList();
            }
        }
    }

    private void _ProcessTurntableBroadcast()
    {
        int id = DCCEXInbound.GetNumber(0);
        int newIndex = DCCEXInbound.GetNumber(1);
        bool moving = Convert.ToBoolean(DCCEXInbound.GetNumber(2));
        Turntable tt = GetTurntableById(id);
        if (tt != null)
        {
            tt.SetIndex(newIndex);
            tt.SetMoving(moving);
        }
        _delegate.ReceivedTurntableAction(id, newIndex, moving);
    }

    // Track management methods
    private void _ProcessTrackPower()
    {
        if (_delegate != null)
        {
            TrackPower state = TrackPower.PowerUnknown;
            if (DCCEXInbound.GetNumber(0) == (int)TrackPower.PowerOff)
            {
                state = TrackPower.PowerOff;
            }
            else if (DCCEXInbound.GetNumber(0) == (int)TrackPower.PowerOn)
            {
                state = TrackPower.PowerOn;
            }

            if (DCCEXInbound.GetParameterCount() == 2)
            {
                int track = DCCEXInbound.GetNumber(1);
                _delegate.ReceivedIndividualTrackPower(state, track);

                if (DCCEXInbound.GetNumber(1) != 2698315)
                {
                    return;
                } // not equal "MAIN"
            }
            _delegate.ReceivedTrackPower(state);
        }
    }

    private void _ProcessTrackType()
    {
        if (_delegate != null)
        {
            char track = (char)DCCEXInbound.GetNumber(0);
            int type = DCCEXInbound.GetNumber(1);
            TrackManagerMode trackType;
            
            switch (type)
            {
                case 2698315:
                    trackType = TrackManagerMode.MAIN;
                    break;
                case 2788330:
                    trackType = TrackManagerMode.PROG;
                    break;
                case 2183:
                    trackType = TrackManagerMode.DC;
                    break;
                case 71999:
                    trackType = TrackManagerMode.DCX;
                    break;
                case 2857034:
                    trackType = TrackManagerMode.NONE;
                    break;
                default:
                    return;
            }
            
            int address = 0;
            if (DCCEXInbound.GetParameterCount() > 2)
                address = DCCEXInbound.GetNumber(2);

            _delegate.ReceivedTrackType(track, trackType, address);
        }
    }

    private void _ProcessValidateCVResponse()
    {
        int cv = DCCEXInbound.GetNumber(0);
        int value = DCCEXInbound.GetNumber(1);
        _delegate.ReceivedValidateCV(cv, value);
    }

    private void _ProcessValidateCVBitResponse()
    {
        int cv = DCCEXInbound.GetNumber(0);
        int bit = DCCEXInbound.GetNumber(1);
        int value = DCCEXInbound.GetNumber(2);
        _delegate.ReceivedValidateCVBit(cv, bit, value);
    }

    private void _ProcessWriteLocoResponse()
    {
        int value = DCCEXInbound.GetNumber(0);
        _delegate.ReceivedWriteLoco(value);
    }

    private void _ProcessWriteCVResponse()
    {
        int cv = DCCEXInbound.GetNumber(0);
        int value = DCCEXInbound.GetNumber(1);
        _delegate.ReceivedWriteCV(cv, value);
    }
}
