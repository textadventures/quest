Public Class IfEditorChild

    Private m_controller As EditorController
    Private m_elseIfMode As Boolean
    Private m_expanded As Boolean

    Public Event Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs)
    Public Event ChangeHeight(ByVal sender As IfEditorChild, ByVal newHeight As Integer)

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ElseIfMode = False
        Expanded = True
        cmdExpand.Font = New Font("Marlett", cmdExpand.Font.Size * 1.1F)
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

    Public Property Expanded As Boolean
        Get
            Return m_expanded
        End Get
        Set(ByVal value As Boolean)
            Dim oldValue As Boolean = m_expanded
            m_expanded = value
            ctlThenScript.Visible = Expanded
            Dim newHeight As Integer
            If Expanded Then
                newHeight = 300
            Else
                newHeight = lblThen.Top + lblThen.Height
            End If

            If (oldValue <> value) Then RaiseEvent ChangeHeight(Me, newHeight)
        End Set
    End Property

    Private Sub IfEditorChild_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.DoubleClick
        ToggleExpand()
    End Sub

    Private Sub cmdExpand_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdExpand.Click
        ToggleExpand()
    End Sub

    Private Sub ToggleExpand()
        Expanded = Not Expanded
    End Sub
End Class
