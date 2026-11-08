using System.Diagnostics;
using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Constants = Microsoft.VisualBasic.Constants;
using Strings = Microsoft.VisualBasic.Strings;

namespace QuestViva.Legacy;

public partial class V4Game
{
    private string GetAfterParameter(string s)
    {
        // Returns everything after the end of the first parameter
        // in a string, i.e. for "use <thing> do <myproc>" it
        // returns "do <myproc>"
        int eop;
        eop = Strings.InStr(s, ">");

        if ((eop == 0) | (eop + 1 > Strings.Len(s)))
        {
            return "";
        }

        return Strings.Trim(Strings.Mid(s, eop + 1));
    }

    private string YesNo(bool yn)
    {
        if (yn)
        {
            return "Yes";
        }

        return "No";
    }

    private bool IsYes(string yn)
    {
        if (Strings.LCase(yn) == "yes")
        {
            return true;
        }

        return false;
    }

    internal bool BeginsWith(string s, string text)
    {
        // Compares the beginning of the line with a given
        // string. Case insensitive.

        // Example: beginswith("Hello there","HeLlO")=TRUE

        return (Strings.Left(Strings.LTrim(Strings.LCase(s)), Strings.Len(text)) ?? "") == (Strings.LCase(text) ?? "");
    }

    private DefineBlock GetDefineBlock(string blockname)
    {
        // Returns the start and end points of a named block.
        // Returns 0 if block not found.

        string l, blockType;

        var result = new DefineBlock();
        result.StartLine = 0;
        result.EndLine = 0;

        for (int i = 1, loopTo = _numberSections; i <= loopTo; i++)
        {
            // Get the first line of the define section:
            l = _lines[_defineBlocks[i].StartLine];

            // Now, starting from the first word after 'define',
            // retrieve the next word and compare it to blockname:

            // Add a space for define blocks with no parameter
            if (Strings.InStr(8, l, " ") == 0)
            {
                l = l + " ";
            }

            blockType = Strings.Mid(l, 8, Strings.InStr(8, l, " ") - 8);

            if ((blockType ?? "") == (blockname ?? ""))
            {
                // Return the start and end points
                result.StartLine = _defineBlocks[i].StartLine;
                result.EndLine = _defineBlocks[i].EndLine;
                return result;
            }
        }

        return result;
    }

    private async Task<DefineBlock> DefineBlockParam(string blockname, string param)
    {
        // Returns the start and end points of a named block

        Dictionary<string, string> cache;
        var result = new DefineBlock();

        param = "k" + param; // protect against numeric block names

        if (!_defineBlockParams.ContainsKey(blockname))
        {
            // Lazily create cache of define block params

            cache = new Dictionary<string, string>();
            _defineBlockParams.Add(blockname, cache);

            for (int i = 1, loopTo = _numberSections; i <= loopTo; i++)
            {
                // get the word after "define", e.g. "procedure"
                var blockType = GetEverythingAfter(_lines[_defineBlocks[i].StartLine], "define ");
                var sp = Strings.InStr(blockType, " ");
                if (sp != 0)
                {
                    blockType = Strings.Trim(Strings.Left(blockType, sp - 1));
                }

                if ((blockType ?? "") == (blockname ?? ""))
                {
                    var blockKey = await GetParameter(_lines[_defineBlocks[i].StartLine], _nullContext, false);

                    blockKey = "k" + blockKey;

                    if (!cache.ContainsKey(blockKey))
                    {
                        cache.Add(blockKey, _defineBlocks[i].StartLine + "," + _defineBlocks[i].EndLine);
                    }
                    // silently ignore duplicates
                }
            }
        }
        else
        {
            cache = _defineBlockParams[blockname];
        }

        if (cache.ContainsKey(param))
        {
            var blocks = Strings.Split(cache[param], ",");
            result.StartLine = Conversions.ToInteger(blocks[0]);
            result.EndLine = Conversions.ToInteger(blocks[1]);
        }

        return result;
    }

    internal string GetEverythingAfter(string s, string text)
    {
        if (Strings.Len(text) > Strings.Len(s))
        {
            return "";
        }

        return Strings.Right(s, Strings.Len(s) - Strings.Len(text));
    }

    internal async Task<string> GetParameter(string s, Context ctx, bool convertStringVariables = true)
    {
        // Returns the parameters between < and > in a string
        string newParam;
        int startPos;
        int endPos;

        startPos = Strings.InStr(s, "<");
        endPos = Strings.InStr(s, ">");

        if ((startPos == 0) | (endPos == 0))
        {
            LogASLError("Expected parameter in '" + ReportErrorLine(s) + "'", LogType.WarningError);
            return "";
        }

        var retrParam = Strings.Mid(s, startPos + 1, endPos - startPos - 1);

        if (convertStringVariables)
        {
            if (ASLVersion >= 320)
            {
                newParam = await ConvertParameter(
                    await ConvertParameter(await ConvertParameter(retrParam, "#", ConvertType.Strings, ctx), "%",
                        ConvertType.Numeric, ctx), "$", ConvertType.Functions, ctx);
            }
            else if (!(Strings.Left(retrParam, 9) == "~Internal"))
            {
                newParam = await ConvertParameter(
                    await ConvertParameter(
                        await ConvertParameter(await ConvertParameter(retrParam, "#", ConvertType.Strings, ctx), "%",
                            ConvertType.Numeric, ctx), "~", ConvertType.Collectables, ctx), "$", ConvertType.Functions,
                    ctx);
            }
            else
            {
                newParam = retrParam;
            }
        }
        else
        {
            newParam = retrParam;
        }

        return await EvaluateInlineExpressions(newParam);
    }

    private string CapFirst(string s)
    {
        return Strings.UCase(Strings.Left(s, 1)) + Strings.Mid(s, 2);
    }

    private async Task<TextAction> GetTextOrScript(string textScript)
    {
        var result = new TextAction();
        textScript = Strings.Trim(textScript);

        if (Strings.Left(textScript, 1) == "<")
        {
            result.Type = TextActionType.Text;
            result.Data = await GetParameter(textScript, _nullContext);
        }
        else
        {
            result.Type = TextActionType.Script;
            result.Data = textScript;
        }

        return result;
    }

    private async Task<string> FindStatement(DefineBlock block, string statement)
    {
        // Finds a statement within a given block of lines

        for (int i = block.StartLine + 1, loopTo = block.EndLine - 1; i <= loopTo; i++)
        {
            // Ignore sub-define blocks
            if (BeginsWith(_lines[i], "define "))
            {
                do
                {
                    i = i + 1;
                } while (Strings.Trim(_lines[i]) != "end define");
            }

            // Check to see if the line matches the statement
            // that is begin searched for
            if (BeginsWith(_lines[i], statement))
            {
                // Return the parameters between < and > :
                return await GetParameter(_lines[i], _nullContext);
            }
        }

        return "";
    }

    private async Task<string> FindLine(DefineBlock block, string statement, string statementParam)
    {
        // Finds a statement within a given block of lines

        for (int i = block.StartLine + 1, loopTo = block.EndLine - 1; i <= loopTo; i++)
        {
            // Ignore sub-define blocks
            if (BeginsWith(_lines[i], "define "))
            {
                do
                {
                    i = i + 1;
                } while (Strings.Trim(_lines[i]) != "end define");
            }

            // Check to see if the line matches the statement
            // that is begin searched for
            if (BeginsWith(_lines[i], statement))
            {
                if ((Strings.UCase(Strings.Trim(await GetParameter(_lines[i], _nullContext))) ?? "") ==
                    (Strings.UCase(Strings.Trim(statementParam)) ?? ""))
                {
                    return Strings.Trim(_lines[i]);
                }
            }
        }

        return "";
    }

    private string GetSecondChunk(string line)
    {
        var endOfFirstBit = Strings.InStr(line, ">") + 1;
        var lengthOfKeyword = Strings.Len(line) - endOfFirstBit + 1;
        return Strings.Trim(Strings.Mid(line, endOfFirstBit, lengthOfKeyword));
    }

    private string RetrLine(string blockType, string param, string line, Context ctx)
    {
        DefineBlock searchblock;

        if (blockType == "object")
        {
            searchblock = GetThingBlock(param, _currentRoom, Thing.Object);
        }
        else
        {
            searchblock = GetThingBlock(param, _currentRoom, Thing.Character);
        }

        if ((searchblock.StartLine == 0) & (searchblock.EndLine == 0))
        {
            return "<undefined>";
        }

        for (int i = searchblock.StartLine + 1, loopTo = searchblock.EndLine - 1; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[i], line))
            {
                return Strings.Trim(_lines[i]);
            }
        }

        return "<unfound>";
    }

    private async Task<string> RetrLineParam(string blockType, string param, string line, string lineParam, Context ctx)
    {
        DefineBlock searchblock;

        if (blockType == "object")
        {
            searchblock = GetThingBlock(param, _currentRoom, Thing.Object);
        }
        else
        {
            searchblock = GetThingBlock(param, _currentRoom, Thing.Character);
        }

        if ((searchblock.StartLine == 0) & (searchblock.EndLine == 0))
        {
            return "<undefined>";
        }

        for (int i = searchblock.StartLine + 1, loopTo = searchblock.EndLine - 1; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[i], line) &&
                (Strings.LCase(await GetParameter(_lines[i], ctx)) ?? "") == (Strings.LCase(lineParam) ?? ""))
            {
                return Strings.Trim(_lines[i]);
            }
        }

        return "<unfound>";
    }
}
