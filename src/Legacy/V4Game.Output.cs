using System.Diagnostics;
using System.Text;
using Microsoft.VisualBasic;
using QuestViva.Common;
using Strings = Microsoft.VisualBasic.Strings;

namespace QuestViva.Legacy;

public partial class V4Game
{
    private string GetHTMLColour(string colour, string defaultColour)
    {
        // Converts a Quest foreground or background colour setting into an HTML colour

        colour = Strings.LCase(colour);

        if (string.IsNullOrEmpty(colour) | (colour == "0"))
        {
            colour = defaultColour;
        }

        switch (colour ?? "")
        {
            case "white":
            {
                return "FFFFFF";
            }
            case "black":
            {
                return "000000";
            }
            case "blue":
            {
                return "0000FF";
            }
            case "yellow":
            {
                return "FFFF00";
            }
            case "red":
            {
                return "FF0000";
            }
            case "green":
            {
                return "00FF00";
            }

            default:
            {
                return colour;
            }
        }
    }

    private void DoPrint(string text)
    {
        PrintText?.Invoke(_textFormatter.OutputHTML(text));
    }

    private void DoClear()
    {
        _player.ClearScreen();
    }

    private async Task<string> DisplayStatusVariableInfo(int id, VarType type, Context ctx)
    {
        var displayData = "";
        int ep;

        if (type == VarType.String)
        {
            displayData = await ConvertVarsIn(_stringVariable[id].DisplayString, ctx);
            ep = Strings.InStr(displayData, "!");

            if (ep != 0)
            {
                displayData = Strings.Left(displayData, ep - 1) + _stringVariable[id].VariableContents[0] +
                              Strings.Mid(displayData, ep + 1);
            }
        }
        else if (type == VarType.Numeric)
        {
            if (_numericVariable[id].NoZeroDisplay &
                (Conversion.Val(_numericVariable[id].VariableContents[0]) == 0d))
            {
                return "";
            }

            displayData = await ConvertVarsIn(_numericVariable[id].DisplayString, ctx);
            ep = Strings.InStr(displayData, "!");

            if (ep != 0)
            {
                displayData = Strings.Left(displayData, ep - 1) + _numericVariable[id].VariableContents[0] +
                              Strings.Mid(displayData, ep + 1);
            }

            if (Strings.InStr(displayData, "*") > 0)
            {
                var firstStar = Strings.InStr(displayData, "*");
                var secondStar = Strings.InStr(firstStar + 1, displayData, "*");
                var beforeStar = Strings.Left(displayData, firstStar - 1);
                var afterStar = Strings.Mid(displayData, secondStar + 1);
                var betweenStar = Strings.Mid(displayData, firstStar + 1, secondStar - firstStar - 1);

                if (Conversion.Val(_numericVariable[id].VariableContents[0]) != 1d)
                {
                    displayData = beforeStar + betweenStar + afterStar;
                }
                else
                {
                    displayData = beforeStar + afterStar;
                }
            }
        }

        return displayData;
    }

    private async Task PlayMediaAsync(string filename)
    {
        await PlayMediaAsync(filename, false, false);
    }

    private async Task PlayMediaAsync(string filename, bool sync, bool looped)
    {
        if (filename.Length == 0)
        {
            _player.StopSound();
            return;
        }

        if (looped & sync) sync = false;
        await _player.PlaySoundAsync(filename, sync, looped);

        if (sync)
        {
            _waitTcs = new TaskCompletionSource();
            SignalTurnSuspended();
            await _waitTcs.Task;
        }
    }

    private async Task PlayWavAsync(string parameter)
    {
        var sync = false;
        var looped = false;
        var @params = new List<string>(parameter.Split(';'));

        @params = new List<string>(@params.Select(p => Strings.Trim(p)));

        var filename = @params[0];

        if (@params.Contains("loop"))
        {
            looped = true;
        }

        if (@params.Contains("sync"))
        {
            sync = true;
        }

        if ((filename.Length > 0) & (Strings.InStr(filename, ".") == 0))
        {
            filename = filename + ".wav";
        }

        await PlayMediaAsync(filename, sync, looped);
    }

    private void SetBackground(string col)
    {
        _player.SetBackground("#" + GetHTMLColour(col, "white"));
    }

    private void SetForeground(string col)
    {
        _player.SetForeground("#" + GetHTMLColour(col, "black"));
    }

    private void SetFont(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = _defaultFontName;
        }

        _player.SetFont(name);
    }

    private void SetFontSize(double size)
    {
        if (size == 0d)
        {
            size = _defaultFontSize;
        }

        _player.SetFontSize(size.ToString());
    }

    private async Task ShowPictureInTextAsync(string filename)
    {
        if (!_useStaticFrameForPictures)
        {
            await _player.ShowPictureAsync(filename);
        }
        else
        {
            // Workaround for a particular game which expects pictures to be in a popup window -
            // use the static picture frame feature so that image is not cleared
            _player.SetPanelContents("<img src=\"" + await _player.GetUrlAsync(filename) + "\" onload=\"setPanelHeight()\"/>");
        }
    }

    private void Speak(string text)
    {
        _player.Speak(text);
    }

    private void AddToObjectList(List<ListData> objList, List<ListData> exitList, string name, Thing type)
    {
        name = CapFirst(name);

        if (type == Thing.Room)
        {
            objList.Add(new ListData(name, _listVerbs[ListType.ExitsList]));
            exitList.Add(new ListData(name, _listVerbs[ListType.ExitsList]));
        }
        else
        {
            objList.Add(new ListData(name, _listVerbs[ListType.ObjectsList]));
        }
    }

    private async Task ShowGameAbout(Context ctx)
    {
        var version = await FindStatement(GetDefineBlock("game"), "game version");
        var author = await FindStatement(GetDefineBlock("game"), "game author");
        var copyright = await FindStatement(GetDefineBlock("game"), "game copyright");
        var info = await FindStatement(GetDefineBlock("game"), "game info");

        await Print("|bGame name:|cl  " + _gameName + "|cb|xb", ctx);
        if (!string.IsNullOrEmpty(version))
        {
            await Print("|bVersion:|xb    " + version, ctx);
        }

        if (!string.IsNullOrEmpty(author))
        {
            await Print("|bAuthor:|xb     " + author, ctx);
        }

        if (!string.IsNullOrEmpty(copyright))
        {
            await Print("|bCopyright:|xb  " + copyright, ctx);
        }

        if (!string.IsNullOrEmpty(info))
        {
            await Print("", ctx);
            await Print(info, ctx);
        }
    }

    private async Task ShowPicture(string filename)
    {
        // In Quest 4.x this function would be used for showing a picture in a popup window, but
        // this is no longer supported - ALL images are displayed in-line with the game text. Any
        // image caption is displayed as text, and any image size specified is ignored.

        var caption = "";

        if (Strings.InStr(filename, ";") != 0)
        {
            caption = Strings.Trim(Strings.Mid(filename, Strings.InStr(filename, ";") + 1));
            filename = Strings.Trim(Strings.Left(filename, Strings.InStr(filename, ";") - 1));
        }

        if (Strings.InStr(filename, "@") != 0)
        {
            // size is ignored
            filename = Strings.Trim(Strings.Left(filename, Strings.InStr(filename, "@") - 1));
        }

        if (caption.Length > 0)
        {
            await Print(caption, _nullContext);
        }

        await ShowPictureInTextAsync(filename);
    }

    private async Task DisplayTextSection(string section, Context ctx)
    {
        DefineBlock block;
        block = await DefineBlockParam("text", section);

        if (block.StartLine == 0)
        {
            return;
        }

        for (int i = block.StartLine + 1, loopTo = block.EndLine - 1; i <= loopTo; i++)
        {
            if (ASLVersion >= 392)
            {
                // Convert string variables etc.
                await Print(await GetParameter("<" + _lines[i] + ">", ctx), ctx);
            }
            else
            {
                await Print(_lines[i], ctx);
            }
        }

        await Print("", ctx);
    }

    internal async Task Print(string txt, Context ctx)
    {
        if (!_outPutOn)
        {
            return;
        }

        var printString = "";

        if (string.IsNullOrEmpty(txt))
        {
            DoPrint(printString);
        }
        else
        {
            for (int i = 1, loopTo = Strings.Len(txt); i <= loopTo; i++)
            {
                var printThis = true;

                if (Strings.Mid(txt, i, 2) == "|w")
                {
                    DoPrint(printString);
                    printString = "";
                    printThis = false;
                    i = i + 1;
                    await DoWaitAsync();
                }

                else if (Strings.Mid(txt, i, 2) == "|c")
                {
                    switch (Strings.Mid(txt, i, 3) ?? "")
                    {
                        // Do nothing - we don't want to remove the colour formatting codes.
                        case "|cb":
                        case "|cr":
                        case "|cl":
                        case "|cy":
                        case "|cg":
                        {
                            break;
                        }

                        default:
                        {
                            DoPrint(printString);
                            printString = "";
                            printThis = false;
                            i = i + 1;
                            DoClear();
                            break;
                        }
                    }
                }

                if (printThis)
                {
                    printString = printString + Strings.Mid(txt, i, 1);
                }
            }

            if (!string.IsNullOrEmpty(printString))
            {
                DoPrint(printString);
            }
        }
    }

    private async Task UpdateItems(Context ctx)
    {
        // displays the items a player has
        var invList = new List<ListData>();

        if (!_outPutOn)
        {
            return;
        }

        string name;

        if (ASLVersion >= 280)
        {
            for (int i = 1, loopTo = _numberObjs; i <= loopTo; i++)
            {
                if ((_objs[i].ContainerRoom == "inventory") & _objs[i].Exists & _objs[i].Visible)
                {
                    if (string.IsNullOrEmpty(_objs[i].ObjectAlias))
                    {
                        name = _objs[i].ObjectName;
                    }
                    else
                    {
                        name = _objs[i].ObjectAlias;
                    }

                    invList.Add(new ListData(CapFirst(name), _listVerbs[ListType.InventoryList]));
                }
            }
        }
        else
        {
            for (int j = 1, loopTo1 = _numberItems; j <= loopTo1; j++)
            {
                if (_items[j].Got)
                {
                    invList.Add(new ListData(CapFirst(_items[j].Name), _listVerbs[ListType.InventoryList]));
                }
            }
        }

        UpdateList?.Invoke(ListType.InventoryList, invList);

        if (ASLVersion >= 284)
        {
            await UpdateStatusVars(ctx);
        }
        else if (_numCollectables > 0)
        {
            var status = "";

            for (int j = 1, loopTo2 = _numCollectables; j <= loopTo2; j++)
            {
                var k = DisplayCollectableInfo(j);
                if (k != "<null>")
                {
                    if (status.Length > 0)
                    {
                        status += Environment.NewLine;
                    }

                    status += k;
                }
            }

            _player.SetStatusText(status);
        }
    }

    private async Task UpdateObjectList(Context ctx)
    {
        // Updates object list
        string shownPlaceName;
        string objSuffix;
        var charsViewable = "";
        var charsFound = default(int);
        string noFormatObjsViewable, charList;
        var objsViewable = "";
        var objsFound = default(int);
        string objListString, noFormatObjListString;

        if (!_outPutOn)
        {
            return;
        }

        var objList = new List<ListData>();
        var exitList = new List<ListData>();

        // find the room
        DefineBlock roomBlock;
        roomBlock = await DefineBlockParam("room", _currentRoom);

        // FIND CHARACTERS ===
        if (ASLVersion < 281)
        {
            // go through Chars() array
            for (int i = 1, loopTo = _numberChars; i <= loopTo; i++)
            {
                if (((_chars[i].ContainerRoom ?? "") == (_currentRoom ?? "")) & _chars[i].Exists & _chars[i].Visible)
                {
                    AddToObjectList(objList, exitList, _chars[i].ObjectName, Thing.Character);
                    charsViewable = charsViewable + _chars[i].Prefix + "|b" + _chars[i].ObjectName + "|xb" +
                                    _chars[i].Suffix + ", ";
                    charsFound = charsFound + 1;
                }
            }

            if (charsFound == 0)
            {
                await SetStringContents("quest.characters", "", ctx);
            }
            else
            {
                // chop off final comma and add full stop (.)
                charList = Strings.Left(charsViewable, Strings.Len(charsViewable) - 2);
                await SetStringContents("quest.characters", charList, ctx);
            }
        }

        // FIND OBJECTS
        noFormatObjsViewable = "";

        for (int i = 1, loopTo1 = _numberObjs; i <= loopTo1; i++)
        {
            if (((Strings.LCase(_objs[i].ContainerRoom) ?? "") == (Strings.LCase(_currentRoom) ?? "")) &
                _objs[i].Exists &
                _objs[i].Visible & !_objs[i].IsExit)
            {
                objSuffix = _objs[i].Suffix;
                if (!string.IsNullOrEmpty(objSuffix))
                {
                    objSuffix = " " + objSuffix;
                }

                if (string.IsNullOrEmpty(_objs[i].ObjectAlias))
                {
                    AddToObjectList(objList, exitList, _objs[i].ObjectName, Thing.Object);
                    objsViewable = objsViewable + _objs[i].Prefix + "|b" + _objs[i].ObjectName + "|xb" + objSuffix +
                                   ", ";
                    noFormatObjsViewable = noFormatObjsViewable + _objs[i].Prefix + _objs[i].ObjectName + ", ";
                }
                else
                {
                    AddToObjectList(objList, exitList, _objs[i].ObjectAlias, Thing.Object);
                    objsViewable = objsViewable + _objs[i].Prefix + "|b" + _objs[i].ObjectAlias + "|xb" + objSuffix +
                                   ", ";
                    noFormatObjsViewable = noFormatObjsViewable + _objs[i].Prefix + _objs[i].ObjectAlias + ", ";
                }

                objsFound = objsFound + 1;
            }
        }

        if (objsFound != 0)
        {
            objListString = Strings.Left(objsViewable, Strings.Len(objsViewable) - 2);
            noFormatObjListString = Strings.Left(noFormatObjsViewable, Strings.Len(noFormatObjsViewable) - 2);
            await SetStringContents("quest.objects",
                Strings.Left(noFormatObjsViewable, Strings.Len(noFormatObjsViewable) - 2), ctx);
            await SetStringContents("quest.formatobjects", objListString, ctx);
        }
        else
        {
            await SetStringContents("quest.objects", "", ctx);
            await SetStringContents("quest.formatobjects", "", ctx);
        }

        // FIND DOORWAYS
        int roomId;
        roomId = GetRoomID(_currentRoom, ctx);
        if (roomId > 0)
        {
            if (ASLVersion >= 410)
            {
                foreach (var roomExit in _rooms[roomId].Exits.GetPlaces().Values)
                {
                    AddToObjectList(objList, exitList, roomExit.GetDisplayName(), Thing.Room);
                }
            }
            else
            {
                var r = _rooms[roomId];

                for (int i = 1, loopTo2 = r.NumberPlaces; i <= loopTo2; i++)
                {
                    if ((ASLVersion >= 311) & string.IsNullOrEmpty(_rooms[roomId].Places[i].Script))
                    {
                        var PlaceID = GetRoomID(_rooms[roomId].Places[i].PlaceName, ctx);
                        if (PlaceID == 0)
                        {
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

                    AddToObjectList(objList, exitList, shownPlaceName, Thing.Room);
                }
            }
        }

        UpdateList?.Invoke(ListType.ObjectsList, objList);
        _gotoExits = exitList;
        UpdateExitsList();
    }

    private async Task UpdateStatusVars(Context ctx)
    {
        string displayData;
        var status = "";

        if (_numDisplayStrings > 0)
        {
            for (int i = 1, loopTo = _numDisplayStrings; i <= loopTo; i++)
            {
                displayData = await DisplayStatusVariableInfo(i, VarType.String, ctx);

                if (!string.IsNullOrEmpty(displayData))
                {
                    if (status.Length > 0)
                    {
                        status += Environment.NewLine;
                    }

                    status += displayData;
                }
            }
        }

        if (_numDisplayNumerics > 0)
        {
            for (int i = 1, loopTo1 = _numDisplayNumerics; i <= loopTo1; i++)
            {
                displayData = await DisplayStatusVariableInfo(i, VarType.Numeric, ctx);
                if (!string.IsNullOrEmpty(displayData))
                {
                    if (status.Length > 0)
                    {
                        status += Environment.NewLine;
                    }

                    status += displayData;
                }
            }
        }

        _player.SetStatusText(status);
    }
}
