Imports TextAdventures.Quest

Public Class Debugger

    Private m_loaded As Boolean = False
    Private m_game As IASLDebug
    Private m_debuggerPanes As New List(Of DebuggerPane)
    Private m_splitHelper As TextAdventures.Utility.SplitterHelper
    Private m_primaryPane As DebuggerPane

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Dim helper As New TextAdventures.Utility.WindowHelper(Me, "Quest", "Debugger")
    End Sub

    Private Sub InitialiseTabs()
        Dim tabPage As System.Windows.Forms.TabPage
        Dim pane As DebuggerPane

        For Each tab As String In m_game.DebuggerObjectTypes
            tabPage = New System.Windows.Forms.TabPage(tab)
            tabsMain.TabPages.Add(tabPage)
            pane = New DebuggerPane
            pane.Filter = tab
            pane.Game = m_game
            tabPage.Controls.Add(pane)
            pane.Dock = DockStyle.Fill
            m_debuggerPanes.Add(pane)

            If m_splitHelper Is Nothing Then
                m_primaryPane = pane
                m_splitHelper = New TextAdventures.Utility.SplitterHelper(pane.splitMain, "Quest", "DebuggerSplitter")
                AddHandler pane.splitMain.SplitterMoved, AddressOf SplitterMoved
            End If
        Next
    End Sub

    Private Sub Debugger_FormClosing(sender As Object, e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason = CloseReason.UserClosing Then
            e.Cancel = True
            Me.Hide()
        End If
    End Sub

    Public Property Game() As IASLDebug
        Get
            Return m_game
        End Get
        Set(value As IASLDebug)
            m_game = value
            InitialiseTabs()
            UpdateObjectList()
        End Set
    End Property

    Private Sub UpdateObjectList()
        For Each pane As DebuggerPane In m_debuggerPanes
            pane.UpdateObjectList()
        Next
    End Sub

    Friend Sub LoadSplitterPositions()
        m_splitHelper.LoadSplitterPositions()
        UpdateAllSplitters()
    End Sub

    Private Sub SplitterMoved(sender As Object, e As System.Windows.Forms.SplitterEventArgs)
        UpdateAllSplitters()
    End Sub

    Private Sub UpdateAllSplitters()
        For Each pane As DebuggerPane In m_debuggerPanes
            pane.splitMain.SplitterDistance = m_primaryPane.splitMain.SplitterDistance
        Next
    End Sub
End Class