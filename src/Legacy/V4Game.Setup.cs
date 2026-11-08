using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using QuestViva.Common;
using Constants = Microsoft.VisualBasic.Constants;
using Strings = Microsoft.VisualBasic.Strings;

namespace QuestViva.Legacy;

public partial class V4Game
{
    private async Task ProcessListInfo(string line, int id)
    {
        var listInfo = new TextAction();
        var propName = "";

        if (BeginsWith(line, "list closed <"))
        {
            listInfo.Type = TextActionType.Text;
            listInfo.Data = await GetParameter(line, _nullContext);
            propName = "list closed";
        }
        else if (Strings.Trim(line) == "list closed off")
        {
            // default for list closed is off anyway
            return;
        }
        else if (BeginsWith(line, "list closed"))
        {
            listInfo.Type = TextActionType.Script;
            listInfo.Data = GetEverythingAfter(line, "list closed");
            propName = "list closed";
        }


        else if (BeginsWith(line, "list empty <"))
        {
            listInfo.Type = TextActionType.Text;
            listInfo.Data = await GetParameter(line, _nullContext);
            propName = "list empty";
        }
        else if (Strings.Trim(line) == "list empty off")
        {
            // default for list empty is off anyway
            return;
        }
        else if (BeginsWith(line, "list empty"))
        {
            listInfo.Type = TextActionType.Script;
            listInfo.Data = GetEverythingAfter(line, "list empty");
            propName = "list empty";
        }


        else if (Strings.Trim(line) == "list off")
        {
            await AddToObjectProperties("not list", id, _nullContext);
            return;
        }
        else if (BeginsWith(line, "list <"))
        {
            listInfo.Type = TextActionType.Text;
            listInfo.Data = await GetParameter(line, _nullContext);
            propName = "list";
        }
        else if (BeginsWith(line, "list "))
        {
            listInfo.Type = TextActionType.Script;
            listInfo.Data = GetEverythingAfter(line, "list ");
            propName = "list";
        }

        if (!string.IsNullOrEmpty(propName))
        {
            if (listInfo.Type == TextActionType.Text)
            {
                await AddToObjectProperties(propName + "=" + listInfo.Data, id, _nullContext);
            }
            else
            {
                await AddToObjectActions("<" + propName + "> " + listInfo.Data, id, _nullContext);
            }
        }
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

    private async Task SetUpDefaultFonts()
    {
        // Sets up default fonts
        var gameblock = GetDefineBlock("game");

        _defaultFontName = "Arial";
        _defaultFontSize = 9d;

        for (int i = gameblock.StartLine + 1, loopTo = gameblock.EndLine - 1; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[i], "default fontname "))
            {
                var name = await GetParameter(_lines[i], _nullContext);
                if (!string.IsNullOrEmpty(name))
                {
                    _defaultFontName = name;
                }
            }
            else if (BeginsWith(_lines[i], "default fontsize "))
            {
                var size = Conversions.ToInteger(await GetParameter(_lines[i], _nullContext));
                if (size != 0)
                {
                    _defaultFontSize = size;
                }
            }
        }
    }

    private async Task SetUpDisplayVariables()
    {
        for (int i = GetDefineBlock("game").StartLine + 1, loopTo = GetDefineBlock("game").EndLine - 1;
             i <= loopTo;
             i++)
        {
            if (BeginsWith(_lines[i], "define variable "))
            {
                var variable = new VariableType();
                variable.VariableContents = new string[1];

                variable.VariableName = await GetParameter(_lines[i], _nullContext);
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

                        variable.DisplayString = await GetParameter(_lines[i], _nullContext, false);
                    }
                    else if (BeginsWith(_lines[i], "value "))
                    {
                        variable.VariableContents[0] = await GetParameter(_lines[i], _nullContext);
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

    private async Task SetUpGameObject()
    {
        _numberObjs = 1;
        // Slot 0 is what "no such object" (id 0) reads. VB6 Quest's records were value types, so it was a blank
        // record there rather than null.
        _objs = new ObjectType[2];
        _objs[0] = new ObjectType();
        _rooms = [new RoomType()];
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
                    await AddToObjectProperties(await GetParameter(_lines[i], _nullContext), _numberObjs, _nullContext);
                }
                else if (BeginsWith(_lines[i], "type "))
                {
                    o.NumberTypesIncluded = o.NumberTypesIncluded + 1;
                    Array.Resize(ref o.TypesIncluded, o.NumberTypesIncluded + 1);
                    o.TypesIncluded[o.NumberTypesIncluded] = await GetParameter(_lines[i], _nullContext);

                    var propertyData = await GetPropertiesInType(await GetParameter(_lines[i], _nullContext));
                    await AddToObjectProperties(propertyData.Properties, _numberObjs, _nullContext);
                    for (int k = 1, loopTo1 = propertyData.NumberActions; k <= loopTo1; k++)
                    {
                        await AddObjectAction(_numberObjs, propertyData.Actions[k].ActionName,
                            propertyData.Actions[k].Script);
                    }
                }
                else if (BeginsWith(_lines[i], "action "))
                {
                    await AddToObjectActions(GetEverythingAfter(_lines[i], "action "), _numberObjs, _nullContext);
                }
            }
            else if (Strings.Trim(_lines[i]) == "end define")
            {
                nestBlock = nestBlock - 1;
            }
        }
    }

    private async Task SetUpMenus()
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
                        "Can't load menu '" + await GetParameter(_lines[_defineBlocks[i].StartLine], _nullContext) +
                        "' - only one menu can be added.", LogType.WarningError);
                }
                else
                {
                    menuTitle = await GetParameter(_lines[_defineBlocks[i].StartLine], _nullContext);

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

    private async Task SetUpRoomData()
    {
        var defaultProperties = new PropertiesActions();

        // see if define type <defaultroom> exists:
        var defaultExists = false;
        for (int i = 1, loopTo = _numberSections; i <= loopTo; i++)
        {
            if (Strings.Trim(_lines[_defineBlocks[i].StartLine]) == "define type <defaultroom>")
            {
                defaultExists = true;
                defaultProperties = await GetPropertiesInType("defaultroom");
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

                r.RoomName = await GetParameter(_lines[_defineBlocks[i].StartLine], _nullContext);
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
                    await AddToObjectProperties(defaultProperties.Properties, _numberObjs, _nullContext);
                    for (int k = 1, loopTo2 = defaultProperties.NumberActions; k <= loopTo2; k++)
                    {
                        await AddObjectAction(_numberObjs, defaultProperties.Actions[k].ActionName,
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
                        r.RoomAlias = await GetParameter(_lines[j], _nullContext);
                        _objs[_numberObjs].ObjectAlias = r.RoomAlias;
                        if (ASLVersion >= 350)
                        {
                            await AddToObjectProperties("alias=" + r.RoomAlias, _numberObjs, _nullContext);
                        }
                    }
                    else if ((ASLVersion >= 280) & BeginsWith(_lines[j], "description "))
                    {
                        r.Description = await GetTextOrScript(GetEverythingAfter(_lines[j], "description "));
                        if (ASLVersion >= 350)
                        {
                            if (r.Description.Type == TextActionType.Script)
                            {
                                await AddObjectAction(_numberObjs, "description", r.Description.Data);
                            }
                            else
                            {
                                await AddToObjectProperties("description=" + r.Description.Data, _numberObjs, _nullContext);
                            }
                        }
                    }
                    else if (BeginsWith(_lines[j], "out "))
                    {
                        r.Out.Text = await GetParameter(_lines[j], _nullContext);
                        r.Out.Script = Strings.Trim(Strings.Mid(_lines[j], Strings.InStr(_lines[j], ">") + 1));
                        if (ASLVersion >= 350)
                        {
                            if (!string.IsNullOrEmpty(r.Out.Script))
                            {
                                await AddObjectAction(_numberObjs, "out", r.Out.Script);
                            }

                            await AddToObjectProperties("out=" + r.Out.Text, _numberObjs, _nullContext);
                        }
                    }
                    else if (BeginsWith(_lines[j], "east "))
                    {
                        r.East = await GetTextOrScript(GetEverythingAfter(_lines[j], "east "));
                        if (ASLVersion >= 350)
                        {
                            if (r.East.Type == TextActionType.Script)
                            {
                                await AddObjectAction(_numberObjs, "east", r.East.Data);
                            }
                            else
                            {
                                await AddToObjectProperties("east=" + r.East.Data, _numberObjs, _nullContext);
                            }
                        }
                    }
                    else if (BeginsWith(_lines[j], "west "))
                    {
                        r.West = await GetTextOrScript(GetEverythingAfter(_lines[j], "west "));
                        if (ASLVersion >= 350)
                        {
                            if (r.West.Type == TextActionType.Script)
                            {
                                await AddObjectAction(_numberObjs, "west", r.West.Data);
                            }
                            else
                            {
                                await AddToObjectProperties("west=" + r.West.Data, _numberObjs, _nullContext);
                            }
                        }
                    }
                    else if (BeginsWith(_lines[j], "north "))
                    {
                        r.North = await GetTextOrScript(GetEverythingAfter(_lines[j], "north "));
                        if (ASLVersion >= 350)
                        {
                            if (r.North.Type == TextActionType.Script)
                            {
                                await AddObjectAction(_numberObjs, "north", r.North.Data);
                            }
                            else
                            {
                                await AddToObjectProperties("north=" + r.North.Data, _numberObjs, _nullContext);
                            }
                        }
                    }
                    else if (BeginsWith(_lines[j], "south "))
                    {
                        r.South = await GetTextOrScript(GetEverythingAfter(_lines[j], "south "));
                        if (ASLVersion >= 350)
                        {
                            if (r.South.Type == TextActionType.Script)
                            {
                                await AddObjectAction(_numberObjs, "south", r.South.Data);
                            }
                            else
                            {
                                await AddToObjectProperties("south=" + r.South.Data, _numberObjs, _nullContext);
                            }
                        }
                    }
                    else if (BeginsWith(_lines[j], "northeast "))
                    {
                        r.NorthEast = await GetTextOrScript(GetEverythingAfter(_lines[j], "northeast "));
                        if (ASLVersion >= 350)
                        {
                            if (r.NorthEast.Type == TextActionType.Script)
                            {
                                await AddObjectAction(_numberObjs, "northeast", r.NorthEast.Data);
                            }
                            else
                            {
                                await AddToObjectProperties("northeast=" + r.NorthEast.Data, _numberObjs, _nullContext);
                            }
                        }
                    }
                    else if (BeginsWith(_lines[j], "northwest "))
                    {
                        r.NorthWest = await GetTextOrScript(GetEverythingAfter(_lines[j], "northwest "));
                        if (ASLVersion >= 350)
                        {
                            if (r.NorthWest.Type == TextActionType.Script)
                            {
                                await AddObjectAction(_numberObjs, "northwest", r.NorthWest.Data);
                            }
                            else
                            {
                                await AddToObjectProperties("northwest=" + r.NorthWest.Data, _numberObjs, _nullContext);
                            }
                        }
                    }
                    else if (BeginsWith(_lines[j], "southeast "))
                    {
                        r.SouthEast = await GetTextOrScript(GetEverythingAfter(_lines[j], "southeast "));
                        if (ASLVersion >= 350)
                        {
                            if (r.SouthEast.Type == TextActionType.Script)
                            {
                                await AddObjectAction(_numberObjs, "southeast", r.SouthEast.Data);
                            }
                            else
                            {
                                await AddToObjectProperties("southeast=" + r.SouthEast.Data, _numberObjs, _nullContext);
                            }
                        }
                    }
                    else if (BeginsWith(_lines[j], "southwest "))
                    {
                        r.SouthWest = await GetTextOrScript(GetEverythingAfter(_lines[j], "southwest "));
                        if (ASLVersion >= 350)
                        {
                            if (r.SouthWest.Type == TextActionType.Script)
                            {
                                await AddObjectAction(_numberObjs, "southwest", r.SouthWest.Data);
                            }
                            else
                            {
                                await AddToObjectProperties("southwest=" + r.SouthWest.Data, _numberObjs, _nullContext);
                            }
                        }
                    }
                    else if (BeginsWith(_lines[j], "up "))
                    {
                        r.Up = await GetTextOrScript(GetEverythingAfter(_lines[j], "up "));
                        if (ASLVersion >= 350)
                        {
                            if (r.Up.Type == TextActionType.Script)
                            {
                                await AddObjectAction(_numberObjs, "up", r.Up.Data);
                            }
                            else
                            {
                                await AddToObjectProperties("up=" + r.Up.Data, _numberObjs, _nullContext);
                            }
                        }
                    }
                    else if (BeginsWith(_lines[j], "down "))
                    {
                        r.Down = await GetTextOrScript(GetEverythingAfter(_lines[j], "down "));
                        if (ASLVersion >= 350)
                        {
                            if (r.Down.Type == TextActionType.Script)
                            {
                                await AddObjectAction(_numberObjs, "down", r.Down.Data);
                            }
                            else
                            {
                                await AddToObjectProperties("down=" + r.Down.Data, _numberObjs, _nullContext);
                            }
                        }
                    }
                    else if ((ASLVersion >= 280) & BeginsWith(_lines[j], "indescription "))
                    {
                        r.InDescription = await GetParameter(_lines[j], _nullContext);
                        if (ASLVersion >= 350)
                        {
                            await AddToObjectProperties("indescription=" + r.InDescription, _numberObjs, _nullContext);
                        }
                    }
                    else if ((ASLVersion >= 280) & BeginsWith(_lines[j], "look "))
                    {
                        r.Look = await GetParameter(_lines[j], _nullContext);
                        if (ASLVersion >= 350)
                        {
                            await AddToObjectProperties("look=" + r.Look, _numberObjs, _nullContext);
                        }
                    }
                    else if (BeginsWith(_lines[j], "prefix "))
                    {
                        r.Prefix = await GetParameter(_lines[j], _nullContext);
                        if (ASLVersion >= 350)
                        {
                            await AddToObjectProperties("prefix=" + r.Prefix, _numberObjs, _nullContext);
                        }
                    }
                    else if (BeginsWith(_lines[j], "script "))
                    {
                        r.Script = GetEverythingAfter(_lines[j], "script ");
                        await AddObjectAction(_numberObjs, "script", r.Script);
                    }
                    else if (BeginsWith(_lines[j], "command "))
                    {
                        r.NumberCommands = r.NumberCommands + 1;
                        Array.Resize(ref r.Commands, r.NumberCommands + 1);
                        r.Commands[r.NumberCommands] = new UserDefinedCommandType();
                        r.Commands[r.NumberCommands].CommandText = await GetParameter(_lines[j], _nullContext, false);
                        r.Commands[r.NumberCommands].CommandScript =
                            Strings.Trim(Strings.Mid(_lines[j], Strings.InStr(_lines[j], ">") + 1));
                    }
                    else if (BeginsWith(_lines[j], "place "))
                    {
                        r.NumberPlaces = r.NumberPlaces + 1;
                        Array.Resize(ref r.Places, r.NumberPlaces + 1);
                        r.Places[r.NumberPlaces] = new PlaceType();
                        var placeData = await GetParameter(_lines[j], _nullContext);
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
                        r.Use[r.NumberUse].Text = await GetParameter(_lines[j], _nullContext);
                        r.Use[r.NumberUse].Script =
                            Strings.Trim(Strings.Mid(_lines[j], Strings.InStr(_lines[j], ">") + 1));
                    }
                    else if (BeginsWith(_lines[j], "properties "))
                    {
                        await AddToObjectProperties(await GetParameter(_lines[j], _nullContext), _numberObjs, _nullContext);
                    }
                    else if (BeginsWith(_lines[j], "type "))
                    {
                        _objs[_numberObjs].NumberTypesIncluded = _objs[_numberObjs].NumberTypesIncluded + 1;
                        Array.Resize(ref _objs[_numberObjs].TypesIncluded, _objs[_numberObjs].NumberTypesIncluded + 1);
                        _objs[_numberObjs].TypesIncluded[_objs[_numberObjs].NumberTypesIncluded] =
                            await GetParameter(_lines[j], _nullContext);

                        var propertyData = await GetPropertiesInType(await GetParameter(_lines[j], _nullContext));
                        await AddToObjectProperties(propertyData.Properties, _numberObjs, _nullContext);
                        for (int k = 1, loopTo4 = propertyData.NumberActions; k <= loopTo4; k++)
                        {
                            await AddObjectAction(_numberObjs, propertyData.Actions[k].ActionName,
                                propertyData.Actions[k].Script);
                        }
                    }
                    else if (BeginsWith(_lines[j], "action "))
                    {
                        await AddToObjectActions(GetEverythingAfter(_lines[j], "action "), _numberObjs, _nullContext);
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

    private async Task SetUpSynonyms()
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

    private async Task SetUpTimers()
    {
        for (int i = 1, loopTo = _numberSections; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[_defineBlocks[i].StartLine], "define timer "))
            {
                _numberTimers = _numberTimers + 1;
                Array.Resize(ref _timers, _numberTimers + 1);
                _timers[_numberTimers] = new TimerType();
                _timers[_numberTimers].TimerName = await GetParameter(_lines[_defineBlocks[i].StartLine], _nullContext);
                _timers[_numberTimers].TimerActive = false;

                for (int j = _defineBlocks[i].StartLine + 1, loopTo1 = _defineBlocks[i].EndLine - 1; j <= loopTo1; j++)
                {
                    if (BeginsWith(_lines[j], "interval "))
                    {
                        _timers[_numberTimers].TimerInterval =
                            Conversions.ToInteger(await GetParameter(_lines[j], _nullContext));
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

    private async Task SetUpTurnScript()
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

    private async Task SetUpUserDefinedPlayerErrors()
    {
        // goes through "define game" block and sets stored error
        // messages accordingly

        var block = GetDefineBlock("game");
        var examineIsCustomised = false;

        for (int i = block.StartLine + 1, loopTo = block.EndLine - 1; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[i], "error "))
            {
                var errorInfo = await GetParameter(_lines[i], _nullContext, false);
                var scp = Strings.InStr(errorInfo, ";");
                var errorName = Strings.Left(errorInfo, scp - 1);
                var errorMsg = Strings.Trim(Strings.Mid(errorInfo, scp + 1));
                // -1 until the name is recognised, as an unknown name used to land on slot 0, overwriting badcommand
                var currentError = -1;

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
                    // Quest's own spelling is "defaultake", but QDK writes "defaulttake"
                    case "defaultake":
                    case "defaulttake":
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

                if (currentError == -1)
                {
                    continue;
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

    private async Task SetUpCharObjectInfo()
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
                defaultProperties = await GetPropertiesInType("default");
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
                origContainerRoomName = await GetParameter(_lines[block.StartLine], _nullContext);
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

                    o.ObjectName = await GetParameter(_lines[j], _nullContext);
                    o.ObjectAlias = o.ObjectName;
                    o.DefinitionSectionStart = j;
                    o.ContainerRoom = containerRoomName;
                    o.Visible = true;
                    o.Gender = "it";
                    o.Article = "it";

                    o.Take.Type = TextActionType.Nothing;

                    if (defaultExists)
                    {
                        await AddToObjectProperties(defaultProperties.Properties, _numberObjs, _nullContext);
                        for (int k = 1, loopTo3 = defaultProperties.NumberActions; k <= loopTo3; k++)
                        {
                            await AddObjectAction(_numberObjs, defaultProperties.Actions[k].ActionName,
                                defaultProperties.Actions[k].Script);
                        }
                    }

                    if (ASLVersion >= 391)
                    {
                        await AddToObjectProperties("list", _numberObjs, _nullContext);
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
                                await AddToObjectProperties("hidden", _numberObjs, _nullContext);
                            }
                        }
                        else if (BeginsWith(_lines[j], "startin ") & (containerRoomName == "__UNKNOWN"))
                        {
                            containerRoomName = await GetParameter(_lines[j], _nullContext);
                        }
                        else if (BeginsWith(_lines[j], "prefix "))
                        {
                            o.Prefix = await GetParameter(_lines[j], _nullContext) + " ";
                            if (ASLVersion >= 311)
                            {
                                await AddToObjectProperties("prefix=" + o.Prefix, _numberObjs, _nullContext);
                            }
                        }
                        else if (BeginsWith(_lines[j], "suffix "))
                        {
                            o.Suffix = await GetParameter(_lines[j], _nullContext);
                            if (ASLVersion >= 311)
                            {
                                await AddToObjectProperties("suffix=" + o.Suffix, _numberObjs, _nullContext);
                            }
                        }
                        else if (Strings.Trim(_lines[j]) == "invisible")
                        {
                            o.Visible = false;
                            if (ASLVersion >= 311)
                            {
                                await AddToObjectProperties("invisible", _numberObjs, _nullContext);
                            }
                        }
                        else if (BeginsWith(_lines[j], "alias "))
                        {
                            o.ObjectAlias = await GetParameter(_lines[j], _nullContext);
                            if (ASLVersion >= 311)
                            {
                                await AddToObjectProperties("alias=" + o.ObjectAlias, _numberObjs, _nullContext);
                            }
                        }
                        else if (BeginsWith(_lines[j], "alt "))
                        {
                            AddToObjectAltNames(await GetParameter(_lines[j], _nullContext), _numberObjs);
                        }
                        else if (BeginsWith(_lines[j], "detail "))
                        {
                            o.Detail = await GetParameter(_lines[j], _nullContext);
                            if (ASLVersion >= 311)
                            {
                                await AddToObjectProperties("detail=" + o.Detail, _numberObjs, _nullContext);
                            }
                        }
                        else if (BeginsWith(_lines[j], "gender "))
                        {
                            o.Gender = await GetParameter(_lines[j], _nullContext);
                            if (ASLVersion >= 311)
                            {
                                await AddToObjectProperties("gender=" + o.Gender, _numberObjs, _nullContext);
                            }
                        }
                        else if (BeginsWith(_lines[j], "article "))
                        {
                            o.Article = await GetParameter(_lines[j], _nullContext);
                            if (ASLVersion >= 311)
                            {
                                await AddToObjectProperties("article=" + o.Article, _numberObjs, _nullContext);
                            }
                        }
                        else if (BeginsWith(_lines[j], "gain "))
                        {
                            o.GainScript = GetEverythingAfter(_lines[j], "gain ");
                            await AddObjectAction(_numberObjs, "gain", o.GainScript);
                        }
                        else if (BeginsWith(_lines[j], "lose "))
                        {
                            o.LoseScript = GetEverythingAfter(_lines[j], "lose ");
                            await AddObjectAction(_numberObjs, "lose", o.LoseScript);
                        }
                        else if (BeginsWith(_lines[j], "displaytype "))
                        {
                            o.DisplayType = await GetParameter(_lines[j], _nullContext);
                            if (ASLVersion >= 311)
                            {
                                await AddToObjectProperties("displaytype=" + o.DisplayType, _numberObjs, _nullContext);
                            }
                        }
                        else if (BeginsWith(_lines[j], "look "))
                        {
                            if (ASLVersion >= 311)
                            {
                                restOfLine = GetEverythingAfter(_lines[j], "look ");
                                if (Strings.Left(restOfLine, 1) == "<")
                                {
                                    await AddToObjectProperties("look=" + await GetParameter(_lines[j], _nullContext), _numberObjs,
                                        _nullContext);
                                }
                                else
                                {
                                    await AddObjectAction(_numberObjs, "look", restOfLine);
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
                                    await AddToObjectProperties("examine=" + await GetParameter(_lines[j], _nullContext),
                                        _numberObjs, _nullContext);
                                }
                                else
                                {
                                    await AddObjectAction(_numberObjs, "examine", restOfLine);
                                }
                            }
                        }
                        else if ((ASLVersion >= 311) & BeginsWith(_lines[j], "speak "))
                        {
                            restOfLine = GetEverythingAfter(_lines[j], "speak ");
                            if (Strings.Left(restOfLine, 1) == "<")
                            {
                                await AddToObjectProperties("speak=" + await GetParameter(_lines[j], _nullContext), _numberObjs,
                                    _nullContext);
                            }
                            else
                            {
                                await AddObjectAction(_numberObjs, "speak", restOfLine);
                            }
                        }
                        else if (BeginsWith(_lines[j], "properties "))
                        {
                            await AddToObjectProperties(await GetParameter(_lines[j], _nullContext), _numberObjs, _nullContext);
                        }
                        else if (BeginsWith(_lines[j], "type "))
                        {
                            o.NumberTypesIncluded = o.NumberTypesIncluded + 1;
                            Array.Resize(ref o.TypesIncluded, o.NumberTypesIncluded + 1);
                            o.TypesIncluded[o.NumberTypesIncluded] = await GetParameter(_lines[j], _nullContext);

                            var PropertyData = await GetPropertiesInType(await GetParameter(_lines[j], _nullContext));
                            await AddToObjectProperties(PropertyData.Properties, _numberObjs, _nullContext);
                            for (int k = 1, loopTo4 = PropertyData.NumberActions; k <= loopTo4; k++)
                            {
                                await AddObjectAction(_numberObjs, PropertyData.Actions[k].ActionName,
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
                            await AddToObjectActions(GetEverythingAfter(_lines[j], "action "), _numberObjs, _nullContext);
                        }
                        else if (BeginsWith(_lines[j], "use "))
                        {
                            await AddToUseInfo(_numberObjs, GetEverythingAfter(_lines[j], "use "));
                        }
                        else if (BeginsWith(_lines[j], "give "))
                        {
                            await AddToGiveInfo(_numberObjs, GetEverythingAfter(_lines[j], "give "));
                        }
                        else if (Strings.Trim(_lines[j]) == "take")
                        {
                            o.Take.Type = TextActionType.Default;
                            await AddToObjectProperties("take", _numberObjs, _nullContext);
                        }
                        else if (BeginsWith(_lines[j], "take "))
                        {
                            if (Strings.Left(GetEverythingAfter(_lines[j], "take "), 1) == "<")
                            {
                                o.Take.Type = TextActionType.Text;
                                o.Take.Data = await GetParameter(_lines[j], _nullContext);

                                await AddToObjectProperties("take=" + await GetParameter(_lines[j], _nullContext), _numberObjs,
                                    _nullContext);
                            }
                            else
                            {
                                o.Take.Type = TextActionType.Script;
                                restOfLine = GetEverythingAfter(_lines[j], "take ");
                                o.Take.Data = restOfLine;

                                await AddObjectAction(_numberObjs, "take", restOfLine);
                            }
                        }
                        else if (Strings.Trim(_lines[j]) == "container")
                        {
                            if (ASLVersion >= 391)
                            {
                                await AddToObjectProperties("container", _numberObjs, _nullContext);
                            }
                        }
                        else if (Strings.Trim(_lines[j]) == "surface")
                        {
                            if (ASLVersion >= 391)
                            {
                                await AddToObjectProperties("container", _numberObjs, _nullContext);
                                await AddToObjectProperties("surface", _numberObjs, _nullContext);
                            }
                        }
                        else if (Strings.Trim(_lines[j]) == "opened")
                        {
                            if (ASLVersion >= 391)
                            {
                                await AddToObjectProperties("opened", _numberObjs, _nullContext);
                            }
                        }
                        else if (Strings.Trim(_lines[j]) == "transparent")
                        {
                            if (ASLVersion >= 391)
                            {
                                await AddToObjectProperties("transparent", _numberObjs, _nullContext);
                            }
                        }
                        else if (Strings.Trim(_lines[j]) == "open")
                        {
                            await AddToObjectProperties("open", _numberObjs, _nullContext);
                        }
                        else if (BeginsWith(_lines[j], "open "))
                        {
                            if (Strings.Left(GetEverythingAfter(_lines[j], "open "), 1) == "<")
                            {
                                await AddToObjectProperties("open=" + await GetParameter(_lines[j], _nullContext), _numberObjs,
                                    _nullContext);
                            }
                            else
                            {
                                restOfLine = GetEverythingAfter(_lines[j], "open ");
                                await AddObjectAction(_numberObjs, "open", restOfLine);
                            }
                        }
                        else if (Strings.Trim(_lines[j]) == "close")
                        {
                            await AddToObjectProperties("close", _numberObjs, _nullContext);
                        }
                        else if (BeginsWith(_lines[j], "close "))
                        {
                            if (Strings.Left(GetEverythingAfter(_lines[j], "close "), 1) == "<")
                            {
                                await AddToObjectProperties("close=" + await GetParameter(_lines[j], _nullContext), _numberObjs,
                                    _nullContext);
                            }
                            else
                            {
                                restOfLine = GetEverythingAfter(_lines[j], "close ");
                                await AddObjectAction(_numberObjs, "close", restOfLine);
                            }
                        }
                        else if (Strings.Trim(_lines[j]) == "add")
                        {
                            await AddToObjectProperties("add", _numberObjs, _nullContext);
                        }
                        else if (BeginsWith(_lines[j], "add "))
                        {
                            if (Strings.Left(GetEverythingAfter(_lines[j], "add "), 1) == "<")
                            {
                                await AddToObjectProperties("add=" + await GetParameter(_lines[j], _nullContext), _numberObjs,
                                    _nullContext);
                            }
                            else
                            {
                                restOfLine = GetEverythingAfter(_lines[j], "add ");
                                await AddObjectAction(_numberObjs, "add", restOfLine);
                            }
                        }
                        else if (Strings.Trim(_lines[j]) == "remove")
                        {
                            await AddToObjectProperties("remove", _numberObjs, _nullContext);
                        }
                        else if (BeginsWith(_lines[j], "remove "))
                        {
                            if (Strings.Left(GetEverythingAfter(_lines[j], "remove "), 1) == "<")
                            {
                                await AddToObjectProperties("remove=" + await GetParameter(_lines[j], _nullContext), _numberObjs,
                                    _nullContext);
                            }
                            else
                            {
                                restOfLine = GetEverythingAfter(_lines[j], "remove ");
                                await AddObjectAction(_numberObjs, "remove", restOfLine);
                            }
                        }
                        else if (BeginsWith(_lines[j], "parent "))
                        {
                            await AddToObjectProperties("parent=" + await GetParameter(_lines[j], _nullContext), _numberObjs,
                                _nullContext);
                        }
                        else if (BeginsWith(_lines[j], "list"))
                        {
                            await ProcessListInfo(_lines[j], _numberObjs);
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
                    _chars[_numberChars].ObjectName = await GetParameter(_lines[j], _nullContext);
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
                            containerRoomName = await GetParameter(_lines[j], _nullContext);
                        }
                        else if (BeginsWith(_lines[j], "prefix "))
                        {
                            _chars[_numberChars].Prefix = await GetParameter(_lines[j], _nullContext) + " ";
                        }
                        else if (BeginsWith(_lines[j], "suffix "))
                        {
                            _chars[_numberChars].Suffix = " " + await GetParameter(_lines[j], _nullContext);
                        }
                        else if (Strings.Trim(_lines[j]) == "invisible")
                        {
                            _chars[_numberChars].Visible = false;
                        }
                        else if (BeginsWith(_lines[j], "alias "))
                        {
                            _chars[_numberChars].ObjectAlias = await GetParameter(_lines[j], _nullContext);
                        }
                        else if (BeginsWith(_lines[j], "detail "))
                        {
                            _chars[_numberChars].Detail = await GetParameter(_lines[j], _nullContext);
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

        await UpdateVisibilityInContainers(_nullContext);
    }

    private async Task SetUpCollectables()
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
                var collectables = Strings.Trim(await GetParameter(_lines[a], _nullContext, false));

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
                    while (!lastItem);

                    _numCollectables = _numCollectables - 1;
                }
            }
        }
    }

    private async Task SetUpItemArrays()
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
                var possItems = await GetParameter(_lines[a], _nullContext);

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
                    while (!lastItem);

                    _numberItems = _numberItems - 1;
                }
            }
        }
    }

    private async Task SetUpStartItems()
    {
        var lastItem = false;

        for (int a = GetDefineBlock("game").StartLine + 1, loopTo = GetDefineBlock("game").EndLine - 1;
             a <= loopTo;
             a++)
        {
            if (BeginsWith(_lines[a], "startitems "))
            {
                var startItems = await GetParameter(_lines[a], _nullContext);

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
                    while (!lastItem);
                }
            }
        }
    }

    private async Task SetUpExits()
    {
        // Exits have to be set up after all the rooms have been initialised

        for (int i = 1, loopTo = _numberSections; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[_defineBlocks[i].StartLine], "define room "))
            {
                var roomName = await GetParameter(_lines[_defineBlocks[i].StartLine], _nullContext);
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

                    await _rooms[roomId].Exits.AddExitFromTag(_lines[j]);
                }
            }
        }
    }
}
