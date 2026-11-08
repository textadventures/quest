using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using QuestViva.Common;
using Constants = Microsoft.VisualBasic.Constants;
using Strings = Microsoft.VisualBasic.Strings;

namespace QuestViva.Legacy;

public partial class V4Game
{
    private string RemoveFormatting(string s)
    {
        string code;
        int pos, len = default;

        do
        {
            pos = Strings.InStr(s, "|");
            if (pos != 0)
            {
                code = Strings.Mid(s, pos + 1, 3);

                if (Strings.Left(code, 1) == "b")
                {
                    len = 1;
                }
                else if (Strings.Left(code, 2) == "xb")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 1) == "u")
                {
                    len = 1;
                }
                else if (Strings.Left(code, 2) == "xu")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 1) == "i")
                {
                    len = 1;
                }
                else if (Strings.Left(code, 2) == "xi")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 2) == "cr")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 2) == "cb")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 2) == "cl")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 2) == "cy")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 2) == "cg")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 1) == "n")
                {
                    len = 1;
                }
                else if (Strings.Left(code, 2) == "xn")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 1) == "s")
                {
                    len = 3;
                }
                else if (Strings.Left(code, 2) == "jc")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 2) == "jl")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 2) == "jr")
                {
                    len = 2;
                }
                else if (Strings.Left(code, 1) == "w")
                {
                    len = 1;
                }
                else if (Strings.Left(code, 1) == "c")
                {
                    len = 1;
                }

                if (len == 0)
                    // unknown code
                {
                    len = 1;
                }

                s = Strings.Left(s, pos - 1) + Strings.Mid(s, pos + len + 1);
            }
        } while (pos != 0);

        return s;
    }

    private async Task<bool> CheckSections()
    {
        int defines, braces;
        var checkLine = "";
        int bracePos;
        int pos;
        var section = "";
        bool hasErrors;
        var skipBlock = default(bool);
        _openErrorReport = "";
        hasErrors = false;
        defines = 0;
        braces = 0;

        for (int i = 1, loopTo = Information.UBound(_lines); i <= loopTo; i++)
        {
            if (!BeginsWith(_lines[i], "#!qdk-note: "))
            {
                if (BeginsWith(_lines[i], "define "))
                {
                    section = _lines[i];
                    braces = 0;
                    defines = defines + 1;
                    skipBlock = BeginsWith(_lines[i], "define text") | BeginsWith(_lines[i], "define synonyms");
                }
                else if (Strings.Trim(_lines[i]) == "end define")
                {
                    defines = defines - 1;

                    if (defines < 0)
                    {
                        LogASLError("Extra 'end define' after block '" + section + "'", LogType.FatalError);
                        _openErrorReport = _openErrorReport + "Extra 'end define' after block '" + section + "'" +
                                           Constants.vbCrLf;
                        hasErrors = true;
                        defines = 0;
                    }

                    if (braces > 0)
                    {
                        LogASLError("Missing } in block '" + section + "'", LogType.FatalError);
                        _openErrorReport = _openErrorReport + "Missing } in block '" + section + "'" + Constants.vbCrLf;
                        hasErrors = true;
                    }
                    else if (braces < 0)
                    {
                        LogASLError("Too many } in block '" + section + "'", LogType.FatalError);
                        _openErrorReport = _openErrorReport + "Too many } in block '" + section + "'" +
                                           Constants.vbCrLf;
                        hasErrors = true;
                    }
                }

                if ((Strings.Left(_lines[i], 1) != "'") & !skipBlock)
                {
                    checkLine = await ObliterateParameters(_lines[i]);
                    if (BeginsWith(checkLine, "'<ERROR;"))
                    {
                        // ObliterateParameters denotes a mismatched $, ( etc.
                        // by prefixing line with '<ERROR;*; where * is the mismatched
                        // character
                        LogASLError(
                            "Expected closing " + Strings.Mid(checkLine, 9, 1) + " character in '" +
                            ReportErrorLine(_lines[i]) + "'", LogType.FatalError);
                        _openErrorReport = _openErrorReport + "Expected closing " + Strings.Mid(checkLine, 9, 1) +
                                           " character in '" + ReportErrorLine(_lines[i]) + "'." + Constants.vbCrLf;
                        return false;
                    }
                }

                if (Strings.Left(Strings.Trim(checkLine), 1) != "'")
                {
                    // Now check {
                    pos = 1;
                    do
                    {
                        bracePos = Strings.InStr(pos, checkLine, "{");
                        if (bracePos != 0)
                        {
                            pos = bracePos + 1;
                            braces = braces + 1;
                        }
                    } while (!((bracePos == 0) | (pos > Strings.Len(checkLine))));

                    // Now check }
                    pos = 1;
                    do
                    {
                        bracePos = Strings.InStr(pos, checkLine, "}");
                        if (bracePos != 0)
                        {
                            pos = bracePos + 1;
                            braces = braces - 1;
                        }
                    } while (!((bracePos == 0) | (pos > Strings.Len(checkLine))));
                }
            }
        }

        if (defines > 0)
        {
            LogASLError("Missing 'end define'", LogType.FatalError);
            _openErrorReport = _openErrorReport + "Missing 'end define'." + Constants.vbCrLf;
            hasErrors = true;
        }

        return !hasErrors;
    }

    private async Task<bool> ConvertFriendlyIfs()
    {
        // Converts
        // if (%something% < 3) then ...
        // to
        // if is <%something%;lt;3> then ...
        // and also repeat until ...

        // Returns False if successful

        int convPos, symbPos;
        string symbol;
        int endParamPos;
        string paramData;
        int startParamPos;
        string firstData, secondData;
        string obscureLine, newParam, varObscureLine;
        int bracketCount;

        for (int i = 1, loopTo = Information.UBound(_lines); i <= loopTo; i++)
        {
            obscureLine = await ObliterateParameters(_lines[i]);
            convPos = Strings.InStr(obscureLine, "if (");
            if (convPos == 0)
            {
                convPos = Strings.InStr(obscureLine, "until (");
            }

            if (convPos == 0)
            {
                convPos = Strings.InStr(obscureLine, "while (");
            }

            if (convPos == 0)
            {
                convPos = Strings.InStr(obscureLine, "not (");
            }

            if (convPos == 0)
            {
                convPos = Strings.InStr(obscureLine, "and (");
            }

            if (convPos == 0)
            {
                convPos = Strings.InStr(obscureLine, "or (");
            }


            if (convPos != 0)
            {
                varObscureLine = ObliterateVariableNames(_lines[i]);
                if (BeginsWith(varObscureLine, "'<ERROR;"))
                {
                    // ObliterateVariableNames denotes a mismatched #, % or $
                    // by prefixing line with '<ERROR;*; where * is the mismatched
                    // character
                    LogASLError(
                        "Expected closing " + Strings.Mid(varObscureLine, 9, 1) + " character in '" +
                        ReportErrorLine(_lines[i]) + "'", LogType.FatalError);
                    return true;
                }

                startParamPos = Strings.InStr(convPos, _lines[i], "(");

                endParamPos = 0;
                bracketCount = 1;
                for (int j = startParamPos + 1, loopTo1 = Strings.Len(_lines[i]); j <= loopTo1; j++)
                {
                    if (Strings.Mid(_lines[i], j, 1) == "(")
                    {
                        bracketCount = bracketCount + 1;
                    }
                    else if (Strings.Mid(_lines[i], j, 1) == ")")
                    {
                        bracketCount = bracketCount - 1;
                    }

                    if (bracketCount == 0)
                    {
                        endParamPos = j;
                        break;
                    }
                }

                if (endParamPos == 0)
                {
                    LogASLError("Expected ) in '" + ReportErrorLine(_lines[i]) + "'", LogType.FatalError);
                    return true;
                }

                paramData = Strings.Mid(_lines[i], startParamPos + 1, endParamPos - startParamPos - 1);

                symbPos = Strings.InStr(paramData, "!=");
                if (symbPos == 0)
                {
                    symbPos = Strings.InStr(paramData, "<>");
                    if (symbPos == 0)
                    {
                        symbPos = Strings.InStr(paramData, "<=");
                        if (symbPos == 0)
                        {
                            symbPos = Strings.InStr(paramData, ">=");
                            if (symbPos == 0)
                            {
                                symbPos = Strings.InStr(paramData, "<");
                                if (symbPos == 0)
                                {
                                    symbPos = Strings.InStr(paramData, ">");
                                    if (symbPos == 0)
                                    {
                                        symbPos = Strings.InStr(paramData, "=");
                                        if (symbPos == 0)
                                        {
                                            LogASLError(
                                                "Unrecognised 'if' condition in '" + ReportErrorLine(_lines[i]) + "'",
                                                LogType.FatalError);
                                            return true;
                                        }

                                        symbol = "=";
                                    }
                                    else
                                    {
                                        symbol = ">";
                                    }
                                }
                                else
                                {
                                    symbol = "<";
                                }
                            }
                            else
                            {
                                symbol = ">=";
                            }
                        }
                        else
                        {
                            symbol = "<=";
                        }
                    }
                    else
                    {
                        symbol = "<>";
                    }
                }
                else
                {
                    symbol = "<>";
                }


                firstData = Strings.Trim(Strings.Left(paramData, symbPos - 1));
                secondData = Strings.Trim(Strings.Mid(paramData, symbPos + Strings.Len(symbol)));

                if (symbol == "=")
                {
                    newParam = "is <" + firstData + ";" + secondData + ">";
                }
                else
                {
                    newParam = "is <" + firstData + ";";
                    if (symbol == "<")
                    {
                        newParam = newParam + "lt";
                    }
                    else if (symbol == ">")
                    {
                        newParam = newParam + "gt";
                    }
                    else if (symbol == ">=")
                    {
                        newParam = newParam + "gt=";
                    }
                    else if (symbol == "<=")
                    {
                        newParam = newParam + "lt=";
                    }
                    else if (symbol == "<>")
                    {
                        newParam = newParam + "!=";
                    }

                    newParam = newParam + ";" + secondData + ">";
                }

                _lines[i] = Strings.Left(_lines[i], startParamPos - 1) + newParam +
                            Strings.Mid(_lines[i], endParamPos + 1);

                // Repeat processing this line, in case there are
                // further changes to be made.
                i = i - 1;
            }
        }

        return false;
    }

    private async Task ConvertMultiLineSections()
    {
        int startLine, braceCount;
        string thisLine, lineToAdd;
        var lastBrace = default(int);
        int i;
        string restOfLine, procName;
        int endLineNum;
        string afterLastBrace, z;
        string startOfOrig;
        string testLine;
        int testBraceCount;
        int obp, cbp;
        var curProc = default(int);

        i = 1;
        do
        {
            z = _lines[_defineBlocks[i].StartLine];
            if (!BeginsWith(z, "define text ") & !BeginsWith(z, "define menu ") & (z != "define synonyms"))
            {
                for (int j = _defineBlocks[i].StartLine + 1, loopTo = _defineBlocks[i].EndLine - 1; j <= loopTo; j++)
                {
                    if (Strings.InStr(_lines[j], "{") > 0)
                    {
                        afterLastBrace = "";
                        thisLine = Strings.Trim(_lines[j]);

                        procName = "<!intproc" + curProc + ">";

                        // see if this brace's corresponding closing
                        // brace is on same line:
                        testLine = Strings.Mid(_lines[j], Strings.InStr(_lines[j], "{") + 1);
                        testBraceCount = 1;
                        do
                        {
                            obp = Strings.InStr(testLine, "{");
                            cbp = Strings.InStr(testLine, "}");
                            if (obp == 0)
                            {
                                obp = Strings.Len(testLine) + 1;
                            }

                            if (cbp == 0)
                            {
                                cbp = Strings.Len(testLine) + 1;
                            }

                            if (obp < cbp)
                            {
                                testBraceCount = testBraceCount + 1;
                                testLine = Strings.Mid(testLine, obp + 1);
                            }
                            else if (cbp < obp)
                            {
                                testBraceCount = testBraceCount - 1;
                                testLine = Strings.Mid(testLine, cbp + 1);
                            }
                        } while (!((obp == cbp) | (testBraceCount == 0)));

                        if (testBraceCount != 0)
                        {
                            AddLine("define procedure " + procName);
                            startLine = Information.UBound(_lines);
                            restOfLine = Strings.Trim(Strings.Right(thisLine,
                                Strings.Len(thisLine) - Strings.InStr(thisLine, "{")));
                            braceCount = 1;
                            if (!string.IsNullOrEmpty(restOfLine))
                            {
                                AddLine(restOfLine);
                            }

                            for (int m = 1, loopTo1 = Strings.Len(restOfLine); m <= loopTo1; m++)
                            {
                                if (Strings.Mid(restOfLine, m, 1) == "{")
                                {
                                    braceCount = braceCount + 1;
                                }
                                else if (Strings.Mid(restOfLine, m, 1) == "}")
                                {
                                    braceCount = braceCount - 1;
                                }
                            }

                            if (braceCount != 0)
                            {
                                var k = j + 1;
                                do
                                {
                                    for (int m = 1, loopTo2 = Strings.Len(_lines[k]); m <= loopTo2; m++)
                                    {
                                        if (Strings.Mid(_lines[k], m, 1) == "{")
                                        {
                                            braceCount = braceCount + 1;
                                        }
                                        else if (Strings.Mid(_lines[k], m, 1) == "}")
                                        {
                                            braceCount = braceCount - 1;
                                        }

                                        if (braceCount == 0)
                                        {
                                            lastBrace = m;
                                            break;
                                        }
                                    }

                                    if (braceCount != 0)
                                    {
                                        // put Lines(k) into another variable, as
                                        // AddLine ReDims Lines, which it can't do if
                                        // passed Lines(x) as a parameter.
                                        lineToAdd = _lines[k];
                                        AddLine(lineToAdd);
                                    }
                                    else
                                    {
                                        AddLine(Strings.Left(_lines[k], lastBrace - 1));
                                        afterLastBrace = Strings.Trim(Strings.Mid(_lines[k], lastBrace + 1));
                                    }

                                    // Clear original line
                                    _lines[k] = "";
                                    k = k + 1;
                                } while (braceCount != 0);
                            }

                            AddLine("end define");
                            endLineNum = Information.UBound(_lines);

                            _numberSections = _numberSections + 1;
                            Array.Resize(ref _defineBlocks, _numberSections + 1);
                            _defineBlocks[_numberSections] = new DefineBlock();
                            _defineBlocks[_numberSections].StartLine = startLine;
                            _defineBlocks[_numberSections].EndLine = endLineNum;

                            // Change original line where the { section
                            // started to call the new procedure.
                            startOfOrig = Strings.Trim(Strings.Left(thisLine, Strings.InStr(thisLine, "{") - 1));
                            _lines[j] = startOfOrig + " do " + procName + " " + afterLastBrace;
                            curProc = curProc + 1;

                            // Process this line again in case there was stuff after the last brace that included
                            // more braces. e.g. } else {
                            j = j - 1;
                        }
                    }
                }
            }

            i = i + 1;
        } while (i <= _numberSections);

        // Join next-line "else"s to corresponding "if"s

        var loopTo3 = _numberSections;
        for (i = 1; i <= loopTo3; i++)
        {
            z = _lines[_defineBlocks[i].StartLine];
            if (!BeginsWith(z, "define text ") & !BeginsWith(z, "define menu ") & (z != "define synonyms"))
            {
                for (int j = _defineBlocks[i].StartLine + 1, loopTo4 = _defineBlocks[i].EndLine - 1; j <= loopTo4; j++)
                {
                    if (BeginsWith(_lines[j], "else "))
                        // Go upwards to find "if" statement that this
                        // belongs to
                    {
                        for (int k = j, loopTo5 = _defineBlocks[i].StartLine + 1; k >= loopTo5; k -= 1)
                        {
                            if (BeginsWith(_lines[k], "if ") |
                                (Strings.InStr(await ObliterateParameters(_lines[k]), " if ") != 0))
                            {
                                _lines[k] = _lines[k] + " " + Strings.Trim(_lines[j]);
                                _lines[j] = "";
                                k = _defineBlocks[i].StartLine;
                            }
                        }
                    }
                }
            }
        }
    }

    private async Task<bool> ErrorCheck()
    {
        // Parses ASL script for errors. Returns TRUE if OK;
        // False if a critical error is encountered.
        int curBegin, curEnd;
        bool hasErrors;
        int curPos;
        int numParamStart, numParamEnd;
        bool finLoop, inText;

        hasErrors = false;
        inText = false;

        // Checks for incorrect number of < and > :
        for (int i = 1, loopTo = Information.UBound(_lines); i <= loopTo; i++)
        {
            numParamStart = 0;
            numParamEnd = 0;

            if (BeginsWith(_lines[i], "define text "))
            {
                inText = true;
            }

            if (inText & (Strings.Trim(Strings.LCase(_lines[i])) == "end define"))
            {
                inText = false;
            }

            if (!inText)
            {
                // Find number of <'s:
                curPos = 1;
                finLoop = false;
                do
                {
                    if (Strings.InStr(curPos, _lines[i], "<") != 0)
                    {
                        numParamStart = numParamStart + 1;
                        curPos = Strings.InStr(curPos, _lines[i], "<") + 1;
                    }
                    else
                    {
                        finLoop = true;
                    }
                } while (!finLoop);

                // Find number of >'s:
                curPos = 1;
                finLoop = false;
                do
                {
                    if (Strings.InStr(curPos, _lines[i], ">") != 0)
                    {
                        numParamEnd = numParamEnd + 1;
                        curPos = Strings.InStr(curPos, _lines[i], ">") + 1;
                    }
                    else
                    {
                        finLoop = true;
                    }
                } while (!finLoop);

                if (numParamStart > numParamEnd)
                {
                    LogASLError("Expected > in " + ReportErrorLine(_lines[i]), LogType.FatalError);
                    hasErrors = true;
                }
                else if (numParamStart < numParamEnd)
                {
                    LogASLError("Too many > in " + ReportErrorLine(_lines[i]), LogType.FatalError);
                    hasErrors = true;
                }
            }
        }

        // Exit if errors found
        if (hasErrors)
        {
            return true;
        }

        // Checks that define sections have parameters:
        for (int i = 1, loopTo1 = _numberSections; i <= loopTo1; i++)
        {
            curBegin = _defineBlocks[i].StartLine;
            curEnd = _defineBlocks[i].EndLine;

            if (BeginsWith(_lines[curBegin], "define game"))
            {
                if (Strings.InStr(_lines[curBegin], "<") == 0)
                {
                    LogASLError("'define game' has no parameter - game has no name", LogType.FatalError);
                    return true;
                }
            }
            else if (!BeginsWith(_lines[curBegin], "define synonyms") & !BeginsWith(_lines[curBegin], "define options"))
            {
                if (Strings.InStr(_lines[curBegin], "<") == 0)
                {
                    LogASLError(_lines[curBegin] + " has no parameter", LogType.FatalError);
                    hasErrors = true;
                }
            }
        }

        return hasErrors;
    }

    private async Task<string> ObliterateParameters(string s)
    {
        bool inParameter;
        var exitCharacter = "";
        string curChar;
        var outputLine = "";
        var obscuringFunctionName = default(bool);

        inParameter = false;

        for (int i = 1, loopTo = Strings.Len(s); i <= loopTo; i++)
        {
            curChar = Strings.Mid(s, i, 1);

            if (inParameter)
            {
                if (exitCharacter == ")")
                {
                    if (Strings.InStr("$#%", curChar) > 0)
                    {
                        // We might be converting a line like:
                        // if ( $rand(1;10)$ < 3 ) then {
                        // and we don't want it to end up like this:
                        // if (~~~~~~~~~~~)$ <~~~~~~~~~~~
                        // which will cause all sorts of confustion. So,
                        // we get rid of everything between the $ characters
                        // in this case, and set a flag so we know what we're
                        // doing.
                        obscuringFunctionName = true;
                        exitCharacter = curChar;

                        // Move along please
                        outputLine = outputLine + "~";
                        i = i + 1;
                        curChar = Strings.Mid(s, i, 1);
                    }
                }
            }

            if (!inParameter)
            {
                outputLine = outputLine + curChar;
                if (curChar == "<")
                {
                    inParameter = true;
                    exitCharacter = ">";
                }

                if (curChar == "(")
                {
                    inParameter = true;
                    exitCharacter = ")";
                }
            }
            else if ((curChar ?? "") == (exitCharacter ?? ""))
            {
                if (!obscuringFunctionName)
                {
                    inParameter = false;
                    outputLine = outputLine + curChar;
                }
                else
                {
                    // We've finished obscuring the function name,
                    // now let's find the next ) as we were before
                    // we found this dastardly function
                    obscuringFunctionName = false;
                    exitCharacter = ")";
                    outputLine = outputLine + "~";
                }
            }
            else
            {
                outputLine = outputLine + "~";
            }
        }

        if (inParameter)
        {
            return "'<ERROR;" + exitCharacter + ";" + outputLine;
        }

        return outputLine;
    }

    private string ObliterateVariableNames(string s)
    {
        bool inParameter;
        var exitCharacter = "";
        var outputLine = "";
        string curChar;

        inParameter = false;

        for (int i = 1, loopTo = Strings.Len(s); i <= loopTo; i++)
        {
            curChar = Strings.Mid(s, i, 1);
            if (!inParameter)
            {
                outputLine = outputLine + curChar;
                if (curChar == "$")
                {
                    inParameter = true;
                    exitCharacter = "$";
                }

                if (curChar == "#")
                {
                    inParameter = true;
                    exitCharacter = "#";
                }

                if (curChar == "%")
                {
                    inParameter = true;
                    exitCharacter = "%";
                }

                // The ~ was for collectables, and this syntax only
                // exists in Quest 2.x. The ~ was only finally
                // allowed to be present on its own in ASL 320.
                if ((curChar == "~") & (ASLVersion < 320))
                {
                    inParameter = true;
                    exitCharacter = "~";
                }
            }
            else if ((curChar ?? "") == (exitCharacter ?? ""))
            {
                inParameter = false;
                outputLine = outputLine + curChar;
            }
            else
            {
                outputLine = outputLine + "X";
            }
        }

        if (inParameter)
        {
            outputLine = "'<ERROR;" + exitCharacter + ";" + outputLine;
        }

        return outputLine;
    }

    private async Task RemoveComments()
    {
        int aposPos;
        var inTextBlock = default(bool);
        var inSynonymsBlock = default(bool);
        string oblitLine;

        // If in a synonyms block, we want to remove lines which are comments, but
        // we don't want to remove synonyms that contain apostrophes, so we only
        // get rid of lines with an "'" at the beginning or with " '" in them

        for (int i = 1, loopTo = Information.UBound(_lines); i <= loopTo; i++)
        {
            if (BeginsWith(_lines[i], "'!qdk-note:"))
            {
                _lines[i] = "#!qdk-note:" + GetEverythingAfter(_lines[i], "'!qdk-note:");
            }
            else
            {
                if (BeginsWith(_lines[i], "define text "))
                {
                    inTextBlock = true;
                }
                else if (Strings.Trim(_lines[i]) == "define synonyms")
                {
                    inSynonymsBlock = true;
                }
                else if (BeginsWith(_lines[i], "define type "))
                {
                    inSynonymsBlock = true;
                }
                else if (Strings.Trim(_lines[i]) == "end define")
                {
                    inTextBlock = false;
                    inSynonymsBlock = false;
                }

                if (!inTextBlock & !inSynonymsBlock)
                {
                    if (Strings.InStr(_lines[i], "'") > 0)
                    {
                        oblitLine = await ObliterateParameters(_lines[i]);
                        if (!BeginsWith(oblitLine, "'<ERROR;"))
                        {
                            aposPos = Strings.InStr(oblitLine, "'");

                            if (aposPos != 0)
                            {
                                _lines[i] = Strings.Trim(Strings.Left(_lines[i], aposPos - 1));
                            }
                        }
                    }
                }
                else if (inSynonymsBlock)
                {
                    if (Strings.Left(Strings.Trim(_lines[i]), 1) == "'")
                    {
                        _lines[i] = "";
                    }
                    else
                    {
                        // we look for " '", not "'" in synonyms lines
                        aposPos = Strings.InStr(await ObliterateParameters(_lines[i]), " '");
                        if (aposPos != 0)
                        {
                            _lines[i] = Strings.Trim(Strings.Left(_lines[i], aposPos - 1));
                        }
                    }
                }
            }
        }
    }

    private string ReportErrorLine(string s)
    {
        // We don't want to see the "!intproc" in logged error reports lines.
        // This function replaces these "do" lines with a nicer-looking "..." for error reporting.

        int replaceFrom;

        replaceFrom = Strings.InStr(s, "do <!intproc");
        if (replaceFrom != 0)
        {
            return Strings.Left(s, replaceFrom - 1) + "...";
        }

        return s;
    }

    private string ConvertCasKeyword(string casChar)
    {
        var c = Encoding.GetEncoding(1252).GetBytes(casChar)[0];
        var keyword = _casKeywords[c];

        if (keyword == "!cr")
        {
            keyword = Constants.vbCrLf;
        }

        return keyword;
    }

    private async Task ConvertMultiLines()
    {
        // Goes through each section capable of containing
        // script commands and puts any multiple-line script commands
        // into separate procedures. Also joins multiple-line "if"
        // statements.

        // This calls RemoveComments after joining lines, so that lines
        // with "'" as part of a multi-line parameter are not destroyed,
        // before looking for braces.

        for (var i = Information.UBound(_lines); i >= 1; i -= 1)
        {
            if (Strings.Right(_lines[i], 2) == "__")
            {
                _lines[i] = Strings.Left(_lines[i], Strings.Len(_lines[i]) - 2) + Strings.LTrim(_lines[i + 1]);
                _lines[i + 1] = "";
                // Recalculate this line again
                i = i + 1;
            }
            else if (Strings.Right(_lines[i], 1) == "_")
            {
                _lines[i] = Strings.Left(_lines[i], Strings.Len(_lines[i]) - 1) + Strings.LTrim(_lines[i + 1]);
                _lines[i + 1] = "";
                // Recalculate this line again
                i = i + 1;
            }
        }

        await RemoveComments();
    }

    private string Keyword2CAS(string KWord)
    {
        if (string.IsNullOrEmpty(KWord))
        {
            return "";
        }

        for (var i = 0; i <= 255; i++)
        {
            if ((Strings.LCase(KWord) ?? "") == (Strings.LCase(_casKeywords[i]) ?? ""))
            {
                return Conversions.ToString(InvariantStrings.Chr(i));
            }
        }

        return Keyword2CAS("!unknown") + KWord + Keyword2CAS("!unknown");
    }

    private void LoadCASKeywords()
    {
        // Loads data required for conversion of CAS files

        var questDatLines = GetResourceLines(Resources.GetResourceBytes(Resources.QuestDAT));

        foreach (var line in questDatLines)
        {
            if (Strings.Left(line, 1) != "#")
            {
                // Lines isn't a comment - so parse it.
                var scp = Strings.InStr(line, ";");
                var keyword = Strings.Trim(Strings.Left(line, scp - 1));
                var num = Conversions.ToInteger(Strings.Right(line, Strings.Len(line) - scp));
                _casKeywords[num] = keyword;
            }
        }
    }

    private string[] GetResourceLines(byte[] res)
    {
        var enc = new UTF8Encoding();
        var resFile = enc.GetString(res);

        return resFile.Split(["\r\n", "\n"], StringSplitOptions.None);
    }

    private async Task<string> GetFileData(GameData gameData)
    {
        var stream = gameData.Data;
        return await new StreamReader(stream, Encoding.GetEncoding(1252)).ReadToEndAsync();
    }

    private async Task<bool> ParseFile(GameData gameData)
    {
        var libCode = new string[1];
        int l;
        var gameLine = 0;
        var synLine = 0;
        bool libFoundThisSweep;
        var libraryList = new string[1];
        var typeLine = 0;
        var filename = gameData.Filename;

        _defineBlockParams = new Dictionary<string, Dictionary<string, string>>();

        var ext = Path.GetExtension(filename).ToLowerInvariant();

        switch (ext)
        {
            case ".asl":
            {
                // Read file into Lines array
                var fileData = await GetFileData(gameData);

                var aslLines = fileData.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);
                _lines = new string[aslLines.Length + 1];
                _lines[0] = "";

                var loopTo = aslLines.Length;
                for (l = 1; l <= loopTo; l++)
                {
                    _lines[l] = aslLines[l - 1];
                    _lines[l] = RemoveTabs(_lines[l]);
                    _lines[l] = _lines[l].Trim(' ', '\n', '\r');
                }

                l = aslLines.Length;
                break;
            }
            case ".cas":
                LogASLError("Loading CAS");
                LoadCASFile(gameData);
                l = Information.UBound(_lines);
                break;
            default:
                throw new InvalidOperationException("Unrecognized file extension");
        }

        // Add libraries to end of code:
        var numLibraries = 0;

        do
        {
            libFoundThisSweep = false;
            for (var i = l; i >= 1; i -= 1)
                // We search for includes backwards as a game might include
                // some-general.lib and then something-specific.lib which needs
                // something-general; if we include something-specific first,
                // then something-general afterwards, something-general's startscript
                // gets executed before something-specific's, as we execute the
                // lib startscripts backwards as well
            {
                if (!BeginsWith(_lines[i], "!include "))
                {
                    continue;
                }

                var libFileName = await GetParameter(_lines[i], _nullContext);
                // Clear !include statement
                _lines[i] = "";
                var libraryAlreadyIncluded = false;
                LogASLError("Including library '" + libFileName + "'...", LogType.Init);

                for (int j = 1, loopTo1 = numLibraries; j <= loopTo1; j++)
                {
                    if ((Strings.LCase(libFileName) ?? "") == (Strings.LCase(libraryList[j]) ?? ""))
                    {
                        libraryAlreadyIncluded = true;
                        break;
                    }
                }

                if (libraryAlreadyIncluded)
                {
                    LogASLError("     - Library already included.", LogType.Init);
                }
                else
                {
                    numLibraries = numLibraries + 1;
                    Array.Resize(ref libraryList, numLibraries + 1);
                    libraryList[numLibraries] = libFileName;

                    libFoundThisSweep = true;
                    string[] libResourceLines = null;

                    LogASLError(" - Searching for " + libFileName + " (game path)", LogType.Init);

                    var libStream = _gameData.GetAdjacentFile(libFileName);

                    if (libStream == null)
                    {
                        // File was not found; try standard Quest libraries (stored here as resources)
                        LogASLError("     - Library not found in game path.", LogType.Init);
                        LogASLError(" - Searching for " + libFileName + " (standard libraries)", LogType.Init);
                        libResourceLines = GetLibraryLines(libFileName);

                        if (libResourceLines is null)
                        {
                            LogASLError("Library not found.", LogType.FatalError);
                            _openErrorReport = _openErrorReport + "Library '" + libraryList[numLibraries] +
                                               "' not found." + Constants.vbCrLf;
                            return false;
                        }
                    }

                    LogASLError("     - Found library, opening...", LogType.Init);

                    var libLines = 0;

                    string libLine;
                    if (libResourceLines is null)
                    {
                        var libData = await new StreamReader(libStream).ReadToEndAsync();
                        var libDataLines = libData.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);

                        foreach (var line in libDataLines)
                        {
                            libLines = libLines + 1;
                            libLine = RemoveTabs(line);
                            Array.Resize(ref libCode, libLines + 1);
                            libCode[libLines] = Strings.Trim(libLine);
                        }
                    }
                    else
                    {
                        foreach (var resLibLine in libResourceLines)
                        {
                            libLines = libLines + 1;
                            Array.Resize(ref libCode, libLines + 1);
                            libLine = resLibLine;
                            libLine = RemoveTabs(libLine);
                            libCode[libLines] = Strings.Trim(libLine);
                        }
                    }

                    var libVer = -1;

                    int c;
                    if (libCode[1] == "!library")
                    {
                        var loopTo2 = libLines;
                        for (c = 1; c <= loopTo2; c++)
                        {
                            if (BeginsWith(libCode[c], "!asl-version "))
                            {
                                libVer = Conversions.ToInteger(await GetParameter(libCode[c], _nullContext));
                                break;
                            }
                        }
                    }
                    else
                    {
                        // Old library
                        libVer = 100;
                    }

                    if (libVer == -1)
                    {
                        LogASLError(" - Library has no asl-version information.", LogType.LibraryWarningError);
                        libVer = 200;
                    }

                    var ignoreMode = false;
                    var loopTo3 = libLines;
                    for (c = 1; c <= loopTo3; c++)
                    {
                        if (BeginsWith(libCode[c], "!include "))
                        {
                            // Quest only honours !include in a library for asl-version
                            // 311 or later, as it ignored them in versions < 3.11
                            if (libVer >= 311)
                            {
                                AddLine(libCode[c]);
                                l = l + 1;
                            }
                        }
                        else if ((Strings.Left(libCode[c], 1) != "!") & (Strings.Left(libCode[c], 1) != "'") &
                                 !ignoreMode)
                        {
                            AddLine(libCode[c]);
                            l = l + 1;
                        }
                        else
                        {
                            int d;
                            if (libCode[c] == "!addto game")
                            {
                                var inDefGameBlock = 0;
                                var loopTo4 = Information.UBound(_lines);
                                for (d = 1; d <= loopTo4; d++)
                                {
                                    if (BeginsWith(_lines[d], "define game "))
                                    {
                                        inDefGameBlock = 1;
                                    }
                                    else if (BeginsWith(_lines[d], "define "))
                                    {
                                        if (inDefGameBlock != 0)
                                        {
                                            inDefGameBlock = inDefGameBlock + 1;
                                        }
                                    }
                                    else if ((_lines[d] == "end define") & (inDefGameBlock == 1))
                                    {
                                        gameLine = d;
                                        d = Information.UBound(_lines);
                                    }
                                    else if (_lines[d] == "end define")
                                    {
                                        if (inDefGameBlock != 0)
                                        {
                                            inDefGameBlock = inDefGameBlock - 1;
                                        }
                                    }
                                }

                                do
                                {
                                    c = c + 1;
                                    if (BeginsWith(libCode[c], "!end"))
                                    {
                                        continue;
                                    }

                                    Array.Resize(ref _lines, Information.UBound(_lines) + 1 + 1);
                                    var loopTo5 = gameLine + 1;
                                    for (d = Information.UBound(_lines); d >= loopTo5; d -= 1)
                                    {
                                        _lines[d] = _lines[d - 1];
                                    }

                                    // startscript lines in a library are prepended
                                    // with "lib" internally so they are executed
                                    // before any startscript specified by the
                                    // calling ASL file, for asl-versions 311 and
                                    // later.
                                    // similarly, commands in a library. NB: without this, lib
                                    // verbs have lower precedence than game verbs anyway. Also
                                    // lib commands have lower precedence than game commands. We
                                    // only need this code so that game verbs have a higher
                                    // precedence than lib commands.
                                    // we also need it so that lib verbs have a higher
                                    // precedence than lib commands.
                                    if ((libVer >= 311) & BeginsWith(libCode[c], "startscript "))
                                    {
                                        _lines[gameLine] = "lib " + libCode[c];
                                    }
                                    else if ((libVer >= 392) & (BeginsWith(libCode[c], "command ") |
                                                                BeginsWith(libCode[c], "verb ")))
                                    {
                                        _lines[gameLine] = "lib " + libCode[c];
                                    }
                                    else
                                    {
                                        _lines[gameLine] = libCode[c];
                                    }

                                    l = l + 1;
                                    gameLine = gameLine + 1;
                                } while (!BeginsWith(libCode[c], "!end"));
                            }
                            else if (libCode[c] == "!addto synonyms")
                            {
                                var inDefSynBlock = 0;
                                var loopTo6 = Information.UBound(_lines);
                                for (d = 1; d <= loopTo6; d++)
                                {
                                    if (_lines[d] == "define synonyms")
                                    {
                                        inDefSynBlock = 1;
                                    }
                                    else if ((_lines[d] == "end define") & (inDefSynBlock == 1))
                                    {
                                        synLine = d;
                                        d = Information.UBound(_lines);
                                    }
                                }

                                if (inDefSynBlock == 0)
                                {
                                    // No "define synonyms" block in game - so add it
                                    AddLine("define synonyms");
                                    AddLine("end define");
                                    synLine = Information.UBound(_lines);
                                }

                                do
                                {
                                    c = c + 1;
                                    if (BeginsWith(libCode[c], "!end"))
                                    {
                                        continue;
                                    }

                                    Array.Resize(ref _lines, Information.UBound(_lines) + 1 + 1);
                                    var loopTo7 = synLine + 1;
                                    for (d = Information.UBound(_lines); d >= loopTo7; d -= 1)
                                    {
                                        _lines[d] = _lines[d - 1];
                                    }

                                    _lines[synLine] = libCode[c];
                                    l = l + 1;
                                    synLine = synLine + 1;
                                } while (!BeginsWith(libCode[c], "!end"));
                            }
                            else if (BeginsWith(libCode[c], "!addto type "))
                            {
                                var inDefTypeBlock = 0;
                                var typeBlockName = Strings.LCase(await GetParameter(libCode[c], _nullContext));
                                var loopTo8 = Information.UBound(_lines);
                                for (d = 1; d <= loopTo8; d++)
                                {
                                    if ((Strings.LCase(_lines[d]) ?? "") ==
                                        ("define type <" + typeBlockName + ">" ?? ""))
                                    {
                                        inDefTypeBlock = 1;
                                    }
                                    else if ((_lines[d] == "end define") & (inDefTypeBlock == 1))
                                    {
                                        typeLine = d;
                                        d = Information.UBound(_lines);
                                    }
                                }

                                if (inDefTypeBlock == 0)
                                {
                                    // No "define type (whatever)" block in game - so add it
                                    AddLine("define type <" + typeBlockName + ">");
                                    AddLine("end define");
                                    typeLine = Information.UBound(_lines);
                                }

                                do
                                {
                                    c = c + 1;
                                    if (c > libLines)
                                    {
                                        break;
                                    }

                                    if (BeginsWith(libCode[c], "!end"))
                                    {
                                        continue;
                                    }

                                    Array.Resize(ref _lines, Information.UBound(_lines) + 1 + 1);
                                    var loopTo9 = typeLine + 1;
                                    for (d = Information.UBound(_lines); d >= loopTo9; d -= 1)
                                    {
                                        _lines[d] = _lines[d - 1];
                                    }

                                    _lines[typeLine] = libCode[c];
                                    l = l + 1;
                                    typeLine = typeLine + 1;
                                } while (!BeginsWith(libCode[c], "!end"));
                            }
                            else if (libCode[c] == "!library")
                            {
                            }
                            // ignore
                            else if (BeginsWith(libCode[c], "!asl-version "))
                            {
                            }
                            // ignore
                            else if (BeginsWith(libCode[c], "'"))
                            {
                            }
                            // ignore
                            else if (BeginsWith(libCode[c], "!QDK"))
                            {
                                ignoreMode = true;
                            }
                            else if (BeginsWith(libCode[c], "!end"))
                            {
                                ignoreMode = false;
                            }
                        }
                    }
                }
            }
        } while (libFoundThisSweep);

        var skipCheck = false;

        int lastSlashPos = default, slashPos;
        var curPos = 1;
        do
        {
            slashPos = Strings.InStr(curPos, filename, @"\");
            if (slashPos == 0)
            {
                slashPos = Strings.InStr(curPos, filename, "/");
            }

            if (slashPos != 0)
            {
                lastSlashPos = slashPos;
            }

            curPos = slashPos + 1;
        } while (slashPos != 0);

        var filenameNoPath = Strings.LCase(Strings.Mid(filename, lastSlashPos + 1));

        for (int i = 1, loopTo10 = _numSkipCheckFiles; i <= loopTo10; i++)
        {
            if ((filenameNoPath ?? "") == (_skipCheckFile[i] ?? ""))
            {
                skipCheck = true;
                break;
            }
        }

        if (filenameNoPath == "musicvf1.cas")
        {
            _useStaticFrameForPictures = true;
        }

        // RemoveComments called within ConvertMultiLines
        await ConvertMultiLines();

        if (!skipCheck)
        {
            if (!await CheckSections())
            {
                return false;
            }
        }

        _numberSections = 1;

        for (int i = 1, loopTo11 = l; i <= loopTo11; i++)
            // find section beginning with 'define'
        {
            if (!BeginsWith(_lines[i], "define"))
            {
                continue;
            }

            // Now, go through until we reach an 'end define'. However, if we
            // encounter another 'define' there is a nested define. So, if we
            // encounter 'define' we increment the definecount. When we find an
            // 'end define' we decrement it. When definecount is zero, we have
            // found the end of the section.
            var defineCount = 1;

            // Don't count the current line - we know it begins with 'define'...
            var curLine = i + 1;
            do
            {
                if (BeginsWith(_lines[curLine], "define"))
                {
                    defineCount = defineCount + 1;
                }
                else if (BeginsWith(_lines[curLine], "end define"))
                {
                    defineCount = defineCount - 1;
                }

                curLine = curLine + 1;
            } while (defineCount != 0);

            curLine = curLine - 1;

            // Now, we know that the define section begins at i and ends at
            // curline. Remember where the section begins and ends:
            Array.Resize(ref _defineBlocks, _numberSections + 1);
            _defineBlocks[_numberSections] = new DefineBlock();
            _defineBlocks[_numberSections].StartLine = i;
            _defineBlocks[_numberSections].EndLine = curLine;

            _numberSections = _numberSections + 1;
            i = curLine;
        }

        _numberSections = _numberSections - 1;

        var gotGameBlock = false;
        for (int i = 1, loopTo12 = _numberSections; i <= loopTo12; i++)
        {
            if (BeginsWith(_lines[_defineBlocks[i].StartLine], "define game "))
            {
                gotGameBlock = true;
                break;
            }
        }

        if (!gotGameBlock)
        {
            _openErrorReport = _openErrorReport + "No 'define game' block." + Constants.vbCrLf;
            return false;
        }

        await ConvertMultiLineSections();

        var hasErrors = await ConvertFriendlyIfs();
        if (!hasErrors)
        {
            hasErrors = await ErrorCheck();
        }

        if (hasErrors)
        {
            throw new InvalidOperationException("Errors found in game file.");
        }

        return true;
    }

    private void AddLine(string line)
    {
        // Adds a line to the game script
        int numLines;

        numLines = Information.UBound(_lines) + 1;
        Array.Resize(ref _lines, numLines + 1);
        _lines[numLines] = line;
    }

    private string GetCASFileData(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.GetEncoding(1252));
        return reader.ReadToEnd();
    }

    private void LoadCASFile(GameData gameData)
    {
        var textMode = false;
        var startCat = "";
        var j = 0;

        _lines = new string[1];

        var fileData = GetCASFileData(gameData.Data);

        var chkVer = Strings.Left(fileData, 7);
        var casVersion = chkVer switch
        {
            "QCGF001" => 1,
            "QCGF002" => 2,
            "QCGF003" => 3,
            _ => throw new InvalidOperationException("Invalid or corrupted CAS file.")
        };

        if (casVersion == 3)
        {
            startCat = Keyword2CAS("!startcat");
        }

        for (int i = 9, loopTo = Strings.Len(fileData); i <= loopTo; i++)
        {
            if ((casVersion == 3) & ((Strings.Mid(fileData, i, 1) ?? "") == (startCat ?? "")))
            {
                // Read catalog
                _startCatPos = i;
                var endCatPos = Strings.InStr(j, fileData, Keyword2CAS("!endcat"));
                ReadCatalog(Strings.Mid(fileData, j + 1, endCatPos - j - 1));
                _hasResources = true;
                _resourceOffset = endCatPos + 1;
                i = Strings.Len(fileData);
                _casFileData = fileData;
            }
            else
            {
                var curLin = "";
                var endLineReached = false;
                if (textMode)
                {
                    var textData = Strings.Mid(fileData, i,
                        Strings.InStr(i, fileData, Conversions.ToString(InvariantStrings.Chr(253))) - (i - 1));
                    textData = Strings.Left(textData, Strings.Len(textData) - 1);
                    var cpos = 1;
                    var finished = false;

                    if (!string.IsNullOrEmpty(textData))
                    {
                        do
                        {
                            var nextLinePos = Strings.InStr(cpos, textData, "\0");
                            if (nextLinePos == 0)
                            {
                                nextLinePos = Strings.Len(textData) + 1;
                                finished = true;
                            }

                            var tl = DecryptString(Strings.Mid(textData, cpos, nextLinePos - cpos));
                            AddLine(tl);
                            cpos = nextLinePos + 1;
                        } while (!finished);
                    }

                    textMode = false;
                    i = Strings.InStr(i, fileData, Conversions.ToString(InvariantStrings.Chr(253)));
                }

                j = i;
                do
                {
                    var ckw = Strings.Mid(fileData, j, 1);
                    var c = ConvertCasKeyword(ckw);

                    if ((c ?? "") == Constants.vbCrLf)
                    {
                        endLineReached = true;
                    }
                    else if (Strings.Left(c, 1) != "!")
                    {
                        curLin = curLin + c + " ";
                    }
                    else
                    {
                        bool exitTheLoop;
                        string d;
                        switch (c)
                        {
                            case "!quote":
                            {
                                exitTheLoop = false;
                                curLin += "<";
                                do
                                {
                                    j = j + 1;
                                    d = Strings.Mid(fileData, j, 1);
                                    if (d != "\0")
                                    {
                                        curLin += DecryptString(d);
                                    }
                                    else
                                    {
                                        curLin += "> ";
                                        exitTheLoop = true;
                                    }
                                } while (!exitTheLoop);

                                break;
                            }
                            case "!unknown":
                            {
                                exitTheLoop = false;
                                do
                                {
                                    j = j + 1;
                                    d = Strings.Mid(fileData, j, 1);
                                    if (d != Conversions.ToString(InvariantStrings.Chr(254)))
                                    {
                                        curLin += d;
                                    }
                                    else
                                    {
                                        exitTheLoop = true;
                                    }
                                } while (!exitTheLoop);

                                curLin += " ";
                                break;
                            }
                        }
                    }

                    j += 1;
                } while (!endLineReached);

                AddLine(Strings.Trim(curLin));
                if (BeginsWith(curLin, "define text") | ((casVersion >= 2) & (BeginsWith(curLin, "define synonyms") |
                                                                              BeginsWith(curLin, "define type") |
                                                                              BeginsWith(curLin, "define menu"))))
                {
                    textMode = true;
                }

                // j is already at correct place, but i will be
                // incremented - so put j back one or we will miss a
                // character.
                i = j - 1;
            }
        }
    }

    private string DecryptString(string s)
    {
        var output = "";
        for (int i = 1, loopTo = Strings.Len(s); i <= loopTo; i++)
        {
            var v = Encoding.GetEncoding(1252).GetBytes(Strings.Mid(s, i, 1))[0];
            output = output + InvariantStrings.Chr(v ^ 255);
        }

        return output;
    }

    private string RemoveTabs(string s)
    {
        if (Strings.InStr(s, "\t") > 0)
        {
            // Remove tab characters and change them into
            // spaces; otherwise they bugger up the Trim
            // commands.
            var cpos = 1;
            var finished = false;
            do
            {
                var tabChar = Strings.InStr(cpos, s, "\t");
                if (tabChar != 0)
                {
                    s = Strings.Left(s, tabChar - 1) + Strings.Space(4) + Strings.Mid(s, tabChar + 1);
                    cpos = tabChar + 1;
                }
                else
                {
                    finished = true;
                }
            } while (!finished);
        }

        return s;
    }

    private Stream ExtractFile(string file)
    {
        var length = 0;
        var startPos = 0;

        if (!_hasResources)
        {
            return null;
        }

        // Find file in catalog

        var found = false;

        for (int i = 1, loopTo = _numResources; i <= loopTo; i++)
        {
            if ((Strings.LCase(file) ?? "") != (Strings.LCase(_resources[i].ResourceName) ?? ""))
            {
                continue;
            }

            found = true;
            startPos = _resources[i].ResourceStart + _resourceOffset;
            length = _resources[i].ResourceLength;
            break;
        }

        if (!found)
        {
            LogASLError("Unable to extract '" + file + "' - not present in resources.", LogType.WarningError);
            return null;
        }

        // Extract file from cached CAS data
        var fileData = Strings.Mid(_casFileData, startPos, length);
        var encoding = Encoding.GetEncoding(1252);
        var bytes = encoding.GetBytes(fileData);
        var stream = new MemoryStream(bytes);

        return stream;
    }

    private async Task<bool> OpenGame(Stream saveData)
    {
        string[] lines = null;

        _gameLoadMethod = "loaded";

        var prevQsgVersion = false;

        using var reader = new StreamReader(saveData, Encoding.GetEncoding(1252));
        var fileData = await reader.ReadToEndAsync();

        // Check version
        var savedQsgVersion = Strings.Left(fileData, 10);

        if (BeginsWith(savedQsgVersion, "QUEST200.1"))
        {
            prevQsgVersion = true;
        }
        else if (!BeginsWith(savedQsgVersion, "QUEST300"))
        {
            return false;
        }

        if (prevQsgVersion)
        {
            lines = fileData.Split([Constants.vbCrLf, Constants.vbLf], StringSplitOptions.None);
        }
        else
        {
            InitFileData(fileData);
            GetNextChunk();

            // This previously contained the original game filename, but we don't need this now as we always get
            // the original game data when the game is initialised.
            var _ = GetNextChunk();
        }

        var result = await InitialiseGame(_gameData, true);

        if (!result)
        {
            return false;
        }

        if (!prevQsgVersion)
        {
            // Open Quest 3.0 saved game file
            _gameLoading = true;
            await RestoreGameData(fileData);
            _gameLoading = false;
        }
        else
        {
            // Open Quest 2.x saved game file

            _currentRoom = lines[3];

            // Start at line 5 as line 4 is always "!c"
            var lineNumber = 5;

            var data = "";
            string name;
            int scp;
            do
            {
                data = lines[lineNumber];
                lineNumber += 1;
                if (data != "!i")
                {
                    scp = Strings.InStr(data, ";");
                    name = Strings.Trim(Strings.Left(data, scp - 1));
                    var cdat = Conversions.ToInteger(Strings.Right(data, Strings.Len(data) - scp));

                    for (int i = 1, loopTo = _numCollectables; i <= loopTo; i++)
                    {
                        if ((_collectables[i].Name ?? "") == (name ?? ""))
                        {
                            _collectables[i].Value = cdat;
                            i = _numCollectables;
                        }
                    }
                }
            } while (data != "!i");

            bool cdatb;
            do
            {
                data = lines[lineNumber];
                lineNumber += 1;
                if (data != "!o")
                {
                    scp = Strings.InStr(data, ";");
                    name = Strings.Trim(Strings.Left(data, scp - 1));
                    cdatb = IsYes(Strings.Right(data, Strings.Len(data) - scp));

                    for (int i = 1, loopTo1 = _numberItems; i <= loopTo1; i++)
                    {
                        if ((_items[i].Name ?? "") == (name ?? ""))
                        {
                            _items[i].Got = cdatb;
                            i = _numberItems;
                        }
                    }
                }
            } while (data != "!o");

            bool visible;
            string room;
            int scp2;
            int scp3;
            do
            {
                data = lines[lineNumber];
                lineNumber += 1;
                if (data != "!p")
                {
                    scp = Strings.InStr(data, ";");
                    scp2 = Strings.InStr(scp + 1, data, ";");
                    scp3 = Strings.InStr(scp2 + 1, data, ";");

                    name = Strings.Trim(Strings.Left(data, scp - 1));
                    cdatb = IsYes(Strings.Mid(data, scp + 1, scp2 - scp - 1));
                    visible = IsYes(Strings.Mid(data, scp2 + 1, scp3 - scp2 - 1));
                    room = Strings.Trim(Strings.Mid(data, scp3 + 1));

                    for (int i = 1, loopTo2 = _numberObjs; i <= loopTo2; i++)
                    {
                        if (((_objs[i].ObjectName ?? "") == (name ?? "")) & !_objs[i].Loaded)
                        {
                            _objs[i].Exists = cdatb;
                            _objs[i].Visible = visible;
                            _objs[i].ContainerRoom = room;
                            _objs[i].Loaded = true;
                            i = _numberObjs;
                        }
                    }
                }
            } while (data != "!p");

            do
            {
                data = lines[lineNumber];
                lineNumber += 1;
                if (data != "!s")
                {
                    scp = Strings.InStr(data, ";");
                    scp2 = Strings.InStr(scp + 1, data, ";");
                    scp3 = Strings.InStr(scp2 + 1, data, ";");

                    name = Strings.Trim(Strings.Left(data, scp - 1));
                    cdatb = IsYes(Strings.Mid(data, scp + 1, scp2 - scp - 1));
                    visible = IsYes(Strings.Mid(data, scp2 + 1, scp3 - scp2 - 1));
                    room = Strings.Trim(Strings.Mid(data, scp3 + 1));

                    for (int i = 1, loopTo3 = _numberChars; i <= loopTo3; i++)
                    {
                        if ((_chars[i].ObjectName ?? "") == (name ?? ""))
                        {
                            _chars[i].Exists = cdatb;
                            _chars[i].Visible = visible;
                            _chars[i].ContainerRoom = room;
                            i = _numberChars;
                        }
                    }
                }
            } while (data != "!s");

            do
            {
                data = lines[lineNumber];
                lineNumber += 1;
                if (data != "!n")
                {
                    scp = Strings.InStr(data, ";");
                    name = Strings.Trim(Strings.Left(data, scp - 1));
                    data = Strings.Right(data, Strings.Len(data) - scp);

                    await SetStringContents(name, data, _nullContext);
                }
            } while (data != "!n");

            do
            {
                data = lines[lineNumber];
                lineNumber += 1;
                if (data != "!e")
                {
                    scp = Strings.InStr(data, ";");
                    name = Strings.Trim(Strings.Left(data, scp - 1));
                    data = Strings.Right(data, Strings.Len(data) - scp);

                    await SetNumericVariableContents(name, Conversion.Val(data), _nullContext);
                }
            } while (data != "!e");
        }

        return true;
    }

    private async Task<bool> InitialiseGame(GameData gameData, bool fromQsg = false)
    {
        _loadedFromQsg = fromQsg;

        _changeLogRooms = new ChangeLog();
        _changeLogObjects = new ChangeLog();
        _changeLogRooms.AppliesToType = ChangeLog.AppliesTo.Room;
        _changeLogObjects.AppliesToType = ChangeLog.AppliesTo.Object;

        _outPutOn = true;
        _useAbbreviations = true;

        // TODO: ?
        // _gamePath = System.IO.Path.GetDirectoryName(filename) + "\"

        LogASLError("Opening file " + gameData.Filename + " on " + DateTime.Now, LogType.Init);

        // Parse file and find where the 'define' blocks are:
        if (!await ParseFile(gameData))
        {
            LogASLError("Unable to open file", LogType.Init);
            var err = "Unable to open " + gameData.Filename;

            if (!string.IsNullOrEmpty(_openErrorReport))
            {
                // Strip last vbcrlf
                _openErrorReport = Strings.Left(_openErrorReport, Strings.Len(_openErrorReport) - 2);
                err = err + ":" + Constants.vbCrLf + Constants.vbCrLf + _openErrorReport;
            }

            await Print("Error: " + err, _nullContext);
            return false;
        }

        // Check version
        DefineBlock gameBlock;
        gameBlock = GetDefineBlock("game");

        var aslVersion = "//";
        for (int i = gameBlock.StartLine + 1, loopTo = gameBlock.EndLine - 1; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[i], "asl-version "))
            {
                aslVersion = await GetParameter(_lines[i], _nullContext);
            }
        }

        if (aslVersion == "//")
        {
            LogASLError("File contains no version header.", LogType.WarningError);
        }
        else
        {
            ASLVersion = Conversions.ToInteger(aslVersion);

            var recognisedVersions =
                "/100/200/210/217/280/281/282/283/284/285/300/310/311/320/350/390/391/392/400/410/";

            if (Strings.InStr(recognisedVersions, "/" + aslVersion + "/") == 0)
            {
                LogASLError("Unrecognised ASL version number.", LogType.WarningError);
            }
        }

        _listVerbs.Add(ListType.ExitsList, new List<string>(new[] {"Go to"}));

        if ((ASLVersion >= 280) & (ASLVersion < 390))
        {
            _listVerbs.Add(ListType.ObjectsList,
                new List<string>(new[] {"Look at", "Examine", "Take", "Speak to"}));
            _listVerbs.Add(ListType.InventoryList,
                new List<string>(new[] {"Look at", "Examine", "Use", "Drop"}));
        }
        else
        {
            _listVerbs.Add(ListType.ObjectsList, new List<string>(new[] {"Look at", "Take", "Speak to"}));
            _listVerbs.Add(ListType.InventoryList, new List<string>(new[] {"Look at", "Use", "Drop"}));
        }

        // Get the name of the game:
        _gameName = await GetParameter(_lines[GetDefineBlock("game").StartLine], _nullContext);

        _player.UpdateGameName(_gameName);
        _player.Show("Panes");
        _player.Show("Location");
        _player.Show("Command");

        await SetUpGameObject();
        SetUpOptions();

        for (int i = GetDefineBlock("game").StartLine + 1, loopTo1 = GetDefineBlock("game").EndLine - 1;
             i <= loopTo1;
             i++)
        {
            if (BeginsWith(_lines[i], "beforesave "))
            {
                _beforeSaveScript = GetEverythingAfter(_lines[i], "beforesave ");
            }
            else if (BeginsWith(_lines[i], "onload "))
            {
                _onLoadScript = GetEverythingAfter(_lines[i], "onload ");
            }
        }

        SetDefaultPlayerErrorMessages();

        await SetUpSynonyms();
        await SetUpRoomData();

        if (ASLVersion >= 410)
        {
            await SetUpExits();
        }

        if (ASLVersion < 280)
        {
            // Set up an array containing the names of all the items
            // used in the game, based on the possitems statement
            // of the 'define game' block.
            await SetUpItemArrays();
        }

        if (ASLVersion < 280)
        {
            // Now, go through the 'startitems' statement and set up
            // the items array so we start with those items mentioned.
            await SetUpStartItems();
        }

        // Set up collectables.
        await SetUpCollectables();

        await SetUpDisplayVariables();

        // Set up characters and objects.
        await SetUpCharObjectInfo();
        await SetUpUserDefinedPlayerErrors();
        await SetUpDefaultFonts();
        await SetUpTurnScript();
        await SetUpTimers();
        await SetUpMenus();

        LogASLError("Finished loading file.", LogType.Init);

        _defaultRoomProperties = await GetPropertiesInType("defaultroom", false);
        _defaultProperties = await GetPropertiesInType("default", false);

        return true;
    }

    private void ReadCatalog(string data)
    {
        var nullPos = Strings.InStr(data, "\0");
        _numResources = Conversions.ToInteger(DecryptString(Strings.Left(data, nullPos - 1)));
        Array.Resize(ref _resources, _numResources + 1);

        data = Strings.Mid(data, nullPos + 1);

        var resourceStart = 0;

        for (int i = 1, loopTo = _numResources; i <= loopTo; i++)
        {
            _resources[i] = new ResourceType();
            var r = _resources[i];
            nullPos = Strings.InStr(data, "\0");
            r.ResourceName = DecryptString(Strings.Left(data, nullPos - 1));
            data = Strings.Mid(data, nullPos + 1);

            nullPos = Strings.InStr(data, "\0");
            r.ResourceLength = Conversions.ToInteger(DecryptString(Strings.Left(data, nullPos - 1)));
            data = Strings.Mid(data, nullPos + 1);

            r.ResourceStart = resourceStart;
            resourceStart = resourceStart + r.ResourceLength;
        }
    }

    private Stream GetResourceStreamInternal(string filename)
    {
        return _hasResources ? ExtractFile(filename) : _gameData.GetAdjacentFile(filename);
    }

    private string[] GetLibraryLines(string libName)
    {
        byte[] libCode = null;
        libName = Strings.LCase(libName);

        switch (libName ?? "")
        {
            case "stdverbs.lib":
            {
                libCode = Resources.GetResourceBytes(Resources.stdverbs);
                break;
            }
            case "standard.lib":
            {
                libCode = Resources.GetResourceBytes(Resources.standard);
                break;
            }
            case "q3ext.qlb":
            {
                libCode = Resources.GetResourceBytes(Resources.q3ext);
                break;
            }
            case "typelib.qlb":
            {
                libCode = Resources.GetResourceBytes(Resources.Typelib);
                break;
            }
            case "net.lib":
            {
                libCode = Resources.GetResourceBytes(Resources.net);
                break;
            }
        }

        if (libCode is null)
        {
            return null;
        }

        return GetResourceLines(libCode);
    }

    private byte[] GetResourcelessCAS()
    {
        throw new NotImplementedException();
        // var fileData = File.ReadAllText(_resourceFile, Encoding.GetEncoding(1252));
        // return Encoding.GetEncoding(1252).GetBytes(Strings.Left(fileData, _startCatPos - 1));
    }
}
