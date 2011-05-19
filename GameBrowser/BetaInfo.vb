Friend Class BetaInfo

    Private m_updateData As UpdatesData
    Private m_currentVersion As Version

    Private Sub LinkLabel1_LinkClicked(sender As System.Object, e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        LaunchURL("mailto:alex@axeuk.com")
    End Sub

    Private Sub LinkLabel2_LinkClicked(sender As System.Object, e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabel2.LinkClicked
        LaunchURL("http://www.quest5.net/")
    End Sub

    Public Property UpdateInfo As UpdatesData
        Get
            Return m_updateData
        End Get
        Set(value As UpdatesData)
            m_updateData = value

            If IsNewVersion() Then
                BeginInvoke(Sub()
                                lblNewVersionDesc.Text = value.Description
                                pnlNewVersion.Visible = True
                            End Sub)
            End If
        End Set
    End Property

    Public Property CurrentVersion As Version
        Get
            Return m_currentVersion
        End Get
        Set(value As Version)
            m_currentVersion = value
        End Set
    End Property

    Private Function IsNewVersion() As Boolean
        If UpdateInfo Is Nothing Then Return False
        Dim latestMajor As Integer
        Dim latestMinor As Integer
        Dim latestBuild As Integer
        Dim latestRevision As Integer

        Integer.TryParse(UpdateInfo.Major, latestMajor)
        Integer.TryParse(UpdateInfo.Minor, latestMinor)
        Integer.TryParse(UpdateInfo.Build, latestBuild)
        Integer.TryParse(UpdateInfo.Revision, latestRevision)

        Dim latestVersion As New Version(latestMajor, latestMinor, latestBuild, latestRevision)
        Return latestVersion > m_currentVersion
    End Function

    Private Sub cmdDownload_Click(sender As System.Object, e As System.EventArgs) Handles cmdDownload.Click
        LaunchURL(UpdateInfo.URL)
    End Sub

    Private Sub LaunchURL(url As String)
        Try
            System.Diagnostics.Process.Start(url)
        Catch ex As Exception
            MsgBox(String.Format("Error launching {0}{1}{2}", url, Environment.NewLine + Environment.NewLine, ex.Message), MsgBoxStyle.Critical, "Quest")
        End Try
    End Sub
End Class
