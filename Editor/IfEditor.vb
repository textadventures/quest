Public Class IfEditor

    Implements ICommandEditor

    Private m_controller As EditorController
    Private m_data As IEditorData
    Private m_currentSplitter As SplitContainer

    Public Event Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs) Implements ICommandEditor.Dirty

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        m_currentSplitter = ctlSplitContainer
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

    Private Sub ctlChild_Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs) Handles ctlChild.Dirty
        RaiseEvent Dirty(sender, args)
    End Sub

    Private Sub AddElseIf()
        m_currentSplitter.Panel2Collapsed = False
        Dim newSplitter As New SplitContainer
        newSplitter.Panel1MinSize = 0
        newSplitter.Panel2MinSize = 0
        newSplitter.Parent = m_currentSplitter.Panel2
        newSplitter.Dock = DockStyle.Fill
        newSplitter.Panel2Collapsed = True
        newSplitter.Orientation = Orientation.Horizontal
        Dim newIfEditorChild As New IfEditorChild
        newIfEditorChild.Parent = newSplitter.Panel1
        newIfEditorChild.Dock = DockStyle.Fill
        newIfEditorChild.Controller = m_controller

        ' also need to populate newchild?
        ' also need to hook up Dirty event

        newSplitter.Visible = True
        newIfEditorChild.Visible = True
        m_currentSplitter = newSplitter
    End Sub

    ' need logic to only add an "else" once, and update button/menu accordingly
    ' as many "else if"s as we like
    ' "else" should appear at the bottom so have a separate parent splitter

    Private Sub cmdAddElse_ButtonClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdAddElse.ButtonClick
        AddElseIf()
    End Sub
End Class
