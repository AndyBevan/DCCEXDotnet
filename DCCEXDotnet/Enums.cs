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

public enum Direction
{
    Reverse = 0,
    Forward = 1
}

public enum TrackPower
{
    PowerOff = 0,
    PowerOn = 1,
    PowerUnknown = 2,
}

public enum TrackManagerMode
{
    MAIN, // Normal DCC track mode
    PROG, // Programming DCC track mode
    DC,   // DC mode
    DCX,  // Reverse polarity DC mode
    NONE, // Track is unused
}

public enum RouteType
{
    //RouteTypeRoute = 0,
    //RouteTypeAutomation = 1
    RouteTypeRoute = 'R',
    RouteTypeAutomation = 'A',
}

public enum TurntableType
{
    TurntableTypeDCC = 0,
    TurntableTypeEXTT = 1,
    TurntableTypeUnknown = 9,
}

public enum LocoSource
{
    LocoSourceRoster = 0,
    LocoSourceEntry = 1,
}

public enum Facing
{
    FacingForward,
    FacingReversed
}
