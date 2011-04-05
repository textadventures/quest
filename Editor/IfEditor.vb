Public Class IfEditor

    Implements ICommandEditor

    Private m_controller As EditorController
    Private WithEvents m_data As EditableIfScript
    Private m_children As New List(Of IfEditorChild)
    Private m_lastEditorChild As IfEditorChild
    Private m_hasElse As Boolean
    Private m_elseEditor As IfEditorChild

    Public Event Dirty(sender As Object, args As DataModifiedEventArgs) Implements ICommandEditor.Dirty

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        AddChild(ctlChild)
    End Sub

    Private Sub AddChild(child As IfEditorChild)
        If Not m_hasElse Then
            m_children.Add(child)
        Else
            ' if we have an "else" then we want to insert a new "else if" before the "else"
            m_children.Insert(m_children.Count - 2, child)
        End If

        If child.ElseIfMode = IfEditorChild.IfEditorChildMode.ElseMode Then
            m_hasElse = True
        End If

        AddHandler child.ChangeHeight, AddressOf IfEditorChild_HeightChanged
        AddHandler child.Dirty, AddressOf IfEditorChild_Dirty
        m_lastEditorChild = child
    End Sub

    Public Sub SaveData() Implements ICommandEditor.SaveData
        For Each child As IfEditorChild In m_children
            child.SaveData(m_data)
        Next
    End Sub

    Public Property Controller As EditorController Implements ICommandEditor.Controller
        Get
            Return m_controller
        End Get
        Set(value As EditorController)
            m_controller = value
            For Each child As IfEditorChild In m_children
                child.Controller = value
            Next
        End Set
    End Property

    Public Sub Populate(data As EditableIfScript)
        m_data = data
        ' The expression is contained in the "expression" attribute of the data IEditorData
        ctlChild.Populate(data, data.ThenScript)
        If Not data.ElseScript Is Nothing Then
            AddElseChildControl()
        End If
    End Sub

    Public Sub UpdateField(attribute As String, newValue As Object, setFocus As Boolean) Implements ICommandEditor.UpdateField
        ctlChild.UpdateField(attribute, newValue, setFocus)
    End Sub

    Private Sub IfEditorChild_Dirty(sender As Object, args As DataModifiedEventArgs)
        Dim newArgs As New DataModifiedEventArgs(String.Empty, m_data.DisplayString(GetChildEditorSectionName(sender), CInt(args.Attribute), DirectCast(args.NewValue, String)))
        RaiseEvent Dirty(sender, newArgs)
    End Sub

    Private Function GetChildEditorSectionName(child As Object) As String
        If child Is ctlChild Then
            Return "then"
        End If

        If child Is m_elseEditor Then
            Return "else"
        End If

        Throw New NotImplementedException
    End Function

    Private Sub AddElseChildControl()
        Dim newChild As IfEditorChild = AddElseChildControl(False)
        m_elseEditor = newChild
        newChild.Populate(Nothing, m_data.ElseScript)
    End Sub

    Private Function AddElseChildControl(addElseIf As Boolean) As IfEditorChild
        ctlChild.Dock = DockStyle.None
        Dim newIfEditorChild As New IfEditorChild
        newIfEditorChild.Parent = pnlContainer
        newIfEditorChild.Controller = m_controller
        newIfEditorChild.Top = m_lastEditorChild.Top + m_lastEditorChild.Height     ' not necessarily....
        newIfEditorChild.Width = pnlContainer.Width
        newIfEditorChild.Anchor = newIfEditorChild.Anchor Or AnchorStyles.Right
        newIfEditorChild.ElseIfMode = If(addElseIf, IfEditorChild.IfEditorChildMode.ElseIfMode, IfEditorChild.IfEditorChildMode.ElseMode)
        newIfEditorChild.Visible = True
        AddChild(newIfEditorChild)
        Return newIfEditorChild
    End Function

    Private Sub RemoveElseChildControl()
        m_elseEditor.Parent = Nothing
        m_elseEditor.Populate(Nothing, Nothing)
        m_children.Remove(m_elseEditor)
        m_elseEditor.Dispose()
        m_elseEditor = Nothing
        m_hasElse = False
    End Sub

    Private Sub IfEditorChild_HeightChanged(sender As IfEditorChild, newHeight As Integer)
        sender.Height = newHeight
        Dim currentTop = 0
        For Each child As IfEditorChild In m_children
            child.Top = currentTop
            Debug.Assert(Not (child Is ctlChild And currentTop > 0))
            currentTop += child.Height
        Next
    End Sub

    ' need logic to only add an "else" once, and update button/menu accordingly
    ' as many "else if"s as we like
    ' "else" should appear at the bottom so have a separate parent splitter

    Private Sub cmdAddElse_ButtonClick(sender As System.Object, e As System.EventArgs) Handles cmdAddElse.ButtonClick
        'AddElse(False)
        AddElse()
    End Sub

    Private Sub mnuAddElse_Click(sender As Object, e As System.EventArgs) Handles mnuAddElse.Click
        'AddElse(False)
        AddElse()
    End Sub

    Private Sub mnuAddElseIf_Click(sender As Object, e As System.EventArgs) Handles mnuAddElseIf.Click
        'AddElse(True)
    End Sub

    Private Sub AddElse()
        m_data.AddElse()

        Dim args As New DataModifiedEventArgs(String.Empty, m_data.DisplayString())
        RaiseEvent Dirty(Me, args)
    End Sub

    Private Sub m_data_AddedElse(sender As Object, e As System.EventArgs) Handles m_data.AddedElse
        AddElseChildControl()
    End Sub

    Private Sub m_data_RemovedElse(sender As Object, e As System.EventArgs) Handles m_data.RemovedElse
        RemoveElseChildControl()
    End Sub

End Class
