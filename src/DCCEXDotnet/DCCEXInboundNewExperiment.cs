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

//This was the version that Claude.AI generated for me.  Left here for experimentation
namespace DCCEXDotnet;

// DCCEXInbound class for parsing
public static class DCCEXInboundNewExperiment
{
    private static int _maxParams;
    private static int[] _params;
    private static string[] _textParams;
    private static int _paramCount;
    private static int _opcode;

    public static void Setup(int maxParams)
    {
        _maxParams = maxParams;
        _params = new int[maxParams];
        _textParams = new string[maxParams];
    }

    public static void Cleanup()
    {
        _params = null;
        _textParams = null;
    }

    public static bool Parse(string buffer)
    {
        if (string.IsNullOrEmpty(buffer)) return false;
        if (buffer[0] != '<' || buffer[buffer.Length - 1] != '>') return false;

        // Reset parameters
        _paramCount = 0;
        for (int i = 0; i < _maxParams; i++)
        {
            _params[i] = 0;
            _textParams[i] = null;
        }

        // Extract opcode
        if (buffer.Length < 2) return false;
        _opcode = buffer[1];

        // Parse parameters
        int startPos = 2;
        while (startPos < buffer.Length - 1 && _paramCount < _maxParams)
        {
            // Skip whitespace
            while (startPos < buffer.Length - 1 && char.IsWhiteSpace(buffer[startPos]))
                startPos++;

            if (startPos >= buffer.Length - 1) break;

            if (buffer[startPos] == '"')
            {
                // Text parameter
                int endPos = buffer.IndexOf('"', startPos + 1);
                if (endPos == -1) return false;
                _textParams[_paramCount] = buffer.Substring(startPos + 1, endPos - startPos - 1);
                _paramCount++;
                startPos = endPos + 1;
            }
            else
            {
                // Numeric parameter
                int endPos = startPos;
                while (endPos < buffer.Length - 1 && !char.IsWhiteSpace(buffer[endPos]) && buffer[endPos] != '>')
                    endPos++;

                string numStr = buffer.Substring(startPos, endPos - startPos);
                if (int.TryParse(numStr, out int value))
                {
                    _params[_paramCount] = value;
                }
                else
                {
                    // If not a number, treat as a word token and hash it
                    _params[_paramCount] = HashWord(numStr);
                }
                _paramCount++;
                startPos = endPos;
            }
        }

        return true;
    }

    private static int HashWord(string word)
    {
        int hash = 0;
        foreach (char c in word)
        {
            hash = hash * 31 + c;
        }
        return hash;
    }

    public static int GetOpcode() { return _opcode; }
    public static int GetParameterCount() { return _paramCount; }
    public static int GetNumber(int index)
    {
        if (index < 0 || index >= _paramCount) return 0;
        return _params[index];
    }
    public static string GetTextParameter(int index)
    {
        if (index < 0 || index >= _paramCount) return null;
        return _textParams[index];
    }
    public static bool IsTextParameter(int index)
    {
        if (index < 0 || index >= _paramCount) return false;
        return _textParams[index] != null;
    }
    public static string CopyTextParameter(int index)
    {
        if (index < 0 || index >= _paramCount) return null;
        return _textParams[index];
    }
}
