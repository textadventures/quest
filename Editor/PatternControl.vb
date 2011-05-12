<ControlType("pattern")> _
Public Class PatternControl
    Inherits TextBoxControl

    Private m_value As IEditableCommandPattern

    Public Overrides ReadOnly Property ExpectedType As System.Type
        Get
            Return GetType(EditableCommandPattern)
        End Get
    End Property

    Protected Overrides Function GetValue() As Object
        If txtTextBox.Text.Length = 0 Then Return Nothing
        Return m_value
    End Function

    Protected Overrides Sub SetValue(value As Object)
        Dim patternValue As IEditableCommandPattern = TryCast(value, IEditableCommandPattern)
        If patternValue IsNot Nothing Then
            txtTextBox.Text = patternValue.Pattern
            OldValue = txtTextBox.Text
        Else
            txtTextBox.Text = String.Empty
            OldValue = Nothing
        End If

        m_value = patternValue
    End Sub

    Protected Overrides Sub SaveData(data As IEditorData)
        If IsDirty Then
            Dim description As String = String.Format("Set {0} to '{1}'", AttributeName, txtTextBox.Text)
            Controller.StartTransaction(description)
            m_value = Controller.CreateNewEditableCommandPattern(data.Name, AttributeName, txtTextBox.Text, False)
            Controller.EndTransaction()
            ' reset the dirty flag
            Value = Value
            Debug.Assert(Not IsDirty)
        End If
    End Sub

End Class
