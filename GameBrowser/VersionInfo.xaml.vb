Public Class VersionInfo
    Private m_updateData As UpdatesData
    Private m_currentVersion As Version

    Public Property UpdateInfo As UpdatesData
        Get
            Return m_updateData
        End Get
        Set(value As UpdatesData)
            m_updateData = value
            If value IsNot Nothing Then
                Dispatcher.BeginInvoke(Sub() lblNewVersionDesc.Content = value.Description)
            End If
        End Set
    End Property

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
