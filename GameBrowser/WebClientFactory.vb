Friend Class WebClientFactory
    Public Shared Function GetNewWebClient() As System.Net.WebClient

        Dim client As New System.Net.WebClient
        client.Headers.Add(Net.HttpRequestHeader.UserAgent, "Quest " + My.Application.Info.Version.ToString)

        ' Setting Proxy to Nothing massively speeds up how quickly the data is fetched
        client.Proxy = Nothing

        Return client

    End Function
End Class
