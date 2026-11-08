using System.Globalization;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using QuestViva.Common;
using Constants = Microsoft.VisualBasic.Constants;
using Strings = Microsoft.VisualBasic.Strings;

namespace QuestViva.Legacy;

public partial class V4Game
{
    private async Task ExecuteSelectCase(string script, Context ctx)
    {
        // ScriptLine passed will look like this:
        // select case <whatever> do <!intprocX>
        // with all the case statements in the intproc.

        var afterLine = GetAfterParameter(script);

        if (!BeginsWith(afterLine, "do <!intproc"))
        {
            LogASLError("No case block specified for '" + script + "'", LogType.WarningError);
            return;
        }

        var blockName = await GetParameter(afterLine, ctx);
        var block = await DefineBlockParam("procedure", blockName);
        var checkValue = await GetParameter(script, ctx);
        var caseMatch = false;

        for (int i = block.StartLine + 1, loopTo = block.EndLine - 1; i <= loopTo; i++)
            // Go through all the cases until we find the one that matches
        {
            if (!string.IsNullOrEmpty(_lines[i]))
            {
                if (!BeginsWith(_lines[i], "case "))
                {
                    LogASLError("Invalid line in 'select case' block: '" + _lines[i] + "'", LogType.WarningError);
                }
                else
                {
                    var caseScript = "";

                    if (BeginsWith(_lines[i], "case else "))
                    {
                        caseMatch = true;
                        caseScript = GetEverythingAfter(_lines[i], "case else ");
                    }
                    else
                    {
                        var thisCase = await GetParameter(_lines[i], ctx);
                        var finished = false;

                        do
                        {
                            var SCP = Strings.InStr(thisCase, ";");
                            if (SCP == 0)
                            {
                                SCP = Strings.Len(thisCase) + 1;
                                finished = true;
                            }

                            var condition = Strings.Trim(Strings.Left(thisCase, SCP - 1));
                            if ((condition ?? "") == (checkValue ?? ""))
                            {
                                caseScript = GetAfterParameter(_lines[i]);
                                caseMatch = true;
                                finished = true;
                            }
                            else
                            {
                                thisCase = Strings.Mid(thisCase, SCP + 1);
                            }
                        } while (!finished);
                    }

                    if (caseMatch)
                    {
                        await ExecuteScript(caseScript, ctx);
                        return;
                    }
                }
            }
        }
    }

    private async Task ExecuteFlag(string line, Context ctx)
    {
        var propertyString = "";

        if (BeginsWith(line, "on "))
        {
            propertyString = await GetParameter(line, ctx);
        }
        else if (BeginsWith(line, "off "))
        {
            propertyString = "not " + await GetParameter(line, ctx);
        }

        // Game object always has ObjID 1
        await AddToObjectProperties(propertyString, 1, ctx);
    }

    private bool ExecuteIfFlag(string flag)
    {
        // Game ObjID is 1
        return GetObjectProperty(flag, 1, true) == "yes";
    }

    private async Task ExecuteIncDec(string line, Context ctx)
    {
        string variable;
        double change;
        var param = await GetParameter(line, ctx);

        var sc = Strings.InStr(param, ";");
        if (sc == 0)
        {
            change = 1d;
            variable = param;
        }
        else
        {
            change = Conversion.Val(Strings.Mid(param, sc + 1));
            variable = Strings.Trim(Strings.Left(param, sc - 1));
        }

        var value = GetNumericContents(variable, ctx, true);
        if (value <= -32766)
        {
            value = 0d;
        }

        if (BeginsWith(line, "inc "))
        {
            value = value + change;
        }
        else if (BeginsWith(line, "dec "))
        {
            value = value - change;
        }

        var arrayIndex = GetArrayIndex(variable, ctx);
        await SetNumericVariableContents(arrayIndex.Name, value, ctx, arrayIndex.Index);
    }

    private async Task ExecType(string typeData, Context ctx)
    {
        var id = default(int);
        var found = default(bool);
        var scp = Strings.InStr(typeData, ";");

        if (scp == 0)
        {
            LogASLError("No type name given in 'type <" + typeData + ">'");
            return;
        }

        var objName = Strings.Trim(Strings.Left(typeData, scp - 1));
        var typeName = Strings.Trim(Strings.Mid(typeData, scp + 1));

        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(objName) ?? ""))
            {
                found = true;
                id = i;
                break;
            }
        }

        if (!found)
        {
            LogASLError("No such object in 'type <" + typeData + ">'");
            return;
        }

        var o = _objs[id];

        o.NumberTypesIncluded = o.NumberTypesIncluded + 1;
        Array.Resize(ref o.TypesIncluded, o.NumberTypesIncluded + 1);
        o.TypesIncluded[o.NumberTypesIncluded] = typeName;

        var propertyData = await GetPropertiesInType(typeName);
        await AddToObjectProperties(propertyData.Properties, id, ctx);
        for (int i = 1, loopTo1 = propertyData.NumberActions; i <= loopTo1; i++)
        {
            await AddObjectAction(id, propertyData.Actions[i].ActionName, propertyData.Actions[i].Script);
        }

        // New as of Quest 4.0. Fixes bug that "if type" would fail for any
        // parent types included by the "type" command.
        for (int i = 1, loopTo2 = propertyData.NumberTypesIncluded; i <= loopTo2; i++)
        {
            o.NumberTypesIncluded = o.NumberTypesIncluded + 1;
            Array.Resize(ref o.TypesIncluded, o.NumberTypesIncluded + 1);
            o.TypesIncluded[o.NumberTypesIncluded] = propertyData.TypesIncluded[i];
        }
    }

    private bool ExecuteIfAction(string actionData)
    {
        var id = default(int);

        var scp = Strings.InStr(actionData, ";");

        if (scp == 0)
        {
            LogASLError("No action name given in condition 'action <" + actionData + ">' ...", LogType.WarningError);
            return false;
        }

        var objName = Strings.Trim(Strings.Left(actionData, scp - 1));
        var actionName = Strings.Trim(Strings.Mid(actionData, scp + 1));
        var found = false;

        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(objName) ?? ""))
            {
                found = true;
                id = i;
                break;
            }
        }

        if (!found)
        {
            LogASLError("No such object '" + objName + "' in condition 'action <" + actionData + ">' ...",
                LogType.WarningError);
            return false;
        }

        var o = _objs[id];

        for (int i = 1, loopTo1 = o.NumberActions; i <= loopTo1; i++)
        {
            if ((Strings.LCase(o.Actions[i].ActionName) ?? "") == (Strings.LCase(actionName) ?? ""))
            {
                return true;
            }
        }

        return false;
    }

    private bool ExecuteIfType(string typeData)
    {
        var id = default(int);

        var scp = Strings.InStr(typeData, ";");

        if (scp == 0)
        {
            LogASLError("No type name given in condition 'type <" + typeData + ">' ...", LogType.WarningError);
            return false;
        }

        var objName = Strings.Trim(Strings.Left(typeData, scp - 1));
        var typeName = Strings.Trim(Strings.Mid(typeData, scp + 1));

        var found = false;

        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(objName) ?? ""))
            {
                found = true;
                id = i;
                break;
            }
        }

        if (!found)
        {
            LogASLError("No such object '" + objName + "' in condition 'type <" + typeData + ">' ...",
                LogType.WarningError);
            return false;
        }

        var o = _objs[id];

        for (int i = 1, loopTo1 = o.NumberTypesIncluded; i <= loopTo1; i++)
        {
            if ((Strings.LCase(o.TypesIncluded[i]) ?? "") == (Strings.LCase(typeName) ?? ""))
            {
                return true;
            }
        }

        return false;
    }

    private async Task ExecForEach(string scriptLine, Context ctx)
    {
        string inLocation, scriptToRun;
        var isExit = default(bool);
        var isRoom = default(bool);

        if (BeginsWith(scriptLine, "object "))
        {
            scriptLine = GetEverythingAfter(scriptLine, "object ");
            if (!BeginsWith(scriptLine, "in "))
            {
                LogASLError("Expected 'in' in 'for each object " + ReportErrorLine(scriptLine) + "'",
                    LogType.WarningError);
                return;
            }
        }
        else if (BeginsWith(scriptLine, "exit "))
        {
            scriptLine = GetEverythingAfter(scriptLine, "exit ");
            if (!BeginsWith(scriptLine, "in "))
            {
                LogASLError("Expected 'in' in 'for each exit " + ReportErrorLine(scriptLine) + "'",
                    LogType.WarningError);
                return;
            }

            isExit = true;
        }
        else if (BeginsWith(scriptLine, "room "))
        {
            scriptLine = GetEverythingAfter(scriptLine, "room ");
            if (!BeginsWith(scriptLine, "in "))
            {
                LogASLError("Expected 'in' in 'for each room " + ReportErrorLine(scriptLine) + "'",
                    LogType.WarningError);
                return;
            }

            isRoom = true;
        }
        else
        {
            LogASLError("Unknown type in 'for each " + ReportErrorLine(scriptLine) + "'", LogType.WarningError);
            return;
        }

        scriptLine = GetEverythingAfter(scriptLine, "in ");

        if (BeginsWith(scriptLine, "game "))
        {
            inLocation = "";
            scriptToRun = GetEverythingAfter(scriptLine, "game ");
        }
        else
        {
            inLocation = Strings.LCase(await GetParameter(scriptLine, ctx));
            var bracketPos = Strings.InStr(scriptLine, ">");
            scriptToRun = Strings.Trim(Strings.Mid(scriptLine, bracketPos + 1));
        }

        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if (string.IsNullOrEmpty(inLocation) |
                ((Strings.LCase(_objs[i].ContainerRoom) ?? "") == (inLocation ?? "")))
            {
                if ((_objs[i].IsRoom == isRoom) & (_objs[i].IsExit == isExit))
                {
                    await SetStringContents("quest.thing", _objs[i].ObjectName, ctx);
                    await ExecuteScript(scriptToRun, ctx);
                }
            }
        }
    }

    private async Task<bool> ExecuteCondition(string condition, Context ctx)
    {
        bool result = default, thisNot;

        if (BeginsWith(condition, "not "))
        {
            thisNot = true;
            condition = GetEverythingAfter(condition, "not ");
        }
        else
        {
            thisNot = false;
        }

        if (BeginsWith(condition, "got "))
        {
            result = ExecuteIfGot(await GetParameter(condition, ctx));
        }
        else if (BeginsWith(condition, "has "))
        {
            result = ExecuteIfHas(await GetParameter(condition, ctx));
        }
        else if (BeginsWith(condition, "ask "))
        {
            result = await ExecuteIfAskAsync(await GetParameter(condition, ctx));
        }
        else if (BeginsWith(condition, "is "))
        {
            result = ExecuteIfIs(await GetParameter(condition, ctx));
        }
        else if (BeginsWith(condition, "here "))
        {
            result = ExecuteIfHere(await GetParameter(condition, ctx), ctx);
        }
        else if (BeginsWith(condition, "exists "))
        {
            result = ExecuteIfExists(await GetParameter(condition, ctx), false);
        }
        else if (BeginsWith(condition, "real "))
        {
            result = ExecuteIfExists(await GetParameter(condition, ctx), true);
        }
        else if (BeginsWith(condition, "property "))
        {
            result = ExecuteIfProperty(await GetParameter(condition, ctx));
        }
        else if (BeginsWith(condition, "action "))
        {
            result = ExecuteIfAction(await GetParameter(condition, ctx));
        }
        else if (BeginsWith(condition, "type "))
        {
            result = ExecuteIfType(await GetParameter(condition, ctx));
        }
        else if (BeginsWith(condition, "flag "))
        {
            result = ExecuteIfFlag(await GetParameter(condition, ctx));
        }

        if (thisNot)
        {
            result = !result;
        }

        return result;
    }

    private async Task<bool> ExecuteConditions(string list, Context ctx)
    {
        var conditions = default(string[]);
        var numConditions = 0;
        var operations = default(string[]);
        var obscuredConditionList = await ObliterateParameters(list);
        var pos = 1;
        var isFinalCondition = false;

        do
        {
            numConditions = numConditions + 1;
            Array.Resize(ref conditions, numConditions + 1);
            Array.Resize(ref operations, numConditions + 1);

            var nextCondition = "AND";
            var nextConditionPos = Strings.InStr(pos, obscuredConditionList, "and ");
            if (nextConditionPos == 0)
            {
                nextConditionPos = Strings.InStr(pos, obscuredConditionList, "or ");
                nextCondition = "OR";
            }

            if (nextConditionPos == 0)
            {
                nextConditionPos = Strings.Len(obscuredConditionList) + 2;
                isFinalCondition = true;
                nextCondition = "FINAL";
            }

            var thisCondition = Strings.Trim(Strings.Mid(list, pos, nextConditionPos - pos - 1));
            conditions[numConditions] = thisCondition;
            operations[numConditions] = nextCondition;

            // next condition starts from space after and/or
            pos = Strings.InStr(nextConditionPos, obscuredConditionList, " ");
        } while (!isFinalCondition);

        operations[0] = "AND";
        var result = true;

        for (int i = 1, loopTo = numConditions; i <= loopTo; i++)
        {
            var thisResult = await ExecuteCondition(conditions[i], ctx);

            if (operations[i - 1] == "AND")
            {
                result = thisResult & result;
            }
            else if (operations[i - 1] == "OR")
            {
                result = thisResult | result;
            }
        }

        return result;
    }

    private async Task ExecuteCreate(string data, Context ctx)
    {
        string newName;

        if (BeginsWith(data, "room "))
        {
            newName = await GetParameter(data, ctx);
            _numberRooms = _numberRooms + 1;
            Array.Resize(ref _rooms, _numberRooms + 1);
            _rooms[_numberRooms] = new RoomType();
            _rooms[_numberRooms].RoomName = newName;

            _numberObjs = _numberObjs + 1;
            Array.Resize(ref _objs, _numberObjs + 1);
            _objs[_numberObjs] = new ObjectType();
            _objs[_numberObjs].ObjectName = newName;
            _objs[_numberObjs].IsRoom = true;
            _objs[_numberObjs].CorresRoom = newName;
            _objs[_numberObjs].CorresRoomId = _numberRooms;

            _rooms[_numberRooms].ObjId = _numberObjs;

            AddToChangeLog("room " + newName, "create");

            if (ASLVersion >= 410)
            {
                await AddToObjectProperties(_defaultRoomProperties.Properties, _numberObjs, ctx);
                for (int j = 1, loopTo = _defaultRoomProperties.NumberActions; j <= loopTo; j++)
                {
                    await AddObjectAction(_numberObjs, _defaultRoomProperties.Actions[j].ActionName,
                        _defaultRoomProperties.Actions[j].Script);
                }

                _rooms[_numberRooms].Exits = new RoomExits(this);
                _rooms[_numberRooms].Exits.SetObjId(_rooms[_numberRooms].ObjId);
            }
        }

        else if (BeginsWith(data, "object "))
        {
            var paramData = await GetParameter(data, ctx);
            var scp = Strings.InStr(paramData, ";");
            string containerRoom;

            if (scp == 0)
            {
                newName = paramData;
                containerRoom = "";
            }
            else
            {
                newName = Strings.Trim(Strings.Left(paramData, scp - 1));
                containerRoom = Strings.Trim(Strings.Mid(paramData, scp + 1));
            }

            _numberObjs = _numberObjs + 1;
            Array.Resize(ref _objs, _numberObjs + 1);
            _objs[_numberObjs] = new ObjectType();

            var o = _objs[_numberObjs];
            o.ObjectName = newName;
            o.ObjectAlias = newName;
            o.ContainerRoom = containerRoom;
            o.Exists = true;
            o.Visible = true;
            o.Gender = "it";
            o.Article = "it";

            AddToChangeLog("object " + newName, "create " + _objs[_numberObjs].ContainerRoom);

            if (ASLVersion >= 410)
            {
                await AddToObjectProperties(_defaultProperties.Properties, _numberObjs, ctx);
                for (int j = 1, loopTo1 = _defaultProperties.NumberActions; j <= loopTo1; j++)
                {
                    await AddObjectAction(_numberObjs, _defaultProperties.Actions[j].ActionName,
                        _defaultProperties.Actions[j].Script);
                }
            }

            if (!_gameLoading)
            {
                await UpdateObjectList(ctx);
            }
        }

        else if (BeginsWith(data, "exit "))
        {
            await ExecuteCreateExit(data, ctx);
        }
    }

    private async Task ExecMoveThing(string data, Thing type, Context ctx)
    {
        var scp = Strings.InStr(data, ";");
        var name = Strings.Trim(Strings.Left(data, scp - 1));
        var place = Strings.Trim(Strings.Mid(data, scp + 1));
        MoveThing(name, place, type, ctx);
    }

    private async Task ExecProperty(string data, Context ctx)
    {
        var id = default(int);
        var found = default(bool);
        var scp = Strings.InStr(data, ";");

        if (scp == 0)
        {
            LogASLError("No property data given in 'property <" + data + ">'", LogType.WarningError);
            return;
        }

        var name = Strings.Trim(Strings.Left(data, scp - 1));
        var properties = Strings.Trim(Strings.Mid(data, scp + 1));

        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(name) ?? ""))
            {
                found = true;
                id = i;
                break;
            }
        }

        if (!found)
        {
            LogASLError("No such object in 'property <" + data + ">'", LogType.WarningError);
            return;
        }

        await AddToObjectProperties(properties, id, ctx);
    }

    private async Task ExecuteDo(string procedureName, Context ctx)
    {
        var newCtx = CopyContext(ctx);
        var numParameters = 0;
        bool useNewCtx;

        if ((ASLVersion >= 392) & (Strings.Left(procedureName, 8) == "!intproc"))
            // If "do" procedure is run in a new context, context info is not passed to any nested
            // script blocks in braces.
        {
            useNewCtx = false;
        }
        else
        {
            useNewCtx = true;
        }

        if (ASLVersion >= 284)
        {
            var obp = Strings.InStr(procedureName, "(");
            var cbp = default(int);
            if (obp != 0)
            {
                cbp = Strings.InStr(obp + 1, procedureName, ")");
            }

            if ((obp != 0) & (cbp != 0))
            {
                var parameters = Strings.Mid(procedureName, obp + 1, cbp - obp - 1);
                procedureName = Strings.Left(procedureName, obp - 1);

                parameters = parameters + ";";
                var pos = 1;
                do
                {
                    numParameters = numParameters + 1;
                    var scp = Strings.InStr(pos, parameters, ";");

                    newCtx.NumParameters = numParameters;
                    Array.Resize(ref newCtx.Parameters, numParameters + 1);
                    newCtx.Parameters[numParameters] = Strings.Trim(Strings.Mid(parameters, pos, scp - pos));

                    pos = scp + 1;
                } while (pos < Strings.Len(parameters));
            }
        }

        var block = await DefineBlockParam("procedure", procedureName);
        if ((block.StartLine == 0) & (block.EndLine == 0))
        {
            LogASLError("No such procedure " + procedureName, LogType.WarningError);
        }
        else
        {
            for (int i = block.StartLine + 1, loopTo = block.EndLine - 1; i <= loopTo; i++)
            {
                if (!useNewCtx)
                {
                    await ExecuteScript(_lines[i], ctx);
                }
                else
                {
                    await ExecuteScript(_lines[i], newCtx);
                    ctx.DontProcessCommand = newCtx.DontProcessCommand;
                }
            }
        }
    }

    private async Task ExecuteDoAction(string data, Context ctx)
    {
        var id = default(int);

        var scp = Strings.InStr(data, ";");
        if (scp == 0)
        {
            LogASLError("No action name specified in 'doaction <" + data + ">'");
            return;
        }

        var objName = Strings.LCase(Strings.Trim(Strings.Left(data, scp - 1)));
        var actionName = Strings.Trim(Strings.Mid(data, scp + 1));
        var found = false;

        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (objName ?? ""))
            {
                found = true;
                id = i;
                break;
            }
        }

        if (!found)
        {
            LogASLError("No such object '" + objName + "'");
            return;
        }

        await DoAction(id, actionName, ctx);
    }

    private bool ExecuteIfHere(string obj, Context ctx)
    {
        if (ASLVersion <= 281)
        {
            for (int i = 1, loopTo = _numberChars; i <= loopTo; i++)
            {
                if (((_chars[i].ContainerRoom ?? "") == (_currentRoom ?? "")) & _chars[i].Exists)
                {
                    if ((Strings.LCase(obj) ?? "") == (Strings.LCase(_chars[i].ObjectName) ?? ""))
                    {
                        return true;
                    }
                }
            }
        }

        for (int i = 1, loopTo1 = _numberObjs; i <= loopTo1; i++)
        {
            if (((Strings.LCase(_objs[i].ContainerRoom) ?? "") == (Strings.LCase(_currentRoom) ?? "")) &
                _objs[i].Exists)
            {
                if ((Strings.LCase(obj) ?? "") == (Strings.LCase(_objs[i].ObjectName) ?? ""))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool ExecuteIfExists(string obj, bool realOnly)
    {
        var result = default(bool);
        var errorReport = false;
        int scp;

        if (Strings.InStr(obj, ";") != 0)
        {
            scp = Strings.InStr(obj, ";");
            if (Strings.LCase(Strings.Trim(Strings.Mid(obj, scp + 1))) == "report")
            {
                errorReport = true;
            }

            obj = Strings.Left(obj, scp - 1);
        }

        var found = false;

        if (ASLVersion < 281)
        {
            for (int i = 1, loopTo = _numberChars; i <= loopTo; i++)
            {
                if ((Strings.LCase(obj) ?? "") == (Strings.LCase(_chars[i].ObjectName) ?? ""))
                {
                    result = _chars[i].Exists;
                    found = true;
                    break;
                }
            }
        }

        if (!found)
        {
            for (int i = 1, loopTo1 = _numberObjs; i <= loopTo1; i++)
            {
                if ((Strings.LCase(obj) ?? "") == (Strings.LCase(_objs[i].ObjectName) ?? ""))
                {
                    result = _objs[i].Exists;
                    found = true;
                    break;
                }
            }
        }

        if (!found & errorReport)
        {
            LogASLError("No such character/object '" + obj + "'.", LogType.UserError);
        }

        if (!found)
        {
            result = false;
        }

        if (realOnly)
        {
            return found;
        }

        return result;
    }

    private bool ExecuteIfProperty(string data)
    {
        var id = default(int);
        var scp = Strings.InStr(data, ";");

        if (scp == 0)
        {
            LogASLError("No property name given in condition 'property <" + data + ">' ...", LogType.WarningError);
            return false;
        }

        var objName = Strings.Trim(Strings.Left(data, scp - 1));
        var propertyName = Strings.Trim(Strings.Mid(data, scp + 1));
        var found = false;

        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(objName) ?? ""))
            {
                found = true;
                id = i;
                break;
            }
        }

        if (!found)
        {
            LogASLError("No such object '" + objName + "' in condition 'property <" + data + ">' ...",
                LogType.WarningError);
            return false;
        }

        return GetObjectProperty(propertyName, id, true) == "yes";
    }

    private async Task ExecuteRepeat(string data, Context ctx)
    {
        bool repeatWhileTrue;
        var repeatScript = "";
        int bracketPos;
        string afterBracket;
        var foundScript = false;

        if (BeginsWith(data, "while "))
        {
            repeatWhileTrue = true;
            data = GetEverythingAfter(data, "while ");
        }
        else if (BeginsWith(data, "until "))
        {
            repeatWhileTrue = false;
            data = GetEverythingAfter(data, "until ");
        }
        else
        {
            LogASLError("Expected 'until' or 'while' in 'repeat " + ReportErrorLine(data) + "'", LogType.WarningError);
            return;
        }

        var pos = 1;
        do
        {
            bracketPos = Strings.InStr(pos, data, ">");
            afterBracket = Strings.Trim(Strings.Mid(data, bracketPos + 1));
            if (!BeginsWith(afterBracket, "and ") & !BeginsWith(afterBracket, "or "))
            {
                repeatScript = afterBracket;
                foundScript = true;
            }
            else
            {
                pos = bracketPos + 1;
            }
        } while (!(foundScript | string.IsNullOrEmpty(afterBracket)));

        var conditions = Strings.Trim(Strings.Left(data, bracketPos));
        var finished = false;

        do
        {
            if (await ExecuteConditions(conditions, ctx) == repeatWhileTrue)
            {
                await ExecuteScript(repeatScript, ctx);
            }
            else
            {
                finished = true;
            }
        } while (!(finished | _gameFinished));
    }

    private async Task<string> DoFunction(string data, Context ctx)
    {
        string name, parameter;
        var intFuncResult = "";
        var intFuncExecuted = false;
        var paramPos = Strings.InStr(data, "(");

        if (paramPos != 0)
        {
            name = Strings.Trim(Strings.Left(data, paramPos - 1));
            var endParamPos = Strings.InStrRev(data, ")");
            if (endParamPos == 0)
            {
                LogASLError("Expected ) in $" + data + "$", LogType.WarningError);
                return "";
            }

            parameter = Strings.Mid(data, paramPos + 1, endParamPos - paramPos - 1);
        }
        else
        {
            name = data;
            parameter = "";
        }

        DefineBlock block;
        block = await DefineBlockParam("function", name);

        if ((block.StartLine == 0) & (block.EndLine == 0))
        {
            // Function does not exist; try an internal function.
            intFuncResult = await DoInternalFunction(name, parameter, ctx);
            if (intFuncResult == "__NOTDEFINED")
            {
                LogASLError("No such function '" + name + "'", LogType.WarningError);
                return "[ERROR]";
            }

            intFuncExecuted = true;
        }

        if (intFuncExecuted)
        {
            return intFuncResult;
        }

        var newCtx = CopyContext(ctx);
        var numParameters = 0;
        var pos = 1;

        if (!string.IsNullOrEmpty(parameter))
        {
            parameter = parameter + ";";
            do
            {
                numParameters = numParameters + 1;
                var scp = Strings.InStr(pos, parameter, ";");

                var parameterData = Strings.Trim(Strings.Mid(parameter, pos, scp - pos));
                await SetStringContents("quest.function.parameter." + Strings.Trim(InvariantStrings.Str(numParameters)),
                    parameterData, ctx);

                newCtx.NumParameters = numParameters;
                Array.Resize(ref newCtx.Parameters, numParameters + 1);
                newCtx.Parameters[numParameters] = parameterData;

                pos = scp + 1;
            } while (pos < Strings.Len(parameter));

            await SetStringContents("quest.function.numparameters", Strings.Trim(InvariantStrings.Str(numParameters)), ctx);
        }
        else
        {
            await SetStringContents("quest.function.numparameters", "0", ctx);
            newCtx.NumParameters = 0;
        }

        var result = "";

        for (int i = block.StartLine + 1, loopTo = block.EndLine - 1; i <= loopTo; i++)
        {
            await ExecuteScript(_lines[i], newCtx);
            result = newCtx.FunctionReturnValue;
        }

        return result;
    }

    private async Task<string> DoInternalFunction(string name, string parameter, Context ctx)
    {
        var parameters = default(string[]);
        var untrimmedParameters = default(string[]);
        var objId = default(int);
        var numParameters = 0;
        var pos = 1;

        if (!string.IsNullOrEmpty(parameter))
        {
            parameter = parameter + ";";
            do
            {
                numParameters = numParameters + 1;
                var scp = Strings.InStr(pos, parameter, ";");
                Array.Resize(ref parameters, numParameters + 1);
                Array.Resize(ref untrimmedParameters, numParameters + 1);

                untrimmedParameters[numParameters] = Strings.Mid(parameter, pos, scp - pos);
                parameters[numParameters] = Strings.Trim(untrimmedParameters[numParameters]);

                pos = scp + 1;
            } while (pos < Strings.Len(parameter));

            // Remove final ";"
            parameter = Strings.Left(parameter, Strings.Len(parameter) - 1);
        }
        else
        {
            numParameters = 1;
            parameters = new string[2];
            untrimmedParameters = new string[2];
            parameters[1] = "";
            untrimmedParameters[1] = "";
        }

        string param2;
        string param3;

        if (name == "displayname")
        {
            objId = await GetObjectId(parameters[1], ctx);
            if (objId == -1)
            {
                LogASLError("Object '" + parameters[1] + "' does not exist", LogType.WarningError);
                return "!";
            }

            return _objs[await GetObjectId(parameters[1], ctx)].ObjectAlias;
        }

        if (name == "numberparameters")
        {
            return Strings.Trim(InvariantStrings.Str(ctx.NumParameters));
        }

        if (name == "parameter")
        {
            if (numParameters == 0)
            {
                LogASLError("No parameter number specified for $parameter$ function", LogType.WarningError);
                return "";
            }

            if (Conversion.Val(parameters[1]) > ctx.NumParameters)
            {
                LogASLError("No parameter number " + parameters[1] + " sent to this function", LogType.WarningError);
                return "";
            }

            return Strings.Trim(ctx.Parameters[Conversions.ToInteger(parameters[1])]);
        }

        if (name == "gettag")
            // Deprecated
        {
            return await FindStatement(await DefineBlockParam("room", parameters[1]), parameters[2]);
        }

        if (name == "objectname")
        {
            return _objs[ctx.CallingObjectId].ObjectName;
        }

        if (name == "locationof")
        {
            for (int i = 1, loopTo = _numberChars; i <= loopTo; i++)
            {
                if ((Strings.LCase(_chars[i].ObjectName) ?? "") == (Strings.LCase(parameters[1]) ?? ""))
                {
                    return _chars[i].ContainerRoom;
                }
            }

            for (int i = 1, loopTo1 = _numberObjs; i <= loopTo1; i++)
            {
                if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(parameters[1]) ?? ""))
                {
                    return _objs[i].ContainerRoom;
                }
            }
        }
        else if (name == "lengthof")
        {
            return InvariantStrings.Str(Strings.Len(untrimmedParameters[1]));
        }
        else if (name == "left")
        {
            if (Conversion.Val(parameters[2]) < 0d)
            {
                LogASLError("Invalid function call in '$Left$(" + parameters[1] + "; " + parameters[2] + ")$'",
                    LogType.WarningError);
                return "!";
            }

            return Strings.Left(parameters[1], Conversions.ToInteger(parameters[2]));
        }
        else if (name == "right")
        {
            if (Conversion.Val(parameters[2]) < 0d)
            {
                LogASLError("Invalid function call in '$Right$(" + parameters[1] + "; " + parameters[2] + ")$'",
                    LogType.WarningError);
                return "!";
            }

            return Strings.Right(parameters[1], Conversions.ToInteger(parameters[2]));
        }
        else if (name == "mid")
        {
            if (numParameters == 3)
            {
                if (Conversion.Val(parameters[2]) < 0d)
                {
                    LogASLError(
                        "Invalid function call in '$Mid$(" + parameters[1] + "; " + parameters[2] + "; " +
                        parameters[3] + ")$'", LogType.WarningError);
                    return "!";
                }

                return Strings.Mid(parameters[1], Conversions.ToInteger(parameters[2]),
                    Conversions.ToInteger(parameters[3]));
            }

            if (numParameters == 2)
            {
                if (Conversion.Val(parameters[2]) < 0d)
                {
                    LogASLError("Invalid function call in '$Mid$(" + parameters[1] + "; " + parameters[2] + ")$'",
                        LogType.WarningError);
                    return "!";
                }

                return Strings.Mid(parameters[1], Conversions.ToInteger(parameters[2]));
            }

            LogASLError("Invalid function call to '$Mid$(...)$'", LogType.WarningError);
            return "";
        }
        else if (name == "rand")
        {
            return InvariantStrings.Str(
                Conversion.Int(_random.NextDouble() *
                               (Conversion.Val(parameters[2]) - Conversion.Val(parameters[1]) + 1d)) +
                Conversion.Val(parameters[1]));
        }
        else if (name == "instr")
        {
            if (numParameters == 3)
            {
                param3 = "";
                if (Strings.InStr(parameters[3], "_") != 0)
                {
                    for (int i = 1, loopTo2 = Strings.Len(parameters[3]); i <= loopTo2; i++)
                    {
                        if (Strings.Mid(parameters[3], i, 1) == "_")
                        {
                            param3 = param3 + " ";
                        }
                        else
                        {
                            param3 = param3 + Strings.Mid(parameters[3], i, 1);
                        }
                    }
                }
                else
                {
                    param3 = parameters[3];
                }

                if (Conversion.Val(parameters[1]) <= 0d)
                {
                    LogASLError(
                        "Invalid function call in '$instr(" + parameters[1] + "; " + parameters[2] + "; " +
                        parameters[3] + ")$'", LogType.WarningError);
                    return "!";
                }

                return Strings.Trim(InvariantStrings.Str(Strings.InStr(Conversions.ToInteger(parameters[1]), parameters[2],
                    param3)));
            }

            if (numParameters == 2)
            {
                param2 = "";
                if (Strings.InStr(parameters[2], "_") != 0)
                {
                    for (int i = 1, loopTo3 = Strings.Len(parameters[2]); i <= loopTo3; i++)
                    {
                        if (Strings.Mid(parameters[2], i, 1) == "_")
                        {
                            param2 = param2 + " ";
                        }
                        else
                        {
                            param2 = param2 + Strings.Mid(parameters[2], i, 1);
                        }
                    }
                }
                else
                {
                    param2 = parameters[2];
                }

                return Strings.Trim(InvariantStrings.Str(Strings.InStr(parameters[1], param2)));
            }

            LogASLError("Invalid function call to '$Instr$(...)$'", LogType.WarningError);
            return "";
        }
        else if (name == "ucase")
        {
            return Strings.UCase(parameters[1]);
        }
        else if (name == "lcase")
        {
            return Strings.LCase(parameters[1]);
        }
        else if (name == "capfirst")
        {
            return Strings.UCase(Strings.Left(parameters[1], 1)) + Strings.Mid(parameters[1], 2);
        }
        else if (name == "symbol")
        {
            if (parameters[1] == "lt")
            {
                return "<";
            }

            if (parameters[1] == "gt")
            {
                return ">";
            }

            return "!";
        }
        else if (name == "loadmethod")
        {
            return _gameLoadMethod;
        }
        else if (name == "timerstate")
        {
            for (int i = 1, loopTo4 = _numberTimers; i <= loopTo4; i++)
            {
                if ((Strings.LCase(_timers[i].TimerName) ?? "") == (Strings.LCase(parameters[1]) ?? ""))
                {
                    if (_timers[i].TimerActive)
                    {
                        return "1";
                    }

                    return "0";
                }
            }

            LogASLError("No such timer '" + parameters[1] + "'", LogType.WarningError);
            return "!";
        }
        else if (name == "timerinterval")
        {
            for (int i = 1, loopTo5 = _numberTimers; i <= loopTo5; i++)
            {
                if ((Strings.LCase(_timers[i].TimerName) ?? "") == (Strings.LCase(parameters[1]) ?? ""))
                {
                    return InvariantStrings.Str(_timers[i].TimerInterval);
                }
            }

            LogASLError("No such timer '" + parameters[1] + "'", LogType.WarningError);
            return "!";
        }
        else if (name == "ubound")
        {
            for (int i = 1, loopTo6 = _numberNumericVariables; i <= loopTo6; i++)
            {
                if ((Strings.LCase(_numericVariable[i].VariableName) ?? "") == (Strings.LCase(parameters[1]) ?? ""))
                {
                    return Strings.Trim(InvariantStrings.Str(_numericVariable[i].VariableUBound));
                }
            }

            for (int i = 1, loopTo7 = _numberStringVariables; i <= loopTo7; i++)
            {
                if ((Strings.LCase(_stringVariable[i].VariableName) ?? "") == (Strings.LCase(parameters[1]) ?? ""))
                {
                    return Strings.Trim(InvariantStrings.Str(_stringVariable[i].VariableUBound));
                }
            }

            LogASLError("No such variable '" + parameters[1] + "'", LogType.WarningError);
            return "!";
        }
        else if (name == "objectproperty")
        {
            var FoundObj = false;
            for (int i = 1, loopTo8 = _numberObjs; i <= loopTo8; i++)
            {
                if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(parameters[1]) ?? ""))
                {
                    FoundObj = true;
                    objId = i;
                    i = _numberObjs;
                }
            }

            if (!FoundObj)
            {
                LogASLError("No such object '" + parameters[1] + "'", LogType.WarningError);
                return "!";
            }

            return GetObjectProperty(parameters[2], objId);
        }
        else if (name == "getobjectname")
        {
            if (numParameters == 3)
            {
                objId = await Disambiguate(parameters[1], parameters[2] + ";" + parameters[3], ctx);
            }
            else if (numParameters == 2)
            {
                objId = await Disambiguate(parameters[1], parameters[2], ctx);
            }
            else
            {
                objId = await Disambiguate(parameters[1], _currentRoom + ";inventory", ctx);
            }

            if (objId <= -1)
            {
                LogASLError("No object found with display name '" + parameters[1] + "'", LogType.WarningError);
                return "!";
            }

            return _objs[objId].ObjectName;
        }
        else if (name == "thisobject")
        {
            return _objs[ctx.CallingObjectId].ObjectName;
        }
        else if (name == "thisobjectname")
        {
            return _objs[ctx.CallingObjectId].ObjectAlias;
        }
        else if (name == "speechenabled")
        {
            return "1";
        }
        else if (name == "removeformatting")
        {
            return RemoveFormatting(parameter);
        }
        else if ((name == "findexit") & (ASLVersion >= 410))
        {
            var e = FindExit(parameter);
            if (e is null)
            {
                return "";
            }

            return _objs[e.GetObjId()].ObjectName;
        }

        return "__NOTDEFINED";
    }

    private async Task ExecFor(string line, Context ctx)
    {
        // See if this is a "for each" loop:
        if (BeginsWith(line, "each "))
        {
            await ExecForEach(GetEverythingAfter(line, "each "), ctx);
            return;
        }

        // Executes a for loop, of form:
        // for <variable; startvalue; endvalue> script
        int endValue;
        int stepValue;
        var forData = await GetParameter(line, ctx);

        // Extract individual components:
        var scp1 = Strings.InStr(forData, ";");
        var scp2 = Strings.InStr(scp1 + 1, forData, ";");
        var scp3 = Strings.InStr(scp2 + 1, forData, ";");
        var counterVariable = Strings.Trim(Strings.Left(forData, scp1 - 1));
        var startValue = Conversions.ToInteger(Strings.Mid(forData, scp1 + 1, scp2 - 1 - scp1));

        if (scp3 != 0)
        {
            endValue = Conversions.ToInteger(Strings.Mid(forData, scp2 + 1, scp3 - 1 - scp2));
            stepValue = Conversions.ToInteger(Strings.Mid(forData, scp3 + 1));
        }
        else
        {
            endValue = Conversions.ToInteger(Strings.Mid(forData, scp2 + 1));
            stepValue = 1;
        }

        var loopScript = Strings.Trim(Strings.Mid(line, Strings.InStr(line, ">") + 1));

        for (double i = startValue, loopTo = endValue;
             (double) stepValue >= 0 ? i <= loopTo : i >= loopTo;
             i += stepValue)
        {
            await SetNumericVariableContents(counterVariable, i, ctx);
            await ExecuteScript(loopScript, ctx);
            i = GetNumericContents(counterVariable, ctx);
        }
    }



    private bool ExecuteIfGot(string item)
    {
        if (ASLVersion >= 280)
        {
            for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
            {
                if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(item) ?? ""))
                {
                    return (_objs[i].ContainerRoom == "inventory") & _objs[i].Exists;
                }
            }

            LogASLError("No object '" + item + "' defined.", LogType.WarningError);
            return false;
        }

        for (int i = 1, loopTo1 = _numberItems; i <= loopTo1; i++)
        {
            if ((Strings.LCase(_items[i].Name) ?? "") == (Strings.LCase(item) ?? ""))
            {
                return _items[i].Got;
            }
        }

        LogASLError("Item '" + item + "' not defined.", LogType.WarningError);
        return false;
    }

    private bool ExecuteIfHas(string condition)
    {
        double checkValue;
        var colNum = default(int);
        var scp = Strings.InStr(condition, ";");
        var name = Strings.Trim(Strings.Left(condition, scp - 1));
        var newVal = Strings.Trim(Strings.Right(condition, Strings.Len(condition) - scp));
        var found = false;

        for (int i = 1, loopTo = _numCollectables; i <= loopTo; i++)
        {
            if ((_collectables[i].Name ?? "") == (name ?? ""))
            {
                colNum = i;
                found = true;
                break;
            }
        }

        if (!found)
        {
            LogASLError("No such collectable in " + condition, LogType.WarningError);
            return false;
        }

        var op = Strings.Left(newVal, 1);
        var newValue = Strings.Trim(Strings.Right(newVal, Strings.Len(newVal) - 1));
        if (Information.IsNumeric(newValue))
        {
            checkValue = Conversion.Val(newValue);
        }
        else
        {
            checkValue = GetCollectableAmount(newValue);
        }

        if (op == "+")
        {
            return _collectables[colNum].Value > checkValue;
        }

        if (op == "-")
        {
            return _collectables[colNum].Value < checkValue;
        }

        if (op == "=")
        {
            return _collectables[colNum].Value == checkValue;
        }

        return false;
    }

    private bool ExecuteIfIs(string condition)
    {
        string value1, value2;
        string op;
        var expectNumerics = default(bool);
        ExpressionResult expResult;

        var scp = Strings.InStr(condition, ";");
        if (scp == 0)
        {
            LogASLError("Expected second parameter in 'is " + condition + "'", LogType.WarningError);
            return false;
        }

        var scp2 = Strings.InStr(scp + 1, condition, ";");
        if (scp2 == 0)
        {
            // Only two parameters => standard "="
            op = "=";
            value1 = Strings.Trim(Strings.Left(condition, scp - 1));
            value2 = Strings.Trim(Strings.Mid(condition, scp + 1));
        }
        else
        {
            value1 = Strings.Trim(Strings.Left(condition, scp - 1));
            op = Strings.Trim(Strings.Mid(condition, scp + 1, scp2 - scp - 1));
            value2 = Strings.Trim(Strings.Mid(condition, scp2 + 1));
        }

        if (ASLVersion >= 391)
        {
            // Evaluate expressions in Value1 and Value2
            expResult = ExpressionHandler(value1);

            if (expResult.Success == ExpressionSuccess.OK)
            {
                value1 = expResult.Result;
            }

            expResult = ExpressionHandler(value2);

            if (expResult.Success == ExpressionSuccess.OK)
            {
                value2 = expResult.Result;
            }
        }

        var result = false;

        switch (op ?? "")
        {
            case "=":
            {
                if ((Strings.LCase(value1) ?? "") == (Strings.LCase(value2) ?? ""))
                {
                    result = true;
                }

                expectNumerics = false;
                break;
            }
            case "!=":
            {
                if ((Strings.LCase(value1) ?? "") != (Strings.LCase(value2) ?? ""))
                {
                    result = true;
                }

                expectNumerics = false;
                break;
            }
            case "gt":
            {
                if (Conversion.Val(value1) > Conversion.Val(value2))
                {
                    result = true;
                }

                expectNumerics = true;
                break;
            }
            case "lt":
            {
                if (Conversion.Val(value1) < Conversion.Val(value2))
                {
                    result = true;
                }

                expectNumerics = true;
                break;
            }
            case "gt=":
            {
                if (Conversion.Val(value1) >= Conversion.Val(value2))
                {
                    result = true;
                }

                expectNumerics = true;
                break;
            }
            case "lt=":
            {
                if (Conversion.Val(value1) <= Conversion.Val(value2))
                {
                    result = true;
                }

                expectNumerics = true;
                break;
            }

            default:
            {
                LogASLError("Unrecognised comparison condition in 'is " + condition + "'", LogType.WarningError);
                break;
            }
        }

        if (expectNumerics)
        {
            if (!(Information.IsNumeric(value1) & Information.IsNumeric(value2)))
            {
                LogASLError("Expected numeric comparison comparing '" + value1 + "' and '" + value2 + "'",
                    LogType.WarningError);
            }
        }

        return result;
    }

    private async Task<string> SetUpChoiceForm(string blockName, Context ctx)
    {
        // Returns script to execute from choice block
        var block = await DefineBlockParam("selection", blockName);
        var prompt = await FindStatement(block, "info");

        var menuOptions = new Dictionary<string, string>();
        var menuScript = new Dictionary<string, string>();

        for (int i = block.StartLine + 1, loopTo = block.EndLine - 1; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[i], "choice "))
            {
                menuOptions.Add(i.ToString(), await GetParameter(_lines[i], ctx));
                menuScript.Add(i.ToString(),
                    Strings.Trim(Strings.Right(_lines[i], Strings.Len(_lines[i]) - Strings.InStr(_lines[i], ">"))));
            }
        }

        await Print("- |i" + prompt + "|xi", ctx);

        var mnu = new MenuData(prompt, menuOptions, false);
        var choice = await ShowMenuAsync(mnu);

        await Print("- " + menuOptions[choice] + "|n", ctx);
        return menuScript[choice];
    }

    private async Task ExecExec(string scriptLine, Context ctx)
    {
        if (ctx.CancelExec)
        {
            return;
        }

        var execLine = await GetParameter(scriptLine, ctx);
        var newCtx = CopyContext(ctx);
        newCtx.StackCounter = newCtx.StackCounter + 1;

        if (newCtx.StackCounter > 500)
        {
            LogASLError("Out of stack space running '" + scriptLine + "' - infinite loop?", LogType.WarningError);
            ctx.CancelExec = true;
            return;
        }

        if (ASLVersion >= 310)
        {
            newCtx.AllowRealNamesInCommand = true;
        }

        if (Strings.InStr(execLine, ";") == 0)
        {
            try
            {
                await ExecCommand(execLine, newCtx, false);
            }
            catch
            {
                LogASLError("Internal error " + Information.Err().Number + " running '" + scriptLine + "'",
                    LogType.WarningError);
                ctx.CancelExec = true;
            }
        }
        else
        {
            var scp = Strings.InStr(execLine, ";");
            var ex = Strings.Trim(Strings.Left(execLine, scp - 1));
            var r = Strings.Trim(Strings.Mid(execLine, scp + 1));
            if (r == "normal")
            {
                await ExecCommand(ex, newCtx, false, false);
            }
            else
            {
                LogASLError("Unrecognised post-command parameter in " + Strings.Trim(scriptLine), LogType.WarningError);
            }
        }
    }

    private async Task ExecuteChoose(string section, Context ctx)
    {
        await ExecuteScript(await SetUpChoiceForm(section, ctx), ctx);
    }

    private async Task ExecuteIf(string scriptLine, Context ctx)
    {
        var ifLine = Strings.Trim(GetEverythingAfter(Strings.Trim(scriptLine), "if "));
        var obscuredLine = await ObliterateParameters(ifLine);
        var thenPos = Strings.InStr(obscuredLine, "then");

        if (thenPos == 0)
        {
            var errMsg = "Expected 'then' missing from script statement '" + ReportErrorLine(scriptLine) +
                         "' - statement bypassed.";
            LogASLError(errMsg, LogType.WarningError);
            return;
        }

        var conditions = Strings.Trim(Strings.Left(ifLine, thenPos - 1));

        thenPos = thenPos + 4;
        var elsePos = Strings.InStr(obscuredLine, "else");
        int thenEndPos;

        if (elsePos == 0)
        {
            thenEndPos = Strings.Len(obscuredLine) + 1;
        }
        else
        {
            thenEndPos = elsePos - 1;
        }

        var thenScript = Strings.Trim(Strings.Mid(ifLine, thenPos, thenEndPos - thenPos));
        var elseScript = "";

        if (elsePos != 0)
        {
            elseScript = Strings.Trim(Strings.Right(ifLine, Strings.Len(ifLine) - (thenEndPos + 4)));
        }

        // Remove braces from around "then" and "else" script
        // commands, if present
        if ((Strings.Left(thenScript, 1) == "{") & (Strings.Right(thenScript, 1) == "}"))
        {
            thenScript = Strings.Mid(thenScript, 2, Strings.Len(thenScript) - 2);
        }

        if ((Strings.Left(elseScript, 1) == "{") & (Strings.Right(elseScript, 1) == "}"))
        {
            elseScript = Strings.Mid(elseScript, 2, Strings.Len(elseScript) - 2);
        }

        if (await ExecuteConditions(conditions, ctx))
        {
            await ExecuteScript(thenScript, ctx);
        }
        else if (elsePos != 0)
        {
            await ExecuteScript(elseScript, ctx);
        }
    }

    private async Task ExecuteScript(string scriptLine, Context ctx, int newCallingObjectId = 0)
    {
        try
        {
            if (string.IsNullOrEmpty(Strings.Trim(scriptLine)))
            {
                return;
            }

            if (_gameFinished)
            {
                return;
            }

            if (Strings.InStr(scriptLine, Constants.vbCrLf) > 0)
            {
                var curPos = 1;
                var finished = false;
                do
                {
                    var crLfPos = Strings.InStr(curPos, scriptLine, Constants.vbCrLf);
                    if (crLfPos == 0)
                    {
                        finished = true;
                        crLfPos = Strings.Len(scriptLine) + 1;
                    }

                    var curScriptLine = Strings.Trim(Strings.Mid(scriptLine, curPos, crLfPos - curPos));
                    if ((curScriptLine ?? "") != Constants.vbCrLf)
                    {
                        await ExecuteScript(curScriptLine, ctx);
                    }

                    curPos = crLfPos + 2;
                } while (!finished);

                return;
            }

            if (newCallingObjectId != 0)
            {
                ctx.CallingObjectId = newCallingObjectId;
            }

            if (BeginsWith(scriptLine, "if "))
            {
                await ExecuteIf(scriptLine, ctx);
            }
            else if (BeginsWith(scriptLine, "select case "))
            {
                await ExecuteSelectCase(scriptLine, ctx);
            }
            else if (BeginsWith(scriptLine, "choose "))
            {
                await ExecuteChoose(await GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "set "))
            {
                await ExecuteSet(GetEverythingAfter(scriptLine, "set "), ctx);
            }
            else if (BeginsWith(scriptLine, "inc ") | BeginsWith(scriptLine, "dec "))
            {
                await ExecuteIncDec(scriptLine, ctx);
            }
            else if (BeginsWith(scriptLine, "say "))
            {
                await Print('"' + await GetParameter(scriptLine, ctx) + '"', ctx);
            }
            else if (BeginsWith(scriptLine, "do "))
            {
                await ExecuteDo(await GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "doaction "))
            {
                await ExecuteDoAction(await GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "give "))
            {
                await PlayerItem(await GetParameter(scriptLine, ctx), true, ctx);
            }
            else if (BeginsWith(scriptLine, "lose ") | BeginsWith(scriptLine, "drop "))
            {
                await PlayerItem(await GetParameter(scriptLine, ctx), false, ctx);
            }
            else if (BeginsWith(scriptLine, "msg "))
            {
                await Print(await GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "speak "))
            {
                Speak(await GetParameter(scriptLine, ctx));
            }
            else if (BeginsWith(scriptLine, "helpmsg "))
            {
                await Print(await GetParameter(scriptLine, ctx), ctx);
            }
            else if (Strings.Trim(Strings.LCase(scriptLine)) == "helpclose")
            {
            }
            // This command does nothing in the Quest 5 player, as there is no separate help window
            else if (BeginsWith(scriptLine, "goto "))
            {
                await PlayGame(await GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "playerwin"))
            {
                await FinishGame(StopType.Win, ctx);
            }
            else if (BeginsWith(scriptLine, "playerlose"))
            {
                await FinishGame(StopType.Lose, ctx);
            }
            else if (Strings.Trim(Strings.LCase(scriptLine)) == "stop")
            {
                await FinishGame(StopType.Null, ctx);
            }
            else if (BeginsWith(scriptLine, "playwav "))
            {
                await PlayWavAsync(await GetParameter(scriptLine, ctx));
            }
            else if (BeginsWith(scriptLine, "playmidi "))
            {
                await PlayMediaAsync(await GetParameter(scriptLine, ctx));
            }
            else if (BeginsWith(scriptLine, "playmp3 "))
            {
                await PlayMediaAsync(await GetParameter(scriptLine, ctx));
            }
            else if (Strings.Trim(Strings.LCase(scriptLine)) == "picture close")
            {
            }
            // This command does nothing in the Quest 5 player, as there is no separate picture window
            else if (((ASLVersion >= 390) & BeginsWith(scriptLine, "picture popup ")) |
                     ((ASLVersion >= 282) & (ASLVersion < 390) & BeginsWith(scriptLine, "picture ")) |
                     ((ASLVersion < 282) & BeginsWith(scriptLine, "show ")))
            {
                await ShowPicture(await GetParameter(scriptLine, ctx));
            }
            else if ((ASLVersion >= 390) & BeginsWith(scriptLine, "picture "))
            {
                await ShowPictureInTextAsync(await GetParameter(scriptLine, ctx));
            }
            else if (BeginsWith(scriptLine, "animate persist "))
            {
                await ShowPicture(await GetParameter(scriptLine, ctx));
            }
            else if (BeginsWith(scriptLine, "animate "))
            {
                await ShowPicture(await GetParameter(scriptLine, ctx));
            }
            else if (BeginsWith(scriptLine, "extract "))
            {
                ExtractFile(await GetParameter(scriptLine, ctx));
            }
            else if ((ASLVersion < 281) & BeginsWith(scriptLine, "hideobject "))
            {
                await SetAvailability(await GetParameter(scriptLine, ctx), false, ctx);
            }
            else if ((ASLVersion < 281) & BeginsWith(scriptLine, "showobject "))
            {
                await SetAvailability(await GetParameter(scriptLine, ctx), true, ctx);
            }
            else if ((ASLVersion < 281) & BeginsWith(scriptLine, "moveobject "))
            {
                await ExecMoveThing(await GetParameter(scriptLine, ctx), Thing.Object, ctx);
            }
            else if ((ASLVersion < 281) & BeginsWith(scriptLine, "hidechar "))
            {
                await SetAvailability(await GetParameter(scriptLine, ctx), false, ctx, Thing.Character);
            }
            else if ((ASLVersion < 281) & BeginsWith(scriptLine, "showchar "))
            {
                await SetAvailability(await GetParameter(scriptLine, ctx), true, ctx, Thing.Character);
            }
            else if ((ASLVersion < 281) & BeginsWith(scriptLine, "movechar "))
            {
                await ExecMoveThing(await GetParameter(scriptLine, ctx), Thing.Character, ctx);
            }
            else if ((ASLVersion < 281) & BeginsWith(scriptLine, "revealobject "))
            {
                await SetVisibility(await GetParameter(scriptLine, ctx), Thing.Object, true, ctx);
            }
            else if ((ASLVersion < 281) & BeginsWith(scriptLine, "concealobject "))
            {
                await SetVisibility(await GetParameter(scriptLine, ctx), Thing.Object, false, ctx);
            }
            else if ((ASLVersion < 281) & BeginsWith(scriptLine, "revealchar "))
            {
                await SetVisibility(await GetParameter(scriptLine, ctx), Thing.Character, true, ctx);
            }
            else if ((ASLVersion < 281) & BeginsWith(scriptLine, "concealchar "))
            {
                await SetVisibility(await GetParameter(scriptLine, ctx), Thing.Character, false, ctx);
            }
            else if ((ASLVersion >= 281) & BeginsWith(scriptLine, "hide "))
            {
                await SetAvailability(await GetParameter(scriptLine, ctx), false, ctx);
            }
            else if ((ASLVersion >= 281) & BeginsWith(scriptLine, "show "))
            {
                await SetAvailability(await GetParameter(scriptLine, ctx), true, ctx);
            }
            else if ((ASLVersion >= 281) & BeginsWith(scriptLine, "move "))
            {
                await ExecMoveThing(await GetParameter(scriptLine, ctx), Thing.Object, ctx);
            }
            else if ((ASLVersion >= 281) & BeginsWith(scriptLine, "reveal "))
            {
                await SetVisibility(await GetParameter(scriptLine, ctx), Thing.Object, true, ctx);
            }
            else if ((ASLVersion >= 281) & BeginsWith(scriptLine, "conceal "))
            {
                await SetVisibility(await GetParameter(scriptLine, ctx), Thing.Object, false, ctx);
            }
            else if ((ASLVersion >= 391) & BeginsWith(scriptLine, "open "))
            {
                await SetOpenClose(await GetParameter(scriptLine, ctx), true, ctx);
            }
            else if ((ASLVersion >= 391) & BeginsWith(scriptLine, "close "))
            {
                await SetOpenClose(await GetParameter(scriptLine, ctx), false, ctx);
            }
            else if ((ASLVersion >= 391) & BeginsWith(scriptLine, "add "))
            {
                await ExecAddRemoveScript(await GetParameter(scriptLine, ctx), true, ctx);
            }
            else if ((ASLVersion >= 391) & BeginsWith(scriptLine, "remove "))
            {
                await ExecAddRemoveScript(await GetParameter(scriptLine, ctx), false, ctx);
            }
            else if (BeginsWith(scriptLine, "clone "))
            {
                await ExecClone(await GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "exec "))
            {
                await ExecExec(scriptLine, ctx);
            }
            else if (BeginsWith(scriptLine, "setstring "))
            {
                await ExecSetString(await GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "setvar "))
            {
                await ExecSetVar(await GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "for "))
            {
                await ExecFor(GetEverythingAfter(scriptLine, "for "), ctx);
            }
            else if (BeginsWith(scriptLine, "property "))
            {
                await ExecProperty(await GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "type "))
            {
                await ExecType(await GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "action "))
            {
                await ExecuteAction(GetEverythingAfter(scriptLine, "action "), ctx);
            }
            else if (BeginsWith(scriptLine, "flag "))
            {
                await ExecuteFlag(GetEverythingAfter(scriptLine, "flag "), ctx);
            }
            else if (BeginsWith(scriptLine, "create "))
            {
                await ExecuteCreate(GetEverythingAfter(scriptLine, "create "), ctx);
            }
            else if (BeginsWith(scriptLine, "destroy exit "))
            {
                await DestroyExit(await GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "repeat "))
            {
                await ExecuteRepeat(GetEverythingAfter(scriptLine, "repeat "), ctx);
            }
            else if (BeginsWith(scriptLine, "enter "))
            {
                await ExecuteEnterAsync(scriptLine, ctx);
            }
            else if (BeginsWith(scriptLine, "displaytext "))
            {
                await DisplayTextSection(await GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "helpdisplaytext "))
            {
                await DisplayTextSection(await GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "font "))
            {
                SetFont(await GetParameter(scriptLine, ctx));
            }
            else if (BeginsWith(scriptLine, "pause "))
            {
                await PauseAsync(Conversions.ToInteger(await GetParameter(scriptLine, ctx)));
            }
            else if (Strings.Trim(Strings.LCase(scriptLine)) == "clear")
            {
                DoClear();
            }
            else if (Strings.Trim(Strings.LCase(scriptLine)) == "helpclear")
            {
            }
            // This command does nothing in the Quest 5 player, as there is no separate help window
            else if (BeginsWith(scriptLine, "background "))
            {
                SetBackground(await GetParameter(scriptLine, ctx));
            }
            else if (BeginsWith(scriptLine, "foreground "))
            {
                SetForeground(await GetParameter(scriptLine, ctx));
            }
            else if (Strings.Trim(Strings.LCase(scriptLine)) == "nointro")
            {
                _autoIntro = false;
            }
            else if (BeginsWith(scriptLine, "debug "))
            {
                LogASLError(await GetParameter(scriptLine, ctx));
            }
            else if (BeginsWith(scriptLine, "mailto "))
            {
                var emailAddress = await GetParameter(scriptLine, ctx);
                PrintText?.Invoke("<a target=\"_blank\" href=\"mailto:" + emailAddress + "\">" + emailAddress + "</a>");
            }
            else if (BeginsWith(scriptLine, "shell ") & (ASLVersion < 410))
            {
                LogASLError("'shell' is not supported in this version of Quest", LogType.WarningError);
            }
            else if (BeginsWith(scriptLine, "shellexe ") & (ASLVersion < 410))
            {
                LogASLError("'shellexe' is not supported in this version of Quest", LogType.WarningError);
            }
            else if (BeginsWith(scriptLine, "wait"))
            {
                await ExecuteWaitAsync(Strings.Trim(GetEverythingAfter(Strings.Trim(scriptLine), "wait")), ctx);
            }
            else if (BeginsWith(scriptLine, "timeron "))
            {
                SetTimerState(await GetParameter(scriptLine, ctx), true);
            }
            else if (BeginsWith(scriptLine, "timeroff "))
            {
                SetTimerState(await GetParameter(scriptLine, ctx), false);
            }
            else if (Strings.Trim(Strings.LCase(scriptLine)) == "outputon")
            {
                _outPutOn = true;
                await UpdateObjectList(ctx);
                await UpdateItems(ctx);
            }
            else if (Strings.Trim(Strings.LCase(scriptLine)) == "outputoff")
            {
                _outPutOn = false;
            }
            else if (Strings.Trim(Strings.LCase(scriptLine)) == "panes off")
            {
                _player.SetPanesVisible("off");
            }
            else if (Strings.Trim(Strings.LCase(scriptLine)) == "panes on")
            {
                _player.SetPanesVisible("on");
            }
            else if (BeginsWith(scriptLine, "lock "))
            {
                await ExecuteLock(await GetParameter(scriptLine, ctx), true);
            }
            else if (BeginsWith(scriptLine, "unlock "))
            {
                await ExecuteLock(await GetParameter(scriptLine, ctx), false);
            }
            else if (BeginsWith(scriptLine, "playmod ") & (ASLVersion < 410))
            {
                LogASLError("'playmod' is not supported in this version of Quest", LogType.WarningError);
            }
            else if (BeginsWith(scriptLine, "modvolume") & (ASLVersion < 410))
            {
                LogASLError("'modvolume' is not supported in this version of Quest", LogType.WarningError);
            }
            else if (Strings.Trim(Strings.LCase(scriptLine)) == "dontprocess")
            {
                ctx.DontProcessCommand = true;
            }
            else if (BeginsWith(scriptLine, "return "))
            {
                ctx.FunctionReturnValue = await GetParameter(scriptLine, ctx);
            }
            else if (!BeginsWith(scriptLine, "'"))
            {
                LogASLError("Unrecognized keyword. Line reads: '" + Strings.Trim(ReportErrorLine(scriptLine)) + "'",
                    LogType.WarningError);
            }
        }
        catch
        {
            await Print("[An internal error occurred]", ctx);
            LogASLError(
                Information.Err().Number + " - '" + Information.Err().Description +
                "' occurred processing script line '" + scriptLine + "'", LogType.InternalError);
        }
    }

    private async Task ExecuteSet(string setInstruction, Context ctx)
    {
        if (ASLVersion >= 280)
        {
            if (BeginsWith(setInstruction, "interval "))
            {
                var interval = await GetParameter(setInstruction, ctx);
                var scp = Strings.InStr(interval, ";");
                if (scp == 0)
                {
                    LogASLError("Too few parameters in 'set " + setInstruction + "'", LogType.WarningError);
                    return;
                }

                var name = Strings.Trim(Strings.Left(interval, scp - 1));
                interval = Conversion.Val(Strings.Trim(Strings.Mid(interval, scp + 1))).ToString(CultureInfo.InvariantCulture);
                var found = false;

                for (int i = 1, loopTo = _numberTimers; i <= loopTo; i++)
                {
                    if ((Strings.LCase(name) ?? "") == (Strings.LCase(_timers[i].TimerName) ?? ""))
                    {
                        found = true;
                        _timers[i].TimerInterval = Conversions.ToInteger(interval);
                        i = _numberTimers;
                    }
                }

                if (!found)
                {
                    LogASLError("No such timer '" + name + "'", LogType.WarningError);
                }
            }
            else if (BeginsWith(setInstruction, "string "))
            {
                await ExecSetString(await GetParameter(setInstruction, ctx), ctx);
            }
            else if (BeginsWith(setInstruction, "numeric "))
            {
                await ExecSetVar(await GetParameter(setInstruction, ctx), ctx);
            }
            else if (BeginsWith(setInstruction, "collectable "))
            {
                ExecuteSetCollectable(await GetParameter(setInstruction, ctx), ctx);
            }
            else
            {
                var result = await SetUnknownVariableType(await GetParameter(setInstruction, ctx), ctx);
                if (result == SetResult.Error)
                {
                    LogASLError("Error on setting 'set " + setInstruction + "'", LogType.WarningError);
                }
                else if (result == SetResult.Unfound)
                {
                    LogASLError("Variable type not specified in 'set " + setInstruction + "'", LogType.WarningError);
                }
            }
        }
        else
        {
            ExecuteSetCollectable(await GetParameter(setInstruction, ctx), ctx);
        }
    }
}
