Public Class ErrorHandler

    Public WriteOnly Property ErrorText() As String
        Set(ByVal value As String)
            txtError.Text = value
        End Set
    End Property

    Private Sub cmdClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdClose.Click
        Me.Close()
    End Sub
End Class