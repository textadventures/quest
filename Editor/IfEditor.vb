Public Class IfEditor

    Implements ICommandEditor

    Private m_controller As EditorController
    Private m_data As IEditorData

    Public Event Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs) Implements ICommandEditor.Dirty

    Public Sub SaveData() Implements ICommandEditor.SaveData
        ctlChild.SaveData(m_data)
    End Sub

    Public Property Controller As EditorController Implements ICommandEditor.Controller
        Get
            Return m_controller
        End Get
        Set(ByVal value As EditorController)
            m_controller = value
            ctlChild.Controller = value
        End Set
    End Property

    Public Sub Populate(ByVal data As EditableIfScript)
        m_data = data
        ctlChild.Populate(data)
    End Sub

    Public Sub UpdateField(ByVal attribute As String, ByVal newValue As Object, ByVal setFocus As Boolean) Implements ICommandEditor.UpdateField
        ctlChild.UpdateField(attribute, newValue, setFocus)
    End Sub

    Private Sub ctlChild_Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs) Handles ctlChild.Dirty
        RaiseEvent Dirty(sender, args)
    End Sub
End Class
