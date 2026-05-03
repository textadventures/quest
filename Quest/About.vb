Public Class About

    Private Sub About_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Dim ApplicationTitle As String
        If My.Application.Info.Title <> "" Then
            ApplicationTitle = My.Application.Info.Title
        Else
            ApplicationTitle = System.IO.Path.GetFileNameWithoutExtension(My.Application.Info.AssemblyName)
        End If
        Dim Abouttext As String = Me.Text
        Me.Text = String.Format(Abouttext + " {0}", ApplicationTitle)
        lblTitle.Text = String.Format("{0} {1}", My.Application.Info.ProductName, Constants.QuestVersion)
        lblBuild.Text = String.Format("Build {0}", My.Application.Info.Version.ToString)

        Dim copyrightText = My.Application.Info.Copyright
        lblCopyright.Text = copyrightText & Environment.NewLine & "Licence information"
        lblCopyright.LinkArea = New System.Windows.Forms.LinkArea(copyrightText.Length + Environment.NewLine.Length, "Licence information".Length)
        lblCopyright.Dock = System.Windows.Forms.DockStyle.None
        lblCopyright.Left = -2
    End Sub

    Private Sub lblCopyright_LinkClicked(sender As Object, e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles lblCopyright.LinkClicked
        Process.Start("https://github.com/textadventures/quest/blob/master/LICENSE")
    End Sub

    Private Sub btnClose_Click(sender As System.Object, e As System.EventArgs)
        Me.Close()
    End Sub
End Class
