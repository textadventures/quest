Public Class EditBrowser
    Private m_recentItems As RecentItems

    Public Event EditGame(filename As String)
    Public Event CreateNewGame()
    Public Event OpenGame()
    Public Event Tutorial()

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        m_recentItems = New RecentItems("EditorRecent")
        ctlGameList.LaunchCaption = "Edit"
        Populate()

        AddHandler ctlEditorWelcome.CreateNewGame, AddressOf DoCreateNewGame
        AddHandler ctlEditorWelcome.OpenGame, AddressOf DoOpenGame
        AddHandler ctlEditorWelcome.Tutorial, AddressOf DoTutorial
    End Sub

    Private ReadOnly Property ctlEditorWelcome As EditorWelcome
        Get
            Return DirectCast(ctlElementHost.Child, EditorWelcome)
        End Get
    End Property

    Public Sub AddToRecent(filename As String, name As String)
        m_recentItems.AddToRecent(filename, name)
    End Sub

    Private Sub ctlGameList_Launch(filename As String) Handles ctlGameList.Launch
        RaiseEvent EditGame(filename)
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
End Class