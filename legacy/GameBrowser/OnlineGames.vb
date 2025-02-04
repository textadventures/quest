Public Class OnlineGames
    Private WithEvents m_client As System.Net.WebClient

    Public Event DataReady()
    Public Event GotUpdateData(data As UpdatesData)
    Public Event DownloadFailed(message As String)

    Public Class GameData
        Public Name As String
        Public Ref As String
        Public Filename As String
        Public Author As String
        Public Rating As Double
        Public GameId As String
        Public Cover As String
        Public Thumbnail As String
    End Class

    Public Class GameCategory
        Public Title As String
        Public Games As List(Of GameData)
    End Class

    Private m_categories As New List(Of GameCategory)

    Public Sub StartDownloadGameData()
        m_client = WebClientFactory.GetNewWebClient
        Dim url As String = WebClientFactory.RootURL
        Dim newThread As New System.Threading.Thread(Sub() m_client.DownloadStringAsync(New System.Uri(url)))
        newThread.Start()
    End Sub

    Private Sub m_client_DownloadStringCompleted(sender As Object, e As System.Net.DownloadStringCompletedEventArgs) Handles m_client.DownloadStringCompleted
        If e.Error Is Nothing Then
            ProcessXML(e.Result)
        Else
            RaiseEvent DownloadFailed(e.Error.Message)
        End If
    End Sub

    Private Sub ProcessXML(xml As String)
        Try
            Dim doc As XDocument = XDocument.Parse(xml)

            Dim versionData = From data In doc.Descendants("questversion")
                              Select New UpdatesData With {
                                  .Major = data.@major,
                                  .Minor = data.@minor,
                                  .Build = data.@build,
                                  .Revision = data.@rev,
                                  .Description = data.@desc,
                                  .URL = data.@url
                              }

            RaiseEvent GotUpdateData(versionData.FirstOrDefault)

            m_categories = (From cat In doc.Descendants("category")
                            Select New GameCategory With {
                                    .Title = cat.@title,
                                    .Games = (From game In cat.Descendants("game")
                                              Select New GameData With {
                                                     .Name = game.@name,
                                                     .Ref = game.@ref,
                                                     .Filename = game.@filename,
                                                     .Author = game.@author,
                                                     .Rating = ParseDouble(game.@rating),
                                                     .GameId = game.@id,
                                                     .Cover = game.@cover,
                                                     .Thumbnail = game.@thumbnail
                                              }).ToList()
                             }).ToList()
        Catch ex As Exception
            RaiseEvent DownloadFailed(ex.Message)
        End Try

        RaiseEvent DataReady()
    End Sub

    Private Function ParseDouble(input As String) As Double
        If String.IsNullOrEmpty(input) Then Return 0
        Return Double.Parse(input, System.Globalization.CultureInfo.InvariantCulture)
    End Function

    Public ReadOnly Property Categories As IEnumerable(Of GameCategory)
        Get
            Return m_categories.AsReadOnly()
        End Get
    End Property

    Private Function GetGames(category As String) As IEnumerable(Of GameData)
        Return m_categories.First(Function(c) c.Title = category).Games
    End Function

    Friend Sub PopulateGameList(category As String, gameListCtl As GameList)
        Dim gamesList As New List(Of GameListItemData)

        If category IsNot Nothing Then
            For Each game In GetGames(category)
                gamesList.Add(New GameListItemData(game.Name, game.Ref, game.Filename) With
                              {.Author = game.Author,
                               .GameId = game.GameId,
                               .Rating = game.Rating,
                               .Cover = game.Cover,
                               .Thumbnail = game.Thumbnail
                              })
            Next

            gameListCtl.CreateListElements(gamesList)
        Else
            gameListCtl.CreateListElements(Nothing)
        End If

    End Sub
End Class
