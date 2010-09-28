Public Class MainToolbar

    Private Class HistoryItem
        Public TreeKey As String
        Public Text As String
    End Class

    Public Delegate Sub ButtonClickHandler()

    Private Const k_numberHistory As Integer = 20
    Private m_mnuBack(k_numberHistory) As System.Windows.Forms.ToolStripMenuItem
    Private m_mnuForward(k_numberHistory) As System.Windows.Forms.ToolStripMenuItem
    Private m_history As New List(Of HistoryItem)
    Private m_historyPosition As Integer
    Private m_Initialising As Boolean
    Private m_handlers As New Dictionary(Of String, ButtonClickHandler)
    Private m_mnuUndo(k_numberHistory) As System.Windows.Forms.ToolStripMenuItem
    Private m_mnuRedo(k_numberHistory) As System.Windows.Forms.ToolStripMenuItem
    Private m_defaultColour As System.Drawing.Color

    Public Event HistoryClicked(ByVal key As String)
    Public Event SaveCurrentEditor()
    Public Event UndoClicked(ByVal level As Integer)
    Public Event RedoClicked(ByVal level As Integer)
    Public Event UndoEnabled(ByVal enabled As Boolean)
    Public Event RedoEnabled(ByVal enabled As Boolean)

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()
        InitialiseToolbar()

        ' Add any initialization after the InitializeComponent() call.
        Me.Height = ctlToolStrip.Height
    End Sub

    Public WriteOnly Property CanCutCopy() As Boolean
        Set(ByVal value As Boolean)
            butCopy.Enabled = value
            butCut.Enabled = value
        End Set
    End Property

    Public WriteOnly Property CanPaste() As Boolean
        Set(ByVal value As Boolean)
            butPaste.Enabled = value
        End Set
    End Property

    Public WriteOnly Property CanDelete() As Boolean
        Set(ByVal value As Boolean)
            butDelete.Enabled = value
        End Set
    End Property

    Public WriteOnly Property Initialising() As Boolean
        Set(ByVal value As Boolean)
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
    End Sub

    Private Sub InitialiseHistoryMenus(ByRef aMenuItem As Array, ByRef butParent As System.Windows.Forms.ToolStripSplitButton, ByRef name As String)
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

    Private Sub InitialiseUndoMenus(ByRef aMenuItem As Array, ByRef butParent As System.Windows.Forms.ToolStripSplitButton, ByRef name As String, ByRef clickHandler As EventHandler, ByRef enterHandler As EventHandler, ByRef leaveHandler As EventHandler)
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
        Dim subMenu As System.Windows.Forms.ToolStripItem

        For Each subMenu In ctlToolStrip.Items
            AddHandler subMenu.Click, AddressOf HandleClick
        Next
    End Sub

    Private Sub HandleClick(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim menu As System.Windows.Forms.ToolStripItem = sender

        If Not menu.Tag Is Nothing Then
            HandleClick(menu.Tag)
        End If
    End Sub

    Private Sub HandleClick(ByVal tag As String)
        If m_handlers.ContainsKey(tag) Then
            m_handlers(tag).Invoke()
        End If
    End Sub

    Private Sub HandleHistoryClick(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim menu As System.Windows.Forms.ToolStripItem
        Dim jump As Integer

        menu = sender
        jump = menu.Tag.ToString

        Select Case menu.Name
            Case "back"
                SetHistoryPosition(m_historyPosition - jump)
            Case "forward"
                SetHistoryPosition(m_historyPosition + jump)
        End Select

    End Sub

    Private Sub HandleUndoClick(ByVal sender As System.Object, ByVal e As System.EventArgs)
        RaiseEvent UndoClicked(DirectCast(sender, ToolStripMenuItem).Tag)
    End Sub

    Private Sub HandleRedoClick(ByVal sender As System.Object, ByVal e As System.EventArgs)
        RaiseEvent RedoClicked(DirectCast(sender, ToolStripMenuItem).Tag)
    End Sub

    Public Sub AddHistory(ByVal TreeKey As String, ByVal Text As String)
        Dim historyItem As New HistoryItem
        historyItem.TreeKey = TreeKey
        historyItem.Text = Text

        If m_historyPosition < m_history.Count - 1 Then
            m_history.RemoveRange(m_historyPosition + 1, m_history.Count - m_historyPosition - 1)
        End If

        m_history.Add(historyItem)

        SetHistoryPosition(m_history.Count - 1, False)
    End Sub

    Private Sub SetHistoryPosition(ByVal position As Integer, Optional ByVal doRaiseEvent As Boolean = True)
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

    Private Sub SetHistoryMenu(ByVal mnuMenu As System.Windows.Forms.ToolStripMenuItem, ByRef historyItem As HistoryItem)
        mnuMenu.Visible = True
        mnuMenu.Text = historyItem.Text
    End Sub

    Private Sub butBack_ButtonClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles butBack.ButtonClick
        SetHistoryPosition(m_historyPosition - 1)
    End Sub

    Private Sub butForward_ButtonClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles butForward.ButtonClick
        SetHistoryPosition(m_historyPosition + 1)
    End Sub

    Public Sub ResetHistory()
        butBack.Enabled = False
        butForward.Enabled = False
        m_history.Clear()
        SetHistoryPosition(0)
    End Sub

    Public Sub AddButtonHandler(ByVal key As String, ByVal handler As ButtonClickHandler)
        m_handlers.Add(key, handler)
    End Sub

    Private Sub ResetUndoMenu()
        UpdateUndoMenu(Nothing)
        UpdateRedoMenu(Nothing)
    End Sub

    Public Sub UpdateUndoMenu(ByVal list As IEnumerable(Of String))
        Dim enabled As Boolean = UpdateUndoMenu(m_mnuUndo, list)
        UndoButtonEnabled = enabled
    End Sub

    Public Sub UpdateRedoMenu(ByVal list As IEnumerable(Of String))
        Dim enabled As Boolean = UpdateUndoMenu(m_mnuRedo, list)
        RedoButtonEnabled = enabled
    End Sub

    Private Function UpdateUndoMenu(ByRef aMenuItem() As System.Windows.Forms.ToolStripMenuItem, ByVal list As IEnumerable(Of String)) As Boolean

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

    Private Property UndoButtonEnabled() As Boolean
        Get
            Return butUndo.Enabled
        End Get
        Set(ByVal value As Boolean)
            butUndo.Enabled = value
            RaiseEvent UndoEnabled(value)
        End Set
    End Property

    Private Property RedoButtonEnabled() As Boolean
        Get
            Return butRedo.Enabled
        End Get
        Set(ByVal value As Boolean)
            butRedo.Enabled = value
            RaiseEvent RedoEnabled(value)
        End Set
    End Property

    Private Sub UndoMenu_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs)
        UndoRedoMenu_MouseEnter(m_mnuUndo, sender)
    End Sub

    Private Sub UndoMenu_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs)
        UndoRedoMenu_MouseLeave()
    End Sub

    Private Sub RedoMenu_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs)
        UndoRedoMenu_MouseEnter(m_mnuRedo, sender)
    End Sub

    Private Sub RedoMenu_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs)
        UndoRedoMenu_MouseLeave()
    End Sub

    Private Sub UndoRedoMenu_MouseEnter(ByRef menu() As ToolStripMenuItem, ByVal sender As Object)
        tmrUndoTimer.Enabled = False
        HighlightMenu(menu, DirectCast(sender, ToolStripMenuItem).Tag)
    End Sub

    Private Sub UndoRedoMenu_MouseLeave()
        tmrUndoTimer.Enabled = True
    End Sub

    Private Sub HighlightMenu(ByRef aMenuItem() As System.Windows.Forms.ToolStripMenuItem, ByVal count As Integer)
        For i As Integer = 1 To count - 1
            aMenuItem(i).BackColor = System.Drawing.SystemColors.MenuHighlight
            aMenuItem(i).Checked = True
        Next
        For i As Integer = count To k_numberHistory
            aMenuItem(i).BackColor = m_defaultColour
            aMenuItem(i).Checked = (i = count)
        Next
    End Sub

    Private Sub tmrUndoTimer_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmrUndoTimer.Tick
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

    Private Sub butUndo_ButtonClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles butUndo.ButtonClick
        HandleClick("undo")
    End Sub

    Private Sub butRedo_ButtonClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles butRedo.ButtonClick
        HandleClick("redo")
    End Sub

    Private Sub butUndo_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles butUndo.Click
        ' If user has made edits to a field, save it first before displaying the undo list or doing the undo
        RaiseEvent SaveCurrentEditor()
    End Sub

    Private Sub butUndo_DropDownOpening(ByVal sender As Object, ByVal e As System.EventArgs) Handles butUndo.DropDownOpening
        ' If user has made edits to a field, save it first before displaying the undo list
        RaiseEvent SaveCurrentEditor()
        ResetHighlight()
    End Sub

    Private Sub butRedo_DropDownOpening(ByVal sender As Object, ByVal e As System.EventArgs) Handles butRedo.DropDownOpening
        ResetHighlight()
    End Sub

End Class