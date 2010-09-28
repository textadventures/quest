Public Class Menu

    Public Enum MenuMode
        GameBrowser
        Player
        Editor
    End Enum

    Private m_menus As New Dictionary(Of String, ToolStripMenuItem)
    Private m_mode As MenuMode
    Private m_menuData As New Dictionary(Of String, MenuData)
    Private m_separators As New List(Of ToolStripSeparator)
    Private m_needToHideDuplicateSeparators As Boolean = False
    Public Delegate Sub MenuClickHandler()
    Private m_handlers As New Dictionary(Of String, MenuClickHandler)

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.Height = ctlMenuStrip.Height

        InitialiseMenuData()
        AddHandlers()

        Mode = MenuMode.GameBrowser
        ChangeMode()
    End Sub

    Private Sub InitialiseMenuData()
        AddMenuData("restart", MenuMode.Player)
        AddMenuData("save", MenuMode.Player, MenuMode.Editor)
        AddMenuData("saveas", MenuMode.Player, MenuMode.Editor)
        AddMenuData("undo", MenuMode.Player, MenuMode.Editor)
        AddMenuData("redo", MenuMode.Editor)
        AddMenuData("selectall", MenuMode.Player)
        AddMenuData("copy", MenuMode.Player, MenuMode.Editor)
        AddMenuData("walkthrough", MenuMode.Player)
        AddMenuData("debugger", MenuMode.Player)
    End Sub

    Private Sub AddMenuData(ByVal key As String, ByVal ParamArray modes() As MenuMode)
        m_menuData.Add(key, New MenuData(modes))
    End Sub

    Private Sub AddHandlers()
        For Each item In ctlMenuStrip.Items
            AddHandlers(item)
        Next
    End Sub

    Private Sub AddHandlers(ByVal menu As ToolStripMenuItem)
        AddHandler menu.Click, AddressOf Menu_Click

        If Not String.IsNullOrEmpty(menu.Tag) Then
            m_menus.Add(menu.Tag, menu)
        End If

        For Each item As ToolStripItem In menu.DropDownItems
            If TypeOf item Is ToolStripMenuItem Then
                AddHandlers(item)
            ElseIf TypeOf item Is ToolStripSeparator Then
                m_separators.Add(item)
            End If
        Next
    End Sub

    Private Sub Menu_Click(ByVal sender As ToolStripMenuItem, ByVal e As EventArgs)
        If sender.Tag Is Nothing Then Exit Sub

        If m_handlers.ContainsKey(sender.Tag) Then
            m_handlers(sender.Tag).Invoke()
        End If
    End Sub

    Public Property MenuEnabled(ByVal key As String) As Boolean
        Get
            Return m_menus(key).Enabled
        End Get
        Set(ByVal value As Boolean)
            m_menus(key).Enabled = value
        End Set
    End Property

    Public Property MenuChecked(ByVal key As String) As Boolean
        Get
            Return m_menus(key).Checked
        End Get
        Set(ByVal value As Boolean)
            m_menus(key).Checked = value
        End Set
    End Property

    Public Property Mode() As MenuMode
        Get
            Return m_mode
        End Get
        Set(ByVal value As MenuMode)
            If m_mode <> value Then
                m_mode = value
                ChangeMode()
            End If
        End Set
    End Property

    Private Sub ChangeMode()

        For Each item In ctlMenuStrip.Items
            CType(item, ToolStripMenuItem).Available = True
        Next

        For Each data As KeyValuePair(Of String, MenuData) In m_menuData
            m_menus(data.Key).Available = data.Value.IsApplicable(m_mode)
        Next

        HideDuplicateSeparators()
        HideEmptyMenus()
    End Sub

    Private Sub HideDuplicateSeparators()
        ' All separators available by default
        For Each sep As ToolStripSeparator In m_separators
            sep.Available = True
        Next

        For Each item In ctlMenuStrip.Items
            HideDuplicateSeparators(item)
        Next
    End Sub

    Private Sub HideDuplicateSeparators(ByVal menu As ToolStripMenuItem)
        Dim lastWasSeparator As Boolean = True  ' so we don't get a separator at the top of the menu

        For Each item As ToolStripItem In menu.DropDownItems
            If item.Available Then
                If TypeOf item Is ToolStripMenuItem Then
                    lastWasSeparator = False
                    HideDuplicateSeparators(item)
                ElseIf TypeOf item Is ToolStripSeparator Then
                    If lastWasSeparator Then
                        item.Available = False
                    End If

                    lastWasSeparator = True
                End If
            End If
        Next
    End Sub

    Private Sub HideEmptyMenus()
        For Each item In ctlMenuStrip.Items
            HideEmptyMenus(item)
        Next
    End Sub

    Private Sub HideEmptyMenus(ByVal menu As ToolStripMenuItem)
        Dim hasVisibleItems = False

        For Each item As ToolStripItem In menu.DropDownItems
            If item.Available And TypeOf item Is ToolStripMenuItem Then
                hasVisibleItems = True
                Exit For
            End If
        Next

        If Not hasVisibleItems Then menu.Available = False
    End Sub

    Public Sub AddMenuClickHandler(ByVal key As String, ByVal handler As MenuClickHandler)
        If m_handlers.ContainsKey(key) Then
            m_handlers.Remove(key)
        End If

        m_handlers.Add(key, handler)
    End Sub

End Class
