<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Editor
    Inherits System.Windows.Forms.UserControl

    'UserControl1 overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Editor))
        Me.splitMain = New System.Windows.Forms.SplitContainer()
        Me.ctlTree = New TextAdventures.Quest.EditorControls.WFEditorTree()
        Me.Splitter1 = New System.Windows.Forms.Splitter()
        Me.pnlContent = New System.Windows.Forms.Panel()
        Me.pnlHeader = New System.Windows.Forms.Panel()
        Me.ctlBanner = New TextAdventures.Quest.AlertBanner()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.lblHeader = New System.Windows.Forms.ToolStripStatusLabel()
        Me.ctlSaveFile = New System.Windows.Forms.SaveFileDialog()
        Me.ctlTextEditor = New TextAdventures.Quest.TextEditorControl()
        Me.ctlReloadBanner = New TextAdventures.Quest.AlertBanner()
        Me.ctlLoading = New TextAdventures.Quest.LoadingControl()
        Me.ctlToolbar = New TextAdventures.Quest.MainToolbar()
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitMain.Panel1.SuspendLayout()
        Me.splitMain.Panel2.SuspendLayout()
        Me.splitMain.SuspendLayout()
        Me.pnlHeader.SuspendLayout()
        Me.StatusStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'splitMain
        '
        resources.ApplyResources(Me.splitMain, "splitMain")
        Me.splitMain.BackColor = System.Drawing.Color.GhostWhite
        Me.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        Me.splitMain.Name = "splitMain"
        '
        'splitMain.Panel1
        '
        resources.ApplyResources(Me.splitMain.Panel1, "splitMain.Panel1")
        Me.splitMain.Panel1.Controls.Add(Me.ctlTree)
        Me.splitMain.Panel1.Controls.Add(Me.Splitter1)
        '
        'splitMain.Panel2
        '
        resources.ApplyResources(Me.splitMain.Panel2, "splitMain.Panel2")
        Me.splitMain.Panel2.Controls.Add(Me.pnlContent)
        Me.splitMain.Panel2.Controls.Add(Me.pnlHeader)
        Me.splitMain.Panel2.Controls.Add(Me.StatusStrip1)
        '
        'ctlTree
        '
        resources.ApplyResources(Me.ctlTree, "ctlTree")
        Me.ctlTree.BackColor = System.Drawing.Color.GhostWhite
        Me.ctlTree.ForeColor = System.Drawing.Color.Black
        Me.ctlTree.IncludeRootLevelInSearchResults = True
        Me.ctlTree.Name = "ctlTree"
        Me.ctlTree.ShowFilterBar = True
        '
        'Splitter1
        '
        resources.ApplyResources(Me.Splitter1, "Splitter1")
        Me.Splitter1.Cursor = System.Windows.Forms.Cursors.Default
        Me.Splitter1.Name = "Splitter1"
        Me.Splitter1.TabStop = False
        '
        'pnlContent
        '
        resources.ApplyResources(Me.pnlContent, "pnlContent")
        Me.pnlContent.BackColor = System.Drawing.Color.GhostWhite
        Me.pnlContent.Name = "pnlContent"
        '
        'pnlHeader
        '
        resources.ApplyResources(Me.pnlHeader, "pnlHeader")
        Me.pnlHeader.Controls.Add(Me.ctlBanner)
        Me.pnlHeader.Name = "pnlHeader"
        '
        'ctlBanner
        '
        resources.ApplyResources(Me.ctlBanner, "ctlBanner")
        Me.ctlBanner.AlertText = "Text"
        Me.ctlBanner.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.ctlBanner.ButtonText = "Action"
        Me.ctlBanner.Name = "ctlBanner"
        '
        'StatusStrip1
        '
        resources.ApplyResources(Me.StatusStrip1, "StatusStrip1")
        Me.StatusStrip1.BackColor = System.Drawing.Color.GhostWhite
        Me.StatusStrip1.ImageScalingSize = New System.Drawing.Size(20, 20)
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.lblHeader})
        Me.StatusStrip1.Name = "StatusStrip1"
        '
        'lblHeader
        '
        resources.ApplyResources(Me.lblHeader, "lblHeader")
        Me.lblHeader.Image = Global.TextAdventures.Quest.My.Resources.Resources.s_object_open
        Me.lblHeader.Name = "lblHeader"
        '
        'ctlSaveFile
        '
        Me.ctlSaveFile.DefaultExt = "aslx"
        resources.ApplyResources(Me.ctlSaveFile, "ctlSaveFile")
        '
        'ctlTextEditor
        '
        resources.ApplyResources(Me.ctlTextEditor, "ctlTextEditor")
        Me.ctlTextEditor.EditText = ""
        Me.ctlTextEditor.Name = "ctlTextEditor"
        Me.ctlTextEditor.WordWrap = False
        '
        'ctlReloadBanner
        '
        resources.ApplyResources(Me.ctlReloadBanner, "ctlReloadBanner")
        Me.ctlReloadBanner.AlertText = "This file has been modified outside Quest. Click Reload to get the latest version" &
    " of the file."
        Me.ctlReloadBanner.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.ctlReloadBanner.ButtonText = "Reload"
        Me.ctlReloadBanner.Name = "ctlReloadBanner"
        '
        'ctlLoading
        '
        resources.ApplyResources(Me.ctlLoading, "ctlLoading")
        Me.ctlLoading.BackColor = System.Drawing.Color.White
        Me.ctlLoading.Name = "ctlLoading"
        '
        'ctlToolbar
        '
        resources.ApplyResources(Me.ctlToolbar, "ctlToolbar")
        Me.ctlToolbar.BackColor = System.Drawing.Color.GhostWhite
        Me.ctlToolbar.CodeView = False
        Me.ctlToolbar.EditorStyle = TextAdventures.Quest.EditorStyle.TextAdventure
        Me.ctlToolbar.Name = "ctlToolbar"
        Me.ctlToolbar.RedoButtonEnabled = False
        Me.ctlToolbar.SimpleMode = False
        Me.ctlToolbar.UndoButtonEnabled = False
        '
        'Editor
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.GhostWhite
        Me.Controls.Add(Me.splitMain)
        Me.Controls.Add(Me.ctlTextEditor)
        Me.Controls.Add(Me.ctlReloadBanner)
        Me.Controls.Add(Me.ctlToolbar)
        Me.Controls.Add(Me.ctlLoading)
        Me.Name = "Editor"
        Me.splitMain.Panel1.ResumeLayout(False)
        Me.splitMain.Panel2.ResumeLayout(False)
        Me.splitMain.Panel2.PerformLayout()
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitMain.ResumeLayout(False)
        Me.pnlHeader.ResumeLayout(False)
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents splitMain As System.Windows.Forms.SplitContainer
    Friend WithEvents ctlTree As TextAdventures.Quest.EditorControls.WFEditorTree
    Private WithEvents ctlSaveFile As System.Windows.Forms.SaveFileDialog
    Friend WithEvents pnlHeader As System.Windows.Forms.Panel
    Friend WithEvents pnlContent As System.Windows.Forms.Panel
    Friend WithEvents ctlTextEditor As TextAdventures.Quest.TextEditorControl
    Friend WithEvents ctlBanner As TextAdventures.Quest.AlertBanner
    Friend WithEvents ctlReloadBanner As TextAdventures.Quest.AlertBanner
    Friend WithEvents ctlLoading As TextAdventures.Quest.LoadingControl
    Friend WithEvents ctlToolbar As MainToolbar
    Friend WithEvents StatusStrip1 As StatusStrip
    Friend WithEvents lblHeader As ToolStripStatusLabel
    Friend WithEvents Splitter1 As Splitter
End Class
