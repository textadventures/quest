Imports System.Windows.Media

Public Class GameList
    Private m_launchCaption As String
    Private m_visibleGameListItems As New List(Of GameListItem)
    Private m_gameListItems As New Dictionary(Of String, GameListItem)
    Private m_enableContextMenu As Boolean
    Private m_isOnlineList As Boolean

    Public Event Launch(filename As String)
    Public Event RemoveItem(filename As String)
    Public Event ClearAllItems()
    Public Event ShowGameDescription(data As GameListItemData)

    Public Property LaunchCaption() As String
        Get
            Return m_launchCaption
        End Get
        Set(value As String)
            m_launchCaption = value
        End Set
    End Property

    Public Sub CreateListElements(list As List(Of GameListItemData))
        Dim newItem As GameListItem
        Dim count As Integer = 0

        ClearListElements()

        For Each data As GameListItemData In list
            count += 1

            If Not String.IsNullOrEmpty(data.DownloadFilename) AndAlso m_gameListItems.ContainsKey(data.DownloadFilename) Then
                newItem = m_gameListItems(data.DownloadFilename)
            Else
                newItem = New GameListItem
                newItem.Data = data
                newItem.GameName = data.GameName
                newItem.Author = ""

                If Not String.IsNullOrEmpty(data.DownloadFilename) Then
                    m_gameListItems.Add(data.DownloadFilename, newItem)
                End If

                If IsOnlineList Then
                    newItem.DisableContextMenu()
                    newItem.ratingBlock.Visibility = Windows.Visibility.Visible
                    newItem.infoBlock.Visibility = Windows.Visibility.Collapsed
                    newItem.Rating = data.Rating
                    newItem.IsOnlineItem = True
                End If

                If String.IsNullOrEmpty(data.Filename) Then
                    newItem.URL = data.URL

                    Dim downloadFolder As String = System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "Quest Games",
                        "Downloaded Games")

                    System.IO.Directory.CreateDirectory(downloadFolder)

                    Dim downloadFilename As String = System.IO.Path.Combine(
                        downloadFolder, data.DownloadFilename)

                    newItem.DownloadFilename = downloadFilename
                    newItem.Author = data.Author
                    newItem.GameId = data.GameId

                    If System.IO.File.Exists(downloadFilename) Then
                        ' The file has already been downloaded
                        data.Filename = downloadFilename
                    End If
                End If

                If Not String.IsNullOrEmpty(data.Filename) Then
                    newItem.GameInfo = System.IO.Path.GetFileName(data.Filename)
                    newItem.Filename = data.Filename
                    newItem.LaunchCaption = m_launchCaption
                Else
                    newItem.CurrentState = GameListItem.State.NotDownloaded
                End If

                AddHandler newItem.Launch, AddressOf LaunchHandler
                AddHandler newItem.RemoveItem, AddressOf RemoveItemHandler
                AddHandler newItem.ClearAllItems, AddressOf ClearItemsHandler
                AddHandler newItem.Clicked, AddressOf ItemClicked
            End If

            m_visibleGameListItems.Add(newItem)

            If count Mod 2 = 0 Then
                newItem.SetBackground(New SolidColorBrush(Colors.WhiteSmoke))
            Else
                newItem.SetBackground(Nothing)
            End If

            stackPanel.Children.Add(newItem)
        Next
    End Sub

    Private Sub ClearListElements()
        stackPanel.Children.Clear()
        m_visibleGameListItems.Clear()
    End Sub

    Private Sub LaunchHandler(filename As String)
        RaiseEvent Launch(filename)
    End Sub

    Private Sub ItemClicked(sender As GameListItem)
        If sender.IsOnlineItem Then
            RaiseEvent ShowGameDescription(sender.Data)
        Else
            RaiseEvent Launch(sender.Filename)
        End If
    End Sub

    Private Sub RemoveItemHandler(sender As GameListItem, filename As String)
        RaiseEvent RemoveItem(filename)
        stackPanel.Children.Remove(sender)
    End Sub

    Private Sub ClearItemsHandler()
        RaiseEvent ClearAllItems()
    End Sub

    Private Sub GameList_Loaded(sender As Object, e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        wfPictureBox.Image = My.Resources.loading
    End Sub

    Public Sub MarkAsFailed()
        loadingPanel.Visibility = Windows.Visibility.Collapsed
        errorBlock.Visibility = Windows.Visibility.Visible
    End Sub

    Public Property IsOnlineList As Boolean
        Get
            Return m_isOnlineList
        End Get
        Set(value As Boolean)
            m_isOnlineList = value
        End Set
    End Property
End Class
