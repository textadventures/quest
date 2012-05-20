<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Player
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
        Me.components = New System.ComponentModel.Container()
        Me.splitMain = New System.Windows.Forms.SplitContainer()
        Me.pnlCommand = New System.Windows.Forms.Panel()
        Me.cmdGo = New System.Windows.Forms.Button()
        Me.txtCommand = New System.Windows.Forms.TextBox()
        Me.tmrTimer = New System.Windows.Forms.Timer(Me.components)
        Me.ctlSaveFile = New System.Windows.Forms.SaveFileDialog()
        Me.tmrInitialise = New System.Windows.Forms.Timer(Me.components)
        Me.ctlOpenFile = New System.Windows.Forms.OpenFileDialog()
        Me.tmrTick = New System.Windows.Forms.Timer(Me.components)
        Me.tmrPause = New System.Windows.Forms.Timer(Me.components)
        Me.ctlPlayerHtml = New AxeSoftware.Quest.PlayerHTML()
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.splitMain.Panel1.SuspendLayout()
        Me.splitMain.SuspendLayout()
        Me.pnlCommand.SuspendLayout()
        Me.SuspendLayout()
        '
        'splitMain
        '
        Me.splitMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2
        Me.splitMain.Location = New System.Drawing.Point(0, 0)
        Me.splitMain.Name = "splitMain"
        '
        'splitMain.Panel1
        '
        Me.splitMain.Panel1.Controls.Add(Me.ctlPlayerHtml)
        Me.splitMain.Panel1.Controls.Add(Me.pnlCommand)
        Me.splitMain.Panel2Collapsed = True
        Me.splitMain.Panel2MinSize = 175
        Me.splitMain.Size = New System.Drawing.Size(695, 482)
        Me.splitMain.SplitterDistance = 510
        Me.splitMain.TabIndex = 1
        '
        'pnlCommand
        '
        Me.pnlCommand.Controls.Add(Me.cmdGo)
        Me.pnlCommand.Controls.Add(Me.txtCommand)
        Me.pnlCommand.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlCommand.Location = New System.Drawing.Point(0, 462)
        Me.pnlCommand.Name = "pnlCommand"
        Me.pnlCommand.Size = New System.Drawing.Size(695, 20)
        Me.pnlCommand.TabIndex = 9
        '
        'cmdGo
        '
        Me.cmdGo.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdGo.Location = New System.Drawing.Point(671, 0)
        Me.cmdGo.Margin = New System.Windows.Forms.Padding(0)
        Me.cmdGo.Name = "cmdGo"
        Me.cmdGo.Size = New System.Drawing.Size(24, 21)
        Me.cmdGo.TabIndex = 5
        Me.cmdGo.UseVisualStyleBackColor = True
        '
        'txtCommand
        '
        Me.txtCommand.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtCommand.Location = New System.Drawing.Point(0, 0)
        Me.txtCommand.Margin = New System.Windows.Forms.Padding(1)
        Me.txtCommand.Name = "txtCommand"
        Me.txtCommand.Size = New System.Drawing.Size(670, 20)
        Me.txtCommand.TabIndex = 4
        '
        'tmrTimer
        '
        Me.tmrTimer.Interval = 50
        '
        'tmrInitialise
        '
        Me.tmrInitialise.Interval = 50
        '
        'tmrTick
        '
        Me.tmrTick.Interval = 1000
        '
        'tmrPause
        '
        '
        'ctlPlayerHtml
        '
        Me.ctlPlayerHtml.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlPlayerHtml.Location = New System.Drawing.Point(0, 0)
        Me.ctlPlayerHtml.Margin = New System.Windows.Forms.Padding(0)
        Me.ctlPlayerHtml.Name = "ctlPlayerHtml"
        Me.ctlPlayerHtml.ScriptErrorsSuppressed = False
        Me.ctlPlayerHtml.Size = New System.Drawing.Size(695, 462)
        Me.ctlPlayerHtml.TabIndex = 7
        '
        'Player
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.splitMain)
        Me.Name = "Player"
        Me.Size = New System.Drawing.Size(695, 482)
        Me.splitMain.Panel1.ResumeLayout(False)
        CType(Me.splitMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.splitMain.ResumeLayout(False)
        Me.pnlCommand.ResumeLayout(False)
        Me.pnlCommand.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents splitMain As System.Windows.Forms.SplitContainer
    Friend WithEvents tmrTimer As System.Windows.Forms.Timer
    Friend WithEvents ctlSaveFile As System.Windows.Forms.SaveFileDialog
    Friend WithEvents tmrInitialise As System.Windows.Forms.Timer
    Friend WithEvents ctlOpenFile As System.Windows.Forms.OpenFileDialog
    Friend WithEvents tmrTick As System.Windows.Forms.Timer
    Friend WithEvents ctlPlayerHtml As AxeSoftware.Quest.PlayerHTML
    Friend WithEvents pnlCommand As System.Windows.Forms.Panel
    Friend WithEvents cmdGo As System.Windows.Forms.Button
    Friend WithEvents txtCommand As System.Windows.Forms.TextBox
    Friend WithEvents tmrPause As System.Windows.Forms.Timer

End Class
