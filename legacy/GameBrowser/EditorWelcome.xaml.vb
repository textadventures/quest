Imports TextAdventures.Utility.Language.L

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
        LaunchURL("https://github.com/textadventures/quest/discussions")
    End Sub

    Private Sub LaunchURL(url As String)
        Try
            System.Diagnostics.Process.Start(url)
        Catch ex As Exception
            MsgBox(String.Format("Error launching {0}{1}{2}", url, Environment.NewLine + Environment.NewLine, ex.Message), MsgBoxStyle.Critical, "Quest")
        End Try
    End Sub

    Private Sub lblCreateNewGame_Initialized(sender As Object, e As EventArgs) Handles lblCreateNewGame.Initialized
        lblCreateNewGame.Text = T("LauncherCreateNewGame")
    End Sub

    Private Sub lblOpenExistingGame_Initialized(sender As Object, e As EventArgs) Handles lblOpenExistingGame.Initialized
        lblOpenExistingGame.Text = T("LauncherOpenExistingGame")
    End Sub

    Private Sub lblReadTheTutorial_Initialized(sender As Object, e As EventArgs) Handles lblReadTheTutorial.Initialized
        lblReadTheTutorial.Text = T("LauncherReadTutorial")
    End Sub

    Private Sub lblGetHelpInDiscussions_Initialized(sender As Object, e As EventArgs) Handles lblGetHelpInDiscussions.Initialized
        lblGetHelpInDiscussions.Text = T("LauncherGetHelpInDiscussions")
    End Sub
End Class
