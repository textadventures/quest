Public Class LoadingControl

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        lblStatus.Text = ""
    End Sub

    Public Sub UpdateStatus(status As String)
        lblStatus.Text = status
    End Sub

End Class
