Public Class TextEditorControl

    Public Property EditText As String
        Get
            Return wpfTextEditor.EditText
        End Get
        Set(value As String)
            wpfTextEditor.EditText = value
        End Set
    End Property

End Class
