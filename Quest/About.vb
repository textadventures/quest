Public Class About

    Private Sub About_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        ' Set the title of the form.
        Dim ApplicationTitle As String
        If My.Application.Info.Title <> "" Then
            ApplicationTitle = My.Application.Info.Title
        Else
            ApplicationTitle = System.IO.Path.GetFileNameWithoutExtension(My.Application.Info.AssemblyName)
        End If
        Me.Text = String.Format("About {0}", ApplicationTitle)

        lblTitle.Text = String.Format("{0} {1}", My.Application.Info.ProductName, Constants.QuestVersion)
        lblBuild.Text = String.Format("Build {0}", My.Application.Info.Version.ToString)
        lblCopyright.Text = My.Application.Info.Copyright
    End Sub

    Private Sub btnClose_Click(sender As System.Object, e As System.EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class