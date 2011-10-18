Imports System.Net

Public Class GameDescription
    Public Event Close()

    Private WithEvents m_client As WebClient

    Private Sub cmdClose_Click(sender As System.Object, e As System.Windows.RoutedEventArgs) Handles cmdClose.Click
        RaiseEvent Close()
    End Sub

    Public Sub Populate(data As GameListItemData)
        author.Text = data.Author
        title.Text = data.GameName
        category.Text = ""
        description.Text = ""
        RequestData(data.GameId)
    End Sub

    Private Sub RequestData(id As String)
        If m_client Is Nothing Then
            m_client = WebClientFactory.GetNewWebClient()
        End If

        Dim URL As String = WebClientFactory.RootURL + "?id=" + id

        Dim newThread As New System.Threading.Thread(Sub() m_client.DownloadStringAsync(New System.Uri(URL)))
        newThread.Start()
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
    End Class

    Private Sub ProcessXML(xml As String)
        Dim doc As XDocument = XDocument.Parse(xml)

        Dim gameData = From data In doc.Descendants("game")
                          Select New GameDescriptionData With {
                              .Description = data.@desc,
                              .GameId = data.@id,
                              .Category = data.@cat
                          }

        Dispatcher.BeginInvoke(Sub() PopulateRequestData(gameData.FirstOrDefault))
    End Sub

    Private Sub PopulateRequestData(data As GameDescriptionData)
        category.Text = data.Category
        description.Text = System.Net.WebUtility.HtmlDecode(data.Description)
    End Sub
End Class
