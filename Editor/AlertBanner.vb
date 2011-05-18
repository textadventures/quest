Public Class AlertBanner
    Public Event ButtonClicked()

    Public Property AlertText As String
        Get
            Return lblAlertText.Text
        End Get
        Set(value As String)
            lblAlertText.Text = value
        End Set
    End Property

    Public Property ButtonText As String
        Get
            Return cmdAction.Text
        End Get
        Set(value As String)
            cmdAction.Text = value
        End Set
    End Property

    Private Sub cmdAction_Click(sender As System.Object, e As System.EventArgs) Handles cmdAction.Click
        RaiseEvent ButtonClicked()
    End Sub
End Class
