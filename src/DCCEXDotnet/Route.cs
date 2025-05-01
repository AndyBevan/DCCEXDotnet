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

public class Route
{
    private int _id;
    private string _name;
    private RouteType _type;
    private static Route _first;
    private Route _next;

    public Route(int id)
    {
        _id = id;

        // Add to linked list
        if (_first == null)
        {
            _first = this;
        }
        else
        {
            Route route = _first;
            while (route.GetNext() != null)
            {
                route = route.GetNext();
            }
            route._next = this;
        }
    }

    public Route GetFirstInstance() { return _first; }
    public static Route GetFirst() { return _first; }
    public Route GetNext() { return _next; }
    public int GetId() { return _id; }
    public string GetName() { return _name; }
    public RouteType GetType() { return _type; }

    public void SetName(string name) { _name = name; }
    public void SetType(RouteType type) { _type = type; }

    public static void ClearRouteList() { _first = null; }
    public static Route GetById(int id)
    {
        for (Route route = _first; route != null; route = route.GetNext())
        {
            if (route.GetId() == id)
            {
                return route;
            }
        }
        return null;
    }

    //TODO - this is a duplicate of the static method above - needs to be combined 
    public Route GetByIdNonStatic(int id)
    {
        for (Route route = _first; route != null; route = route.GetNext())
        {
            if (route.GetId() == id)
            {
                return route;
            }
        }
        return null;
    }
}
