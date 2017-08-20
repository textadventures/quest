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
        Me.splitMain = New System.Windows.Forms.SplitContainer()
        Me.ctlTree = New TextAdventures.Quest.EditorControls.WFEditorTree()
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
        Me.splitMain.BackColor = System.Drawing.Color.GhostWhite
        Me.splitMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        Me.splitMain.Location = New System.Drawing.Point(0, 80)
        Me.splitMain.Margin = New System.Windows.Forms.Padding(4)
        Me.splitMain.Name = "splitMain"
        '
        'splitMain.Panel1
        '
        Me.splitMain.Panel1.Controls.Add(Me.ctlTree)
        '
        'splitMain.Panel2
        '
        Me.splitMain.Panel2.Controls.Add(Me.pnlContent)
        Me.splitMain.Panel2.Controls.Add(Me.pnlHeader)
        Me.splitMain.Panel2.Controls.Add(Me.StatusStrip1)
        Me.splitMain.Size = New System.Drawing.Size(824, 325)
        Me.splitMain.SplitterDistance = 300
        Me.splitMain.SplitterWidth = 5
        Me.splitMain.TabIndex = 0
        '
        'ctlTree
        '
        Me.ctlTree.BackColor = System.Drawing.Color.GhostWhite
        Me.ctlTree.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlTree.IncludeRootLevelInSearchResults = True
        Me.ctlTree.Location = New System.Drawing.Point(0, 0)
        Me.ctlTree.Margin = New System.Windows.Forms.Padding(5)
        Me.ctlTree.Name = "ctlTree"
        Me.ctlTree.ShowFilterBar = True
        Me.ctlTree.Size = New System.Drawing.Size(300, 325)
        Me.ctlTree.TabIndex = 0
        '
        'pnlContent
        '
        Me.pnlContent.BackColor = System.Drawing.Color.GhostWhite
        Me.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlContent.Location = New System.Drawing.Point(0, 30)
        Me.pnlContent.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlContent.Name = "pnlContent"
        Me.pnlContent.Size = New System.Drawing.Size(519, 270)
        Me.pnlContent.TabIndex = 1
        '
        'pnlHeader
        '
        Me.pnlHeader.AutoSize = True
        Me.pnlHeader.Controls.Add(Me.ctlBanner)
        Me.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlHeader.Location = New System.Drawing.Point(0, 0)
        Me.pnlHeader.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlHeader.Name = "pnlHeader"
        Me.pnlHeader.Size = New System.Drawing.Size(519, 30)
        Me.pnlHeader.TabIndex = 0
        '
        'ctlBanner
        '
        Me.ctlBanner.AlertText = "Text"
        Me.ctlBanner.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.ctlBanner.ButtonText = "Action"
        Me.ctlBanner.Dock = System.Windows.Forms.DockStyle.Top
        Me.ctlBanner.Location = New System.Drawing.Point(0, 0)
        Me.ctlBanner.Margin = New System.Windows.Forms.Padding(5)
        Me.ctlBanner.Name = "ctlBanner"
        Me.ctlBanner.Padding = New System.Windows.Forms.Padding(1, 1, 5, 1)
        Me.ctlBanner.Size = New System.Drawing.Size(519, 30)
        Me.ctlBanner.TabIndex = 1
        '
        'StatusStrip1
        '
        Me.StatusStrip1.AutoSize = False
        Me.StatusStrip1.BackColor = System.Drawing.Color.GhostWhite
        Me.StatusStrip1.ImageScalingSize = New System.Drawing.Size(20, 20)
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.lblHeader})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 300)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(519, 25)
        Me.StatusStrip1.TabIndex = 3
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'lblHeader
        '
        Me.lblHeader.Image = Global.TextAdventures.Quest.My.Resources.Resources.icons8_Ordner_öffnen_16
        Me.lblHeader.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None
        Me.lblHeader.Name = "lblHeader"
        Me.lblHeader.Size = New System.Drawing.Size(68, 20)
        Me.lblHeader.Text = "(none)"
        '
        'ctlSaveFile
        '
        Me.ctlSaveFile.DefaultExt = "aslx"
        Me.ctlSaveFile.Filter = "Quest Games|*.aslx|All files|*.*"
        '
        'ctlTextEditor
        '
        Me.ctlTextEditor.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlTextEditor.EditText = ""
        Me.ctlTextEditor.Location = New System.Drawing.Point(0, 80)
        Me.ctlTextEditor.Margin = New System.Windows.Forms.Padding(5)
        Me.ctlTextEditor.Name = "ctlTextEditor"
        Me.ctlTextEditor.Size = New System.Drawing.Size(824, 325)
        Me.ctlTextEditor.TabIndex = 3
        Me.ctlTextEditor.Visible = False
        Me.ctlTextEditor.WordWrap = False
        '
        'ctlReloadBanner
        '
        Me.ctlReloadBanner.AlertText = "This file has been modified outside Quest. Click Reload to get the latest version" &
    " of the file."
        Me.ctlReloadBanner.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.ctlReloadBanner.ButtonText = "Reload"
        Me.ctlReloadBanner.Dock = System.Windows.Forms.DockStyle.Top
        Me.ctlReloadBanner.Location = New System.Drawing.Point(0, 50)
        Me.ctlReloadBanner.Margin = New System.Windows.Forms.Padding(5)
        Me.ctlReloadBanner.Name = "ctlReloadBanner"
        Me.ctlReloadBanner.Padding = New System.Windows.Forms.Padding(1, 1, 5, 1)
        Me.ctlReloadBanner.Size = New System.Drawing.Size(824, 30)
        Me.ctlReloadBanner.TabIndex = 4
        Me.ctlReloadBanner.Visible = False
        '
        'ctlLoading
        '
        Me.ctlLoading.BackColor = System.Drawing.Color.White
        Me.ctlLoading.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlLoading.Location = New System.Drawing.Point(0, 0)
        Me.ctlLoading.Margin = New System.Windows.Forms.Padding(5)
        Me.ctlLoading.Name = "ctlLoading"
        Me.ctlLoading.Size = New System.Drawing.Size(824, 405)
        Me.ctlLoading.TabIndex = 9
        '
        'ctlToolbar
        '
        Me.ctlToolbar.BackColor = System.Drawing.Color.GhostWhite
        Me.ctlToolbar.CodeView = False
        Me.ctlToolbar.Dock = System.Windows.Forms.DockStyle.Top
        Me.ctlToolbar.EditorStyle = TextAdventures.Quest.EditorStyle.TextAdventure
        Me.ctlToolbar.Location = New System.Drawing.Point(0, 0)
        Me.ctlToolbar.Margin = New System.Windows.Forms.Padding(5)
        Me.ctlToolbar.Name = "ctlToolbar"
        Me.ctlToolbar.RedoButtonEnabled = False
        Me.ctlToolbar.SimpleMode = False
        Me.ctlToolbar.Size = New System.Drawing.Size(824, 50)
        Me.ctlToolbar.TabIndex = 2
        Me.ctlToolbar.UndoButtonEnabled = False
        '
        'Editor
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.GhostWhite
        Me.Controls.Add(Me.splitMain)
        Me.Controls.Add(Me.ctlTextEditor)
        Me.Controls.Add(Me.ctlReloadBanner)
        Me.Controls.Add(Me.ctlToolbar)
        Me.Controls.Add(Me.ctlLoading)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "Editor"
        Me.Size = New System.Drawing.Size(824, 405)
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
End Class
