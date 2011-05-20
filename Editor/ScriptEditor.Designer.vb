<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ScriptEditor
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ScriptEditor))
        Me.ctlContainer = New System.Windows.Forms.SplitContainer()
        Me.lstScripts = New System.Windows.Forms.ListView()
        Me.chScript = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ctlToolStrip = New System.Windows.Forms.ToolStrip()
        Me.cmdDelete = New System.Windows.Forms.ToolStripButton()
        Me.cmdMoveUp = New System.Windows.Forms.ToolStripButton()
        Me.cmdMoveDown = New System.Windows.Forms.ToolStripButton()
        Me.cmdPopOut = New System.Windows.Forms.ToolStripButton()
        Me.ctlScriptAdder = New AxeSoftware.Quest.ScriptAdder()
        Me.ctlScriptCommandEditor = New AxeSoftware.Quest.ScriptCommandEditor()
        CType(Me.ctlContainer, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ctlContainer.Panel1.SuspendLayout()
        Me.ctlContainer.Panel2.SuspendLayout()
        Me.ctlContainer.SuspendLayout()
        Me.ctlToolStrip.SuspendLayout()
        Me.SuspendLayout()
        '
        'ctlContainer
        '
        Me.ctlContainer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1
        Me.ctlContainer.Location = New System.Drawing.Point(0, 0)
        Me.ctlContainer.Name = "ctlContainer"
        Me.ctlContainer.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'ctlContainer.Panel1
        '
        Me.ctlContainer.Panel1.Controls.Add(Me.lstScripts)
        Me.ctlContainer.Panel1.Controls.Add(Me.ctlToolStrip)
        '
        'ctlContainer.Panel2
        '
        Me.ctlContainer.Panel2.Controls.Add(Me.ctlScriptAdder)
        Me.ctlContainer.Panel2.Controls.Add(Me.ctlScriptCommandEditor)
        Me.ctlContainer.Size = New System.Drawing.Size(604, 285)
        Me.ctlContainer.SplitterDistance = 80
        Me.ctlContainer.TabIndex = 1
        '
        'lstScripts
        '
        Me.lstScripts.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.chScript})
        Me.lstScripts.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstScripts.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None
        Me.lstScripts.HideSelection = False
        Me.lstScripts.Location = New System.Drawing.Point(0, 25)
        Me.lstScripts.Name = "lstScripts"
        Me.lstScripts.Size = New System.Drawing.Size(604, 55)
        Me.lstScripts.TabIndex = 0
        Me.lstScripts.UseCompatibleStateImageBehavior = False
        Me.lstScripts.View = System.Windows.Forms.View.Details
        '
        'chScript
        '
        Me.chScript.Text = "Script"
        '
        'ctlToolStrip
        '
        Me.ctlToolStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.cmdDelete, Me.cmdMoveUp, Me.cmdMoveDown, Me.cmdPopOut})
        Me.ctlToolStrip.Location = New System.Drawing.Point(0, 0)
        Me.ctlToolStrip.Name = "ctlToolStrip"
        Me.ctlToolStrip.Size = New System.Drawing.Size(604, 25)
        Me.ctlToolStrip.TabIndex = 1
        Me.ctlToolStrip.Text = "ToolStrip1"
        '
        'cmdDelete
        '
        Me.cmdDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.cmdDelete.Image = CType(resources.GetObject("cmdDelete.Image"), System.Drawing.Image)
        Me.cmdDelete.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.cmdDelete.Name = "cmdDelete"
        Me.cmdDelete.Size = New System.Drawing.Size(23, 22)
        Me.cmdDelete.Text = "Delete"
        '
        'cmdMoveUp
        '
        Me.cmdMoveUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.cmdMoveUp.Image = CType(resources.GetObject("cmdMoveUp.Image"), System.Drawing.Image)
        Me.cmdMoveUp.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.cmdMoveUp.Name = "cmdMoveUp"
        Me.cmdMoveUp.Size = New System.Drawing.Size(23, 22)
        Me.cmdMoveUp.Text = "Move up"
        '
        'cmdMoveDown
        '
        Me.cmdMoveDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.cmdMoveDown.Image = CType(resources.GetObject("cmdMoveDown.Image"), System.Drawing.Image)
        Me.cmdMoveDown.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.cmdMoveDown.Name = "cmdMoveDown"
        Me.cmdMoveDown.Size = New System.Drawing.Size(23, 22)
        Me.cmdMoveDown.Text = "Move down"
        '
        'cmdPopOut
        '
        Me.cmdPopOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.cmdPopOut.Image = CType(resources.GetObject("cmdPopOut.Image"), System.Drawing.Image)
        Me.cmdPopOut.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.cmdPopOut.Name = "cmdPopOut"
        Me.cmdPopOut.Size = New System.Drawing.Size(23, 22)
        Me.cmdPopOut.Text = "Pop out"
        '
        'ctlScriptAdder
        '
        Me.ctlScriptAdder.Controller = Nothing
        Me.ctlScriptAdder.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlScriptAdder.IsReadOnly = False
        Me.ctlScriptAdder.Location = New System.Drawing.Point(0, 0)
        Me.ctlScriptAdder.Name = "ctlScriptAdder"
        Me.ctlScriptAdder.ShowCloseButton = False
        Me.ctlScriptAdder.Size = New System.Drawing.Size(604, 201)
        Me.ctlScriptAdder.TabIndex = 0
        '
        'ctlScriptCommandEditor
        '
        Me.ctlScriptCommandEditor.Controller = Nothing
        Me.ctlScriptCommandEditor.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlScriptCommandEditor.IsReadOnly = False
        Me.ctlScriptCommandEditor.Location = New System.Drawing.Point(0, 0)
        Me.ctlScriptCommandEditor.Name = "ctlScriptCommandEditor"
        Me.ctlScriptCommandEditor.ShowCloseButton = False
        Me.ctlScriptCommandEditor.Size = New System.Drawing.Size(604, 201)
        Me.ctlScriptCommandEditor.TabIndex = 1
        '
        'ScriptEditor
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.ctlContainer)
        Me.Name = "ScriptEditor"
        Me.Size = New System.Drawing.Size(604, 285)
        Me.ctlContainer.Panel1.ResumeLayout(False)
        Me.ctlContainer.Panel1.PerformLayout()
        Me.ctlContainer.Panel2.ResumeLayout(False)
        CType(Me.ctlContainer, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ctlContainer.ResumeLayout(False)
        Me.ctlToolStrip.ResumeLayout(False)
        Me.ctlToolStrip.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ctlContainer As System.Windows.Forms.SplitContainer
    Friend WithEvents ctlScriptAdder As AxeSoftware.Quest.ScriptAdder
    Friend WithEvents ctlScriptCommandEditor As AxeSoftware.Quest.ScriptCommandEditor
    Friend WithEvents lstScripts As System.Windows.Forms.ListView
    Friend WithEvents ctlToolStrip As System.Windows.Forms.ToolStrip
    Friend WithEvents cmdDelete As System.Windows.Forms.ToolStripButton
    Friend WithEvents cmdMoveUp As System.Windows.Forms.ToolStripButton
    Friend WithEvents cmdMoveDown As System.Windows.Forms.ToolStripButton
    Friend WithEvents cmdPopOut As System.Windows.Forms.ToolStripButton
    Friend WithEvents chScript As System.Windows.Forms.ColumnHeader

End Class
