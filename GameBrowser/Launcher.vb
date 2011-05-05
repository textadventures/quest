Public Class Launcher
    Public Event BrowseForGame()
    Public Event LaunchGame(filename As String)
    Public Event EditGame(filename As String)

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ctlTabs.SelectedIndex = CInt(AxeSoftware.Utility.Registry.GetSetting("Quest", "Settings", "SelectedTab", 0))
    End Sub

    Public Sub AddToRecent(filename As String, name As String)
        ctlPlayBrowser.AddToRecent(filename, name)
    End Sub

    Public Sub AddToEditorRecent(filename As String, name As String)
        ctlEditBrowser.AddToRecent(filename, name)
    End Sub

    Private Sub ctlPlayBrowser_LaunchGame(filename As String) Handles ctlPlayBrowser.LaunchGame
        RaiseEvent LaunchGame(filename)
    End Sub

    Private Sub ctlEditBrowser_EditGame(filename As String) Handles ctlEditBrowser.EditGame
        RaiseEvent EditGame(filename)
    End Sub

    Private Sub ctlTabs_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles ctlTabs.SelectedIndexChanged
        AxeSoftware.Utility.Registry.SaveSetting("Quest", "Settings", "SelectedTab", ctlTabs.SelectedIndex)
    End Sub

    Public Sub RefreshLists()
        ctlPlayBrowser.Populate()
        ctlEditBrowser.Populate()
    End Sub
End Class
