Public Class PlayBrowser
    Private m_recentItems As RecentItems
    Private WithEvents m_onlineGames As New OnlineGames
    Private m_initialised As Boolean = False

    Public Event LaunchGame(filename As String)
    Public Event GotUpdateData(data As UpdatesData)

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        m_recentItems = New RecentItems("Recent")
        ctlGameList.LaunchCaption = "Play"
        ctlOnlineGameList.LaunchCaption = "Play"
        ctlOnlineGameList.IsOnlineList = True
        Populate()
    End Sub

    Public Sub AddToRecent(filename As String, name As String)
        m_recentItems.AddToRecent(filename, name)
    End Sub

    Private Sub ctlGameList_Launch(filename As String) Handles ctlGameList.Launch
        RaiseEvent LaunchGame(filename)
    End Sub

    Private Sub ctlGameList_ClearAllItems() Handles ctlGameList.ClearAllItems
        m_recentItems.Clear()
        Populate()
    End Sub

    Private Sub ctlGameList_RemoveItem(filename As String) Handles ctlGameList.RemoveItem
        m_recentItems.Remove(filename)
    End Sub

    Private Sub ctlOnlineGameList_Launch(filename As String) Handles ctlOnlineGameList.Launch
        RaiseEvent LaunchGame(filename)
    End Sub

    Public Sub Populate()
        m_recentItems.PopulateGameList(ctlGameList)
    End Sub

    Public Sub MainWindowShown()
        m_initialised = True
        ctlOnlineGameList.IsDownloading = True
        m_onlineGames.StartDownloadGameData()
    End Sub

    Private Sub m_onlineGames_DataReady() Handles m_onlineGames.DataReady
        Dispatcher.BeginInvoke(Sub()
                                   ctlOnlineGameList.IsDownloading = False
                                   PopulateCategories()
                               End Sub)
    End Sub

    Private Sub m_onlineGames_DownloadFailed() Handles m_onlineGames.DownloadFailed
        Dispatcher.BeginInvoke(Sub() ctlOnlineGameList.MarkAsFailed())
    End Sub

    Private Sub PopulateCategories()
        ctlBrowseFilter.Populate((From cat In m_onlineGames.Categories Select cat.Title).ToArray())
    End Sub

    Private Sub ctlBrowseFilter_CategoryChanged(category As String) Handles ctlBrowseFilter.CategoryChanged
        SetDescriptionVisible(False)
        PopulateGames(category)
    End Sub

    Private Sub PopulateGames(category As String)
        m_onlineGames.PopulateGameList(category, ctlOnlineGameList)
    End Sub

    Private Sub m_onlineGames_GotUpdateData(data As UpdatesData) Handles m_onlineGames.GotUpdateData
        RaiseEvent GotUpdateData(data)
    End Sub

    Private Sub ctlOnlineGameList_ShowGameDescription(data As GameListItemData, control As GameListItem) Handles ctlOnlineGameList.ShowGameDescription
        ctlGameDescription.Populate(data, control)
        SetDescriptionVisible(True)
    End Sub

    Private Sub SetDescriptionVisible(visible As Boolean)
        Dim gameListVisibility As Windows.Visibility = If(visible, Windows.Visibility.Collapsed, Windows.Visibility.Visible)
        Dim descriptionVisibility As Windows.Visibility = If(visible, Windows.Visibility.Visible, Windows.Visibility.Collapsed)
        lblRecent.Visibility = gameListVisibility
        ctlGameList.Visibility = gameListVisibility
        ctlGameDescription.Visibility = descriptionVisibility
    End Sub

    Private Sub ctlGameDescription_Close() Handles ctlGameDescription.Close
        SetDescriptionVisible(False)
        ctlOnlineGameList.UnselectCurrentItem()
    End Sub

    Public Property DownloadFolder As String
        Get
            Return ctlOnlineGameList.DownloadFolder
        End Get
        Set(value As String)
            ctlOnlineGameList.DownloadFolder = value
            Refresh()
        End Set
    End Property

    Private Sub Refresh()
        If m_initialised Then
            PopulateGames(ctlBrowseFilter.Category)
        End If
    End Sub

    Public Property ShowSandpit As Boolean
        Get
            Return m_onlineGames.ShowSandpit
        End Get
        Set(value As Boolean)
            m_onlineGames.ShowSandpit = value
            RedownloadGameData()
        End Set
    End Property

    Public Property ShowAdult As Boolean
        Get
            Return m_onlineGames.ShowAdult
        End Get
        Set(value As Boolean)
            m_onlineGames.ShowAdult = value
            RedownloadGameData()
        End Set
    End Property

    Private Sub RedownloadGameData()
        If m_initialised Then
            ctlOnlineGameList.IsDownloading = True
            m_onlineGames.StartDownloadGameData()
        End If
    End Sub
End Class
