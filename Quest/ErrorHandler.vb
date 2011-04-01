Public Class ErrorHandler

    Public WriteOnly Property ErrorText() As String
        Set(value As String)
            txtError.Text = value
        End Set
    End Property

    Private Sub cmdClose_Click(sender As System.Object, e As System.EventArgs) Handles cmdClose.Click
        Me.Close()
    End Sub
End Class