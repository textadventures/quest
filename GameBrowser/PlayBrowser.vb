Friend Class PlayBrowser
    Private m_recentItems As RecentItems

    Public Event LaunchGame(filename As String)

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        m_recentItems = New RecentItems("Recent")
        ctlGameList.LaunchCaption = "Play"
        Populate()
    End Sub

    Public Sub AddToRecent(filename As String, name As String)
        m_recentItems.AddToRecent(filename, name)
    End Sub

    Private Sub ctlGameList_Launch(filename As String) Handles ctlGameList.Launch
        RaiseEvent LaunchGame(filename)
    End Sub

    Public Sub Populate()
        m_recentItems.PopulateGameList(ctlGameList)
    End Sub
End Class
