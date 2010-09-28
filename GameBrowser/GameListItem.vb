Friend Class GameListItem
    Public Event Launch(ByVal filename As String)

    Private m_filename As String

    Public WriteOnly Property GameName() As String
        Set(ByVal value As String)
            lblName.Text = value
        End Set
    End Property

    Public WriteOnly Property GameInfo() As String
        Set(ByVal value As String)
            lblInfo.Text = value
        End Set
    End Property

    Public WriteOnly Property LaunchCaption() As String
        Set(ByVal value As String)
            cmdLaunch.Text = value
        End Set
    End Property

    Public Property Filename() As String
        Get
            Return m_filename
        End Get
        Set(ByVal value As String)
            m_filename = value
        End Set
    End Property

    Private Sub cmdLaunch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdLaunch.Click
        RaiseEvent Launch(m_filename)
    End Sub
End Class
