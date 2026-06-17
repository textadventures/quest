using Microsoft.VisualBasic;

namespace QuestViva.Legacy;

internal class RoomExits
{
    private readonly Dictionary<V4Game.Direction, RoomExit> _directions = new();
    private readonly V4Game _game;
    private readonly Dictionary<string, RoomExit> _places = new();
    private Dictionary<object, RoomExit> _allExits;
    private int _objId;
    private bool _regenerateAllExits;

    public RoomExits(V4Game game)
    {
        _game = game;
        _regenerateAllExits = true;
    }

    private void SetDirection(ref V4Game.Direction direction, ref RoomExit roomExit)
    {
        if (_directions.ContainsKey(direction))
        {
            roomExit = _directions[direction];
            _game._objs[roomExit.GetObjId()].Exists = true;
        }
        else
        {
            roomExit = new RoomExit(_game);
            _directions.Add(direction, roomExit);
        }

        _regenerateAllExits = true;
    }

    public RoomExit GetDirectionExit(ref V4Game.Direction direction)
    {
        if (_directions.ContainsKey(direction))
        {
            return _directions[direction];
        }

        return null;
    }

    public void AddPlaceExit(ref RoomExit roomExit)
    {
        if (_places.ContainsKey(roomExit.GetToRoom()))
        {
            var removeItem = _places[roomExit.GetToRoom()];
            RemoveExit(ref removeItem);
        }

        _places.Add(roomExit.GetToRoom(), roomExit);
        _regenerateAllExits = true;
    }

    public void AddExitFromTag(string tag)
    {
        V4Game.Direction thisDir;
        var roomExit = (RoomExit) null;
        var @params = new string[1];
        string afterParam;
        var param = default(bool);

        if (_game.BeginsWith(tag, "out "))
        {
            tag = _game.GetEverythingAfter(tag, "out ");
            thisDir = V4Game.Direction.Out;
        }
        else if (_game.BeginsWith(tag, "east "))
        {
            tag = _game.GetEverythingAfter(tag, "east ");
            thisDir = V4Game.Direction.East;
        }
        else if (_game.BeginsWith(tag, "west "))
        {
            tag = _game.GetEverythingAfter(tag, "west ");
            thisDir = V4Game.Direction.West;
        }
        else if (_game.BeginsWith(tag, "north "))
        {
            tag = _game.GetEverythingAfter(tag, "north ");
            thisDir = V4Game.Direction.North;
        }
        else if (_game.BeginsWith(tag, "south "))
        {
            tag = _game.GetEverythingAfter(tag, "south ");
            thisDir = V4Game.Direction.South;
        }
        else if (_game.BeginsWith(tag, "northeast "))
        {
            tag = _game.GetEverythingAfter(tag, "northeast ");
            thisDir = V4Game.Direction.NorthEast;
        }
        else if (_game.BeginsWith(tag, "northwest "))
        {
            tag = _game.GetEverythingAfter(tag, "northwest ");
            thisDir = V4Game.Direction.NorthWest;
        }
        else if (_game.BeginsWith(tag, "southeast "))
        {
            tag = _game.GetEverythingAfter(tag, "southeast ");
            thisDir = V4Game.Direction.SouthEast;
        }
        else if (_game.BeginsWith(tag, "southwest "))
        {
            tag = _game.GetEverythingAfter(tag, "southwest ");
            thisDir = V4Game.Direction.SouthWest;
        }
        else if (_game.BeginsWith(tag, "up "))
        {
            tag = _game.GetEverythingAfter(tag, "up ");
            thisDir = V4Game.Direction.Up;
        }
        else if (_game.BeginsWith(tag, "down "))
        {
            tag = _game.GetEverythingAfter(tag, "down ");
            thisDir = V4Game.Direction.Down;
        }
        else if (_game.BeginsWith(tag, "place "))
        {
            tag = _game.GetEverythingAfter(tag, "place ");
            thisDir = V4Game.Direction.None;
        }
        else
        {
            return;
        }

        if (thisDir != V4Game.Direction.None)
        {
            // This will reuse an existing Exit object if we're resetting
            // the destination of an existing directional exit.
            SetDirection(ref thisDir, ref roomExit);
        }
        else
        {
            roomExit = new RoomExit(_game);
        }

        roomExit.SetParent(this);
        roomExit.SetDirection(thisDir);

        if (_game.BeginsWith(tag, "locked "))
        {
            roomExit.SetIsLocked(true);
            tag = _game.GetEverythingAfter(tag, "locked ");
        }

        if (Strings.Left(Strings.Trim(tag), 1) == "<")
        {
            @params = Strings.Split(_game.GetParameter(tag, _game._nullContext), ";");
            afterParam = Strings.Trim(Strings.Mid(tag, Strings.InStr(tag, ">") + 1));
            param = true;
        }
        else
        {
            afterParam = tag;
        }

        if (Strings.Len(afterParam) > 0)
        {
            // Script exit
            roomExit.SetScript(afterParam);

            if (thisDir == V4Game.Direction.None)
            {
                // A place exit with a script still has a ToRoom
                roomExit.SetToRoom(@params[0]);

                // and may have a lock message
                if (Information.UBound(@params) > 0)
                {
                    roomExit.SetLockMessage(@params[1]);
                }
            }
            // A directional exit with a script may have no parameter.
            // If it does have a parameter it will be a lock message.
            else if (param)
            {
                roomExit.SetLockMessage(@params[0]);
            }
        }
        else
        {
            roomExit.SetToRoom(@params[0]);
            if (Information.UBound(@params) > 0)
            {
                roomExit.SetLockMessage(@params[1]);
            }
        }

        if (thisDir == V4Game.Direction.None)
        {
            AddPlaceExit(ref roomExit);
        }
    }

    internal void AddExitFromCreateScript(string script, ref V4Game.Context ctx)
    {
        // sScript is the "create exit ..." script, but without the "create exit" at the beginning.
        // So it's very similar to creating an exit from a tag, except we have the source room
        // name before the semicolon (which we don't even care about as we ARE the source room).

        string param;
        string[] @params;
        int paramStart;
        int paramEnd;

        // Just need to convert e.g.
        // create exit <src_room; dest_room> { script }
        // to
        // place <dest_room> { script }
        // And
        // create exit north <src_room> { script }
        // to
        // north { script }
        // And
        // create exit north <src_room; dest_room>
        // to
        // north <dest_room>

        param = _game.GetParameter(script, ctx);
        @params = Strings.Split(param, ";");

        paramStart = Strings.InStr(script, "<");
        paramEnd = Strings.InStr(paramStart, script, ">");

        if (paramStart > 1)
        {
            // Directional exit
            if (Information.UBound(@params) == 0)
            {
                // Script directional exit
                AddExitFromTag(Strings.Trim(Strings.Left(script, paramStart - 1)) + " " +
                               Strings.Trim(Strings.Mid(script, paramEnd + 1)));
            }
            else
            {
                // "Normal" directional exit
                AddExitFromTag(Strings.Trim(Strings.Left(script, paramStart - 1)) + " <" + Strings.Trim(@params[1]) +
                               ">");
            }
        }
        else
        {
            if (Information.UBound(@params) < 1)
            {
                _game.LogASLError("No exit destination given in 'create exit " + script + "'",
                    V4Game.LogType.WarningError);
                return;
            }

            // Place exit so add "place" tag at the beginning
            AddExitFromTag("place <" + Strings.Trim(@params[1]) + Strings.Mid(script, paramEnd));
        }
    }

    public void SetObjId(int value)
    {
        _objId = value;
    }

    public int GetObjId()
    {
        return _objId;
    }

    public Dictionary<string, RoomExit> GetPlaces()
    {
        return _places;
    }

    internal void ExecuteGo(string cmd, ref V4Game.Context ctx)
    {
        // This will handle "n", "go east", "go [to] library" etc.

        int lExitID;
        RoomExit oExit;

        if (_game.BeginsWith(cmd, "go to "))
        {
            cmd = _game.GetEverythingAfter(cmd, "go to ");
        }
        else if (_game.BeginsWith(cmd, "go "))
        {
            cmd = _game.GetEverythingAfter(cmd, "go ");
        }

        lExitID = _game.Disambiguate(cmd, _game._currentRoom, ctx, true);

        if (lExitID == -1)
        {
            _game.PlayerErrorMessage(V4Game.PlayerError.BadPlace, ctx);
        }
        else
        {
            oExit = GetExitByObjectId(ref lExitID);
            oExit.Go(ref ctx);
        }
    }

    internal void GetAvailableDirectionsDescription(ref string description, ref string list)
    {
        RoomExit roomExit;
        int count;
        string descPrefix;
        string orString;

        descPrefix = "You can go";
        orString = "or";

        list = "";
        count = 0;

        foreach (var kvp in AllExits())
        {
            count = count + 1;
            roomExit = kvp.Value;

            string localGetDirectionToken()
            {
                var argdir = roomExit.GetDirection();
                var ret = GetDirectionToken(ref argdir);
                return ret;
            }

            list = list + localGetDirectionToken();
            description = description + GetDirectionNameDisplay(ref roomExit);

            if (count < AllExits().Count - 1)
            {
                description = description + ", ";
            }
            else if (count == AllExits().Count - 1)
            {
                description = description + " " + orString + " ";
            }
        }

        _game.SetStringContents("quest.doorways", description, _game._nullContext);

        if (count > 0)
        {
            description = descPrefix + " " + description + ".";
        }
    }

    public string GetDirectionName(ref V4Game.Direction dir)
    {
        switch (dir)
        {
            case V4Game.Direction.Out:
            {
                return "out";
            }
            case V4Game.Direction.North:
            {
                return "north";
            }
            case V4Game.Direction.South:
            {
                return "south";
            }
            case V4Game.Direction.East:
            {
                return "east";
            }
            case V4Game.Direction.West:
            {
                return "west";
            }
            case V4Game.Direction.NorthWest:
            {
                return "northwest";
            }
            case V4Game.Direction.NorthEast:
            {
                return "northeast";
            }
            case V4Game.Direction.SouthWest:
            {
                return "southwest";
            }
            case V4Game.Direction.SouthEast:
            {
                return "southeast";
            }
            case V4Game.Direction.Up:
            {
                return "up";
            }
            case V4Game.Direction.Down:
            {
                return "down";
            }
        }

        return null;
    }

    public V4Game.Direction GetDirectionEnum(ref string dir)
    {
        switch (dir ?? "")
        {
            case "out":
            {
                return V4Game.Direction.Out;
            }
            case "north":
            {
                return V4Game.Direction.North;
            }
            case "south":
            {
                return V4Game.Direction.South;
            }
            case "east":
            {
                return V4Game.Direction.East;
            }
            case "west":
            {
                return V4Game.Direction.West;
            }
            case "northwest":
            {
                return V4Game.Direction.NorthWest;
            }
            case "northeast":
            {
                return V4Game.Direction.NorthEast;
            }
            case "southwest":
            {
                return V4Game.Direction.SouthWest;
            }
            case "southeast":
            {
                return V4Game.Direction.SouthEast;
            }
            case "up":
            {
                return V4Game.Direction.Up;
            }
            case "down":
            {
                return V4Game.Direction.Down;
            }
        }

        return V4Game.Direction.None;
    }

    public string GetDirectionToken(ref V4Game.Direction dir)
    {
        switch (dir)
        {
            case V4Game.Direction.Out:
            {
                return "o";
            }
            case V4Game.Direction.North:
            {
                return "n";
            }
            case V4Game.Direction.South:
            {
                return "s";
            }
            case V4Game.Direction.East:
            {
                return "e";
            }
            case V4Game.Direction.West:
            {
                return "w";
            }
            case V4Game.Direction.NorthWest:
            {
                return "b";
            }
            case V4Game.Direction.NorthEast:
            {
                return "a";
            }
            case V4Game.Direction.SouthWest:
            {
                return "d";
            }
            case V4Game.Direction.SouthEast:
            {
                return "c";
            }
            case V4Game.Direction.Up:
            {
                return "u";
            }
            case V4Game.Direction.Down:
            {
                return "f";
            }
        }

        return null;
    }

    public string GetDirectionNameDisplay(ref RoomExit roomExit)
    {
        if (roomExit.GetDirection() != V4Game.Direction.None)
        {
            var argdir = roomExit.GetDirection();
            var dir = GetDirectionName(ref argdir);
            return "|b" + dir + "|xb";
        }

        var sDisplay = "|b" + roomExit.GetDisplayName() + "|xb";
        if (Strings.Len(roomExit.GetPrefix()) > 0)
        {
            sDisplay = roomExit.GetPrefix() + " " + sDisplay;
        }

        return "to " + sDisplay;
    }

    private RoomExit GetExitByObjectId(ref int id)
    {
        foreach (var kvp in AllExits())
        {
            if (kvp.Value.GetObjId() == id)
            {
                return kvp.Value;
            }
        }

        return null;
    }

    private Dictionary<object, RoomExit> AllExits()
    {
        if (!_regenerateAllExits)
        {
            return _allExits;
        }

        _allExits = new Dictionary<object, RoomExit>();

        foreach (var dir in _directions.Keys)
        {
            var roomExit = _directions[dir];
            if (_game._objs[roomExit.GetObjId()].Exists)
            {
                _allExits.Add(dir, _directions[dir]);
            }
        }

        foreach (var dir in _places.Keys)
        {
            var roomExit = _places[dir];
            if (_game._objs[roomExit.GetObjId()].Exists)
            {
                _allExits.Add(dir, _places[dir]);
            }
        }

        return _allExits;
    }

    public void RemoveExit(ref RoomExit roomExit)
    {
        // Don't remove directional exits, as if they're recreated
        // a new object will be created which will have the same name
        // as the old one. This is because we can't delete objects yet...

        if (roomExit.GetDirection() == V4Game.Direction.None)
        {
            if (_places.ContainsKey(roomExit.GetToRoom()))
            {
                _places.Remove(roomExit.GetToRoom());
            }
        }

        _game._objs[roomExit.GetObjId()].Exists = false;
        _regenerateAllExits = true;
    }
}