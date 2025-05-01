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

public class Turntable
{
    private static Turntable _first;
    private int _id;
    private TurntableType _type;
    private int _index;
    private int _numberOfIndexes;
    private string _name;
    private bool _isMoving;
    private TurntableIndex _firstIndex;
    private int _indexCount;
    private Turntable _next;

    //TODO - these are not used consistently and duplicate the underlying set methods
    public TurntableType Type { get => _type; set => SetType(value); }
    public int Index { get => _index; set => SetIndex(value); }
    public string Name { get => _name; set => SetName(value); }
    public int NumberOfIndexes { get => _numberOfIndexes; set => SetNumberOfIndexes(value); }
    

    public Turntable(int id)
    {
        _id = id;
        _type = TurntableType.TurntableTypeUnknown;
        _index = 0;
        _numberOfIndexes = 0;
        _name = null;
        _isMoving = false;
        _firstIndex = null;
        _indexCount = 0;
        _next = null;

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

    public int GetId() => _id;
    public void SetType(TurntableType type) => _type = type;
    public TurntableType GetType() => _type;
    public void SetIndex(int index) => _index = index;
    public int GetIndex() => _index;
    public void SetNumberOfIndexes(int numberOfIndexes) => _numberOfIndexes = numberOfIndexes;
    public int GetNumberOfIndexes() => _numberOfIndexes;
    public void SetName(string name) => _name = name;
    public string GetName() => _name;
    public void SetMoving(bool moving) => _isMoving = moving;
    public bool IsMoving() => _isMoving;
    public int GetIndexCount() => _indexCount;
    public Turntable GetFirstInstance() { return _first; }
    public static Turntable GetFirst() => _first;
    public void SetNext(Turntable turntable) => _next = turntable;
    public Turntable GetNext() => _next;


    //public void AddIndex(TurntableIndex index)
    //{
    //    if (_firstIndex == null)
    //    {
    //        _firstIndex = index;
    //    }
    //    else
    //    {
    //        var current = _firstIndex;
    //        while (current.GetNextIndex() != null)
    //        {
    //            current = current.NextIndex;
    //        }
    //        current.NextIndex = index;
    //    }
    //    _indexCount++;
    //}
    public void AddIndex(TurntableIndex index)
    {
        if (_firstIndex == null)
        {
            _firstIndex = index;
        }
        else
        {
            var current = _firstIndex;
            while (current.GetNextIndex() != null)
            {
                current = current.GetNextIndex();
            }
            current.SetNextIndex(index);
        }
        _indexCount++;
    }

    public TurntableIndex GetFirstIndex() => _firstIndex;

    public static Turntable GetById(int id)
    {
        for (var tt = GetFirst(); tt != null; tt = tt.GetNext())
        {
            if (tt.GetId() == id) return tt;
        }
        return null;
    }

    public TurntableIndex GetIndexById(int id)
    {
        for (var index = GetFirstIndex(); index != null; index = index.GetNextIndex())
        {
            if (index.GetId() == id) return index;
        }
        return null;
    }

    public static void ClearTurntableList()
    {
        var list = new List<Turntable>();
        var current = GetFirst();
        while (current != null)
        {
            list.Add(current);
            current = current.GetNext();
        }

        foreach (var tt in list)
        {
            tt.Dispose();
        }

        _first = null;
    }

    public void Dispose()
    {
        RemoveFromList(this);
        _name = null;

        var currentIndex = _firstIndex;
        while (currentIndex != null)
        {
            var nextIndex = currentIndex.GetNextIndex();
            //currentIndex.Dispose(); //TODO - not needed in C#?
            currentIndex = nextIndex;
        }

        _firstIndex = null;
        _next = null;
    }

    private static void RemoveFromList(Turntable turntable)
    {
        if (_first == turntable)
        {
            _first = turntable.GetNext();
        }
        else
        {
            var current = _first;
            while (current != null && current.GetNext() != turntable)
            {
                current = current.GetNext();
            }
            if (current != null)
            {
                current.SetNext(turntable.GetNext());
            }
        }
    }
}
