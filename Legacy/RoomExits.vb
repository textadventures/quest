Option Strict On
Option Explicit On

Imports AxeSoftware.Quest.LegacyASL.LegacyGame
Imports System.Collections.Generic

Friend Class RoomExits

    Private m_oDirections As New Dictionary(Of LegacyGame.eDirection, RoomExit)
    Private m_oPlaces As New Dictionary(Of String, RoomExit)
    Private m_lObjID As Integer
    Private m_oAllExits As Dictionary(Of Object, RoomExit)
    Private m_bRegenerateAllExits As Boolean
    Private m_game As LegacyGame

    Public Sub New(ByVal game As LegacyGame)
        m_game = game
        m_bRegenerateAllExits = True
    End Sub

    Private Sub SetDirection(ByRef Direction As LegacyGame.eDirection, ByRef oExit As RoomExit)

        If m_oDirections.ContainsKey(Direction) Then
            oExit = m_oDirections.Item(Direction)
            m_game.Objs(oExit.ObjID).Exists = True
        Else
            oExit = New RoomExit(m_game)
            m_oDirections.Add(Direction, oExit)
        End If

        m_bRegenerateAllExits = True

    End Sub

    Public Function GetDirectionExit(ByRef Direction As LegacyGame.eDirection) As RoomExit
        If m_oDirections.ContainsKey(Direction) Then
            GetDirectionExit = m_oDirections.Item(Direction)
        End If
        Return Nothing
    End Function

    Public Sub AddPlaceExit(ByRef oExit As RoomExit)

        If m_oPlaces.ContainsKey(oExit.ToRoom) Then
            RemoveExit(m_oPlaces.Item(oExit.ToRoom))
        End If

        m_oPlaces.Add(oExit.ToRoom, oExit)
        m_bRegenerateAllExits = True

    End Sub

    Public Function AddExitFromTag(ByVal sTag As String) As Boolean

        Dim dirThis As LegacyGame.eDirection
        Dim oExit As RoomExit = Nothing
        Dim asParams(0) As String
        Dim sAfterParam As String
        Dim bParam As Boolean

        If m_game.BeginsWith(sTag, "out ") Then
            sTag = m_game.GetEverythingAfter(sTag, "out ")
            dirThis = LegacyGame.eDirection.dirOut
        ElseIf m_game.BeginsWith(sTag, "east ") Then
            sTag = m_game.GetEverythingAfter(sTag, "east ")
            dirThis = LegacyGame.eDirection.dirEast
        ElseIf m_game.BeginsWith(sTag, "west ") Then
            sTag = m_game.GetEverythingAfter(sTag, "west ")
            dirThis = LegacyGame.eDirection.dirWest
        ElseIf m_game.BeginsWith(sTag, "north ") Then
            sTag = m_game.GetEverythingAfter(sTag, "north ")
            dirThis = LegacyGame.eDirection.dirNorth
        ElseIf m_game.BeginsWith(sTag, "south ") Then
            sTag = m_game.GetEverythingAfter(sTag, "south ")
            dirThis = LegacyGame.eDirection.dirSouth
        ElseIf m_game.BeginsWith(sTag, "northeast ") Then
            sTag = m_game.GetEverythingAfter(sTag, "northeast ")
            dirThis = LegacyGame.eDirection.dirNorthEast
        ElseIf m_game.BeginsWith(sTag, "northwest ") Then
            sTag = m_game.GetEverythingAfter(sTag, "northwest ")
            dirThis = LegacyGame.eDirection.dirNorthWest
        ElseIf m_game.BeginsWith(sTag, "southeast ") Then
            sTag = m_game.GetEverythingAfter(sTag, "southeast ")
            dirThis = LegacyGame.eDirection.dirSouthEast
        ElseIf m_game.BeginsWith(sTag, "southwest ") Then
            sTag = m_game.GetEverythingAfter(sTag, "southwest ")
            dirThis = LegacyGame.eDirection.dirSouthWest
        ElseIf m_game.BeginsWith(sTag, "up ") Then
            sTag = m_game.GetEverythingAfter(sTag, "up ")
            dirThis = LegacyGame.eDirection.dirUp
        ElseIf m_game.BeginsWith(sTag, "down ") Then
            sTag = m_game.GetEverythingAfter(sTag, "down ")
            dirThis = LegacyGame.eDirection.dirDown
        ElseIf m_game.BeginsWith(sTag, "place ") Then
            sTag = m_game.GetEverythingAfter(sTag, "place ")
            dirThis = LegacyGame.eDirection.dirNone
        Else
            AddExitFromTag = False
            Exit Function
        End If

        AddExitFromTag = True

        If dirThis <> LegacyGame.eDirection.dirNone Then
            ' This will reuse an existing Exit object if we're resetting
            ' the destination of an existing directional exit.
            SetDirection(dirThis, oExit)
        Else
            oExit = New RoomExit(m_game)
        End If

        With oExit
            .Parent = Me
            .Direction = dirThis

            If m_game.BeginsWith(sTag, "locked ") Then
                .IsLocked = True
                sTag = m_game.GetEverythingAfter(sTag, "locked ")
            End If

            If Left(Trim(sTag), 1) = "<" Then
                asParams = Split(m_game.RetrieveParameter(sTag, m_game.NullThread), ";")
                sAfterParam = Trim(Mid(sTag, InStr(sTag, ">") + 1))
                bParam = True
            Else
                sAfterParam = sTag
            End If

            If Len(sAfterParam) > 0 Then
                ' Script exit
                .Script = sAfterParam

                If dirThis = LegacyGame.eDirection.dirNone Then
                    ' A place exit with a script still has a ToRoom
                    .ToRoom = asParams(0)

                    ' and may have a lock message
                    If UBound(asParams) > 0 Then
                        .LockMessage = asParams(1)
                    End If
                Else
                    ' A directional exit with a script may have no parameter.
                    ' If it does have a parameter it will be a lock message.
                    If bParam Then
                        .LockMessage = asParams(0)
                    End If
                End If
            Else
                .ToRoom = asParams(0)
                If UBound(asParams) > 0 Then
                    .LockMessage = asParams(1)
                End If
            End If

            If dirThis = LegacyGame.eDirection.dirNone Then
                AddPlaceExit(oExit)
            End If
        End With

    End Function

    Friend Function AddExitFromCreateScript(ByVal sScript As String, ByRef Thread As ThreadData) As Boolean
        ' sScript is the "create exit ..." script, but without the "create exit" at the beginning.
        ' So it's very similar to creating an exit from a tag, except we have the source room
        ' name before the semicolon (which we don't even care about as we ARE the source room).

        Dim sParam As String
        Dim asParam() As String
        Dim lParamStart As Integer
        Dim lParamEnd As Integer

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

        sParam = m_game.RetrieveParameter(sScript, Thread)
        asParam = Split(sParam, ";")

        lParamStart = InStr(sScript, "<")
        lParamEnd = InStr(lParamStart, sScript, ">")

        If lParamStart > 1 Then
            ' Directional exit
            If UBound(asParam) = 0 Then
                ' Script directional exit
                AddExitFromTag(Trim(Left(sScript, lParamStart - 1)) & " " & Trim(Mid(sScript, lParamEnd + 1)))
            Else
                ' "Normal" directional exit
                AddExitFromTag(Trim(Left(sScript, lParamStart - 1)) & " <" & Trim(asParam(1)) & ">")
            End If
        Else
            If UBound(asParam) < 1 Then
                m_game.LogASLError("No exit destination given in 'create exit " & sScript & "'", LOGTYPE_WARNINGERROR)
                Exit Function
            End If

            ' Place exit so add "place" tag at the beginning
            AddExitFromTag("place <" & Trim(asParam(1)) & Mid(sScript, lParamEnd))
        End If

    End Function


    Public Property ObjID() As Integer
        Get
            ObjID = m_lObjID
        End Get
        Set(ByVal Value As Integer)
            m_lObjID = Value
        End Set
    End Property

    Public ReadOnly Property Places() As Dictionary(Of String, RoomExit)
        Get
            Places = m_oPlaces
        End Get
    End Property

    Friend Sub ExecuteGo(ByVal sCommand As String, ByRef Thread As ThreadData)
        ' This will handle "n", "go east", "go [to] library" etc.

        Dim lExitID As Integer
        Dim oExit As RoomExit

        If m_game.BeginsWith(sCommand, "go to ") Then
            sCommand = m_game.GetEverythingAfter(sCommand, "go to ")
        ElseIf m_game.BeginsWith(sCommand, "go ") Then
            sCommand = m_game.GetEverythingAfter(sCommand, "go ")
        End If

        lExitID = m_game.Disambiguate(sCommand, m_game.CurrentRoom, Thread, True)

        If lExitID = -1 Then
            m_game.PlayerErrorMessage(ERROR_BADPLACE, Thread)
        Else
            oExit = GetExitByObjectID(lExitID)
            oExit.Go(Thread)
        End If

    End Sub

    Friend Sub GetAvailableDirectionsDescription(ByRef sDescription As String, ByRef sList As String)

        Dim oExit As RoomExit
        Dim lCount As Integer
        Dim sDescPrefix As String
        Dim sOr As String

        sDescPrefix = "You can go"
        sOr = "or"

        sList = ""
        lCount = 0

        For Each kvp As KeyValuePair(Of Object, RoomExit) In AllExits()
            lCount = lCount + 1
            oExit = kvp.Value

            sList = sList & GetDirectionToken((oExit.Direction))
            sDescription = sDescription & GetDirectionNameDisplay(oExit)

            If lCount < AllExits.Count - 1 Then
                sDescription = sDescription & ", "
            ElseIf lCount = AllExits.Count - 1 Then
                sDescription = sDescription & " " & sOr & " "
            End If
        Next

        m_game.SetStringContents("quest.doorways", sDescription, m_game.NullThread)

        If lCount > 0 Then
            sDescription = sDescPrefix & " " & sDescription & "."
        End If

    End Sub

    Public Function GetDirectionName(ByRef lDir As LegacyGame.eDirection) As String

        Dim sDir As String = ""

        Select Case lDir
            Case LegacyGame.eDirection.dirOut
                sDir = "out"
            Case LegacyGame.eDirection.dirNorth
                sDir = "north"
            Case LegacyGame.eDirection.dirSouth
                sDir = "south"
            Case LegacyGame.eDirection.dirEast
                sDir = "east"
            Case LegacyGame.eDirection.dirWest
                sDir = "west"
            Case LegacyGame.eDirection.dirNorthWest
                sDir = "northwest"
            Case LegacyGame.eDirection.dirNorthEast
                sDir = "northeast"
            Case LegacyGame.eDirection.dirSouthWest
                sDir = "southwest"
            Case LegacyGame.eDirection.dirSouthEast
                sDir = "southeast"
            Case LegacyGame.eDirection.dirUp
                sDir = "up"
            Case LegacyGame.eDirection.dirDown
                sDir = "down"
        End Select

        GetDirectionName = sDir
    End Function

    Public Function GetDirectionEnum(ByRef sDir As String) As LegacyGame.eDirection
        Select Case sDir
            Case "out"
                GetDirectionEnum = LegacyGame.eDirection.dirOut
            Case "north"
                GetDirectionEnum = LegacyGame.eDirection.dirNorth
            Case "south"
                GetDirectionEnum = LegacyGame.eDirection.dirSouth
            Case "east"
                GetDirectionEnum = LegacyGame.eDirection.dirEast
            Case "west"
                GetDirectionEnum = LegacyGame.eDirection.dirWest
            Case "northwest"
                GetDirectionEnum = LegacyGame.eDirection.dirNorthWest
            Case "northeast"
                GetDirectionEnum = LegacyGame.eDirection.dirNorthEast
            Case "southwest"
                GetDirectionEnum = LegacyGame.eDirection.dirSouthWest
            Case "southeast"
                GetDirectionEnum = LegacyGame.eDirection.dirSouthEast
            Case "up"
                GetDirectionEnum = LegacyGame.eDirection.dirUp
            Case "down"
                GetDirectionEnum = LegacyGame.eDirection.dirDown
            Case Else
                GetDirectionEnum = LegacyGame.eDirection.dirNone
        End Select
    End Function

    Public Function GetDirectionToken(ByRef lDir As LegacyGame.eDirection) As String

        Dim sDir As String = ""

        Select Case lDir
            Case LegacyGame.eDirection.dirOut
                sDir = "o"
            Case LegacyGame.eDirection.dirNorth
                sDir = "n"
            Case LegacyGame.eDirection.dirSouth
                sDir = "s"
            Case LegacyGame.eDirection.dirEast
                sDir = "e"
            Case LegacyGame.eDirection.dirWest
                sDir = "w"
            Case LegacyGame.eDirection.dirNorthWest
                sDir = "b"
            Case LegacyGame.eDirection.dirNorthEast
                sDir = "a"
            Case LegacyGame.eDirection.dirSouthWest
                sDir = "d"
            Case LegacyGame.eDirection.dirSouthEast
                sDir = "c"
            Case LegacyGame.eDirection.dirUp
                sDir = "u"
            Case LegacyGame.eDirection.dirDown
                sDir = "f"
        End Select

        GetDirectionToken = sDir
    End Function

    Public Function GetDirectionNameDisplay(ByRef oExit As RoomExit) As String

        Dim sDir As String = ""
        Dim sDisplay As String

        If oExit.Direction <> LegacyGame.eDirection.dirNone Then
            sDir = GetDirectionName((oExit.Direction))
            GetDirectionNameDisplay = "|b" & sDir & "|xb"
        Else
            sDisplay = "|b" & oExit.DisplayName & "|xb"
            If Len(oExit.Prefix) > 0 Then
                sDisplay = oExit.Prefix & " " & sDisplay
            End If
            GetDirectionNameDisplay = "to " & sDisplay
        End If

    End Function

    Private Function GetExitByObjectID(ByRef lID As Integer) As RoomExit
        Dim oExit As RoomExit

        For Each kvp As KeyValuePair(Of Object, RoomExit) In AllExits()
            oExit = kvp.Value
            If oExit.ObjID = lID Then
                GetExitByObjectID = oExit
                Exit Function
            End If
        Next

        Return Nothing

    End Function

    Private Function AllExits() As Dictionary(Of Object, RoomExit)

        Dim oAllExits As Dictionary(Of Object, RoomExit)
        Dim oExit As RoomExit

        If Not m_bRegenerateAllExits Then
            AllExits = m_oAllExits
            Exit Function
        End If

        oAllExits = New Dictionary(Of Object, RoomExit)

        For Each dir As eDirection In m_oDirections.Keys
            oExit = m_oDirections.Item(dir)
            If m_game.Objs(oExit.ObjID).Exists Then
                oAllExits.Add(dir, m_oDirections.Item(dir))
            End If
        Next

        For Each dir As String In m_oPlaces.Keys
            oExit = m_oPlaces.Item(dir)
            If m_game.Objs(oExit.ObjID).Exists Then
                oAllExits.Add(dir, m_oPlaces.Item(dir))
            End If
        Next

        m_oAllExits = oAllExits
        AllExits = oAllExits

        m_bRegenerateAllExits = False

    End Function

    Public Sub RemoveExit(ByRef oExit As RoomExit)

        ' Don't remove directional exits, as if they're recreated
        ' a new object will be created which will have the same name
        ' as the old one. This is because we can't delete objects yet...

        If oExit.Direction = LegacyGame.eDirection.dirNone Then
            If m_oPlaces.ContainsKey(oExit.ToRoom) Then
                m_oPlaces.Remove(oExit.ToRoom)
            End If
        End If

        m_game.Objs(oExit.ObjID).Exists = False

        m_bRegenerateAllExits = True

    End Sub
End Class