<ControlType("objects")> _
Public Class DropDownObjectsControl
    Inherits DropDownControl

    Private m_value As IEditableObjectReference

    Protected Overrides Sub InitialiseControl(controlData As IEditorControl)
        ' populate list with all objects in game

        If controlData IsNot Nothing Then
            PopulateList()
        Else
            lstDropdown.Items.Clear()
        End If
    End Sub

    Public Overrides ReadOnly Property ExpectedType As System.Type
        Get
            Return GetType(IEditableObjectReference)
        End Get
    End Property

    Protected Overrides Sub SaveData(data As IEditorData)
        Dim currentValue As String = If(m_value Is Nothing, String.Empty, m_value.Reference)
        If lstDropdown.Text <> currentValue Then
            Controller.StartTransaction(String.Format("Change '{0}' to '{1}'", AttributeName, lstDropdown.Text))
            m_value = Controller.CreateNewEditableObjectReference(data.Name, AttributeName, False)
            m_value.Reference = lstDropdown.Text
            Controller.EndTransaction()
        End If
    End Sub

    Protected Overrides Sub PopulateData(data As IEditorData)
        PopulateList()
        SetValue(data.GetAttribute(AttributeName))
    End Sub

    Protected Overrides Sub SetValue(value As Object)
        m_value = DirectCast(value, IEditableObjectReference)
        lstDropdown.Text = If(m_value Is Nothing, "", m_value.Reference)
    End Sub

    Protected Overrides Function GetValue() As Object
        Return m_value
    End Function

    Private Sub PopulateList()
        lstDropdown.Items.Clear()
        Dim allObjects As IEnumerable(Of String) = Controller.GetObjectNames("object")
        lstDropdown.Items.AddRange(allObjects.ToArray())
    End Sub

End Class
