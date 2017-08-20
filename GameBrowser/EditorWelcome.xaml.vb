Public Class EditorWelcome

    Public Event CreateNewGame()
    Public Event OpenGame()
    Public Event Tutorial()

    Private Sub cmdNewGame_Click(sender As System.Object, e As System.Windows.RoutedEventArgs)
        RaiseEvent CreateNewGame()
    End Sub

    Private Sub cmdOpenGame_Click(sender As System.Object, e As System.Windows.RoutedEventArgs)
        RaiseEvent OpenGame()
    End Sub

    Private Sub cmdTutorial_Click(sender As System.Object, e As System.Windows.RoutedEventArgs)
        RaiseEvent Tutorial()
    End Sub

    Private Sub Hyperlink_Click(sender As System.Object, e As System.Windows.RoutedEventArgs)
        LaunchURL("http://forum.textadventures.co.uk/")
    End Sub

    Private Sub LaunchURL(url As String)
        Try
            System.Diagnostics.Process.Start(url)
        Catch ex As Exception
            MsgBox(String.Format("Error launching {0}{1}{2}", url, Environment.NewLine + Environment.NewLine, ex.Message), MsgBoxStyle.Critical, "Quest")
        End Try
    End Sub

End Class
