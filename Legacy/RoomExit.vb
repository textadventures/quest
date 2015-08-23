Option Strict On
Option Explicit On

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
        With game._objs(_objId)
            .IsExit = True
            .Visible = True
            .Exists = True
        End With
    End Sub

    ' If this code was properly object oriented, we could set up properties properly
    ' on the "object" object.
    Private Property ExitProperty(propertyName As String) As String
        Get
            ExitProperty = _game.GetObjectProperty(propertyName, _objId, False, False)
        End Get
        Set(Value As String)
            _game.AddToObjectProperties(propertyName & "=" & Value, _objId, _game._nullContext)
        End Set
    End Property

    Private Property ExitPropertyBool(propertyName As String) As Boolean
        Get
            ExitPropertyBool = (_game.GetObjectProperty(propertyName, _objId, True, False) = "yes")
        End Get
        Set(Value As Boolean)
            Dim sPropertyString As String
            sPropertyString = propertyName
            If Not Value Then sPropertyString = "not " & sPropertyString
            _game.AddToObjectProperties(sPropertyString, _objId, _game._nullContext)
        End Set
    End Property

    Private WriteOnly Property Action(actionName As String) As String
        Set(Value As String)
            _game.AddToObjectActions("<" & actionName & "> " & Value, _objId, _game._nullContext)
        End Set
    End Property


    Public Property ToRoom() As String
        Get
            ToRoom = ExitProperty("to")
        End Get
        Set(Value As String)
            ExitProperty("to") = Value
            UpdateObjectName()
        End Set
    End Property


    Public Property Prefix() As String
        Get
            Prefix = ExitProperty("prefix")
        End Get
        Set(Value As String)
            ExitProperty("prefix") = Value
        End Set
    End Property

    Public WriteOnly Property Script() As String
        Set(Value As String)
            If Len(Value) > 0 Then
                Action("script") = Value
            End If
        End Set
    End Property

    Private ReadOnly Property IsScript() As Boolean
        Get
            IsScript = _game.HasAction(_objId, "script")
        End Get
    End Property


    Public Property Direction() As LegacyGame.Direction
        Get
            Direction = _direction
        End Get
        Set(Value As LegacyGame.Direction)
            _direction = Value
            If Value <> LegacyGame.Direction.None Then UpdateObjectName()
        End Set
    End Property

    Public Property Parent() As RoomExits
        Get
            Parent = _parent
        End Get
        Set(Value As RoomExits)
            _parent = Value
        End Set
    End Property

    Public ReadOnly Property ObjId() As Integer
        Get
            ObjId = _objId
        End Get
    End Property

    Private ReadOnly Property RoomId() As Integer
        Get
            If _roomId = 0 Then
                _roomId = _game.GetRoomID(ToRoom, _game._nullContext)
            End If

            RoomId = _roomId
        End Get
    End Property

    Public ReadOnly Property DisplayName() As String
        Get
            DisplayName = _displayName
        End Get
    End Property

    Public ReadOnly Property DisplayText() As String
        Get
            DisplayText = _displayName
        End Get
    End Property


    Public Property IsLocked() As Boolean
        Get
            IsLocked = ExitPropertyBool("locked")
        End Get
        Set(Value As Boolean)
            ExitPropertyBool("locked") = Value
        End Set
    End Property

    Public Property LockMessage() As String
        Get
            LockMessage = ExitProperty("lockmessage")
        End Get
        Set(Value As String)
            ExitProperty("lockmessage") = Value
        End Set
    End Property
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

        parentRoom = _game._objs(_parent.ObjId).ObjectName

        objName = parentRoom

        If _direction <> LegacyGame.Direction.None Then
            objName = objName & "." & _parent.GetDirectionName(_direction)
            _game._objs(_objId).ObjectAlias = _parent.GetDirectionName(_direction)
        Else
            Dim lastExitIdString As String = _game.GetObjectProperty("quest.lastexitid", (_parent.ObjId), , False)
            If lastExitIdString.Length = 0 Then
                lastExitId = 0
            Else
                lastExitId = CInt(lastExitId)
            End If
            lastExitId = lastExitId + 1
            _game.AddToObjectProperties("quest.lastexitid=" & CStr(lastExitId), (_parent.ObjId), _game._nullContext)
            objName = objName & ".exit" & CStr(lastExitId)

            If RoomId = 0 Then
                ' the room we're pointing at might not exist, especially if this is a script exit
                _displayName = ToRoom
            Else
                If Len(_game._rooms(RoomId).RoomAlias) > 0 Then
                    _displayName = _game._rooms(RoomId).RoomAlias
                Else
                    _displayName = ToRoom
                End If
            End If

            _game._objs(_objId).ObjectAlias = _displayName
            Prefix = _game._rooms(RoomId).Prefix

        End If

        _game._objs(_objId).ObjectName = objName
        _game._objs(_objId).ContainerRoom = parentRoom

        _objName = objName

    End Sub

    Friend Sub Go(ByRef ctx As Context)
        If IsLocked Then
            If ExitPropertyBool("lockmessage") Then
                _game.Print(ExitProperty("lockmessage"), ctx)
            Else
                _game.PlayerErrorMessage(PlayerError.Locked, ctx)
            End If
        Else
            If IsScript Then
                RunScript(ctx)
            Else
                _game.PlayGame(ToRoom, ctx)
            End If
        End If
    End Sub
End Class