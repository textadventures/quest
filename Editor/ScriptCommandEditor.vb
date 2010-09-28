Public Class ScriptCommandEditor

    Private m_controller As EditorController
    Private WithEvents m_currentEditor As ElementEditor

    Public Event Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs)

    Public Property Controller() As EditorController
        Get
            Return m_controller
        End Get
        Set(ByVal value As EditorController)
            m_controller = value
        End Set
    End Property

    Public Sub ShowEditor(ByVal script As IEditableScript)
        Dim newEditor As ElementEditor
        Dim newEditorKey As String = script.EditorName

        newEditor = New ElementEditor
        newEditor.Parent = Me
        newEditor.Dock = DockStyle.Fill
        newEditor.Initialise(m_controller, m_controller.GetEditorDefinition(script.EditorName))
        newEditor.Populate(m_controller.GetScriptEditorData(script))

        If Not m_currentEditor Is Nothing Then
            newEditor.Visible = True
            m_currentEditor.Visible = False
        End If

        m_currentEditor = newEditor
    End Sub

    Public Sub Save()
        If Not m_currentEditor Is Nothing Then
            m_currentEditor.SaveData()
        End If
    End Sub

    Private Sub m_editor_Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs) Handles m_currentEditor.Dirty
        RaiseEvent Dirty(sender, args)
    End Sub

    Public Sub UpdateField(ByVal index As Integer, ByVal newValue As String)
        m_currentEditor.UpdateField(index.ToString(), newValue, True)
    End Sub
End Class
