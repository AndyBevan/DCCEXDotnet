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

public interface IDCCEXProtocolDelegate
{
    void ReceivedServerVersion(int major, int minor, int patch);
    void ReceivedMessage(string message);
    void ReceivedScreenUpdate(int screen, int row, string message);
    void ReceivedLocoUpdate(Loco loco);
    void ReceivedLocoBroadcast(int address, int speed, Direction direction, int functionMap);
    void ReceivedRosterList();
    void ReceivedTurnoutList();
    void ReceivedTurnoutAction(int id, bool thrown);
    void ReceivedRouteList();
    void ReceivedTurntableList();
    void ReceivedTurntableAction(int id, int newIndex, bool moving);
    void ReceivedTrackPower(TrackPower state);
    void ReceivedIndividualTrackPower(TrackPower state, int track);
    void ReceivedTrackType(char track, TrackManagerMode trackType, int address);
    void ReceivedReadLoco(int address);
    void ReceivedWriteLoco(int value);
    void ReceivedWriteCV(int cv, int value);
    void ReceivedValidateCV(int cv, int value);
    void ReceivedValidateCVBit(int cv, int bit, int value);
}
