Imports TextAdventures.Utility.Language.L

Public Class EditBrowser
    Private m_recentItems As RecentItems

    Public Event EditGame(filename As String)
    Public Event CreateNewGame()
    Public Event OpenGame()
    Public Event Tutorial()

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        m_recentItems = New RecentItems("EditorRecent")
        ctlGameList.LaunchCaption = T("LauncherEdit")
        Populate()

        AddHandler ctlEditorWelcome.CreateNewGame, AddressOf DoCreateNewGame
        AddHandler ctlEditorWelcome.OpenGame, AddressOf DoOpenGame
        AddHandler ctlEditorWelcome.Tutorial, AddressOf DoTutorial

        AddHandler ctlGameList.Launch, AddressOf ctlGameList_Launch
        AddHandler ctlGameList.ClearAllItems, AddressOf ctlGameList_ClearAllItems
        AddHandler ctlGameList.RemoveItem, AddressOf ctlGameList_RemoveItem
    End Sub

    Public Sub AddToRecent(filename As String, name As String)
        m_recentItems.AddToRecent(filename, name)
    End Sub

    Private Sub ctlGameList_Launch(filename As String)
        RaiseEvent EditGame(filename)
    End Sub

    Private Sub ctlGameList_ClearAllItems()
        m_recentItems.Clear()
        Populate()
    End Sub

    Private Sub ctlGameList_RemoveItem(filename As String)
        m_recentItems.Remove(filename)
    End Sub

    Public Sub Populate()
        m_recentItems.PopulateGameList(ctlGameList)
    End Sub

    Private Sub DoCreateNewGame()
        RaiseEvent CreateNewGame()
    End Sub

    Private Sub DoOpenGame()
        RaiseEvent OpenGame()
    End Sub

    Private Sub DoTutorial()
        RaiseEvent Tutorial()
    End Sub

    Private Sub lblRecent_Initialized(sender As Object, e As EventArgs) Handles lblRecent.Initialized
        lblRecent.Content = T("LauncherRecent")
    End Sub
End Class
