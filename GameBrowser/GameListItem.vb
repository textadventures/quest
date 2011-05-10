Friend Class GameListItem
    Public Event Launch(filename As String)

    Private m_filename As String
    Private m_url As String
    Private m_downloading As Boolean = False
    Private WithEvents m_client As System.Net.WebClient
    Private m_downloadFilename As String
    Private m_progressBar As ProgressBar
    Private m_setDownloadTooltip As Boolean

    Public Enum State
        ReadyToPlay
        NotDownloaded
        Downloading
    End Enum

    Private m_state As State = State.ReadyToPlay
    Private m_playButtonWidth As Integer
    Private m_playButtonRight As Integer

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        m_playButtonWidth = cmdLaunch.Width
        m_playButtonRight = Me.Width - (cmdLaunch.Left + cmdLaunch.Width)

    End Sub

    Public Property CurrentState As State
        Get
            Return m_state
        End Get
        Set(value As State)
            m_state = value

            Select Case m_state
                Case State.ReadyToPlay
                    cmdLaunch.Text = "Play"
                    cmdLaunch.Width = m_playButtonWidth
                    cmdLaunch.Left = Me.Width - m_playButtonRight - cmdLaunch.Width
                    lblInfo.Text = "Download complete"
                Case State.NotDownloaded
                    cmdLaunch.Text = "Download"
                    cmdLaunch.Width = 70
                    cmdLaunch.Left = Me.Width - m_playButtonRight - cmdLaunch.Width
                    lblInfo.Text = "Not downloaded"
                Case State.Downloading
                    cmdLaunch.Text = "Cancel"
                    cmdLaunch.Width = 70
                    cmdLaunch.Left = Me.Width - m_playButtonRight - cmdLaunch.Width

                    If m_progressBar Is Nothing Then
                        m_progressBar = New ProgressBar
                        m_progressBar.Parent = Me
                        m_progressBar.Top = lblInfo.Top
                        m_progressBar.Left = lblInfo.Left
                        m_progressBar.Height = lblInfo.Height
                        m_progressBar.Width = Me.Width - cmdLaunch.Width - 20
                        m_progressBar.Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top
                    End If
            End Select

            lblInfo.Visible = Not m_state = State.Downloading

            If m_progressBar IsNot Nothing Then
                m_progressBar.Visible = (m_state = State.Downloading)
            End If
        End Set
    End Property

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

    Public WriteOnly Property URL As String
        Set(value As String)
            m_url = value
        End Set
    End Property

    Public WriteOnly Property DownloadFilename As String
        Set(value As String)
            m_downloadFilename = value
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
        Dim tooltipCtls As New List(Of Control)(New Control() {lblInfo, lblName})
        For Each ctl As Control In tooltipCtls
            ctlToolTip.SetToolTip(ctl, text)
        Next
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
        m_client = New System.Net.WebClient
        Dim newThread As New System.Threading.Thread(Sub() m_client.DownloadFileAsync(New System.Uri(m_url), m_downloadFilename))
        newThread.Start()
    End Sub

    Private Sub CancelDownload()
        CurrentState = State.NotDownloaded
        m_client.CancelAsync()
    End Sub

    Private Sub m_client_DownloadFileCompleted(sender As Object, e As System.ComponentModel.AsyncCompletedEventArgs) Handles m_client.DownloadFileCompleted
        BeginInvoke(Sub()
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
        BeginInvoke(Sub()
                        m_progressBar.Value = e.ProgressPercentage
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
End Class
