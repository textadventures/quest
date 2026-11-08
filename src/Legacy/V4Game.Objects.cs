using System.Diagnostics;
using System.Text;
using Microsoft.VisualBasic;
using QuestViva.Common;
using Constants = Microsoft.VisualBasic.Constants;
using Strings = Microsoft.VisualBasic.Strings;

namespace QuestViva.Legacy;

public partial class V4Game
{
    private async Task<string> ListContents(int id, Context ctx)
    {
        // Returns a formatted list of the contents of a container.
        // If the list action causes a script to be run instead, ListContents
        // returns "<script>"

        var contentsIDs = new int[1];

        if (!IsYes(GetObjectProperty("container", id, true, false)))
        {
            return "";
        }

        if (!IsYes(GetObjectProperty("opened", id, true, false)) &
            !IsYes(GetObjectProperty("transparent", id, true, false)) &
            !IsYes(GetObjectProperty("surface", id, true, false)))
        {
            // Container is closed, so return "list closed" property if there is one.

            if (await DoAction(id, "list closed", ctx, false))
            {
                return "<script>";
            }

            return GetObjectProperty("list closed", id, false, false);
        }

        // populate contents string

        var numContents = 0;

        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if (_objs[i].Exists & _objs[i].Visible)
            {
                if ((Strings.LCase(GetObjectProperty("parent", i, false, false)) ?? "") ==
                    (Strings.LCase(_objs[id].ObjectName) ?? ""))
                {
                    numContents = numContents + 1;
                    Array.Resize(ref contentsIDs, numContents + 1);
                    contentsIDs[numContents] = i;
                }
            }
        }

        var contents = "";

        if (numContents > 0)
        {
            // Check if list property is set.

            if (await DoAction(id, "list", ctx, false))
            {
                return "<script>";
            }

            if (IsYes(GetObjectProperty("list", id, true, false)))
            {
                // Read header, if any
                var listString = GetObjectProperty("list", id, false, false);
                var displayList = true;

                if (!string.IsNullOrEmpty(listString))
                {
                    if (Strings.Right(listString, 1) == ":")
                    {
                        contents = Strings.Left(listString, Strings.Len(listString) - 1) + " ";
                    }
                    else
                    {
                        // If header doesn't end in a colon, then the header is the only text to print
                        contents = listString;
                        displayList = false;
                    }
                }
                else
                {
                    contents = Strings.UCase(Strings.Left(_objs[id].Article, 1)) + Strings.Mid(_objs[id].Article, 2) +
                               " contains ";
                }

                if (displayList)
                {
                    for (int i = 1, loopTo1 = numContents; i <= loopTo1; i++)
                    {
                        if (i > 1)
                        {
                            if (i < numContents)
                            {
                                contents = contents + ", ";
                            }
                            else
                            {
                                contents = contents + " and ";
                            }
                        }

                        var o = _objs[contentsIDs[i]];
                        if (!string.IsNullOrEmpty(o.Prefix))
                        {
                            contents = contents + o.Prefix;
                        }

                        if (!string.IsNullOrEmpty(o.ObjectAlias))
                        {
                            contents = contents + "|b" + o.ObjectAlias + "|xb";
                        }
                        else
                        {
                            contents = contents + "|b" + o.ObjectName + "|xb";
                        }

                        if (!string.IsNullOrEmpty(o.Suffix))
                        {
                            contents = contents + " " + o.Suffix;
                        }
                    }
                }

                return contents + ".";
            }

            // The "list" property is not set, so do not list contents.
            return "";
        }

        // Container is empty, so return "list empty" property if there is one.

        if (await DoAction(id, "list empty", ctx, false))
        {
            return "<script>";
        }

        return GetObjectProperty("list empty", id, false, false);
    }

    private async Task AddObjectAction(int id, string name, string script, bool noUpdate = false)
    {
        // Use NoUpdate in e.g. AddToGiveInfo, otherwise ObjectActionUpdate will call
        // AddToGiveInfo again leading to a big loop

        var actionNum = default(int);
        var foundExisting = false;

        var o = _objs[id];

        for (int i = 1, loopTo = o.NumberActions; i <= loopTo; i++)
        {
            if ((o.Actions[i].ActionName ?? "") == (name ?? ""))
            {
                foundExisting = true;
                actionNum = i;
                break;
            }
        }

        if (!foundExisting)
        {
            o.NumberActions = o.NumberActions + 1;
            Array.Resize(ref o.Actions, o.NumberActions + 1);
            o.Actions[o.NumberActions] = new ActionType();
            actionNum = o.NumberActions;
        }

        o.Actions[actionNum].ActionName = name;
        o.Actions[actionNum].Script = script;

        await ObjectActionUpdate(id, name, script, noUpdate);
    }

    private async Task AddToGiveInfo(int id, string giveData)
    {
        GiveType giveType;
        string actionName;
        string actionScript;

        var o = _objs[id];

        if (BeginsWith(giveData, "to "))
        {
            giveData = GetEverythingAfter(giveData, "to ");
            if (BeginsWith(giveData, "anything "))
            {
                o.GiveToAnything = GetEverythingAfter(giveData, "anything ");
                await AddObjectAction(id, "give to anything", o.GiveToAnything, true);
                return;
            }

            giveType = GiveType.GiveToSomething;
            actionName = "give to ";
        }
        else if (BeginsWith(giveData, "anything "))
        {
            o.GiveAnything = GetEverythingAfter(giveData, "anything ");

            await AddObjectAction(id, "give anything", o.GiveAnything, true);
            return;
        }
        else
        {
            giveType = GiveType.GiveSomethingTo;
            actionName = "give ";
        }

        if (Strings.Left(Strings.Trim(giveData), 1) == "<")
        {
            var name = await GetParameter(giveData, _nullContext);
            var dataId = default(int);

            actionName = actionName + "'" + name + "'";

            var found = false;
            for (int i = 1, loopTo = o.NumberGiveData; i <= loopTo; i++)
            {
                if ((o.GiveData[i].GiveType == giveType) &
                    ((Strings.LCase(o.GiveData[i].GiveObject) ?? "") == (Strings.LCase(name) ?? "")))
                {
                    dataId = i;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                o.NumberGiveData = o.NumberGiveData + 1;
                Array.Resize(ref o.GiveData, o.NumberGiveData + 1);
                o.GiveData[o.NumberGiveData] = new GiveDataType();
                dataId = o.NumberGiveData;
            }

            var EP = Strings.InStr(giveData, ">");
            o.GiveData[dataId].GiveType = giveType;
            o.GiveData[dataId].GiveObject = name;
            o.GiveData[dataId].GiveScript = Strings.Mid(giveData, EP + 2);

            actionScript = o.GiveData[dataId].GiveScript;
            await AddObjectAction(id, actionName, actionScript, true);
        }
    }

    internal async Task AddToObjectActions(string actionInfo, int id, Context ctx)
    {
        var actionNum = default(int);
        var foundExisting = false;

        var name = Strings.LCase(await GetParameter(actionInfo, ctx));
        var ep = Strings.InStr(actionInfo, ">");
        if (ep == Strings.Len(actionInfo))
        {
            LogASLError("No script given for '" + name + "' action data", LogType.WarningError);
            return;
        }

        var script = Strings.Trim(Strings.Mid(actionInfo, ep + 1));

        var o = _objs[id];

        for (int i = 1, loopTo = o.NumberActions; i <= loopTo; i++)
        {
            if ((o.Actions[i].ActionName ?? "") == (name ?? ""))
            {
                foundExisting = true;
                actionNum = i;
                break;
            }
        }

        if (!foundExisting)
        {
            o.NumberActions = o.NumberActions + 1;
            Array.Resize(ref o.Actions, o.NumberActions + 1);
            o.Actions[o.NumberActions] = new ActionType();
            actionNum = o.NumberActions;
        }

        o.Actions[actionNum].ActionName = name;
        o.Actions[actionNum].Script = script;

        await ObjectActionUpdate(id, name, script);
    }

    private void AddToObjectAltNames(string altNames, int id)
    {
        var o = _objs[id];

        do
        {
            var endPos = Strings.InStr(altNames, ";");
            if (endPos == 0)
            {
                endPos = Strings.Len(altNames) + 1;
            }

            var curName = Strings.Trim(Strings.Left(altNames, endPos - 1));

            if (!string.IsNullOrEmpty(curName))
            {
                o.NumberAltNames = o.NumberAltNames + 1;
                Array.Resize(ref o.AltNames, o.NumberAltNames + 1);
                o.AltNames[o.NumberAltNames] = curName;
            }

            altNames = Strings.Mid(altNames, endPos + 1);
        } while (!string.IsNullOrEmpty(Strings.Trim(altNames)));
    }

    internal async Task AddToObjectProperties(string propertyInfo, int id, Context ctx)
    {
        if (id == 0)
        {
            return;
        }

        if (Strings.Right(propertyInfo, 1) != ";")
        {
            propertyInfo = propertyInfo + ";";
        }

        var num = default(int);
        do
        {
            var scp = Strings.InStr(propertyInfo, ";");
            var info = Strings.Left(propertyInfo, scp - 1);
            propertyInfo = Strings.Trim(Strings.Mid(propertyInfo, scp + 1));

            string name, value;

            if (string.IsNullOrEmpty(info))
            {
                break;
            }

            var ep = Strings.InStr(info, "=");
            if (ep != 0)
            {
                name = Strings.Trim(Strings.Left(info, ep - 1));
                value = Strings.Trim(Strings.Mid(info, ep + 1));
            }
            else
            {
                name = info;
                value = "";
            }

            var falseProperty = false;
            if (BeginsWith(name, "not ") & string.IsNullOrEmpty(value))
            {
                falseProperty = true;
                name = GetEverythingAfter(name, "not ");
            }

            var o = _objs[id];

            var found = false;
            for (int i = 1, loopTo = o.NumberProperties; i <= loopTo; i++)
            {
                if ((Strings.LCase(o.Properties[i].PropertyName) ?? "") == (Strings.LCase(name) ?? ""))
                {
                    found = true;
                    num = i;
                    i = o.NumberProperties;
                }
            }

            if (!found)
            {
                o.NumberProperties = o.NumberProperties + 1;
                Array.Resize(ref o.Properties, o.NumberProperties + 1);
                o.Properties[o.NumberProperties] = new PropertyType();
                num = o.NumberProperties;
            }

            if (falseProperty)
            {
                o.Properties[num].PropertyName = "";
            }
            else
            {
                o.Properties[num].PropertyName = name;
                o.Properties[num].PropertyValue = value;
            }

            AddToObjectChangeLog(ChangeLog.AppliesTo.Object, _objs[id].ObjectName, name, "properties " + info);

            switch (name ?? "")
            {
                case "alias":
                {
                    if (o.IsRoom)
                    {
                        _rooms[o.CorresRoomId].RoomAlias = value;
                    }
                    else
                    {
                        o.ObjectAlias = value;
                    }

                    if (_gameFullyLoaded)
                    {
                        _ = UpdateObjectList(ctx);
                        _ = UpdateItems(ctx);
                    }

                    break;
                }
                case "prefix":
                {
                    if (o.IsRoom)
                    {
                        _rooms[o.CorresRoomId].Prefix = value;
                    }
                    else if (!string.IsNullOrEmpty(value))
                    {
                        o.Prefix = value + " ";
                    }
                    else
                    {
                        o.Prefix = "";
                    }

                    break;
                }
                case "indescription":
                {
                    if (o.IsRoom)
                    {
                        _rooms[o.CorresRoomId].InDescription = value;
                    }

                    break;
                }
                case "description":
                {
                    if (o.IsRoom)
                    {
                        _rooms[o.CorresRoomId].Description.Data = value;
                        _rooms[o.CorresRoomId].Description.Type = TextActionType.Text;
                    }

                    break;
                }
                case "look":
                {
                    if (o.IsRoom)
                    {
                        _rooms[o.CorresRoomId].Look = value;
                    }

                    break;
                }
                case "suffix":
                {
                    o.Suffix = value;
                    break;
                }
                case "displaytype":
                {
                    o.DisplayType = value;
                    if (_gameFullyLoaded)
                    {
                        await UpdateObjectList(ctx);
                    }

                    break;
                }
                case "gender":
                {
                    o.Gender = value;
                    break;
                }
                case "article":
                {
                    o.Article = value;
                    break;
                }
                case "detail":
                {
                    o.Detail = value;
                    break;
                }
                case "hidden":
                {
                    if (falseProperty)
                    {
                        o.Exists = true;
                    }
                    else
                    {
                        o.Exists = false;
                    }

                    if (_gameFullyLoaded)
                    {
                        await UpdateObjectList(ctx);
                    }

                    break;
                }
                case "invisible":
                {
                    if (falseProperty)
                    {
                        o.Visible = true;
                    }
                    else
                    {
                        o.Visible = false;
                    }

                    if (_gameFullyLoaded)
                    {
                        await UpdateObjectList(ctx);
                    }

                    break;
                }
                case "take":
                {
                    if (ASLVersion >= 392)
                    {
                        if (falseProperty)
                        {
                            o.Take.Type = TextActionType.Nothing;
                        }
                        else if (string.IsNullOrEmpty(value))
                        {
                            o.Take.Type = TextActionType.Default;
                        }
                        else
                        {
                            o.Take.Type = TextActionType.Text;
                            o.Take.Data = value;
                        }
                    }

                    break;
                }
            }
        } while (Strings.Len(Strings.Trim(propertyInfo)) != 0);
    }

    private async Task AddToUseInfo(int id, string useData)
    {
        UseType useType;

        var o = _objs[id];

        if (BeginsWith(useData, "on "))
        {
            useData = GetEverythingAfter(useData, "on ");
            if (BeginsWith(useData, "anything "))
            {
                o.UseOnAnything = GetEverythingAfter(useData, "anything ");
                return;
            }

            useType = UseType.UseOnSomething;
        }
        else if (BeginsWith(useData, "anything "))
        {
            o.UseAnything = GetEverythingAfter(useData, "anything ");
            return;
        }
        else
        {
            useType = UseType.UseSomethingOn;
        }

        if (Strings.Left(Strings.Trim(useData), 1) == "<")
        {
            var objectName = await GetParameter(useData, _nullContext);
            var dataId = default(int);
            var found = false;

            for (int i = 1, loopTo = o.NumberUseData; i <= loopTo; i++)
            {
                if ((o.UseData[i].UseType == useType) &
                    ((Strings.LCase(o.UseData[i].UseObject) ?? "") == (Strings.LCase(objectName) ?? "")))
                {
                    dataId = i;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                o.NumberUseData = o.NumberUseData + 1;
                Array.Resize(ref o.UseData, o.NumberUseData + 1);
                o.UseData[o.NumberUseData] = new UseDataType();
                dataId = o.NumberUseData;
            }

            var ep = Strings.InStr(useData, ">");
            o.UseData[dataId].UseType = useType;
            o.UseData[dataId].UseObject = objectName;
            o.UseData[dataId].UseScript = Strings.Mid(useData, ep + 2);
        }
        else
        {
            o.Use = Strings.Trim(useData);
        }
    }

    private async Task ExecClone(string cloneString, Context ctx)
    {
        int id;
        string newName, cloneTo;

        var scp = Strings.InStr(cloneString, ";");
        if (scp == 0)
        {
            LogASLError("No new object name specified in 'clone <" + cloneString + ">", LogType.WarningError);
            return;
        }

        var objectToClone = Strings.Trim(Strings.Left(cloneString, scp - 1));
        id = GetObjectIdNoAlias(objectToClone);

        var SC2 = Strings.InStr(scp + 1, cloneString, ";");
        if (SC2 == 0)
        {
            cloneTo = _objs[id].ContainerRoom;
            newName = Strings.Trim(Strings.Mid(cloneString, scp + 1));
        }
        else
        {
            cloneTo = Strings.Trim(Strings.Mid(cloneString, SC2 + 1));
            newName = Strings.Trim(Strings.Mid(cloneString, scp + 1, SC2 - scp - 1));
        }

        _numberObjs = _numberObjs + 1;
        Array.Resize(ref _objs, _numberObjs + 1);
        _objs[_numberObjs] = _objs[id].Copy();
        _objs[_numberObjs].ContainerRoom = cloneTo;
        _objs[_numberObjs].ObjectName = newName;

        if (_objs[id].IsRoom)
        {
            // This is a room so create the corresponding room as well

            _numberRooms = _numberRooms + 1;
            Array.Resize(ref _rooms, _numberRooms + 1);
            _rooms[_numberRooms] = _rooms[_objs[id].CorresRoomId].Copy();
            _rooms[_numberRooms].RoomName = newName;
            _rooms[_numberRooms].ObjId = _numberObjs;

            _objs[_numberObjs].CorresRoom = newName;
            _objs[_numberObjs].CorresRoomId = _numberRooms;

            AddToChangeLog("room " + newName, "create");
        }
        else
        {
            AddToChangeLog("object " + newName, "create " + _objs[_numberObjs].ContainerRoom);
        }

        await UpdateObjectList(ctx);
    }

    internal async Task<bool> DoAction(int id, string action, Context ctx, bool logError = true)
    {
        var found = default(bool);
        var script = "";

        var o = _objs[id];

        for (int i = 1, loopTo = o.NumberActions; i <= loopTo; i++)
        {
            if ((o.Actions[i].ActionName ?? "") == (Strings.LCase(action) ?? ""))
            {
                found = true;
                script = o.Actions[i].Script;
                break;
            }
        }

        if (!found)
        {
            if (logError)
            {
                LogASLError("No such action '" + action + "' defined for object '" + o.ObjectName + "'");
            }

            return false;
        }

        var newCtx = CopyContext(ctx);
        newCtx.CallingObjectId = id;

        await ExecuteScript(script, newCtx, id);

        return true;
    }

    public bool HasAction(int id, string action)
    {
        var o = _objs[id];

        for (int i = 1, loopTo = o.NumberActions; i <= loopTo; i++)
        {
            if ((o.Actions[i].ActionName ?? "") == (Strings.LCase(action) ?? ""))
            {
                return true;
            }
        }

        return false;
    }

    private async Task ExecuteAction(string data, Context ctx)
    {
        string actionName;
        string script;
        var actionNum = default(int);
        var id = default(int);
        var foundExisting = false;
        var foundObject = false;

        var param = await GetParameter(data, ctx);
        var scp = Strings.InStr(param, ";");
        if (scp == 0)
        {
            LogASLError("No action name specified in 'action " + data + "'", LogType.WarningError);
            return;
        }

        var objName = Strings.Trim(Strings.Left(param, scp - 1));
        actionName = Strings.Trim(Strings.Mid(param, scp + 1));

        var ep = Strings.InStr(data, ">");
        if (ep == Strings.Len(Strings.Trim(data)))
        {
            script = "";
        }
        else
        {
            script = Strings.Trim(Strings.Mid(data, ep + 1));
        }

        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(objName) ?? ""))
            {
                foundObject = true;
                id = i;
                break;
            }
        }

        if (!foundObject)
        {
            LogASLError("No such object '" + objName + "' in 'action " + data + "'", LogType.WarningError);
            return;
        }

        var o = _objs[id];

        for (int i = 1, loopTo1 = o.NumberActions; i <= loopTo1; i++)
        {
            if ((o.Actions[i].ActionName ?? "") == (actionName ?? ""))
            {
                foundExisting = true;
                actionNum = i;
                break;
            }
        }

        if (!foundExisting)
        {
            o.NumberActions = o.NumberActions + 1;
            Array.Resize(ref o.Actions, o.NumberActions + 1);
            o.Actions[o.NumberActions] = new ActionType();
            actionNum = o.NumberActions;
        }

        o.Actions[actionNum].ActionName = actionName;
        o.Actions[actionNum].Script = script;

        await ObjectActionUpdate(id, actionName, script);
    }

    private void ExecuteSetCollectable(string param, Context ctx)
    {
        double val;
        var id = default(int);
        var scp = Strings.InStr(param, ";");
        var name = Strings.Trim(Strings.Left(param, scp - 1));
        var newVal = Strings.Trim(Strings.Right(param, Strings.Len(param) - scp));
        var found = false;

        for (int i = 1, loopTo = _numCollectables; i <= loopTo; i++)
        {
            if ((_collectables[i].Name ?? "") == (name ?? ""))
            {
                id = i;
                found = true;
                break;
            }
        }

        if (!found)
        {
            LogASLError("No such collectable '" + param + "'", LogType.WarningError);
            return;
        }

        var op = Strings.Left(newVal, 1);
        var newValue = Strings.Trim(Strings.Right(newVal, Strings.Len(newVal) - 1));
        if (Information.IsNumeric(newValue))
        {
            val = Conversion.Val(newValue);
        }
        else
        {
            val = GetCollectableAmount(newValue);
        }

        if (op == "+")
        {
            _collectables[id].Value = _collectables[id].Value + val;
        }
        else if (op == "-")
        {
            _collectables[id].Value = _collectables[id].Value - val;
        }
        else if (op == "=")
        {
            _collectables[id].Value = val;
        }

        CheckCollectable(id);
        _ = UpdateItems(ctx);
    }

    private async Task<ActionType> GetObjectActions(string actionInfo)
    {
        var name = Strings.LCase(await GetParameter(actionInfo, _nullContext));
        var ep = Strings.InStr(actionInfo, ">");
        if (ep == Strings.Len(actionInfo))
        {
            LogASLError("No script given for '" + name + "' action data", LogType.WarningError);
            return new ActionType();
        }

        var script = Strings.Trim(Strings.Mid(actionInfo, ep + 1));
        var result = new ActionType();
        result.ActionName = name;
        result.Script = script;
        return result;
    }

    private async Task<int> GetObjectId(string name, Context ctx, string room = "")
    {
        var id = default(int);
        var found = false;

        if (BeginsWith(name, "the "))
        {
            name = GetEverythingAfter(name, "the ");
        }

        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if ((((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(name) ?? "")) |
                 ((Strings.LCase(_objs[i].ObjectName) ?? "") == ("the " + Strings.LCase(name) ?? ""))) &
                (((Strings.LCase(_objs[i].ContainerRoom) ?? "") == (Strings.LCase(room) ?? "")) |
                 string.IsNullOrEmpty(room)) & _objs[i].Exists)
            {
                id = i;
                found = true;
                break;
            }
        }

        if (!found & (ASLVersion >= 280))
        {
            id = await Disambiguate(name, room, ctx);
            if (id > 0)
            {
                found = true;
            }
        }

        if (found)
        {
            return id;
        }

        return -1;
    }

    private int GetObjectIdNoAlias(string name)
    {
        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(name) ?? ""))
            {
                return i;
            }
        }

        return 0;
    }

    internal string GetObjectProperty(string name, int id, bool existsOnly = false, bool logError = true)
    {
        var result = "";
        var found = false;
        var o = _objs[id];

        for (int i = 1, loopTo = o.NumberProperties; i <= loopTo; i++)
        {
            if ((Strings.LCase(o.Properties[i].PropertyName) ?? "") == (Strings.LCase(name) ?? ""))
            {
                found = true;
                result = o.Properties[i].PropertyValue;
                break;
            }
        }

        if (existsOnly)
        {
            if (found)
            {
                return "yes";
            }

            return "no";
        }

        if (found)
        {
            return result;
        }

        if (logError)
        {
            LogASLError("Object '" + _objs[id].ObjectName + "' has no property '" + name + "'", LogType.WarningError);
            return "!";
        }

        return "";
    }

    private async Task<PropertiesActions> GetPropertiesInType(string type, bool err = true)
    {
        var blockId = default(int);
        var propertyList = new PropertiesActions();
        var found = false;

        for (int i = 1, loopTo = _numberSections; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[_defineBlocks[i].StartLine], "define type"))
            {
                if ((Strings.LCase(await GetParameter(_lines[_defineBlocks[i].StartLine], _nullContext)) ?? "") ==
                    (Strings.LCase(type) ?? ""))
                {
                    blockId = i;
                    found = true;
                    break;
                }
            }
        }

        if (!found)
        {
            if (err)
            {
                LogASLError("No such type '" + type + "'", LogType.WarningError);
            }

            return new PropertiesActions();
        }

        for (int i = _defineBlocks[blockId].StartLine + 1, loopTo1 = _defineBlocks[blockId].EndLine - 1;
             i <= loopTo1;
             i++)
        {
            if (BeginsWith(_lines[i], "type "))
            {
                var typeName = Strings.LCase(await GetParameter(_lines[i], _nullContext));
                var newProperties = await GetPropertiesInType(typeName);
                propertyList.Properties = propertyList.Properties + newProperties.Properties;
                Array.Resize(ref propertyList.Actions, propertyList.NumberActions + newProperties.NumberActions + 1);
                for (int j = propertyList.NumberActions + 1,
                     loopTo2 = propertyList.NumberActions + newProperties.NumberActions;
                     j <= loopTo2;
                     j++)
                {
                    propertyList.Actions[j] = new ActionType();
                    propertyList.Actions[j].ActionName =
                        newProperties.Actions[j - propertyList.NumberActions].ActionName;
                    propertyList.Actions[j].Script = newProperties.Actions[j - propertyList.NumberActions].Script;
                }

                propertyList.NumberActions = propertyList.NumberActions + newProperties.NumberActions;

                // Add this type name to the TypesIncluded list...
                propertyList.NumberTypesIncluded = propertyList.NumberTypesIncluded + 1;
                Array.Resize(ref propertyList.TypesIncluded, propertyList.NumberTypesIncluded + 1);
                propertyList.TypesIncluded[propertyList.NumberTypesIncluded] = typeName;

                // and add the names of the types included by it...
                Array.Resize(ref propertyList.TypesIncluded,
                    propertyList.NumberTypesIncluded + newProperties.NumberTypesIncluded + 1);
                for (int j = propertyList.NumberTypesIncluded + 1,
                     loopTo3 = propertyList.NumberTypesIncluded + newProperties.NumberTypesIncluded;
                     j <= loopTo3;
                     j++)
                {
                    propertyList.TypesIncluded[j] = newProperties.TypesIncluded[j - propertyList.NumberTypesIncluded];
                }

                propertyList.NumberTypesIncluded = propertyList.NumberTypesIncluded + newProperties.NumberTypesIncluded;
            }
            else if (BeginsWith(_lines[i], "action "))
            {
                propertyList.NumberActions = propertyList.NumberActions + 1;
                Array.Resize(ref propertyList.Actions, propertyList.NumberActions + 1);
                propertyList.Actions[propertyList.NumberActions] =
                    await GetObjectActions(GetEverythingAfter(_lines[i], "action "));
            }
            else if (BeginsWith(_lines[i], "properties "))
            {
                propertyList.Properties = propertyList.Properties + await GetParameter(_lines[i], _nullContext) + ";";
            }
            else if (!string.IsNullOrEmpty(Strings.Trim(_lines[i])))
            {
                propertyList.Properties = propertyList.Properties + _lines[i] + ";";
            }
        }

        return propertyList;
    }

    private int GetThingNumber(string name, string room, Thing type)
    {
        // Returns the number in the Chars() or _objs() array of the specified char/obj

        if (type == Thing.Character)
        {
            for (int i = 1, loopTo = _numberChars; i <= loopTo; i++)
            {
                if ((!string.IsNullOrEmpty(room) &
                     ((Strings.LCase(_chars[i].ObjectName) ?? "") == (Strings.LCase(name) ?? "")) &
                     ((Strings.LCase(_chars[i].ContainerRoom) ?? "") == (Strings.LCase(room) ?? ""))) |
                    (string.IsNullOrEmpty(room) &
                     ((Strings.LCase(_chars[i].ObjectName) ?? "") == (Strings.LCase(name) ?? ""))))
                {
                    return i;
                }
            }
        }
        else if (type == Thing.Object)
        {
            for (int i = 1, loopTo1 = _numberObjs; i <= loopTo1; i++)
            {
                if ((!string.IsNullOrEmpty(room) &
                     ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(name) ?? "")) &
                     ((Strings.LCase(_objs[i].ContainerRoom) ?? "") == (Strings.LCase(room) ?? ""))) |
                    (string.IsNullOrEmpty(room) &
                     ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(name) ?? ""))))
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private DefineBlock GetThingBlock(string name, string room, Thing type)
    {
        // Returns position where specified char/obj is defined in ASL code

        var result = new DefineBlock();

        if (type == Thing.Character)
        {
            for (int i = 1, loopTo = _numberChars; i <= loopTo; i++)
            {
                if (((Strings.LCase(_chars[i].ObjectName) ?? "") == (Strings.LCase(name) ?? "")) &
                    ((Strings.LCase(_chars[i].ContainerRoom) ?? "") == (Strings.LCase(room) ?? "")))
                {
                    result.StartLine = _chars[i].DefinitionSectionStart;
                    result.EndLine = _chars[i].DefinitionSectionEnd;
                    return result;
                }
            }
        }
        else if (type == Thing.Object)
        {
            for (int i = 1, loopTo1 = _numberObjs; i <= loopTo1; i++)
            {
                if (((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(name) ?? "")) &
                    ((Strings.LCase(_objs[i].ContainerRoom) ?? "") == (Strings.LCase(room) ?? "")))
                {
                    result.StartLine = _objs[i].DefinitionSectionStart;
                    result.EndLine = _objs[i].DefinitionSectionEnd;
                    return result;
                }
            }
        }

        result.StartLine = 0;
        result.EndLine = 0;
        return result;
    }

    private void MoveThing(string name, string room, Thing type, Context ctx)
    {
        var oldRoom = "";

        var id = GetThingNumber(name, "", type);

        if (Strings.InStr(room, "[") > 0)
        {
            var idx = GetArrayIndex(room, ctx);
            room = idx.Name + Strings.Trim(InvariantStrings.Str(idx.Index));
        }

        if (type == Thing.Character)
        {
            _chars[id].ContainerRoom = room;
        }
        else if (type == Thing.Object)
        {
            oldRoom = _objs[id].ContainerRoom;
            _objs[id].ContainerRoom = room;
        }

        if (ASLVersion >= 391)
            // If this object contains other objects, move them too
        {
            for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
            {
                if ((Strings.LCase(GetObjectProperty("parent", i, false, false)) ?? "") == (Strings.LCase(name) ?? ""))
                {
                    MoveThing(_objs[i].ObjectName, room, type, ctx);
                }
            }
        }

        _ = UpdateObjectList(ctx);

        if (BeginsWith(Strings.LCase(room), "inventory") | BeginsWith(Strings.LCase(oldRoom), "inventory"))
        {
            _ = UpdateItems(ctx);
        }
    }

    private async Task SetOpenClose(string name, bool open, Context ctx)
    {
        string cmd;

        if (open)
        {
            cmd = "open";
        }
        else
        {
            cmd = "close";
        }

        var id = GetObjectIdNoAlias(name);
        if (id == 0)
        {
            LogASLError("Invalid object name specified in '" + cmd + " <" + name + ">", LogType.WarningError);
            return;
        }

        await DoOpenClose(id, open, false, ctx);
    }

    private async Task SetVisibility(string thing, Thing type, bool visible, Context ctx)
    {
        // Sets visibilty of objects and characters        

        if (ASLVersion >= 280)
        {
            var found = false;

            for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
            {
                if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(thing) ?? ""))
                {
                    _objs[i].Visible = visible;
                    if (visible)
                    {
                        await AddToObjectProperties("not invisible", i, ctx);
                    }
                    else
                    {
                        await AddToObjectProperties("invisible", i, ctx);
                    }

                    found = true;
                    break;
                }
            }

            if (!found)
            {
                LogASLError("Not found object '" + thing + "'", LogType.WarningError);
            }
        }
        else
        {
            // split ThingString into character name and room
            // (thingstring of form name@room)

            var atPos = Strings.InStr(thing, "@");
            string room, name;

            // If no room specified, current room presumed
            if (atPos == 0)
            {
                room = _currentRoom;
                name = thing;
            }
            else
            {
                name = Strings.Trim(Strings.Left(thing, atPos - 1));
                room = Strings.Trim(Strings.Right(thing, Strings.Len(thing) - atPos));
            }

            if (type == Thing.Character)
            {
                for (int i = 1, loopTo1 = _numberChars; i <= loopTo1; i++)
                {
                    if (((Strings.LCase(_chars[i].ContainerRoom) ?? "") == (Strings.LCase(room) ?? "")) &
                        ((Strings.LCase(_chars[i].ObjectName) ?? "") == (Strings.LCase(name) ?? "")))
                    {
                        _chars[i].Visible = visible;
                        break;
                    }
                }
            }
            else if (type == Thing.Object)
            {
                for (int i = 1, loopTo2 = _numberObjs; i <= loopTo2; i++)
                {
                    if (((Strings.LCase(_objs[i].ContainerRoom) ?? "") == (Strings.LCase(room) ?? "")) &
                        ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(name) ?? "")))
                    {
                        _objs[i].Visible = visible;
                        break;
                    }
                }
            }
        }

        await UpdateObjectList(ctx);
    }

    private async Task<string> GetGender(string character, bool capitalise, Context ctx)
    {
        string result;

        if (ASLVersion >= 281)
        {
            result = _objs[GetObjectIdNoAlias(character)].Gender;
        }
        else
        {
            var resultLine = RetrLine("character", character, "gender", ctx);

            if (resultLine == "<unfound>")
            {
                result = "it ";
            }
            else
            {
                result = await GetParameter(resultLine, ctx) + " ";
            }
        }

        if (capitalise)
        {
            result = Strings.UCase(Strings.Left(result, 1)) + Strings.Right(result, Strings.Len(result) - 1);
        }

        return result;
    }

    private bool IsAvailable(string thingName, Thing type, Context ctx)
    {
        // Returns availability of object/character

        // split ThingString into character name and room
        // (thingstring of form name@room)

        string room, name;

        var atPos = Strings.InStr(thingName, "@");

        // If no room specified, current room presumed
        if (atPos == 0)
        {
            room = _currentRoom;
            name = thingName;
        }
        else
        {
            name = Strings.Trim(Strings.Left(thingName, atPos - 1));
            room = Strings.Trim(Strings.Right(thingName, Strings.Len(thingName) - atPos));
        }

        if (type == Thing.Character)
        {
            for (int i = 1, loopTo = _numberChars; i <= loopTo; i++)
            {
                if (((Strings.LCase(_chars[i].ContainerRoom) ?? "") == (Strings.LCase(room) ?? "")) &
                    ((Strings.LCase(_chars[i].ObjectName) ?? "") == (Strings.LCase(name) ?? "")))
                {
                    return _chars[i].Exists;
                }
            }
        }
        else if (type == Thing.Object)
        {
            for (int i = 1, loopTo1 = _numberObjs; i <= loopTo1; i++)
            {
                if (((Strings.LCase(_objs[i].ContainerRoom) ?? "") == (Strings.LCase(room) ?? "")) &
                    ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(name) ?? "")))
                {
                    return _objs[i].Exists;
                }
            }
        }

        return default;
    }

    private async Task SetAvailability(string thingString, bool exists, Context ctx, Thing type = Thing.Object)
    {
        // Sets availability of objects (and characters in ASL<281)

        if (ASLVersion >= 281)
        {
            var found = false;
            for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
            {
                if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(thingString) ?? ""))
                {
                    _objs[i].Exists = exists;
                    if (exists)
                    {
                        await AddToObjectProperties("not hidden", i, ctx);
                    }
                    else
                    {
                        await AddToObjectProperties("hidden", i, ctx);
                    }

                    found = true;
                    break;
                }
            }

            if (!found)
            {
                LogASLError("Not found object '" + thingString + "'", LogType.WarningError);
            }
        }
        else
        {
            // split ThingString into character name and room
            // (thingstring of form name@room)

            string room, name;

            var atPos = Strings.InStr(thingString, "@");
            // If no room specified, currentroom presumed
            if (atPos == 0)
            {
                room = _currentRoom;
                name = thingString;
            }
            else
            {
                name = Strings.Trim(Strings.Left(thingString, atPos - 1));
                room = Strings.Trim(Strings.Right(thingString, Strings.Len(thingString) - atPos));
            }

            if (type == Thing.Character)
            {
                for (int i = 1, loopTo1 = _numberChars; i <= loopTo1; i++)
                {
                    if (((Strings.LCase(_chars[i].ContainerRoom) ?? "") == (Strings.LCase(room) ?? "")) &
                        ((Strings.LCase(_chars[i].ObjectName) ?? "") == (Strings.LCase(name) ?? "")))
                    {
                        _chars[i].Exists = exists;
                        break;
                    }
                }
            }
            else if (type == Thing.Object)
            {
                for (int i = 1, loopTo2 = _numberObjs; i <= loopTo2; i++)
                {
                    if (((Strings.LCase(_objs[i].ContainerRoom) ?? "") == (Strings.LCase(room) ?? "")) &
                        ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(name) ?? "")))
                    {
                        _objs[i].Exists = exists;
                        break;
                    }
                }
            }
        }

        await UpdateItems(ctx);
        await UpdateObjectList(ctx);
    }

    private void CheckCollectable(int id)
    {
        // Checks to see whether a collectable item has exceeded
        // its range - if so, it resets the number to the nearest
        // valid number. It's a handy quick way of making sure that
        // a player's health doesn't reach 101%, for example.

        double max = default, value, min = default;
        int m;

        var type = _collectables[id].Type;
        value = _collectables[id].Value;

        if ((type == "%") & (value > 100d))
        {
            value = 100d;
        }

        if (((type == "%") | (type == "p")) & (value < 0d))
        {
            value = 0d;
        }

        if (Strings.InStr(type, "r") > 0)
        {
            if (Strings.InStr(type, "r") == 1)
            {
                max = Conversion.Val(Strings.Mid(type, Strings.Len(type) - 1));
                m = 1;
            }
            else if (Strings.InStr(type, "r") == Strings.Len(type))
            {
                min = Conversion.Val(Strings.Left(type, Strings.Len(type) - 1));
                m = 2;
            }
            else
            {
                min = Conversion.Val(Strings.Left(type, Strings.InStr(type, "r") - 1));
                max = Conversion.Val(Strings.Mid(type, Strings.InStr(type, "r") + 1));
                m = 3;
            }

            if (((m == 1) | (m == 3)) & (value > max))
            {
                value = max;
            }

            if (((m == 2) | (m == 3)) & (value < min))
            {
                value = min;
            }
        }

        _collectables[id].Value = value;
    }

    private string DisplayCollectableInfo(int id)
    {
        string display;

        if (_collectables[id].Display == "<def>")
        {
            display = "You have " + Strings.Trim(InvariantStrings.Str(_collectables[id].Value)) + " " +
                      _collectables[id].Name;
        }
        else if (string.IsNullOrEmpty(_collectables[id].Display))
        {
            display = "<null>";
        }
        else
        {
            var ep = Strings.InStr(_collectables[id].Display, "!");
            if (ep == 0)
            {
                display = _collectables[id].Display;
            }
            else
            {
                var firstBit = Strings.Left(_collectables[id].Display, ep - 1);
                var nextBit = Strings.Right(_collectables[id].Display, Strings.Len(_collectables[id].Display) - ep);
                display = firstBit + Strings.Trim(InvariantStrings.Str(_collectables[id].Value)) + nextBit;
            }

            if (Strings.InStr(display, "*") > 0)
            {
                var firstStarPos = Strings.InStr(display, "*");
                var secondStarPos = Strings.InStr(firstStarPos + 1, display, "*");
                var beforeStar = Strings.Left(display, firstStarPos - 1);
                var afterStar = Strings.Mid(display, secondStarPos + 1);
                var betweenStar = Strings.Mid(display, firstStarPos + 1, secondStarPos - firstStarPos - 1);

                if (_collectables[id].Value != 1d)
                {
                    display = beforeStar + betweenStar + afterStar;
                }
                else
                {
                    display = beforeStar + afterStar;
                }
            }
        }

        if ((_collectables[id].Value == 0d) & !_collectables[id].DisplayWhenZero)
        {
            display = "<null>";
        }

        return display;
    }

    private async Task ObjectActionUpdate(int id, string name, string script, bool noUpdate = false)
    {
        string objectName;
        int sp, ep;
        name = Strings.LCase(name);

        if (!noUpdate)
        {
            if (name == "take")
            {
                _objs[id].Take.Data = script;
                _objs[id].Take.Type = TextActionType.Script;
            }
            else if (name == "use")
            {
                await AddToUseInfo(id, script);
            }
            else if (name == "gain")
            {
                _objs[id].GainScript = script;
            }
            else if (name == "lose")
            {
                _objs[id].LoseScript = script;
            }
            else if (BeginsWith(name, "use "))
            {
                name = GetEverythingAfter(name, "use ");
                if (Strings.InStr(name, "'") > 0)
                {
                    sp = Strings.InStr(name, "'");
                    ep = Strings.InStr(sp + 1, name, "'");
                    if (ep == 0)
                    {
                        LogASLError("Missing ' in 'action <use " + name + "> " + ReportErrorLine(script));
                        return;
                    }

                    objectName = Strings.Mid(name, sp + 1, ep - sp - 1);

                    await AddToUseInfo(id, Strings.Trim(Strings.Left(name, sp - 1)) + " <" + objectName + "> " + script);
                }
                else
                {
                    await AddToUseInfo(id, name + " " + script);
                }
            }
            else if (BeginsWith(name, "give "))
            {
                name = GetEverythingAfter(name, "give ");
                if (Strings.InStr(name, "'") > 0)
                {
                    sp = Strings.InStr(name, "'");
                    ep = Strings.InStr(sp + 1, name, "'");
                    if (ep == 0)
                    {
                        LogASLError("Missing ' in 'action <give " + name + "> " + ReportErrorLine(script));
                        return;
                    }

                    objectName = Strings.Mid(name, sp + 1, ep - sp - 1);

                    await AddToGiveInfo(id, Strings.Trim(Strings.Left(name, sp - 1)) + " <" + objectName + "> " + script);
                }
                else
                {
                    await AddToGiveInfo(id, name + " " + script);
                }
            }
        }

        if (_gameFullyLoaded)
        {
            AddToObjectChangeLog(ChangeLog.AppliesTo.Object, _objs[id].ObjectName, name,
                "action <" + name + "> " + script);
        }
    }

    private double GetCollectableAmount(string name)
    {
        for (int i = 1, loopTo = _numCollectables; i <= loopTo; i++)
        {
            if ((_collectables[i].Name ?? "") == (name ?? ""))
            {
                return _collectables[i].Value;
            }
        }

        return 0d;
    }

    private async Task PlayerItem(string item, bool got, Context ctx, int objId = 0)
    {
        // Gives the player an item (if got=True) or takes an
        // item away from the player (if got=False).

        // If ASL>280, setting got=TRUE moves specified
        // *object* to room "inventory"; setting got=FALSE
        // drops object into current room.

        var foundObjectName = false;

        if (ASLVersion >= 280)
        {
            if (objId == 0)
            {
                for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
                {
                    if ((Strings.LCase(_objs[i].ObjectName) ?? "") == (Strings.LCase(item) ?? ""))
                    {
                        objId = i;
                        break;
                    }
                }
            }

            if (objId != 0)
            {
                if (got)
                {
                    if (ASLVersion >= 391)
                    {
                        // Unset parent information, if any
                        await AddToObjectProperties("not parent", objId, ctx);
                    }

                    MoveThing(_objs[objId].ObjectName, "inventory", Thing.Object, ctx);

                    if (!string.IsNullOrEmpty(_objs[objId].GainScript))
                    {
                        await ExecuteScript(_objs[objId].GainScript, ctx);
                    }
                }
                else
                {
                    MoveThing(_objs[objId].ObjectName, _currentRoom, Thing.Object, ctx);

                    if (!string.IsNullOrEmpty(_objs[objId].LoseScript))
                    {
                        await ExecuteScript(_objs[objId].LoseScript, ctx);
                    }
                }

                foundObjectName = true;
            }

            if (!foundObjectName)
            {
                LogASLError("No such object '" + item + "'", LogType.WarningError);
            }
            else
            {
                await UpdateItems(ctx);
                await UpdateObjectList(ctx);
            }
        }
        else
        {
            for (int i = 1, loopTo1 = _numberItems; i <= loopTo1; i++)
            {
                if ((_items[i].Name ?? "") == (item ?? ""))
                {
                    _items[i].Got = got;
                    i = _numberItems;
                }
            }

            await UpdateItems(ctx);
        }
    }

    private async Task UpdateVisibilityInContainers(Context ctx, string onlyParent = "")
    {
        // Use OnlyParent to only update objects that are contained by a specific parent

        int parentId;
        string parent;
        bool parentIsTransparent = default, parentIsOpen = default, parentIsSeen = default;
        var parentIsSurface = default(bool);

        if (ASLVersion < 391)
        {
            return;
        }

        if (!string.IsNullOrEmpty(onlyParent))
        {
            onlyParent = Strings.LCase(onlyParent);
            parentId = GetObjectIdNoAlias(onlyParent);

            parentIsOpen = IsYes(GetObjectProperty("opened", parentId, true, false));
            parentIsTransparent = IsYes(GetObjectProperty("transparent", parentId, true, false));
            parentIsSeen = IsYes(GetObjectProperty("seen", parentId, true, false));
            parentIsSurface = IsYes(GetObjectProperty("surface", parentId, true, false));
        }

        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            // If object has a parent object
            parent = GetObjectProperty("parent", i, false, false);

            if (!string.IsNullOrEmpty(parent))
            {
                // Check if that parent is open, or transparent
                if (string.IsNullOrEmpty(onlyParent))
                {
                    parentId = GetObjectIdNoAlias(parent);
                    parentIsOpen = IsYes(GetObjectProperty("opened", parentId, true, false));
                    parentIsTransparent = IsYes(GetObjectProperty("transparent", parentId, true, false));
                    parentIsSeen = IsYes(GetObjectProperty("seen", parentId, true, false));
                    parentIsSurface = IsYes(GetObjectProperty("surface", parentId, true, false));
                }

                if (string.IsNullOrEmpty(onlyParent) | ((Strings.LCase(parent) ?? "") == (onlyParent ?? "")))
                {
                    if (parentIsSurface | ((parentIsOpen | parentIsTransparent) & parentIsSeen))
                    {
                        // If the parent is a surface, then the contents are always available.
                        // Otherwise, only if the parent has been seen, AND is either open or transparent,
                        // then the contents are available.

                        await SetAvailability(_objs[i].ObjectName, true, ctx);
                    }
                    else
                    {
                        await SetAvailability(_objs[i].ObjectName, false, ctx);
                    }
                }
            }
        }
    }

    private PlayerCanAccessObjectResult PlayerCanAccessObject(int id, List<int> colObjects = null)
    {
        // Called to see if a player can interact with an object (take it, open it etc.).
        // For example, if the object is on a surface which is inside a closed container,
        // the object cannot be accessed.

        string parent;
        int parentId;
        string parentDisplayName;
        var result = new PlayerCanAccessObjectResult();

        var hierarchy = "";
        if (IsYes(GetObjectProperty("parent", id, true, false)))
        {
            // Object is in a container...

            parent = GetObjectProperty("parent", id, false, false);
            parentId = GetObjectIdNoAlias(parent);

            // But if it's a surface then it's OK

            if (!IsYes(GetObjectProperty("surface", parentId, true, false)) &
                !IsYes(GetObjectProperty("opened", parentId, true, false)))
            {
                // Parent has no "opened" property, so it's closed. Hence
                // object can't be accessed

                if (!string.IsNullOrEmpty(_objs[parentId].ObjectAlias))
                {
                    parentDisplayName = _objs[parentId].ObjectAlias;
                }
                else
                {
                    parentDisplayName = _objs[parentId].ObjectName;
                }

                result.CanAccessObject = false;
                result.ErrorMsg = "inside closed " + parentDisplayName;
                return result;
            }

            // Is the parent itself accessible?
            if (colObjects is null)
            {
                colObjects = new List<int>();
            }

            if (colObjects.Contains(parentId))
            {
                // We've already encountered this parent while recursively calling
                // this function - we're in a loop of parents!
                foreach (var objId in colObjects)
                {
                    hierarchy = hierarchy + _objs[objId].ObjectName + " -> ";
                }

                hierarchy = hierarchy + _objs[parentId].ObjectName;
                LogASLError("Looped object parents detected: " + hierarchy);

                result.CanAccessObject = false;
                return result;
            }

            colObjects.Add(parentId);

            return PlayerCanAccessObject(parentId, colObjects);
        }

        result.CanAccessObject = true;
        return result;
    }

    private class PlayerCanAccessObjectResult
    {
        public bool CanAccessObject;
        public string ErrorMsg;
    }
}
