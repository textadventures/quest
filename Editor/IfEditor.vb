Public Class IfEditor

    Implements ICommandEditor

    Private m_controller As EditorController

    Public Event Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs) Implements ICommandEditor.Dirty

    Public Sub SaveData() Implements ICommandEditor.SaveData
        ctlThenScript.Save(Nothing)
    End Sub

    Public Property Controller As EditorController Implements ICommandEditor.Controller
        Get
            Return m_controller
        End Get
        Set(ByVal value As EditorController)
            m_controller = value
            ctlThenScript.Controller = value
            ctlThenScript.Initialise(Nothing)
        End Set
    End Property

    Public Sub Populate(ByVal data As EditableIfScript)
        ctlThenScript.Populate(data.ThenScript)
    End Sub

    Private Sub ctlThenScript_Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs) Handles ctlThenScript.Dirty
        RaiseEvent Dirty(sender, args)
    End Sub
End Class
