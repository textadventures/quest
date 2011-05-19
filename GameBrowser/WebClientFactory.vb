Friend Class WebClientFactory
    Private Shared m_questVersion As String

    Public Shared Function GetNewWebClient() As System.Net.WebClient

        Dim client As New System.Net.WebClient
        client.Headers.Add(Net.HttpRequestHeader.UserAgent, "Quest " + m_questVersion)

        ' Setting Proxy to Nothing massively speeds up how quickly the data is fetched
        client.Proxy = Nothing

        Return client

    End Function

    Public Shared Property QuestVersion As String
        Get
            Return m_questVersion
        End Get
        Set(value As String)
            m_questVersion = value
        End Set
    End Property
End Class
