Imports System.Runtime.InteropServices

Public Class EditorTree

    Private m_nodes As New Dictionary(Of String, TreeNode)
    Private m_filterSettings As FilterOptions
    Private m_previousSelection As String
    Private m_openNodes As List(Of String)
    Private m_selection As String
    Private m_updatingSelection As Boolean = False

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

    Public Sub AddNode(key As String, text As String, parentKey As String, foreColor As System.Drawing.Color?, backColor As System.Drawing.Color?)

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
            newNode.ForeColor = foreColor.Value
        End If

        m_nodes.Add(key, newNode)

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
            m_previousSelection = DirectCast(ctlTreeView.SelectedNode.Tag, String)
        End If

        m_openNodes = New List(Of String)

        For Each node As TreeNode In m_nodes.Values
            If node.IsExpanded Then
                m_openNodes.Add(DirectCast(node.Tag, String))
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
            If (m_openNodes.Contains(DirectCast(node.Tag, String))) Then
                node.Expand()
            End If
        Next
    End Sub

    Private Sub ctlTreeView_AfterSelect(sender As System.Object, e As System.Windows.Forms.TreeViewEventArgs) Handles ctlTreeView.AfterSelect
        Dim key As String

        If m_updatingSelection Then Exit Sub

        If ctlTreeView.SelectedNode Is Nothing Then
            key = Nothing
        Else
            key = DirectCast(ctlTreeView.SelectedNode.Tag, String)
        End If

        If key <> m_selection Then
            m_selection = key
            RaiseEvent SelectionChanged(key)
        End If
    End Sub

    Public Sub SetSelectedItem(key As String)
        m_updatingSelection = True
        ctlTreeView.SelectedNode = m_nodes(key)
        m_updatingSelection = False
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

End Class
