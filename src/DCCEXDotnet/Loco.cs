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

// Forward declarations of classes used in DCCEXProtocol
public class Loco
{

    const int MAX_FUNCTIONS = 32;
    const int MAX_OBJECT_NAME_LENGTH = 30;
    private string[] _functionNames = new string[MAX_FUNCTIONS];
    private int _momentaryFlags;

    private int _address;
    private string _name;
    private int _speed;
    private Direction _direction;
    private int _functionStates;
    private LocoSource _source;
    private static Loco _first;
    private Loco _next;

    //public Loco(int address, LocoSource source)
    //{
    //    _direction = Direction.Forward;
    //    _address = address;
    //    _source = source;

    //    // Add to linked list
    //    if (_first == null)
    //    {
    //        _first = this;
    //    }
    //    else
    //    {
    //        Loco loco = _first;
    //        while (loco.GetNext() != null)
    //        {
    //            loco = loco.GetNext();
    //        }
    //        loco._next = this;
    //    }
    //}

    public Loco(int address, LocoSource source)
    {
        _direction = Direction.Forward;
        _address = address;
        _source = source;

        for (int i = 0; i < MAX_FUNCTIONS; i++)
        {
            _functionNames[i] = null;
        }

        _direction = Direction.Forward;
        _speed = 0;
        _name = null;
        _functionStates = 0;
        _momentaryFlags = 0;
        _next = null;

        if (_source == LocoSource.LocoSourceRoster)
        {
            if (_first == null)
            {
                _first = this;
            }
            else
            {
                var current = _first;
                while (current._next != null)
                {
                    current = current._next;
                }
                current._next = this;
            }
        }
    }

    public static Loco GetFirst() { return _first; }
    public Loco GetNext() { return _next; }
    public int GetAddress() { return _address; }
    public string GetName() { return _name; }
    public int GetSpeed() { return _speed; }
    public Direction GetDirection() { return _direction; }
    public LocoSource GetSource() { return _source; }
    public bool IsFunctionOn(int function) { return (_functionStates & 1 << function) != 0; }

    public void SetName(string name) { _name = name; }
    public void SetSpeed(int speed) { _speed = speed; }
    public void SetDirection(Direction dir) { _direction = dir; }
    public void SetFunctionStates(int states) { _functionStates = states; }
    public void SetupFunctions(string functionNames)
    {
        if (string.IsNullOrEmpty(functionNames)) return;

        // Remove existing names
        for (int i = 0; i < MAX_FUNCTIONS; i++)
        {
            _functionNames[i] = null;
        }

        int fNameIndex = 0;
        string[] tokens = functionNames.Split('/');

        foreach (var token in tokens)
        {
            if (fNameIndex >= MAX_FUNCTIONS) break;

            string name = token;
            bool momentary = false;

            if (!string.IsNullOrEmpty(name) && name.StartsWith("*"))
            {
                momentary = true;
                name = name.Substring(1);
            }

            _functionNames[fNameIndex] = name;

            if (momentary)
                _momentaryFlags |= 1 << fNameIndex;
            else
                _momentaryFlags &= ~(1 << fNameIndex);

            fNameIndex++;
        }
    }



    public int GetFunctionStates() => _functionStates;

    public string GetFunctionName(int function) => _functionNames[function];

    public bool IsFunctionMomentary(int function) => (_momentaryFlags & 1 << function) != 0;

    public static Loco First => _first;

    public void SetNext(Loco loco) => _next = loco;

    public Loco Next => _next;


    public static void ClearRoster() { _first = null; }
    public static Loco GetByAddress(int address)
    {
        for (Loco loco = _first; loco != null; loco = loco.GetNext())
        {
            if (loco.GetAddress() == address)
            {
                return loco;
            }
        }
        return null;
    }
}
