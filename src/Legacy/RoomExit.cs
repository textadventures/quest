using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace QuestViva.Legacy;

internal class RoomExit
{
    private readonly V4Game _game;
    private readonly int _objId;
    private V4Game.Direction _direction;
    private string _displayName; // this could be a place exit's alias
    private string _objName;
    private RoomExits _parent;
    private int _roomId;

    public RoomExit(V4Game game)
    {
        _game = game;
        game._numberObjs = game._numberObjs + 1;
        Array.Resize(ref game._objs, game._numberObjs + 1);
        game._objs[game._numberObjs] = new V4Game.ObjectType();
        _objId = game._numberObjs;
        var o = game._objs[_objId];
        o.IsExit = true;
        o.Visible = true;
        o.Exists = true;
    }

    private async Task SetExitProperty(string propertyName, string value)
    {
        await _game.AddToObjectProperties(propertyName + "=" + value, _objId, _game._nullContext);
    }

    private string GetExitProperty(string propertyName)
    {
        return _game.GetObjectProperty(propertyName, _objId, false, false);
    }

    private async Task SetExitPropertyBool(string propertyName, bool value)
    {
        string sPropertyString;
        sPropertyString = propertyName;
        if (!value)
        {
            sPropertyString = "not " + sPropertyString;
        }

        await _game.AddToObjectProperties(sPropertyString, _objId, _game._nullContext);
    }

    private bool GetExitPropertyBool(string propertyName)
    {
        return _game.GetObjectProperty(propertyName, _objId, true, false) == "yes";
    }

    private async Task SetAction(string actionName, string value)
    {
        await _game.AddToObjectActions("<" + actionName + "> " + value, _objId, _game._nullContext);
    }

    public async Task SetToRoom(string value)
    {
        await SetExitProperty("to", value);
        await UpdateObjectName();
    }

    public string GetToRoom()
    {
        return GetExitProperty("to");
    }

    public async Task SetPrefix(string value)
    {
        await SetExitProperty("prefix", value);
    }

    public string GetPrefix()
    {
        return GetExitProperty("prefix");
    }

    public async Task SetScript(string value)
    {
        if (Strings.Len(value) > 0)
        {
            await SetAction("script", value);
        }
    }

    private bool IsScript()
    {
        return _game.HasAction(_objId, "script");
    }

    public async Task SetDirection(V4Game.Direction value)
    {
        _direction = value;
        if (value != V4Game.Direction.None)
        {
            await UpdateObjectName();
        }
    }

    public V4Game.Direction GetDirection()
    {
        return _direction;
    }

    public void SetParent(RoomExits value)
    {
        _parent = value;
    }

    public RoomExits GetParent()
    {
        return _parent;
    }

    public int GetObjId()
    {
        return _objId;
    }

    private int GetRoomId()
    {
        if (_roomId == 0)
        {
            _roomId = _game.GetRoomID(GetToRoom(), _game._nullContext);
        }

        return _roomId;
    }

    public string GetDisplayName()
    {
        return _displayName;
    }

    public string GetDisplayText()
    {
        return _displayName;
    }

    public async Task SetIsLocked(bool value)
    {
        await SetExitPropertyBool("locked", value);
    }

    public bool GetIsLocked()
    {
        return GetExitPropertyBool("locked");
    }

    public async Task SetLockMessage(string value)
    {
        await SetExitProperty("lockmessage", value);
    }

    public string GetLockMessage()
    {
        return GetExitProperty("lockmessage");
    }

    private async Task RunAction(string actionName, V4Game.Context ctx)
    {
        await _game.DoAction(_objId, actionName, ctx);
    }

    internal async Task RunScript(V4Game.Context ctx)
    {
        var argactionName = "script";
        await RunAction(argactionName, ctx);
    }

    private async Task UpdateObjectName()
    {
        string objName;
        var lastExitId = default(int);
        string parentRoom;

        if (Strings.Len(_objName) > 0)
        {
            return;
        }

        if (_parent is null)
        {
            return;
        }

        parentRoom = _game._objs[_parent.GetObjId()].ObjectName;

        objName = parentRoom;

        if (_direction != V4Game.Direction.None)
        {
            objName = objName + "." + _parent.GetDirectionName(ref _direction);
            _game._objs[_objId].ObjectAlias = _parent.GetDirectionName(ref _direction);
        }
        else
        {
            var lastExitIdString = _game.GetObjectProperty("quest.lastexitid", _parent.GetObjId(), logError: false);
            if (lastExitIdString.Length == 0)
            {
                lastExitId = 0;
            }
            else
            {
                lastExitId = Conversions.ToInteger(lastExitIdString);
            }

            lastExitId = lastExitId + 1;
            await _game.AddToObjectProperties("quest.lastexitid=" + lastExitId, _parent.GetObjId(), _game._nullContext);
            objName = objName + ".exit" + lastExitId;

            if (GetRoomId() == 0)
            {
                // the room we're pointing at might not exist, especially if this is a script exit
                _displayName = GetToRoom();
            }
            else if (Strings.Len(_game._rooms[GetRoomId()].RoomAlias) > 0)
            {
                _displayName = _game._rooms[GetRoomId()].RoomAlias;
            }
            else
            {
                _displayName = GetToRoom();
            }

            _game._objs[_objId].ObjectAlias = _displayName;
            await SetPrefix(_game._rooms[GetRoomId()].Prefix);
        }

        _game._objs[_objId].ObjectName = objName;
        _game._objs[_objId].ContainerRoom = parentRoom;

        _objName = objName;
    }

    internal async Task Go(V4Game.Context ctx)
    {
        if (GetIsLocked())
        {
            if (GetExitPropertyBool("lockmessage"))
            {
                await _game.Print(GetExitProperty("lockmessage"), ctx);
            }
            else
            {
                await _game.PlayerErrorMessage(V4Game.PlayerError.Locked, ctx);
            }
        }
        else if (IsScript())
        {
            await RunScript(ctx);
        }
        else
        {
            await _game.PlayGame(GetToRoom(), ctx);
        }
    }
}
