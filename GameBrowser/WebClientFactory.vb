Friend Class WebClientFactory
    Private Const GamesListURL As String = "http://www.textadventures.co.uk/gamesxml.php"

    Private Shared m_questVersion As String
    Private Shared m_maxAslVersion As Integer
    Private Shared m_url As String

    Public Shared Function GetNewWebClient() As System.Net.WebClient

        Dim client As New System.Net.WebClient
        client.Encoding = System.Text.Encoding.UTF8
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

    Public Shared Property MaxASLVersion As Integer
        Get
            Return m_maxAslVersion
        End Get
        Set(value As Integer)
            m_maxAslVersion = value
            m_url = GamesListURL + "?maxasl=" + value.ToString()
        End Set
    End Property

    Public Shared ReadOnly Property RootURL As String
        Get
            Return m_url
        End Get
    End Property
End Class
