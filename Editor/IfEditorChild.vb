Public Class IfEditorChild

    Private m_controller As EditorController
    Private m_elseIfMode As Boolean

    Public Event Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs)

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ElseIfMode = False
    End Sub

    Public Sub SaveData(ByVal data As IEditorData)
        ctlExpression.Save(data)
        ctlThenScript.Save(data)
    End Sub

    Public Property Controller As EditorController
        Get
            Return m_controller
        End Get
        Set(ByVal value As EditorController)
            m_controller = value
            If Not value Is Nothing Then
                ctlThenScript.Controller = value
                ctlExpression.Controller = value
                ctlThenScript.Initialise(Nothing)
            End If
        End Set
    End Property

    Public Sub Populate(ByVal data As EditableIfScript)
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

    Public Sub UpdateField(ByVal attribute As String, ByVal newValue As Object, ByVal setFocus As Boolean)
        ' TO DO: The string "0" here is hacky, but it's a consequence of the mess of string vs int for the "attribute" name
        ' when we're editing scripts. The 0 comes from the index passed to NotifyUpdate in IfScript.SetExpressionSilent(). This
        ' should be tidied up so we can pass an Enum perhaps.
        If attribute = "0" Then
            ctlExpression.Value = newValue
            If setFocus Then ctlExpression.Focus()
        End If
    End Sub

    Public Property ElseIfMode As Boolean
        Get
            Return m_elseIfMode
        End Get
        Set(ByVal value As Boolean)
            m_elseIfMode = value

            If value Then
                lblIf.Text = "Else If:"
            Else
                lblIf.Text = "If:"
            End If
        End Set
    End Property
End Class
