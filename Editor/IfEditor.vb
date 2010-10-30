Public Class IfEditor

    Implements ICommandEditor

    Private m_controller As EditorController
    Private m_data As IEditorData

    Public Event Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs) Implements ICommandEditor.Dirty

    ' TO DO: We should use the fact that ctlExpression and ctlThenScript are controls implementing IElementEditorControl,
    ' then we just need a list of embedded controls and we don't have to manually specify each control when populating/saving

    Public Sub SaveData() Implements ICommandEditor.SaveData
        ctlExpression.Save(m_data)
        ctlThenScript.Save(m_data)
    End Sub

    Public Property Controller As EditorController Implements ICommandEditor.Controller
        Get
            Return m_controller
        End Get
        Set(ByVal value As EditorController)
            m_controller = value
            ctlThenScript.Controller = value
            ctlExpression.Controller = value
            ctlThenScript.Initialise(Nothing)
        End Set
    End Property

    Public Sub Populate(ByVal data As EditableIfScript)
        m_data = data
        ctlExpression.Initialise("expression")
        ctlExpression.Populate(data)
        ctlThenScript.Populate(data.ThenScript)
    End Sub

    Private Sub ctlThenScript_Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs) Handles ctlThenScript.Dirty
        args.Attribute = "1"
        RaiseEvent Dirty(sender, args)
    End Sub

    Private Sub ctlExpression_Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs) Handles ctlExpression.Dirty
        args.Attribute = "0"
        RaiseEvent Dirty(sender, args)
    End Sub

    Public Sub UpdateField(ByVal attribute As String, ByVal newValue As Object, ByVal setFocus As Boolean) Implements ICommandEditor.UpdateField
        ' TO DO: The string "0" here is hacky, but it's a consequence of the mess of string vs int for the "attribute" name
        ' when we're editing scripts. The 0 comes from the index passed to NotifyUpdate in IfScript.SetExpressionSilent(). This
        ' should be tidied up so we can pass an Enum perhaps.
        If attribute = "0" Then
            ctlExpression.Value = newValue
            If setFocus Then ctlExpression.Focus()
        End If
    End Sub
End Class
