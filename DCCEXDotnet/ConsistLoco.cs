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

public interface IConsistLoco
{
    /// <summary>Get the associated Loco object for this consist entry</summary>
    /// <returns>The Loco object</returns>
    Loco GetLoco();

    /// <summary>Set which way the loco is facing in the consist</summary>
    /// <param name="facing">Facing direction (Forward or Reversed)</param>
    void SetFacing(Facing facing);

    /// <summary>Get which way the loco is facing in the consist</summary>
    /// <returns>Facing direction</returns>
    Facing GetFacing();

    /// <summary>Get the next ConsistLoco in the consist chain</summary>
    /// <returns>The next ConsistLoco</returns>
    IConsistLoco GetNext();

    /// <summary>Set the next ConsistLoco in the consist chain</summary>
    /// <param name="consistLoco">The next ConsistLoco</param>
    void SetNext(IConsistLoco consistLoco);
}

public class ConsistLoco
{
    private Loco _loco;
    private Facing _facing;
    private ConsistLoco _next;

    public ConsistLoco(Loco loco, Facing facing)
    {
        _loco = loco;
        _facing = facing;
        _next = null;
    }

    public Loco GetLoco() => _loco;

    public void SetFacing(Facing facing) => _facing = facing;

    public Facing GetFacing() => _facing;

    public ConsistLoco GetNext() => _next;

    public void SetNext(ConsistLoco consistLoco) => _next = consistLoco;

    // Optional cleanup method if you want to mimic the destructor logic
    public void Cleanup()
    {
        if (_loco != null && _loco.GetSource() == LocoSource.LocoSourceEntry)
        {
            _loco = null;
        }

        _next = null;
    }
}


//public class ConsistLoco
//{
//    private Loco _loco;
//    private Facing _facing;
//    private ConsistLoco _next;

//    public ConsistLoco(Loco loco, Facing facing)
//    {
//        _loco = loco;
//        _facing = facing;
//    }

//    public Loco GetLoco() { return _loco; }
//    public Facing GetFacing() { return _facing; }
//    public ConsistLoco GetNext() { return _next; }
//    public void SetNext(ConsistLoco next) { _next = next; }
//}
