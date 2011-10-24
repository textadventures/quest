Imports System.Net

Public Class GameDescription
    Public Event Close()

    Private WithEvents m_client As WebClient
    Private WithEvents m_reviewsClient As WebClient
    Private m_cache As New Dictionary(Of String, GameDescriptionData)
    Private m_linkUrl As String
    Private WithEvents m_listItemControl As GameListItem
    Private m_downloadedReviews As Boolean
    Private m_data As GameListItemData

    Private Sub cmdClose_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles cmdClose.Click
        RaiseEvent Close()
    End Sub

    Public Sub Populate(data As GameListItemData, control As GameListItem)
        m_data = data
        author.Text = data.Author
        title.Text = data.GameName
        category.Text = ""
        description.Text = ""
        dateAdded.Text = ""
        m_linkUrl = ""
        m_downloadedReviews = False
        expander.IsExpanded = False
        downloadingReviews.Visibility = Windows.Visibility.Visible
        reviewsStack.Children.Clear()
        linkBlock.Visibility = Windows.Visibility.Collapsed
        m_listItemControl = control
        UpdateState()
        RequestData(data.GameId)
    End Sub

    Private Sub UpdateState()
        Select Case m_listItemControl.CurrentState
            Case GameListItem.State.ReadyToPlay
                cmdAction.Content = "Play"
                cmdAction.IsEnabled = True
            Case GameListItem.State.NotDownloaded
                cmdAction.Content = "Download"
                cmdAction.IsEnabled = True
            Case GameListItem.State.Downloading
                cmdAction.Content = "Downloading"
                cmdAction.IsEnabled = False
        End Select
    End Sub

    Private Sub RequestData(id As String)
        If m_client Is Nothing Then
            m_client = WebClientFactory.GetNewWebClient()
        End If

        If m_cache.ContainsKey(id) Then
            PopulateRequestData(m_cache(id))
        Else
            Dim URL As String = WebClientFactory.RootURL + "?id=" + id

            m_client.CancelAsync()

            Dim newThread As New System.Threading.Thread(Sub() m_client.DownloadStringAsync(New System.Uri(URL)))
            newThread.Start()
        End If

    End Sub

    Private Sub m_client_DownloadStringCompleted(sender As Object, e As System.Net.DownloadStringCompletedEventArgs) Handles m_client.DownloadStringCompleted
        If e.Error Is Nothing Then
            ProcessXML(e.Result)
        Else
            ' TO DO: raise error
        End If
    End Sub

    Private Class GameDescriptionData
        Public Description As String
        Public GameId As String
        Public Category As String
        Public DateAdded As String
        Public URL As String
        Public NumberReviews As Integer
        Public Reviews As List(Of ReviewData)
    End Class

    Private Sub ProcessXML(xml As String)
        Dim doc As XDocument = XDocument.Parse(xml)

        Dim gameData = From data In doc.Descendants("game")
                          Select New GameDescriptionData With {
                              .Description = data.@desc,
                              .GameId = data.@id,
                              .Category = data.@cat,
                              .DateAdded = data.@date,
                              .URL = data.@url,
                              .NumberReviews = CInt(data.@reviews)
                          }

        Dim gameItemdata As GameDescriptionData = gameData.FirstOrDefault
        m_cache.Add(gameItemdata.GameId, gameItemdata)

        Dispatcher.BeginInvoke(Sub() PopulateRequestData(gameItemdata))
    End Sub

    Private Sub PopulateRequestData(data As GameDescriptionData)
        category.Text = data.Category
        description.Text = System.Net.WebUtility.HtmlDecode(data.Description)
        dateAdded.Text = "Added " + Date.Parse(data.DateAdded).ToShortDateString()
        m_linkUrl = data.URL
        linkBlock.Visibility = Windows.Visibility.Visible
    End Sub

    Private Sub link_Click(sender As System.Object, e As System.Windows.RoutedEventArgs)
        LaunchURL(m_linkUrl)
    End Sub

    Private Sub LaunchURL(url As String)
        Try
            System.Diagnostics.Process.Start(url)
        Catch ex As Exception
            MsgBox(String.Format("Error launching {0}{1}{2}", url, Environment.NewLine + Environment.NewLine, ex.Message), MsgBoxStyle.Critical, "Quest")
        End Try
    End Sub

    Private Sub m_listItemControl_StateChanged() Handles m_listItemControl.StateChanged
        UpdateState()
    End Sub

    Private Sub cmdAction_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles cmdAction.Click
        m_listItemControl.LaunchButtonClick()
    End Sub

    Private Sub expander_Expanded(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles expander.Expanded
        If Not m_downloadedReviews Then
            m_downloadedReviews = True
            If m_reviewsClient Is Nothing Then
                m_reviewsClient = WebClientFactory.GetNewWebClient()
            End If

            If m_cache(m_data.GameId).Reviews Is Nothing Then
                Dim URL As String = WebClientFactory.RootURL + "?id=" + m_data.GameId + "&reviews=1"

                m_reviewsClient.CancelAsync()
                Dim newThread As New System.Threading.Thread(Sub() m_reviewsClient.DownloadStringAsync(New System.Uri(URL)))
                newThread.Start()
            Else
                PopulateReviewData(m_cache(m_data.GameId).Reviews)
            End If
        End If
    End Sub

    Private Sub m_reviewsClient_DownloadStringCompleted(sender As Object, e As System.Net.DownloadStringCompletedEventArgs) Handles m_reviewsClient.DownloadStringCompleted
        If e.Error Is Nothing Then
            ProcessReviewsXML(e.Result)
        Else
            ' TO DO: raise error
        End If
    End Sub

    Private Class ReviewData
        Public Rating As Integer
        Public ReviewDate As String
        Public ReviewText As String
        Public Reviewer As String
    End Class

    Private Sub ProcessReviewsXML(xml As String)

        Dim reviewData As IEnumerable(Of ReviewData) = Nothing

        Try
            Dim doc As XDocument = XDocument.Parse(xml)

            reviewData = From data In doc.Descendants("review")
                              Select New ReviewData With {
                                  .ReviewText = data.@review,
                                  .Rating = CInt(data.@rating),
                                  .ReviewDate = data.@date,
                                  .Reviewer = data.@user
                              }

            m_cache(m_data.GameId).Reviews = New List(Of ReviewData)(reviewData)

        Catch
            Dispatcher.BeginInvoke(Sub()
                                       Dim textBlock As New Windows.Controls.TextBlock
                                       textBlock.Text = "Failed to download review data"
                                       reviewsStack.Children.Add(textBlock)
                                   End Sub)
        End Try

        Dispatcher.BeginInvoke(Sub() PopulateReviewData(ReviewData))
    End Sub

    Private Sub PopulateReviewData(data As IEnumerable(Of ReviewData))
        downloadingReviews.Visibility = Windows.Visibility.Collapsed
        If data Is Nothing Then Return
        For Each review As ReviewData In data
            Dim reviewItem As New ReviewItem
            reviewItem.Populate(review.Rating > 0, review.Reviewer, review.ReviewText, review.Rating)
            reviewsStack.Children.Add(reviewItem)
        Next

        If Not data.Any Then
            Dim textBlock As New Windows.Controls.TextBlock
            textBlock.Margin = New Windows.Thickness(5)
            textBlock.Text = "No reviews or comments"
            reviewsStack.Children.Add(textBlock)
        End If
    End Sub

End Class
