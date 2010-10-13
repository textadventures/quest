Public Class ScriptCommandEditor

    Private m_controller As EditorController
    Private WithEvents m_currentEditor As ICommandEditor

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
        Dim newEditor As Control
        Dim newEditorKey As String = script.EditorName

        If script.Type = ScriptType.If Then
            Dim newIfEditor = New IfEditor
            newEditor = newIfEditor
            newIfEditor.Populate(script)
        Else
            Dim newElemEditor = New ElementEditor
            newEditor = newElemEditor
            newElemEditor.Initialise(m_controller, m_controller.GetEditorDefinition(script.EditorName))
            newElemEditor.Populate(m_controller.GetScriptEditorData(script))
        End If

        Dim newCommandEditor As ICommandEditor = newEditor
        newCommandEditor.Controller = m_controller

        newEditor.Parent = Me
        newEditor.Dock = DockStyle.Fill

        If Not m_currentEditor Is Nothing Then
            newEditor.Visible = True
            DirectCast(m_currentEditor, Control).Visible = False
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
        Dim elemEditor As ElementEditor = TryCast(m_currentEditor, ElementEditor)
        If elemEditor Is Nothing Then Exit Sub
        elemEditor.UpdateField(index.ToString(), newValue, True)
    End Sub
End Class
