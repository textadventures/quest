Public Class IfEditorChild

    Public Enum IfEditorChildMode
        IfMode
        ElseIfMode
        ElseMode
    End Enum

    Private m_controller As EditorController
    Private m_mode As IfEditorChildMode
    Private m_expanded As Boolean
    Private m_scriptEditorMinHeight As Integer
    Private m_script As IEditableScripts
    Private m_data As IEditorData
    Private m_elseIf As EditableIfScript.EditableElseIf

    Public Event Dirty(sender As Object, args As DataModifiedEventArgs)
    Public Event ChangeHeight(sender As IfEditorChild, newHeight As Integer)
    Public Event Delete(sender As IfEditorChild)

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        m_scriptEditorMinHeight = ctlThenScript.Height
        ElseIfMode = IfEditorChildMode.IfMode
        Expanded = True
        cmdExpand.Font = New Font("Marlett", cmdExpand.Font.Size * 1.1F)
        cmdDelete.Font = New Font("Marlett", cmdExpand.Font.Size * 1.1F)
    End Sub

    Public Sub SaveData()
        ctlExpression.Save(m_data)
        ctlThenScript.Save(m_data)
    End Sub

    Public Property Controller As EditorController
        Get
            Return m_controller
        End Get
        Set(value As EditorController)
            m_controller = value
            If Not value Is Nothing Then
                ctlThenScript.Controller = value
                ctlExpression.Controller = value
                ctlThenScript.Initialise(value, Nothing)
            End If
        End Set
    End Property

    Public Sub Populate(data As IEditorData, script As IEditableScripts)
        ' Currently the IEditorData only passes in an "expression" attribute, and it will be null when populating
        ' an "Else"
        If Not data Is Nothing Then
            ctlExpression.Initialise("expression")
            ctlExpression.Populate(data)
        End If
        ctlThenScript.Populate(script)
        m_script = script
        m_data = data
    End Sub

    Private Sub ctlThenScript_Dirty(sender As Object, args As DataModifiedEventArgs) Handles ctlThenScript.Dirty
        args.Attribute = "1"
        RaiseEvent Dirty(Me, args)
    End Sub

    Private Sub ctlExpression_Dirty(sender As Object, args As DataModifiedEventArgs) Handles ctlExpression.Dirty
        args.Attribute = "0"
        RaiseEvent Dirty(Me, args)
    End Sub

    Public Sub UpdateField(attribute As String, newValue As Object, setFocus As Boolean)
        ' TO DO: The string "0" here is hacky, but it's a consequence of the mess of string vs int for the "attribute" name
        ' when we're editing scripts. The 0 comes from the index passed to NotifyUpdate in IfScript.SetExpressionSilent(). This
        ' should be tidied up so we can pass an Enum perhaps.
        If attribute = "0" Then
            ctlExpression.Value = newValue

            ' TO DO: Disabled the setFocus capability at the moment as it can cause some mad things to happen - I think if you
            ' SetFocus during a Leave event then two controls might end up with focus at the same time, and they won't paint
            ' properly.
            'If setFocus Then ctlExpression.Focus()
        End If
    End Sub

    Public Property ElseIfMode As IfEditorChildMode
        Get
            Return m_mode
        End Get
        Set(value As IfEditorChildMode)
            m_mode = value

            Select Case value
                Case IfEditorChildMode.IfMode
                    lblIf.Text = "If:"
                    cmdDelete.Visible = False
                    ctlExpression.Width = cmdExpand.Left - ctlExpression.Left - 4
                Case IfEditorChildMode.ElseIfMode
                    lblIf.Text = "Else If:"
                    cmdDelete.Visible = True
                    ctlExpression.Width = cmdDelete.Left - ctlExpression.Left - 4
                Case IfEditorChildMode.ElseMode
                    lblIf.Text = "Else:"
                    cmdDelete.Visible = True
                    ctlThenScript.Height += ctlThenScript.Top - ctlExpression.Top
                    ctlThenScript.Top = ctlExpression.Top
                    lblThen.Visible = False
                    ctlExpression.Visible = False
                    ctlExpression.Width = cmdDelete.Left - ctlExpression.Left - 4
            End Select

        End Set
    End Property

    Public Property Expanded As Boolean
        Get
            Return m_expanded
        End Get
        Set(value As Boolean)
            Dim oldValue As Boolean = m_expanded
            m_expanded = value
            ctlThenScript.Visible = Expanded
            lblThen.Visible = Expanded
            Dim newHeight As Integer
            If Expanded Then
                newHeight = m_scriptEditorMinHeight
            Else
                newHeight = lblThen.Top + lblThen.Height
            End If

            If (oldValue <> value) Then RaiseEvent ChangeHeight(Me, newHeight)
        End Set
    End Property

    Private Sub IfEditorChild_DoubleClick(sender As Object, e As System.EventArgs) Handles Me.DoubleClick
        ToggleExpand()
    End Sub

    Private Sub cmdExpand_Click(sender As System.Object, e As System.EventArgs) Handles cmdExpand.Click
        ToggleExpand()
    End Sub

    Private Sub ToggleExpand()
        Expanded = Not Expanded
    End Sub

    Private Sub ctlThenScript_HeightChanged(sender As Object, height As Integer) Handles ctlThenScript.HeightChanged
        m_scriptEditorMinHeight = height
        RaiseEvent ChangeHeight(Me, ctlThenScript.Top + height)
    End Sub

    Public ReadOnly Property Script As IEditableScripts
        Get
            Return m_script
        End Get
    End Property

    Private Sub cmdDelete_Click(sender As System.Object, e As System.EventArgs) Handles cmdDelete.Click
        RaiseEvent Delete(Me)
    End Sub

    Public Property ElseIfData As EditableIfScript.EditableElseIf
        Get
            Return m_elseIf
        End Get
        Set(value As EditableIfScript.EditableElseIf)
            m_elseIf = value
        End Set
    End Property

End Class
