Friend Class GameListItemData
    Private m_filename As String
    Private m_name As String

    Public Sub New(ByVal filename As String, ByVal gameName As String)
        m_filename = filename
        m_name = gameName
    End Sub

    Public ReadOnly Property Filename()
        Get
            Return m_filename
        End Get
    End Property

    Public ReadOnly Property GameName()
        Get
            Return m_name
        End Get
    End Property
End Class