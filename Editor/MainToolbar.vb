Public Class MainToolbar

    Private Class HistoryItem
        Public TreeKey As String
        Public Text As String
    End Class

    Public Delegate Sub ButtonClickHandler()

    Private Const k_numberHistory As Integer = 20
    Private m_mnuBack(k_numberHistory) As ToolStripMenuItem
    Private m_mnuForward(k_numberHistory) As ToolStripMenuItem
    Private m_history As New List(Of HistoryItem)
    Private m_historyPosition As Integer
    Private m_Initialising As Boolean
    Private m_handlers As New Dictionary(Of String, ButtonClickHandler)
    Private m_mnuUndo(k_numberHistory) As ToolStripMenuItem
    Private m_mnuRedo(k_numberHistory) As ToolStripMenuItem
    Private m_defaultColour As Color
    Private m_buttons As New Dictionary(Of String, ToolStripButton)
    Private m_codeView As Boolean
    Private m_simpleMode As Boolean
    Private m_editorStyle As EditorStyle = Quest.EditorStyle.TextAdventure

    Public Event HistoryClicked(key As String)
    Public Event SaveCurrentEditor()
    Public Event UndoClicked(level As Integer)
    Public Event RedoClicked(level As Integer)
    Public Event UndoEnabled(enabled As Boolean)
    Public Event RedoEnabled(enabled As Boolean)

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()
        InitialiseToolbar()

        ' Add any initialization after the InitializeComponent() call.
        CodeView = False
        Me.Height = ctlToolStrip.Height
    End Sub

    Public WriteOnly Property CanCut() As Boolean
        Set(value As Boolean)
            butCut.Enabled = value
        End Set
    End Property

    Public WriteOnly Property CanCopy() As Boolean
        Set(value As Boolean)
            butCopy.Enabled = value
        End Set
    End Property

    Public WriteOnly Property CanPaste() As Boolean
        Set(value As Boolean)
            butPaste.Enabled = value
        End Set
    End Property

    Public WriteOnly Property CanDelete() As Boolean
        Set(value As Boolean)
            butDelete.Enabled = value
        End Set
    End Property

    Public WriteOnly Property Initialising() As Boolean
        Set(value As Boolean)
            m_Initialising = value
        End Set
    End Property

    Private Sub InitialiseToolbar()

        InitialiseHistoryMenus(m_mnuBack, butBack, "back")
        InitialiseHistoryMenus(m_mnuForward, butForward, "forward")
        InitialiseUndoMenus(m_mnuUndo, butUndo, "undo", AddressOf HandleUndoClick, AddressOf UndoMenu_MouseEnter, AddressOf UndoMenu_MouseLeave)
        InitialiseUndoMenus(m_mnuRedo, butRedo, "redo", AddressOf HandleRedoClick, AddressOf RedoMenu_MouseEnter, AddressOf RedoMenu_MouseLeave)
        m_defaultColour = m_mnuBack(1).BackColor

        AddClickHandlers()

        ResetHistory()
        ResetUndoMenu()

    End Sub

    Public Sub ResetToolbar()
        ResetHistory()
        ResetUndoMenu()
        m_handlers.Clear()
    End Sub

    Private Sub InitialiseHistoryMenus(ByRef aMenuItem() As ToolStripMenuItem, ByRef butParent As System.Windows.Forms.ToolStripSplitButton, ByRef name As String)
        Dim i As Integer
        Dim mnuMenu As System.Windows.Forms.ToolStripMenuItem

        For i = 1 To k_numberHistory
            aMenuItem(i) = New System.Windows.Forms.ToolStripMenuItem
            mnuMenu = aMenuItem(i)

            mnuMenu.Tag = i.ToString
            mnuMenu.Name = name
            mnuMenu.Visible = False
            mnuMenu.Text = i.ToString
            butParent.DropDownItems.Add(aMenuItem(i))

            AddHandler mnuMenu.Click, AddressOf HandleHistoryClick
        Next
    End Sub

    Private Sub InitialiseUndoMenus(ByRef aMenuItem() As ToolStripMenuItem, ByRef butParent As System.Windows.Forms.ToolStripSplitButton, ByRef name As String, ByRef clickHandler As EventHandler, ByRef enterHandler As EventHandler, ByRef leaveHandler As EventHandler)
        Dim i As Integer
        Dim mnuMenu As System.Windows.Forms.ToolStripMenuItem

        For i = 1 To k_numberHistory
            aMenuItem(i) = New System.Windows.Forms.ToolStripMenuItem
            mnuMenu = aMenuItem(i)

            mnuMenu.Tag = i
            mnuMenu.Name = name
            mnuMenu.Visible = False
            mnuMenu.Text = i.ToString
            butParent.DropDownItems.Add(aMenuItem(i))

            AddHandler mnuMenu.Click, clickHandler
            AddHandler mnuMenu.MouseEnter, enterHandler
            AddHandler mnuMenu.MouseLeave, leaveHandler
        Next
    End Sub

    Private Sub AddClickHandlers()
        For Each item As ToolStripItem In ctlToolStrip.Items
            Dim button As ToolStripButton = TryCast(item, ToolStripButton)
            If button IsNot Nothing Then
                m_buttons.Add(DirectCast(button.Tag, String), button)
                AddHandler button.Click, AddressOf HandleClick
            End If
        Next
    End Sub

    Private Sub HandleClick(sender As Object, e As System.EventArgs)
        RaiseEvent SaveCurrentEditor()

        Dim menu As ToolStripItem = DirectCast(sender, ToolStripItem)

        If Not menu.Tag Is Nothing Then
            HandleClick(DirectCast(menu.Tag, String))
        End If
    End Sub

    Private Sub HandleClick(tag As String)
        If m_handlers.ContainsKey(tag) Then
            m_handlers(tag).Invoke()
        End If
    End Sub

    Private Sub HandleHistoryClick(sender As Object, e As System.EventArgs)
        Dim menu As ToolStripItem = DirectCast(sender, ToolStripItem)
        Dim jump As Integer = CType(menu.Tag, Integer)

        Select Case menu.Name
            Case "back"
                SetHistoryPosition(m_historyPosition - jump)
            Case "forward"
                SetHistoryPosition(m_historyPosition + jump)
        End Select

    End Sub

    Private Sub HandleUndoClick(sender As Object, e As System.EventArgs)
        RaiseEvent UndoClicked(CType(DirectCast(sender, ToolStripMenuItem).Tag, Integer))
    End Sub

    Private Sub HandleRedoClick(sender As Object, e As System.EventArgs)
        RaiseEvent RedoClicked(CType(DirectCast(sender, ToolStripMenuItem).Tag, Integer))
    End Sub

    Public Sub AddHistory(TreeKey As String, Text As String)
        Dim historyItem As New HistoryItem
        historyItem.TreeKey = TreeKey
        historyItem.Text = Text

        If m_historyPosition < m_history.Count - 1 Then
            m_history.RemoveRange(m_historyPosition + 1, m_history.Count - m_historyPosition - 1)
        End If

        m_history.Add(historyItem)

        SetHistoryPosition(m_history.Count - 1, False)
    End Sub

    Private Sub SetHistoryPosition(position As Integer, Optional doRaiseEvent As Boolean = True)
        Dim i As Integer
        Dim index As Integer

        If position < 0 Then Exit Sub
        If position > m_history.Count - 1 Then Exit Sub

        m_historyPosition = position

        For i = 1 To k_numberHistory

            ' Back menu
            index = m_historyPosition - i
            If index < 0 Then
                m_mnuBack(i).Visible = False
            Else
                SetHistoryMenu(m_mnuBack(i), m_history(index))
            End If

            ' Forward menu
            index = m_historyPosition + i
            If index > m_history.Count - 1 Then
                m_mnuForward(i).Visible = False
            Else
                SetHistoryMenu(m_mnuForward(i), m_history(index))
            End If
        Next

        butBack.Enabled = (m_historyPosition > 0)
        butForward.Enabled = (m_historyPosition < m_history.Count - 1)

        If Not m_Initialising And doRaiseEvent Then
            RaiseEvent HistoryClicked(m_history(m_historyPosition).TreeKey)
        End If
    End Sub

    Private Sub SetHistoryMenu(mnuMenu As System.Windows.Forms.ToolStripMenuItem, ByRef historyItem As HistoryItem)
        mnuMenu.Visible = True
        mnuMenu.Text = historyItem.Text
    End Sub

    Private Sub butBack_ButtonClick(sender As System.Object, e As System.EventArgs) Handles butBack.ButtonClick
        SetHistoryPosition(m_historyPosition - 1)
    End Sub

    Private Sub butForward_ButtonClick(sender As System.Object, e As System.EventArgs) Handles butForward.ButtonClick
        SetHistoryPosition(m_historyPosition + 1)
    End Sub

    Public Sub ResetHistory()
        butBack.Enabled = False
        butForward.Enabled = False
        m_history.Clear()
        SetHistoryPosition(0)
    End Sub

    Public Sub AddButtonHandler(key As String, handler As ButtonClickHandler)
        m_handlers.Add(key, handler)
    End Sub

    Private Sub ResetUndoMenu()
        UpdateUndoMenu(Nothing)
        UpdateRedoMenu(Nothing)
    End Sub

    Public Sub UpdateUndoMenu(list As IEnumerable(Of String))
        Dim enabled As Boolean = UpdateUndoMenu(m_mnuUndo, list)
        UndoButtonEnabled = enabled
    End Sub

    Public Sub UpdateRedoMenu(list As IEnumerable(Of String))
        Dim enabled As Boolean = UpdateUndoMenu(m_mnuRedo, list)
        RedoButtonEnabled = enabled
    End Sub

    Private Function UpdateUndoMenu(ByRef aMenuItem() As System.Windows.Forms.ToolStripMenuItem, list As IEnumerable(Of String)) As Boolean

        Dim count As Integer = 0

        If Not list Is Nothing Then
            For Each item As String In list
                count += 1

                aMenuItem(count).Visible = True
                aMenuItem(count).Text = item

                If count = k_numberHistory Then Exit For
            Next
        End If

        For i As Integer = count + 1 To k_numberHistory
            aMenuItem(i).Visible = False
        Next

        Return (count > 0)

    End Function

    Public Sub EnableUndo()
        ' Called when an edit control is made dirty. We need to do this as we won't have an
        ' undo transaction until the user finishes editing, but we want them to be able to
        ' undo the current edit.
        UndoButtonEnabled = True
        RedoButtonEnabled = False
    End Sub

    Public Property UndoButtonEnabled() As Boolean
        Get
            Return butUndo.Enabled
        End Get
        Set(value As Boolean)
            butUndo.Enabled = value
            butUndoSimple.Enabled = value
            RaiseEvent UndoEnabled(value)
        End Set
    End Property

    Public Property RedoButtonEnabled() As Boolean
        Get
            Return butRedo.Enabled
        End Get
        Set(value As Boolean)
            butRedo.Enabled = value
            butRedoSimple.Enabled = value
            RaiseEvent RedoEnabled(value)
        End Set
    End Property

    Private Sub UndoMenu_MouseEnter(sender As Object, e As System.EventArgs)
        UndoRedoMenu_MouseEnter(m_mnuUndo, sender)
    End Sub

    Private Sub UndoMenu_MouseLeave(sender As Object, e As System.EventArgs)
        UndoRedoMenu_MouseLeave()
    End Sub

    Private Sub RedoMenu_MouseEnter(sender As Object, e As System.EventArgs)
        UndoRedoMenu_MouseEnter(m_mnuRedo, sender)
    End Sub

    Private Sub RedoMenu_MouseLeave(sender As Object, e As System.EventArgs)
        UndoRedoMenu_MouseLeave()
    End Sub

    Private Sub UndoRedoMenu_MouseEnter(ByRef menu() As ToolStripMenuItem, sender As Object)
        tmrUndoTimer.Enabled = False
        HighlightMenu(menu, CType(DirectCast(sender, ToolStripMenuItem).Tag, Integer))
    End Sub

    Private Sub UndoRedoMenu_MouseLeave()
        tmrUndoTimer.Enabled = True
    End Sub

    Private Sub HighlightMenu(ByRef aMenuItem() As System.Windows.Forms.ToolStripMenuItem, count As Integer)
        For i As Integer = 1 To count - 1
            aMenuItem(i).BackColor = System.Drawing.SystemColors.MenuHighlight
            aMenuItem(i).Checked = True
        Next
        For i As Integer = count To k_numberHistory
            aMenuItem(i).BackColor = m_defaultColour
            aMenuItem(i).Checked = (i = count)
        Next
    End Sub

    Private Sub tmrUndoTimer_Tick(sender As Object, e As System.EventArgs) Handles tmrUndoTimer.Tick
        ' Unhighlight the Undo menu
        ResetHighlight()
        tmrUndoTimer.Enabled = False
    End Sub

    Private Sub ResetHighlight()
        HighlightMenu(m_mnuUndo, 1)
        HighlightMenu(m_mnuRedo, 1)
        m_mnuUndo(1).Checked = False
        m_mnuRedo(1).Checked = False
    End Sub

    Private Sub butUndo_ButtonClick(sender As System.Object, e As System.EventArgs) Handles butUndo.ButtonClick
        HandleClick("undo")
    End Sub

    Private Sub butRedo_ButtonClick(sender As Object, e As System.EventArgs) Handles butRedo.ButtonClick
        HandleClick("redo")
    End Sub

    Private Sub butUndo_Click(sender As Object, e As System.EventArgs) Handles butUndo.Click
        ' If user has made edits to a field, save it first before displaying the undo list or doing the undo
        RaiseEvent SaveCurrentEditor()
    End Sub

    Private Sub butUndo_DropDownOpening(sender As Object, e As System.EventArgs) Handles butUndo.DropDownOpening
        ' If user has made edits to a field, save it first before displaying the undo list
        RaiseEvent SaveCurrentEditor()
        ResetHighlight()
    End Sub

    Private Sub butRedo_DropDownOpening(sender As Object, e As System.EventArgs) Handles butRedo.DropDownOpening
        ResetHighlight()
    End Sub

    Public Sub RenameHistory(oldKey As String, newKey As String)
        For Each item In m_history.Where(Function(h) h.TreeKey = oldKey)
            item.TreeKey = newKey
        Next
    End Sub

    Public Sub RetitleHistory(key As String, title As String)
        For Each item In m_history.Where(Function(h) h.TreeKey = key)
            item.Text = title
        Next

        For Each mnuItem In (m_mnuBack.Union(m_mnuForward)).Where(Function(m) m IsNot Nothing AndAlso DirectCast(m.Tag, String) = key)
            mnuItem.Text = title
        Next
    End Sub

    Public Sub SetToggle(key As String, checked As Boolean)
        m_buttons(key).Checked = checked
    End Sub

    Public Property CodeView As Boolean
        Get
            Return m_codeView
        End Get
        Set(value As Boolean)
            m_codeView = value
            SetButtonVisibility()
        End Set
    End Property

    Public Property SimpleMode As Boolean
        Get
            Return m_simpleMode
        End Get
        Set(value As Boolean)
            m_simpleMode = value

            SetButtonVisibility()
        End Set
    End Property

    Public Property EditorStyle As EditorStyle
        Get
            Return m_editorStyle
        End Get
        Set(value As EditorStyle)
            If m_editorStyle <> value Then
                m_editorStyle = value
                SetButtonVisibility()
            End If
        End Set
    End Property

    Private Sub SetButtonVisibility()
        butCode.Visible = Not SimpleMode
        butAddPage.Visible = (EditorStyle = Quest.EditorStyle.GameBook) And Not CodeView
        butAddObject.Visible = (EditorStyle = Quest.EditorStyle.TextAdventure) And Not CodeView
        butAddRoom.Visible = (EditorStyle = Quest.EditorStyle.TextAdventure) And Not CodeView
        butUndo.Visible = Not CodeView
        butRedo.Visible = Not CodeView
        butUndoSimple.Visible = CodeView
        butRedoSimple.Visible = CodeView
        butBack.Visible = Not CodeView
        butForward.Visible = Not CodeView
        butDelete.Visible = Not CodeView
        ToolStripSeparator8.Visible = Not CodeView
        ToolStripSeparator2.Visible = Not CodeView
    End Sub
End Class