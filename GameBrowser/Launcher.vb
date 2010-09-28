Public Class Launcher
    Public Event BrowseForGame()
    Public Event LaunchGame(ByVal filename As String)
    Public Event EditGame(ByVal filename As String)

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ctlTabs.SelectedIndex = AxeSoftware.Utility.Registry.GetSetting("Quest", "Settings", "SelectedTab", 0)
    End Sub

    Public Sub AddToRecent(ByVal filename As String, ByVal name As String)
        ctlPlayBrowser.AddToRecent(filename, name)
    End Sub

    Public Sub AddToEditorRecent(ByVal filename As String, ByVal name As String)
        ctlEditBrowser.AddToRecent(filename, name)
    End Sub

    Private Sub ctlPlayBrowser_LaunchGame(ByVal filename As String) Handles ctlPlayBrowser.LaunchGame
        RaiseEvent LaunchGame(filename)
    End Sub

    Private Sub ctlEditBrowser_EditGame(ByVal filename As String) Handles ctlEditBrowser.EditGame
        RaiseEvent EditGame(filename)
    End Sub

    Private Sub ctlTabs_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ctlTabs.SelectedIndexChanged
        AxeSoftware.Utility.Registry.SaveSetting("Quest", "Settings", "SelectedTab", ctlTabs.SelectedIndex)
    End Sub
End Class
