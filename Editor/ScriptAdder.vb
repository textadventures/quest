Public Class ScriptAdder

    Private m_controller As EditorController
    Private m_selection As String

    Public Event AddScript(keyword As String)
    Public Event CloseButtonClicked()

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ShowCloseButton = False
    End Sub

    Public Property Controller() As EditorController
        Get
            Return m_controller
        End Get
        Set(value As EditorController)
            m_controller = value
        End Set
    End Property

    Public Property ShowCloseButton() As Boolean
        Get
            Return cmdClose.Visible
        End Get
        Set(value As Boolean)
            cmdClose.Visible = value
        End Set
    End Property

    Public Sub Initialise()
        For Each cat As String In m_controller.GetAllScriptEditorCategories
            ctlEditorTree.AddNode(cat, cat, Nothing, Nothing, Nothing)
        Next

        For Each data As KeyValuePair(Of String, EditableScriptData) In m_controller.GetScriptEditorData
            ctlEditorTree.AddNode(data.Key, data.Value.AdderDisplayString, data.Value.Category, Nothing, Nothing)
        Next

        ctlEditorTree.ExpandAll()
    End Sub

    Private Sub cmdAdd_Click(sender As System.Object, e As System.EventArgs) Handles cmdAdd.Click
        AddCurrent()
    End Sub

    Private Sub ctlEditorTree_CommitSelection() Handles ctlEditorTree.CommitSelection
        AddCurrent()
    End Sub

    Private Sub ctlEditorTree_SelectionChanged(key As String) Handles ctlEditorTree.SelectionChanged
        m_selection = key
    End Sub

    Public Sub AddCurrent()
        Dim data As EditableScriptData = Nothing
        If (Not m_selection Is Nothing) AndAlso m_controller.GetScriptEditorData().TryGetValue(m_selection, data) Then
            RaiseEvent AddScript(data.CreateString)
        End If
    End Sub

    Private Sub cmdClose_Click(sender As Object, e As System.EventArgs) Handles cmdClose.Click
        RaiseEvent CloseButtonClicked()
    End Sub
End Class
