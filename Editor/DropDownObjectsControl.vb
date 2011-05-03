<ControlType("objects")> _
Public Class DropDownObjectsControl
    Inherits DropDownControl

    Protected Overrides Sub InitialiseControl(controlData As IEditorControl)
        ' populate list with all objects in game

        If controlData IsNot Nothing Then
            Dim allObjects As IEnumerable(Of String) = Controller.GetObjectNames("object")
            lstDropdown.Items.AddRange(allObjects.ToArray())
        Else
            lstDropdown.Items.Clear()
        End If

        ' will also need to be kept updated as objects are added and removed
    End Sub

    Public Overrides ReadOnly Property ExpectedType As System.Type
        Get
            Return GetType(IEditableObjectReference)
        End Get
    End Property

    Protected Overrides Sub SaveData(data As IEditorData)
        ' Do nothing when a Save is requested. The IEditableObjectReference is updated directly
        ' when the dropdown selection changes.
    End Sub

    Protected Overrides Sub PopulateData(data As IEditorData)
        SetValue(data.GetAttribute(AttributeName))
    End Sub

    Protected Overrides Sub SetValue(value As Object)
        Dim obj = DirectCast(value, IEditableObjectReference)
        lstDropdown.Text = obj.Reference
    End Sub
End Class
