Public Class EditBrowser
    Private m_recentItems As RecentItems

    Public Event EditGame(ByVal filename As String)

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        m_recentItems = New RecentItems("EditorRecent")
        ctlGameList.LaunchCaption = "Edit"
        m_recentItems.PopulateGameList(ctlGameList)
    End Sub

    Public Sub AddToRecent(ByVal filename As String, ByVal name As String)
        m_recentItems.AddToRecent(filename, name)
    End Sub

    Private Sub ctlGameList_Launch(ByVal filename As String) Handles ctlGameList.Launch
        RaiseEvent EditGame(filename)
    End Sub
End Class
