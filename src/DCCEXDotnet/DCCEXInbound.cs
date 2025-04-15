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

using System.Text;

namespace DCCEXDotnet;

// DCCEXInbound class for parsing

public static class DCCEXInbound
{
    private const int QUOTE_FLAG = 0x77777000;
    private const int QUOTE_FLAG_AREA = unchecked((int)0xFFFFF000);
    //private const int QUOTE_FLAG_AREA = 0xFFFFF000;

    private enum SplitState
    {
        FindStart,
        SetOpcode,
        SkipSpaces,
        CheckSign,
        BuildParam,
        SkipOverText,
        CompleteICommand
    }

    private static int _maxParams = 0;
    private static int _parameterCount = 0;
    private static char _opcode = '\0';
    private static int[] _parameterValues = null;
    private static string _cmdBuffer = null;
    private static StringBuilder _mutableCmdBuffer;
    public static void Setup(int maxParameterValues)
    {
        _parameterValues = new int[maxParameterValues];
        _maxParams = maxParameterValues;
        _mutableCmdBuffer = new StringBuilder();
        _parameterCount = 0;
        _opcode = '\0';
    }

    public static void Cleanup()
    {
        _parameterValues = null;
    }

    public static char GetOpcode() => _opcode;

    public static int GetParameterCount() => _parameterCount;

    public static int GetNumber(int parameterNumber)
    {
        if (parameterNumber < 0 || parameterNumber >= _parameterCount)
            return 0;
        if (IsTextInternal(parameterNumber))
            return 0;
        return _parameterValues[parameterNumber];
    }

    public static bool IsTextParameter(int parameterNumber)
    {
        if (parameterNumber < 0 || parameterNumber >= _parameterCount)
            return false;
        return IsTextInternal(parameterNumber);
    }

    public static string GetTextParameterOld(int parameterNumber)
    {
        if (parameterNumber < 0 || parameterNumber >= _parameterCount)
            return null;
        if (!IsTextInternal(parameterNumber))
            return null;
        int offset = _parameterValues[parameterNumber] & ~QUOTE_FLAG_AREA;
        return _cmdBuffer.Substring(offset);
    }

    public static string GetTextParameter(int parameterNumber)
    {
        if (parameterNumber < 0 || parameterNumber >= _parameterCount)
            return null;
        if (!IsTextParameter(parameterNumber))
            return null;

        int offset = _parameterValues[parameterNumber] & ~QUOTE_FLAG_AREA;

        int end = offset;
        while (end < _mutableCmdBuffer.Length && _mutableCmdBuffer[end] != '\0')
            end++;

        return _mutableCmdBuffer.ToString(offset, end - offset);
    }

    public static string CopyTextParameter(int parameterNumber)
    {
        var text = GetTextParameter(parameterNumber);
        return text == null ? null : string.Copy(text);
    }

    public static bool Parse(string command)
    {
        _parameterCount = 0;
        _opcode = '\0';
        _mutableCmdBuffer = new StringBuilder(command);

        int runningValue = 0;
        int pos = 0;
        bool signNegative = false;
        var state = SplitState.FindStart;

        while (_parameterCount < _maxParams && pos < _mutableCmdBuffer.Length)
        {
            char hot = _mutableCmdBuffer[pos];

            switch (state)
            {
                case SplitState.FindStart:
                    if (hot == '<') state = SplitState.SetOpcode;
                    break;

                case SplitState.SetOpcode:
                    _opcode = hot;
                    if (_opcode == 'i')
                    {
                        _parameterValues[_parameterCount++] = QUOTE_FLAG | pos + 1;
                        state = SplitState.CompleteICommand;
                        break;
                    }
                    state = SplitState.SkipSpaces;
                    break;

                case SplitState.SkipSpaces:
                    if (hot == ' ') break;
                    if (hot == '>') return true;
                    state = SplitState.CheckSign;
                    continue;

                case SplitState.CheckSign:
                    if (hot == '"')
                    {
                        _parameterValues[_parameterCount++] = QUOTE_FLAG | pos + 1;
                        state = SplitState.SkipOverText;
                        break;
                    }
                    runningValue = 0;
                    signNegative = hot == '-';
                    if (signNegative) break;
                    state = SplitState.BuildParam;
                    continue;

                case SplitState.BuildParam:
                    if (char.IsDigit(hot))
                    {
                        runningValue = 10 * runningValue + (hot - '0');
                        break;
                    }
                    else if (char.IsLetter(hot) || hot == '_')
                    {
                        char upper = char.ToUpper(hot); //The C++ version had the following, but we simplified       if (hot >= 'a' && hot <= 'z')  hot = hot - 'a' + 'A';
                        // Super Kluge to turn keywords into a hash value that can be recognised later
                        runningValue = (runningValue << 5) + runningValue ^ upper;
                        break;
                    }
                    // did not detect 0-9 or keyword so end of parameter detected
                    _parameterValues[_parameterCount++] = runningValue * (signNegative ? -1 : 1);
                    state = SplitState.SkipSpaces;
                    continue;

                case SplitState.SkipOverText:
                    if (hot == '"')
                    {
                        _mutableCmdBuffer[pos] = '\0';  // simulate null-termination - // overwrite " in command buffer with the end-of-string
                        state = SplitState.SkipSpaces;
                    }
                    break;

                case SplitState.CompleteICommand:
                    if (hot == '>')
                    {
                        _mutableCmdBuffer[pos] = '\0';  // simulate null-termination - // overwrite " in command buffer with the end-of-string
                        return true;
                    }
                    break;
            }

            pos++;
        }

        return false;// we ran out of max parameters
    }

    public static bool ParseOld(string command)
    {
        _parameterCount = 0;
        _opcode = '\0';
        _cmdBuffer = command;

        int runningValue = 0;
        int pos = 0;
        bool signNegative = false;
        var state = SplitState.FindStart;

        while (_parameterCount < _maxParams && pos < command.Length)
        {
            char hot = command[pos];

            //TODO - need to implement this check
            //byte hot = *remainingCmd;
            //if (hot == 0)
            //    return false; // no > on end of command.

            // In this switch, break will go on to next char but continue will
            // rescan the current char.
            switch (state)
            {
                case SplitState.FindStart: // looking for <
                    if (hot == '<') state = SplitState.SetOpcode;
                    break;

                case SplitState.SetOpcode:
                    _opcode = hot;
                    if (_opcode == 'i')
                    {
                        // special case <iDCCEX stuff > breaks all normal rules
                        _parameterValues[_parameterCount++] = QUOTE_FLAG | pos + 1;
                        state = SplitState.CompleteICommand;
                        break;
                    }
                    state = SplitState.SkipSpaces;
                    break;

                case SplitState.SkipSpaces: // skipping spaces before a param
                    if (hot == ' ') break; // ignore
                    if (hot == '>') return true;
                    state = SplitState.CheckSign;
                    continue;

                case SplitState.CheckSign: // checking sign or quotes start param.
                    if (hot == '"')
                    {
                        // for a string parameter, the value is the offset of the first char in the cmd.
                        _parameterValues[_parameterCount++] = QUOTE_FLAG | pos + 1;
                        state = SplitState.SkipOverText;
                        break;
                    }
                    runningValue = 0;
                    signNegative = hot == '-';
                    if (signNegative) break;
                    state = SplitState.BuildParam;
                    continue;

                case SplitState.BuildParam:
                    if (char.IsDigit(hot))
                    {
                        runningValue = 10 * runningValue + (hot - '0');
                        break;
                    }
                    else if (char.IsLetter(hot) || hot == '_')
                    {
                        char upper = char.ToUpper(hot);
                        // Super Kluge to turn keywords into a hash value that can be recognised later
                        runningValue = (runningValue << 5) + runningValue ^ upper;
                        break;
                    }
                    // did not detect 0-9 or keyword so end of parameter detected
                    _parameterValues[_parameterCount++] = runningValue * (signNegative ? -1 : 1);
                    state = SplitState.SkipSpaces;
                    continue;

                case SplitState.SkipOverText:
                    if (hot == '"') state = SplitState.SkipSpaces;  
                    break;

                case SplitState.CompleteICommand:
                    if (hot == '>') return true;
                    break;
            }

            pos++;
        }

        return false;
    }

    public static void Dump(IStream output)
    {
        output.WriteLine(""); // Equivalent to println()

        output.Print("DCCEXInbound Opcode='");
        output.Print(_opcode != '\0' ? _opcode.ToString() : "\\0");
        output.Println("'");

        for (int i = 0; i < GetParameterCount(); i++)
        {
            if (IsTextParameter(i))
            {
                output.Print($"GetTextParameter({i})=\"");
                output.Print(GetTextParameter(i));
                output.Println("\"");
            }
            else
            {
                output.Print($"GetNumber({i})=");
                output.Println(GetNumber(i).ToString());
            }
        }
    }


    private static bool IsTextInternal(int param) => (_parameterValues[param] & QUOTE_FLAG_AREA) == QUOTE_FLAG;
}
