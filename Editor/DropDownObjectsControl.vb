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
End Class
