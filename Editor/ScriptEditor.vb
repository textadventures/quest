Public Class ScriptEditor

    Private m_controller As EditorController
    Private WithEvents m_scripts As IEditableScripts
    Private m_editIndex As Integer
    Private m_elementName As String
    Private m_attribute As String
    Private m_currentScript As IEditableScript

    Public Event Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs)

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        UpdateList()
        lstScripts.Items(0).Selected = True
    End Sub

    Private Sub ctlScriptAdder_AddScript(ByVal keyword As String) Handles ctlScriptAdder.AddScript
        If m_scripts Is Nothing Then
            m_scripts = m_controller.CreateNewEditableScripts(m_elementName, m_attribute, keyword)
        Else
            m_scripts.AddNew(keyword, m_elementName)
        End If
        UpdateList()
        lstScripts.SelectedIndices.Clear()
        lstScripts.SelectedIndices.Add(lstScripts.Items.Count - 2)
    End Sub

    Public Property Controller() As EditorController
        Get
            Return m_controller
        End Get
        Set(ByVal value As EditorController)
            m_controller = value
            ctlScriptCommandEditor.Controller = value
            ctlScriptAdder.Controller = value
        End Set
    End Property

    Private Sub UpdateList()
        lstScripts.Items.Clear()
        If Not m_scripts Is Nothing Then
            For Each script As IEditableScript In m_scripts.Scripts
                lstScripts.Items.Add(FormatScript(script.DisplayString))
            Next
        End If
        lstScripts.Items.Add("Add a new command...")
    End Sub

    Private Sub ctlScriptCommandEditor_Dirty(ByVal sender As Object, ByVal args As DataModifiedEventArgs) Handles ctlScriptCommandEditor.Dirty
        lstScripts.Items(m_editIndex).Text = FormatScript(args.NewValue)
        Dim newArgs As New DataModifiedEventArgs(String.Empty, m_scripts.DisplayString(m_editIndex, args.NewValue))
        RaiseEvent Dirty(Me, newArgs)
    End Sub

    Private Sub lstScripts_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstScripts.SelectedIndexChanged
        If lstScripts.SelectedIndices.Count = 0 Then Exit Sub

        m_editIndex = lstScripts.SelectedIndices.Item(0)
        ShowEditor(m_editIndex)
    End Sub

    Private Function FormatScript(ByVal script As String) As String
        Return script.Replace(Environment.NewLine, " / ")
    End Function

    Private Sub ShowEditor(ByVal index As Integer)
        Dim showAdder As Boolean = (m_scripts Is Nothing OrElse index >= m_scripts.Scripts.Count)
        ctlScriptAdder.Visible = showAdder
        ctlScriptCommandEditor.Visible = Not showAdder

        If showAdder Then
            m_currentScript = Nothing
        Else
            m_currentScript = m_scripts(index)
            ctlScriptCommandEditor.ShowEditor(m_currentScript)
        End If
    End Sub

    Public Property Value() As IEditableScripts
        Get
            ctlScriptCommandEditor.Save()
            Return m_scripts
        End Get
        Set(ByVal value As IEditableScripts)
            m_scripts = value
            UpdateList()
        End Set
    End Property

    Public Property ElementName() As String
        Get
            Return m_elementName
        End Get
        Set(ByVal value As String)
            m_elementName = value
        End Set
    End Property

    Public Property AttributeName() As String
        Get
            Return m_attribute
        End Get
        Set(ByVal value As String)
            m_attribute = value
        End Set
    End Property

    Private Sub m_scripts_Updated(ByVal sender As Object, ByVal e As EditableScriptsUpdatedEventArgs) Handles m_scripts.Updated
        If (e.UpdatedScript Is Nothing) Then
            ' Update to the whole script list
            UpdateList()
        Else
            ' Update to one script within the list
            If e.UpdatedScript Is m_currentScript Then
                ctlScriptCommandEditor.UpdateField(e.UpdatedScriptEventArgs.Index, e.UpdatedScriptEventArgs.NewValue)
            Else
                UpdateList()
            End If
        End If
    End Sub

    Public Sub Save()
        ctlScriptCommandEditor.Save()
    End Sub

    Public Sub Initialise()
        ctlScriptAdder.Initialise()
    End Sub

    Private Sub cmdDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdDelete.Click
        m_scripts.Remove(m_editIndex)
    End Sub
End Class
