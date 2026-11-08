using System.Diagnostics;
using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using QuestViva.Common;
using Strings = Microsoft.VisualBasic.Strings;

namespace QuestViva.Legacy;

public partial class V4Game
{
    private async Task<bool> ExecVerb(string cmd, Context ctx, bool libCommands = false)
    {
        DefineBlock gameBlock;
        var foundVerb = false;
        var verbProperty = "";
        var script = "";
        string verbsList;
        var thisVerb = "";
        int scp;
        int id;
        var verbObject = "";
        string verbTag;
        var thisScript = "";

        if (!libCommands)
        {
            verbTag = "verb ";
        }
        else
        {
            verbTag = "lib verb ";
        }

        gameBlock = GetDefineBlock("game");
        for (int i = gameBlock.StartLine + 1, loopTo = gameBlock.EndLine - 1; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[i], verbTag))
            {
                verbsList = await GetParameter(_lines[i], ctx);

                // The property or action the verb uses is either after a colon,
                // or it's the first (or only) verb on the line.
                var colonPos = Strings.InStr(verbsList, ":");
                if (colonPos != 0)
                {
                    verbProperty = Strings.LCase(Strings.Trim(Strings.Mid(verbsList, colonPos + 1)));
                    verbsList = Strings.Trim(Strings.Left(verbsList, colonPos - 1));
                }
                else
                {
                    scp = Strings.InStr(verbsList, ";");
                    if (scp == 0)
                    {
                        verbProperty = Strings.LCase(verbsList);
                    }
                    else
                    {
                        verbProperty = Strings.LCase(Strings.Trim(Strings.Left(verbsList, scp - 1)));
                    }
                }

                // Now let's see if this matches:
                do
                {
                    scp = Strings.InStr(verbsList, ";");
                    if (scp == 0)
                    {
                        thisVerb = Strings.LCase(verbsList);
                    }
                    else
                    {
                        thisVerb = Strings.LCase(Strings.Trim(Strings.Left(verbsList, scp - 1)));
                    }

                    if (BeginsWith(cmd, thisVerb + " "))
                    {
                        foundVerb = true;
                        verbObject = GetEverythingAfter(cmd, thisVerb + " ");
                        script = Strings.Trim(Strings.Mid(_lines[i], Strings.InStr(_lines[i], ">") + 1));
                    }

                    if (scp != 0)
                    {
                        verbsList = Strings.Trim(Strings.Mid(verbsList, scp + 1));
                    }
                } while (!((scp == 0) | string.IsNullOrEmpty(Strings.Trim(verbsList)) | foundVerb));

                if (foundVerb)
                {
                    break;
                }
            }
        }

        if (foundVerb)
        {
            id = await Disambiguate(verbObject, "inventory;" + _currentRoom, ctx);

            if (id < 0)
            {
                if (id != -2)
                {
                    await PlayerErrorMessage(PlayerError.BadThing, ctx);
                }

                _badCmdBefore = thisVerb;
            }
            else
            {
                await SetStringContents("quest.error.article", _objs[id].Article, ctx);

                var foundAction = false;

                // Now see if this object has the relevant action or property
                var o = _objs[id];
                for (int i = 1, loopTo1 = o.NumberActions; i <= loopTo1; i++)
                {
                    if ((Strings.LCase(o.Actions[i].ActionName) ?? "") == (verbProperty ?? ""))
                    {
                        foundAction = true;
                        thisScript = o.Actions[i].Script;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(thisScript))
                    // Avoid an RTE "this array is fixed or temporarily locked"
                {
                    await ExecuteScript(thisScript, ctx, id);
                }

                if (!foundAction)
                    // Check properties for a message
                {
                    for (int i = 1, loopTo2 = o.NumberProperties; i <= loopTo2; i++)
                    {
                        if ((Strings.LCase(o.Properties[i].PropertyName) ?? "") == (verbProperty ?? ""))
                        {
                            foundAction = true;
                            await Print(o.Properties[i].PropertyValue, ctx);
                            break;
                        }
                    }
                }

                if (!foundAction)
                    // Execute the default script from the verb definition
                {
                    await ExecuteScript(script, ctx);
                }
            }
        }

        return foundVerb;
    }

    private bool DisambObjHere(Context ctx, int id, string firstPlace, bool twoPlaces = false, string secondPlace = "",
        bool isExit = false)
    {
        var isSeen = default(bool);
        var onlySeen = false;

        if (firstPlace == "game")
        {
            firstPlace = "";
            if (secondPlace == "seen")
            {
                twoPlaces = false;
                secondPlace = "";
                onlySeen = true;
                var roomObjId = _rooms[GetRoomID(_objs[id].ContainerRoom, ctx)].ObjId;

                if (_objs[id].ContainerRoom == "inventory")
                {
                    isSeen = true;
                }
                else if (IsYes(GetObjectProperty("visited", roomObjId, true, false)))
                {
                    isSeen = true;
                }
                else if (IsYes(GetObjectProperty("seen", id, true, false)))
                {
                    isSeen = true;
                }
            }
        }

        if (((!twoPlaces &
              (((Strings.LCase(_objs[id].ContainerRoom) ?? "") == (Strings.LCase(firstPlace) ?? "")) |
               string.IsNullOrEmpty(firstPlace))) |
             (twoPlaces & (((Strings.LCase(_objs[id].ContainerRoom) ?? "") == (Strings.LCase(firstPlace) ?? "")) |
                           ((Strings.LCase(_objs[id].ContainerRoom) ?? "") == (Strings.LCase(secondPlace) ?? ""))))) &
            _objs[id].Exists & (_objs[id].IsExit == isExit))
        {
            if (!onlySeen)
            {
                return true;
            }

            return isSeen;
        }

        return false;
    }

    private async Task ExecOops(string correction, Context ctx)
    {
        if (!string.IsNullOrEmpty(_badCmdBefore))
        {
            if (string.IsNullOrEmpty(_badCmdAfter))
            {
                await ExecCommand(_badCmdBefore + " " + correction, ctx, false);
            }
            else
            {
                await ExecCommand(_badCmdBefore + " " + correction + " " + _badCmdAfter, ctx, false);
            }
        }
    }

    internal async Task<int> Disambiguate(string name, string containedIn, Context ctx, bool isExit = false)
    {
        // Returns object ID being referred to by player.
        // Returns -1 if object doesn't exist, calling function
        // then expected to print relevant error.
        // Returns -2 if "it" meaningless, prints own error.
        // If it returns an object ID, it also sets quest.lastobject to the name
        // of the object referred to.
        // If ctx.AllowRealNamesInCommand is True, will allow an object's real
        // name to be used even when the object has an alias - this is used when
        // Disambiguate has been called after an "exec" command to prevent the
        // player having to choose an object from the disambiguation menu twice

        var numberCorresIds = 0;
        var idNumbers = new int[1];
        string firstPlace;
        var secondPlace = "";
        bool twoPlaces;
        string[] descriptionText;
        string[] validNames;
        int numValidNames;

        name = Strings.Trim(name);

        await SetStringContents("quest.lastobject", "", ctx);

        if (Strings.InStr(containedIn, ";") != 0)
        {
            var scp = Strings.InStr(containedIn, ";");
            twoPlaces = true;
            firstPlace = Strings.Trim(Strings.Left(containedIn, scp - 1));
            secondPlace = Strings.Trim(Strings.Mid(containedIn, scp + 1));
        }
        else
        {
            twoPlaces = false;
            firstPlace = containedIn;
        }

        if (ctx.AllowRealNamesInCommand)
        {
            for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
            {
                if (DisambObjHere(ctx, i, firstPlace, twoPlaces, secondPlace))
                {
                    if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(name) ?? ""))
                    {
                        await SetStringContents("quest.lastobject", _objs[i].ObjectName, ctx);
                        return i;
                    }
                }
            }
        }

        // If player uses "it", "them" etc. as name:
        if ((name == "it") | (name == "them") | (name == "this") | (name == "those") | (name == "these") |
            (name == "that"))
        {
            await SetStringContents("quest.error.pronoun", name, ctx);
            if ((_lastIt != 0) & (_lastItMode == ItType.Inanimate) &
                DisambObjHere(ctx, _lastIt, firstPlace, twoPlaces, secondPlace))
            {
                await SetStringContents("quest.lastobject", _objs[_lastIt].ObjectName, ctx);
                return _lastIt;
            }

            await PlayerErrorMessage(PlayerError.BadPronoun, ctx);
            return -2;
        }

        if (name == "him")
        {
            await SetStringContents("quest.error.pronoun", name, ctx);
            if ((_lastIt != 0) & (_lastItMode == ItType.Male) &
                DisambObjHere(ctx, _lastIt, firstPlace, twoPlaces, secondPlace))
            {
                await SetStringContents("quest.lastobject", _objs[_lastIt].ObjectName, ctx);
                return _lastIt;
            }

            await PlayerErrorMessage(PlayerError.BadPronoun, ctx);
            return -2;
        }

        if (name == "her")
        {
            await SetStringContents("quest.error.pronoun", name, ctx);
            if ((_lastIt != 0) & (_lastItMode == ItType.Female) &
                DisambObjHere(ctx, _lastIt, firstPlace, twoPlaces, secondPlace))
            {
                await SetStringContents("quest.lastobject", _objs[_lastIt].ObjectName, ctx);
                return _lastIt;
            }

            await PlayerErrorMessage(PlayerError.BadPronoun, ctx);
            return -2;
        }

        _thisTurnIt = 0;

        if (BeginsWith(name, "the "))
        {
            name = GetEverythingAfter(name, "the ");
        }

        for (int i = 1, loopTo1 = _numberObjs; i <= loopTo1; i++)
        {
            if (DisambObjHere(ctx, i, firstPlace, twoPlaces, secondPlace, isExit))
            {
                numValidNames = _objs[i].NumberAltNames + 1;
                validNames = new string[numValidNames + 1];
                validNames[1] = _objs[i].ObjectAlias;
                for (int j = 1, loopTo2 = _objs[i].NumberAltNames; j <= loopTo2; j++)
                {
                    validNames[j + 1] = _objs[i].AltNames[j];
                }

                for (int j = 1, loopTo3 = numValidNames; j <= loopTo3; j++)
                {
                    if (((Strings.LCase(validNames[j]) ?? "") == (Strings.LCase(name) ?? "")) |
                        (("the " + Strings.LCase(name) ?? "") == (Strings.LCase(validNames[j]) ?? "")))
                    {
                        numberCorresIds = numberCorresIds + 1;
                        Array.Resize(ref idNumbers, numberCorresIds + 1);
                        idNumbers[numberCorresIds] = i;
                        j = numValidNames;
                    }
                }
            }
        }

        if ((ASLVersion >= 391) & (numberCorresIds == 0) & _useAbbreviations & (Strings.Len(name) > 0))
            // Check for abbreviated object names
        {
            for (int i = 1, loopTo4 = _numberObjs; i <= loopTo4; i++)
            {
                if (DisambObjHere(ctx, i, firstPlace, twoPlaces, secondPlace, isExit))
                {
                    string thisName;
                    if (!string.IsNullOrEmpty(_objs[i].ObjectAlias))
                    {
                        thisName = Strings.LCase(_objs[i].ObjectAlias);
                    }
                    else
                    {
                        thisName = Strings.LCase(_objs[i].ObjectName);
                    }

                    if (ASLVersion >= 410)
                    {
                        if (!string.IsNullOrEmpty(_objs[i].Prefix))
                        {
                            thisName = Strings.Trim(Strings.LCase(_objs[i].Prefix)) + " " + thisName;
                        }

                        if (!string.IsNullOrEmpty(_objs[i].Suffix))
                        {
                            thisName = thisName + " " + Strings.Trim(Strings.LCase(_objs[i].Suffix));
                        }
                    }

                    if (Strings.InStr(" " + thisName, " " + Strings.LCase(name)) != 0)
                    {
                        numberCorresIds = numberCorresIds + 1;
                        Array.Resize(ref idNumbers, numberCorresIds + 1);
                        idNumbers[numberCorresIds] = i;
                    }
                }
            }
        }

        if (numberCorresIds == 1)
        {
            await SetStringContents("quest.lastobject", _objs[idNumbers[1]].ObjectName, ctx);
            _thisTurnIt = idNumbers[1];

            switch (_objs[idNumbers[1]].Article ?? "")
            {
                case "him":
                {
                    _thisTurnItMode = ItType.Male;
                    break;
                }
                case "her":
                {
                    _thisTurnItMode = ItType.Female;
                    break;
                }

                default:
                {
                    _thisTurnItMode = ItType.Inanimate;
                    break;
                }
            }

            return idNumbers[1];
        }

        if (numberCorresIds > 1)
        {
            descriptionText = new string[numberCorresIds + 1];

            var question = "Please select which " + name + " you mean:";
            await Print("- |i" + question + "|xi", ctx);

            var menuItems = new Dictionary<string, string>();

            for (int i = 1, loopTo5 = numberCorresIds; i <= loopTo5; i++)
            {
                descriptionText[i] = _objs[idNumbers[i]].Detail;
                if (string.IsNullOrEmpty(descriptionText[i]))
                {
                    if (string.IsNullOrEmpty(_objs[idNumbers[i]].Prefix))
                    {
                        descriptionText[i] = _objs[idNumbers[i]].ObjectAlias;
                    }
                    else
                    {
                        descriptionText[i] = _objs[idNumbers[i]].Prefix + _objs[idNumbers[i]].ObjectAlias;
                    }
                }

                menuItems.Add(i.ToString(), descriptionText[i]);
            }

            var mnu = new MenuData(question, menuItems, false);
            var response = await ShowMenuAsync(mnu);

            _choiceNumber = Conversions.ToInteger(response);

            await SetStringContents("quest.lastobject", _objs[idNumbers[_choiceNumber]].ObjectName, ctx);

            _thisTurnIt = idNumbers[_choiceNumber];

            switch (_objs[idNumbers[_choiceNumber]].Article ?? "")
            {
                case "him":
                {
                    _thisTurnItMode = ItType.Male;
                    break;
                }
                case "her":
                {
                    _thisTurnItMode = ItType.Female;
                    break;
                }

                default:
                {
                    _thisTurnItMode = ItType.Inanimate;
                    break;
                }
            }

            await Print("- " + descriptionText[_choiceNumber] + "|n", ctx);

            return idNumbers[_choiceNumber];
        }

        _thisTurnIt = _lastIt;
        await SetStringContents("quest.error.object", name, ctx);
        return -1;
    }

    internal async Task PlayerErrorMessage(PlayerError e, Context ctx)
    {
        await Print(await GetErrorMessage(e, ctx), ctx);
    }

    private async Task PlayerErrorMessage_ExtendInfo(PlayerError e, Context ctx, string extraInfo)
    {
        var message = await GetErrorMessage(e, ctx);

        if (!string.IsNullOrEmpty(extraInfo))
        {
            if (Strings.Right(message, 1) == ".")
            {
                message = Strings.Left(message, Strings.Len(message) - 1);
            }

            message = message + " - " + extraInfo + ".";
        }

        await Print(message, ctx);
    }

    private async Task<string> GetErrorMessage(PlayerError e, Context ctx)
    {
        return await ConvertParameter(
            await ConvertParameter(await ConvertParameter(_playerErrorMessageString[(int) e], "%", ConvertType.Numeric, ctx), "$",
                ConvertType.Functions, ctx), "#", ConvertType.Strings, ctx);
    }

    private async Task<bool> ExecUserCommand(string cmd, Context ctx, bool libCommands = false)
    {
        // Executes a user-defined command. If unavailable, returns
        // false.
        string curCmd, commandList;
        var script = "";
        string commandTag;
        var commandLine = "";
        var foundCommand = false;

        // First, check for a command in the current room block
        var roomId = GetRoomID(_currentRoom, ctx);

        // RoomID is 0 if we have no rooms in the game. Unlikely, but we get an RTE otherwise.
        if (roomId != 0)
        {
            var r = _rooms[roomId];
            for (int i = 1, loopTo = r.NumberCommands; i <= loopTo; i++)
            {
                commandList = r.Commands[i].CommandText;
                int ep;
                var exitFor = false;
                do
                {
                    ep = Strings.InStr(commandList, ";");
                    if (ep == 0)
                    {
                        curCmd = commandList;
                    }
                    else
                    {
                        curCmd = Strings.Trim(Strings.Left(commandList, ep - 1));
                        commandList = Strings.Trim(Strings.Mid(commandList, ep + 1));
                    }

                    if (IsCompatible(Strings.LCase(cmd), Strings.LCase(curCmd)))
                    {
                        commandLine = curCmd;
                        script = r.Commands[i].CommandScript;
                        foundCommand = true;
                        ep = 0;
                        exitFor = true;
                        break;
                    }
                } while (ep != 0);

                if (exitFor)
                {
                    break;
                }
            }
        }

        if (!libCommands)
        {
            commandTag = "command";
        }
        else
        {
            commandTag = "lib command";
        }

        if (!foundCommand)
        {
            // Check "define game" block
            var block = GetDefineBlock("game");
            for (int i = block.StartLine + 1, loopTo1 = block.EndLine - 1; i <= loopTo1; i++)
            {
                if (BeginsWith(_lines[i], commandTag))
                {
                    commandList = await GetParameter(_lines[i], ctx, false);
                    int ep;
                    var exitFor1 = false;
                    do
                    {
                        ep = Strings.InStr(commandList, ";");
                        if (ep == 0)
                        {
                            curCmd = commandList;
                        }
                        else
                        {
                            curCmd = Strings.Trim(Strings.Left(commandList, ep - 1));
                            commandList = Strings.Trim(Strings.Mid(commandList, ep + 1));
                        }

                        if (IsCompatible(Strings.LCase(cmd), Strings.LCase(curCmd)))
                        {
                            commandLine = curCmd;
                            var ScriptPos = Strings.InStr(_lines[i], ">") + 1;
                            script = Strings.Trim(Strings.Mid(_lines[i], ScriptPos));
                            foundCommand = true;
                            ep = 0;
                            exitFor1 = true;
                            break;
                        }
                    } while (ep != 0);

                    if (exitFor1)
                    {
                        break;
                    }
                }
            }
        }

        if (foundCommand)
        {
            if (await GetCommandParameters(cmd, commandLine, ctx))
            {
                await ExecuteScript(script, ctx);
            }
        }

        return foundCommand;
    }

    private async Task<bool> GetCommandParameters(string test, string required, Context ctx)
    {
        // Gets parameters from line. For example, if 'required'
        // is "read #1#" and 'test' is "read sign", #1# returns
        // "sign".

        // Returns FALSE if #@object# form used and object doesn't
        // exist.

        var chunksBegin = default(int[]);
        var chunksEnd = default(int[]);
        var varName = default(string[]);
        var var2Pos = default(int);

        // Add dots before and after both strings. This fudge
        // stops problems caused when variables are right at the
        // beginning or end of a line.
        // PostScript: well, it used to, I'm not sure if it's really
        // required now though....
        // As of Quest 4.0 we use the ¦ character rather than a dot.
        test = "¦" + Strings.Trim(test) + "¦";
        required = "¦" + required + "¦";

        // Go through RequiredLine in chunks going up to variables.
        var currentReqLinePos = 1;
        var currentTestLinePos = 1;
        var finished = false;
        var numberChunks = 0;
        do
        {
            var nextVarPos = Strings.InStr(currentReqLinePos, required, "#");
            var currentVariable = "";

            if (nextVarPos == 0)
            {
                finished = true;
                nextVarPos = Strings.Len(required) + 1;
            }
            else
            {
                var2Pos = Strings.InStr(nextVarPos + 1, required, "#");
                currentVariable = Strings.Mid(required, nextVarPos + 1, var2Pos - 1 - nextVarPos);
            }

            var checkChunk = Strings.Mid(required, currentReqLinePos, nextVarPos - 1 - (currentReqLinePos - 1));
            var chunkBegin = Strings.InStr(currentTestLinePos, Strings.LCase(test), Strings.LCase(checkChunk));
            var chunkEnd = chunkBegin + Strings.Len(checkChunk);

            numberChunks = numberChunks + 1;
            Array.Resize(ref chunksBegin, numberChunks + 1);
            Array.Resize(ref chunksEnd, numberChunks + 1);
            Array.Resize(ref varName, numberChunks + 1);
            chunksBegin[numberChunks] = chunkBegin;
            chunksEnd[numberChunks] = chunkEnd;
            varName[numberChunks] = currentVariable;

            // Get to end of variable name
            currentReqLinePos = var2Pos + 1;

            currentTestLinePos = chunkEnd;
        } while (!finished);

        var success = true;

        // Return values to string variable
        for (int i = 1, loopTo = numberChunks - 1; i <= loopTo; i++)
        {
            int arrayIndex;
            // If VarName contains array name, change to index number
            if (Strings.InStr(varName[i], "[") > 0)
            {
                var indexResult = GetArrayIndex(varName[i], ctx);
                varName[i] = indexResult.Name;
                arrayIndex = indexResult.Index;
            }
            else
            {
                arrayIndex = 0;
            }

            var curChunk = Strings.Mid(test, chunksEnd[i], chunksBegin[i + 1] - chunksEnd[i]);

            if (BeginsWith(varName[i], "@"))
            {
                varName[i] = GetEverythingAfter(varName[i], "@");
                var id = await Disambiguate(curChunk, _currentRoom + ";" + "inventory", ctx);

                if (id == -1)
                {
                    if (ASLVersion >= 391)
                    {
                        await PlayerErrorMessage(PlayerError.BadThing, ctx);
                    }
                    else
                    {
                        await PlayerErrorMessage(PlayerError.BadItem, ctx);
                    }

                    // The Mid$(...,2) and Left$(...,2) removes the initial/final "."
                    _badCmdBefore = Strings.Mid(Strings.Trim(Strings.Left(test, chunksEnd[i] - 1)), 2);
                    _badCmdAfter = Strings.Trim(Strings.Mid(test, chunksBegin[i + 1]));
                    _badCmdAfter = Strings.Left(_badCmdAfter, Strings.Len(_badCmdAfter) - 1);
                    success = false;
                }
                else if (id == -2)
                {
                    _badCmdBefore = Strings.Trim(Strings.Left(test, chunksEnd[i] - 1));
                    _badCmdAfter = Strings.Trim(Strings.Mid(test, chunksBegin[i + 1]));
                    success = false;
                }
                else
                {
                    await SetStringContents(varName[i], _objs[id].ObjectName, ctx, arrayIndex);
                }
            }
            else
            {
                await SetStringContents(varName[i], curChunk, ctx, arrayIndex);
            }
        }

        return success;
    }

    private bool IsCompatible(string test, string required)
    {
        // Tests to see if 'test' "works" with 'required'.
        // For example, if 'required' = "read #text#", then the
        // tests of "read book" and "read sign" are compatible.
        var var2Pos = default(int);

        // This avoids "xxx123" being compatible with "xxx".
        test = "^" + Strings.Trim(test) + "^";
        required = "^" + required + "^";

        // Go through RequiredLine in chunks going up to variables.
        var currentReqLinePos = 1;
        var currentTestLinePos = 1;
        var finished = false;
        do
        {
            var nextVarPos = Strings.InStr(currentReqLinePos, required, "#");
            if (nextVarPos == 0)
            {
                nextVarPos = Strings.Len(required) + 1;
                finished = true;
            }
            else
            {
                var2Pos = Strings.InStr(nextVarPos + 1, required, "#");
            }

            var checkChunk = Strings.Mid(required, currentReqLinePos, nextVarPos - 1 - (currentReqLinePos - 1));

            if (Strings.InStr(currentTestLinePos, test, checkChunk) != 0)
            {
                currentTestLinePos = Strings.InStr(currentTestLinePos, test, checkChunk) + Strings.Len(checkChunk);
            }
            else
            {
                return false;
            }

            // Skip to end of variable
            currentReqLinePos = var2Pos + 1;
        } while (!finished);

        return true;
    }

    // Returns true if the system is ready to process a new command after completion - so it will be
    // in most cases, except when ExecCommand just caused an "enter" script command to complete

    private async Task<bool> ExecCommand(string input, Context ctx, bool echo = true, bool runUserCommand = true,
        bool dontSetIt = false)
    {
        string parameter;
        var skipAfterTurn = false;
        input = RemoveFormatting(input);

        var oldBadCmdBefore = _badCmdBefore;

        var roomID = GetRoomID(_currentRoom, ctx);

        if (string.IsNullOrEmpty(input))
        {
            return true;
        }

        var cmd = Strings.LCase(input);

        if (_commandOverrideModeOn)
        {
            // Commands have been overridden for this command,
            // so put input into previously specified variable
            // and exit:

            await SetStringContents(_commandOverrideVariable, input, ctx);
            _commandTcs!.TrySetResult();
            return false;
        }

        bool userCommandReturn;

        if (echo)
        {
            await Print("> " + input, ctx);
        }

        input = Strings.LCase(input);

        await SetStringContents("quest.originalcommand", input, ctx);

        var newCommand = " " + input + " ";

        // Convert synonyms:
        for (int i = 1, loopTo = _numberSynonyms; i <= loopTo; i++)
        {
            var cp = 1;
            int n;
            do
            {
                n = Strings.InStr(cp, newCommand, " " + _synonyms[i].OriginalWord + " ");
                if (n != 0)
                {
                    newCommand = Strings.Left(newCommand, n - 1) + " " + _synonyms[i].ConvertTo + " " +
                                 Strings.Mid(newCommand, n + Strings.Len(_synonyms[i].OriginalWord) + 2);
                    cp = n + 1;
                }
            } while (n != 0);
        }

        // strip starting and ending spaces
        input = Strings.Mid(newCommand, 2, Strings.Len(newCommand) - 2);

        await SetStringContents("quest.command", input, ctx);

        // Execute any "beforeturn" script:

        var newCtx = CopyContext(ctx);
        var globalOverride = false;

        // RoomID is 0 if there are no rooms in the game. Unlikely, but we get an RTE otherwise.
        if (roomID != 0)
        {
            if (!string.IsNullOrEmpty(_rooms[roomID].BeforeTurnScript))
            {
                if (BeginsWith(_rooms[roomID].BeforeTurnScript, "override"))
                {
                    await ExecuteScript(GetEverythingAfter(_rooms[roomID].BeforeTurnScript, "override"), newCtx);
                    globalOverride = true;
                }
                else
                {
                    await ExecuteScript(_rooms[roomID].BeforeTurnScript, newCtx);
                }
            }
        }

        if (!string.IsNullOrEmpty(_beforeTurnScript) & !globalOverride)
        {
            await ExecuteScript(_beforeTurnScript, newCtx);
        }

        // In executing BeforeTurn script, "dontprocess" sets ctx.DontProcessCommand,
        // in which case we don't process the command.

        if (!newCtx.DontProcessCommand)
        {
            // Try to execute user defined command, if allowed:

            userCommandReturn = false;
            if (runUserCommand)
            {
                userCommandReturn = await ExecUserCommand(input, ctx);

                if (!userCommandReturn)
                {
                    userCommandReturn = await ExecVerb(input, ctx);
                }

                if (!userCommandReturn)
                {
                    // Try command defined by a library
                    userCommandReturn = await ExecUserCommand(input, ctx, true);
                }

                if (!userCommandReturn)
                {
                    // Try verb defined by a library
                    userCommandReturn = await ExecVerb(input, ctx, true);
                }
            }

            input = Strings.LCase(input);
        }
        else
        {
            // Set the UserCommand flag to fudge not processing any more commands
            userCommandReturn = true;
        }

        var invList = "";

        if (!userCommandReturn)
        {
            if (CmdStartsWith(input, "speak to "))
            {
                parameter = GetEverythingAfter(input, "speak to ");
                await ExecSpeak(parameter, ctx);
            }
            else if (CmdStartsWith(input, "talk to "))
            {
                parameter = GetEverythingAfter(input, "talk to ");
                await ExecSpeak(parameter, ctx);
            }
            else if ((cmd == "exit") | (cmd == "out") | (cmd == "leave"))
            {
                await GoDirection("out", ctx);
                _lastIt = 0;
            }
            else if ((cmd == "north") | (cmd == "south") | (cmd == "east") | (cmd == "west"))
            {
                await GoDirection(input, ctx);
                _lastIt = 0;
            }
            else if ((cmd == "n") | (cmd == "s") | (cmd == "w") | (cmd == "e"))
            {
                switch (Strings.InStr("nswe", cmd))
                {
                    case 1:
                    {
                        await GoDirection("north", ctx);
                        break;
                    }
                    case 2:
                    {
                        await GoDirection("south", ctx);
                        break;
                    }
                    case 3:
                    {
                        await GoDirection("west", ctx);
                        break;
                    }
                    case 4:
                    {
                        await GoDirection("east", ctx);
                        break;
                    }
                }

                _lastIt = 0;
            }
            else if ((cmd == "ne") | (cmd == "northeast") | (cmd == "north-east") | (cmd == "north east") |
                     (cmd == "go ne") |
                     (cmd == "go northeast") | (cmd == "go north-east") | (cmd == "go north east"))
            {
                await GoDirection("northeast", ctx);
                _lastIt = 0;
            }
            else if ((cmd == "nw") | (cmd == "northwest") | (cmd == "north-west") | (cmd == "north west") |
                     (cmd == "go nw") |
                     (cmd == "go northwest") | (cmd == "go north-west") | (cmd == "go north west"))
            {
                await GoDirection("northwest", ctx);
                _lastIt = 0;
            }
            else if ((cmd == "se") | (cmd == "southeast") | (cmd == "south-east") | (cmd == "south east") |
                     (cmd == "go se") |
                     (cmd == "go southeast") | (cmd == "go south-east") | (cmd == "go south east"))
            {
                await GoDirection("southeast", ctx);
                _lastIt = 0;
            }
            else if ((cmd == "sw") | (cmd == "southwest") | (cmd == "south-west") | (cmd == "south west") |
                     (cmd == "go sw") |
                     (cmd == "go southwest") | (cmd == "go south-west") | (cmd == "go south west"))
            {
                await GoDirection("southwest", ctx);
                _lastIt = 0;
            }
            else if ((cmd == "up") | (cmd == "u"))
            {
                await GoDirection("up", ctx);
                _lastIt = 0;
            }
            else if ((cmd == "down") | (cmd == "d"))
            {
                await GoDirection("down", ctx);
                _lastIt = 0;
            }
            else if (CmdStartsWith(input, "go "))
            {
                if (ASLVersion >= 410)
                {
                    await _rooms[GetRoomID(_currentRoom, ctx)].Exits.ExecuteGo(input, ctx);
                }
                else
                {
                    parameter = GetEverythingAfter(input, "go ");
                    if (parameter == "out")
                    {
                        await GoDirection("out", ctx);
                    }
                    else if ((parameter == "north") | (parameter == "south") | (parameter == "east") |
                             (parameter == "west") |
                             (parameter == "up") | (parameter == "down"))
                    {
                        await GoDirection(parameter, ctx);
                    }
                    else if (BeginsWith(parameter, "to "))
                    {
                        parameter = GetEverythingAfter(parameter, "to ");
                        await GoToPlace(parameter, ctx);
                    }
                    else
                    {
                        await PlayerErrorMessage(PlayerError.BadGo, ctx);
                    }
                }

                _lastIt = 0;
            }
            else if (CmdStartsWith(input, "give "))
            {
                parameter = GetEverythingAfter(input, "give ");
                await ExecGive(parameter, ctx);
            }
            else if (CmdStartsWith(input, "take "))
            {
                parameter = GetEverythingAfter(input, "take ");
                await ExecTake(parameter, ctx);
            }
            else if (CmdStartsWith(input, "drop ") & (ASLVersion >= 280))
            {
                parameter = GetEverythingAfter(input, "drop ");
                await ExecDrop(parameter, ctx);
            }
            else if (CmdStartsWith(input, "get "))
            {
                parameter = GetEverythingAfter(input, "get ");
                await ExecTake(parameter, ctx);
            }
            else if (CmdStartsWith(input, "pick up "))
            {
                parameter = GetEverythingAfter(input, "pick up ");
                await ExecTake(parameter, ctx);
            }
            else if ((cmd == "pick it up") | (cmd == "pick them up") | (cmd == "pick this up") |
                     (cmd == "pick that up") |
                     (cmd == "pick these up") | (cmd == "pick those up") | (cmd == "pick him up") |
                     (cmd == "pick her up"))
            {
                await ExecTake(Strings.Mid(cmd, 6, Strings.InStr(7, cmd, " ") - 6), ctx);
            }
            else if (CmdStartsWith(input, "look "))
            {
                await ExecLook(input, ctx);
            }
            else if (CmdStartsWith(input, "l "))
            {
                await ExecLook("look " + GetEverythingAfter(input, "l "), ctx);
            }
            else if (CmdStartsWith(input, "examine "))
            {
                if (ASLVersion >= 280)
                {
                    await ExecExamine(input, ctx);
                }
                else
                {
                    await ExecLook("look " + GetEverythingAfter(input, "examine "), ctx);
                }
            }
            else if (CmdStartsWith(input, "x "))
            {
                if (ASLVersion >= 280)
                {
                    await ExecExamine("examine " + GetEverythingAfter(input, "x "), ctx);
                }
                else
                {
                    await ExecLook("look " + GetEverythingAfter(input, "x "), ctx);
                }
            }
            else if ((cmd == "l") | (cmd == "look"))
            {
                await ExecLook("look", ctx);
            }
            else if ((cmd == "x") | (cmd == "examine"))
            {
                await ExecExamine("examine", ctx);
            }
            else if (CmdStartsWith(input, "use "))
            {
                await ExecUse(input, ctx);
            }
            else if (CmdStartsWith(input, "open ") & (ASLVersion >= 391))
            {
                await ExecOpenClose(input, ctx);
            }
            else if (CmdStartsWith(input, "close ") & (ASLVersion >= 391))
            {
                await ExecOpenClose(input, ctx);
            }
            else if (CmdStartsWith(input, "put ") & (ASLVersion >= 391))
            {
                await ExecAddRemove(input, ctx);
            }
            else if (CmdStartsWith(input, "add ") & (ASLVersion >= 391))
            {
                await ExecAddRemove(input, ctx);
            }
            else if (CmdStartsWith(input, "remove ") & (ASLVersion >= 391))
            {
                await ExecAddRemove(input, ctx);
            }
            else if (cmd == "save")
            {
                _player.RequestSave(null);
            }
            else if (cmd == "quit")
            {
                GameFinished();
            }
            else if (BeginsWith(cmd, "help"))
            {
                await ShowHelp(ctx);
            }
            else if (cmd == "about")
            {
                await ShowGameAbout(ctx);
            }
            else if (cmd == "clear")
            {
                DoClear();
            }
            else if (cmd == "debug")
            {
                // TO DO: This is temporary, would be better to have a log viewer built in to Player
                foreach (var logEntry in _log)
                {
                    await Print(logEntry, ctx);
                }
            }
            else if ((cmd == "inventory") | (cmd == "inv") | (cmd == "i"))
            {
                if (ASLVersion >= 280)
                {
                    for (int i = 1, loopTo1 = _numberObjs; i <= loopTo1; i++)
                    {
                        if ((_objs[i].ContainerRoom == "inventory") & _objs[i].Exists & _objs[i].Visible)
                        {
                            invList = invList + _objs[i].Prefix;

                            if (string.IsNullOrEmpty(_objs[i].ObjectAlias))
                            {
                                invList = invList + "|b" + _objs[i].ObjectName + "|xb";
                            }
                            else
                            {
                                invList = invList + "|b" + _objs[i].ObjectAlias + "|xb";
                            }

                            if (!string.IsNullOrEmpty(_objs[i].Suffix))
                            {
                                invList = invList + " " + _objs[i].Suffix;
                            }

                            invList = invList + ", ";
                        }
                    }
                }
                else
                {
                    for (int j = 1, loopTo2 = _numberItems; j <= loopTo2; j++)
                    {
                        if (_items[j].Got)
                        {
                            invList = invList + _items[j].Name + ", ";
                        }
                    }
                }

                if (!string.IsNullOrEmpty(invList))
                {
                    invList = Strings.Left(invList, Strings.Len(invList) - 2);
                    invList = Strings.UCase(Strings.Left(invList, 1)) + Strings.Mid(invList, 2);

                    var pos = 1;
                    int lastComma = default, thisComma;
                    do
                    {
                        thisComma = Strings.InStr(pos, invList, ",");
                        if (thisComma != 0)
                        {
                            lastComma = thisComma;
                            pos = thisComma + 1;
                        }
                    } while (thisComma != 0);

                    if (lastComma != 0)
                    {
                        invList = Strings.Left(invList, lastComma - 1) + " and" + Strings.Mid(invList, lastComma + 1);
                    }

                    await Print("You are carrying:|n" + invList + ".", ctx);
                }
                else
                {
                    await Print("You are not carrying anything.", ctx);
                }
            }
            else if (CmdStartsWith(input, "oops "))
            {
                await ExecOops(GetEverythingAfter(input, "oops "), ctx);
            }
            else if (CmdStartsWith(input, "the "))
            {
                await ExecOops(GetEverythingAfter(input, "the "), ctx);
            }
            else
            {
                await PlayerErrorMessage(PlayerError.BadCommand, ctx);
            }
        }

        if (!skipAfterTurn)
        {
            // Execute any "afterturn" script:
            globalOverride = false;

            if (roomID != 0)
            {
                if (!string.IsNullOrEmpty(_rooms[roomID].AfterTurnScript))
                {
                    if (BeginsWith(_rooms[roomID].AfterTurnScript, "override"))
                    {
                        await ExecuteScript(GetEverythingAfter(_rooms[roomID].AfterTurnScript, "override"), ctx);
                        globalOverride = true;
                    }
                    else
                    {
                        await ExecuteScript(_rooms[roomID].AfterTurnScript, ctx);
                    }
                }
            }

            // was set to NullThread here for some reason
            if (!string.IsNullOrEmpty(_afterTurnScript) & !globalOverride)
            {
                await ExecuteScript(_afterTurnScript, ctx);
            }
        }

        await Print("", ctx);

        if (!dontSetIt)
        {
            // Use "DontSetIt" when we don't want "it" etc. to refer to the object used in this turn.
            // This is used for e.g. auto-remove object from container when taking.
            _lastIt = _thisTurnIt;
            _lastItMode = _thisTurnItMode;
        }

        if ((_badCmdBefore ?? "") == (oldBadCmdBefore ?? ""))
        {
            _badCmdBefore = "";
        }

        return true;
    }

    private bool CmdStartsWith(string cmd, string startsWith)
    {
        // When we are checking user input in ExecCommand, we check things like whether
        // the player entered something beginning with "put ". We need to trim what the player entered
        // though, otherwise they might just type "put " and then we would try disambiguating an object
        // called "".

        return BeginsWith(Strings.Trim(cmd), startsWith);
    }

    private async Task ShowHelp(Context ctx)
    {
        // In Quest 4 and below, the help text displays in a separate window. In Quest 5, it displays
        // in the same window as the game text.
        await Print("|b|cl|s14Quest Quick Help|xb|cb|s00", ctx);
        await Print("", ctx);
        await Print(
            "|cl|bMoving|xb|cb Press the direction buttons in the 'Compass' pane, or type |bGO NORTH|xb, |bSOUTH|xb, |bE|xb, etc. |xn",
            ctx);
        await Print(
            "To go into a place, type |bGO TO ...|xb . To leave a place, type |bOUT, EXIT|xb or |bLEAVE|xb, or press the '|crOUT|cb' button.|n",
            ctx);
        await Print(
            "|cl|bObjects and Characters|xb|cb Use |bTAKE ...|xb, |bGIVE ... TO ...|xb, |bTALK|xb/|bSPEAK TO ...|xb, |bUSE ... ON|xb/|bWITH ...|xb, |bLOOK AT ...|xb, etc.|n",
            ctx);
        await Print("|cl|bExit Quest|xb|cb Type |bQUIT|xb to leave Quest.|n", ctx);
        await Print(
            "|cl|bMisc|xb|cb Type |bABOUT|xb to get information on the current game. The next turn after referring to an object or character, you can use |bIT|xb, |bHIM|xb etc. as appropriate to refer to it/him/etc. again. If you make a mistake when typing an object's name, type |bOOPS|xb followed by your correction.|n",
            ctx);
        await Print(
            "|cl|bKeyboard shortcuts|xb|cb Press the |crup arrow|cb and |crdown arrow|cb to scroll through commands you have already typed in. Press |crEsc|cb to clear the command box.|n|n",
            ctx);
        await Print("Further information is available by selecting |iQuest Documentation|xi from the |iHelp|xi menu.", ctx);
    }
}
