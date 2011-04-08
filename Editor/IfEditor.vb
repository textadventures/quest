Public Class IfEditor

    Implements ICommandEditor

    Private m_controller As EditorController
    Private WithEvents m_data As EditableIfScript
    Private m_activeChildren As New List(Of IfEditorChild)
    Private m_inactiveChildren As New List(Of IfEditorChild)
    Private m_lastEditorChild As IfEditorChild
    Private m_hasElse As Boolean
    Private m_elseEditor As IfEditorChild
    Private m_fullHeight As Integer
    Private m_elseIfEditor As New Dictionary(Of String, IfEditorChild)

    Public Event Dirty(sender As Object, args As DataModifiedEventArgs) Implements ICommandEditor.Dirty

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        AddChild(ctlChild)
    End Sub

    Private Sub AddChild(child As IfEditorChild)
        If Not m_hasElse Then
            m_activeChildren.Add(child)
        Else
            ' if we have an "else" then we want to insert a new "else if" before the "else"
            m_activeChildren.Insert(m_activeChildren.Count - 1, child)
        End If

        If child.ElseIfMode = IfEditorChild.IfEditorChildMode.ElseMode Then
            m_hasElse = True
        End If

        UpdateElseButton()

        AddHandler child.ChangeHeight, AddressOf IfEditorChild_HeightChanged
        AddHandler child.Dirty, AddressOf IfEditorChild_Dirty
        AddHandler child.Delete, AddressOf IfEditorChild_Delete
        m_lastEditorChild = child
    End Sub

    Public Sub SaveData() Implements ICommandEditor.SaveData
        For Each child As IfEditorChild In m_activeChildren
            child.SaveData()
        Next
    End Sub

    Public Property Controller As EditorController Implements ICommandEditor.Controller
        Get
            Return m_controller
        End Get
        Set(value As EditorController)
            m_controller = value
            For Each child As IfEditorChild In m_activeChildren
                child.Controller = value
            Next
        End Set
    End Property

    Public Sub Populate(data As EditableIfScript)
        LayoutSuspend()
        m_elseIfEditor.Clear()
        m_elseEditor = Nothing
        m_hasElse = False
        RemoveAllChildren()

        m_data = data
        ' The expression is contained in the "expression" attribute of the data IEditorData
        ctlChild.Populate(data, data.ThenScript)

        For Each elseIfScript In data.ElseIfScripts
            AddElseIfChildControl(elseIfScript)
        Next

        If Not data.ElseScript Is Nothing Then
            AddElseChildControl()
        End If
        LayoutResume()
    End Sub

    Public Sub UpdateField(attribute As String, newValue As Object, setFocus As Boolean) Implements ICommandEditor.UpdateField
        If m_elseIfEditor.ContainsKey(attribute) Then
            ' this is an update to an "else if" expression
            m_elseIfEditor(attribute).UpdateField("0", newValue, setFocus)
        Else
            ctlChild.UpdateField(attribute, newValue, setFocus)
        End If
    End Sub

    Private Sub IfEditorChild_Dirty(sender As Object, args As DataModifiedEventArgs)
        Dim newArgs As New DataModifiedEventArgs(String.Empty, m_data.DisplayString(GetChildEditorScript(sender), CInt(args.Attribute), DirectCast(args.NewValue, String)))
        RaiseEvent Dirty(sender, newArgs)
    End Sub

    Private Sub IfEditorChild_HeightChanged(sender As IfEditorChild, newHeight As Integer)
        sender.Height = newHeight
        SetChildControlPositions()
    End Sub

    Private Sub IfEditorChild_Delete(sender As IfEditorChild)
        If sender Is m_elseEditor Then
            m_data.RemoveElse()
        Else
            m_data.RemoveElseIf(sender.ElseIfData)
        End If
    End Sub

    Private Function GetChildEditorScript(child As Object) As IEditableScripts
        Dim childEditor As IfEditorChild = DirectCast(child, IfEditorChild)
        Return childEditor.Script
    End Function

    Private Sub AddElseChildControl()
        Dim newChild As IfEditorChild = AddElseChildControl(False)
        m_elseEditor = newChild
        newChild.Populate(Nothing, m_data.ElseScript)
    End Sub

    Private Sub AddElseIfChildControl(elseIfData As EditableIfScript.EditableElseIf)
        Dim newChild As IfEditorChild = AddElseChildControl(True)
        m_elseIfEditor.Add(elseIfData.Id, newChild)
        newChild.Populate(elseIfData, elseIfData.EditableScripts)
        newChild.ElseIfData = elseIfData
    End Sub

    Private Function AddElseChildControl(addElseIf As Boolean) As IfEditorChild
        LayoutSuspend()
        ctlChild.Dock = DockStyle.None
        Dim newIfEditorChild As IfEditorChild

        ' Reuse an unused existing IfEditorChild if possible, otherwise create a new one
        If (m_inactiveChildren.Count > 0) Then
            newIfEditorChild = m_inactiveChildren(0)
            m_inactiveChildren.Remove(newIfEditorChild)
            newIfEditorChild.Visible = True
        Else
            newIfEditorChild = New IfEditorChild
            newIfEditorChild.Controller = m_controller
            newIfEditorChild.Parent = pnlContainer
            newIfEditorChild.Width = pnlContainer.Width
            newIfEditorChild.Anchor = newIfEditorChild.Anchor Or AnchorStyles.Right
        End If

        newIfEditorChild.ElseIfMode = If(addElseIf, IfEditorChild.IfEditorChildMode.ElseIfMode, IfEditorChild.IfEditorChildMode.ElseMode)
        AddChild(newIfEditorChild)
        SetChildControlPositions()
        LayoutResume()
        Return newIfEditorChild
    End Function

    Private Sub RemoveElseChildControl()
        RemoveChild(m_elseEditor)
        m_elseEditor = Nothing
        m_hasElse = False
        UpdateElseButton()
    End Sub

    Private Sub RemoveAllChildren()
        ' Remove all "else if" and "else" controls i.e. everything apart from "then", as that's always present
        Dim children As New List(Of IfEditorChild)(m_activeChildren)
        For Each child In children
            If child IsNot ctlChild Then
                RemoveChild(child)
            End If
        Next
    End Sub

    Private Sub RemoveChild(child As IfEditorChild)
        LayoutSuspend()
        RemoveHandler child.ChangeHeight, AddressOf IfEditorChild_HeightChanged
        RemoveHandler child.Dirty, AddressOf IfEditorChild_Dirty
        RemoveHandler child.Delete, AddressOf IfEditorChild_Delete
        child.Populate(Nothing, Nothing)
        child.Visible = False
        m_activeChildren.Remove(child)
        m_inactiveChildren.Add(child)
        SetChildControlPositions()
        LayoutResume()
    End Sub

    Private Sub SetChildControlPositions()
        Dim currentTop = 0

        LayoutSuspend()
        For Each child As IfEditorChild In m_activeChildren
            child.Top = currentTop
            Debug.Assert(Not (child Is ctlChild And currentTop > 0))
            currentTop += child.Height
        Next
        LayoutResume()

        m_fullHeight = currentTop
    End Sub

    Private Sub UpdateElseButton()
        cmdAddElse.Text = If(Not m_hasElse, "Add Else", "Add Else If")
        mnuAddElse.Enabled = Not m_hasElse
    End Sub

    Private Sub cmdAddElse_ButtonClick(sender As System.Object, e As System.EventArgs) Handles cmdAddElse.ButtonClick
        If Not m_hasElse Then
            AddElse()
        Else
            AddElseIf()
        End If
    End Sub

    Private Sub mnuAddElse_Click(sender As Object, e As System.EventArgs) Handles mnuAddElse.Click
        AddElse()
    End Sub

    Private Sub mnuAddElseIf_Click(sender As Object, e As System.EventArgs) Handles mnuAddElseIf.Click
        AddElseIf()
    End Sub

    Private Sub AddElse()
        m_data.AddElse()
        RaiseDirtyEvent()
    End Sub

    Private Sub AddElseIf()
        m_data.AddElseIf()
        RaiseDirtyEvent()
    End Sub

    Private Sub RaiseDirtyEvent()
        Dim args As New DataModifiedEventArgs(String.Empty, m_data.DisplayString())
        RaiseEvent Dirty(Me, args)
    End Sub

    Private Sub m_data_AddedElse(sender As Object, e As System.EventArgs) Handles m_data.AddedElse
        AddElseChildControl()
    End Sub

    Private Sub m_data_RemovedElse(sender As Object, e As System.EventArgs) Handles m_data.RemovedElse
        RemoveElseChildControl()
    End Sub

    Private Sub m_data_AddedElseIf(sender As Object, e As EditableIfScript.ElseIfEventArgs) Handles m_data.AddedElseIf
        AddElseIfChildControl(e.Script)
    End Sub

    Private Sub m_data_RemovedElseIf(sender As Object, e As EditableIfScript.ElseIfEventArgs) Handles m_data.RemovedElseIf
        Dim childControl As IfEditorChild = m_elseIfEditor(e.Script.Id)
        RemoveChild(childControl)
        m_elseIfEditor.Remove(e.Script.Id)
    End Sub

    Public ReadOnly Property MinHeight As Integer Implements ICommandEditor.MinHeight
        Get
            Return m_fullHeight
        End Get
    End Property

    Private m_layoutSuspendStack As Integer = 0

    Private Sub LayoutSuspend()
        If m_layoutSuspendStack = 0 Then
            Me.SuspendLayout()
        End If

        m_layoutSuspendStack += 1
    End Sub

    Private Sub LayoutResume()
        m_layoutSuspendStack -= 1
        If m_layoutSuspendStack = 0 Then
            Me.ResumeLayout()
        End If
    End Sub

End Class
