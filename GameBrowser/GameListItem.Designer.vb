<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class GameListItem
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
        Me.lblName = New System.Windows.Forms.Label()
        Me.lblInfo = New System.Windows.Forms.Label()
        Me.cmdLaunch = New System.Windows.Forms.Button()
        Me.ctlToolTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.SuspendLayout()
        '
        'lblName
        '
        Me.lblName.AutoSize = True
        Me.lblName.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblName.Location = New System.Drawing.Point(3, 0)
        Me.lblName.Name = "lblName"
        Me.lblName.Size = New System.Drawing.Size(73, 13)
        Me.lblName.TabIndex = 0
        Me.lblName.Text = "Game name"
        '
        'lblInfo
        '
        Me.lblInfo.AutoSize = True
        Me.lblInfo.Location = New System.Drawing.Point(3, 13)
        Me.lblInfo.Name = "lblInfo"
        Me.lblInfo.Size = New System.Drawing.Size(25, 13)
        Me.lblInfo.TabIndex = 1
        Me.lblInfo.Text = "Info"
        '
        'cmdLaunch
        '
        Me.cmdLaunch.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdLaunch.Location = New System.Drawing.Point(163, 3)
        Me.cmdLaunch.Name = "cmdLaunch"
        Me.cmdLaunch.Size = New System.Drawing.Size(35, 24)
        Me.cmdLaunch.TabIndex = 2
        Me.cmdLaunch.Text = "Play"
        Me.cmdLaunch.UseVisualStyleBackColor = True
        '
        'ctlToolTip
        '
        '
        'GameListItem
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.Controls.Add(Me.cmdLaunch)
        Me.Controls.Add(Me.lblInfo)
        Me.Controls.Add(Me.lblName)
        Me.Name = "GameListItem"
        Me.Size = New System.Drawing.Size(204, 34)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents cmdLaunch As System.Windows.Forms.Button
    Private WithEvents lblName As System.Windows.Forms.Label
    Private WithEvents lblInfo As System.Windows.Forms.Label
    Friend WithEvents ctlToolTip As System.Windows.Forms.ToolTip

End Class
