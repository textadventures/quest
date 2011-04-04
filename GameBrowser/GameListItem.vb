Friend Class GameListItem
    Public Event Launch(filename As String)

    Private m_filename As String

    Public WriteOnly Property GameName() As String
        Set(value As String)
            lblName.Text = value
        End Set
    End Property

    Public WriteOnly Property GameInfo() As String
        Set(value As String)
            lblInfo.Text = value
        End Set
    End Property

    Public WriteOnly Property LaunchCaption() As String
        Set(value As String)
            cmdLaunch.Text = value
        End Set
    End Property

    Public Property Filename() As String
        Get
            Return m_filename
        End Get
        Set(value As String)
            m_filename = value
            Dim tooltipCtls As New List(Of Control)(New Control() {lblInfo, lblName})
            For Each ctl As Control In tooltipCtls
                ctlToolTip.SetToolTip(ctl, m_filename)
            Next
        End Set
    End Property

    Private Sub cmdLaunch_Click(sender As System.Object, e As System.EventArgs) Handles cmdLaunch.Click
        RaiseEvent Launch(m_filename)
    End Sub

End Class
