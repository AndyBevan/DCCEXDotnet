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

public interface IConsist
{
    /// <summary>Set consist name</summary>
    /// <param name="name">Name to set for the consist</param>
    void SetName(string name);

    /// <summary>Get consist name</summary>
    /// <returns>Current name of the consist</returns>
    string GetName();

    /// <summary>Add a loco to the consist using a Loco object</summary>
    /// <param name="loco">Loco object</param>
    /// <param name="facing">Direction the loco is facing</param>
    void AddLoco(Loco loco, Facing facing);

    /// <summary>Add a loco to the consist using a DCC address</summary>
    /// <param name="address">DCC address</param>
    /// <param name="facing">Direction the loco is facing</param>
    void AddLoco(int address, Facing facing);

    /// <summary>Remove a loco from the consist</summary>
    /// <param name="loco">Loco object to remove</param>
    void RemoveLoco(Loco loco);

    /// <summary>Remove all locos from the consist</summary>
    void RemoveAllLocos();

    /// <summary>Set the direction of a loco in the consist</summary>
    /// <param name="loco">Loco to update</param>
    /// <param name="facing">New facing direction</param>
    void SetLocoFacing(Loco loco, Facing facing);

    /// <summary>Get number of locos in the consist</summary>
    /// <returns>Number of locos</returns>
    int GetLocoCount();

    /// <summary>Check if the provided loco is in the consist</summary>
    /// <param name="loco">Loco to check</param>
    /// <returns>true if present</returns>
    bool InConsist(Loco loco);

    /// <summary>Check if a loco with the given address is in the consist</summary>
    /// <param name="address">DCC address to check</param>
    /// <returns>true if present</returns>
    bool InConsist(int address);

    /// <summary>Get speed of the consist, based on the first loco</summary>
    /// <returns>Speed</returns>
    int GetSpeed();

    /// <summary>Get direction of the consist, based on the first loco</summary>
    /// <returns>Direction</returns>
    Direction GetDirection();

    /// <summary>Get the first loco in the consist</summary>
    /// <returns>First ConsistLoco</returns>
    ConsistLoco GetFirst();

    /// <summary>Get a ConsistLoco by its address</summary>
    /// <param name="address">DCC address</param>
    /// <returns>Matching ConsistLoco or null</returns>
    ConsistLoco GetByAddress(int address);
}


//public class Consist
//{
//    private ConsistLoco _first;

//    public void AddLoco(Loco loco, Facing facing)
//    {
//        ConsistLoco cl = new ConsistLoco(loco, facing);
//        if (_first == null)
//        {
//            _first = cl;
//        }
//        else
//        {
//            ConsistLoco current = _first;
//            while (current.GetNext() != null)
//            {
//                current = current.GetNext();
//            }
//            current.SetNext(cl);
//        }
//    }

//    public ConsistLoco GetFirst() { return _first; }
//}
