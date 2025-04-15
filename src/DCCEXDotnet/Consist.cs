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

public class Consist : IConsist
{
    private string _name;
    private int _locoCount;
    private ConsistLoco _first;

    public Consist()
    {
        _name = null;
        _locoCount = 0;
        _first = null;
    }

    public void SetName(string name) => _name = name;
    public string GetName() => _name;

    public void AddLoco(Loco loco, Facing facing)
    {
        if (InConsist(loco)) return;
        if (_locoCount == 0)
        {
            facing = Facing.FacingForward;
            if (_name == null) SetName(loco.GetName());
        }
        var conLoco = new ConsistLoco(loco, facing);
        _addLocoToConsist(conLoco);
    }

    public void AddLoco(int address, Facing facing)
    {
        if (InConsist(address)) return;
        if (_locoCount == 0)
        {
            facing = Facing.FacingForward;
            if (_name == null) SetName(address.ToString());
        }
        var loco = new Loco(address, LocoSource.LocoSourceEntry);
        var conLoco = new ConsistLoco(loco, facing);
        _addLocoToConsist(conLoco);
    }

    public void RemoveLoco(Loco loco)
    {
        ConsistLoco previous = null;
        ConsistLoco current = _first;

        while (current != null)
        {
            if (current.GetLoco() == loco)
            {
                var next = current.GetNext();
                if (previous != null)
                    previous.SetNext(next);
                else
                    _first = next;

                _locoCount--;
                current = next;
            }
            else
            {
                previous = current;
                current = current.GetNext();
            }
        }

        if (_first == null)
            _locoCount = 0;
    }

    public void RemoveAllLocos()
    {
        _first = null;
        _locoCount = 0;
    }

    public void SetLocoFacing(Loco loco, Facing facing)
    {
        for (var cl = _first; cl != null; cl = cl.GetNext())
            if (cl.GetLoco() == loco) cl.SetFacing(facing);
    }

    public int GetLocoCount() => _locoCount;

    public bool InConsist(Loco loco)
    {
        for (var cl = _first; cl != null; cl = cl.GetNext())
            if (cl.GetLoco() == loco) return true;
        return false;
    }

    public bool InConsist(int address)
    {
        for (var cl = _first; cl != null; cl = cl.GetNext())
            if (cl.GetLoco().GetAddress() == address) return true;
        return false;
    }

    public int GetSpeed()
    {
        if (_first == null) return 0;
        return _first.GetLoco().GetSpeed();
    }

    public Direction GetDirection()
    {
        if (_first == null) return Direction.Forward;
        return _first.GetLoco().GetDirection();
    }

    public ConsistLoco GetFirst() => _first;

    public ConsistLoco GetByAddress(int address)
    {
        for (var cl = _first; cl != null; cl = cl.GetNext())
            if (cl.GetLoco().GetAddress() == address) return cl;
        return null;
    }

    private void _addLocoToConsist(ConsistLoco conLoco)
    {
        if (_first == null)
        {
            _first = conLoco;
        }
        else
        {
            var current = _first;
            while (current.GetNext() != null)
                current = current.GetNext();
            current.SetNext(conLoco);
        }
        _locoCount++;
    }
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
