Public Class ScriptEditorPopOut

    Private Sub ctlScriptEditor_CloseButtonClicked() Handles ctlScriptEditor.CloseButtonClicked
        Me.Close()
    End Sub

    Private Sub ScriptEditorPopOut_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyDown
        If e.KeyCode = Keys.Escape Then
            Me.Close()
        End If
    End Sub
End Class