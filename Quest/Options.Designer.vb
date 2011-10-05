<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Options
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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
        Me.chkOverrideColours = New System.Windows.Forms.CheckBox()
        Me.lblForeground = New System.Windows.Forms.Label()
        Me.lblBackground = New System.Windows.Forms.Label()
        Me.dlgColor = New System.Windows.Forms.ColorDialog()
        Me.cmdForeground = New System.Windows.Forms.Button()
        Me.cmdBackground = New System.Windows.Forms.Button()
        Me.cmdCancel = New System.Windows.Forms.Button()
        Me.cmdOK = New System.Windows.Forms.Button()
        Me.lblLink = New System.Windows.Forms.Label()
        Me.cmdLink = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'chkOverrideColours
        '
        Me.chkOverrideColours.AutoSize = True
        Me.chkOverrideColours.Location = New System.Drawing.Point(13, 13)
        Me.chkOverrideColours.Name = "chkOverrideColours"
        Me.chkOverrideColours.Size = New System.Drawing.Size(146, 17)
        Me.chkOverrideColours.TabIndex = 0
        Me.chkOverrideColours.Text = "Use default game colours"
        Me.chkOverrideColours.UseVisualStyleBackColor = True
        '
        'lblForeground
        '
        Me.lblForeground.AutoSize = True
        Me.lblForeground.Location = New System.Drawing.Point(56, 37)
        Me.lblForeground.Name = "lblForeground"
        Me.lblForeground.Size = New System.Drawing.Size(31, 13)
        Me.lblForeground.TabIndex = 1
        Me.lblForeground.Text = "Text:"
        '
        'lblBackground
        '
        Me.lblBackground.AutoSize = True
        Me.lblBackground.Location = New System.Drawing.Point(56, 66)
        Me.lblBackground.Name = "lblBackground"
        Me.lblBackground.Size = New System.Drawing.Size(68, 13)
        Me.lblBackground.TabIndex = 2
        Me.lblBackground.Text = "Background:"
        '
        'cmdForeground
        '
        Me.cmdForeground.BackColor = System.Drawing.Color.White
        Me.cmdForeground.Location = New System.Drawing.Point(126, 32)
        Me.cmdForeground.Name = "cmdForeground"
        Me.cmdForeground.Size = New System.Drawing.Size(42, 23)
        Me.cmdForeground.TabIndex = 3
        Me.cmdForeground.UseVisualStyleBackColor = False
        '
        'cmdBackground
        '
        Me.cmdBackground.BackColor = System.Drawing.Color.Black
        Me.cmdBackground.Location = New System.Drawing.Point(126, 61)
        Me.cmdBackground.Name = "cmdBackground"
        Me.cmdBackground.Size = New System.Drawing.Size(42, 23)
        Me.cmdBackground.TabIndex = 4
        Me.cmdBackground.UseVisualStyleBackColor = False
        '
        'cmdCancel
        '
        Me.cmdCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.cmdCancel.Location = New System.Drawing.Point(193, 139)
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.Size = New System.Drawing.Size(75, 23)
        Me.cmdCancel.TabIndex = 5
        Me.cmdCancel.Text = "Cancel"
        Me.cmdCancel.UseVisualStyleBackColor = True
        '
        'cmdOK
        '
        Me.cmdOK.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdOK.Location = New System.Drawing.Point(112, 139)
        Me.cmdOK.Name = "cmdOK"
        Me.cmdOK.Size = New System.Drawing.Size(75, 23)
        Me.cmdOK.TabIndex = 6
        Me.cmdOK.Text = "OK"
        Me.cmdOK.UseVisualStyleBackColor = True
        '
        'lblLink
        '
        Me.lblLink.AutoSize = True
        Me.lblLink.Location = New System.Drawing.Point(56, 95)
        Me.lblLink.Name = "lblLink"
        Me.lblLink.Size = New System.Drawing.Size(30, 13)
        Me.lblLink.TabIndex = 7
        Me.lblLink.Text = "Link:"
        '
        'cmdLink
        '
        Me.cmdLink.BackColor = System.Drawing.Color.Blue
        Me.cmdLink.Location = New System.Drawing.Point(126, 90)
        Me.cmdLink.Name = "cmdLink"
        Me.cmdLink.Size = New System.Drawing.Size(42, 23)
        Me.cmdLink.TabIndex = 8
        Me.cmdLink.UseVisualStyleBackColor = False
        '
        'Options
        '
        Me.AcceptButton = Me.cmdOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.cmdCancel
        Me.ClientSize = New System.Drawing.Size(280, 174)
        Me.Controls.Add(Me.cmdLink)
        Me.Controls.Add(Me.lblLink)
        Me.Controls.Add(Me.cmdOK)
        Me.Controls.Add(Me.cmdCancel)
        Me.Controls.Add(Me.cmdBackground)
        Me.Controls.Add(Me.cmdForeground)
        Me.Controls.Add(Me.lblBackground)
        Me.Controls.Add(Me.lblForeground)
        Me.Controls.Add(Me.chkOverrideColours)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "Options"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Options"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents chkOverrideColours As System.Windows.Forms.CheckBox
    Friend WithEvents lblForeground As System.Windows.Forms.Label
    Friend WithEvents lblBackground As System.Windows.Forms.Label
    Friend WithEvents dlgColor As System.Windows.Forms.ColorDialog
    Friend WithEvents cmdForeground As System.Windows.Forms.Button
    Friend WithEvents cmdBackground As System.Windows.Forms.Button
    Friend WithEvents cmdCancel As System.Windows.Forms.Button
    Friend WithEvents cmdOK As System.Windows.Forms.Button
    Friend WithEvents lblLink As System.Windows.Forms.Label
    Friend WithEvents cmdLink As System.Windows.Forms.Button
End Class
