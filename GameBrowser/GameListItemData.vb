Friend Class GameListItemData
    Private m_filename As String
    Private m_name As String

    Public Sub New(filename As String, gameName As String)
        m_filename = filename
        m_name = gameName
    End Sub

    Public ReadOnly Property Filename() As String
        Get
            Return m_filename
        End Get
    End Property

    Public ReadOnly Property GameName() As String
        Get
            Return m_name
        End Get
    End Property
End Class