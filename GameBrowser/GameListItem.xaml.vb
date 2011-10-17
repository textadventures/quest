Public Class GameListItem
    Public Event Launch(filename As String)
    Public Event ClearAllItems()
    Public Event RemoveItem(filename As String)

    Private m_filename As String
    Private m_url As String
    Private m_downloading As Boolean = False
    Private WithEvents m_client As System.Net.WebClient
    Private m_downloadFilename As String
    'Private m_progressBar As ProgressBar
    Private m_setDownloadTooltip As Boolean

    Public Enum State
        ReadyToPlay
        NotDownloaded
        Downloading
    End Enum

    Private m_state As State = State.ReadyToPlay
    Private m_playButtonWidth As Integer
    Private m_playButtonRight As Integer

    Public Property CurrentState As State
        Get
            Return m_state
        End Get
        Set(value As State)
            m_state = value

            Select Case m_state
                Case State.ReadyToPlay
                    cmdLaunch.Content = "Play"
                    info.Text = "Download complete"
                Case State.NotDownloaded
                    cmdLaunch.Content = "Download"
                    info.Text = "Not downloaded"
                Case State.Downloading
                    cmdLaunch.Content = "Cancel"
                    info.Text = ""

                    'If m_progressBar Is Nothing Then
                    '    m_progressBar = New ProgressBar
                    '    m_progressBar.Parent = Me
                    '    m_progressBar.Top = lblInfo.Top
                    '    m_progressBar.Left = lblInfo.Left
                    '    m_progressBar.Height = lblInfo.Height
                    '    m_progressBar.Width = Me.Width - cmdLaunch.Width - 20
                    '    m_progressBar.Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top
                    'End If
            End Select

            'If m_progressBar IsNot Nothing Then
            '    m_progressBar.Visible = (m_state = State.Downloading)
            'End If
        End Set
    End Property

    Public Property GameName() As String
        Get
            Return title.Text
        End Get
        Set(value As String)
            title.Text = value
        End Set
    End Property

    Public Property GameInfo() As String
        Get
            Return info.Text
        End Get
        Set(value As String)
            info.Text = value
        End Set
    End Property

    Public Property LaunchCaption() As String
        Get
            Return DirectCast(cmdLaunch.Content, String)
        End Get
        Set(value As String)
            cmdLaunch.Content = value
        End Set
    End Property

    Public Property URL As String
        Get
            Return m_url
        End Get
        Set(value As String)
            m_url = value
        End Set
    End Property

    Public Property DownloadFilename As String
        Get
            Return m_downloadFilename
        End Get
        Set(value As String)
            m_downloadFilename = value
        End Set
    End Property

    Private m_author As String

    Public Property Author As String
        Get
            Return m_author
        End Get
        Set(value As String)
            m_author = value
            authorLabel.Text = "by " + value
        End Set
    End Property

    Public Property Filename() As String
        Get
            Return m_filename
        End Get
        Set(value As String)
            m_filename = value
            SetToolTipText(m_filename)
        End Set
    End Property

    Private Sub SetToolTipText(text As String)
        'Dim tooltipCtls As New List(Of Control)(New Control() {lblInfo, lblName})
        'For Each ctl As Control In tooltipCtls
        '    ctlToolTip.SetToolTip(ctl, text)
        'Next
    End Sub

    Private Sub cmdLaunch_Click(sender As System.Object, e As System.EventArgs) Handles cmdLaunch.Click
        Select Case CurrentState
            Case State.ReadyToPlay
                RaiseEvent Launch(m_filename)
            Case State.NotDownloaded
                StartDownload()
            Case State.Downloading
                CancelDownload()
        End Select
    End Sub

    Private Sub StartDownload()
        CurrentState = State.Downloading
        m_client = WebClientFactory.GetNewWebClient
        Dim newThread As New System.Threading.Thread(Sub() m_client.DownloadFileAsync(New System.Uri(m_url), m_downloadFilename))
        newThread.Start()
    End Sub

    Private Sub CancelDownload()
        CurrentState = State.NotDownloaded
        m_client.CancelAsync()
    End Sub

    Private Sub m_client_DownloadFileCompleted(sender As Object, e As System.ComponentModel.AsyncCompletedEventArgs) Handles m_client.DownloadFileCompleted
        Dispatcher.BeginInvoke(Sub()
                                   If e.Error Is Nothing Then
                                       Filename = m_downloadFilename
                                       CurrentState = State.ReadyToPlay
                                   Else
                                       If Not e.Cancelled Then
                                           MsgBox(String.Format(
                                                  "Failed to download file. The following error occurred:{0}{1}{2}",
                                                  Environment.NewLine,
                                                  Environment.NewLine,
                                                  e.Error.Message), MsgBoxStyle.Critical, "Download Failed")
                                       End If
                                       CurrentState = State.NotDownloaded
                                       DeleteDownloadedFile()
                                   End If
                               End Sub)
    End Sub

    Private Sub m_client_DownloadProgressChanged(sender As Object, e As System.Net.DownloadProgressChangedEventArgs) Handles m_client.DownloadProgressChanged
        Dispatcher.BeginInvoke(Sub()
                                   'm_progressBar.Value = e.ProgressPercentage
                                   If Not m_setDownloadTooltip Then
                                       SetToolTipText(String.Format("Downloading ({0} bytes)", e.TotalBytesToReceive))
                                       m_setDownloadTooltip = True
                                   End If
                               End Sub)
    End Sub

    Private Sub DeleteDownloadedFile()
        If System.IO.File.Exists(m_downloadFilename) Then
            System.IO.File.Delete(m_downloadFilename)
        End If
    End Sub

    Public Sub DisableContextMenu()
        'Me.ContextMenuStrip = Nothing
    End Sub

    'Private Sub mnuClear_Click(sender As Object, e As System.EventArgs) Handles mnuClear.Click
    '    RaiseEvent ClearAllItems()
    'End Sub

    'Private Sub mnuRemove_Click(sender As Object, e As System.EventArgs) Handles mnuRemove.Click
    '    RaiseEvent RemoveItem(m_filename)
    'End Sub

End Class
