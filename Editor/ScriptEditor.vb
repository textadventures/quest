Public Class ScriptEditor

    Private m_controller As EditorController
    Private WithEvents m_scripts As IEditableScripts
    Private m_editIndex As Integer
    Private m_elementName As String
    Private m_attribute As String
    Private m_currentScript As IEditableScript
    Private m_isPopOut As Boolean
    Private m_showingAdder As Boolean
    Private m_readOnly As Boolean

    Public Event Dirty(sender As Object, args As DataModifiedEventArgs)
    Public Event CloseButtonClicked()
    Public Event HeightChanged(sender As Object, height As Integer)

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        PopOut = False
        UpdateList()
        lstScripts.Items(0).Selected = True
    End Sub

    Private Sub ctlScriptAdder_AddScript(keyword As String) Handles ctlScriptAdder.AddScript
        If m_scripts Is Nothing Then
            m_scripts = m_controller.CreateNewEditableScripts(m_elementName, m_attribute, keyword, True)
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
        Set(value As EditorController)
            m_controller = value
            ctlScriptCommandEditor.Controller = value
            ctlScriptAdder.Controller = value
        End Set
    End Property

    Public Sub UpdateList()
        Dim oldSelection As Integer? = Nothing

        If lstScripts.SelectedIndices.Count > 0 Then
            oldSelection = lstScripts.SelectedIndices(0)
        End If

        lstScripts.Items.Clear()
        If Not m_scripts Is Nothing Then
            For Each script As IEditableScript In m_scripts.Scripts
                lstScripts.Items.Add(Utility.FormatAsOneLine(script.DisplayString))
            Next
        End If
        lstScripts.Items.Add("Add a new command...")

        If (oldSelection.HasValue) Then
            If oldSelection > -1 And lstScripts.Items.Count - 1 >= oldSelection Then
                lstScripts.Items(oldSelection.Value).Selected = True
            End If
        End If

        If lstScripts.SelectedIndices.Count = 0 Then
            lstScripts.Items(0).Selected = True
        End If

        If Not m_scripts Is Nothing Then
            Dim newArgs As New DataModifiedEventArgs(String.Empty, m_scripts.DisplayString())
            RaiseEvent Dirty(Me, newArgs)
        End If
    End Sub

    Private Sub ctlScriptCommandEditor_CloseButtonClicked() Handles ctlScriptCommandEditor.CloseButtonClicked
        RaiseEvent CloseButtonClicked()
    End Sub

    Private Sub ctlScriptCommandEditor_Dirty(sender As Object, args As DataModifiedEventArgs) Handles ctlScriptCommandEditor.Dirty
        lstScripts.Items(m_editIndex).Text = Utility.FormatAsOneLine(DirectCast(args.NewValue, String))
        Dim newArgs As New DataModifiedEventArgs(String.Empty, m_scripts.DisplayString(m_editIndex, DirectCast(args.NewValue, String)))
        RaiseEvent Dirty(Me, newArgs)
    End Sub

    Private Sub lstScripts_SelectedIndexChanged(sender As Object, e As System.EventArgs) Handles lstScripts.SelectedIndexChanged
        If lstScripts.SelectedIndices.Count = 0 Then
            SetEditButtonsEnabled(False)
            Exit Sub
        End If

        m_editIndex = lstScripts.SelectedIndices.Item(0)
        ShowEditor(m_editIndex)
    End Sub

    Private Sub ShowEditor(index As Integer)
        Dim showAdder As Boolean = (m_scripts Is Nothing OrElse index >= m_scripts.Scripts.Count)

        Me.SuspendLayout()
        ctlScriptAdder.Visible = showAdder
        ctlScriptCommandEditor.Visible = Not showAdder
        m_showingAdder = showAdder

        If showAdder Then
            SetEditButtonsEnabled(False)
            m_currentScript = Nothing
            ctlScriptCommandEditor.ShowEditor(Nothing)
            ctlScriptAdder.ScrollToTop()
        Else
            SetEditButtonsEnabled(True)
            If m_currentScript IsNot m_scripts(index) Then
                m_currentScript = m_scripts(index)
                ctlScriptCommandEditor.ShowEditor(m_currentScript)
            End If
        End If

        UpdateHeight()
        Me.ResumeLayout()
    End Sub

    Private Sub UpdateHeight()
        If m_showingAdder Then
            RaiseEvent HeightChanged(Me, ctlToolStrip.Height + ctlContainer.SplitterDistance + ctlScriptAdder.Top + 300)
        Else
            RaiseEvent HeightChanged(Me, ctlToolStrip.Height + ctlContainer.SplitterDistance + ctlScriptCommandEditor.Top + ctlScriptCommandEditor.MinHeight)
        End If
    End Sub

    Public Property Value() As IEditableScripts
        Get
            ctlScriptCommandEditor.Save()
            Return m_scripts
        End Get
        Set(value As IEditableScripts)
            m_scripts = value
            UpdateList()
        End Set
    End Property

    Public Property ElementName() As String
        Get
            Return m_elementName
        End Get
        Set(value As String)
            m_elementName = value
        End Set
    End Property

    Public Property AttributeName() As String
        Get
            Return m_attribute
        End Get
        Set(value As String)
            m_attribute = value
        End Set
    End Property

    Private Sub m_scripts_Updated(sender As Object, e As EditableScriptsUpdatedEventArgs) Handles m_scripts.Updated
        If (e.UpdatedScript Is Nothing) Then
            ' Update to the whole script list
            UpdateList()
        Else
            ' Update to one script within the list
            If e.UpdatedScript Is m_currentScript Then
                If e.UpdatedScriptEventArgs.IsParameterUpdate Then
                    ctlScriptCommandEditor.UpdateField(e.UpdatedScriptEventArgs.Index, e.UpdatedScriptEventArgs.NewValue)
                ElseIf e.UpdatedScriptEventArgs.IsNamedParameterUpdate Then
                    ctlScriptCommandEditor.UpdateField(e.UpdatedScriptEventArgs.Id, e.UpdatedScriptEventArgs.NewValue)
                ElseIf e.UpdatedScriptEventArgs.IsWholeScriptUpdate Then
                    UpdateList()
                End If
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

    Private Sub cmdDelete_Click(sender As System.Object, e As System.EventArgs) Handles cmdDelete.Click
        Save()
        If m_editIndex < m_scripts.Count Then
            m_scripts.Remove(m_editIndex)
        End If
    End Sub

    Private Sub cmdPopOut_Click(sender As System.Object, e As System.EventArgs) Handles cmdPopOut.Click
        Save()
        PopupEditors.EditScript(m_controller, m_scripts, m_attribute, m_elementName, IsReadOnly)
        UpdateList()
    End Sub

    Public Property PopOut() As Boolean
        Get
            Return m_isPopOut
        End Get
        Set(value As Boolean)
            m_isPopOut = value
            cmdPopOut.Visible = Not value
            ctlScriptAdder.ShowCloseButton = value
            ctlScriptCommandEditor.ShowCloseButton = value
        End Set
    End Property

    Public Property Scripts As IEditableScripts
        Get
            Return m_scripts
        End Get
        Set(value As IEditableScripts)
            m_scripts = value
        End Set
    End Property

    Private Sub ctlScriptAdder_CloseButtonClicked() Handles ctlScriptAdder.CloseButtonClicked
        RaiseEvent CloseButtonClicked()
    End Sub

    Private Sub ScriptEditor_Resize(sender As Object, e As System.EventArgs) Handles Me.Resize
        chScript.Width = lstScripts.Width - lstScripts.Margin.Left * 2 - SystemInformation.VerticalScrollBarWidth
    End Sub

    Private Sub SetEditButtonsEnabled(enabled As Boolean)
        If m_readOnly Then enabled = False
        cmdDelete.Enabled = enabled
        cmdMoveUp.Enabled = enabled
        cmdMoveDown.Enabled = enabled
    End Sub

    Private Sub ctlContainer_SplitterMoved(sender As Object, e As System.Windows.Forms.SplitterEventArgs) Handles ctlContainer.SplitterMoved
        If Me.IsHandleCreated Then
            ' Run UpdateHeight asynchronously as we want the resize to have finished before calling it. Otherwise
            ' the contents of Panel2 are squashed by the splitter resize.
            BeginInvoke(Sub() UpdateHeight())
        End If
    End Sub

    Public Property IsReadOnly As Boolean
        Get
            Return m_readOnly
        End Get
        Set(value As Boolean)
            m_readOnly = value

            SetEditButtonsEnabled(False)
            ctlScriptAdder.IsReadOnly = m_readOnly
            ctlScriptCommandEditor.IsReadOnly = m_readOnly
        End Set
    End Property

End Class
