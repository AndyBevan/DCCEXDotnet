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

public class TurntableIndex
{
    private int _ttId;
    private int _id;
    private int _angle;
    private string _name;
    private TurntableIndex _nextIndex;

    public TurntableIndex(int ttId, int id, int angle, string name)
    {
        _ttId = ttId;
        _id = id;
        _angle = angle;
        _name = name;
        _nextIndex = null;
    }

    public int GetTtId() { return _ttId; }
    public int GetId() { return _id; }
    public int GetAngle() { return _angle; }
    public string GetName() { return _name; }
    public TurntableIndex GetNextIndex() => _nextIndex;
    public void SetNextIndex(TurntableIndex index) => _nextIndex = index;
}
