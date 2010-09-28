Public Class EditorTree

    Private m_nodes As New Dictionary(Of String, TreeNode)
    Private m_filterSettings As FilterOptions
    Private m_previousSelection As String
    Private m_openNodes As List(Of String)
    Private m_selection As String
    Private m_updatingSelection As Boolean = False

    Public Event FiltersUpdated()
    Public Event SelectionChanged(ByVal key As String)
    Public Event CommitSelection()

    Public Sub AddNode(ByVal key As String, ByVal text As String, ByVal parentKey As String, ByVal foreColor As System.Drawing.Color?, ByVal backColor As System.Drawing.Color?)

        Dim newNode As TreeNode
        Dim parent As TreeNodeCollection

        If String.IsNullOrEmpty(parentKey) Then
            parent = ctlTreeView.Nodes
        Else
            parent = m_nodes(parentKey).Nodes
        End If

        newNode = parent.Add(key, text)
        newNode.Tag = key
        If foreColor.HasValue Then
            newNode.ForeColor = foreColor
        End If

        m_nodes.Add(key, newNode)

    End Sub

    Public Sub SetAvailableFilters(ByVal filters As AvailableFilters)
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

    Private Sub FilterClicked(ByVal sender As System.Windows.Forms.ToolStripMenuItem, ByVal e As System.EventArgs)
        Dim key As String = sender.Tag
        m_filterSettings.Set(key, Not m_filterSettings.IsSet(key))
        sender.Checked = m_filterSettings.IsSet(key)
        RaiseEvent FiltersUpdated()
    End Sub

    Public ReadOnly Property FilterSettings()
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
            m_previousSelection = ctlTreeView.SelectedNode.Tag
        End If

        m_openNodes = New List(Of String)

        For Each node As TreeNode In m_nodes.Values
            If node.IsExpanded Then
                m_openNodes.Add(node.Tag)
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
            If (m_openNodes.Contains(node.Tag)) Then
                node.Expand()
            End If
        Next
    End Sub

    Private Sub ctlTreeView_AfterSelect(ByVal sender As System.Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) Handles ctlTreeView.AfterSelect
        Dim key As String

        If m_updatingSelection Then Exit Sub

        If ctlTreeView.SelectedNode Is Nothing Then
            key = Nothing
        Else
            key = ctlTreeView.SelectedNode.Tag
        End If

        If key <> m_selection Then
            m_selection = key
            RaiseEvent SelectionChanged(key)
        End If
    End Sub

    Public Sub SetSelectedItem(ByVal key As String)
        m_updatingSelection = True
        ctlTreeView.SelectedNode = m_nodes(key)
        m_updatingSelection = False
    End Sub

    Public Property ShowFilterBar() As Boolean
        Get
            Return ctlToolStrip.Visible
        End Get
        Set(ByVal value As Boolean)
            ctlToolStrip.Visible = value
        End Set
    End Property

    Public Sub ExpandAll()
        ctlTreeView.ExpandAll()
    End Sub

    Public Sub CollapseAll()
        ctlTreeView.CollapseAll()
    End Sub

    Private Sub ctlTreeView_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles ctlTreeView.DoubleClick
        RaiseEvent CommitSelection()
    End Sub
End Class
