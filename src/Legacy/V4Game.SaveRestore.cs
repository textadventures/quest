using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Constants = Microsoft.VisualBasic.Constants;
using Strings = Microsoft.VisualBasic.Strings;

namespace QuestViva.Legacy;

public partial class V4Game
{
    private void AddToChangeLog(string appliesTo, string changeData)
    {
        _gameChangeData.NumberChanges = _gameChangeData.NumberChanges + 1;
        Array.Resize(ref _gameChangeData.ChangeData, _gameChangeData.NumberChanges + 1);
        _gameChangeData.ChangeData[_gameChangeData.NumberChanges] = new ChangeType();
        _gameChangeData.ChangeData[_gameChangeData.NumberChanges].AppliesTo = appliesTo;
        _gameChangeData.ChangeData[_gameChangeData.NumberChanges].Change = changeData;
    }

    private void AddToObjectChangeLog(ChangeLog.AppliesTo appliesToType, string appliesTo, string element,
        string changeData)
    {
        ChangeLog changeLog;

        // NOTE: We're only actually ever using the object changelog.
        // Rooms only get logged for creating rooms and creating/destroying exits, so we don't
        // need the refactored ChangeLog component for those.

        switch (appliesToType)
        {
            case ChangeLog.AppliesTo.Object:
            {
                changeLog = _changeLogObjects;
                break;
            }
            case ChangeLog.AppliesTo.Room:
            {
                changeLog = _changeLogRooms;
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        changeLog.AddItem(ref appliesTo, ref element, ref changeData);
    }

    private void InitFileData(string fileData)
    {
        _fileData = fileData;
        _fileDataPos = 1;
    }

    private string GetNextChunk()
    {
        var nullPos = Strings.InStr(_fileDataPos, _fileData, "\0");
        var result = Strings.Mid(_fileData, _fileDataPos, nullPos - _fileDataPos);

        if (nullPos < Strings.Len(_fileData))
        {
            _fileDataPos = nullPos + 1;
        }

        return result;
    }

    public string GetFileDataChars(int count)
    {
        var result = Strings.Mid(_fileData, _fileDataPos, count);
        _fileDataPos = _fileDataPos + count;
        return result;
    }

    private string MakeRestoreData()
    {
        var data = new StringBuilder();
        var objectData = new ChangeType[1];
        var roomData = new ChangeType[1];
        var numObjectData = default(int);
        var numRoomData = default(int);

        // <<< FILE HEADER DATA >>>

        data.Append("QUEST300" + '\0' + "filename-not-used.asl" + '\0');

        // The start point for encrypted data is after the filename
        var start = data.Length + 1;

        data.Append(_currentRoom + '\0');

        // Organise Change Log

        for (int i = 1, loopTo = _gameChangeData.NumberChanges; i <= loopTo; i++)
        {
            if (BeginsWith(_gameChangeData.ChangeData[i].AppliesTo, "object "))
            {
                numObjectData = numObjectData + 1;
                Array.Resize(ref objectData, numObjectData + 1);
                objectData[numObjectData] = new ChangeType();
                objectData[numObjectData] = _gameChangeData.ChangeData[i];
            }
            else if (BeginsWith(_gameChangeData.ChangeData[i].AppliesTo, "room "))
            {
                numRoomData = numRoomData + 1;
                Array.Resize(ref roomData, numRoomData + 1);
                roomData[numRoomData] = new ChangeType();
                roomData[numRoomData] = _gameChangeData.ChangeData[i];
            }
        }

        // <<< OBJECT CREATE/CHANGE DATA >>>

        data.Append(Strings.Trim(InvariantStrings.Str(numObjectData + _changeLogObjects.Changes.Count)) + '\0');

        for (int i = 1, loopTo1 = numObjectData; i <= loopTo1; i++)
        {
            data.Append(GetEverythingAfter(objectData[i].AppliesTo, "object ") + '\0' + objectData[i].Change + '\0');
        }

        foreach (var key in _changeLogObjects.Changes.Keys)
        {
            var appliesTo = Strings.Split(key, "#")[0];
            var changeData = _changeLogObjects.Changes[key];

            data.Append(appliesTo + '\0' + changeData + '\0');
        }

        // <<< OBJECT EXIST/VISIBLE/ROOM DATA >>>

        data.Append(Strings.Trim(InvariantStrings.Str(_numberObjs)) + '\0');

        for (int i = 1, loopTo2 = _numberObjs; i <= loopTo2; i++)
        {
            string exists;
            string visible;

            if (_objs[i].Exists)
            {
                exists = "\u0001";
            }
            else
            {
                exists = "\0";
            }

            if (_objs[i].Visible)
            {
                visible = "\u0001";
            }
            else
            {
                visible = "\0";
            }

            data.Append(_objs[i].ObjectName + '\0' + exists + visible + _objs[i].ContainerRoom + '\0');
        }

        // <<< ROOM CREATE/CHANGE DATA >>>

        data.Append(Strings.Trim(InvariantStrings.Str(numRoomData)) + '\0');

        for (int i = 1, loopTo3 = numRoomData; i <= loopTo3; i++)
        {
            data.Append(GetEverythingAfter(roomData[i].AppliesTo, "room ") + '\0' + roomData[i].Change + '\0');
        }

        // <<< TIMER STATE DATA >>>

        data.Append(Strings.Trim(InvariantStrings.Str(_numberTimers)) + '\0');

        for (int i = 1, loopTo4 = _numberTimers; i <= loopTo4; i++)
        {
            var t = _timers[i];
            data.Append(t.TimerName + '\0');

            if (t.TimerActive)
            {
                data.Append('\u0001');
            }
            else
            {
                data.Append('\0');
            }

            data.Append(Strings.Trim(InvariantStrings.Str(t.TimerInterval)) + '\0');
            data.Append(Strings.Trim(InvariantStrings.Str(t.TimerTicks)) + '\0');
        }

        // <<< STRING VARIABLE DATA >>>

        data.Append(Strings.Trim(InvariantStrings.Str(_numberStringVariables)) + '\0');

        for (int i = 1, loopTo5 = _numberStringVariables; i <= loopTo5; i++)
        {
            var s = _stringVariable[i];
            data.Append(s.VariableName + '\0' + Strings.Trim(InvariantStrings.Str(s.VariableUBound)) + '\0');

            for (int j = 0, loopTo6 = s.VariableUBound; j <= loopTo6; j++)
            {
                data.Append(s.VariableContents[j] + '\0');
            }
        }

        // <<< NUMERIC VARIABLE DATA >>>

        data.Append(Strings.Trim(InvariantStrings.Str(_numberNumericVariables)) + '\0');

        for (int i = 1, loopTo7 = _numberNumericVariables; i <= loopTo7; i++)
        {
            var n = _numericVariable[i];
            data.Append(n.VariableName + '\0' + Strings.Trim(InvariantStrings.Str(n.VariableUBound)) + '\0');

            for (int j = 0, loopTo8 = n.VariableUBound; j <= loopTo8; j++)
            {
                data.Append(n.VariableContents[j] + '\0');
            }
        }

        // Now encrypt data
        string dataString;
        var newFileData = new StringBuilder();

        dataString = data.ToString();

        newFileData.Append(Strings.Left(dataString, start - 1));

        for (int i = start, loopTo9 = Strings.Len(dataString); i <= loopTo9; i++)
        {
            newFileData.Append(InvariantStrings.Chr(255 - InvariantStrings.Asc(Strings.Mid(dataString, i, 1))));
        }

        return newFileData.ToString();
    }

    public async Task<byte[]> SaveAsync(string html)
    {
        var ctx = new Context();
        string saveData;

        if (ASLVersion >= 391)
        {
            await ExecuteScript(_beforeSaveScript, ctx);
        }

        if (ASLVersion >= 280)
        {
            saveData = MakeRestoreData();
        }
        else
        {
            saveData = MakeRestoreDataV2();
        }

        return Encoding.GetEncoding(1252).GetBytes(saveData);
    }

    private async Task RestoreGameData(string fileData)
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
            decryptedFile.Append(InvariantStrings.Chr(255 - InvariantStrings.Asc(Strings.Mid(fileData, i, 1))));
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
                    await ExecuteCreate("object <" + createData + ">", _nullContext);
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
                await ExecuteCreate(data, _nullContext);
            }
            else if (data == "create")
            {
                await ExecuteCreate("room <" + appliesTo + ">", _nullContext);
            }
            else if (BeginsWith(data, "destroy exit "))
            {
                await DestroyExit(appliesTo + "; " + GetEverythingAfter(data, "destroy exit "), _nullContext);
            }
        }

        // Now go through and apply object properties and actions

        for (int i = 1, loopTo4 = numStoredData; i <= loopTo4; i++)
        {
            var d = storedData[i];
            if (BeginsWith(d.Change, "properties "))
            {
                await AddToObjectProperties(GetEverythingAfter(d.Change, "properties "), GetObjectIdNoAlias(d.AppliesTo),
                    _nullContext);
            }
            else if (BeginsWith(d.Change, "action "))
            {
                await AddToObjectActions(GetEverythingAfter(d.Change, "action "), GetObjectIdNoAlias(d.AppliesTo),
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
                await SetStringContents(appliesTo, data, _nullContext);
            }
            else
            {
                for (int j = 0, loopTo8 = varUbound; j <= loopTo8; j++)
                {
                    data = GetNextChunk();
                    await SetStringContents(appliesTo, data, _nullContext, j);
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
                await SetNumericVariableContents(appliesTo, Conversion.Val(data), _nullContext);
            }
            else
            {
                for (int j = 0, loopTo10 = varUbound; j <= loopTo10; j++)
                {
                    data = GetNextChunk();
                    await SetNumericVariableContents(appliesTo, Conversion.Val(data), _nullContext, j);
                }
            }
        }

        _gameIsRestoring = false;
    }

    private string MakeRestoreDataV2()
    {
        var lines = new List<string>();
        int i;

        lines.Add("QUEST200.1");
        lines.Add("filename-not-used.asl");
        lines.Add(_gameName);
        lines.Add(_currentRoom);

        lines.Add("!c");
        var loopTo = _numCollectables;
        for (i = 1; i <= loopTo; i++)
        {
            lines.Add(_collectables[i].Name + ";" + InvariantStrings.Str(_collectables[i].Value));
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
                      InvariantStrings.Str(Conversion.Val(_numericVariable[i].VariableContents[0])));
        }

        lines.Add("!e");

        return string.Join(Constants.vbCrLf, lines);
    }
}
