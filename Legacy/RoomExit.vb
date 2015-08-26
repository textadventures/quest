Option Strict On
Option Explicit On
Option Infer On

Imports TextAdventures.Quest.LegacyASL.LegacyGame

Friend Class RoomExit

    Public Id As String

    Private _objId As Integer
    Private _roomId As Integer
    Private _direction As LegacyGame.Direction
    Private _parent As RoomExits
    Private _objName As String
    Private _displayName As String ' this could be a place exit's alias
    Private _game As LegacyGame

    Public Sub New(game As LegacyGame)
        _game = game
        game._numberObjs = game._numberObjs + 1
        ReDim Preserve game._objs(game._numberObjs)
        game._objs(game._numberObjs) = New ObjectType
        _objId = game._numberObjs
        Dim o = game._objs(_objId)
        o.IsExit = True
        o.Visible = True
        o.Exists = True
    End Sub

    Private Sub SetExitProperty(propertyName As String, value As String)
        _game.AddToObjectProperties(propertyName & "=" & value, _objId, _game._nullContext)
    End Sub

    Private Function GetExitProperty(propertyName As String) As String
        Return _game.GetObjectProperty(propertyName, _objId, False, False)
    End Function

    Private Sub SetExitPropertyBool(propertyName As String, value As Boolean)
        Dim sPropertyString As String
        sPropertyString = propertyName
        If Not value Then sPropertyString = "not " & sPropertyString
        _game.AddToObjectProperties(sPropertyString, _objId, _game._nullContext)
    End Sub

    Private Function GetExitPropertyBool(propertyName As String) As Boolean
        Return (_game.GetObjectProperty(propertyName, _objId, True, False) = "yes")
    End Function

    Private Sub SetAction(actionName As String, value As String)
        _game.AddToObjectActions("<" & actionName & "> " & value, _objId, _game._nullContext)
    End Sub

    Public Sub SetToRoom(value As String)
        SetExitProperty("to", value)
        UpdateObjectName()
    End Sub

    Public Function GetToRoom() As String
        Return GetExitProperty("to")
    End Function

    Public Sub SetPrefix(value As String)
        SetExitProperty("prefix", value)
    End Sub

    Public Function GetPrefix() As String
        Return GetExitProperty("prefix")
    End Function

    Public Sub SetScript(value As String)
        If Len(Value) > 0 Then
            SetAction("script", Value)
        End If
    End Sub

    Private Function IsScript() As Boolean
        Return _game.HasAction(_objId, "script")
    End Function

    Public Sub SetDirection(value As Direction)
        _direction = value
        If value <> LegacyGame.Direction.None Then UpdateObjectName()
    End Sub

    Public Function GetDirection() As Direction
        Return _direction
    End Function

    Public Sub SetParent(value As RoomExits)
        _parent = value
    End Sub

    Public Function GetParent() As RoomExits
        Return _parent
    End Function

    Public Function GetObjId() As Integer
        Return _objId
    End Function

    Private Function GetRoomId() As Integer
        If _roomId = 0 Then
            _roomId = _game.GetRoomID(GetToRoom(), _game._nullContext)
        End If

        Return _roomId
    End Function

    Public Function GetDisplayName() As String
        Return _displayName
    End Function

    Public Function GetDisplayText() As String
        Return _displayName
    End Function

    Public Sub SetIsLocked(value As Boolean)
        SetExitPropertyBool("locked", value)
    End Sub

    Public Function GetIsLocked() As Boolean
        Return GetExitPropertyBool("locked")
    End Function

    Public Sub SetLockMessage(value As String)
        SetExitProperty("lockmessage", value)
    End Sub

    Public Function GetLockMessage() As String
        Return GetExitProperty("lockmessage")
    End Function

    Private Sub RunAction(ByRef actionName As String, ByRef ctx As Context)
        _game.DoAction(_objId, actionName, ctx)
    End Sub

    Friend Sub RunScript(ByRef ctx As Context)
        RunAction("script", ctx)
    End Sub

    Private Sub UpdateObjectName()

        Dim objName As String
        Dim lastExitId As Integer
        Dim parentRoom As String

        If Len(_objName) > 0 Then Exit Sub
        If _parent Is Nothing Then Exit Sub

        parentRoom = _game._objs(_parent.GetObjId()).ObjectName

        objName = parentRoom

        If _direction <> LegacyGame.Direction.None Then
            objName = objName & "." & _parent.GetDirectionName(_direction)
            _game._objs(_objId).ObjectAlias = _parent.GetDirectionName(_direction)
        Else
            Dim lastExitIdString As String = _game.GetObjectProperty("quest.lastexitid", (_parent.GetObjId()), , False)
            If lastExitIdString.Length = 0 Then
                lastExitId = 0
            Else
                lastExitId = CInt(lastExitId)
            End If
            lastExitId = lastExitId + 1
            _game.AddToObjectProperties("quest.lastexitid=" & CStr(lastExitId), (_parent.GetObjId()), _game._nullContext)
            objName = objName & ".exit" & CStr(lastExitId)

            If GetRoomId() = 0 Then
                ' the room we're pointing at might not exist, especially if this is a script exit
                _displayName = GetToRoom()
            Else
                If Len(_game._rooms(GetRoomId()).RoomAlias) > 0 Then
                    _displayName = _game._rooms(GetRoomId()).RoomAlias
                Else
                    _displayName = GetToRoom()
                End If
            End If

            _game._objs(_objId).ObjectAlias = _displayName
            SetPrefix(_game._rooms(GetRoomId()).Prefix)

        End If

        _game._objs(_objId).ObjectName = objName
        _game._objs(_objId).ContainerRoom = parentRoom

        _objName = objName

    End Sub

    Friend Sub Go(ByRef ctx As Context)
        If GetIsLocked() Then
            If GetExitPropertyBool("lockmessage") Then
                _game.Print(GetExitProperty("lockmessage"), ctx)
            Else
                _game.PlayerErrorMessage(PlayerError.Locked, ctx)
            End If
        Else
            If IsScript() Then
                RunScript(ctx)
            Else
                _game.PlayGame(GetToRoom(), ctx)
            End If
        End If
    End Sub
End Class