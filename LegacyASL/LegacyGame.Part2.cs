using System.Diagnostics;
using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace TextAdventures.Quest.LegacyASL;

public partial class LegacyGame
{
    private void RestoreGameData(string fileData)
    {
        string appliesTo;
        var data = "";
        int objId, timerNum = default;
        int varUbound;
        bool found;
        var numStoredData = default(int);
        var storedData = new ChangeType[1];
        var decryptedFile = new StringBuilder();

        // Decrypt file
        for (int i = 1, loopTo = Strings.Len(fileData); i <= loopTo; i++)
        {
            decryptedFile.Append(Strings.Chr(255 - Strings.Asc(Strings.Mid(fileData, i, 1))));
        }

        _fileData = decryptedFile.ToString();
        _currentRoom = GetNextChunk();

        // OBJECTS

        var numData = Conversions.ToInteger(GetNextChunk());
        var createdObjects = new List<string>();

        for (int i = 1, loopTo1 = numData; i <= loopTo1; i++)
        {
            appliesTo = GetNextChunk();
            data = GetNextChunk();

            // As of Quest 4.0, properties and actions are put into StoredData while we load the file,
            // and then processed later. This is so any created rooms pick up their properties - otherwise
            // we try to set them before they've been created.

            if (BeginsWith(data, "properties ") | BeginsWith(data, "action "))
            {
                numStoredData = numStoredData + 1;
                Array.Resize(ref storedData, numStoredData + 1);
                storedData[numStoredData] = new ChangeType();
                storedData[numStoredData].AppliesTo = appliesTo;
                storedData[numStoredData].Change = data;
            }
            else if (BeginsWith(data, "create "))
            {
                var createData = appliesTo + ";" + GetEverythingAfter(data, "create ");
                // workaround bug where duplicate "create" entries appear in the restore data
                if (!createdObjects.Contains(createData))
                {
                    ExecuteCreate("object <" + createData + ">", _nullContext);
                    createdObjects.Add(createData);
                }
            }
            else
            {
                LogASLError("QSG Error: Unrecognised item '" + appliesTo + "; " + data + "'", LogType.InternalError);
            }
        }

        numData = Conversions.ToInteger(GetNextChunk());
        for (int i = 1, loopTo2 = numData; i <= loopTo2; i++)
        {
            appliesTo = GetNextChunk();
            data = GetFileDataChars(2);
            objId = GetObjectIdNoAlias(appliesTo);

            if (Strings.Left(data, 1) == "\u0001")
            {
                _objs[objId].Exists = true;
            }
            else
            {
                _objs[objId].Exists = false;
            }

            if (Strings.Right(data, 1) == "\u0001")
            {
                _objs[objId].Visible = true;
            }
            else
            {
                _objs[objId].Visible = false;
            }

            _objs[objId].ContainerRoom = GetNextChunk();
        }

        // ROOMS

        numData = Conversions.ToInteger(GetNextChunk());

        for (int i = 1, loopTo3 = numData; i <= loopTo3; i++)
        {
            appliesTo = GetNextChunk();
            data = GetNextChunk();

            if (BeginsWith(data, "exit "))
            {
                ExecuteCreate(data, _nullContext);
            }
            else if (data == "create")
            {
                ExecuteCreate("room <" + appliesTo + ">", _nullContext);
            }
            else if (BeginsWith(data, "destroy exit "))
            {
                DestroyExit(appliesTo + "; " + GetEverythingAfter(data, "destroy exit "), _nullContext);
            }
        }

        // Now go through and apply object properties and actions

        for (int i = 1, loopTo4 = numStoredData; i <= loopTo4; i++)
        {
            var d = storedData[i];
            if (BeginsWith(d.Change, "properties "))
            {
                AddToObjectProperties(GetEverythingAfter(d.Change, "properties "), GetObjectIdNoAlias(d.AppliesTo),
                    _nullContext);
            }
            else if (BeginsWith(d.Change, "action "))
            {
                AddToObjectActions(GetEverythingAfter(d.Change, "action "), GetObjectIdNoAlias(d.AppliesTo),
                    _nullContext);
            }
        }

        // TIMERS

        numData = Conversions.ToInteger(GetNextChunk());
        for (int i = 1, loopTo5 = numData; i <= loopTo5; i++)
        {
            found = false;
            appliesTo = GetNextChunk();
            for (int j = 1, loopTo6 = _numberTimers; j <= loopTo6; j++)
            {
                if ((_timers[j].TimerName ?? "") == (appliesTo ?? ""))
                {
                    timerNum = j;
                    found = true;
                    break;
                }
            }

            if (found)
            {
                var t = _timers[timerNum];
                var thisChar = GetFileDataChars(1);

                if (thisChar == "\u0001")
                {
                    t.TimerActive = true;
                }
                else
                {
                    t.TimerActive = false;
                }

                t.TimerInterval = Conversions.ToInteger(GetNextChunk());
                t.TimerTicks = Conversions.ToInteger(GetNextChunk());
            }
        }

        // STRING VARIABLES

        // Set this flag so we don't run any status variable onchange scripts while restoring
        _gameIsRestoring = true;

        numData = Conversions.ToInteger(GetNextChunk());
        for (int i = 1, loopTo7 = numData; i <= loopTo7; i++)
        {
            appliesTo = GetNextChunk();
            varUbound = Conversions.ToInteger(GetNextChunk());

            if (varUbound == 0)
            {
                data = GetNextChunk();
                SetStringContents(appliesTo, data, _nullContext);
            }
            else
            {
                for (int j = 0, loopTo8 = varUbound; j <= loopTo8; j++)
                {
                    data = GetNextChunk();
                    SetStringContents(appliesTo, data, _nullContext, j);
                }
            }
        }

        // NUMERIC VARIABLES

        numData = Conversions.ToInteger(GetNextChunk());
        for (int i = 1, loopTo9 = numData; i <= loopTo9; i++)
        {
            appliesTo = GetNextChunk();
            varUbound = Conversions.ToInteger(GetNextChunk());

            if (varUbound == 0)
            {
                data = GetNextChunk();
                SetNumericVariableContents(appliesTo, Conversion.Val(data), _nullContext);
            }
            else
            {
                for (int j = 0, loopTo10 = varUbound; j <= loopTo10; j++)
                {
                    data = GetNextChunk();
                    SetNumericVariableContents(appliesTo, Conversion.Val(data), _nullContext, j);
                }
            }
        }

        _gameIsRestoring = false;
    }

    private void SetBackground(string col)
    {
        _player.SetBackground("#" + GetHTMLColour(col, "white"));
    }

    private void SetForeground(string col)
    {
        _player.SetForeground("#" + GetHTMLColour(col, "black"));
    }

    private void SetDefaultPlayerErrorMessages()
    {
        _playerErrorMessageString[(int) PlayerError.BadCommand] =
            "I don't understand your command. Type HELP for a list of valid commands.";
        _playerErrorMessageString[(int) PlayerError.BadGo] =
            "I don't understand your use of 'GO' - you must either GO in some direction, or GO TO a place.";
        _playerErrorMessageString[(int) PlayerError.BadGive] = "You didn't say who you wanted to give that to.";
        _playerErrorMessageString[(int) PlayerError.BadCharacter] = "I can't see anybody of that name here.";
        _playerErrorMessageString[(int) PlayerError.NoItem] = "You don't have that.";
        _playerErrorMessageString[(int) PlayerError.ItemUnwanted] =
            "#quest.error.gender# doesn't want #quest.error.article#.";
        _playerErrorMessageString[(int) PlayerError.BadLook] = "You didn't say what you wanted to look at.";
        _playerErrorMessageString[(int) PlayerError.BadThing] = "I can't see that here.";
        _playerErrorMessageString[(int) PlayerError.DefaultLook] = "Nothing out of the ordinary.";
        _playerErrorMessageString[(int) PlayerError.DefaultSpeak] = "#quest.error.gender# says nothing.";
        _playerErrorMessageString[(int) PlayerError.BadItem] = "I can't see that anywhere.";
        _playerErrorMessageString[(int) PlayerError.DefaultTake] = "You pick #quest.error.article# up.";
        _playerErrorMessageString[(int) PlayerError.BadUse] = "You didn't say what you wanted to use that on.";
        _playerErrorMessageString[(int) PlayerError.DefaultUse] = "You can't use that here.";
        _playerErrorMessageString[(int) PlayerError.DefaultOut] = "There's nowhere you can go out to around here.";
        _playerErrorMessageString[(int) PlayerError.BadPlace] = "You can't go there.";
        _playerErrorMessageString[(int) PlayerError.DefaultExamine] = "Nothing out of the ordinary.";
        _playerErrorMessageString[(int) PlayerError.BadTake] = "You can't take #quest.error.article#.";
        _playerErrorMessageString[(int) PlayerError.CantDrop] = "You can't drop that here.";
        _playerErrorMessageString[(int) PlayerError.DefaultDrop] = "You drop #quest.error.article#.";
        _playerErrorMessageString[(int) PlayerError.BadDrop] = "You are not carrying such a thing.";
        _playerErrorMessageString[(int) PlayerError.BadPronoun] =
            "I don't know what '#quest.error.pronoun#' you are referring to.";
        _playerErrorMessageString[(int) PlayerError.BadExamine] = "You didn't say what you wanted to examine.";
        _playerErrorMessageString[(int) PlayerError.AlreadyOpen] = "It is already open.";
        _playerErrorMessageString[(int) PlayerError.AlreadyClosed] = "It is already closed.";
        _playerErrorMessageString[(int) PlayerError.CantOpen] = "You can't open that.";
        _playerErrorMessageString[(int) PlayerError.CantClose] = "You can't close that.";
        _playerErrorMessageString[(int) PlayerError.DefaultOpen] = "You open it.";
        _playerErrorMessageString[(int) PlayerError.DefaultClose] = "You close it.";
        _playerErrorMessageString[(int) PlayerError.BadPut] =
            "You didn't specify what you wanted to put #quest.error.article# on or in.";
        _playerErrorMessageString[(int) PlayerError.CantPut] = "You can't put that there.";
        _playerErrorMessageString[(int) PlayerError.DefaultPut] = "Done.";
        _playerErrorMessageString[(int) PlayerError.CantRemove] = "You can't remove that.";
        _playerErrorMessageString[(int) PlayerError.AlreadyPut] = "It is already there.";
        _playerErrorMessageString[(int) PlayerError.DefaultRemove] = "Done.";
        _playerErrorMessageString[(int) PlayerError.Locked] = "The exit is locked.";
        _playerErrorMessageString[(int) PlayerError.DefaultWait] = "Press a key to continue...";
        _playerErrorMessageString[(int) PlayerError.AlreadyTaken] = "You already have that.";
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

    private void SetNumericVariableContents(string name, double content, Context ctx, int arrayIndex = 0)
    {
        var numNumber = default(int);
        var exists = false;

        if (Information.IsNumeric(name))
        {
            LogASLError(
                "Illegal numeric variable name '" + name +
                "' - check you didn't put % around the variable name in the ASL code", LogType.WarningError);
            return;
        }

        // First, see if variable already exists. If it does,
        // modify it. If not, create it.

        if (_numberNumericVariables > 0)
        {
            for (int i = 1, loopTo = _numberNumericVariables; i <= loopTo; i++)
            {
                if ((Strings.LCase(_numericVariable[i].VariableName) ?? "") == (Strings.LCase(name) ?? ""))
                {
                    numNumber = i;
                    exists = true;
                    break;
                }
            }
        }

        if (exists == false)
        {
            _numberNumericVariables = _numberNumericVariables + 1;
            numNumber = _numberNumericVariables;
            Array.Resize(ref _numericVariable, numNumber + 1);
            _numericVariable[numNumber] = new VariableType();
            _numericVariable[numNumber].VariableUBound = arrayIndex;
        }

        if (arrayIndex > _numericVariable[numNumber].VariableUBound)
        {
            Array.Resize(ref _numericVariable[numNumber].VariableContents, arrayIndex + 1);
            _numericVariable[numNumber].VariableUBound = arrayIndex;
        }

        // Now, set the contents
        _numericVariable[numNumber].VariableName = name;
        Array.Resize(ref _numericVariable[numNumber].VariableContents, _numericVariable[numNumber].VariableUBound + 1);
        _numericVariable[numNumber].VariableContents[arrayIndex] = content.ToString();

        if (!string.IsNullOrEmpty(_numericVariable[numNumber].OnChangeScript) & !_gameIsRestoring)
        {
            var script = _numericVariable[numNumber].OnChangeScript;
            ExecuteScript(script, ctx);
        }

        if (!string.IsNullOrEmpty(_numericVariable[numNumber].DisplayString))
        {
            UpdateStatusVars(ctx);
        }
    }

    private void SetOpenClose(string name, bool open, Context ctx)
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

        DoOpenClose(id, open, false, ctx);
    }

    private void SetTimerState(string name, bool state)
    {
        for (int i = 1, loopTo = _numberTimers; i <= loopTo; i++)
        {
            if ((Strings.LCase(name) ?? "") == (Strings.LCase(_timers[i].TimerName) ?? ""))
            {
                _timers[i].TimerActive = state;
                _timers[i].BypassThisTurn = true; // don't trigger timer during the turn it was first enabled
                return;
            }
        }

        LogASLError("No such timer '" + name + "'", LogType.WarningError);
    }

    private SetResult SetUnknownVariableType(string variableData, Context ctx)
    {
        var scp = Strings.InStr(variableData, ";");
        if (scp == 0)
        {
            return SetResult.Error;
        }

        var name = Strings.Trim(Strings.Left(variableData, scp - 1));
        if ((Strings.InStr(name, "[") != 0) & (Strings.InStr(name, "]") != 0))
        {
            var pos = Strings.InStr(name, "[");
            name = Strings.Left(name, pos - 1);
        }

        var contents = Strings.Trim(Strings.Mid(variableData, scp + 1));

        for (int i = 1, loopTo = _numberStringVariables; i <= loopTo; i++)
        {
            if ((Strings.LCase(_stringVariable[i].VariableName) ?? "") == (Strings.LCase(name) ?? ""))
            {
                ExecSetString(variableData, ctx);
                return SetResult.Found;
            }
        }

        for (int i = 1, loopTo1 = _numberNumericVariables; i <= loopTo1; i++)
        {
            if ((Strings.LCase(_numericVariable[i].VariableName) ?? "") == (Strings.LCase(name) ?? ""))
            {
                ExecSetVar(variableData, ctx);
                return SetResult.Found;
            }
        }

        for (int i = 1, loopTo2 = _numCollectables; i <= loopTo2; i++)
        {
            if ((Strings.LCase(_collectables[i].Name) ?? "") == (Strings.LCase(name) ?? ""))
            {
                ExecuteSetCollectable(variableData, ctx);
                return SetResult.Found;
            }
        }

        return SetResult.Unfound;
    }

    private string SetUpChoiceForm(string blockName, Context ctx)
    {
        // Returns script to execute from choice block
        var block = DefineBlockParam("selection", blockName);
        var prompt = FindStatement(block, "info");

        var menuOptions = new Dictionary<string, string>();
        var menuScript = new Dictionary<string, string>();

        for (int i = block.StartLine + 1, loopTo = block.EndLine - 1; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[i], "choice "))
            {
                menuOptions.Add(i.ToString(), GetParameter(_lines[i], ctx));
                menuScript.Add(i.ToString(),
                    Strings.Trim(Strings.Right(_lines[i], Strings.Len(_lines[i]) - Strings.InStr(_lines[i], ">"))));
            }
        }

        Print("- |i" + prompt + "|xi", ctx);

        var mnu = new MenuData(prompt, menuOptions, false);
        var choice = ShowMenu(mnu);

        Print("- " + menuOptions[choice] + "|n", ctx);
        return menuScript[choice];
    }

    private void SetUpDefaultFonts()
    {
        // Sets up default fonts
        var gameblock = GetDefineBlock("game");

        _defaultFontName = "Arial";
        _defaultFontSize = 9d;

        for (int i = gameblock.StartLine + 1, loopTo = gameblock.EndLine - 1; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[i], "default fontname "))
            {
                var name = GetParameter(_lines[i], _nullContext);
                if (!string.IsNullOrEmpty(name))
                {
                    _defaultFontName = name;
                }
            }
            else if (BeginsWith(_lines[i], "default fontsize "))
            {
                var size = Conversions.ToInteger(GetParameter(_lines[i], _nullContext));
                if (size != 0)
                {
                    _defaultFontSize = size;
                }
            }
        }
    }

    private void SetUpDisplayVariables()
    {
        for (int i = GetDefineBlock("game").StartLine + 1, loopTo = GetDefineBlock("game").EndLine - 1;
             i <= loopTo;
             i++)
        {
            if (BeginsWith(_lines[i], "define variable "))
            {
                var variable = new VariableType();
                variable.VariableContents = new string[1];

                variable.VariableName = GetParameter(_lines[i], _nullContext);
                variable.DisplayString = "";
                variable.NoZeroDisplay = false;
                variable.OnChangeScript = "";
                variable.VariableContents[0] = "";
                variable.VariableUBound = 0;

                var type = "numeric";

                do
                {
                    i = i + 1;

                    if (BeginsWith(_lines[i], "type "))
                    {
                        type = GetEverythingAfter(_lines[i], "type ");
                        if ((type != "string") & (type != "numeric"))
                        {
                            LogASLError(
                                "Unrecognised variable type in variable '" + variable.VariableName + "' - type '" +
                                type + "'", LogType.WarningError);
                            break;
                        }
                    }
                    else if (BeginsWith(_lines[i], "onchange "))
                    {
                        variable.OnChangeScript = GetEverythingAfter(_lines[i], "onchange ");
                    }
                    else if (BeginsWith(_lines[i], "display "))
                    {
                        var displayString = GetEverythingAfter(_lines[i], "display ");
                        if (BeginsWith(displayString, "nozero "))
                        {
                            variable.NoZeroDisplay = true;
                        }

                        variable.DisplayString = GetParameter(_lines[i], _nullContext, false);
                    }
                    else if (BeginsWith(_lines[i], "value "))
                    {
                        variable.VariableContents[0] = GetParameter(_lines[i], _nullContext);
                    }
                } while (Strings.Trim(_lines[i]) != "end define");

                if (type == "string")
                {
                    // Create string variable
                    _numberStringVariables = _numberStringVariables + 1;
                    var id = _numberStringVariables;
                    Array.Resize(ref _stringVariable, id + 1);
                    _stringVariable[id] = variable;
                    _numDisplayStrings = _numDisplayStrings + 1;
                }
                else if (type == "numeric")
                {
                    if (string.IsNullOrEmpty(variable.VariableContents[0]))
                    {
                        variable.VariableContents[0] = 0.ToString();
                    }

                    _numberNumericVariables = _numberNumericVariables + 1;
                    var iNumNumber = _numberNumericVariables;
                    Array.Resize(ref _numericVariable, iNumNumber + 1);
                    _numericVariable[iNumNumber] = variable;
                    _numDisplayNumerics = _numDisplayNumerics + 1;
                }
            }
        }
    }

    private void SetUpGameObject()
    {
        _numberObjs = 1;
        _objs = new ObjectType[2];
        _objs[1] = new ObjectType();
        var o = _objs[1];
        o.ObjectName = "game";
        o.ObjectAlias = "";
        o.Visible = false;
        o.Exists = true;

        var nestBlock = 0;
        for (int i = GetDefineBlock("game").StartLine + 1, loopTo = GetDefineBlock("game").EndLine - 1;
             i <= loopTo;
             i++)
        {
            if (nestBlock == 0)
            {
                if (BeginsWith(_lines[i], "define "))
                {
                    nestBlock = nestBlock + 1;
                }
                else if (BeginsWith(_lines[i], "properties "))
                {
                    AddToObjectProperties(GetParameter(_lines[i], _nullContext), _numberObjs, _nullContext);
                }
                else if (BeginsWith(_lines[i], "type "))
                {
                    o.NumberTypesIncluded = o.NumberTypesIncluded + 1;
                    Array.Resize(ref o.TypesIncluded, o.NumberTypesIncluded + 1);
                    o.TypesIncluded[o.NumberTypesIncluded] = GetParameter(_lines[i], _nullContext);

                    var propertyData = GetPropertiesInType(GetParameter(_lines[i], _nullContext));
                    AddToObjectProperties(propertyData.Properties, _numberObjs, _nullContext);
                    for (int k = 1, loopTo1 = propertyData.NumberActions; k <= loopTo1; k++)
                    {
                        AddObjectAction(_numberObjs, propertyData.Actions[k].ActionName,
                            propertyData.Actions[k].Script);
                    }
                }
                else if (BeginsWith(_lines[i], "action "))
                {
                    AddToObjectActions(GetEverythingAfter(_lines[i], "action "), _numberObjs, _nullContext);
                }
            }
            else if (Strings.Trim(_lines[i]) == "end define")
            {
                nestBlock = nestBlock - 1;
            }
        }
    }

    private void SetUpMenus()
    {
        var exists = false;
        var menuTitle = "";
        var menuOptions = new Dictionary<string, string>();

        for (int i = 1, loopTo = _numberSections; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[_defineBlocks[i].StartLine], "define menu "))
            {
                if (exists)
                {
                    LogASLError(
                        "Can't load menu '" + GetParameter(_lines[_defineBlocks[i].StartLine], _nullContext) +
                        "' - only one menu can be added.", LogType.WarningError);
                }
                else
                {
                    menuTitle = GetParameter(_lines[_defineBlocks[i].StartLine], _nullContext);

                    for (int j = _defineBlocks[i].StartLine + 1, loopTo1 = _defineBlocks[i].EndLine - 1;
                         j <= loopTo1;
                         j++)
                    {
                        if (!string.IsNullOrEmpty(Strings.Trim(_lines[j])))
                        {
                            var scp = Strings.InStr(_lines[j], ":");
                            if ((scp == 0) & (_lines[j] != "-"))
                            {
                                LogASLError("No menu command specified in menu '" + menuTitle + "', item '" + _lines[j],
                                    LogType.WarningError);
                            }
                            else if (_lines[j] == "-")
                            {
                                menuOptions.Add("k" + j, "-");
                            }
                            else
                            {
                                menuOptions.Add(Strings.Trim(Strings.Mid(_lines[j], scp + 1)),
                                    Strings.Trim(Strings.Left(_lines[j], scp - 1)));
                            }
                        }
                    }

                    if (menuOptions.Count > 0)
                    {
                        exists = true;
                    }
                }
            }
        }

        if (exists)
        {
            var windowMenu = new MenuData(menuTitle, menuOptions, false);
            _player.SetWindowMenu(windowMenu);
        }
    }

    private void SetUpOptions()
    {
        string opt;

        for (int i = GetDefineBlock("options").StartLine + 1, loopTo = GetDefineBlock("options").EndLine - 1;
             i <= loopTo;
             i++)
        {
            if (BeginsWith(_lines[i], "panes "))
            {
                opt = Strings.LCase(Strings.Trim(GetEverythingAfter(_lines[i], "panes ")));
                _player.SetPanesVisible(opt);
            }
            else if (BeginsWith(_lines[i], "abbreviations "))
            {
                opt = Strings.LCase(Strings.Trim(GetEverythingAfter(_lines[i], "abbreviations ")));
                if (opt == "off")
                {
                    _useAbbreviations = false;
                }
                else
                {
                    _useAbbreviations = true;
                }
            }
        }
    }

    private void SetUpRoomData()
    {
        var defaultProperties = new PropertiesActions();

        // see if define type <defaultroom> exists:
        var defaultExists = false;
        for (int i = 1, loopTo = _numberSections; i <= loopTo; i++)
        {
            if (Strings.Trim(_lines[_defineBlocks[i].StartLine]) == "define type <defaultroom>")
            {
                defaultExists = true;
                defaultProperties = GetPropertiesInType("defaultroom");
                break;
            }
        }

        for (int i = 1, loopTo1 = _numberSections; i <= loopTo1; i++)
        {
            if (BeginsWith(_lines[_defineBlocks[i].StartLine], "define room "))
            {
                _numberRooms = _numberRooms + 1;
                Array.Resize(ref _rooms, _numberRooms + 1);
                _rooms[_numberRooms] = new RoomType();

                _numberObjs = _numberObjs + 1;
                Array.Resize(ref _objs, _numberObjs + 1);
                _objs[_numberObjs] = new ObjectType();

                var r = _rooms[_numberRooms];

                r.RoomName = GetParameter(_lines[_defineBlocks[i].StartLine], _nullContext);
                _objs[_numberObjs].ObjectName = r.RoomName;
                _objs[_numberObjs].IsRoom = true;
                _objs[_numberObjs].CorresRoom = r.RoomName;
                _objs[_numberObjs].CorresRoomId = _numberRooms;

                r.ObjId = _numberObjs;

                if (ASLVersion >= 410)
                {
                    r.Exits = new RoomExits(this);
                    r.Exits.SetObjId(r.ObjId);
                }

                if (defaultExists)
                {
                    AddToObjectProperties(defaultProperties.Properties, _numberObjs, _nullContext);
                    for (int k = 1, loopTo2 = defaultProperties.NumberActions; k <= loopTo2; k++)
                    {
                        AddObjectAction(_numberObjs, defaultProperties.Actions[k].ActionName,
                            defaultProperties.Actions[k].Script);
                    }
                }

                for (int j = _defineBlocks[i].StartLine + 1, loopTo3 = _defineBlocks[i].EndLine - 1; j <= loopTo3; j++)
                {
                    if (BeginsWith(_lines[j], "define "))
                    {
                        // skip nested blocks
                        var nestedBlock = 1;
                        do
                        {
                            j = j + 1;
                            if (BeginsWith(_lines[j], "define "))
                            {
                                nestedBlock = nestedBlock + 1;
                            }
                            else if (Strings.Trim(_lines[j]) == "end define")
                            {
                                nestedBlock = nestedBlock - 1;
                            }
                        } while (nestedBlock != 0);
                    }

                    if ((ASLVersion >= 280) & BeginsWith(_lines[j], "alias "))
                    {
                        r.RoomAlias = GetParameter(_lines[j], _nullContext);
                        _objs[_numberObjs].ObjectAlias = r.RoomAlias;
                        if (ASLVersion >= 350)
                        {
                            AddToObjectProperties("alias=" + r.RoomAlias, _numberObjs, _nullContext);
                        }
                    }
                    else if ((ASLVersion >= 280) & BeginsWith(_lines[j], "description "))
                    {
                        r.Description = GetTextOrScript(GetEverythingAfter(_lines[j], "description "));
                        if (ASLVersion >= 350)
                        {
                            if (r.Description.Type == TextActionType.Script)
                            {
                                AddObjectAction(_numberObjs, "description", r.Description.Data);
                            }
                            else
                            {
                                AddToObjectProperties("description=" + r.Description.Data, _numberObjs, _nullContext);
                            }
                        }
                    }
                    else if (BeginsWith(_lines[j], "out "))
                    {
                        r.Out.Text = GetParameter(_lines[j], _nullContext);
                        r.Out.Script = Strings.Trim(Strings.Mid(_lines[j], Strings.InStr(_lines[j], ">") + 1));
                        if (ASLVersion >= 350)
                        {
                            if (!string.IsNullOrEmpty(r.Out.Script))
                            {
                                AddObjectAction(_numberObjs, "out", r.Out.Script);
                            }

                            AddToObjectProperties("out=" + r.Out.Text, _numberObjs, _nullContext);
                        }
                    }
                    else if (BeginsWith(_lines[j], "east "))
                    {
                        r.East = GetTextOrScript(GetEverythingAfter(_lines[j], "east "));
                        if (ASLVersion >= 350)
                        {
                            if (r.East.Type == TextActionType.Script)
                            {
                                AddObjectAction(_numberObjs, "east", r.East.Data);
                            }
                            else
                            {
                                AddToObjectProperties("east=" + r.East.Data, _numberObjs, _nullContext);
                            }
                        }
                    }
                    else if (BeginsWith(_lines[j], "west "))
                    {
                        r.West = GetTextOrScript(GetEverythingAfter(_lines[j], "west "));
                        if (ASLVersion >= 350)
                        {
                            if (r.West.Type == TextActionType.Script)
                            {
                                AddObjectAction(_numberObjs, "west", r.West.Data);
                            }
                            else
                            {
                                AddToObjectProperties("west=" + r.West.Data, _numberObjs, _nullContext);
                            }
                        }
                    }
                    else if (BeginsWith(_lines[j], "north "))
                    {
                        r.North = GetTextOrScript(GetEverythingAfter(_lines[j], "north "));
                        if (ASLVersion >= 350)
                        {
                            if (r.North.Type == TextActionType.Script)
                            {
                                AddObjectAction(_numberObjs, "north", r.North.Data);
                            }
                            else
                            {
                                AddToObjectProperties("north=" + r.North.Data, _numberObjs, _nullContext);
                            }
                        }
                    }
                    else if (BeginsWith(_lines[j], "south "))
                    {
                        r.South = GetTextOrScript(GetEverythingAfter(_lines[j], "south "));
                        if (ASLVersion >= 350)
                        {
                            if (r.South.Type == TextActionType.Script)
                            {
                                AddObjectAction(_numberObjs, "south", r.South.Data);
                            }
                            else
                            {
                                AddToObjectProperties("south=" + r.South.Data, _numberObjs, _nullContext);
                            }
                        }
                    }
                    else if (BeginsWith(_lines[j], "northeast "))
                    {
                        r.NorthEast = GetTextOrScript(GetEverythingAfter(_lines[j], "northeast "));
                        if (ASLVersion >= 350)
                        {
                            if (r.NorthEast.Type == TextActionType.Script)
                            {
                                AddObjectAction(_numberObjs, "northeast", r.NorthEast.Data);
                            }
                            else
                            {
                                AddToObjectProperties("northeast=" + r.NorthEast.Data, _numberObjs, _nullContext);
                            }
                        }
                    }
                    else if (BeginsWith(_lines[j], "northwest "))
                    {
                        r.NorthWest = GetTextOrScript(GetEverythingAfter(_lines[j], "northwest "));
                        if (ASLVersion >= 350)
                        {
                            if (r.NorthWest.Type == TextActionType.Script)
                            {
                                AddObjectAction(_numberObjs, "northwest", r.NorthWest.Data);
                            }
                            else
                            {
                                AddToObjectProperties("northwest=" + r.NorthWest.Data, _numberObjs, _nullContext);
                            }
                        }
                    }
                    else if (BeginsWith(_lines[j], "southeast "))
                    {
                        r.SouthEast = GetTextOrScript(GetEverythingAfter(_lines[j], "southeast "));
                        if (ASLVersion >= 350)
                        {
                            if (r.SouthEast.Type == TextActionType.Script)
                            {
                                AddObjectAction(_numberObjs, "southeast", r.SouthEast.Data);
                            }
                            else
                            {
                                AddToObjectProperties("southeast=" + r.SouthEast.Data, _numberObjs, _nullContext);
                            }
                        }
                    }
                    else if (BeginsWith(_lines[j], "southwest "))
                    {
                        r.SouthWest = GetTextOrScript(GetEverythingAfter(_lines[j], "southwest "));
                        if (ASLVersion >= 350)
                        {
                            if (r.SouthWest.Type == TextActionType.Script)
                            {
                                AddObjectAction(_numberObjs, "southwest", r.SouthWest.Data);
                            }
                            else
                            {
                                AddToObjectProperties("southwest=" + r.SouthWest.Data, _numberObjs, _nullContext);
                            }
                        }
                    }
                    else if (BeginsWith(_lines[j], "up "))
                    {
                        r.Up = GetTextOrScript(GetEverythingAfter(_lines[j], "up "));
                        if (ASLVersion >= 350)
                        {
                            if (r.Up.Type == TextActionType.Script)
                            {
                                AddObjectAction(_numberObjs, "up", r.Up.Data);
                            }
                            else
                            {
                                AddToObjectProperties("up=" + r.Up.Data, _numberObjs, _nullContext);
                            }
                        }
                    }
                    else if (BeginsWith(_lines[j], "down "))
                    {
                        r.Down = GetTextOrScript(GetEverythingAfter(_lines[j], "down "));
                        if (ASLVersion >= 350)
                        {
                            if (r.Down.Type == TextActionType.Script)
                            {
                                AddObjectAction(_numberObjs, "down", r.Down.Data);
                            }
                            else
                            {
                                AddToObjectProperties("down=" + r.Down.Data, _numberObjs, _nullContext);
                            }
                        }
                    }
                    else if ((ASLVersion >= 280) & BeginsWith(_lines[j], "indescription "))
                    {
                        r.InDescription = GetParameter(_lines[j], _nullContext);
                        if (ASLVersion >= 350)
                        {
                            AddToObjectProperties("indescription=" + r.InDescription, _numberObjs, _nullContext);
                        }
                    }
                    else if ((ASLVersion >= 280) & BeginsWith(_lines[j], "look "))
                    {
                        r.Look = GetParameter(_lines[j], _nullContext);
                        if (ASLVersion >= 350)
                        {
                            AddToObjectProperties("look=" + r.Look, _numberObjs, _nullContext);
                        }
                    }
                    else if (BeginsWith(_lines[j], "prefix "))
                    {
                        r.Prefix = GetParameter(_lines[j], _nullContext);
                        if (ASLVersion >= 350)
                        {
                            AddToObjectProperties("prefix=" + r.Prefix, _numberObjs, _nullContext);
                        }
                    }
                    else if (BeginsWith(_lines[j], "script "))
                    {
                        r.Script = GetEverythingAfter(_lines[j], "script ");
                        AddObjectAction(_numberObjs, "script", r.Script);
                    }
                    else if (BeginsWith(_lines[j], "command "))
                    {
                        r.NumberCommands = r.NumberCommands + 1;
                        Array.Resize(ref r.Commands, r.NumberCommands + 1);
                        r.Commands[r.NumberCommands] = new UserDefinedCommandType();
                        r.Commands[r.NumberCommands].CommandText = GetParameter(_lines[j], _nullContext, false);
                        r.Commands[r.NumberCommands].CommandScript =
                            Strings.Trim(Strings.Mid(_lines[j], Strings.InStr(_lines[j], ">") + 1));
                    }
                    else if (BeginsWith(_lines[j], "place "))
                    {
                        r.NumberPlaces = r.NumberPlaces + 1;
                        Array.Resize(ref r.Places, r.NumberPlaces + 1);
                        r.Places[r.NumberPlaces] = new PlaceType();
                        var placeData = GetParameter(_lines[j], _nullContext);
                        var scp = Strings.InStr(placeData, ";");
                        if (scp == 0)
                        {
                            r.Places[r.NumberPlaces].PlaceName = placeData;
                        }
                        else
                        {
                            r.Places[r.NumberPlaces].PlaceName = Strings.Trim(Strings.Mid(placeData, scp + 1));
                            r.Places[r.NumberPlaces].Prefix = Strings.Trim(Strings.Left(placeData, scp - 1));
                        }

                        r.Places[r.NumberPlaces].Script =
                            Strings.Trim(Strings.Mid(_lines[j], Strings.InStr(_lines[j], ">") + 1));
                    }
                    else if (BeginsWith(_lines[j], "use "))
                    {
                        r.NumberUse = r.NumberUse + 1;
                        Array.Resize(ref r.Use, r.NumberUse + 1);
                        r.Use[r.NumberUse] = new ScriptText();
                        r.Use[r.NumberUse].Text = GetParameter(_lines[j], _nullContext);
                        r.Use[r.NumberUse].Script =
                            Strings.Trim(Strings.Mid(_lines[j], Strings.InStr(_lines[j], ">") + 1));
                    }
                    else if (BeginsWith(_lines[j], "properties "))
                    {
                        AddToObjectProperties(GetParameter(_lines[j], _nullContext), _numberObjs, _nullContext);
                    }
                    else if (BeginsWith(_lines[j], "type "))
                    {
                        _objs[_numberObjs].NumberTypesIncluded = _objs[_numberObjs].NumberTypesIncluded + 1;
                        Array.Resize(ref _objs[_numberObjs].TypesIncluded, _objs[_numberObjs].NumberTypesIncluded + 1);
                        _objs[_numberObjs].TypesIncluded[_objs[_numberObjs].NumberTypesIncluded] =
                            GetParameter(_lines[j], _nullContext);

                        var propertyData = GetPropertiesInType(GetParameter(_lines[j], _nullContext));
                        AddToObjectProperties(propertyData.Properties, _numberObjs, _nullContext);
                        for (int k = 1, loopTo4 = propertyData.NumberActions; k <= loopTo4; k++)
                        {
                            AddObjectAction(_numberObjs, propertyData.Actions[k].ActionName,
                                propertyData.Actions[k].Script);
                        }
                    }
                    else if (BeginsWith(_lines[j], "action "))
                    {
                        AddToObjectActions(GetEverythingAfter(_lines[j], "action "), _numberObjs, _nullContext);
                    }
                    else if (BeginsWith(_lines[j], "beforeturn "))
                    {
                        r.BeforeTurnScript = r.BeforeTurnScript + GetEverythingAfter(_lines[j], "beforeturn ") +
                                             Constants.vbCrLf;
                    }
                    else if (BeginsWith(_lines[j], "afterturn "))
                    {
                        r.AfterTurnScript = r.AfterTurnScript + GetEverythingAfter(_lines[j], "afterturn ") +
                                            Constants.vbCrLf;
                    }
                }
            }
        }
    }

    private void SetUpSynonyms()
    {
        var block = GetDefineBlock("synonyms");
        _numberSynonyms = 0;

        if ((block.StartLine == 0) & (block.EndLine == 0))
        {
            return;
        }

        for (int i = block.StartLine + 1, loopTo = block.EndLine - 1; i <= loopTo; i++)
        {
            var eqp = Strings.InStr(_lines[i], "=");
            if (eqp != 0)
            {
                var originalWordsList = Strings.Trim(Strings.Left(_lines[i], eqp - 1));
                var convertWord = Strings.Trim(Strings.Mid(_lines[i], eqp + 1));

                // Go through each word in OriginalWordsList (sep.
                // by ";"):

                originalWordsList = originalWordsList + ";";
                var pos = 1;

                do
                {
                    var endOfWord = Strings.InStr(pos, originalWordsList, ";");
                    var thisWord = Strings.Trim(Strings.Mid(originalWordsList, pos, endOfWord - pos));

                    if (Strings.InStr(" " + convertWord + " ", " " + thisWord + " ") > 0)
                    {
                        // Recursive synonym
                        LogASLError(
                            "Recursive synonym detected: '" + thisWord + "' converting to '" + convertWord + "'",
                            LogType.WarningError);
                    }
                    else
                    {
                        _numberSynonyms = _numberSynonyms + 1;
                        Array.Resize(ref _synonyms, _numberSynonyms + 1);
                        _synonyms[_numberSynonyms] = new SynonymType();
                        _synonyms[_numberSynonyms].OriginalWord = thisWord;
                        _synonyms[_numberSynonyms].ConvertTo = convertWord;
                    }

                    pos = endOfWord + 1;
                } while (pos < Strings.Len(originalWordsList));
            }
        }
    }

    private void SetUpTimers()
    {
        for (int i = 1, loopTo = _numberSections; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[_defineBlocks[i].StartLine], "define timer "))
            {
                _numberTimers = _numberTimers + 1;
                Array.Resize(ref _timers, _numberTimers + 1);
                _timers[_numberTimers] = new TimerType();
                _timers[_numberTimers].TimerName = GetParameter(_lines[_defineBlocks[i].StartLine], _nullContext);
                _timers[_numberTimers].TimerActive = false;

                for (int j = _defineBlocks[i].StartLine + 1, loopTo1 = _defineBlocks[i].EndLine - 1; j <= loopTo1; j++)
                {
                    if (BeginsWith(_lines[j], "interval "))
                    {
                        _timers[_numberTimers].TimerInterval =
                            Conversions.ToInteger(GetParameter(_lines[j], _nullContext));
                    }
                    else if (BeginsWith(_lines[j], "action "))
                    {
                        _timers[_numberTimers].TimerAction = GetEverythingAfter(_lines[j], "action ");
                    }
                    else if (Strings.Trim(Strings.LCase(_lines[j])) == "enabled")
                    {
                        _timers[_numberTimers].TimerActive = true;
                    }
                    else if (Strings.Trim(Strings.LCase(_lines[j])) == "disabled")
                    {
                        _timers[_numberTimers].TimerActive = false;
                    }
                }
            }
        }
    }

    private void SetUpTurnScript()
    {
        var block = GetDefineBlock("game");

        _beforeTurnScript = "";
        _afterTurnScript = "";

        for (int i = block.StartLine + 1, loopTo = block.EndLine - 1; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[i], "beforeturn "))
            {
                _beforeTurnScript = _beforeTurnScript + GetEverythingAfter(Strings.Trim(_lines[i]), "beforeturn ") +
                                    Constants.vbCrLf;
            }
            else if (BeginsWith(_lines[i], "afterturn "))
            {
                _afterTurnScript = _afterTurnScript + GetEverythingAfter(Strings.Trim(_lines[i]), "afterturn ") +
                                   Constants.vbCrLf;
            }
        }
    }

    private void SetUpUserDefinedPlayerErrors()
    {
        // goes through "define game" block and sets stored error
        // messages accordingly

        var block = GetDefineBlock("game");
        var examineIsCustomised = false;

        for (int i = block.StartLine + 1, loopTo = block.EndLine - 1; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[i], "error "))
            {
                var errorInfo = GetParameter(_lines[i], _nullContext, false);
                var scp = Strings.InStr(errorInfo, ";");
                var errorName = Strings.Left(errorInfo, scp - 1);
                var errorMsg = Strings.Trim(Strings.Mid(errorInfo, scp + 1));
                var currentError = 0;

                switch (errorName ?? "")
                {
                    case "badcommand":
                    {
                        currentError = (int) PlayerError.BadCommand;
                        break;
                    }
                    case "badgo":
                    {
                        currentError = (int) PlayerError.BadGo;
                        break;
                    }
                    case "badgive":
                    {
                        currentError = (int) PlayerError.BadGive;
                        break;
                    }
                    case "badcharacter":
                    {
                        currentError = (int) PlayerError.BadCharacter;
                        break;
                    }
                    case "noitem":
                    {
                        currentError = (int) PlayerError.NoItem;
                        break;
                    }
                    case "itemunwanted":
                    {
                        currentError = (int) PlayerError.ItemUnwanted;
                        break;
                    }
                    case "badlook":
                    {
                        currentError = (int) PlayerError.BadLook;
                        break;
                    }
                    case "badthing":
                    {
                        currentError = (int) PlayerError.BadThing;
                        break;
                    }
                    case "defaultlook":
                    {
                        currentError = (int) PlayerError.DefaultLook;
                        break;
                    }
                    case "defaultspeak":
                    {
                        currentError = (int) PlayerError.DefaultSpeak;
                        break;
                    }
                    case "baditem":
                    {
                        currentError = (int) PlayerError.BadItem;
                        break;
                    }
                    case "baddrop":
                    {
                        currentError = (int) PlayerError.BadDrop;
                        break;
                    }
                    case "defaultake":
                    {
                        if (ASLVersion <= 280)
                        {
                            currentError = (int) PlayerError.BadTake;
                        }
                        else
                        {
                            currentError = (int) PlayerError.DefaultTake;
                        }

                        break;
                    }
                    case "baduse":
                    {
                        currentError = (int) PlayerError.BadUse;
                        break;
                    }
                    case "defaultuse":
                    {
                        currentError = (int) PlayerError.DefaultUse;
                        break;
                    }
                    case "defaultout":
                    {
                        currentError = (int) PlayerError.DefaultOut;
                        break;
                    }
                    case "badplace":
                    {
                        currentError = (int) PlayerError.BadPlace;
                        break;
                    }
                    case "badexamine":
                    {
                        if (ASLVersion >= 310)
                        {
                            currentError = (int) PlayerError.BadExamine;
                        }

                        break;
                    }
                    case "defaultexamine":
                    {
                        currentError = (int) PlayerError.DefaultExamine;
                        examineIsCustomised = true;
                        break;
                    }
                    case "badtake":
                    {
                        currentError = (int) PlayerError.BadTake;
                        break;
                    }
                    case "cantdrop":
                    {
                        currentError = (int) PlayerError.CantDrop;
                        break;
                    }
                    case "defaultdrop":
                    {
                        currentError = (int) PlayerError.DefaultDrop;
                        break;
                    }
                    case "badpronoun":
                    {
                        currentError = (int) PlayerError.BadPronoun;
                        break;
                    }
                    case "alreadyopen":
                    {
                        currentError = (int) PlayerError.AlreadyOpen;
                        break;
                    }
                    case "alreadyclosed":
                    {
                        currentError = (int) PlayerError.AlreadyClosed;
                        break;
                    }
                    case "cantopen":
                    {
                        currentError = (int) PlayerError.CantOpen;
                        break;
                    }
                    case "cantclose":
                    {
                        currentError = (int) PlayerError.CantClose;
                        break;
                    }
                    case "defaultopen":
                    {
                        currentError = (int) PlayerError.DefaultOpen;
                        break;
                    }
                    case "defaultclose":
                    {
                        currentError = (int) PlayerError.DefaultClose;
                        break;
                    }
                    case "badput":
                    {
                        currentError = (int) PlayerError.BadPut;
                        break;
                    }
                    case "cantput":
                    {
                        currentError = (int) PlayerError.CantPut;
                        break;
                    }
                    case "defaultput":
                    {
                        currentError = (int) PlayerError.DefaultPut;
                        break;
                    }
                    case "cantremove":
                    {
                        currentError = (int) PlayerError.CantRemove;
                        break;
                    }
                    case "alreadyput":
                    {
                        currentError = (int) PlayerError.AlreadyPut;
                        break;
                    }
                    case "defaultremove":
                    {
                        currentError = (int) PlayerError.DefaultRemove;
                        break;
                    }
                    case "locked":
                    {
                        currentError = (int) PlayerError.Locked;
                        break;
                    }
                    case "defaultwait":
                    {
                        currentError = (int) PlayerError.DefaultWait;
                        break;
                    }
                    case "alreadytaken":
                    {
                        currentError = (int) PlayerError.AlreadyTaken;
                        break;
                    }
                }

                _playerErrorMessageString[currentError] = errorMsg;
                if ((currentError == (int) PlayerError.DefaultLook) & !examineIsCustomised)
                {
                    // If we're setting the default look message, and we've not already customised the
                    // default examine message, then set the default examine message to the same thing.
                    _playerErrorMessageString[(int) PlayerError.DefaultExamine] = errorMsg;
                }
            }
        }
    }

    private void SetVisibility(string thing, Thing type, bool visible, Context ctx)
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
                        AddToObjectProperties("not invisible", i, ctx);
                    }
                    else
                    {
                        AddToObjectProperties("invisible", i, ctx);
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

        UpdateObjectList(ctx);
    }

    private void ShowPictureInText(string filename)
    {
        if (!_useStaticFrameForPictures)
        {
            _player.ShowPicture(filename);
        }
        else
        {
            // Workaround for a particular game which expects pictures to be in a popup window -
            // use the static picture frame feature so that image is not cleared
            _player.SetPanelContents("<img src=\"" + _player.GetURL(filename) + "\" onload=\"setPanelHeight()\"/>");
        }
    }

    private void ShowRoomInfoV2(string room)
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
        roomBlock = DefineBlockParam("room", room);
        bool finishedFindingCommas;

        charsViewable = "";
        charsFound = 0;

        // see if room has an alias
        for (int i = roomBlock.StartLine + 1, loopTo = roomBlock.EndLine - 1; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[i], "alias"))
            {
                aliasName = GetParameter(_lines[i], _nullContext);
                i = roomBlock.EndLine;
            }
        }

        if (string.IsNullOrEmpty(aliasName))
        {
            aliasName = room;
        }

        // see if room has a prefix
        prefix = FindStatement(roomBlock, "prefix");
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
                inDesc = Strings.Trim(GetParameter(_lines[i], _nullContext));
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

        SetStringContents("quest.formatroom", prefixAliasNoFormat, _nullContext);

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
            SetStringContents("quest.characters", "", _nullContext);
        }
        else
        {
            // chop off final comma and add full stop (.)
            charList = Strings.Left(charsViewable, Strings.Len(charsViewable) - 2);
            SetStringContents("quest.characters", charList, _nullContext);

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
            SetStringContents("quest.objects",
                Strings.Left(noFormatObjsViewable, Strings.Len(noFormatObjsViewable) - 2), _nullContext);
            SetStringContents("quest.formatobjects", objListString, _nullContext);
            roomDisplayText = roomDisplayText + objsViewable + Constants.vbCrLf;
        }
        else
        {
            SetStringContents("quest.objects", "", _nullContext);
            SetStringContents("quest.formatobjects", "", _nullContext);
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
                doorways = GetParameter(_lines[i], _nullContext);
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
                place = GetParameter(_lines[i], _nullContext);
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
            outside = DefineBlockParam("room", doorways);
            for (int i = outside.StartLine + 1, loopTo5 = outside.EndLine - 1; i <= loopTo5; i++)
            {
                if (BeginsWith(_lines[i], "alias"))
                {
                    aliasOut = GetParameter(_lines[i], _nullContext);
                    i = outside.EndLine;
                }
            }

            if (string.IsNullOrEmpty(aliasOut))
            {
                aliasOut = doorways;
            }

            roomDisplayText = roomDisplayText + "You can go out to " + aliasOut + "." + Constants.vbCrLf;
            possDir = possDir + "o";
            SetStringContents("quest.doorways.out", aliasOut, _nullContext);
        }
        else
        {
            SetStringContents("quest.doorways.out", "", _nullContext);
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
            SetStringContents("quest.doorways.dirs", nsew, _nullContext);
        }
        else
        {
            SetStringContents("quest.doorways.dirs", "", _nullContext);
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
            SetStringContents("quest.doorways.places", places, _nullContext);
        }
        else
        {
            SetStringContents("quest.doorways.places", "", _nullContext);
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

        if (descTagExist == false)
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

        if (descTagExist == false)
        {
            // Remove final newline:
            roomDisplayText = Strings.Left(roomDisplayText, Strings.Len(roomDisplayText) - 2);
            Print(roomDisplayText, _nullContext);
        }
        else
        {
            // execute description tag:
            // If no script, just print the tag's parameter.
            // Otherwise, execute it as ASL script:

            descLine = GetEverythingAfter(Strings.Trim(descLine), "description ");
            if (Strings.Left(descLine, 1) == "<")
            {
                Print(GetParameter(descLine, _nullContext), _nullContext);
            }
            else
            {
                ExecuteScript(descLine, _nullContext);
            }
        }

        UpdateObjectList(_nullContext);

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
                lookString = GetParameter(_lines[i], _nullContext);
                i = roomBlock.EndLine;
            }
        }

        if (!string.IsNullOrEmpty(lookString))
        {
            Print(lookString, _nullContext);
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

    private void ExecExec(string scriptLine, Context ctx)
    {
        if (ctx.CancelExec)
        {
            return;
        }

        var execLine = GetParameter(scriptLine, ctx);
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
                ExecCommand(execLine, newCtx, false);
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
                ExecCommand(ex, newCtx, false, false);
            }
            else
            {
                LogASLError("Unrecognised post-command parameter in " + Strings.Trim(scriptLine), LogType.WarningError);
            }
        }
    }

    private void ExecSetString(string info, Context ctx)
    {
        // Sets string contents from a script parameter.
        // Eg <string1;contents> sets string variable string1
        // to "contents"

        var scp = Strings.InStr(info, ";");
        var name = Strings.Trim(Strings.Left(info, scp - 1));
        var value = Strings.Mid(info, scp + 1);

        if (Information.IsNumeric(name))
        {
            LogASLError("Invalid string name '" + name + "' - string names cannot be numeric", LogType.WarningError);
            return;
        }

        if (ASLVersion >= 281)
        {
            value = Strings.Trim(value);
            if ((Strings.Left(value, 1) == "[") & (Strings.Right(value, 1) == "]"))
            {
                value = Strings.Mid(value, 2, Strings.Len(value) - 2);
            }
        }

        var idx = GetArrayIndex(name, ctx);
        SetStringContents(idx.Name, value, ctx, idx.Index);
    }

    private bool ExecUserCommand(string cmd, Context ctx, bool libCommands = false)
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
                    commandList = GetParameter(_lines[i], ctx, false);
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
            if (GetCommandParameters(cmd, commandLine, ctx))
            {
                ExecuteScript(script, ctx);
            }
        }

        return foundCommand;
    }

    private void ExecuteChoose(string section, Context ctx)
    {
        ExecuteScript(SetUpChoiceForm(section, ctx), ctx);
    }

    private bool GetCommandParameters(string test, string required, Context ctx)
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
                var id = Disambiguate(curChunk, _currentRoom + ";" + "inventory", ctx);

                if (id == -1)
                {
                    if (ASLVersion >= 391)
                    {
                        PlayerErrorMessage(PlayerError.BadThing, ctx);
                    }
                    else
                    {
                        PlayerErrorMessage(PlayerError.BadItem, ctx);
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
                    SetStringContents(varName[i], _objs[id].ObjectName, ctx, arrayIndex);
                }
            }
            else
            {
                SetStringContents(varName[i], curChunk, ctx, arrayIndex);
            }
        }

        return success;
    }

    private string GetGender(string character, bool capitalise, Context ctx)
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
                result = GetParameter(resultLine, ctx) + " ";
            }
        }

        if (capitalise)
        {
            result = Strings.UCase(Strings.Left(result, 1)) + Strings.Right(result, Strings.Len(result) - 1);
        }

        return result;
    }

    private string GetStringContents(string name, Context ctx)
    {
        var returnAlias = false;
        var arrayIndex = 0;

        // Check for property shortcut
        var cp = Strings.InStr(name, ":");
        if (cp != 0)
        {
            var objName = Strings.Trim(Strings.Left(name, cp - 1));
            var propName = Strings.Trim(Strings.Mid(name, cp + 1));

            var obp = Strings.InStr(objName, "(");
            if (obp != 0)
            {
                var cbp = Strings.InStr(obp, objName, ")");
                if (cbp != 0)
                {
                    objName = GetStringContents(Strings.Mid(objName, obp + 1, cbp - obp - 1), ctx);
                }
            }

            return GetObjectProperty(propName, GetObjectIdNoAlias(objName));
        }

        if (Strings.Left(name, 1) == "@")
        {
            returnAlias = true;
            name = Strings.Mid(name, 2);
        }

        if ((Strings.InStr(name, "[") != 0) & (Strings.InStr(name, "]") != 0))
        {
            var bp = Strings.InStr(name, "[");
            var ep = Strings.InStr(name, "]");
            var arrayIndexData = Strings.Mid(name, bp + 1, ep - bp - 1);
            if (Information.IsNumeric(arrayIndexData))
            {
                arrayIndex = Conversions.ToInteger(arrayIndexData);
            }
            else
            {
                arrayIndex = (int) Math.Round(GetNumericContents(arrayIndexData, ctx));
                if (arrayIndex == -32767)
                {
                    LogASLError(
                        "Array index in '" + name +
                        "' is not valid. An array index must be either a number or a numeric variable (without surrounding '%' characters)",
                        LogType.WarningError);
                    return "";
                }
            }

            name = Strings.Left(name, bp - 1);
        }

        // First, see if the string already exists. If it does,
        // get its contents. If not, generate an error.

        var exists = false;
        var id = default(int);

        if (_numberStringVariables > 0)
        {
            for (int i = 1, loopTo = _numberStringVariables; i <= loopTo; i++)
            {
                if ((Strings.LCase(_stringVariable[i].VariableName) ?? "") == (Strings.LCase(name) ?? ""))
                {
                    id = i;
                    exists = true;
                    break;
                }
            }
        }

        if (!exists)
        {
            LogASLError("No string variable '" + name + "' defined.", LogType.WarningError);
            return "";
        }

        if (arrayIndex > _stringVariable[id].VariableUBound)
        {
            LogASLError("Array index of '" + name + "[" + Strings.Trim(Conversion.Str(arrayIndex)) + "]' too big.",
                LogType.WarningError);
            return "";
        }

        // Now, set the contents
        if (!returnAlias)
        {
            return _stringVariable[id].VariableContents[arrayIndex];
        }

        return _objs[GetObjectIdNoAlias(_stringVariable[id].VariableContents[arrayIndex])].ObjectAlias;
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

    private bool OpenGame(string filename)
    {
        bool cdatb, result;
        bool visible;
        string room;
        var fileData = "";
        string savedQsgVersion;
        var data = "";
        string name;
        int scp, cdat;
        int scp2, scp3;
        string[] lines = null;

        _gameLoadMethod = "loaded";

        var prevQsgVersion = false;

        // TODO: Need a way to pass in the QSG file data instead of reading it from the file system
        fileData = File.ReadAllText(filename, Encoding.GetEncoding(1252));

        // Check version
        savedQsgVersion = Strings.Left(fileData, 10);

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
            lines = fileData.Split(new[] {Constants.vbCrLf, Constants.vbLf}, StringSplitOptions.None);
            _gameFileName = lines[1];
        }
        else
        {
            InitFileData(fileData);
            GetNextChunk();

            _gameFileName = GetNextChunk();
        }

        if (!File.Exists(_gameFileName))
        {
            _gameFileName = _player.GetNewGameFile(_gameFileName, "*.asl;*.cas;*.zip");
            if (string.IsNullOrEmpty(_gameFileName))
            {
                return false;
            }
        }

        // TODO: Need to load the original game file here
        throw new NotImplementedException();
        // result = InitialiseGame(_gameFileName, True)

        if (result == false)
        {
            return false;
        }

        if (!prevQsgVersion)
        {
            // Open Quest 3.0 saved game file
            _gameLoading = true;
            RestoreGameData(fileData);
            _gameLoading = false;
        }
        else
        {
            // Open Quest 2.x saved game file

            _currentRoom = lines[3];

            // Start at line 5 as line 4 is always "!c"
            var lineNumber = 5;

            do
            {
                data = lines[lineNumber];
                lineNumber += 1;
                if (data != "!i")
                {
                    scp = Strings.InStr(data, ";");
                    name = Strings.Trim(Strings.Left(data, scp - 1));
                    cdat = Conversions.ToInteger(Strings.Right(data, Strings.Len(data) - scp));

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

                    SetStringContents(name, data, _nullContext);
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

                    SetNumericVariableContents(name, Conversion.Val(data), _nullContext);
                }
            } while (data != "!e");
        }

        _saveGameFile = filename;

        return true;
    }

    private byte[] SaveGame(string filename, bool saveFile = true)
    {
        var ctx = new Context();
        string saveData;

        if (ASLVersion >= 391)
        {
            ExecuteScript(_beforeSaveScript, ctx);
        }

        if (ASLVersion >= 280)
        {
            saveData = MakeRestoreData();
        }
        else
        {
            saveData = MakeRestoreDataV2();
        }

        if (saveFile)
        {
            File.WriteAllText(filename, saveData, Encoding.GetEncoding(1252));
        }

        _saveGameFile = filename;

        return Encoding.GetEncoding(1252).GetBytes(saveData);
    }

    private string MakeRestoreDataV2()
    {
        var lines = new List<string>();
        int i;

        lines.Add("QUEST200.1");
        lines.Add(GetOriginalFilenameForQSG());
        lines.Add(_gameName);
        lines.Add(_currentRoom);

        lines.Add("!c");
        var loopTo = _numCollectables;
        for (i = 1; i <= loopTo; i++)
        {
            lines.Add(_collectables[i].Name + ";" + Conversion.Str(_collectables[i].Value));
        }

        lines.Add("!i");
        var loopTo1 = _numberItems;
        for (i = 1; i <= loopTo1; i++)
        {
            lines.Add(_items[i].Name + ";" + YesNo(_items[i].Got));
        }

        lines.Add("!o");
        var loopTo2 = _numberObjs;
        for (i = 1; i <= loopTo2; i++)
        {
            lines.Add(_objs[i].ObjectName + ";" + YesNo(_objs[i].Exists) + ";" + YesNo(_objs[i].Visible) + ";" +
                      _objs[i].ContainerRoom);
        }

        lines.Add("!p");
        var loopTo3 = _numberChars;
        for (i = 1; i <= loopTo3; i++)
        {
            lines.Add(_chars[i].ObjectName + ";" + YesNo(_chars[i].Exists) + ";" + YesNo(_chars[i].Visible) + ";" +
                      _chars[i].ContainerRoom);
        }

        lines.Add("!s");
        var loopTo4 = _numberStringVariables;
        for (i = 1; i <= loopTo4; i++)
        {
            lines.Add(_stringVariable[i].VariableName + ";" + _stringVariable[i].VariableContents[0]);
        }

        lines.Add("!n");
        var loopTo5 = _numberNumericVariables;
        for (i = 1; i <= loopTo5; i++)
        {
            lines.Add(_numericVariable[i].VariableName + ";" +
                      Conversion.Str(Conversions.ToDouble(_numericVariable[i].VariableContents[0])));
        }

        lines.Add("!e");

        return string.Join(Constants.vbCrLf, lines);
    }

    private void SetAvailability(string thingString, bool exists, Context ctx, Thing type = Thing.Object)
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
                        AddToObjectProperties("not hidden", i, ctx);
                    }
                    else
                    {
                        AddToObjectProperties("hidden", i, ctx);
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

        UpdateItems(ctx);
        UpdateObjectList(ctx);
    }

    internal void SetStringContents(string name, string value, Context ctx, int arrayIndex = 0)
    {
        var id = default(int);
        var exists = false;

        if (string.IsNullOrEmpty(name))
        {
            LogASLError("Internal error - tried to set empty string name to '" + value + "'", LogType.WarningError);
            return;
        }

        if (ASLVersion >= 281)
        {
            var bp = Strings.InStr(name, "[");
            if (bp != 0)
            {
                arrayIndex = GetArrayIndex(name, ctx).Index;
                name = Strings.Left(name, bp - 1);
            }
        }

        if (arrayIndex < 0)
        {
            LogASLError(
                "'" + name + "[" + Strings.Trim(Conversion.Str(arrayIndex)) + "]' is invalid - did not assign to array",
                LogType.WarningError);
            return;
        }

        // First, see if the string already exists. If it does,
        // modify it. If not, create it.

        if (_numberStringVariables > 0)
        {
            for (int i = 1, loopTo = _numberStringVariables; i <= loopTo; i++)
            {
                if ((Strings.LCase(_stringVariable[i].VariableName) ?? "") == (Strings.LCase(name) ?? ""))
                {
                    id = i;
                    exists = true;
                    break;
                }
            }
        }

        if (!exists)
        {
            _numberStringVariables = _numberStringVariables + 1;
            id = _numberStringVariables;
            Array.Resize(ref _stringVariable, id + 1);
            _stringVariable[id] = new VariableType();
            _stringVariable[id].VariableUBound = arrayIndex;
        }

        if (arrayIndex > _stringVariable[id].VariableUBound)
        {
            Array.Resize(ref _stringVariable[id].VariableContents, arrayIndex + 1);
            _stringVariable[id].VariableUBound = arrayIndex;
        }

        // Now, set the contents
        _stringVariable[id].VariableName = name;
        Array.Resize(ref _stringVariable[id].VariableContents, _stringVariable[id].VariableUBound + 1);
        _stringVariable[id].VariableContents[arrayIndex] = value;

        if (!string.IsNullOrEmpty(_stringVariable[id].OnChangeScript))
        {
            var script = _stringVariable[id].OnChangeScript;
            ExecuteScript(script, ctx);
        }

        if (!string.IsNullOrEmpty(_stringVariable[id].DisplayString))
        {
            UpdateStatusVars(ctx);
        }
    }

    private void SetUpCharObjectInfo()
    {
        var defaultProperties = new PropertiesActions();

        _numberChars = 0;

        // see if define type <default> exists:
        var defaultExists = false;
        for (int i = 1, loopTo = _numberSections; i <= loopTo; i++)
        {
            if (Strings.Trim(_lines[_defineBlocks[i].StartLine]) == "define type <default>")
            {
                defaultExists = true;
                defaultProperties = GetPropertiesInType("default");
                break;
            }
        }

        for (int i = 1, loopTo1 = _numberSections; i <= loopTo1; i++)
        {
            var block = _defineBlocks[i];
            if (!(BeginsWith(_lines[block.StartLine], "define room") |
                  BeginsWith(_lines[block.StartLine], "define game") |
                  BeginsWith(_lines[block.StartLine], "define object ")))
            {
                continue;
            }

            string restOfLine;
            string origContainerRoomName, containerRoomName;

            if (BeginsWith(_lines[block.StartLine], "define room"))
            {
                origContainerRoomName = GetParameter(_lines[block.StartLine], _nullContext);
            }
            else
            {
                origContainerRoomName = "";
            }

            var startLine = block.StartLine;
            var endLine = block.EndLine;

            if (BeginsWith(_lines[block.StartLine], "define object "))
            {
                startLine = startLine - 1;
                endLine = endLine + 1;
            }

            for (int j = startLine + 1, loopTo2 = endLine - 1; j <= loopTo2; j++)
            {
                if (BeginsWith(_lines[j], "define object"))
                {
                    containerRoomName = origContainerRoomName;

                    _numberObjs = _numberObjs + 1;
                    Array.Resize(ref _objs, _numberObjs + 1);
                    _objs[_numberObjs] = new ObjectType();

                    var o = _objs[_numberObjs];

                    o.ObjectName = GetParameter(_lines[j], _nullContext);
                    o.ObjectAlias = o.ObjectName;
                    o.DefinitionSectionStart = j;
                    o.ContainerRoom = containerRoomName;
                    o.Visible = true;
                    o.Gender = "it";
                    o.Article = "it";

                    o.Take.Type = TextActionType.Nothing;

                    if (defaultExists)
                    {
                        AddToObjectProperties(defaultProperties.Properties, _numberObjs, _nullContext);
                        for (int k = 1, loopTo3 = defaultProperties.NumberActions; k <= loopTo3; k++)
                        {
                            AddObjectAction(_numberObjs, defaultProperties.Actions[k].ActionName,
                                defaultProperties.Actions[k].Script);
                        }
                    }

                    if (ASLVersion >= 391)
                    {
                        AddToObjectProperties("list", _numberObjs, _nullContext);
                    }

                    var hidden = false;
                    do
                    {
                        j = j + 1;
                        if (Strings.Trim(_lines[j]) == "hidden")
                        {
                            o.Exists = false;
                            hidden = true;
                            if (ASLVersion >= 311)
                            {
                                AddToObjectProperties("hidden", _numberObjs, _nullContext);
                            }
                        }
                        else if (BeginsWith(_lines[j], "startin ") & (containerRoomName == "__UNKNOWN"))
                        {
                            containerRoomName = GetParameter(_lines[j], _nullContext);
                        }
                        else if (BeginsWith(_lines[j], "prefix "))
                        {
                            o.Prefix = GetParameter(_lines[j], _nullContext) + " ";
                            if (ASLVersion >= 311)
                            {
                                AddToObjectProperties("prefix=" + o.Prefix, _numberObjs, _nullContext);
                            }
                        }
                        else if (BeginsWith(_lines[j], "suffix "))
                        {
                            o.Suffix = GetParameter(_lines[j], _nullContext);
                            if (ASLVersion >= 311)
                            {
                                AddToObjectProperties("suffix=" + o.Suffix, _numberObjs, _nullContext);
                            }
                        }
                        else if (Strings.Trim(_lines[j]) == "invisible")
                        {
                            o.Visible = false;
                            if (ASLVersion >= 311)
                            {
                                AddToObjectProperties("invisible", _numberObjs, _nullContext);
                            }
                        }
                        else if (BeginsWith(_lines[j], "alias "))
                        {
                            o.ObjectAlias = GetParameter(_lines[j], _nullContext);
                            if (ASLVersion >= 311)
                            {
                                AddToObjectProperties("alias=" + o.ObjectAlias, _numberObjs, _nullContext);
                            }
                        }
                        else if (BeginsWith(_lines[j], "alt "))
                        {
                            AddToObjectAltNames(GetParameter(_lines[j], _nullContext), _numberObjs);
                        }
                        else if (BeginsWith(_lines[j], "detail "))
                        {
                            o.Detail = GetParameter(_lines[j], _nullContext);
                            if (ASLVersion >= 311)
                            {
                                AddToObjectProperties("detail=" + o.Detail, _numberObjs, _nullContext);
                            }
                        }
                        else if (BeginsWith(_lines[j], "gender "))
                        {
                            o.Gender = GetParameter(_lines[j], _nullContext);
                            if (ASLVersion >= 311)
                            {
                                AddToObjectProperties("gender=" + o.Gender, _numberObjs, _nullContext);
                            }
                        }
                        else if (BeginsWith(_lines[j], "article "))
                        {
                            o.Article = GetParameter(_lines[j], _nullContext);
                            if (ASLVersion >= 311)
                            {
                                AddToObjectProperties("article=" + o.Article, _numberObjs, _nullContext);
                            }
                        }
                        else if (BeginsWith(_lines[j], "gain "))
                        {
                            o.GainScript = GetEverythingAfter(_lines[j], "gain ");
                            AddObjectAction(_numberObjs, "gain", o.GainScript);
                        }
                        else if (BeginsWith(_lines[j], "lose "))
                        {
                            o.LoseScript = GetEverythingAfter(_lines[j], "lose ");
                            AddObjectAction(_numberObjs, "lose", o.LoseScript);
                        }
                        else if (BeginsWith(_lines[j], "displaytype "))
                        {
                            o.DisplayType = GetParameter(_lines[j], _nullContext);
                            if (ASLVersion >= 311)
                            {
                                AddToObjectProperties("displaytype=" + o.DisplayType, _numberObjs, _nullContext);
                            }
                        }
                        else if (BeginsWith(_lines[j], "look "))
                        {
                            if (ASLVersion >= 311)
                            {
                                restOfLine = GetEverythingAfter(_lines[j], "look ");
                                if (Strings.Left(restOfLine, 1) == "<")
                                {
                                    AddToObjectProperties("look=" + GetParameter(_lines[j], _nullContext), _numberObjs,
                                        _nullContext);
                                }
                                else
                                {
                                    AddObjectAction(_numberObjs, "look", restOfLine);
                                }
                            }
                        }
                        else if (BeginsWith(_lines[j], "examine "))
                        {
                            if (ASLVersion >= 311)
                            {
                                restOfLine = GetEverythingAfter(_lines[j], "examine ");
                                if (Strings.Left(restOfLine, 1) == "<")
                                {
                                    AddToObjectProperties("examine=" + GetParameter(_lines[j], _nullContext),
                                        _numberObjs, _nullContext);
                                }
                                else
                                {
                                    AddObjectAction(_numberObjs, "examine", restOfLine);
                                }
                            }
                        }
                        else if ((ASLVersion >= 311) & BeginsWith(_lines[j], "speak "))
                        {
                            restOfLine = GetEverythingAfter(_lines[j], "speak ");
                            if (Strings.Left(restOfLine, 1) == "<")
                            {
                                AddToObjectProperties("speak=" + GetParameter(_lines[j], _nullContext), _numberObjs,
                                    _nullContext);
                            }
                            else
                            {
                                AddObjectAction(_numberObjs, "speak", restOfLine);
                            }
                        }
                        else if (BeginsWith(_lines[j], "properties "))
                        {
                            AddToObjectProperties(GetParameter(_lines[j], _nullContext), _numberObjs, _nullContext);
                        }
                        else if (BeginsWith(_lines[j], "type "))
                        {
                            o.NumberTypesIncluded = o.NumberTypesIncluded + 1;
                            Array.Resize(ref o.TypesIncluded, o.NumberTypesIncluded + 1);
                            o.TypesIncluded[o.NumberTypesIncluded] = GetParameter(_lines[j], _nullContext);

                            var PropertyData = GetPropertiesInType(GetParameter(_lines[j], _nullContext));
                            AddToObjectProperties(PropertyData.Properties, _numberObjs, _nullContext);
                            for (int k = 1, loopTo4 = PropertyData.NumberActions; k <= loopTo4; k++)
                            {
                                AddObjectAction(_numberObjs, PropertyData.Actions[k].ActionName,
                                    PropertyData.Actions[k].Script);
                            }

                            Array.Resize(ref o.TypesIncluded,
                                o.NumberTypesIncluded + PropertyData.NumberTypesIncluded + 1);
                            for (int k = 1, loopTo5 = PropertyData.NumberTypesIncluded; k <= loopTo5; k++)
                            {
                                o.TypesIncluded[k + o.NumberTypesIncluded] = PropertyData.TypesIncluded[k];
                            }

                            o.NumberTypesIncluded = o.NumberTypesIncluded + PropertyData.NumberTypesIncluded;
                        }
                        else if (BeginsWith(_lines[j], "action "))
                        {
                            AddToObjectActions(GetEverythingAfter(_lines[j], "action "), _numberObjs, _nullContext);
                        }
                        else if (BeginsWith(_lines[j], "use "))
                        {
                            AddToUseInfo(_numberObjs, GetEverythingAfter(_lines[j], "use "));
                        }
                        else if (BeginsWith(_lines[j], "give "))
                        {
                            AddToGiveInfo(_numberObjs, GetEverythingAfter(_lines[j], "give "));
                        }
                        else if (Strings.Trim(_lines[j]) == "take")
                        {
                            o.Take.Type = TextActionType.Default;
                            AddToObjectProperties("take", _numberObjs, _nullContext);
                        }
                        else if (BeginsWith(_lines[j], "take "))
                        {
                            if (Strings.Left(GetEverythingAfter(_lines[j], "take "), 1) == "<")
                            {
                                o.Take.Type = TextActionType.Text;
                                o.Take.Data = GetParameter(_lines[j], _nullContext);

                                AddToObjectProperties("take=" + GetParameter(_lines[j], _nullContext), _numberObjs,
                                    _nullContext);
                            }
                            else
                            {
                                o.Take.Type = TextActionType.Script;
                                restOfLine = GetEverythingAfter(_lines[j], "take ");
                                o.Take.Data = restOfLine;

                                AddObjectAction(_numberObjs, "take", restOfLine);
                            }
                        }
                        else if (Strings.Trim(_lines[j]) == "container")
                        {
                            if (ASLVersion >= 391)
                            {
                                AddToObjectProperties("container", _numberObjs, _nullContext);
                            }
                        }
                        else if (Strings.Trim(_lines[j]) == "surface")
                        {
                            if (ASLVersion >= 391)
                            {
                                AddToObjectProperties("container", _numberObjs, _nullContext);
                                AddToObjectProperties("surface", _numberObjs, _nullContext);
                            }
                        }
                        else if (Strings.Trim(_lines[j]) == "opened")
                        {
                            if (ASLVersion >= 391)
                            {
                                AddToObjectProperties("opened", _numberObjs, _nullContext);
                            }
                        }
                        else if (Strings.Trim(_lines[j]) == "transparent")
                        {
                            if (ASLVersion >= 391)
                            {
                                AddToObjectProperties("transparent", _numberObjs, _nullContext);
                            }
                        }
                        else if (Strings.Trim(_lines[j]) == "open")
                        {
                            AddToObjectProperties("open", _numberObjs, _nullContext);
                        }
                        else if (BeginsWith(_lines[j], "open "))
                        {
                            if (Strings.Left(GetEverythingAfter(_lines[j], "open "), 1) == "<")
                            {
                                AddToObjectProperties("open=" + GetParameter(_lines[j], _nullContext), _numberObjs,
                                    _nullContext);
                            }
                            else
                            {
                                restOfLine = GetEverythingAfter(_lines[j], "open ");
                                AddObjectAction(_numberObjs, "open", restOfLine);
                            }
                        }
                        else if (Strings.Trim(_lines[j]) == "close")
                        {
                            AddToObjectProperties("close", _numberObjs, _nullContext);
                        }
                        else if (BeginsWith(_lines[j], "close "))
                        {
                            if (Strings.Left(GetEverythingAfter(_lines[j], "close "), 1) == "<")
                            {
                                AddToObjectProperties("close=" + GetParameter(_lines[j], _nullContext), _numberObjs,
                                    _nullContext);
                            }
                            else
                            {
                                restOfLine = GetEverythingAfter(_lines[j], "close ");
                                AddObjectAction(_numberObjs, "close", restOfLine);
                            }
                        }
                        else if (Strings.Trim(_lines[j]) == "add")
                        {
                            AddToObjectProperties("add", _numberObjs, _nullContext);
                        }
                        else if (BeginsWith(_lines[j], "add "))
                        {
                            if (Strings.Left(GetEverythingAfter(_lines[j], "add "), 1) == "<")
                            {
                                AddToObjectProperties("add=" + GetParameter(_lines[j], _nullContext), _numberObjs,
                                    _nullContext);
                            }
                            else
                            {
                                restOfLine = GetEverythingAfter(_lines[j], "add ");
                                AddObjectAction(_numberObjs, "add", restOfLine);
                            }
                        }
                        else if (Strings.Trim(_lines[j]) == "remove")
                        {
                            AddToObjectProperties("remove", _numberObjs, _nullContext);
                        }
                        else if (BeginsWith(_lines[j], "remove "))
                        {
                            if (Strings.Left(GetEverythingAfter(_lines[j], "remove "), 1) == "<")
                            {
                                AddToObjectProperties("remove=" + GetParameter(_lines[j], _nullContext), _numberObjs,
                                    _nullContext);
                            }
                            else
                            {
                                restOfLine = GetEverythingAfter(_lines[j], "remove ");
                                AddObjectAction(_numberObjs, "remove", restOfLine);
                            }
                        }
                        else if (BeginsWith(_lines[j], "parent "))
                        {
                            AddToObjectProperties("parent=" + GetParameter(_lines[j], _nullContext), _numberObjs,
                                _nullContext);
                        }
                        else if (BeginsWith(_lines[j], "list"))
                        {
                            ProcessListInfo(_lines[j], _numberObjs);
                        }
                    } while (Strings.Trim(_lines[j]) != "end define");

                    o.DefinitionSectionEnd = j;
                    if (!hidden)
                    {
                        o.Exists = true;
                    }
                }
                else if ((ASLVersion <= 280) & BeginsWith(_lines[j], "define character"))
                {
                    containerRoomName = origContainerRoomName;
                    _numberChars = _numberChars + 1;
                    Array.Resize(ref _chars, _numberChars + 1);
                    _chars[_numberChars] = new ObjectType();
                    _chars[_numberChars].ObjectName = GetParameter(_lines[j], _nullContext);
                    _chars[_numberChars].DefinitionSectionStart = j;
                    _chars[_numberChars].ContainerRoom = "";
                    _chars[_numberChars].Visible = true;
                    var hidden = false;
                    do
                    {
                        j = j + 1;
                        if (Strings.Trim(_lines[j]) == "hidden")
                        {
                            _chars[_numberChars].Exists = false;
                            hidden = true;
                        }
                        else if (BeginsWith(_lines[j], "startin ") & (containerRoomName == "__UNKNOWN"))
                        {
                            containerRoomName = GetParameter(_lines[j], _nullContext);
                        }
                        else if (BeginsWith(_lines[j], "prefix "))
                        {
                            _chars[_numberChars].Prefix = GetParameter(_lines[j], _nullContext) + " ";
                        }
                        else if (BeginsWith(_lines[j], "suffix "))
                        {
                            _chars[_numberChars].Suffix = " " + GetParameter(_lines[j], _nullContext);
                        }
                        else if (Strings.Trim(_lines[j]) == "invisible")
                        {
                            _chars[_numberChars].Visible = false;
                        }
                        else if (BeginsWith(_lines[j], "alias "))
                        {
                            _chars[_numberChars].ObjectAlias = GetParameter(_lines[j], _nullContext);
                        }
                        else if (BeginsWith(_lines[j], "detail "))
                        {
                            _chars[_numberChars].Detail = GetParameter(_lines[j], _nullContext);
                        }

                        _chars[_numberChars].ContainerRoom = containerRoomName;
                    } while (Strings.Trim(_lines[j]) != "end define");

                    _chars[_numberChars].DefinitionSectionEnd = j;
                    if (!hidden)
                    {
                        _chars[_numberChars].Exists = true;
                    }
                }
            }
        }

        UpdateVisibilityInContainers(_nullContext);
    }

    private void ShowGameAbout(Context ctx)
    {
        var version = FindStatement(GetDefineBlock("game"), "game version");
        var author = FindStatement(GetDefineBlock("game"), "game author");
        var copyright = FindStatement(GetDefineBlock("game"), "game copyright");
        var info = FindStatement(GetDefineBlock("game"), "game info");

        Print("|bGame name:|cl  " + _gameName + "|cb|xb", ctx);
        if (!string.IsNullOrEmpty(version))
        {
            Print("|bVersion:|xb    " + version, ctx);
        }

        if (!string.IsNullOrEmpty(author))
        {
            Print("|bAuthor:|xb     " + author, ctx);
        }

        if (!string.IsNullOrEmpty(copyright))
        {
            Print("|bCopyright:|xb  " + copyright, ctx);
        }

        if (!string.IsNullOrEmpty(info))
        {
            Print("", ctx);
            Print(info, ctx);
        }
    }

    private void ShowPicture(string filename)
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
            Print(caption, _nullContext);
        }

        ShowPictureInText(filename);
    }

    private void ShowRoomInfo(string room, Context ctx, bool noPrint = false)
    {
        if (ASLVersion < 280)
        {
            ShowRoomInfoV2(room);
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

        SetStringContents("quest.formatroom", roomDisplayNameNoFormat, ctx);

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
            SetStringContents("quest.formatobjects", visibleObjects, ctx);
            visibleObjects = "There is " + visibleObjects + " here.";
            SetStringContents("quest.objects", visibleObjectsNoFormat, ctx);
            roomDisplayText = roomDisplayText + visibleObjects + Constants.vbCrLf;
        }
        else
        {
            SetStringContents("quest.objects", "", ctx);
            SetStringContents("quest.formatobjects", "", ctx);
        }

        // SHOW EXITS ***************************************************************

        doorwayString = UpdateDoorways(id, ctx);

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
                SetStringContents("quest.doorways.places", placeList, ctx);
            }
            else
            {
                SetStringContents("quest.doorways.places", "", ctx);
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

        SetStringContents("quest.lookdesc", lookDesc, ctx);


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

        if (descTagExist == false)
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
                        descLine = GetParameter(descLine, ctx);
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
            if (descTagExist == false)
            {
                // Remove final vbCrLf:
                roomDisplayText = Strings.Left(roomDisplayText, Strings.Len(roomDisplayText) - 2);
                Print(roomDisplayText, ctx);
                if (!string.IsNullOrEmpty(doorwayString))
                {
                    Print(doorwayString, ctx);
                }
            }
            // execute description tag:
            // If no script, just print the tag's parameter.
            // Otherwise, execute it as ASL script:

            else if (descType == (int) TextActionType.Text)
            {
                Print(descLine, ctx);
            }
            else
            {
                ExecuteScript(descLine, ctx);
            }

            UpdateObjectList(ctx);

            // SHOW "LOOK" DESCRIPTION **************************************************

            if (showLookText)
            {
                if (!string.IsNullOrEmpty(lookDesc))
                {
                    Print(lookDesc, ctx);
                }
            }
        }
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
            display = "You have " + Strings.Trim(Conversion.Str(_collectables[id].Value)) + " " +
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
                display = firstBit + Strings.Trim(Conversion.Str(_collectables[id].Value)) + nextBit;
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

        if ((_collectables[id].Value == 0d) & (_collectables[id].DisplayWhenZero == false))
        {
            display = "<null>";
        }

        return display;
    }

    private void DisplayTextSection(string section, Context ctx)
    {
        DefineBlock block;
        block = DefineBlockParam("text", section);

        if (block.StartLine == 0)
        {
            return;
        }

        for (int i = block.StartLine + 1, loopTo = block.EndLine - 1; i <= loopTo; i++)
        {
            if (ASLVersion >= 392)
            {
                // Convert string variables etc.
                Print(GetParameter("<" + _lines[i] + ">", ctx), ctx);
            }
            else
            {
                Print(_lines[i], ctx);
            }
        }

        Print("", ctx);
    }

    // Returns true if the system is ready to process a new command after completion - so it will be
    // in most cases, except when ExecCommand just caused an "enter" script command to complete

    private bool ExecCommand(string input, Context ctx, bool echo = true, bool runUserCommand = true,
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

        lock (_commandLock)
        {
            if (_commandOverrideModeOn)
            {
                // Commands have been overridden for this command,
                // so put input into previously specified variable
                // and exit:

                SetStringContents(_commandOverrideVariable, input, ctx);
                Monitor.PulseAll(_commandLock);
                return false;
            }
        }

        bool userCommandReturn;

        if (echo)
        {
            Print("> " + input, ctx);
        }

        input = Strings.LCase(input);

        SetStringContents("quest.originalcommand", input, ctx);

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

        SetStringContents("quest.command", input, ctx);

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
                    ExecuteScript(GetEverythingAfter(_rooms[roomID].BeforeTurnScript, "override"), newCtx);
                    globalOverride = true;
                }
                else
                {
                    ExecuteScript(_rooms[roomID].BeforeTurnScript, newCtx);
                }
            }
        }

        if (!string.IsNullOrEmpty(_beforeTurnScript) & (globalOverride == false))
        {
            ExecuteScript(_beforeTurnScript, newCtx);
        }

        // In executing BeforeTurn script, "dontprocess" sets ctx.DontProcessCommand,
        // in which case we don't process the command.

        if (!newCtx.DontProcessCommand)
        {
            // Try to execute user defined command, if allowed:

            userCommandReturn = false;
            if (runUserCommand)
            {
                userCommandReturn = ExecUserCommand(input, ctx);

                if (!userCommandReturn)
                {
                    userCommandReturn = ExecVerb(input, ctx);
                }

                if (!userCommandReturn)
                {
                    // Try command defined by a library
                    userCommandReturn = ExecUserCommand(input, ctx, true);
                }

                if (!userCommandReturn)
                {
                    // Try verb defined by a library
                    userCommandReturn = ExecVerb(input, ctx, true);
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
                ExecSpeak(parameter, ctx);
            }
            else if (CmdStartsWith(input, "talk to "))
            {
                parameter = GetEverythingAfter(input, "talk to ");
                ExecSpeak(parameter, ctx);
            }
            else if ((cmd == "exit") | (cmd == "out") | (cmd == "leave"))
            {
                GoDirection("out", ctx);
                _lastIt = 0;
            }
            else if ((cmd == "north") | (cmd == "south") | (cmd == "east") | (cmd == "west"))
            {
                GoDirection(input, ctx);
                _lastIt = 0;
            }
            else if ((cmd == "n") | (cmd == "s") | (cmd == "w") | (cmd == "e"))
            {
                switch (Strings.InStr("nswe", cmd))
                {
                    case 1:
                    {
                        GoDirection("north", ctx);
                        break;
                    }
                    case 2:
                    {
                        GoDirection("south", ctx);
                        break;
                    }
                    case 3:
                    {
                        GoDirection("west", ctx);
                        break;
                    }
                    case 4:
                    {
                        GoDirection("east", ctx);
                        break;
                    }
                }

                _lastIt = 0;
            }
            else if ((cmd == "ne") | (cmd == "northeast") | (cmd == "north-east") | (cmd == "north east") |
                     (cmd == "go ne") |
                     (cmd == "go northeast") | (cmd == "go north-east") | (cmd == "go north east"))
            {
                GoDirection("northeast", ctx);
                _lastIt = 0;
            }
            else if ((cmd == "nw") | (cmd == "northwest") | (cmd == "north-west") | (cmd == "north west") |
                     (cmd == "go nw") |
                     (cmd == "go northwest") | (cmd == "go north-west") | (cmd == "go north west"))
            {
                GoDirection("northwest", ctx);
                _lastIt = 0;
            }
            else if ((cmd == "se") | (cmd == "southeast") | (cmd == "south-east") | (cmd == "south east") |
                     (cmd == "go se") |
                     (cmd == "go southeast") | (cmd == "go south-east") | (cmd == "go south east"))
            {
                GoDirection("southeast", ctx);
                _lastIt = 0;
            }
            else if ((cmd == "sw") | (cmd == "southwest") | (cmd == "south-west") | (cmd == "south west") |
                     (cmd == "go sw") |
                     (cmd == "go southwest") | (cmd == "go south-west") | (cmd == "go south west"))
            {
                GoDirection("southwest", ctx);
                _lastIt = 0;
            }
            else if ((cmd == "up") | (cmd == "u"))
            {
                GoDirection("up", ctx);
                _lastIt = 0;
            }
            else if ((cmd == "down") | (cmd == "d"))
            {
                GoDirection("down", ctx);
                _lastIt = 0;
            }
            else if (CmdStartsWith(input, "go "))
            {
                if (ASLVersion >= 410)
                {
                    _rooms[GetRoomID(_currentRoom, ctx)].Exits.ExecuteGo(input, ref ctx);
                }
                else
                {
                    parameter = GetEverythingAfter(input, "go ");
                    if (parameter == "out")
                    {
                        GoDirection("out", ctx);
                    }
                    else if ((parameter == "north") | (parameter == "south") | (parameter == "east") |
                             (parameter == "west") |
                             (parameter == "up") | (parameter == "down"))
                    {
                        GoDirection(parameter, ctx);
                    }
                    else if (BeginsWith(parameter, "to "))
                    {
                        parameter = GetEverythingAfter(parameter, "to ");
                        GoToPlace(parameter, ctx);
                    }
                    else
                    {
                        PlayerErrorMessage(PlayerError.BadGo, ctx);
                    }
                }

                _lastIt = 0;
            }
            else if (CmdStartsWith(input, "give "))
            {
                parameter = GetEverythingAfter(input, "give ");
                ExecGive(parameter, ctx);
            }
            else if (CmdStartsWith(input, "take "))
            {
                parameter = GetEverythingAfter(input, "take ");
                ExecTake(parameter, ctx);
            }
            else if (CmdStartsWith(input, "drop ") & (ASLVersion >= 280))
            {
                parameter = GetEverythingAfter(input, "drop ");
                ExecDrop(parameter, ctx);
            }
            else if (CmdStartsWith(input, "get "))
            {
                parameter = GetEverythingAfter(input, "get ");
                ExecTake(parameter, ctx);
            }
            else if (CmdStartsWith(input, "pick up "))
            {
                parameter = GetEverythingAfter(input, "pick up ");
                ExecTake(parameter, ctx);
            }
            else if ((cmd == "pick it up") | (cmd == "pick them up") | (cmd == "pick this up") |
                     (cmd == "pick that up") |
                     (cmd == "pick these up") | (cmd == "pick those up") | (cmd == "pick him up") |
                     (cmd == "pick her up"))
            {
                ExecTake(Strings.Mid(cmd, 6, Strings.InStr(7, cmd, " ") - 6), ctx);
            }
            else if (CmdStartsWith(input, "look "))
            {
                ExecLook(input, ctx);
            }
            else if (CmdStartsWith(input, "l "))
            {
                ExecLook("look " + GetEverythingAfter(input, "l "), ctx);
            }
            else if (CmdStartsWith(input, "examine ") & (ASLVersion >= 280))
            {
                ExecExamine(input, ctx);
            }
            else if (CmdStartsWith(input, "x ") & (ASLVersion >= 280))
            {
                ExecExamine("examine " + GetEverythingAfter(input, "x "), ctx);
            }
            else if ((cmd == "l") | (cmd == "look"))
            {
                ExecLook("look", ctx);
            }
            else if ((cmd == "x") | (cmd == "examine"))
            {
                ExecExamine("examine", ctx);
            }
            else if (CmdStartsWith(input, "use "))
            {
                ExecUse(input, ctx);
            }
            else if (CmdStartsWith(input, "open ") & (ASLVersion >= 391))
            {
                ExecOpenClose(input, ctx);
            }
            else if (CmdStartsWith(input, "close ") & (ASLVersion >= 391))
            {
                ExecOpenClose(input, ctx);
            }
            else if (CmdStartsWith(input, "put ") & (ASLVersion >= 391))
            {
                ExecAddRemove(input, ctx);
            }
            else if (CmdStartsWith(input, "add ") & (ASLVersion >= 391))
            {
                ExecAddRemove(input, ctx);
            }
            else if (CmdStartsWith(input, "remove ") & (ASLVersion >= 391))
            {
                ExecAddRemove(input, ctx);
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
                ShowHelp(ctx);
            }
            else if (cmd == "about")
            {
                ShowGameAbout(ctx);
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
                    Print(logEntry, ctx);
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

                    Print("You are carrying:|n" + invList + ".", ctx);
                }
                else
                {
                    Print("You are not carrying anything.", ctx);
                }
            }
            else if (CmdStartsWith(input, "oops "))
            {
                ExecOops(GetEverythingAfter(input, "oops "), ctx);
            }
            else if (CmdStartsWith(input, "the "))
            {
                ExecOops(GetEverythingAfter(input, "the "), ctx);
            }
            else
            {
                PlayerErrorMessage(PlayerError.BadCommand, ctx);
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
                        ExecuteScript(GetEverythingAfter(_rooms[roomID].AfterTurnScript, "override"), ctx);
                        globalOverride = true;
                    }
                    else
                    {
                        ExecuteScript(_rooms[roomID].AfterTurnScript, ctx);
                    }
                }
            }

            // was set to NullThread here for some reason
            if (!string.IsNullOrEmpty(_afterTurnScript) & (globalOverride == false))
            {
                ExecuteScript(_afterTurnScript, ctx);
            }
        }

        Print("", ctx);

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

    private void ExecGive(string giveString, Context ctx)
    {
        var article = "";
        string item, character;
        Thing type;
        var id = default(int);
        var script = "";
        var toPos = Strings.InStr(giveString, " to ");

        if (toPos == 0)
        {
            toPos = Strings.InStr(giveString, " the ");
            if (toPos == 0)
            {
                PlayerErrorMessage(PlayerError.BadGive, ctx);
                return;
            }

            item = Strings.Trim(Strings.Mid(giveString, toPos + 4, Strings.Len(giveString) - (toPos + 2)));
            character = Strings.Trim(Strings.Mid(giveString, 1, toPos));
        }
        else
        {
            item = Strings.Trim(Strings.Mid(giveString, 1, toPos));
            character = Strings.Trim(Strings.Mid(giveString, toPos + 3, Strings.Len(giveString) - (toPos + 2)));
        }

        if (ASLVersion >= 281)
        {
            type = Thing.Object;
        }
        else
        {
            type = Thing.Character;
        }

        // First see if player has "ItemToGive":
        if (ASLVersion >= 280)
        {
            id = Disambiguate(item, "inventory", ctx);

            if (id == -1)
            {
                PlayerErrorMessage(PlayerError.NoItem, ctx);
                _badCmdBefore = "give";
                _badCmdAfter = "to " + character;
                return;
            }

            if (id == -2)
            {
                return;
            }

            article = _objs[id].Article;
        }
        else
        {
            // ASL2:
            var notGot = true;

            for (int i = 1, loopTo = _numberItems; i <= loopTo; i++)
            {
                if ((Strings.LCase(_items[i].Name) ?? "") == (Strings.LCase(item) ?? ""))
                {
                    if (_items[i].Got == false)
                    {
                        notGot = true;
                        i = _numberItems;
                    }
                    else
                    {
                        notGot = false;
                    }
                }
            }

            if (notGot)
            {
                PlayerErrorMessage(PlayerError.NoItem, ctx);
                return;
            }
        }

        if (ASLVersion >= 281)
        {
            var foundScript = false;
            var foundObject = false;

            var giveToId = Disambiguate(character, _currentRoom, ctx);
            if (giveToId > 0)
            {
                foundObject = true;
            }

            if (!foundObject)
            {
                if (giveToId != -2)
                {
                    PlayerErrorMessage(PlayerError.BadCharacter, ctx);
                }

                _badCmdBefore = "give " + item + " to";
                return;
            }

            // Find appropriate give script ****
            // now, for "give a to b", we have
            // ItemID=a and GiveToObjectID=b

            var o = _objs[giveToId];

            for (int i = 1, loopTo1 = o.NumberGiveData; i <= loopTo1; i++)
            {
                if ((o.GiveData[i].GiveType == GiveType.GiveSomethingTo) &
                    ((Strings.LCase(o.GiveData[i].GiveObject) ?? "") == (Strings.LCase(_objs[id].ObjectName) ?? "")))
                {
                    foundScript = true;
                    script = o.GiveData[i].GiveScript;
                    break;
                }
            }

            if (!foundScript)
            {
                // check a for give to <b>:

                var g = _objs[id];

                for (int i = 1, loopTo2 = g.NumberGiveData; i <= loopTo2; i++)
                {
                    if ((g.GiveData[i].GiveType == GiveType.GiveToSomething) &
                        ((Strings.LCase(g.GiveData[i].GiveObject) ?? "") ==
                         (Strings.LCase(_objs[giveToId].ObjectName) ?? "")))
                    {
                        foundScript = true;
                        script = g.GiveData[i].GiveScript;
                        break;
                    }
                }
            }

            if (!foundScript)
            {
                // check b for give anything:
                script = _objs[giveToId].GiveAnything;
                if (!string.IsNullOrEmpty(script))
                {
                    foundScript = true;
                    SetStringContents("quest.give.object.name", _objs[id].ObjectName, ctx);
                }
            }

            if (!foundScript)
            {
                // check a for give to anything:
                script = _objs[id].GiveToAnything;
                if (!string.IsNullOrEmpty(script))
                {
                    foundScript = true;
                    SetStringContents("quest.give.object.name", _objs[giveToId].ObjectName, ctx);
                }
            }

            if (foundScript)
            {
                ExecuteScript(script, ctx, id);
            }
            else
            {
                SetStringContents("quest.error.charactername", _objs[giveToId].ObjectName, ctx);

                var gender = Strings.Trim(_objs[giveToId].Gender);
                gender = Strings.UCase(Strings.Left(gender, 1)) + Strings.Mid(gender, 2);
                SetStringContents("quest.error.gender", gender, ctx);

                SetStringContents("quest.error.article", article, ctx);
                PlayerErrorMessage(PlayerError.ItemUnwanted, ctx);
            }
        }
        else
        {
            // ASL2:
            var block = GetThingBlock(character, _currentRoom, type);

            if (((block.StartLine == 0) & (block.EndLine == 0)) |
                (IsAvailable(character + "@" + _currentRoom, type, ctx) == false))
            {
                PlayerErrorMessage(PlayerError.BadCharacter, ctx);
                return;
            }

            var realName = _chars[GetThingNumber(character, _currentRoom, type)].ObjectName;

            // now, see if there's a give statement for this item in
            // this characters definition:

            var giveLine = 0;
            for (int i = block.StartLine + 1, loopTo3 = block.EndLine - 1; i <= loopTo3; i++)
            {
                if (BeginsWith(_lines[i], "give"))
                {
                    var ItemCheck = GetParameter(_lines[i], ctx);
                    if ((Strings.LCase(ItemCheck) ?? "") == (Strings.LCase(item) ?? ""))
                    {
                        giveLine = i;
                    }
                }
            }

            if (giveLine == 0)
            {
                if (string.IsNullOrEmpty(article))
                {
                    article = "it";
                }

                SetStringContents("quest.error.charactername", realName, ctx);
                SetStringContents("quest.error.gender", Strings.Trim(GetGender(character, true, ctx)), ctx);
                SetStringContents("quest.error.article", article, ctx);
                PlayerErrorMessage(PlayerError.ItemUnwanted, ctx);
                return;
            }

            // now, execute the statement on GiveLine
            ExecuteScript(GetSecondChunk(_lines[giveLine]), ctx);
        }
    }

    private void ExecLook(string lookLine, Context ctx)
    {
        string item;

        if (Strings.Trim(lookLine) == "look")
        {
            ShowRoomInfo(_currentRoom, ctx);
        }
        else
        {
            if (ASLVersion < 391)
            {
                var atPos = Strings.InStr(lookLine, " at ");

                if (atPos == 0)
                {
                    item = GetEverythingAfter(lookLine, "look ");
                }
                else
                {
                    item = Strings.Trim(Strings.Mid(lookLine, atPos + 4));
                }
            }
            else if (BeginsWith(lookLine, "look at "))
            {
                item = GetEverythingAfter(lookLine, "look at ");
            }
            else if (BeginsWith(lookLine, "look in "))
            {
                item = GetEverythingAfter(lookLine, "look in ");
            }
            else if (BeginsWith(lookLine, "look on "))
            {
                item = GetEverythingAfter(lookLine, "look on ");
            }
            else if (BeginsWith(lookLine, "look inside "))
            {
                item = GetEverythingAfter(lookLine, "look inside ");
            }
            else
            {
                item = GetEverythingAfter(lookLine, "look ");
            }

            if (ASLVersion >= 280)
            {
                var id = Disambiguate(item, "inventory;" + _currentRoom, ctx);

                if (id <= 0)
                {
                    if (id != -2)
                    {
                        PlayerErrorMessage(PlayerError.BadThing, ctx);
                    }

                    _badCmdBefore = "look at";
                    return;
                }

                DoLook(id, ctx);
            }
            else
            {
                if (BeginsWith(item, "the "))
                {
                    item = GetEverythingAfter(item, "the ");
                }

                lookLine = RetrLine("object", item, "look", ctx);

                if (lookLine != "<unfound>")
                {
                    // Check for availability
                    if (IsAvailable(item, Thing.Object, ctx) == false)
                    {
                        lookLine = "<unfound>";
                    }
                }

                if (lookLine == "<unfound>")
                {
                    lookLine = RetrLine("character", item, "look", ctx);

                    if (lookLine != "<unfound>")
                    {
                        if (IsAvailable(item, Thing.Character, ctx) == false)
                        {
                            lookLine = "<unfound>";
                        }
                    }

                    if (lookLine == "<unfound>")
                    {
                        PlayerErrorMessage(PlayerError.BadThing, ctx);
                        return;
                    }

                    if (lookLine == "<undefined>")
                    {
                        PlayerErrorMessage(PlayerError.DefaultLook, ctx);
                        return;
                    }
                }
                else if (lookLine == "<undefined>")
                {
                    PlayerErrorMessage(PlayerError.DefaultLook, ctx);
                    return;
                }

                var lookData = Strings.Trim(GetEverythingAfter(Strings.Trim(lookLine), "look "));
                if (Strings.Left(lookData, 1) == "<")
                {
                    var LookText = GetParameter(lookLine, ctx);
                    Print(LookText, ctx);
                }
                else
                {
                    ExecuteScript(lookData, ctx);
                }
            }
        }
    }

    private void ExecSpeak(string cmd, Context ctx)
    {
        if (BeginsWith(cmd, "the "))
        {
            cmd = GetEverythingAfter(cmd, "the ");
        }

        var name = cmd;

        // if the "speak" parameter of the character c$'s definition
        // is just a parameter, say it - otherwise execute it as
        // a script.

        if (ASLVersion >= 281)
        {
            var speakLine = "";

            var ObjID = Disambiguate(name, "inventory;" + _currentRoom, ctx);
            if (ObjID <= 0)
            {
                if (ObjID != -2)
                {
                    PlayerErrorMessage(PlayerError.BadThing, ctx);
                }

                _badCmdBefore = "speak to";
                return;
            }

            var foundSpeak = false;

            // First look for action, then look
            // for property, then check define
            // section:

            var o = _objs[ObjID];

            for (int i = 1, loopTo = o.NumberActions; i <= loopTo; i++)
            {
                if (o.Actions[i].ActionName == "speak")
                {
                    speakLine = "speak " + o.Actions[i].Script;
                    foundSpeak = true;
                    break;
                }
            }

            if (!foundSpeak)
            {
                for (int i = 1, loopTo1 = o.NumberProperties; i <= loopTo1; i++)
                {
                    if (o.Properties[i].PropertyName == "speak")
                    {
                        speakLine = "speak <" + o.Properties[i].PropertyValue + ">";
                        foundSpeak = true;
                        break;
                    }
                }
            }

            // For some reason ASL3 < 311 looks for a "look" tag rather than
            // having had this set up at initialisation.
            if ((ASLVersion < 311) & !foundSpeak)
            {
                for (int i = o.DefinitionSectionStart, loopTo2 = o.DefinitionSectionEnd; i <= loopTo2; i++)
                {
                    if (BeginsWith(_lines[i], "speak "))
                    {
                        speakLine = _lines[i];
                        foundSpeak = true;
                    }
                }
            }

            if (!foundSpeak)
            {
                SetStringContents("quest.error.gender",
                    Strings.UCase(Strings.Left(_objs[ObjID].Gender, 1)) + Strings.Mid(_objs[ObjID].Gender, 2), ctx);
                PlayerErrorMessage(PlayerError.DefaultSpeak, ctx);
                return;
            }

            speakLine = GetEverythingAfter(speakLine, "speak ");

            if (BeginsWith(speakLine, "<"))
            {
                var text = GetParameter(speakLine, ctx);
                if (ASLVersion >= 350)
                {
                    Print(text, ctx);
                }
                else
                {
                    Print('"' + text + '"', ctx);
                }
            }
            else
            {
                ExecuteScript(speakLine, ctx, ObjID);
            }
        }

        else
        {
            var line = RetrLine("character", cmd, "speak", ctx);
            var type = Thing.Character;

            var data = Strings.Trim(GetEverythingAfter(Strings.Trim(line), "speak "));

            if ((line != "<unfound>") & (line != "<undefined>"))
            {
                // Character exists; but is it available??
                if (IsAvailable(cmd + "@" + _currentRoom, type, ctx) == false)
                {
                    line = "<undefined>";
                }
            }

            if (line == "<undefined>")
            {
                PlayerErrorMessage(PlayerError.BadCharacter, ctx);
            }
            else if (line == "<unfound>")
            {
                SetStringContents("quest.error.gender", Strings.Trim(GetGender(cmd, true, ctx)), ctx);
                SetStringContents("quest.error.charactername", cmd, ctx);
                PlayerErrorMessage(PlayerError.DefaultSpeak, ctx);
            }
            else if (BeginsWith(data, "<"))
            {
                data = GetParameter(line, ctx);
                Print('"' + data + '"', ctx);
            }
            else
            {
                ExecuteScript(data, ctx);
            }
        }
    }

    private void ExecTake(string item, Context ctx)
    {
        var parentID = default(int);
        string parentDisplayName;
        var foundItem = true;
        var foundTake = false;
        var id = Disambiguate(item, _currentRoom, ctx);

        if (id < 0)
        {
            foundItem = false;
        }
        else
        {
            foundItem = true;
        }

        if (!foundItem)
        {
            if (id != -2)
            {
                if (ASLVersion >= 410)
                {
                    id = Disambiguate(item, "inventory", ctx);
                    if (id >= 0)
                    {
                        // Player already has this item
                        PlayerErrorMessage(PlayerError.AlreadyTaken, ctx);
                    }
                    else
                    {
                        PlayerErrorMessage(PlayerError.BadThing, ctx);
                    }
                }
                else if (ASLVersion >= 391)
                {
                    PlayerErrorMessage(PlayerError.BadThing, ctx);
                }
                else
                {
                    PlayerErrorMessage(PlayerError.BadItem, ctx);
                }
            }

            _badCmdBefore = "take";
            return;
        }

        SetStringContents("quest.error.article", _objs[id].Article, ctx);

        var isInContainer = false;

        if (ASLVersion >= 391)
        {
            var canAccessObject = PlayerCanAccessObject(id);
            if (!canAccessObject.CanAccessObject)
            {
                PlayerErrorMessage_ExtendInfo(PlayerError.BadTake, ctx, canAccessObject.ErrorMsg);
                return;
            }

            var parent = GetObjectProperty("parent", id, false, false);
            parentID = GetObjectIdNoAlias(parent);
        }

        if (ASLVersion >= 280)
        {
            var t = _objs[id].Take;

            if (isInContainer & ((t.Type == TextActionType.Default) | (t.Type == TextActionType.Text)))
            {
                // So, we want to take an object that's in a container or surface. So first
                // we have to remove the object from that container.

                if (!string.IsNullOrEmpty(_objs[parentID].ObjectAlias))
                {
                    parentDisplayName = _objs[parentID].ObjectAlias;
                }
                else
                {
                    parentDisplayName = _objs[parentID].ObjectName;
                }

                Print("(first removing " + _objs[id].Article + " from " + parentDisplayName + ")", ctx);

                // Try to remove the object
                ctx.AllowRealNamesInCommand = true;
                ExecCommand("remove " + _objs[id].ObjectName, ctx, false, dontSetIt: true);

                if (!string.IsNullOrEmpty(GetObjectProperty("parent", id, false, false)))
                {
                    // removing the object failed
                    return;
                }
            }

            if (t.Type == TextActionType.Default)
            {
                PlayerErrorMessage(PlayerError.DefaultTake, ctx);
                PlayerItem(item, true, ctx, id);
            }
            else if (t.Type == TextActionType.Text)
            {
                Print(t.Data, ctx);
                PlayerItem(item, true, ctx, id);
            }
            else if (t.Type == TextActionType.Script)
            {
                ExecuteScript(t.Data, ctx, id);
            }
            else
            {
                PlayerErrorMessage(PlayerError.BadTake, ctx);
            }
        }
        else
        {
            // find 'take' line
            for (int i = _objs[id].DefinitionSectionStart + 1, loopTo = _objs[id].DefinitionSectionEnd - 1;
                 i <= loopTo;
                 i++)
            {
                if (BeginsWith(_lines[i], "take"))
                {
                    var script = Strings.Trim(GetEverythingAfter(Strings.Trim(_lines[i]), "take"));
                    ExecuteScript(script, ctx, id);
                    foundTake = true;
                    i = _objs[id].DefinitionSectionEnd;
                }
            }

            if (!foundTake)
            {
                PlayerErrorMessage(PlayerError.BadTake, ctx);
            }
        }
    }

    private void ExecUse(string useLine, Context ctx)
    {
        int endOnWith;
        var useDeclareLine = "";
        string useOn, useItem;

        useLine = Strings.Trim(GetEverythingAfter(useLine, "use "));

        int roomId;
        roomId = GetRoomID(_currentRoom, ctx);

        var onWithPos = Strings.InStr(useLine, " on ");
        if (onWithPos == 0)
        {
            onWithPos = Strings.InStr(useLine, " with ");
            endOnWith = onWithPos + 4;
        }
        else
        {
            endOnWith = onWithPos + 2;
        }

        if (onWithPos != 0)
        {
            useOn = Strings.Trim(Strings.Right(useLine, Strings.Len(useLine) - endOnWith));
            useItem = Strings.Trim(Strings.Left(useLine, onWithPos - 1));
        }
        else
        {
            useOn = "";
            useItem = useLine;
        }

        // see if player has this item:

        var id = default(int);
        bool notGotItem;
        if (ASLVersion >= 280)
        {
            var foundItem = false;

            id = Disambiguate(useItem, "inventory", ctx);
            if (id > 0)
            {
                foundItem = true;
            }

            if (!foundItem)
            {
                if (id != -2)
                {
                    PlayerErrorMessage(PlayerError.NoItem, ctx);
                }

                if (string.IsNullOrEmpty(useOn))
                {
                    _badCmdBefore = "use";
                }
                else
                {
                    _badCmdBefore = "use";
                    _badCmdAfter = "on " + useOn;
                }

                return;
            }
        }
        else
        {
            notGotItem = true;

            for (int i = 1, loopTo = _numberItems; i <= loopTo; i++)
            {
                if ((Strings.LCase(_items[i].Name) ?? "") == (Strings.LCase(useItem) ?? ""))
                {
                    if (_items[i].Got == false)
                    {
                        notGotItem = true;
                        i = _numberItems;
                    }
                    else
                    {
                        notGotItem = false;
                    }
                }
            }

            if (notGotItem)
            {
                PlayerErrorMessage(PlayerError.NoItem, ctx);
                return;
            }
        }

        var useScript = "";
        bool foundUseScript;
        bool foundUseOnObject;
        int useOnObjectId;
        bool found;
        if (ASLVersion >= 280)
        {
            foundUseScript = false;

            if (string.IsNullOrEmpty(useOn))
            {
                if (ASLVersion < 410)
                {
                    var r = _rooms[roomId];
                    for (int i = 1, loopTo1 = r.NumberUse; i <= loopTo1; i++)
                    {
                        if ((Strings.LCase(_objs[id].ObjectName) ?? "") == (Strings.LCase(r.Use[i].Text) ?? ""))
                        {
                            foundUseScript = true;
                            useScript = r.Use[i].Script;
                            break;
                        }
                    }
                }

                if (!foundUseScript)
                {
                    useScript = _objs[id].Use;
                    if (!string.IsNullOrEmpty(useScript))
                    {
                        foundUseScript = true;
                    }
                }
            }
            else
            {
                foundUseOnObject = false;

                useOnObjectId = Disambiguate(useOn, _currentRoom, ctx);
                if (useOnObjectId > 0)
                {
                    foundUseOnObject = true;
                }
                else
                {
                    useOnObjectId = Disambiguate(useOn, "inventory", ctx);
                    if (useOnObjectId > 0)
                    {
                        foundUseOnObject = true;
                    }
                }

                if (!foundUseOnObject)
                {
                    if (useOnObjectId != -2)
                    {
                        PlayerErrorMessage(PlayerError.BadThing, ctx);
                    }

                    _badCmdBefore = "use " + useItem + " on";
                    return;
                }

                // now, for "use a on b", we have
                // ItemID=a and UseOnObjectID=b

                // first check b for use <a>:

                var o = _objs[useOnObjectId];

                for (int i = 1, loopTo2 = o.NumberUseData; i <= loopTo2; i++)
                {
                    if ((o.UseData[i].UseType == UseType.UseSomethingOn) &
                        ((Strings.LCase(o.UseData[i].UseObject) ?? "") == (Strings.LCase(_objs[id].ObjectName) ?? "")))
                    {
                        foundUseScript = true;
                        useScript = o.UseData[i].UseScript;
                        break;
                    }
                }

                if (!foundUseScript)
                {
                    // check a for use on <b>:
                    var u = _objs[id];
                    for (int i = 1, loopTo3 = u.NumberUseData; i <= loopTo3; i++)
                    {
                        if ((u.UseData[i].UseType == UseType.UseOnSomething) &
                            ((Strings.LCase(u.UseData[i].UseObject) ?? "") ==
                             (Strings.LCase(_objs[useOnObjectId].ObjectName) ?? "")))
                        {
                            foundUseScript = true;
                            useScript = u.UseData[i].UseScript;
                            break;
                        }
                    }
                }

                if (!foundUseScript)
                {
                    // check b for use anything:
                    useScript = _objs[useOnObjectId].UseAnything;
                    if (!string.IsNullOrEmpty(useScript))
                    {
                        foundUseScript = true;
                        SetStringContents("quest.use.object.name", _objs[id].ObjectName, ctx);
                    }
                }

                if (!foundUseScript)
                {
                    // check a for use on anything:
                    useScript = _objs[id].UseOnAnything;
                    if (!string.IsNullOrEmpty(useScript))
                    {
                        foundUseScript = true;
                        SetStringContents("quest.use.object.name", _objs[useOnObjectId].ObjectName, ctx);
                    }
                }
            }

            if (foundUseScript)
            {
                ExecuteScript(useScript, ctx, id);
            }
            else
            {
                PlayerErrorMessage(PlayerError.DefaultUse, ctx);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(useOn))
            {
                useDeclareLine = RetrLineParam("object", useOn, "use", useItem, ctx);
            }
            else
            {
                found = false;
                for (int i = 1, loopTo4 = _rooms[roomId].NumberUse; i <= loopTo4; i++)
                {
                    if ((Strings.LCase(_rooms[roomId].Use[i].Text) ?? "") == (Strings.LCase(useItem) ?? ""))
                    {
                        useDeclareLine = "use <> " + _rooms[roomId].Use[i].Script;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    useDeclareLine = FindLine(GetDefineBlock("game"), "use", useItem);
                }

                if (!found & string.IsNullOrEmpty(useDeclareLine))
                {
                    PlayerErrorMessage(PlayerError.DefaultUse, ctx);
                    return;
                }
            }

            if ((useDeclareLine != "<unfound>") & (useDeclareLine != "<undefined>") & !string.IsNullOrEmpty(useOn))
            {
                // Check for object availablity
                if (IsAvailable(useOn, Thing.Object, ctx) == false)
                {
                    useDeclareLine = "<undefined>";
                }
            }

            if (useDeclareLine == "<undefined>")
            {
                useDeclareLine = RetrLineParam("character", useOn, "use", useItem, ctx);

                if (useDeclareLine != "<undefined>")
                {
                    // Check for character availability
                    if (IsAvailable(useOn, Thing.Character, ctx) == false)
                    {
                        useDeclareLine = "<undefined>";
                    }
                }

                if (useDeclareLine == "<undefined>")
                {
                    PlayerErrorMessage(PlayerError.BadThing, ctx);
                    return;
                }

                if (useDeclareLine == "<unfound>")
                {
                    PlayerErrorMessage(PlayerError.DefaultUse, ctx);
                    return;
                }
            }
            else if (useDeclareLine == "<unfound>")
            {
                PlayerErrorMessage(PlayerError.DefaultUse, ctx);
                return;
            }

            var script = Strings.Right(useDeclareLine,
                Strings.Len(useDeclareLine) - Strings.InStr(useDeclareLine, ">"));
            ExecuteScript(script, ctx);
        }
    }

    private void ObjectActionUpdate(int id, string name, string script, bool noUpdate = false)
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
                AddToUseInfo(id, script);
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

                    AddToUseInfo(id, Strings.Trim(Strings.Left(name, sp - 1)) + " <" + objectName + "> " + script);
                }
                else
                {
                    AddToUseInfo(id, name + " " + script);
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

                    AddToGiveInfo(id, Strings.Trim(Strings.Left(name, sp - 1)) + " <" + objectName + "> " + script);
                }
                else
                {
                    AddToGiveInfo(id, name + " " + script);
                }
            }
        }

        if (_gameFullyLoaded)
        {
            AddToObjectChangeLog(ChangeLog.AppliesTo.Object, _objs[id].ObjectName, name,
                "action <" + name + "> " + script);
        }
    }

    private void ExecuteIf(string scriptLine, Context ctx)
    {
        var ifLine = Strings.Trim(GetEverythingAfter(Strings.Trim(scriptLine), "if "));
        var obscuredLine = ObliterateParameters(ifLine);
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

        if (ExecuteConditions(conditions, ctx))
        {
            ExecuteScript(thenScript, ctx);
        }
        else if (elsePos != 0)
        {
            ExecuteScript(elseScript, ctx);
        }
    }

    private void ExecuteScript(string scriptLine, Context ctx, int newCallingObjectId = 0)
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
                        ExecuteScript(curScriptLine, ctx);
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
                ExecuteIf(scriptLine, ctx);
            }
            else if (BeginsWith(scriptLine, "select case "))
            {
                ExecuteSelectCase(scriptLine, ctx);
            }
            else if (BeginsWith(scriptLine, "choose "))
            {
                ExecuteChoose(GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "set "))
            {
                ExecuteSet(GetEverythingAfter(scriptLine, "set "), ctx);
            }
            else if (BeginsWith(scriptLine, "inc ") | BeginsWith(scriptLine, "dec "))
            {
                ExecuteIncDec(scriptLine, ctx);
            }
            else if (BeginsWith(scriptLine, "say "))
            {
                Print('"' + GetParameter(scriptLine, ctx) + '"', ctx);
            }
            else if (BeginsWith(scriptLine, "do "))
            {
                ExecuteDo(GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "doaction "))
            {
                ExecuteDoAction(GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "give "))
            {
                PlayerItem(GetParameter(scriptLine, ctx), true, ctx);
            }
            else if (BeginsWith(scriptLine, "lose ") | BeginsWith(scriptLine, "drop "))
            {
                PlayerItem(GetParameter(scriptLine, ctx), false, ctx);
            }
            else if (BeginsWith(scriptLine, "msg "))
            {
                Print(GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "speak "))
            {
                Speak(GetParameter(scriptLine, ctx));
            }
            else if (BeginsWith(scriptLine, "helpmsg "))
            {
                Print(GetParameter(scriptLine, ctx), ctx);
            }
            else if (Strings.Trim(Strings.LCase(scriptLine)) == "helpclose")
            {
            }
            // This command does nothing in the Quest 5 player, as there is no separate help window
            else if (BeginsWith(scriptLine, "goto "))
            {
                PlayGame(GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "playerwin"))
            {
                FinishGame(StopType.Win, ctx);
            }
            else if (BeginsWith(scriptLine, "playerlose"))
            {
                FinishGame(StopType.Lose, ctx);
            }
            else if (Strings.Trim(Strings.LCase(scriptLine)) == "stop")
            {
                FinishGame(StopType.Null, ctx);
            }
            else if (BeginsWith(scriptLine, "playwav "))
            {
                PlayWav(GetParameter(scriptLine, ctx));
            }
            else if (BeginsWith(scriptLine, "playmidi "))
            {
                PlayMedia(GetParameter(scriptLine, ctx));
            }
            else if (BeginsWith(scriptLine, "playmp3 "))
            {
                PlayMedia(GetParameter(scriptLine, ctx));
            }
            else if (Strings.Trim(Strings.LCase(scriptLine)) == "picture close")
            {
            }
            // This command does nothing in the Quest 5 player, as there is no separate picture window
            else if (((ASLVersion >= 390) & BeginsWith(scriptLine, "picture popup ")) |
                     ((ASLVersion >= 282) & (ASLVersion < 390) & BeginsWith(scriptLine, "picture ")) |
                     ((ASLVersion < 282) & BeginsWith(scriptLine, "show ")))
            {
                ShowPicture(GetParameter(scriptLine, ctx));
            }
            else if ((ASLVersion >= 390) & BeginsWith(scriptLine, "picture "))
            {
                ShowPictureInText(GetParameter(scriptLine, ctx));
            }
            else if (BeginsWith(scriptLine, "animate persist "))
            {
                ShowPicture(GetParameter(scriptLine, ctx));
            }
            else if (BeginsWith(scriptLine, "animate "))
            {
                ShowPicture(GetParameter(scriptLine, ctx));
            }
            else if (BeginsWith(scriptLine, "extract "))
            {
                ExtractFile(GetParameter(scriptLine, ctx));
            }
            else if ((ASLVersion < 281) & BeginsWith(scriptLine, "hideobject "))
            {
                SetAvailability(GetParameter(scriptLine, ctx), false, ctx);
            }
            else if ((ASLVersion < 281) & BeginsWith(scriptLine, "showobject "))
            {
                SetAvailability(GetParameter(scriptLine, ctx), true, ctx);
            }
            else if ((ASLVersion < 281) & BeginsWith(scriptLine, "moveobject "))
            {
                ExecMoveThing(GetParameter(scriptLine, ctx), Thing.Object, ctx);
            }
            else if ((ASLVersion < 281) & BeginsWith(scriptLine, "hidechar "))
            {
                SetAvailability(GetParameter(scriptLine, ctx), false, ctx, Thing.Character);
            }
            else if ((ASLVersion < 281) & BeginsWith(scriptLine, "showchar "))
            {
                SetAvailability(GetParameter(scriptLine, ctx), true, ctx, Thing.Character);
            }
            else if ((ASLVersion < 281) & BeginsWith(scriptLine, "movechar "))
            {
                ExecMoveThing(GetParameter(scriptLine, ctx), Thing.Character, ctx);
            }
            else if ((ASLVersion < 281) & BeginsWith(scriptLine, "revealobject "))
            {
                SetVisibility(GetParameter(scriptLine, ctx), Thing.Object, true, ctx);
            }
            else if ((ASLVersion < 281) & BeginsWith(scriptLine, "concealobject "))
            {
                SetVisibility(GetParameter(scriptLine, ctx), Thing.Object, false, ctx);
            }
            else if ((ASLVersion < 281) & BeginsWith(scriptLine, "revealchar "))
            {
                SetVisibility(GetParameter(scriptLine, ctx), Thing.Character, true, ctx);
            }
            else if ((ASLVersion < 281) & BeginsWith(scriptLine, "concealchar "))
            {
                SetVisibility(GetParameter(scriptLine, ctx), Thing.Character, false, ctx);
            }
            else if ((ASLVersion >= 281) & BeginsWith(scriptLine, "hide "))
            {
                SetAvailability(GetParameter(scriptLine, ctx), false, ctx);
            }
            else if ((ASLVersion >= 281) & BeginsWith(scriptLine, "show "))
            {
                SetAvailability(GetParameter(scriptLine, ctx), true, ctx);
            }
            else if ((ASLVersion >= 281) & BeginsWith(scriptLine, "move "))
            {
                ExecMoveThing(GetParameter(scriptLine, ctx), Thing.Object, ctx);
            }
            else if ((ASLVersion >= 281) & BeginsWith(scriptLine, "reveal "))
            {
                SetVisibility(GetParameter(scriptLine, ctx), Thing.Object, true, ctx);
            }
            else if ((ASLVersion >= 281) & BeginsWith(scriptLine, "conceal "))
            {
                SetVisibility(GetParameter(scriptLine, ctx), Thing.Object, false, ctx);
            }
            else if ((ASLVersion >= 391) & BeginsWith(scriptLine, "open "))
            {
                SetOpenClose(GetParameter(scriptLine, ctx), true, ctx);
            }
            else if ((ASLVersion >= 391) & BeginsWith(scriptLine, "close "))
            {
                SetOpenClose(GetParameter(scriptLine, ctx), false, ctx);
            }
            else if ((ASLVersion >= 391) & BeginsWith(scriptLine, "add "))
            {
                ExecAddRemoveScript(GetParameter(scriptLine, ctx), true, ctx);
            }
            else if ((ASLVersion >= 391) & BeginsWith(scriptLine, "remove "))
            {
                ExecAddRemoveScript(GetParameter(scriptLine, ctx), false, ctx);
            }
            else if (BeginsWith(scriptLine, "clone "))
            {
                ExecClone(GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "exec "))
            {
                ExecExec(scriptLine, ctx);
            }
            else if (BeginsWith(scriptLine, "setstring "))
            {
                ExecSetString(GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "setvar "))
            {
                ExecSetVar(GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "for "))
            {
                ExecFor(GetEverythingAfter(scriptLine, "for "), ctx);
            }
            else if (BeginsWith(scriptLine, "property "))
            {
                ExecProperty(GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "type "))
            {
                ExecType(GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "action "))
            {
                ExecuteAction(GetEverythingAfter(scriptLine, "action "), ctx);
            }
            else if (BeginsWith(scriptLine, "flag "))
            {
                ExecuteFlag(GetEverythingAfter(scriptLine, "flag "), ctx);
            }
            else if (BeginsWith(scriptLine, "create "))
            {
                ExecuteCreate(GetEverythingAfter(scriptLine, "create "), ctx);
            }
            else if (BeginsWith(scriptLine, "destroy exit "))
            {
                DestroyExit(GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "repeat "))
            {
                ExecuteRepeat(GetEverythingAfter(scriptLine, "repeat "), ctx);
            }
            else if (BeginsWith(scriptLine, "enter "))
            {
                ExecuteEnter(scriptLine, ctx);
            }
            else if (BeginsWith(scriptLine, "displaytext "))
            {
                DisplayTextSection(GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "helpdisplaytext "))
            {
                DisplayTextSection(GetParameter(scriptLine, ctx), ctx);
            }
            else if (BeginsWith(scriptLine, "font "))
            {
                SetFont(GetParameter(scriptLine, ctx));
            }
            else if (BeginsWith(scriptLine, "pause "))
            {
                Pause(Conversions.ToInteger(GetParameter(scriptLine, ctx)));
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
                SetBackground(GetParameter(scriptLine, ctx));
            }
            else if (BeginsWith(scriptLine, "foreground "))
            {
                SetForeground(GetParameter(scriptLine, ctx));
            }
            else if (Strings.Trim(Strings.LCase(scriptLine)) == "nointro")
            {
                _autoIntro = false;
            }
            else if (BeginsWith(scriptLine, "debug "))
            {
                LogASLError(GetParameter(scriptLine, ctx));
            }
            else if (BeginsWith(scriptLine, "mailto "))
            {
                var emailAddress = GetParameter(scriptLine, ctx);
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
                ExecuteWait(Strings.Trim(GetEverythingAfter(Strings.Trim(scriptLine), "wait")), ctx);
            }
            else if (BeginsWith(scriptLine, "timeron "))
            {
                SetTimerState(GetParameter(scriptLine, ctx), true);
            }
            else if (BeginsWith(scriptLine, "timeroff "))
            {
                SetTimerState(GetParameter(scriptLine, ctx), false);
            }
            else if (Strings.Trim(Strings.LCase(scriptLine)) == "outputon")
            {
                _outPutOn = true;
                UpdateObjectList(ctx);
                UpdateItems(ctx);
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
                ExecuteLock(GetParameter(scriptLine, ctx), true);
            }
            else if (BeginsWith(scriptLine, "unlock "))
            {
                ExecuteLock(GetParameter(scriptLine, ctx), false);
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
                ctx.FunctionReturnValue = GetParameter(scriptLine, ctx);
            }
            else if (BeginsWith(scriptLine, "'") == false)
            {
                LogASLError("Unrecognized keyword. Line reads: '" + Strings.Trim(ReportErrorLine(scriptLine)) + "'",
                    LogType.WarningError);
            }
        }
        catch
        {
            Print("[An internal error occurred]", ctx);
            LogASLError(
                Information.Err().Number + " - '" + Information.Err().Description +
                "' occurred processing script line '" + scriptLine + "'", LogType.InternalError);
        }
    }

    private void ExecuteEnter(string scriptLine, Context ctx)
    {
        _commandOverrideModeOn = true;
        _commandOverrideVariable = GetParameter(scriptLine, ctx);

        // Now, wait for CommandOverrideModeOn to be set
        // to False by ExecCommand. Execution can then resume.

        ChangeState(State.Waiting, true);

        lock (_commandLock)
        {
            Monitor.Wait(_commandLock);
        }

        _commandOverrideModeOn = false;

        // State will have been changed to Working when the user typed their response,
        // and will be set back to Ready when the call to ExecCommand has finished
    }

    private void ExecuteSet(string setInstruction, Context ctx)
    {
        if (ASLVersion >= 280)
        {
            if (BeginsWith(setInstruction, "interval "))
            {
                var interval = GetParameter(setInstruction, ctx);
                var scp = Strings.InStr(interval, ";");
                if (scp == 0)
                {
                    LogASLError("Too few parameters in 'set " + setInstruction + "'", LogType.WarningError);
                    return;
                }

                var name = Strings.Trim(Strings.Left(interval, scp - 1));
                interval = Conversion.Val(Strings.Trim(Strings.Mid(interval, scp + 1))).ToString();
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
                ExecSetString(GetParameter(setInstruction, ctx), ctx);
            }
            else if (BeginsWith(setInstruction, "numeric "))
            {
                ExecSetVar(GetParameter(setInstruction, ctx), ctx);
            }
            else if (BeginsWith(setInstruction, "collectable "))
            {
                ExecuteSetCollectable(GetParameter(setInstruction, ctx), ctx);
            }
            else
            {
                var result = SetUnknownVariableType(GetParameter(setInstruction, ctx), ctx);
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
            ExecuteSetCollectable(GetParameter(setInstruction, ctx), ctx);
        }
    }

    private string FindStatement(DefineBlock block, string statement)
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
                return GetParameter(_lines[i], _nullContext);
            }
        }

        return "";
    }

    private string FindLine(DefineBlock block, string statement, string statementParam)
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
                if ((Strings.UCase(Strings.Trim(GetParameter(_lines[i], _nullContext))) ?? "") ==
                    (Strings.UCase(Strings.Trim(statementParam)) ?? ""))
                {
                    return Strings.Trim(_lines[i]);
                }
            }
        }

        return "";
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

    private string GetSecondChunk(string line)
    {
        var endOfFirstBit = Strings.InStr(line, ">") + 1;
        var lengthOfKeyword = Strings.Len(line) - endOfFirstBit + 1;
        return Strings.Trim(Strings.Mid(line, endOfFirstBit, lengthOfKeyword));
    }

    private void GoDirection(string direction, Context ctx)
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
            _rooms[id].Exits.ExecuteGo(direction, ref ctx);
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
            ExecuteScript(dirData.Data, ctx);
        }
        else if (!string.IsNullOrEmpty(dirData.Data))
        {
            var newRoom = dirData.Data;
            var scp = Strings.InStr(newRoom, ";");
            if (scp != 0)
            {
                newRoom = Strings.Trim(Strings.Mid(newRoom, scp + 1));
            }

            PlayGame(newRoom, ctx);
        }
        else if (direction == "out")
        {
            PlayerErrorMessage(PlayerError.DefaultOut, ctx);
        }
        else
        {
            PlayerErrorMessage(PlayerError.BadPlace, ctx);
        }
    }

    private void GoToPlace(string place, Context ctx)
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
                ExecuteScript(s, ctx);
            }
            else
            {
                PlayGame(destination, ctx);
            }
        }

        if (disallowed)
        {
            PlayerErrorMessage(PlayerError.BadPlace, ctx);
        }
    }

    private async Task<bool> InitialiseGame(IGameData gameData, bool fromQsg = false)
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
        if (await ParseFile(gameData) == false)
        {
            LogASLError("Unable to open file", LogType.Init);
            var err = "Unable to open " + gameData.Filename;

            if (!string.IsNullOrEmpty(_openErrorReport))
            {
                // Strip last vbcrlf
                _openErrorReport = Strings.Left(_openErrorReport, Strings.Len(_openErrorReport) - 2);
                err = err + ":" + Constants.vbCrLf + Constants.vbCrLf + _openErrorReport;
            }

            Print("Error: " + err, _nullContext);
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
                aslVersion = GetParameter(_lines[i], _nullContext);
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
        _gameName = GetParameter(_lines[GetDefineBlock("game").StartLine], _nullContext);

        _player.UpdateGameName(_gameName);
        _player.Show("Panes");
        _player.Show("Location");
        _player.Show("Command");

        SetUpGameObject();
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

        SetUpSynonyms();
        SetUpRoomData();

        if (ASLVersion >= 410)
        {
            SetUpExits();
        }

        if (ASLVersion < 280)
        {
            // Set up an array containing the names of all the items
            // used in the game, based on the possitems statement
            // of the 'define game' block.
            SetUpItemArrays();
        }

        if (ASLVersion < 280)
        {
            // Now, go through the 'startitems' statement and set up
            // the items array so we start with those items mentioned.
            SetUpStartItems();
        }

        // Set up collectables.
        SetUpCollectables();

        SetUpDisplayVariables();

        // Set up characters and objects.
        SetUpCharObjectInfo();
        SetUpUserDefinedPlayerErrors();
        SetUpDefaultFonts();
        SetUpTurnScript();
        SetUpTimers();
        SetUpMenus();

        _gameFileName = gameData.Filename;

        LogASLError("Finished loading file.", LogType.Init);

        _defaultRoomProperties = GetPropertiesInType("defaultroom", false);
        _defaultProperties = GetPropertiesInType("default", false);

        return true;
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

    private void PlayerItem(string item, bool got, Context ctx, int objId = 0)
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
                        AddToObjectProperties("not parent", objId, ctx);
                    }

                    MoveThing(_objs[objId].ObjectName, "inventory", Thing.Object, ctx);

                    if (!string.IsNullOrEmpty(_objs[objId].GainScript))
                    {
                        ExecuteScript(_objs[objId].GainScript, ctx);
                    }
                }
                else
                {
                    MoveThing(_objs[objId].ObjectName, _currentRoom, Thing.Object, ctx);

                    if (!string.IsNullOrEmpty(_objs[objId].LoseScript))
                    {
                        ExecuteScript(_objs[objId].LoseScript, ctx);
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
                UpdateItems(ctx);
                UpdateObjectList(ctx);
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

            UpdateItems(ctx);
        }
    }

    internal void PlayGame(string room, Context ctx)
    {
        // plays the specified room

        var id = GetRoomID(room, ctx);

        if (id == 0)
        {
            LogASLError("No such room '" + room + "'", LogType.WarningError);
            return;
        }

        _currentRoom = room;

        SetStringContents("quest.currentroom", room, ctx);

        if ((ASLVersion >= 391) & (ASLVersion < 410))
        {
            AddToObjectProperties("visited", _rooms[id].ObjId, ctx);
        }

        ShowRoomInfo(room, ctx);
        UpdateItems(ctx);

        // Find script lines and execute them.

        if (!string.IsNullOrEmpty(_rooms[id].Script))
        {
            var script = _rooms[id].Script;
            ExecuteScript(script, ctx);
        }

        if (ASLVersion >= 410)
        {
            AddToObjectProperties("visited", _rooms[id].ObjId, ctx);
        }
    }

    internal void Print(string txt, Context ctx)
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
                    ExecuteScript("wait <>", ctx);
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
                            ExecuteScript("clear", ctx);
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

    private string RetrLineParam(string blockType, string param, string line, string lineParam, Context ctx)
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
                (Strings.LCase(GetParameter(_lines[i], ctx)) ?? "") == (Strings.LCase(lineParam) ?? ""))
            {
                return Strings.Trim(_lines[i]);
            }
        }

        return "<unfound>";
    }

    private void SetUpCollectables()
    {
        var lastItem = false;

        _numCollectables = 0;

        // Initialise collectables:
        // First, find the collectables section within the define
        // game block, and get its parameters:

        for (int a = GetDefineBlock("game").StartLine + 1, loopTo = GetDefineBlock("game").EndLine - 1;
             a <= loopTo;
             a++)
        {
            if (BeginsWith(_lines[a], "collectables "))
            {
                var collectables = Strings.Trim(GetParameter(_lines[a], _nullContext, false));

                // if collectables is a null string, there are no
                // collectables. Otherwise, there is one more object than
                // the number of commas. So, first check to see if we have
                // no objects:

                if (!string.IsNullOrEmpty(collectables))
                {
                    _numCollectables = 1;
                    var pos = 1;
                    do
                    {
                        Array.Resize(ref _collectables, _numCollectables + 1);
                        _collectables[_numCollectables] = new Collectable();
                        var nextComma = Strings.InStr(pos + 1, collectables, ",");
                        if (nextComma == 0)
                        {
                            nextComma = Strings.InStr(pos + 1, collectables, ";");
                        }

                        // If there are no more commas, we want everything
                        // up to the end of the string, and then to exit
                        // the loop:
                        if (nextComma == 0)
                        {
                            nextComma = Strings.Len(collectables) + 1;
                            lastItem = true;
                        }

                        // Get item info
                        var info = Strings.Trim(Strings.Mid(collectables, pos, nextComma - pos));
                        _collectables[_numCollectables].Name =
                            Strings.Trim(Strings.Left(info, Strings.InStr(info, " ")));

                        var ep = Strings.InStr(info, "=");
                        var sp1 = Strings.InStr(info, " ");
                        var sp2 = Strings.InStr(ep, info, " ");
                        if (sp2 == 0)
                        {
                            sp2 = Strings.Len(info) + 1;
                        }

                        var t = Strings.Trim(Strings.Mid(info, sp1 + 1, ep - sp1 - 1));
                        var i = Strings.Trim(Strings.Mid(info, ep + 1, sp2 - ep - 1));

                        if (Strings.Left(t, 1) == "d")
                        {
                            t = Strings.Mid(t, 2);
                            _collectables[_numCollectables].DisplayWhenZero = false;
                        }
                        else
                        {
                            _collectables[_numCollectables].DisplayWhenZero = true;
                        }

                        _collectables[_numCollectables].Type = t;
                        _collectables[_numCollectables].Value = Conversion.Val(i);

                        // Get display string between square brackets
                        var obp = Strings.InStr(info, "[");
                        var cbp = Strings.InStr(info, "]");
                        if (obp == 0)
                        {
                            _collectables[_numCollectables].Display = "<def>";
                        }
                        else
                        {
                            var b = Strings.Mid(info, obp + 1, cbp - 1 - obp);
                            _collectables[_numCollectables].Display = Strings.Trim(b);
                        }

                        pos = nextComma + 1;
                        _numCollectables = _numCollectables + 1;
                    }

                    // lastitem set when nextcomma=0, above.
                    while (lastItem != true);

                    _numCollectables = _numCollectables - 1;
                }
            }
        }
    }

    private void SetUpItemArrays()
    {
        var lastItem = false;

        _numberItems = 0;

        // Initialise items:
        // First, find the possitems section within the define game
        // block, and get its parameters:
        for (int a = GetDefineBlock("game").StartLine + 1, loopTo = GetDefineBlock("game").EndLine - 1;
             a <= loopTo;
             a++)
        {
            if (BeginsWith(_lines[a], "possitems ") | BeginsWith(_lines[a], "items "))
            {
                var possItems = GetParameter(_lines[a], _nullContext);

                if (!string.IsNullOrEmpty(possItems))
                {
                    _numberItems = _numberItems + 1;
                    var pos = 1;
                    do
                    {
                        Array.Resize(ref _items, _numberItems + 1);
                        _items[_numberItems] = new ItemType();
                        var nextComma = Strings.InStr(pos + 1, possItems, ",");
                        if (nextComma == 0)
                        {
                            nextComma = Strings.InStr(pos + 1, possItems, ";");
                        }

                        // If there are no more commas, we want everything
                        // up to the end of the string, and then to exit
                        // the loop:
                        if (nextComma == 0)
                        {
                            nextComma = Strings.Len(possItems) + 1;
                            lastItem = true;
                        }

                        // Get item name
                        _items[_numberItems].Name = Strings.Trim(Strings.Mid(possItems, pos, nextComma - pos));
                        _items[_numberItems].Got = false;

                        pos = nextComma + 1;
                        _numberItems = _numberItems + 1;
                    }

                    // lastitem set when nextcomma=0, above.
                    while (lastItem != true);

                    _numberItems = _numberItems - 1;
                }
            }
        }
    }

    private void SetUpStartItems()
    {
        var lastItem = false;

        for (int a = GetDefineBlock("game").StartLine + 1, loopTo = GetDefineBlock("game").EndLine - 1;
             a <= loopTo;
             a++)
        {
            if (BeginsWith(_lines[a], "startitems "))
            {
                var startItems = GetParameter(_lines[a], _nullContext);

                if (!string.IsNullOrEmpty(startItems))
                {
                    var pos = 1;
                    do
                    {
                        var nextComma = Strings.InStr(pos + 1, startItems, ",");
                        if (nextComma == 0)
                        {
                            nextComma = Strings.InStr(pos + 1, startItems, ";");
                        }

                        // If there are no more commas, we want everything
                        // up to the end of the string, and then to exit
                        // the loop:
                        if (nextComma == 0)
                        {
                            nextComma = Strings.Len(startItems) + 1;
                            lastItem = true;
                        }

                        // Get item name
                        var name = Strings.Trim(Strings.Mid(startItems, pos, nextComma - pos));

                        // Find which item this is, and set it
                        for (int i = 1, loopTo1 = _numberItems; i <= loopTo1; i++)
                        {
                            if ((_items[i].Name ?? "") == (name ?? ""))
                            {
                                _items[i].Got = true;
                                break;
                            }
                        }

                        pos = nextComma + 1;
                    }

                    // lastitem set when nextcomma=0, above.
                    while (lastItem != true);
                }
            }
        }
    }

    private void ShowHelp(Context ctx)
    {
        // In Quest 4 and below, the help text displays in a separate window. In Quest 5, it displays
        // in the same window as the game text.
        Print("|b|cl|s14Quest Quick Help|xb|cb|s00", ctx);
        Print("", ctx);
        Print(
            "|cl|bMoving|xb|cb Press the direction buttons in the 'Compass' pane, or type |bGO NORTH|xb, |bSOUTH|xb, |bE|xb, etc. |xn",
            ctx);
        Print(
            "To go into a place, type |bGO TO ...|xb . To leave a place, type |bOUT, EXIT|xb or |bLEAVE|xb, or press the '|crOUT|cb' button.|n",
            ctx);
        Print(
            "|cl|bObjects and Characters|xb|cb Use |bTAKE ...|xb, |bGIVE ... TO ...|xb, |bTALK|xb/|bSPEAK TO ...|xb, |bUSE ... ON|xb/|bWITH ...|xb, |bLOOK AT ...|xb, etc.|n",
            ctx);
        Print("|cl|bExit Quest|xb|cb Type |bQUIT|xb to leave Quest.|n", ctx);
        Print(
            "|cl|bMisc|xb|cb Type |bABOUT|xb to get information on the current game. The next turn after referring to an object or character, you can use |bIT|xb, |bHIM|xb etc. as appropriate to refer to it/him/etc. again. If you make a mistake when typing an object's name, type |bOOPS|xb followed by your correction.|n",
            ctx);
        Print(
            "|cl|bKeyboard shortcuts|xb|cb Press the |crup arrow|cb and |crdown arrow|cb to scroll through commands you have already typed in. Press |crEsc|cb to clear the command box.|n|n",
            ctx);
        Print("Further information is available by selecting |iQuest Documentation|xi from the |iHelp|xi menu.", ctx);
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

            r.Extracted = false;
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

    private string UpdateDoorways(int roomId, Context ctx)
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
            _rooms[roomId].Exits.GetAvailableDirectionsDescription(ref roomDisplayText, ref directions);
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
                    SetStringContents("quest.doorways.out", outPlaceName, ctx);
                }
                else
                {
                    SetStringContents("quest.doorways.out", outPlaceAlias, ctx);
                }

                SetStringContents("quest.doorways.out.display", outPlaceAlias, ctx);
            }
            else
            {
                SetStringContents("quest.doorways.out", "", ctx);
                SetStringContents("quest.doorways.out.display", "", ctx);
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
                SetStringContents("quest.doorways.dirs", nsew, ctx);
            }
            else
            {
                SetStringContents("quest.doorways.dirs", "", ctx);
            }
        }

        UpdateDirButtons(directions, ctx);

        return roomDisplayText;
    }

    private void UpdateItems(Context ctx)
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
            UpdateStatusVars(ctx);
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

    private void FinishGame(StopType stopType, Context ctx)
    {
        if (stopType == StopType.Win)
        {
            DisplayTextSection("win", ctx);
        }
        else if (stopType == StopType.Lose)
        {
            DisplayTextSection("lose", ctx);
        }

        GameFinished();
    }

    private void UpdateObjectList(Context ctx)
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
        roomBlock = DefineBlockParam("room", _currentRoom);

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
                SetStringContents("quest.characters", "", ctx);
            }
            else
            {
                // chop off final comma and add full stop (.)
                charList = Strings.Left(charsViewable, Strings.Len(charsViewable) - 2);
                SetStringContents("quest.characters", charList, ctx);
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
            SetStringContents("quest.objects",
                Strings.Left(noFormatObjsViewable, Strings.Len(noFormatObjsViewable) - 2), ctx);
            SetStringContents("quest.formatobjects", objListString, ctx);
        }
        else
        {
            SetStringContents("quest.objects", "", ctx);
            SetStringContents("quest.formatobjects", "", ctx);
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

    private void UpdateStatusVars(Context ctx)
    {
        string displayData;
        var status = "";

        if (_numDisplayStrings > 0)
        {
            for (int i = 1, loopTo = _numDisplayStrings; i <= loopTo; i++)
            {
                displayData = DisplayStatusVariableInfo(i, VarType.String, ctx);

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
                displayData = DisplayStatusVariableInfo(i, VarType.Numeric, ctx);
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

    private void UpdateVisibilityInContainers(Context ctx, string onlyParent = "")
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

                        SetAvailability(_objs[i].ObjectName, true, ctx);
                    }
                    else
                    {
                        SetAvailability(_objs[i].ObjectName, false, ctx);
                    }
                }
            }
        }
    }

    private class PlayerCanAccessObjectResult
    {
        public bool CanAccessObject;
        public string ErrorMsg;
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

    private void SetUpExits()
    {
        // Exits have to be set up after all the rooms have been initialised

        for (int i = 1, loopTo = _numberSections; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[_defineBlocks[i].StartLine], "define room "))
            {
                var roomName = GetParameter(_lines[_defineBlocks[i].StartLine], _nullContext);
                var roomId = GetRoomID(roomName, _nullContext);

                for (int j = _defineBlocks[i].StartLine + 1, loopTo1 = _defineBlocks[i].EndLine - 1; j <= loopTo1; j++)
                {
                    if (BeginsWith(_lines[j], "define "))
                    {
                        // skip nested blocks
                        var nestedBlock = 1;
                        do
                        {
                            j = j + 1;
                            if (BeginsWith(_lines[j], "define "))
                            {
                                nestedBlock = nestedBlock + 1;
                            }
                            else if (Strings.Trim(_lines[j]) == "end define")
                            {
                                nestedBlock = nestedBlock - 1;
                            }
                        } while (nestedBlock != 0);
                    }

                    _rooms[roomId].Exits.AddExitFromTag(_lines[j]);
                }
            }
        }
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

    private void ExecuteLock(string tag, bool @lock)
    {
        RoomExit roomExit;

        roomExit = FindExit(tag);

        if (roomExit is null)
        {
            LogASLError("Can't find exit '" + tag + "'", LogType.WarningError);
            return;
        }

        roomExit.SetIsLocked(@lock);
    }

    public void Begin()
    {
        var runnerThread = new Thread(DoBegin);
        ChangeState(State.Working);
        runnerThread.Start();

        lock (_stateLock)
        {
            while ((_state == State.Working) & !_gameFinished)
            {
                Monitor.Wait(_stateLock);
            }
        }
    }

    private void DoBegin()
    {
        var gameBlock = GetDefineBlock("game");
        var ctx = new Context();

        SetFont("");
        SetFontSize(0d);

        for (int i = GetDefineBlock("game").StartLine + 1, loopTo = GetDefineBlock("game").EndLine - 1;
             i <= loopTo;
             i++)
        {
            if (BeginsWith(_lines[i], "background "))
            {
                SetBackground(GetParameter(_lines[i], _nullContext));
            }
        }

        for (int i = GetDefineBlock("game").StartLine + 1, loopTo1 = GetDefineBlock("game").EndLine - 1;
             i <= loopTo1;
             i++)
        {
            if (BeginsWith(_lines[i], "foreground "))
            {
                SetForeground(GetParameter(_lines[i], _nullContext));
            }
        }

        // Execute any startscript command that appears in the
        // "define game" block:

        _autoIntro = true;

        // For ASL>=391, we only run startscripts if LoadMethod is normal (i.e. we haven't started
        // from a saved QSG file)

        if ((ASLVersion < 391) | ((ASLVersion >= 391) & (_gameLoadMethod == "normal")))
        {
            // for GameASLVersion 311 and later, any library startscript is executed first:
            if (ASLVersion >= 311)
            {
                // We go through the game block executing these in reverse order, as
                // the statements which are included last should be executed first.
                for (int i = gameBlock.EndLine - 1, loopTo2 = gameBlock.StartLine + 1; i >= loopTo2; i -= 1)
                {
                    if (BeginsWith(_lines[i], "lib startscript "))
                    {
                        ctx = _nullContext;
                        ExecuteScript(Strings.Trim(GetEverythingAfter(Strings.Trim(_lines[i]), "lib startscript ")),
                            ctx);
                    }
                }
            }

            for (int i = gameBlock.StartLine + 1, loopTo3 = gameBlock.EndLine - 1; i <= loopTo3; i++)
            {
                if (BeginsWith(_lines[i], "startscript "))
                {
                    ctx = _nullContext;
                    ExecuteScript(Strings.Trim(GetEverythingAfter(Strings.Trim(_lines[i]), "startscript")), ctx);
                }
                else if (BeginsWith(_lines[i], "lib startscript ") & (ASLVersion < 311))
                {
                    ctx = _nullContext;
                    ExecuteScript(Strings.Trim(GetEverythingAfter(Strings.Trim(_lines[i]), "lib startscript ")), ctx);
                }
            }
        }

        _gameFullyLoaded = true;

        // Display intro text
        if (_autoIntro & (_gameLoadMethod == "normal"))
        {
            DisplayTextSection("intro", _nullContext);
        }

        // Start game from room specified by "start" statement
        var startRoom = "";
        for (int i = gameBlock.StartLine + 1, loopTo4 = gameBlock.EndLine - 1; i <= loopTo4; i++)
        {
            if (BeginsWith(_lines[i], "start "))
            {
                startRoom = GetParameter(_lines[i], _nullContext);
            }
        }

        if (!_loadedFromQsg)
        {
            ctx = _nullContext;
            PlayGame(startRoom, ctx);
            Print("", _nullContext);
        }
        else
        {
            UpdateItems(_nullContext);

            Print("Restored saved game", _nullContext);
            Print("", _nullContext);
            PlayGame(_currentRoom, _nullContext);
            Print("", _nullContext);

            if (ASLVersion >= 391)
            {
                // For ASL>=391, OnLoad is now run for all games.
                ctx = _nullContext;
                ExecuteScript(_onLoadScript, ctx);
            }
        }

        RaiseNextTimerTickRequest();

        ChangeState(State.Ready);
    }

    public List<string> Errors => new();

    public void Finish()
    {
        GameFinished();
    }

    public event FinishedHandler Finished;

    public event ErrorHandler LogError;

    public event PrintTextHandler PrintText;

    public void Save(string filename, string html)
    {
        SaveGame(filename);
    }

    public byte[] Save(string html)
    {
        return SaveGame(_gameData.Filename, false);
    }

    public void SendCommand(string command)
    {
        SendCommand(command, 0, null);
    }

    public void SendCommand(string command, IDictionary<string, string> metadata)
    {
        SendCommand(command, 0, metadata);
    }

    public void SendCommand(string command, int elapsedTime, IDictionary<string, string> metadata)
    {
        // The processing of commands is done in a separate thread, so things like the "enter" command can
        // lock the thread while waiting for further input. After starting to process the command, we wait
        // for something to happen before returning from the SendCommand call - either the command will have
        // finished processing, or perhaps a prompt has been printed and now the game is waiting for further
        // user input after hitting an "enter" script command.

        if (!_readyForCommand)
        {
            return;
        }

        var runnerThread =
            new Thread(ProcessCommandInNewThread);
        ChangeState(State.Working);
        runnerThread.Start(command);

        WaitForStateChange(State.Working);

        if (elapsedTime > 0)
        {
            Tick(elapsedTime);
        }
        else
        {
            RaiseNextTimerTickRequest();
        }
    }

    private void WaitForStateChange(State changedFromState)
    {
        lock (_stateLock)
        {
            while ((_state == changedFromState) & !_gameFinished)
            {
                Monitor.Wait(_stateLock);
            }
        }
    }

    private void ProcessCommandInNewThread(object command)
    {
        // Process command, and change state to Ready if the command finished processing

        try
        {
            if (ExecCommand((string) command, new Context()))
            {
                ChangeState(State.Ready);
            }
        }
        catch (Exception ex)
        {
            LogException(ex);
            ChangeState(State.Ready);
        }
    }

    public void SendEvent(string eventName, string param)
    {
    }

    public event UpdateListHandler UpdateList;

    public async Task<bool> Initialise(IPlayer player, bool? isCompiled = default)
    {
        _player = player;
        if (Strings.LCase(Strings.Right(_gameData.Filename, 4)) == ".qsg")
        {
            return OpenGame(_gameData.Filename);
        }

        return await InitialiseGame(_gameData);
    }

    private void GameFinished()
    {
        _gameFinished = true;

        Finished?.Invoke();

        ChangeState(State.Finished);

        // In case we're in the middle of processing an "enter" command, nudge the thread along
        lock (_commandLock)
        {
            Monitor.PulseAll(_commandLock);
        }

        lock (_waitLock)
        {
            Monitor.PulseAll(_waitLock);
        }

        lock (_stateLock)
        {
            Monitor.PulseAll(_stateLock);
        }

        Cleanup();
    }

    private string GetResourcePath(string filename)
    {
        if (_resourceFile is not null && _resourceFile.Length > 0)
        {
            var extractResult = ExtractFile(filename);
            return extractResult;
        }

        return Path.Combine(_gamePath, filename);
    }

    string IASL.GetResourcePath(string filename)
    {
        return GetResourcePath(filename);
    }

    private void Cleanup()
    {
        DeleteDirectory(TempFolder);
    }

    private void DeleteDirectory(string dir)
    {
        if (Directory.Exists(dir))
        {
            try
            {
                Directory.Delete(dir, true);
            }
            catch
            {
            }
        }
    }

    ~LegacyGame()
    {
        Cleanup();
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

    public string SaveExtension => "qsg";

    public void Tick(int elapsedTime)
    {
        int i;
        var timerScripts = new List<string>();

        Debug.Print("Tick: " + elapsedTime);

        var loopTo = _numberTimers;
        for (i = 1; i <= loopTo; i++)
        {
            if (_timers[i].TimerActive)
            {
                if (_timers[i].BypassThisTurn)
                {
                    // don't trigger timer during the turn it was first enabled
                    _timers[i].BypassThisTurn = false;
                }
                else
                {
                    _timers[i].TimerTicks = _timers[i].TimerTicks + elapsedTime;

                    if (_timers[i].TimerTicks >= _timers[i].TimerInterval)
                    {
                        _timers[i].TimerTicks = 0;
                        timerScripts.Add(_timers[i].TimerAction);
                    }
                }
            }
        }

        if (timerScripts.Count > 0)
        {
            var runnerThread =
                new Thread(RunTimersInNewThread);

            ChangeState(State.Working);
            runnerThread.Start(timerScripts);
            WaitForStateChange(State.Working);
        }

        RaiseNextTimerTickRequest();
    }

    private void RunTimersInNewThread(object scripts)
    {
        var scriptList = (List<string>) scripts;

        foreach (var script in scriptList)
        {
            try
            {
                ExecuteScript(script, _nullContext);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        ChangeState(State.Ready);
    }

    private void RaiseNextTimerTickRequest()
    {
        var anyTimerActive = false;
        var nextTrigger = 60;

        for (int i = 1, loopTo = _numberTimers; i <= loopTo; i++)
        {
            if (_timers[i].TimerActive)
            {
                anyTimerActive = true;

                var thisNextTrigger = _timers[i].TimerInterval - _timers[i].TimerTicks;
                if (thisNextTrigger < nextTrigger)
                {
                    nextTrigger = thisNextTrigger;
                }
            }
        }

        if (!anyTimerActive)
        {
            nextTrigger = 0;
        }

        if (_gameFinished)
        {
            nextTrigger = 0;
        }

        Debug.Print("RaiseNextTimerTickRequest " + nextTrigger);

        RequestNextTimerTick?.Invoke(nextTrigger);
    }

    private void ChangeState(State newState)
    {
        var acceptCommands = newState == State.Ready;
        ChangeState(newState, acceptCommands);
    }

    private void ChangeState(State newState, bool acceptCommands)
    {
        _readyForCommand = acceptCommands;
        lock (_stateLock)
        {
            _state = newState;
            Monitor.PulseAll(_stateLock);
        }
    }

    public void FinishWait()
    {
        if (_state != State.Waiting)
        {
            return;
        }

        var runnerThread = new Thread(FinishWaitInNewThread);
        ChangeState(State.Working);
        runnerThread.Start();
        WaitForStateChange(State.Working);
    }

    private void FinishWaitInNewThread()
    {
        lock (_waitLock)
        {
            Monitor.PulseAll(_waitLock);
        }
    }

    public void FinishPause()
    {
        FinishWait();
    }

    private string m_menuResponse;

    private string ShowMenu(MenuData menuData)
    {
        _player.ShowMenu(menuData);
        ChangeState(State.Waiting);

        lock (_waitLock)
        {
            Monitor.Wait(_waitLock);
        }

        return m_menuResponse;
    }

    public void SetMenuResponse(string response)
    {
        var runnerThread =
            new Thread(SetMenuResponseInNewThread);
        ChangeState(State.Working);
        runnerThread.Start(response);
        WaitForStateChange(State.Working);
    }

    private void SetMenuResponseInNewThread(object response)
    {
        m_menuResponse = (string) response;

        lock (_waitLock)
        {
            Monitor.PulseAll(_waitLock);
        }
    }

    private void LogException(Exception ex)
    {
        LogError?.Invoke(ex.Message + Environment.NewLine + ex.StackTrace);
    }

    public IEnumerable<string> GetExternalScripts()
    {
        return null;
    }

    public IEnumerable<string> GetExternalStylesheets()
    {
        return null;
    }

    public event Action<int> RequestNextTimerTick;

    private string GetOriginalFilenameForQSG()
    {
        if (_originalFilename is not null)
        {
            return _originalFilename;
        }

        return _gameFileName;
    }

    public delegate string UnzipFunctionDelegate(string filename, out string tempDir);

    private UnzipFunctionDelegate m_unzipFunction;

    public void SetUnzipFunction(UnzipFunctionDelegate unzipFunction)
    {
        m_unzipFunction = unzipFunction;
    }

    private string GetUnzippedFile(string filename)
    {
        string tempDir = null;
        var result = m_unzipFunction.Invoke(filename, out tempDir);
        TempFolder = tempDir;
        return result;
    }

    public string TempFolder { get; set; }

    public int ASLVersion { get; private set; }

    public Stream GetResource(string file)
    {
        if (file == "_game.cas")
        {
            return new MemoryStream(GetResourcelessCAS());
        }

        var path = GetResourcePath(file);
        if (!File.Exists(path))
        {
            return null;
        }

        return new FileStream(path, FileMode.Open, FileAccess.Read);
    }

    public string GameID
    {
        get
        {
            if (string.IsNullOrEmpty(_gameFileName))
            {
                return null;
            }

            return Utility.Utility.FileMD5Hash(_gameFileName);
        }
    }

    public IEnumerable<string> GetResourceNames()
    {
        for (int i = 1, loopTo = _numResources; i <= loopTo; i++)
        {
            yield return _resources[i].ResourceName;
        }

        if (_numResources > 0)
        {
            yield return "_game.cas";
        }
    }

    private byte[] GetResourcelessCAS()
    {
        var fileData = File.ReadAllText(_resourceFile, Encoding.GetEncoding(1252));
        return Encoding.GetEncoding(1252).GetBytes(Strings.Left(fileData, _startCatPos - 1));
    }
}