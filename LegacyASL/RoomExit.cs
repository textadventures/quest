using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace TextAdventures.Quest.LegacyASL
{
    internal class RoomExit
    {
        private int _objId;
        private int _roomId;
        private LegacyASL.LegacyGame.Direction _direction;
        private LegacyASL.RoomExits _parent;
        private string _objName;
        private string _displayName; // this could be a place exit's alias
        private LegacyASL.LegacyGame _game;

        public RoomExit(LegacyASL.LegacyGame game)
        {
            _game = game;
            game._numberObjs = game._numberObjs + 1;
            Array.Resize(ref game._objs, game._numberObjs + 1);
            game._objs[game._numberObjs] = new LegacyASL.LegacyGame.ObjectType();
            _objId = game._numberObjs;
            var o = game._objs[_objId];
            o.IsExit = true;
            o.Visible = true;
            o.Exists = true;
        }

        private void SetExitProperty(string propertyName, string value)
        {
            _game.AddToObjectProperties(propertyName + "=" + value, _objId, _game._nullContext);
        }

        private string GetExitProperty(string propertyName)
        {
            return _game.GetObjectProperty(propertyName, _objId, false, false);
        }

        private void SetExitPropertyBool(string propertyName, bool value)
        {
            string sPropertyString;
            sPropertyString = propertyName;
            if (!value)
                sPropertyString = "not " + sPropertyString;
            _game.AddToObjectProperties(sPropertyString, _objId, _game._nullContext);
        }

        private bool GetExitPropertyBool(string propertyName)
        {
            return _game.GetObjectProperty(propertyName, _objId, true, false) == "yes";
        }

        private void SetAction(string actionName, string value)
        {
            _game.AddToObjectActions("<" + actionName + "> " + value, _objId, _game._nullContext);
        }

        public void SetToRoom(string value)
        {
            SetExitProperty("to", value);
            UpdateObjectName();
        }

        public string GetToRoom()
        {
            return GetExitProperty("to");
        }

        public void SetPrefix(string value)
        {
            SetExitProperty("prefix", value);
        }

        public string GetPrefix()
        {
            return GetExitProperty("prefix");
        }

        public void SetScript(string value)
        {
            if (Strings.Len(value) > 0)
            {
                SetAction("script", value);
            }
        }

        private bool IsScript()
        {
            return _game.HasAction(_objId, "script");
        }

        public void SetDirection(LegacyASL.LegacyGame.Direction value)
        {
            _direction = value;
            if (value != LegacyASL.LegacyGame.Direction.None)
                UpdateObjectName();
        }

        public LegacyASL.LegacyGame.Direction GetDirection()
        {
            return _direction;
        }

        public void SetParent(LegacyASL.RoomExits value)
        {
            _parent = value;
        }

        public LegacyASL.RoomExits GetParent()
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

        public void SetIsLocked(bool value)
        {
            SetExitPropertyBool("locked", value);
        }

        public bool GetIsLocked()
        {
            return GetExitPropertyBool("locked");
        }

        public void SetLockMessage(string value)
        {
            SetExitProperty("lockmessage", value);
        }

        public string GetLockMessage()
        {
            return GetExitProperty("lockmessage");
        }

        private void RunAction(ref string actionName, ref LegacyASL.LegacyGame.Context ctx)
        {
            _game.DoAction(_objId, actionName, ctx);
        }

        internal void RunScript(ref LegacyASL.LegacyGame.Context ctx)
        {
            string argactionName = "script";
            RunAction(ref argactionName, ref ctx);
        }

        private void UpdateObjectName()
        {

            string objName;
            var lastExitId = default(int);
            string parentRoom;

            if (Strings.Len(_objName) > 0)
                return;
            if (_parent is null)
                return;

            parentRoom = _game._objs[_parent.GetObjId()].ObjectName;

            objName = parentRoom;

            if (_direction != LegacyASL.LegacyGame.Direction.None)
            {
                objName = objName + "." + _parent.GetDirectionName(ref _direction);
                _game._objs[_objId].ObjectAlias = _parent.GetDirectionName(ref _direction);
            }
            else
            {
                string lastExitIdString = _game.GetObjectProperty("quest.lastexitid", _parent.GetObjId(), logError: false);
                if (lastExitIdString.Length == 0)
                {
                    lastExitId = 0;
                }
                else
                {
                    lastExitId = Conversions.ToInteger(lastExitIdString);
                }
                lastExitId = lastExitId + 1;
                _game.AddToObjectProperties("quest.lastexitid=" + lastExitId.ToString(), _parent.GetObjId(), _game._nullContext);
                objName = objName + ".exit" + lastExitId.ToString();

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
                this.SetPrefix(_game._rooms[GetRoomId()].Prefix);

            }

            _game._objs[_objId].ObjectName = objName;
            _game._objs[_objId].ContainerRoom = parentRoom;

            _objName = objName;

        }

        internal void Go(ref LegacyASL.LegacyGame.Context ctx)
        {
            if (GetIsLocked())
            {
                if (GetExitPropertyBool("lockmessage"))
                {
                    _game.Print(GetExitProperty("lockmessage"), ctx);
                }
                else
                {
                    _game.PlayerErrorMessage(LegacyASL.LegacyGame.PlayerError.Locked, ctx);
                }
            }
            else if (IsScript())
            {
                RunScript(ref ctx);
            }
            else
            {
                _game.PlayGame(GetToRoom(), ctx);
            }
        }
    }
}