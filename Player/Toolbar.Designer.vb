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
        Me.ctlToolStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.butStop, Me.butDebugger, Me.butLog, Me.butHTML})
        Me.ctlToolStrip.Location = New System.Drawing.Point(0, 0)
        Me.ctlToolStrip.Name = "ctlToolStrip"
        Me.ctlToolStrip.Size = New System.Drawing.Size(601, 25)
        Me.ctlToolStrip.TabIndex = 0
        Me.ctlToolStrip.Text = "ToolStrip1"
        '
        'butStop
        '
        Me.butStop.Image = CType(resources.GetObject("butStop.Image"), System.Drawing.Image)
        Me.butStop.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.butStop.Name = "butStop"
        Me.butStop.Size = New System.Drawing.Size(85, 22)
        Me.butStop.Tag = "stop"
        Me.butStop.Text = "Stop Game"
        '
        'butDebugger
        '
        Me.butDebugger.Image = CType(resources.GetObject("butDebugger.Image"), System.Drawing.Image)
        Me.butDebugger.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.butDebugger.Name = "butDebugger"
        Me.butDebugger.Size = New System.Drawing.Size(79, 22)
        Me.butDebugger.Tag = "debugger"
        Me.butDebugger.Text = "Debugger"
        '
        'butLog
        '
        Me.butLog.Image = CType(resources.GetObject("butLog.Image"), System.Drawing.Image)
        Me.butLog.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.butLog.Name = "butLog"
        Me.butLog.Size = New System.Drawing.Size(47, 22)
        Me.butLog.Tag = "log"
        Me.butLog.Text = "Log"
        '
        'butHTML
        '
        Me.butHTML.Image = CType(resources.GetObject("butHTML.Image"), System.Drawing.Image)
        Me.butHTML.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.butHTML.Name = "butHTML"
        Me.butHTML.Size = New System.Drawing.Size(92, 22)
        Me.butHTML.Tag = "htmldevtools"
        Me.butHTML.Text = "HTML Tools"
        '
        'Toolbar
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.ctlToolStrip)
        Me.Name = "Toolbar"
        Me.Size = New System.Drawing.Size(601, 33)
        Me.ctlToolStrip.ResumeLayout(False)
        Me.ctlToolStrip.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ctlToolStrip As System.Windows.Forms.ToolStrip
    Friend WithEvents butStop As System.Windows.Forms.ToolStripButton
    Friend WithEvents butDebugger As System.Windows.Forms.ToolStripButton
    Friend WithEvents butLog As System.Windows.Forms.ToolStripButton
    Friend WithEvents butHTML As System.Windows.Forms.ToolStripButton

End Class
