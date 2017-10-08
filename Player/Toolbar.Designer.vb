<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Toolbar
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Toolbar))
        Me.ctlToolStrip = New System.Windows.Forms.ToolStrip()
        Me.butStop = New System.Windows.Forms.ToolStripButton()
        Me.butDebugger = New System.Windows.Forms.ToolStripButton()
        Me.butLog = New System.Windows.Forms.ToolStripButton()
        Me.butHTML = New System.Windows.Forms.ToolStripButton()
        Me.ctlToolStrip.SuspendLayout()
        Me.SuspendLayout()
        '
        'ctlToolStrip
        '
        resources.ApplyResources(Me.ctlToolStrip, "ctlToolStrip")
        Me.ctlToolStrip.BackColor = System.Drawing.Color.Transparent
        Me.ctlToolStrip.ImageScalingSize = New System.Drawing.Size(20, 20)
        Me.ctlToolStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.butStop, Me.butDebugger, Me.butLog, Me.butHTML})
        Me.ctlToolStrip.Name = "ctlToolStrip"
        '
        'butStop
        '
        Me.butStop.Image = Global.TextAdventures.Quest.My.Resources.Resources.b_stop
        resources.ApplyResources(Me.butStop, "butStop")
        Me.butStop.Name = "butStop"
        Me.butStop.Tag = "stop"
        '
        'butDebugger
        '
        Me.butDebugger.Image = Global.TextAdventures.Quest.My.Resources.Resources.b_debug
        resources.ApplyResources(Me.butDebugger, "butDebugger")
        Me.butDebugger.Name = "butDebugger"
        Me.butDebugger.Tag = "debugger"
        '
        'butLog
        '
        Me.butLog.Image = Global.TextAdventures.Quest.My.Resources.Resources.b_log
        resources.ApplyResources(Me.butLog, "butLog")
        Me.butLog.Name = "butLog"
        Me.butLog.Tag = "log"
        '
        'butHTML
        '
        Me.butHTML.Image = Global.TextAdventures.Quest.My.Resources.Resources.b_html
        resources.ApplyResources(Me.butHTML, "butHTML")
        Me.butHTML.Name = "butHTML"
        Me.butHTML.Tag = "htmldevtools"
        '
        'Toolbar
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.GhostWhite
        Me.Controls.Add(Me.ctlToolStrip)
        Me.Name = "Toolbar"
        Me.ctlToolStrip.ResumeLayout(False)
        Me.ctlToolStrip.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ctlToolStrip As System.Windows.Forms.ToolStrip
    Friend WithEvents butStop As System.Windows.Forms.ToolStripButton
    Friend WithEvents butDebugger As System.Windows.Forms.ToolStripButton
    Friend WithEvents butLog As System.Windows.Forms.ToolStripButton
    Friend WithEvents butHTML As System.Windows.Forms.ToolStripButton

End Class
