Option Strict On
Option Explicit On
Option Infer On

Imports TextAdventures.Quest.LegacyASL.LegacyGame
Imports System.Collections.Generic

Friend Class RoomExits

    Private _directions As New Dictionary(Of Direction, RoomExit)
    Private _places As New Dictionary(Of String, RoomExit)
    Private _objId As Integer
    Private _allExits As Dictionary(Of Object, RoomExit)
    Private _regenerateAllExits As Boolean
    Private _game As LegacyGame

    Public Sub New(game As LegacyGame)
        _game = game
        _regenerateAllExits = True
    End Sub

    Private Sub SetDirection(ByRef direction As Direction, ByRef roomExit As RoomExit)

        If _directions.ContainsKey(direction) Then
            roomExit = _directions.Item(direction)
            _game._objs(roomExit.ObjId).Exists = True
        Else
            roomExit = New RoomExit(_game)
            _directions.Add(direction, roomExit)
        End If

        _regenerateAllExits = True

    End Sub

    Public Function GetDirectionExit(ByRef direction As Direction) As RoomExit
        If _directions.ContainsKey(direction) Then
            Return _directions.Item(direction)
        End If
        Return Nothing
    End Function

    Public Sub AddPlaceExit(ByRef roomExit As RoomExit)

        If _places.ContainsKey(roomExit.ToRoom) Then
            Dim removeItem As RoomExit = _places.Item(roomExit.ToRoom)
            RemoveExit(removeItem)
        End If

        _places.Add(roomExit.ToRoom, roomExit)
        _regenerateAllExits = True

    End Sub

    Public Sub AddExitFromTag(tag As String)

        Dim thisDir As Direction
        Dim roomExit As RoomExit = Nothing
        Dim params As String() = New String(0) {}
        Dim afterParam As String
        Dim param As Boolean

        If _game.BeginsWith(tag, "out ") Then
            tag = _game.GetEverythingAfter(tag, "out ")
            thisDir = Direction.Out
        ElseIf _game.BeginsWith(tag, "east ") Then
            tag = _game.GetEverythingAfter(tag, "east ")
            thisDir = Direction.East
        ElseIf _game.BeginsWith(tag, "west ") Then
            tag = _game.GetEverythingAfter(tag, "west ")
            thisDir = Direction.West
        ElseIf _game.BeginsWith(tag, "north ") Then
            tag = _game.GetEverythingAfter(tag, "north ")
            thisDir = Direction.North
        ElseIf _game.BeginsWith(tag, "south ") Then
            tag = _game.GetEverythingAfter(tag, "south ")
            thisDir = Direction.South
        ElseIf _game.BeginsWith(tag, "northeast ") Then
            tag = _game.GetEverythingAfter(tag, "northeast ")
            thisDir = Direction.NorthEast
        ElseIf _game.BeginsWith(tag, "northwest ") Then
            tag = _game.GetEverythingAfter(tag, "northwest ")
            thisDir = Direction.NorthWest
        ElseIf _game.BeginsWith(tag, "southeast ") Then
            tag = _game.GetEverythingAfter(tag, "southeast ")
            thisDir = Direction.SouthEast
        ElseIf _game.BeginsWith(tag, "southwest ") Then
            tag = _game.GetEverythingAfter(tag, "southwest ")
            thisDir = Direction.SouthWest
        ElseIf _game.BeginsWith(tag, "up ") Then
            tag = _game.GetEverythingAfter(tag, "up ")
            thisDir = Direction.Up
        ElseIf _game.BeginsWith(tag, "down ") Then
            tag = _game.GetEverythingAfter(tag, "down ")
            thisDir = Direction.Down
        ElseIf _game.BeginsWith(tag, "place ") Then
            tag = _game.GetEverythingAfter(tag, "place ")
            thisDir = Direction.None
        Else
            Exit Sub
        End If

        If thisDir <> Direction.None Then
            ' This will reuse an existing Exit object if we're resetting
            ' the destination of an existing directional exit.
            SetDirection(thisDir, roomExit)
        Else
            roomExit = New RoomExit(_game)
        End If

        roomExit.Parent = Me
        roomExit.Direction = thisDir

        If _game.BeginsWith(tag, "locked ") Then
            roomExit.IsLocked = True
            tag = _game.GetEverythingAfter(tag, "locked ")
        End If

        If Left(Trim(tag), 1) = "<" Then
            params = Split(_game.GetParameter(tag, _game._nullContext), ";")
            afterParam = Trim(Mid(tag, InStr(tag, ">") + 1))
            param = True
        Else
            afterParam = tag
        End If

        If Len(afterParam) > 0 Then
            ' Script exit
            roomExit.Script = afterParam

            If thisDir = Direction.None Then
                ' A place exit with a script still has a ToRoom
                roomExit.ToRoom = params(0)

                ' and may have a lock message
                If UBound(params) > 0 Then
                    roomExit.LockMessage = params(1)
                End If
            Else
                ' A directional exit with a script may have no parameter.
                ' If it does have a parameter it will be a lock message.
                If param Then
                    roomExit.LockMessage = params(0)
                End If
            End If
        Else
            roomExit.ToRoom = params(0)
            If UBound(params) > 0 Then
                roomExit.LockMessage = params(1)
            End If
        End If

        If thisDir = Direction.None Then
            AddPlaceExit(roomExit)
        End If

    End Sub

    Friend Sub AddExitFromCreateScript(script As String, ByRef ctx As Context)
        ' sScript is the "create exit ..." script, but without the "create exit" at the beginning.
        ' So it's very similar to creating an exit from a tag, except we have the source room
        ' name before the semicolon (which we don't even care about as we ARE the source room).

        Dim param As String
        Dim params As String()
        Dim paramStart As Integer
        Dim paramEnd As Integer

        ' Just need to convert e.g.
        '   create exit <src_room; dest_room> { script }
        ' to
        '   place <dest_room> { script }
        ' And
        '   create exit north <src_room> { script }
        ' to
        '   north { script }
        ' And
        '   create exit north <src_room; dest_room>
        ' to
        '   north <dest_room>

        param = _game.GetParameter(script, ctx)
        params = Split(param, ";")

        paramStart = InStr(script, "<")
        paramEnd = InStr(paramStart, script, ">")

        If paramStart > 1 Then
            ' Directional exit
            If UBound(params) = 0 Then
                ' Script directional exit
                AddExitFromTag(Trim(Left(script, paramStart - 1)) & " " & Trim(Mid(script, paramEnd + 1)))
            Else
                ' "Normal" directional exit
                AddExitFromTag(Trim(Left(script, paramStart - 1)) & " <" & Trim(params(1)) & ">")
            End If
        Else
            If UBound(params) < 1 Then
                _game.LogASLError("No exit destination given in 'create exit " & script & "'", LogType.WarningError)
                Exit Sub
            End If

            ' Place exit so add "place" tag at the beginning
            AddExitFromTag("place <" & Trim(params(1)) & Mid(script, paramEnd))
        End If

    End Sub

    Public Property ObjId() As Integer
        Get
            ObjId = _objId
        End Get
        Set(Value As Integer)
            _objId = Value
        End Set
    End Property

    Public ReadOnly Property Places() As Dictionary(Of String, RoomExit)
        Get
            Places = _places
        End Get
    End Property

    Friend Sub ExecuteGo(cmd As String, ByRef ctx As Context)
        ' This will handle "n", "go east", "go [to] library" etc.

        Dim lExitID As Integer
        Dim oExit As RoomExit

        If _game.BeginsWith(cmd, "go to ") Then
            cmd = _game.GetEverythingAfter(cmd, "go to ")
        ElseIf _game.BeginsWith(cmd, "go ") Then
            cmd = _game.GetEverythingAfter(cmd, "go ")
        End If

        lExitID = _game.Disambiguate(cmd, _game._currentRoom, ctx, True)

        If lExitID = -1 Then
            _game.PlayerErrorMessage(PlayerError.BadPlace, ctx)
        Else
            oExit = GetExitByObjectId(lExitID)
            oExit.Go(ctx)
        End If

    End Sub

    Friend Sub GetAvailableDirectionsDescription(ByRef description As String, ByRef list As String)

        Dim roomExit As RoomExit
        Dim count As Integer
        Dim descPrefix As String
        Dim orString As String

        descPrefix = "You can go"
        orString = "or"

        list = ""
        count = 0

        For Each kvp As KeyValuePair(Of Object, RoomExit) In AllExits()
            count = count + 1
            roomExit = kvp.Value

            list = list & GetDirectionToken((roomExit.Direction))
            description = description & GetDirectionNameDisplay(roomExit)

            If count < AllExits.Count - 1 Then
                description = description & ", "
            ElseIf count = AllExits.Count - 1 Then
                description = description & " " & orString & " "
            End If
        Next

        _game.SetStringContents("quest.doorways", description, _game._nullContext)

        If count > 0 Then
            description = descPrefix & " " & description & "."
        End If

    End Sub

    Public Function GetDirectionName(ByRef dir As Direction) As String
        Select Case dir
            Case Direction.Out
                Return "out"
            Case Direction.North
                Return "north"
            Case Direction.South
                Return "south"
            Case Direction.East
                Return "east"
            Case Direction.West
                Return "west"
            Case Direction.NorthWest
                Return "northwest"
            Case Direction.NorthEast
                Return "northeast"
            Case Direction.SouthWest
                Return "southwest"
            Case Direction.SouthEast
                Return "southeast"
            Case Direction.Up
                Return "up"
            Case Direction.Down
                Return "down"
        End Select

        Return Nothing
    End Function

    Public Function GetDirectionEnum(ByRef dir As String) As Direction
        Select Case dir
            Case "out"
                Return Direction.Out
            Case "north"
                Return Direction.North
            Case "south"
                Return Direction.South
            Case "east"
                Return Direction.East
            Case "west"
                Return Direction.West
            Case "northwest"
                Return Direction.NorthWest
            Case "northeast"
                Return Direction.NorthEast
            Case "southwest"
                Return Direction.SouthWest
            Case "southeast"
                Return Direction.SouthEast
            Case "up"
                Return Direction.Up
            Case "down"
                Return Direction.Down
        End Select
        Return Direction.None
    End Function

    Public Function GetDirectionToken(ByRef dir As Direction) As String
        Select Case dir
            Case Direction.Out
                Return "o"
            Case Direction.North
                Return "n"
            Case Direction.South
                Return "s"
            Case Direction.East
                Return "e"
            Case Direction.West
                Return "w"
            Case Direction.NorthWest
                Return "b"
            Case Direction.NorthEast
                Return "a"
            Case Direction.SouthWest
                Return "d"
            Case Direction.SouthEast
                Return "c"
            Case Direction.Up
                Return "u"
            Case Direction.Down
                Return "f"
        End Select

        Return Nothing
    End Function

    Public Function GetDirectionNameDisplay(ByRef roomExit As RoomExit) As String
        If roomExit.Direction <> Direction.None Then
            Dim dir = GetDirectionName((roomExit.Direction))
            Return "|b" & dir & "|xb"
        End If

        Dim sDisplay = "|b" & roomExit.DisplayName & "|xb"
        If Len(roomExit.Prefix) > 0 Then
            sDisplay = roomExit.Prefix & " " & sDisplay
        End If
        Return "to " & sDisplay
    End Function

    Private Function GetExitByObjectId(ByRef id As Integer) As RoomExit
        For Each kvp As KeyValuePair(Of Object, RoomExit) In AllExits()
            If kvp.Value.ObjId = id Then
                Return kvp.Value
            End If
        Next
        Return Nothing
    End Function

    Private Function AllExits() As Dictionary(Of Object, RoomExit)
        If Not _regenerateAllExits Then
            AllExits = _allExits
            Exit Function
        End If

        _allExits = New Dictionary(Of Object, RoomExit)

        For Each dir As Direction In _directions.Keys
            Dim roomExit = _directions.Item(dir)
            If _game._objs(roomExit.ObjId).Exists Then
                _allExits.Add(dir, _directions.Item(dir))
            End If
        Next

        For Each dir As String In _places.Keys
            Dim roomExit = _places.Item(dir)
            If _game._objs(roomExit.ObjId).Exists Then
                _allExits.Add(dir, _places.Item(dir))
            End If
        Next

        Return _allExits
    End Function

    Public Sub RemoveExit(ByRef roomExit As RoomExit)
        ' Don't remove directional exits, as if they're recreated
        ' a new object will be created which will have the same name
        ' as the old one. This is because we can't delete objects yet...

        If roomExit.Direction = Direction.None Then
            If _places.ContainsKey(roomExit.ToRoom) Then
                _places.Remove(roomExit.ToRoom)
            End If
        End If

        _game._objs(roomExit.ObjId).Exists = False
        _regenerateAllExits = True
    End Sub
End Class