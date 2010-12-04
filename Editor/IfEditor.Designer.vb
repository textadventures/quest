<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class IfEditor
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(IfEditor))
        Me.ctlToolStrip = New System.Windows.Forms.ToolStrip()
        Me.ToolStripLabel1 = New System.Windows.Forms.ToolStripLabel()
        Me.cmdAddElse = New System.Windows.Forms.ToolStripSplitButton()
        Me.mnuAddElse = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuAddElseIf = New System.Windows.Forms.ToolStripMenuItem()
        Me.pnlContainer = New System.Windows.Forms.Panel()
        Me.ctlChild = New AxeSoftware.Quest.IfEditorChild()
        Me.ctlToolStrip.SuspendLayout()
        Me.pnlContainer.SuspendLayout()
        Me.SuspendLayout()
        '
        'ctlToolStrip
        '
        Me.ctlToolStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripLabel1, Me.cmdAddElse})
        Me.ctlToolStrip.Location = New System.Drawing.Point(0, 0)
        Me.ctlToolStrip.Name = "ctlToolStrip"
        Me.ctlToolStrip.Size = New System.Drawing.Size(435, 25)
        Me.ctlToolStrip.TabIndex = 5
        Me.ctlToolStrip.Text = "ToolStrip1"
        '
        'ToolStripLabel1
        '
        Me.ToolStripLabel1.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.ToolStripLabel1.Name = "ToolStripLabel1"
        Me.ToolStripLabel1.Size = New System.Drawing.Size(25, 22)
        Me.ToolStripLabel1.Text = "If..."
        '
        'cmdAddElse
        '
        Me.cmdAddElse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.cmdAddElse.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuAddElse, Me.mnuAddElseIf})
        Me.cmdAddElse.Image = CType(resources.GetObject("cmdAddElse.Image"), System.Drawing.Image)
        Me.cmdAddElse.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.cmdAddElse.Name = "cmdAddElse"
        Me.cmdAddElse.Size = New System.Drawing.Size(68, 22)
        Me.cmdAddElse.Text = "Add Else"
        '
        'mnuAddElse
        '
        Me.mnuAddElse.Name = "mnuAddElse"
        Me.mnuAddElse.Size = New System.Drawing.Size(152, 22)
        Me.mnuAddElse.Text = "Add Else"
        '
        'mnuAddElseIf
        '
        Me.mnuAddElseIf.Name = "mnuAddElseIf"
        Me.mnuAddElseIf.Size = New System.Drawing.Size(152, 22)
        Me.mnuAddElseIf.Text = "Add Else If"
        '
        'pnlContainer
        '
        Me.pnlContainer.AutoScroll = True
        Me.pnlContainer.Controls.Add(Me.ctlChild)
        Me.pnlContainer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlContainer.Location = New System.Drawing.Point(0, 25)
        Me.pnlContainer.Name = "pnlContainer"
        Me.pnlContainer.Size = New System.Drawing.Size(435, 278)
        Me.pnlContainer.TabIndex = 7
        '
        'ctlChild
        '
        Me.ctlChild.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ctlChild.Controller = Nothing
        Me.ctlChild.ElseIfMode = AxeSoftware.Quest.IfEditorChild.IfEditorChildMode.IfMode
        Me.ctlChild.Expanded = True
        Me.ctlChild.Location = New System.Drawing.Point(0, 0)
        Me.ctlChild.Name = "ctlChild"
        Me.ctlChild.Padding = New System.Windows.Forms.Padding(0, 3, 0, 0)
        Me.ctlChild.Size = New System.Drawing.Size(435, 278)
        Me.ctlChild.TabIndex = 7
        '
        'IfEditor
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.pnlContainer)
        Me.Controls.Add(Me.ctlToolStrip)
        Me.Name = "IfEditor"
        Me.Size = New System.Drawing.Size(435, 303)
        Me.ctlToolStrip.ResumeLayout(False)
        Me.ctlToolStrip.PerformLayout()
        Me.pnlContainer.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ctlToolStrip As System.Windows.Forms.ToolStrip
    Friend WithEvents cmdAddElse As System.Windows.Forms.ToolStripSplitButton
    Friend WithEvents mnuAddElse As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuAddElseIf As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents pnlContainer As System.Windows.Forms.Panel
    Friend WithEvents ctlChild As AxeSoftware.Quest.IfEditorChild
    Friend WithEvents ToolStripLabel1 As System.Windows.Forms.ToolStripLabel

End Class
