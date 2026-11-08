using System.Text;
using Microsoft.VisualBasic;
using QuestViva.Common;
using Constants = Microsoft.VisualBasic.Constants;
using Strings = Microsoft.VisualBasic.Strings;

namespace QuestViva.Legacy;

public partial class V4Game
{
    private async Task DestroyExit(string exitData, Context ctx)
    {
        var fromRoom = "";
        var toRoom = "";
        int roomId = default, exitId = default;

        var scp = Strings.InStr(exitData, ";");
        if (scp == 0)
        {
            LogASLError("No exit name specified in 'destroy exit <" + exitData + ">'");
            return;
        }

        RoomExit roomExit;
        if (ASLVersion >= 410)
        {
            roomExit = FindExit(exitData);
            if (roomExit is null)
            {
                LogASLError("Can't find exit in 'destroy exit <" + exitData + ">'");
                return;
            }

            roomExit.GetParent().RemoveExit(ref roomExit);
        }

        else
        {
            fromRoom = Strings.LCase(Strings.Trim(Strings.Left(exitData, scp - 1)));
            toRoom = Strings.Trim(Strings.Mid(exitData, scp + 1));

            // Find From Room:
            var found = false;

            for (int i = 1, loopTo = _numberRooms; i <= loopTo; i++)
            {
                if ((Strings.LCase(_rooms[i].RoomName) ?? "") == (fromRoom ?? ""))
                {
                    found = true;
                    roomId = i;
                    break;
                }
            }

            if (!found)
            {
                LogASLError("No such room '" + fromRoom + "'");
                return;
            }

            found = false;
            var r = _rooms[roomId];

            for (int i = 1, loopTo1 = r.NumberPlaces; i <= loopTo1; i++)
            {
                if ((r.Places[i].PlaceName ?? "") == (toRoom ?? ""))
                {
                    exitId = i;
                    found = true;
                    break;
                }
            }

            if (found)
            {
                for (int i = exitId, loopTo2 = r.NumberPlaces - 1; i <= loopTo2; i++)
                {
                    r.Places[i] = r.Places[i + 1];
                }

                Array.Resize(ref r.Places, r.NumberPlaces);
                r.NumberPlaces = r.NumberPlaces - 1;
            }
        }

        // Update quest.* vars and obj list
        await ShowRoomInfo(_currentRoom, ctx, true);
        await UpdateObjectList(ctx);

        AddToChangeLog("room " + fromRoom, "destroy exit " + toRoom);
    }

    private async Task ExecuteCreateExit(string data, Context ctx)
    {
        string scrRoom;
        var destRoom = "";
        var destId = default(int);
        var exitData = GetEverythingAfter(data, "exit ");
        var newName = await GetParameter(data, ctx);
        var scp = Strings.InStr(newName, ";");

        if (ASLVersion < 410)
        {
            if (scp == 0)
            {
                LogASLError("No exit destination given in 'create exit " + exitData + "'", LogType.WarningError);
                return;
            }
        }

        if (scp == 0)
        {
            scrRoom = Strings.Trim(newName);
        }
        else
        {
            scrRoom = Strings.Trim(Strings.Left(newName, scp - 1));
        }

        var srcId = GetRoomID(scrRoom, ctx);

        if (srcId == 0)
        {
            LogASLError("No such room '" + scrRoom + "'", LogType.WarningError);
            return;
        }

        if (ASLVersion < 410)
        {
            // only do destination room check for ASL <410, as can now have scripts on dynamically
            // created exits, so the destination doesn't necessarily have to exist.

            destRoom = Strings.Trim(Strings.Mid(newName, scp + 1));
            if (!string.IsNullOrEmpty(destRoom))
            {
                destId = GetRoomID(destRoom, ctx);

                if (destId == 0)
                {
                    LogASLError("No such room '" + destRoom + "'", LogType.WarningError);
                    return;
                }
            }
        }

        // If it's a "go to" exit, check if it already exists:
        var exists = false;
        if (BeginsWith(exitData, "<"))
        {
            if (ASLVersion >= 410)
            {
                exists = _rooms[srcId].Exits.GetPlaces().ContainsKey(destRoom);
            }
            else
            {
                for (int i = 1, loopTo = _rooms[srcId].NumberPlaces; i <= loopTo; i++)
                {
                    if ((Strings.LCase(_rooms[srcId].Places[i].PlaceName) ?? "") == (Strings.LCase(destRoom) ?? ""))
                    {
                        exists = true;
                        break;
                    }
                }
            }

            if (exists)
            {
                LogASLError("Exit from '" + scrRoom + "' to '" + destRoom + "' already exists", LogType.WarningError);
                return;
            }
        }

        var paramPos = Strings.InStr(exitData, "<");
        string saveData;
        if (paramPos == 0)
        {
            saveData = exitData;
        }
        else
        {
            saveData = Strings.Left(exitData, paramPos - 1);
            // We do this so the changelog doesn't contain unconverted variable names
            saveData = saveData + "<" + await GetParameter(exitData, ctx) + ">";
        }

        AddToChangeLog("room " + _rooms[srcId].RoomName, "exit " + saveData);

        var r = _rooms[srcId];

        if (ASLVersion >= 410)
        {
            await r.Exits.AddExitFromCreateScript(exitData, ctx);
        }
        else if (BeginsWith(exitData, "north "))
        {
            r.North.Data = destRoom;
            r.North.Type = TextActionType.Text;
        }
        else if (BeginsWith(exitData, "south "))
        {
            r.South.Data = destRoom;
            r.South.Type = TextActionType.Text;
        }
        else if (BeginsWith(exitData, "east "))
        {
            r.East.Data = destRoom;
            r.East.Type = TextActionType.Text;
        }
        else if (BeginsWith(exitData, "west "))
        {
            r.West.Data = destRoom;
            r.West.Type = TextActionType.Text;
        }
        else if (BeginsWith(exitData, "northeast "))
        {
            r.NorthEast.Data = destRoom;
            r.NorthEast.Type = TextActionType.Text;
        }
        else if (BeginsWith(exitData, "northwest "))
        {
            r.NorthWest.Data = destRoom;
            r.NorthWest.Type = TextActionType.Text;
        }
        else if (BeginsWith(exitData, "southeast "))
        {
            r.SouthEast.Data = destRoom;
            r.SouthEast.Type = TextActionType.Text;
        }
        else if (BeginsWith(exitData, "southwest "))
        {
            r.SouthWest.Data = destRoom;
            r.SouthWest.Type = TextActionType.Text;
        }
        else if (BeginsWith(exitData, "up "))
        {
            r.Up.Data = destRoom;
            r.Up.Type = TextActionType.Text;
        }
        else if (BeginsWith(exitData, "down "))
        {
            r.Down.Data = destRoom;
            r.Down.Type = TextActionType.Text;
        }
        else if (BeginsWith(exitData, "out "))
        {
            r.Out.Text = destRoom;
        }
        else if (BeginsWith(exitData, "<"))
        {
            r.NumberPlaces = r.NumberPlaces + 1;
            Array.Resize(ref r.Places, r.NumberPlaces + 1);
            r.Places[r.NumberPlaces] = new PlaceType();
            r.Places[r.NumberPlaces].PlaceName = destRoom;
        }
        else
        {
            LogASLError("Invalid direction in 'create exit " + exitData + "'", LogType.WarningError);
        }

        if (!_gameLoading)
        {
            // Update quest.doorways variables
           await ShowRoomInfo(_currentRoom, ctx, true);

            await UpdateObjectList(ctx);

            if (ASLVersion < 410)
            {
                if ((_currentRoom ?? "") == (_rooms[srcId].RoomName ?? ""))
                {
                    await UpdateDoorways(srcId, ctx);
                }
                else if ((_currentRoom ?? "") == (_rooms[destId].RoomName ?? ""))
                {
                    await UpdateDoorways(destId, ctx);
                }
            }
            else
            {
                // Don't have DestID in ASL410 CreateExit code, so just UpdateDoorways
                // for current room anyway.
                await UpdateDoorways(GetRoomID(_currentRoom, ctx), ctx);
            }
        }
    }

    internal int GetRoomID(string name, Context ctx)
    {
        if (Strings.InStr(name, "[") > 0)
        {
            var idx = GetArrayIndex(name, ctx);
            name = idx.Name + Strings.Trim(InvariantStrings.Str(idx.Index));
        }

        for (int i = 1, loopTo = _numberRooms; i <= loopTo; i++)
        {
            if ((Strings.LCase(_rooms[i].RoomName) ?? "") == (Strings.LCase(name) ?? ""))
            {
                return i;
            }
        }

        return 0;
    }

    private async Task ShowRoomInfoV2(string room)
    {
        // ShowRoomInfo for Quest 2.x games

        var roomDisplayText = "";
        bool descTagExist;
        DefineBlock gameBlock;
        string charsViewable;
        int charsFound;
        string prefixAliasNoFormat, prefix, prefixAlias, inDesc;
        var aliasName = "";
        string charList;
        int foundLastComma, cp, ncp;
        string noFormatObjsViewable;
        var objsViewable = "";
        var objsFound = default(int);
        string objListString, noFormatObjListString;
        string possDir, nsew, doorways, places, place;
        var aliasOut = "";
        string placeNoFormat;
        var descLine = "";
        int lastComma, oldLastComma;
        int defineBlock;
        var lookString = "";

        gameBlock = GetDefineBlock("game");
        _currentRoom = room;

        // find the room
        DefineBlock roomBlock;
        roomBlock = await DefineBlockParam("room", room);
        bool finishedFindingCommas;

        charsViewable = "";
        charsFound = 0;

        // see if room has an alias
        for (int i = roomBlock.StartLine + 1, loopTo = roomBlock.EndLine - 1; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[i], "alias"))
            {
                aliasName = await GetParameter(_lines[i], _nullContext);
                i = roomBlock.EndLine;
            }
        }

        if (string.IsNullOrEmpty(aliasName))
        {
            aliasName = room;
        }

        // see if room has a prefix
        prefix = await FindStatement(roomBlock, "prefix");
        if (string.IsNullOrEmpty(prefix))
        {
            prefixAlias = "|cr" + aliasName + "|cb";
            prefixAliasNoFormat = aliasName; // No formatting version, for label
        }
        else
        {
            prefixAlias = prefix + " |cr" + aliasName + "|cb";
            prefixAliasNoFormat = prefix + " " + aliasName;
        }

        // print player's location
        // find indescription line:
        inDesc = "unfound";
        for (int i = roomBlock.StartLine + 1, loopTo1 = roomBlock.EndLine - 1; i <= loopTo1; i++)
        {
            if (BeginsWith(_lines[i], "indescription"))
            {
                inDesc = Strings.Trim(await GetParameter(_lines[i], _nullContext));
                i = roomBlock.EndLine;
            }
        }

        if (inDesc != "unfound")
        {
            // Print player's location according to indescription:
            if (Strings.Right(inDesc, 1) == ":")
            {
                // if line ends with a colon, add place name:
                roomDisplayText = roomDisplayText + Strings.Left(inDesc, Strings.Len(inDesc) - 1) + " " + prefixAlias +
                                  "." + Constants.vbCrLf;
            }
            else
            {
                // otherwise, just print the indescription line:
                roomDisplayText = roomDisplayText + inDesc + Constants.vbCrLf;
            }
        }
        else
        {
            // if no indescription line, print the default.
            roomDisplayText = roomDisplayText + "You are in " + prefixAlias + "." + Constants.vbCrLf;
        }

        _player.LocationUpdated(prefixAliasNoFormat);

        await SetStringContents("quest.formatroom", prefixAliasNoFormat, _nullContext);

        // FIND CHARACTERS ===

        for (int i = 1, loopTo2 = _numberChars; i <= loopTo2; i++)
        {
            if (((_chars[i].ContainerRoom ?? "") == (room ?? "")) & _chars[i].Exists & _chars[i].Visible)
            {
                charsViewable = charsViewable + _chars[i].Prefix + "|b" + _chars[i].ObjectName + "|xb" +
                                _chars[i].Suffix + ", ";
                charsFound = charsFound + 1;
            }
        }

        if (charsFound == 0)
        {
            charsViewable = "There is nobody here.";
            await SetStringContents("quest.characters", "", _nullContext);
        }
        else
        {
            // chop off final comma and add full stop (.)
            charList = Strings.Left(charsViewable, Strings.Len(charsViewable) - 2);
            await SetStringContents("quest.characters", charList, _nullContext);

            // if more than one character, add "and" before
            // last one:
            cp = Strings.InStr(charList, ",");
            if (cp != 0)
            {
                foundLastComma = 0;
                do
                {
                    ncp = Strings.InStr(cp + 1, charList, ",");
                    if (ncp == 0)
                    {
                        foundLastComma = 1;
                    }
                    else
                    {
                        cp = ncp;
                    }
                } while (foundLastComma != 1);

                charList = Strings.Trim(Strings.Left(charList, cp - 1)) + " and " +
                           Strings.Trim(Strings.Mid(charList, cp + 1));
            }

            charsViewable = "You can see " + charList + " here.";
        }

        roomDisplayText = roomDisplayText + charsViewable + Constants.vbCrLf;

        // FIND OBJECTS

        noFormatObjsViewable = "";

        for (int i = 1, loopTo3 = _numberObjs; i <= loopTo3; i++)
        {
            if (((_objs[i].ContainerRoom ?? "") == (room ?? "")) & _objs[i].Exists & _objs[i].Visible)
            {
                objsViewable = objsViewable + _objs[i].Prefix + "|b" + _objs[i].ObjectName + "|xb" + _objs[i].Suffix +
                               ", ";
                noFormatObjsViewable = noFormatObjsViewable + _objs[i].Prefix + _objs[i].ObjectName + ", ";

                objsFound = objsFound + 1;
            }
        }

        var finishedLoop = default(bool);
        if (objsFound != 0)
        {
            objListString = Strings.Left(objsViewable, Strings.Len(objsViewable) - 2);
            noFormatObjListString = Strings.Left(noFormatObjsViewable, Strings.Len(noFormatObjsViewable) - 2);

            cp = Strings.InStr(objListString, ",");
            if (cp != 0)
            {
                do
                {
                    ncp = Strings.InStr(cp + 1, objListString, ",");
                    if (ncp == 0)
                    {
                        finishedLoop = true;
                    }
                    else
                    {
                        cp = ncp;
                    }
                } while (!finishedLoop);

                objListString = Strings.Trim(Strings.Left(objListString, cp - 1) + " and " +
                                             Strings.Trim(Strings.Mid(objListString, cp + 1)));
            }

            objsViewable = "There is " + objListString + " here.";
            await SetStringContents("quest.objects",
                Strings.Left(noFormatObjsViewable, Strings.Len(noFormatObjsViewable) - 2), _nullContext);
            await SetStringContents("quest.formatobjects", objListString, _nullContext);
            roomDisplayText = roomDisplayText + objsViewable + Constants.vbCrLf;
        }
        else
        {
            await SetStringContents("quest.objects", "", _nullContext);
            await SetStringContents("quest.formatobjects", "", _nullContext);
        }

        // FIND DOORWAYS
        doorways = "";
        nsew = "";
        places = "";
        possDir = "";

        for (int i = roomBlock.StartLine + 1, loopTo4 = roomBlock.EndLine - 1; i <= loopTo4; i++)
        {
            if (BeginsWith(_lines[i], "out"))
            {
                doorways = await GetParameter(_lines[i], _nullContext);
            }

            if (BeginsWith(_lines[i], "north "))
            {
                nsew = nsew + "|bnorth|xb, ";
                possDir = possDir + "n";
            }
            else if (BeginsWith(_lines[i], "south "))
            {
                nsew = nsew + "|bsouth|xb, ";
                possDir = possDir + "s";
            }
            else if (BeginsWith(_lines[i], "east "))
            {
                nsew = nsew + "|beast|xb, ";
                possDir = possDir + "e";
            }
            else if (BeginsWith(_lines[i], "west "))
            {
                nsew = nsew + "|bwest|xb, ";
                possDir = possDir + "w";
            }
            else if (BeginsWith(_lines[i], "northeast "))
            {
                nsew = nsew + "|bnortheast|xb, ";
                possDir = possDir + "a";
            }
            else if (BeginsWith(_lines[i], "northwest "))
            {
                nsew = nsew + "|bnorthwest|xb, ";
                possDir = possDir + "b";
            }
            else if (BeginsWith(_lines[i], "southeast "))
            {
                nsew = nsew + "|bsoutheast|xb, ";
                possDir = possDir + "c";
            }
            else if (BeginsWith(_lines[i], "southwest "))
            {
                nsew = nsew + "|bsouthwest|xb, ";
                possDir = possDir + "d";
            }

            if (BeginsWith(_lines[i], "place"))
            {
                // remove any prefix semicolon from printed text
                place = await GetParameter(_lines[i], _nullContext);
                placeNoFormat = place; // Used in object list - no formatting or prefix
                if (Strings.InStr(place, ";") > 0)
                {
                    placeNoFormat = Strings.Right(place, Strings.Len(place) - (Strings.InStr(place, ";") + 1));
                    place = Strings.Trim(Strings.Left(place, Strings.InStr(place, ";") - 1)) + " |b" +
                            Strings.Right(place, Strings.Len(place) - (Strings.InStr(place, ";") + 1)) + "|xb";
                }
                else
                {
                    place = "|b" + place + "|xb";
                }

                places = places + place + ", ";
            }
        }

        DefineBlock outside;
        if (!string.IsNullOrEmpty(doorways))
        {
            // see if outside has an alias
            outside = await DefineBlockParam("room", doorways);
            for (int i = outside.StartLine + 1, loopTo5 = outside.EndLine - 1; i <= loopTo5; i++)
            {
                if (BeginsWith(_lines[i], "alias"))
                {
                    aliasOut = await GetParameter(_lines[i], _nullContext);
                    i = outside.EndLine;
                }
            }

            if (string.IsNullOrEmpty(aliasOut))
            {
                aliasOut = doorways;
            }

            roomDisplayText = roomDisplayText + "You can go out to " + aliasOut + "." + Constants.vbCrLf;
            possDir = possDir + "o";
            await SetStringContents("quest.doorways.out", aliasOut, _nullContext);
        }
        else
        {
            await SetStringContents("quest.doorways.out", "", _nullContext);
        }

        bool finished;
        if (!string.IsNullOrEmpty(nsew))
        {
            // strip final comma
            nsew = Strings.Left(nsew, Strings.Len(nsew) - 2);
            cp = Strings.InStr(nsew, ",");
            if (cp != 0)
            {
                finished = false;
                do
                {
                    ncp = Strings.InStr(cp + 1, nsew, ",");
                    if (ncp == 0)
                    {
                        finished = true;
                    }
                    else
                    {
                        cp = ncp;
                    }
                } while (!finished);

                nsew = Strings.Trim(Strings.Left(nsew, cp - 1)) + " or " + Strings.Trim(Strings.Mid(nsew, cp + 1));
            }

            roomDisplayText = roomDisplayText + "You can go " + nsew + "." + Constants.vbCrLf;
            await SetStringContents("quest.doorways.dirs", nsew, _nullContext);
        }
        else
        {
            await SetStringContents("quest.doorways.dirs", "", _nullContext);
        }

        UpdateDirButtons(possDir, _nullContext);

        if (!string.IsNullOrEmpty(places))
        {
            // strip final comma
            places = Strings.Left(places, Strings.Len(places) - 2);

            // if there is still a comma here, there is more than
            // one place, so add the word "or" before the last one.
            if (Strings.InStr(places, ",") > 0)
            {
                lastComma = 0;
                finishedFindingCommas = false;
                do
                {
                    oldLastComma = lastComma;
                    lastComma = Strings.InStr(lastComma + 1, places, ",");
                    if (lastComma == 0)
                    {
                        finishedFindingCommas = true;
                        lastComma = oldLastComma;
                    }
                } while (!finishedFindingCommas);

                places = Strings.Left(places, lastComma) + " or" +
                         Strings.Right(places, Strings.Len(places) - lastComma);
            }

            roomDisplayText = roomDisplayText + "You can go to " + places + "." + Constants.vbCrLf;
            await SetStringContents("quest.doorways.places", places, _nullContext);
        }
        else
        {
            await SetStringContents("quest.doorways.places", "", _nullContext);
        }

        // Print RoomDisplayText if there is no "description" tag,
        // otherwise execute the description tag information:

        // First, look in the "define room" block:
        descTagExist = false;
        for (int i = roomBlock.StartLine + 1, loopTo6 = roomBlock.EndLine - 1; i <= loopTo6; i++)
        {
            if (BeginsWith(_lines[i], "description "))
            {
                descLine = _lines[i];
                descTagExist = true;
                break;
            }
        }

        if (!descTagExist)
        {
            // Look in the "define game" block:
            for (int i = gameBlock.StartLine + 1, loopTo7 = gameBlock.EndLine - 1; i <= loopTo7; i++)
            {
                if (BeginsWith(_lines[i], "description "))
                {
                    descLine = _lines[i];
                    descTagExist = true;
                    break;
                }
            }
        }

        if (!descTagExist)
        {
            // Remove final newline:
            roomDisplayText = Strings.Left(roomDisplayText, Strings.Len(roomDisplayText) - 2);
            await Print(roomDisplayText, _nullContext);
        }
        else
        {
            // execute description tag:
            // If no script, just print the tag's parameter.
            // Otherwise, execute it as ASL script:

            descLine = GetEverythingAfter(Strings.Trim(descLine), "description ");
            if (Strings.Left(descLine, 1) == "<")
            {
                await Print(await GetParameter(descLine, _nullContext), _nullContext);
            }
            else
            {
                await ExecuteScript(descLine, _nullContext);
            }
        }

        await UpdateObjectList(_nullContext);

        defineBlock = 0;

        for (int i = roomBlock.StartLine + 1, loopTo8 = roomBlock.EndLine - 1; i <= loopTo8; i++)
        {
            // don't get the 'look' statements in nested define blocks
            if (BeginsWith(_lines[i], "define"))
            {
                defineBlock = defineBlock + 1;
            }

            if (BeginsWith(_lines[i], "end define"))
            {
                defineBlock = defineBlock - 1;
            }

            if (BeginsWith(_lines[i], "look") & (defineBlock == 0))
            {
                lookString = await GetParameter(_lines[i], _nullContext);
                i = roomBlock.EndLine;
            }
        }

        if (!string.IsNullOrEmpty(lookString))
        {
            await Print(lookString, _nullContext);
        }
    }

    private async Task ShowRoomInfo(string room, Context ctx, bool noPrint = false)
    {
        if (ASLVersion < 280)
        {
            await ShowRoomInfoV2(room);
            return;
        }

        var roomDisplayText = "";
        bool descTagExist;
        string doorwayString, roomAlias;
        bool finishedFindingCommas;
        string prefix, roomDisplayName;
        string roomDisplayNameNoFormat, inDescription;
        var visibleObjects = "";
        string visibleObjectsNoFormat;
        string placeList;
        int lastComma, oldLastComma;
        var descType = default(int);
        var descLine = "";
        bool showLookText;
        var lookDesc = "";
        string objLook;
        string objSuffix;

        var gameBlock = GetDefineBlock("game");

        _currentRoom = room;
        var id = GetRoomID(_currentRoom, ctx);

        if (id == 0)
        {
            return;
        }

        // FIRST LINE - YOU ARE IN... ***********************************************

        roomAlias = _rooms[id].RoomAlias;
        if (string.IsNullOrEmpty(roomAlias))
        {
            roomAlias = _rooms[id].RoomName;
        }

        prefix = _rooms[id].Prefix;

        if (string.IsNullOrEmpty(prefix))
        {
            roomDisplayName = "|cr" + roomAlias + "|cb";
            roomDisplayNameNoFormat = roomAlias; // No formatting version, for label
        }
        else
        {
            roomDisplayName = prefix + " |cr" + roomAlias + "|cb";
            roomDisplayNameNoFormat = prefix + " " + roomAlias;
        }

        inDescription = _rooms[id].InDescription;

        if (!string.IsNullOrEmpty(inDescription))
        {
            // Print player's location according to indescription:
            if (Strings.Right(inDescription, 1) == ":")
            {
                // if line ends with a colon, add place name:
                roomDisplayText = roomDisplayText + Strings.Left(inDescription, Strings.Len(inDescription) - 1) + " " +
                                  roomDisplayName + "." + Constants.vbCrLf;
            }
            else
            {
                // otherwise, just print the indescription line:
                roomDisplayText = roomDisplayText + inDescription + Constants.vbCrLf;
            }
        }
        else
        {
            // if no indescription line, print the default.
            roomDisplayText = roomDisplayText + "You are in " + roomDisplayName + "." + Constants.vbCrLf;
        }

        _player.LocationUpdated(Strings.UCase(Strings.Left(roomAlias, 1)) + Strings.Mid(roomAlias, 2));

        await SetStringContents("quest.formatroom", roomDisplayNameNoFormat, ctx);

        // SHOW OBJECTS *************************************************************

        visibleObjectsNoFormat = "";

        var visibleObjectsList = new List<int>(); // of object IDs
        var count = default(int);

        for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
        {
            if (((Strings.LCase(_objs[i].ContainerRoom) ?? "") == (Strings.LCase(room) ?? "")) & _objs[i].Exists &
                _objs[i].Visible & !_objs[i].IsExit)
            {
                visibleObjectsList.Add(i);
            }
        }

        foreach (var objId in visibleObjectsList)
        {
            objSuffix = _objs[objId].Suffix;
            if (!string.IsNullOrEmpty(objSuffix))
            {
                objSuffix = " " + objSuffix;
            }

            if (string.IsNullOrEmpty(_objs[objId].ObjectAlias))
            {
                visibleObjects = visibleObjects + _objs[objId].Prefix + "|b" + _objs[objId].ObjectName + "|xb" +
                                 objSuffix;
                visibleObjectsNoFormat = visibleObjectsNoFormat + _objs[objId].Prefix + _objs[objId].ObjectName;
            }
            else
            {
                visibleObjects = visibleObjects + _objs[objId].Prefix + "|b" + _objs[objId].ObjectAlias + "|xb" +
                                 objSuffix;
                visibleObjectsNoFormat = visibleObjectsNoFormat + _objs[objId].Prefix + _objs[objId].ObjectAlias;
            }

            count = count + 1;
            if (count < visibleObjectsList.Count - 1)
            {
                visibleObjects = visibleObjects + ", ";
                visibleObjectsNoFormat = visibleObjectsNoFormat + ", ";
            }
            else if (count == visibleObjectsList.Count - 1)
            {
                visibleObjects = visibleObjects + " and ";
                visibleObjectsNoFormat = visibleObjectsNoFormat + ", ";
            }
        }

        if (visibleObjectsList.Count > 0)
        {
            await SetStringContents("quest.formatobjects", visibleObjects, ctx);
            visibleObjects = "There is " + visibleObjects + " here.";
            await SetStringContents("quest.objects", visibleObjectsNoFormat, ctx);
            roomDisplayText = roomDisplayText + visibleObjects + Constants.vbCrLf;
        }
        else
        {
            await SetStringContents("quest.objects", "", ctx);
            await SetStringContents("quest.formatobjects", "", ctx);
        }

        // SHOW EXITS ***************************************************************

        doorwayString = await UpdateDoorways(id, ctx);

        if (ASLVersion < 410)
        {
            placeList = GetGoToExits(id, ctx);

            if (!string.IsNullOrEmpty(placeList))
            {
                // strip final comma
                placeList = Strings.Left(placeList, Strings.Len(placeList) - 2);

                // if there is still a comma here, there is more than
                // one place, so add the word "or" before the last one.
                if (Strings.InStr(placeList, ",") > 0)
                {
                    lastComma = 0;
                    finishedFindingCommas = false;
                    do
                    {
                        oldLastComma = lastComma;
                        lastComma = Strings.InStr(lastComma + 1, placeList, ",");
                        if (lastComma == 0)
                        {
                            finishedFindingCommas = true;
                            lastComma = oldLastComma;
                        }
                    } while (!finishedFindingCommas);

                    placeList = Strings.Left(placeList, lastComma - 1) + " or" +
                                Strings.Right(placeList, Strings.Len(placeList) - lastComma);
                }

                roomDisplayText = roomDisplayText + "You can go to " + placeList + "." + Constants.vbCrLf;
                await SetStringContents("quest.doorways.places", placeList, ctx);
            }
            else
            {
                await SetStringContents("quest.doorways.places", "", ctx);
            }
        }

        // GET "LOOK" DESCRIPTION (but don't print it yet) **************************

        objLook = GetObjectProperty("look", _rooms[id].ObjId, logError: false);

        if (string.IsNullOrEmpty(objLook))
        {
            if (!string.IsNullOrEmpty(_rooms[id].Look))
            {
                lookDesc = _rooms[id].Look;
            }
        }
        else
        {
            lookDesc = objLook;
        }

        await SetStringContents("quest.lookdesc", lookDesc, ctx);


        // FIND DESCRIPTION TAG, OR ACTION ******************************************

        // In Quest versions prior to 3.1, with any custom description, the "look"
        // text was always displayed after the "description" tag was printed/executed.
        // In Quest 3.1 and later, it isn't - descriptions should print the look
        // tag themselves when and where necessary.

        showLookText = true;

        if (!string.IsNullOrEmpty(_rooms[id].Description.Data))
        {
            descLine = _rooms[id].Description.Data;
            descType = (int) _rooms[id].Description.Type;
            descTagExist = true;
        }
        else
        {
            descTagExist = false;
        }

        if (!descTagExist)
        {
            // Look in the "define game" block:
            for (int i = gameBlock.StartLine + 1, loopTo1 = gameBlock.EndLine - 1; i <= loopTo1; i++)
            {
                if (BeginsWith(_lines[i], "description "))
                {
                    descLine = GetEverythingAfter(_lines[i], "description ");
                    descTagExist = true;
                    if (Strings.Left(descLine, 1) == "<")
                    {
                        descLine = await GetParameter(descLine, ctx);
                        descType = (int) TextActionType.Text;
                    }
                    else
                    {
                        descType = (int) TextActionType.Script;
                    }

                    i = gameBlock.EndLine;
                }
            }
        }

        if (descTagExist & (ASLVersion >= 310))
        {
            showLookText = false;
        }

        if (!noPrint)
        {
            if (!descTagExist)
            {
                // Remove final vbCrLf:
                roomDisplayText = Strings.Left(roomDisplayText, Strings.Len(roomDisplayText) - 2);
                await Print(roomDisplayText, ctx);
                if (!string.IsNullOrEmpty(doorwayString))
                {
                    await Print(doorwayString, ctx);
                }
            }
            // execute description tag:
            // If no script, just print the tag's parameter.
            // Otherwise, execute it as ASL script:

            else if (descType == (int) TextActionType.Text)
            {
                await Print(descLine, ctx);
            }
            else
            {
                await ExecuteScript(descLine, ctx);
            }

            await UpdateObjectList(ctx);

            // SHOW "LOOK" DESCRIPTION **************************************************

            if (showLookText)
            {
                if (!string.IsNullOrEmpty(lookDesc))
                {
                    await Print(lookDesc, ctx);
                }
            }
        }
    }

    private async Task GoDirection(string direction, Context ctx)
    {
        // leaves the current room in direction specified by
        // 'direction'

        var dirData = new TextAction();
        var id = GetRoomID(_currentRoom, ctx);

        if (id == 0)
        {
            return;
        }

        if (ASLVersion >= 410)
        {
            await _rooms[id].Exits.ExecuteGo(direction, ctx);
            return;
        }

        var r = _rooms[id];

        if (direction == "north")
        {
            dirData = r.North;
        }
        else if (direction == "south")
        {
            dirData = r.South;
        }
        else if (direction == "west")
        {
            dirData = r.West;
        }
        else if (direction == "east")
        {
            dirData = r.East;
        }
        else if (direction == "northeast")
        {
            dirData = r.NorthEast;
        }
        else if (direction == "northwest")
        {
            dirData = r.NorthWest;
        }
        else if (direction == "southeast")
        {
            dirData = r.SouthEast;
        }
        else if (direction == "southwest")
        {
            dirData = r.SouthWest;
        }
        else if (direction == "up")
        {
            dirData = r.Up;
        }
        else if (direction == "down")
        {
            dirData = r.Down;
        }
        else if (direction == "out")
        {
            if (string.IsNullOrEmpty(r.Out.Script))
            {
                dirData.Data = r.Out.Text;
                dirData.Type = TextActionType.Text;
            }
            else
            {
                dirData.Data = r.Out.Script;
                dirData.Type = TextActionType.Script;
            }
        }

        if ((dirData.Type == TextActionType.Script) & !string.IsNullOrEmpty(dirData.Data))
        {
            await ExecuteScript(dirData.Data, ctx);
        }
        else if (!string.IsNullOrEmpty(dirData.Data))
        {
            var newRoom = dirData.Data;
            var scp = Strings.InStr(newRoom, ";");
            if (scp != 0)
            {
                newRoom = Strings.Trim(Strings.Mid(newRoom, scp + 1));
            }

            await PlayGame(newRoom, ctx);
        }
        else if (direction == "out")
        {
            await PlayerErrorMessage(PlayerError.DefaultOut, ctx);
        }
        else
        {
            await PlayerErrorMessage(PlayerError.BadPlace, ctx);
        }
    }

    private async Task GoToPlace(string place, Context ctx)
    {
        // leaves the current room in direction specified by
        // 'direction'

        var destination = "";
        string placeData;
        var disallowed = false;

        placeData = PlaceExist(place, ctx);

        if (!string.IsNullOrEmpty(placeData))
        {
            destination = placeData;
        }
        else if (BeginsWith(place, "the "))
        {
            var np = GetEverythingAfter(place, "the ");
            placeData = PlaceExist(np, ctx);
            if (!string.IsNullOrEmpty(placeData))
            {
                destination = placeData;
            }
            else
            {
                disallowed = true;
            }
        }
        else
        {
            disallowed = true;
        }

        if (!string.IsNullOrEmpty(destination))
        {
            if (Strings.InStr(destination, ";") > 0)
            {
                var s = Strings.Trim(Strings.Right(destination,
                    Strings.Len(destination) - Strings.InStr(destination, ";")));
                await ExecuteScript(s, ctx);
            }
            else
            {
                await PlayGame(destination, ctx);
            }
        }

        if (disallowed)
        {
            await PlayerErrorMessage(PlayerError.BadPlace, ctx);
        }
    }

    private string PlaceExist(string placeName, Context ctx)
    {
        // Returns actual name of an available "place" exit, and if
        // script is executed on going in that direction, that script
        // is returned after a ";"

        var roomId = GetRoomID(_currentRoom, ctx);

        // check if place is available
        var r = _rooms[roomId];

        for (int i = 1, loopTo = r.NumberPlaces; i <= loopTo; i++)
        {
            var checkPlace = r.Places[i].PlaceName;

            // remove any prefix and semicolon
            if (Strings.InStr(checkPlace, ";") > 0)
            {
                checkPlace = Strings.Trim(Strings.Right(checkPlace,
                    Strings.Len(checkPlace) - (Strings.InStr(checkPlace, ";") + 1)));
            }

            var checkPlaceName = checkPlace;

            if ((ASLVersion >= 311) & string.IsNullOrEmpty(r.Places[i].Script))
            {
                var destRoomId = GetRoomID(checkPlace, ctx);
                if (destRoomId != 0)
                {
                    if (!string.IsNullOrEmpty(_rooms[destRoomId].RoomAlias))
                    {
                        checkPlaceName = _rooms[destRoomId].RoomAlias;
                    }
                }
            }

            if ((Strings.LCase(checkPlaceName) ?? "") == (Strings.LCase(placeName) ?? ""))
            {
                if (!string.IsNullOrEmpty(r.Places[i].Script))
                {
                    return checkPlace + ";" + r.Places[i].Script;
                }

                return checkPlace;
            }
        }

        return "";
    }

    internal async Task PlayGame(string room, Context ctx)
    {
        // plays the specified room

        var id = GetRoomID(room, ctx);

        if (id == 0)
        {
            LogASLError("No such room '" + room + "'", LogType.WarningError);
            return;
        }

        _currentRoom = room;

        await SetStringContents("quest.currentroom", room, ctx);

        if ((ASLVersion >= 391) & (ASLVersion < 410))
        {
            await AddToObjectProperties("visited", _rooms[id].ObjId, ctx);
        }

        await ShowRoomInfo(room, ctx);
        await UpdateItems(ctx);

        // Find script lines and execute them.

        if (!string.IsNullOrEmpty(_rooms[id].Script))
        {
            var script = _rooms[id].Script;
            await ExecuteScript(script, ctx);
        }

        if (ASLVersion >= 410)
        {
            await AddToObjectProperties("visited", _rooms[id].ObjId, ctx);
        }
    }

    private void UpdateDirButtons(string dirs, Context ctx)
    {
        var compassExits = new List<ListData>();

        if (Strings.InStr(dirs, "n") > 0)
        {
            AddCompassExit(compassExits, "north");
        }

        if (Strings.InStr(dirs, "s") > 0)
        {
            AddCompassExit(compassExits, "south");
        }

        if (Strings.InStr(dirs, "w") > 0)
        {
            AddCompassExit(compassExits, "west");
        }

        if (Strings.InStr(dirs, "e") > 0)
        {
            AddCompassExit(compassExits, "east");
        }

        if (Strings.InStr(dirs, "o") > 0)
        {
            AddCompassExit(compassExits, "out");
        }

        if (Strings.InStr(dirs, "a") > 0)
        {
            AddCompassExit(compassExits, "northeast");
        }

        if (Strings.InStr(dirs, "b") > 0)
        {
            AddCompassExit(compassExits, "northwest");
        }

        if (Strings.InStr(dirs, "c") > 0)
        {
            AddCompassExit(compassExits, "southeast");
        }

        if (Strings.InStr(dirs, "d") > 0)
        {
            AddCompassExit(compassExits, "southwest");
        }

        if (Strings.InStr(dirs, "u") > 0)
        {
            AddCompassExit(compassExits, "up");
        }

        if (Strings.InStr(dirs, "f") > 0)
        {
            AddCompassExit(compassExits, "down");
        }

        _compassExits = compassExits;
        UpdateExitsList();
    }

    private void AddCompassExit(List<ListData> exitList, string name)
    {
        exitList.Add(new ListData(name, _listVerbs[ListType.ExitsList]));
    }

    private async Task<string> UpdateDoorways(int roomId, Context ctx)
    {
        var roomDisplayText = "";
        var outPlace = "";
        var directions = "";
        var nsew = "";
        var outPlaceName = "";
        var outPlacePrefix = "";

        var n = "north";
        var s = "south";
        var e = "east";
        var w = "west";
        var ne = "northeast";
        var nw = "northwest";
        var se = "southeast";
        var sw = "southwest";
        var u = "up";
        var d = "down";

        if (ASLVersion >= 410)
        {
            (roomDisplayText, directions) = await _rooms[roomId].Exits.GetAvailableDirectionsDescription();
        }
        else
        {
            if (!string.IsNullOrEmpty(_rooms[roomId].Out.Text))
            {
                outPlace = _rooms[roomId].Out.Text;

                // remove any prefix semicolon from printed text
                var scp = Strings.InStr(outPlace, ";");
                if (scp == 0)
                {
                    outPlaceName = outPlace;
                }
                else
                {
                    outPlaceName = Strings.Trim(Strings.Mid(outPlace, scp + 1));
                    outPlacePrefix = Strings.Trim(Strings.Left(outPlace, scp - 1));
                    outPlace = outPlacePrefix + " " + outPlaceName;
                }
            }

            if (!string.IsNullOrEmpty(_rooms[roomId].North.Data))
            {
                nsew = nsew + "|b" + n + "|xb, ";
                directions = directions + "n";
            }

            if (!string.IsNullOrEmpty(_rooms[roomId].South.Data))
            {
                nsew = nsew + "|b" + s + "|xb, ";
                directions = directions + "s";
            }

            if (!string.IsNullOrEmpty(_rooms[roomId].East.Data))
            {
                nsew = nsew + "|b" + e + "|xb, ";
                directions = directions + "e";
            }

            if (!string.IsNullOrEmpty(_rooms[roomId].West.Data))
            {
                nsew = nsew + "|b" + w + "|xb, ";
                directions = directions + "w";
            }

            if (!string.IsNullOrEmpty(_rooms[roomId].NorthEast.Data))
            {
                nsew = nsew + "|b" + ne + "|xb, ";
                directions = directions + "a";
            }

            if (!string.IsNullOrEmpty(_rooms[roomId].NorthWest.Data))
            {
                nsew = nsew + "|b" + nw + "|xb, ";
                directions = directions + "b";
            }

            if (!string.IsNullOrEmpty(_rooms[roomId].SouthEast.Data))
            {
                nsew = nsew + "|b" + se + "|xb, ";
                directions = directions + "c";
            }

            if (!string.IsNullOrEmpty(_rooms[roomId].SouthWest.Data))
            {
                nsew = nsew + "|b" + sw + "|xb, ";
                directions = directions + "d";
            }

            if (!string.IsNullOrEmpty(_rooms[roomId].Up.Data))
            {
                nsew = nsew + "|b" + u + "|xb, ";
                directions = directions + "u";
            }

            if (!string.IsNullOrEmpty(_rooms[roomId].Down.Data))
            {
                nsew = nsew + "|b" + d + "|xb, ";
                directions = directions + "f";
            }

            if (!string.IsNullOrEmpty(outPlace))
            {
                // see if outside has an alias

                var outPlaceAlias = _rooms[GetRoomID(outPlaceName, ctx)].RoomAlias;
                if (string.IsNullOrEmpty(outPlaceAlias))
                {
                    outPlaceAlias = outPlace;
                }
                else if (ASLVersion >= 360)
                {
                    if (!string.IsNullOrEmpty(outPlacePrefix))
                    {
                        outPlaceAlias = outPlacePrefix + " " + outPlaceAlias;
                    }
                }

                roomDisplayText = roomDisplayText + "You can go |bout|xb to " + outPlaceAlias + ".";
                if (!string.IsNullOrEmpty(nsew))
                {
                    roomDisplayText = roomDisplayText + " ";
                }

                directions = directions + "o";
                if (ASLVersion >= 280)
                {
                    await SetStringContents("quest.doorways.out", outPlaceName, ctx);
                }
                else
                {
                    await SetStringContents("quest.doorways.out", outPlaceAlias, ctx);
                }

                await SetStringContents("quest.doorways.out.display", outPlaceAlias, ctx);
            }
            else
            {
                await SetStringContents("quest.doorways.out", "", ctx);
                await SetStringContents("quest.doorways.out.display", "", ctx);
            }

            if (!string.IsNullOrEmpty(nsew))
            {
                // strip final comma
                nsew = Strings.Left(nsew, Strings.Len(nsew) - 2);
                var cp = Strings.InStr(nsew, ",");
                if (cp != 0)
                {
                    var finished = false;
                    do
                    {
                        var ncp = Strings.InStr(cp + 1, nsew, ",");
                        if (ncp == 0)
                        {
                            finished = true;
                        }
                        else
                        {
                            cp = ncp;
                        }
                    } while (!finished);

                    nsew = Strings.Trim(Strings.Left(nsew, cp - 1)) + " or " + Strings.Trim(Strings.Mid(nsew, cp + 1));
                }

                roomDisplayText = roomDisplayText + "You can go " + nsew + ".";
                await SetStringContents("quest.doorways.dirs", nsew, ctx);
            }
            else
            {
                await SetStringContents("quest.doorways.dirs", "", ctx);
            }
        }

        UpdateDirButtons(directions, ctx);

        return roomDisplayText;
    }

    private void UpdateExitsList()
    {
        // The Quest 5.0 Player takes a combined list of compass and "go to" exits, whereas the
        // ASL4 code produces these separately. So we keep track of them separately and then
        // merge to send to the Player.

        var mergedList = new List<ListData>();

        foreach (var listItem in _compassExits)
        {
            mergedList.Add(listItem);
        }

        foreach (var listItem in _gotoExits)
        {
            mergedList.Add(listItem);
        }

        UpdateList?.Invoke(ListType.ExitsList, mergedList);
    }

    private string GetGoToExits(int roomId, Context ctx)
    {
        var placeList = "";
        string shownPlaceName;

        for (int i = 1, loopTo = _rooms[roomId].NumberPlaces; i <= loopTo; i++)
        {
            if ((ASLVersion >= 311) & string.IsNullOrEmpty(_rooms[roomId].Places[i].Script))
            {
                var PlaceID = GetRoomID(_rooms[roomId].Places[i].PlaceName, ctx);
                if (PlaceID == 0)
                {
                    LogASLError("No such room '" + _rooms[roomId].Places[i].PlaceName + "'", LogType.WarningError);
                    shownPlaceName = _rooms[roomId].Places[i].PlaceName;
                }
                else if (!string.IsNullOrEmpty(_rooms[PlaceID].RoomAlias))
                {
                    shownPlaceName = _rooms[PlaceID].RoomAlias;
                }
                else
                {
                    shownPlaceName = _rooms[roomId].Places[i].PlaceName;
                }
            }
            else
            {
                shownPlaceName = _rooms[roomId].Places[i].PlaceName;
            }

            var shownPrefix = _rooms[roomId].Places[i].Prefix;
            if (!string.IsNullOrEmpty(shownPrefix))
            {
                shownPrefix = shownPrefix + " ";
            }

            placeList = placeList + shownPrefix + "|b" + shownPlaceName + "|xb, ";
        }

        return placeList;
    }

    private RoomExit FindExit(string tag)
    {
        // e.g. Takes a tag of the form "room; north" and return's the north exit of room.

        var @params = Strings.Split(tag, ";");
        if (Information.UBound(@params) < 1)
        {
            LogASLError("No exit specified in '" + tag + "'", LogType.WarningError);
            return new RoomExit(this);
        }

        var room = Strings.Trim(@params[0]);
        var exitName = Strings.Trim(@params[1]);

        var roomId = GetRoomID(room, _nullContext);

        if (roomId == 0)
        {
            LogASLError("Can't find room '" + room + "'", LogType.WarningError);
            return null;
        }

        var exits = _rooms[roomId].Exits;
        var dir = exits.GetDirectionEnum(ref exitName);
        if (dir == Direction.None)
        {
            if (exits.GetPlaces().ContainsKey(exitName))
            {
                return exits.GetPlaces()[exitName];
            }
        }
        else
        {
            return exits.GetDirectionExit(ref dir);
        }

        return null;
    }

    private async Task ExecuteLock(string tag, bool @lock)
    {
        RoomExit roomExit;

        roomExit = FindExit(tag);

        if (roomExit is null)
        {
            LogASLError("Can't find exit '" + tag + "'", LogType.WarningError);
            return;
        }

        await roomExit.SetIsLocked(@lock);
    }
}
