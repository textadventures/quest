Public Class IfEditor

    Implements ICommandEditor

    Private m_controller As EditorController
    Private m_data As IEditorData
    Private m_children As New List(Of IfEditorChild)
    Private m_lastEditorChild As IfEditorChild

    Public Event Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs) Implements ICommandEditor.Dirty

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        AddChild(ctlChild)
    End Sub

    Private Sub AddChild(ByVal child As IfEditorChild)
        m_children.Add(child)
        AddHandler child.ChangeHeight, AddressOf IfEditorChild_HeightChanged
        m_lastEditorChild = child
    End Sub

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

    Private Sub ctlChild_Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs)
        RaiseEvent Dirty(sender, args)
    End Sub

    Private Sub AddElseIf()
        ctlChild.Dock = DockStyle.None
        Dim newIfEditorChild As New IfEditorChild
        newIfEditorChild.Parent = pnlContainer
        newIfEditorChild.Controller = m_controller
        newIfEditorChild.Top = m_lastEditorChild.Top + m_lastEditorChild.Height
        newIfEditorChild.Width = pnlContainer.Width
        newIfEditorChild.Anchor = newIfEditorChild.Anchor Or AnchorStyles.Right
        newIfEditorChild.Visible = True
        AddChild(newIfEditorChild)

        ' also need to populate newchild?
        ' also need to hook up Dirty event
    End Sub

    Private Sub IfEditorChild_HeightChanged(ByVal sender As IfEditorChild, ByVal newHeight As Integer)
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

    Private Sub cmdAddElse_ButtonClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdAddElse.ButtonClick
        AddElseIf()
    End Sub


End Class
