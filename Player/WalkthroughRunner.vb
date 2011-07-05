Friend Class WalkthroughRunner
    Private m_gameDebug As IASLDebug
    Private m_game As IASL
    Private m_walkthrough As String
    Private m_showingMenu As Boolean
    Private m_menuOptions As IDictionary(Of String, String)

    Public Event Output(text As String)

    Public Sub New(game As IASLDebug, walkthrough As String)
        m_gameDebug = game
        m_game = DirectCast(game, IASL)
        m_walkthrough = walkthrough
    End Sub

    Public Sub Run()
        For Each cmd As String In m_gameDebug.Walkthroughs.Walkthroughs(m_walkthrough).Steps
            If m_showingMenu Then
                SetMenuResponse(cmd)
            Else
                m_game.SendCommand(cmd)
            End If
        Next
    End Sub

    Public Sub ShowMenu(menu As MenuData)
        m_showingMenu = True
        WriteLine("Menu: " + menu.Caption)
        m_menuOptions = menu.Options
    End Sub

    Private Sub SetMenuResponse(response As String)
        If response.StartsWith("menu:") Then
            m_showingMenu = False
            Dim menuResponse As String = response.Substring(5)
            WriteLine("  - " + menuResponse)
            If (m_menuOptions.ContainsKey(menuResponse)) Then
                m_game.SetMenuResponse(menuResponse)
            Else
                Throw New Exception("Menu response was not an option")
            End If
        Else
            Throw New Exception("No menu response defined in walkthrough")
        End If
    End Sub

    Private Sub WriteLine(text As String)
        RaiseEvent Output(text)
    End Sub
End Class
