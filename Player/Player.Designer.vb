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
        Me.tmrTimer = New System.Windows.Forms.Timer(Me.components)
        Me.ctlSaveFile = New System.Windows.Forms.SaveFileDialog()
        Me.tmrInitialise = New System.Windows.Forms.Timer(Me.components)
        Me.ctlOpenFile = New System.Windows.Forms.OpenFileDialog()
        Me.tmrTick = New System.Windows.Forms.Timer(Me.components)
        Me.tmrPause = New System.Windows.Forms.Timer(Me.components)
        Me.ctlPlayerHtml = New AxeSoftware.Quest.PlayerHTML()
        Me.SuspendLayout()
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
        Me.ctlPlayerHtml.Size = New System.Drawing.Size(695, 482)
        Me.ctlPlayerHtml.TabIndex = 8
        '
        'Player
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.ctlPlayerHtml)
        Me.Name = "Player"
        Me.Size = New System.Drawing.Size(695, 482)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents tmrTimer As System.Windows.Forms.Timer
    Friend WithEvents ctlSaveFile As System.Windows.Forms.SaveFileDialog
    Friend WithEvents tmrInitialise As System.Windows.Forms.Timer
    Friend WithEvents ctlOpenFile As System.Windows.Forms.OpenFileDialog
    Friend WithEvents tmrTick As System.Windows.Forms.Timer
    Friend WithEvents tmrPause As System.Windows.Forms.Timer
    Friend WithEvents ctlPlayerHtml As AxeSoftware.Quest.PlayerHTML

End Class
