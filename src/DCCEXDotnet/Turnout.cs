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

public class Turnout
{
    private int _id;
    private string _name;
    private bool _thrown;
    private static Turnout _first;
    private Turnout _next;

    public Turnout(int id, bool thrown)
    {
        _id = id;
        _thrown = thrown;

        // Add to linked list
        if (_first == null)
        {
            _first = this;
        }
        else
        {
            Turnout turnout = _first;
            while (turnout.GetNext() != null)
            {
                turnout = turnout.GetNext();
            }
            turnout._next = this;
        }
    }

    public Turnout GetFirstInstance() { return _first; }
    public static Turnout GetFirst() { return _first; }
    public Turnout GetNext() { return _next; }
    public int GetId() { return _id; }
    public string GetName() { return _name; }
    public bool GetThrown() { return _thrown; }

    public void SetName(string name) { _name = name; }
    public void SetThrown(bool thrown) { _thrown = thrown; }

    public static void ClearTurnoutList() { _first = null; }
    public static Turnout GetById(int id)
    {
        for (Turnout turnout = _first; turnout != null; turnout = turnout.GetNext())
        {
            if (turnout.GetId() == id)
            {
                return turnout;
            }
        }
        return null;
    }

    public Turnout GetByIdNonStatic(int id)
    {
        for (Turnout turnout = _first; turnout != null; turnout = turnout.GetNext())
        {
            if (turnout.GetId() == id)
            {
                return turnout;
            }
        }
        return null;
    }
}
