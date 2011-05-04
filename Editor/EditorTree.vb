Imports System.Runtime.InteropServices

Public Class EditorTree

    Private m_nodes As New Dictionary(Of String, TreeNode)
    Private m_filterSettings As FilterOptions
    Private m_previousSelection As String
    Private m_openNodes As List(Of String)
    Private m_nodesWithChildren As List(Of String)
    Private m_selection As String
    Private m_updatingSelection As Boolean = False
    Private m_showSearchResults As Boolean = False
    Private m_canDragDelegate As Func(Of String, String, Boolean)
    Private m_doDragDelegate As Action(Of String, String)
    Public Delegate Sub MenuClickHandler()
    Private m_handlers As New Dictionary(Of String, MenuClickHandler)

    Public Event FiltersUpdated()
    Public Event SelectionChanged(key As String)
    Public Event CommitSelection()

    ' Code for setting "Search" hint on textbox
    Private Const ECM_FIRST As UInteger = &H1500
    Private Const EM_SETCUEBANNER As UInteger = ECM_FIRST + 1

    <DllImport("user32", CharSet:=CharSet.Auto, SetLastError:=False)> _
    Private Shared Function SendMessage(hWnd As HandleRef, Msg As UInteger, wParam As IntPtr, lParam As String) As IntPtr
    End Function

    Protected Overrides Sub OnHandleCreated(e As System.EventArgs)
        SetTextboxHint()
        MyBase.OnHandleCreated(e)
    End Sub

    Private Sub SetTextboxHint()
        Dim hintText As String = "Search"
        SendMessage(New HandleRef(Me, txtSearch.Handle), EM_SETCUEBANNER, IntPtr.Zero, hintText)
    End Sub

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ShowSearchResults = False
        ResizeColumn()
        AddHandlers()
    End Sub

    Public Sub AddNode(key As String, text As String, parentKey As String, foreColor As System.Drawing.Color?, backColor As System.Drawing.Color?)

        Dim newNode As TreeNode
        Dim parent As TreeNodeCollection

        If String.IsNullOrEmpty(parentKey) Then
            parent = ctlTreeView.Nodes
        Else
            parent = m_nodes(parentKey).Nodes
        End If

        newNode = parent.Add(key, text)
        If foreColor.HasValue Then
            newNode.ForeColor = foreColor.Value
        End If

        m_nodes.Add(key, newNode)

    End Sub

    Public Sub RemoveNode(key As String)
        ctlTreeView.Nodes.Remove(m_nodes(key))
        m_nodes.Remove(key)
    End Sub

    Public Sub RenameNode(oldKey As String, newKey As String)
        Dim node As TreeNode = m_nodes(oldKey)
        m_nodes.Remove(oldKey)
        m_nodes.Add(newKey, node)
        node.Name = newKey
        node.Text = newKey
    End Sub

    Public Sub SetAvailableFilters(filters As AvailableFilters)
        mnuFilter.DropDownItems.Clear()
        m_filterSettings = New FilterOptions

        For Each key As String In filters.AllFilters
            Dim newMenu As New System.Windows.Forms.ToolStripMenuItem
            newMenu.Text = filters.Get(key)
            newMenu.Tag = key
            AddHandler newMenu.Click, AddressOf FilterClicked
            mnuFilter.DropDownItems.Add(newMenu)
        Next
    End Sub

    Private Sub FilterClicked(sender As Object, e As System.EventArgs)
        Dim menuItem As ToolStripMenuItem = DirectCast(sender, ToolStripMenuItem)
        Dim key As String = DirectCast(menuItem.Tag, String)
        m_filterSettings.Set(key, Not m_filterSettings.IsSet(key))
        menuItem.Checked = m_filterSettings.IsSet(key)
        RaiseEvent FiltersUpdated()
    End Sub

    Public ReadOnly Property FilterSettings() As FilterOptions
        Get
            Return m_filterSettings
        End Get
    End Property

    Public Sub Clear()
        ctlTreeView.Nodes.Clear()
        m_nodes.Clear()
    End Sub

    Public Sub BeginUpdate()
        If (ctlTreeView.SelectedNode Is Nothing) Then
            m_previousSelection = ""
        Else
            m_previousSelection = ctlTreeView.SelectedNode.Name
        End If

        m_openNodes = New List(Of String)
        m_nodesWithChildren = New List(Of String)

        For Each node As TreeNode In m_nodes.Values
            If node.IsExpanded Then
                m_openNodes.Add(node.Name)
            End If

            If node.GetNodeCount(False) > 0 Then
                m_nodesWithChildren.Add(node.Name)
            End If
        Next
    End Sub

    Public Sub EndUpdate()
        If m_previousSelection <> "" Then
            If m_nodes.ContainsKey(m_previousSelection) Then
                ctlTreeView.SelectedNode = m_nodes(m_previousSelection)
            End If
        End If

        For Each node As TreeNode In m_nodes.Values
            If (m_openNodes.Contains(node.Name)) Then
                node.Expand()
            End If

            If Not m_nodesWithChildren.Contains(node.Name) AndAlso node.GetNodeCount(False) > 0 Then
                ' Expand any nodes that have new children
                node.Expand()
            End If
        Next
    End Sub

    Public Sub CollapseAdvancedNode()
        m_nodes("_advanced").Collapse()
    End Sub

    Private Sub ctlTreeView_AfterSelect(sender As System.Object, e As System.Windows.Forms.TreeViewEventArgs) Handles ctlTreeView.AfterSelect
        If m_updatingSelection Then Exit Sub
        SelectCurrentTreeViewItem()
    End Sub

    Private Sub ctlTreeView_MouseUp(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles ctlTreeView.MouseUp
        ' When right-clicking, select the item first before displaying the context menu
        If e.Button = Windows.Forms.MouseButtons.Right Then
            ctlTreeView.SelectedNode = ctlTreeView.GetNodeAt(e.X, e.Y)
            If ctlTreeView.SelectedNode IsNot Nothing AndAlso ctlTreeView.ContextMenuStrip IsNot Nothing Then
                ctlTreeView.ContextMenuStrip.Show(ctlTreeView, e.Location)
            End If
        End If
    End Sub

    Private Sub SelectCurrentTreeViewItem()
        Dim key As String

        If ctlTreeView.SelectedNode Is Nothing Then
            key = Nothing
        Else
            key = ctlTreeView.SelectedNode.Name
        End If

        ChangeSelection(key)
    End Sub

    Private Sub ChangeSelection(key As String)
        If key Is Nothing Then Return

        If key <> m_selection Then
            m_selection = key
            RaiseEvent SelectionChanged(key)
        End If
    End Sub

    Public Sub SetSelectedItemNoEvent(key As String)
        m_updatingSelection = True
        ctlTreeView.SelectedNode = m_nodes(key)
        m_updatingSelection = False
    End Sub

    Public Sub SetSelectedItem(key As String)
        ctlTreeView.SelectedNode = m_nodes(key)
    End Sub

    Public Property ShowFilterBar() As Boolean
        Get
            Return ctlToolStrip.Visible
        End Get
        Set(value As Boolean)
            ctlToolStrip.Visible = value
        End Set
    End Property

    Public Sub ExpandAll()
        ctlTreeView.ExpandAll()
    End Sub

    Public Sub CollapseAll()
        ctlTreeView.CollapseAll()
    End Sub

    Private Sub ctlTreeView_DoubleClick(sender As Object, e As System.EventArgs) Handles ctlTreeView.DoubleClick
        RaiseEvent CommitSelection()
    End Sub

    Private Sub cmdSearch_Click(sender As System.Object, e As System.EventArgs) Handles cmdSearch.Click
        SearchButtonClicked()
    End Sub

    Private Sub SearchButtonClicked()
        If txtSearch.Text.Length = 0 Then Return
        ShowSearchResults = True
        PopulateSearchResults(txtSearch.Text)
        lstSearchResults.Focus()
    End Sub

    Private Sub cmdClose_Click(sender As Object, e As System.EventArgs) Handles cmdClose.Click
        CloseButtonClicked()
    End Sub

    Private Sub CloseButtonClicked()
        ShowSearchResults = False
        txtSearch.Text = ""
    End Sub

    Private Property ShowSearchResults As Boolean
        Get
            Return m_showSearchResults
        End Get
        Set(value As Boolean)
            If value = m_showSearchResults Then Return
            m_showSearchResults = value

            SuspendLayout()
            lstSearchResults.Visible = value
            cmdClose.Visible = value
            ctlTreeView.Visible = Not value
            If value Then
                txtSearch.Width = cmdClose.Left
            Else
                txtSearch.Width = cmdSearch.Left
            End If
            ResumeLayout()

            If Not value Then SelectCurrentTreeViewItem()
        End Set
    End Property

    Private Sub PopulateSearchResults(search As String)
        ResizeColumn()
        lstSearchResults.Items.Clear()
        For Each item As TreeNode In ctlTreeView.Nodes
            AddSearchResultsForNode(item, search, 0)
        Next
    End Sub

    Private Sub AddSearchResultsForNode(node As TreeNode, search As String, level As Integer)
        If level > 0 OrElse IncludeRootLevelInSearchResults Then
            If node.Text.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) > -1 Then
                lstSearchResults.Items.Add(New ListViewItem With {.Name = node.Name, .Text = node.Text})
            End If
        End If

        For Each item As TreeNode In node.Nodes
            AddSearchResultsForNode(item, search, level + 1)
        Next
    End Sub

    Private Sub lstSearchResults_DoubleClick(sender As Object, e As System.EventArgs) Handles lstSearchResults.DoubleClick
        CommitSearchSelection()
    End Sub

    Private Sub lstSearchResults_ItemSelectionChanged(sender As Object, e As System.Windows.Forms.ListViewItemSelectionChangedEventArgs) Handles lstSearchResults.ItemSelectionChanged
        If ShowSearchResults Then
            If lstSearchResults.SelectedItems.Count = 0 Then Return
            ChangeSelection(lstSearchResults.SelectedItems(0).Name)
        End If
    End Sub

    Private Sub txtSearch_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles txtSearch.KeyDown
        If e.KeyCode = Keys.Enter Then
            SearchButtonClicked()
            If lstSearchResults.Items.Count > 0 Then
                lstSearchResults.SelectedItems.Clear()
                lstSearchResults.Items(0).Selected = True
            End If
            e.Handled = True
            e.SuppressKeyPress = True
        End If
        If e.KeyCode = Keys.Escape Then
            CloseButtonClicked()
            e.Handled = True
            e.SuppressKeyPress = True
        End If
    End Sub

    Private Sub lstSearchResults_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles lstSearchResults.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.Handled = True
            e.SuppressKeyPress = True
            CommitSearchSelection()
            CloseButtonClicked()
        End If
        If e.KeyCode = Keys.Escape Then
            txtSearch.Focus()
            e.Handled = True
            e.SuppressKeyPress = True
        End If
    End Sub

    Private Sub CommitSearchSelection()
        RaiseEvent CommitSelection()
    End Sub

    Private Sub EditorTree_Resize(sender As Object, e As System.EventArgs) Handles Me.Resize
        ResizeColumn()
    End Sub

    Private Sub ResizeColumn()
        lstSearchResults.Columns(0).Width = lstSearchResults.Width - SystemInformation.VerticalScrollBarWidth
    End Sub

    Private m_includeRootLevelInSearchResults As Boolean = True

    Public Property IncludeRootLevelInSearchResults As Boolean
        Get
            Return m_includeRootLevelInSearchResults
        End Get
        Set(value As Boolean)
            m_includeRootLevelInSearchResults = value
        End Set
    End Property

    Public Sub ScrollToTop()
        If ctlTreeView.Nodes.Count = 0 Then Return
        ctlTreeView.Nodes(0).EnsureVisible()
    End Sub

    Private Sub ctlTreeView_DragDrop(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles ctlTreeView.DragDrop
        HighlightNode(Nothing)
        Dim nodeOver As TreeNode = ctlTreeView.GetNodeAt(ctlTreeView.PointToClient(Cursor.Position))
        Dim element As String = DirectCast(e.Data.GetData(DataFormats.Text), String)
        Dim newParent As String = nodeOver.Name
        If nodeOver IsNot Nothing AndAlso CanDrag(element, newParent) Then
            m_doDragDelegate.Invoke(element, newParent)
        End If
    End Sub

    Private Sub ctlTreeView_DragOver(sender As Object, e As System.Windows.Forms.DragEventArgs) Handles ctlTreeView.DragOver
        Dim nodeOver As TreeNode = ctlTreeView.GetNodeAt(ctlTreeView.PointToClient(Cursor.Position))
        If nodeOver IsNot Nothing AndAlso CanDrag(DirectCast(e.Data.GetData(DataFormats.Text), String), nodeOver.Name) Then
            e.Effect = DragDropEffects.Move
            HighlightNode(nodeOver)
        Else
            e.Effect = DragDropEffects.None
            HighlightNode(Nothing)
        End If
    End Sub

    Private m_currentHighlight As TreeNode
    Private m_currentHighlightOldForeColor As Color
    Private m_currentHighlightOldBackColor As Color

    Private Sub HighlightNode(node As TreeNode)
        If node Is m_currentHighlight Then Return

        If m_currentHighlight IsNot Nothing Then
            m_currentHighlight.ForeColor = m_currentHighlightOldForeColor
            m_currentHighlight.BackColor = m_currentHighlightOldBackColor
        End If

        m_currentHighlight = node

        If m_currentHighlight Is Nothing Then Return

        m_currentHighlightOldForeColor = m_currentHighlight.ForeColor
        m_currentHighlightOldBackColor = m_currentHighlight.BackColor
        m_currentHighlight.ForeColor = SystemColors.HighlightText
        m_currentHighlight.BackColor = SystemColors.Highlight
    End Sub

    Private Sub ctlTreeView_ItemDrag(sender As Object, e As System.Windows.Forms.ItemDragEventArgs) Handles ctlTreeView.ItemDrag
        ctlTreeView.DoDragDrop(DirectCast(e.Item, TreeNode).Name, DragDropEffects.Move)
    End Sub

    Private Function CanDrag(dragItem As String, dragTo As String) As Boolean
        If m_canDragDelegate Is Nothing Then Return False
        Return m_canDragDelegate.Invoke(dragItem, dragTo)
    End Function

    Public Sub SetCanDragDelegate(del As Func(Of String, String, Boolean))
        m_canDragDelegate = del
    End Sub

    Public Sub SetDoDragDelegate(del As Action(Of String, String))
        m_doDragDelegate = del
    End Sub

    Public ReadOnly Property SelectedItem As String
        Get
            Return m_selection
        End Get
    End Property

    Public Sub RemoveContextMenu()
        ctlTreeView.ContextMenuStrip = Nothing
    End Sub

    Private Sub AddHandlers()
        For Each item In ctlContextMenu.Items
            AddHandlers(DirectCast(item, ToolStripMenuItem))
        Next
    End Sub

    Private Sub AddHandlers(menu As ToolStripMenuItem)
        AddHandler menu.Click, AddressOf Menu_Click
    End Sub

    Private Sub Menu_Click(sender As Object, e As EventArgs)
        Dim menu As ToolStripMenuItem = DirectCast(sender, ToolStripMenuItem)
        If menu.Tag Is Nothing Then Exit Sub

        If m_handlers.ContainsKey(DirectCast(menu.Tag, String)) Then
            m_handlers(DirectCast(menu.Tag, String)).Invoke()
        End If
    End Sub

    Public Sub AddMenuClickHandler(key As String, handler As MenuClickHandler)
        If m_handlers.ContainsKey(key) Then
            m_handlers.Remove(key)
        End If

        m_handlers.Add(key, handler)
    End Sub

End Class
