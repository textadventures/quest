<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class OptionsDialog
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
        Me.chkUseDefaultColours = New System.Windows.Forms.CheckBox()
        Me.lblForeground = New System.Windows.Forms.Label()
        Me.lblBackground = New System.Windows.Forms.Label()
        Me.dlgColor = New System.Windows.Forms.ColorDialog()
        Me.cmdForeground = New System.Windows.Forms.Button()
        Me.cmdBackground = New System.Windows.Forms.Button()
        Me.cmdCancel = New System.Windows.Forms.Button()
        Me.cmdOK = New System.Windows.Forms.Button()
        Me.lblLink = New System.Windows.Forms.Label()
        Me.cmdLink = New System.Windows.Forms.Button()
        Me.chkUseDefaultFont = New System.Windows.Forms.CheckBox()
        Me.dlgFont = New System.Windows.Forms.FontDialog()
        Me.lblFontSample = New System.Windows.Forms.Label()
        Me.cmdFont = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'chkUseDefaultColours
        '
        Me.chkUseDefaultColours.AutoSize = True
        Me.chkUseDefaultColours.Location = New System.Drawing.Point(13, 13)
        Me.chkUseDefaultColours.Name = "chkUseDefaultColours"
        Me.chkUseDefaultColours.Size = New System.Drawing.Size(146, 17)
        Me.chkUseDefaultColours.TabIndex = 0
        Me.chkUseDefaultColours.Text = "Use default game &colours"
        Me.chkUseDefaultColours.UseVisualStyleBackColor = True
        '
        'lblForeground
        '
        Me.lblForeground.AutoSize = True
        Me.lblForeground.Location = New System.Drawing.Point(56, 37)
        Me.lblForeground.Name = "lblForeground"
        Me.lblForeground.Size = New System.Drawing.Size(31, 13)
        Me.lblForeground.TabIndex = 1
        Me.lblForeground.Text = "&Text:"
        '
        'lblBackground
        '
        Me.lblBackground.AutoSize = True
        Me.lblBackground.Location = New System.Drawing.Point(56, 66)
        Me.lblBackground.Name = "lblBackground"
        Me.lblBackground.Size = New System.Drawing.Size(68, 13)
        Me.lblBackground.TabIndex = 3
        Me.lblBackground.Text = "&Background:"
        '
        'cmdForeground
        '
        Me.cmdForeground.BackColor = System.Drawing.Color.White
        Me.cmdForeground.Location = New System.Drawing.Point(126, 32)
        Me.cmdForeground.Name = "cmdForeground"
        Me.cmdForeground.Size = New System.Drawing.Size(42, 23)
        Me.cmdForeground.TabIndex = 2
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
        Me.cmdCancel.Location = New System.Drawing.Point(193, 274)
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.Size = New System.Drawing.Size(75, 23)
        Me.cmdCancel.TabIndex = 9
        Me.cmdCancel.Text = "Cancel"
        Me.cmdCancel.UseVisualStyleBackColor = True
        '
        'cmdOK
        '
        Me.cmdOK.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdOK.Location = New System.Drawing.Point(112, 274)
        Me.cmdOK.Name = "cmdOK"
        Me.cmdOK.Size = New System.Drawing.Size(75, 23)
        Me.cmdOK.TabIndex = 8
        Me.cmdOK.Text = "OK"
        Me.cmdOK.UseVisualStyleBackColor = True
        '
        'lblLink
        '
        Me.lblLink.AutoSize = True
        Me.lblLink.Location = New System.Drawing.Point(56, 95)
        Me.lblLink.Name = "lblLink"
        Me.lblLink.Size = New System.Drawing.Size(30, 13)
        Me.lblLink.TabIndex = 5
        Me.lblLink.Text = "&Link:"
        '
        'cmdLink
        '
        Me.cmdLink.BackColor = System.Drawing.Color.Blue
        Me.cmdLink.Location = New System.Drawing.Point(126, 90)
        Me.cmdLink.Name = "cmdLink"
        Me.cmdLink.Size = New System.Drawing.Size(42, 23)
        Me.cmdLink.TabIndex = 6
        Me.cmdLink.UseVisualStyleBackColor = False
        '
        'chkUseDefaultFont
        '
        Me.chkUseDefaultFont.AutoSize = True
        Me.chkUseDefaultFont.Location = New System.Drawing.Point(13, 130)
        Me.chkUseDefaultFont.Name = "chkUseDefaultFont"
        Me.chkUseDefaultFont.Size = New System.Drawing.Size(130, 17)
        Me.chkUseDefaultFont.TabIndex = 7
        Me.chkUseDefaultFont.Text = "Use default game &font"
        Me.chkUseDefaultFont.UseVisualStyleBackColor = True
        '
        'dlgFont
        '
        Me.dlgFont.ScriptsOnly = True
        Me.dlgFont.ShowEffects = False
        '
        'lblFontSample
        '
        Me.lblFontSample.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.lblFontSample.Location = New System.Drawing.Point(12, 200)
        Me.lblFontSample.Name = "lblFontSample"
        Me.lblFontSample.Size = New System.Drawing.Size(256, 47)
        Me.lblFontSample.TabIndex = 10
        Me.lblFontSample.Text = "Sample Text"
        Me.lblFontSample.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'cmdFont
        '
        Me.cmdFont.Location = New System.Drawing.Point(59, 154)
        Me.cmdFont.Name = "cmdFont"
        Me.cmdFont.Size = New System.Drawing.Size(75, 23)
        Me.cmdFont.TabIndex = 11
        Me.cmdFont.Text = "&Modify..."
        Me.cmdFont.UseVisualStyleBackColor = True
        '
        'OptionsDialog
        '
        Me.AcceptButton = Me.cmdOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.cmdCancel
        Me.ClientSize = New System.Drawing.Size(280, 309)
        Me.Controls.Add(Me.cmdFont)
        Me.Controls.Add(Me.lblFontSample)
        Me.Controls.Add(Me.chkUseDefaultFont)
        Me.Controls.Add(Me.cmdOK)
        Me.Controls.Add(Me.cmdCancel)
        Me.Controls.Add(Me.lblForeground)
        Me.Controls.Add(Me.cmdForeground)
        Me.Controls.Add(Me.lblBackground)
        Me.Controls.Add(Me.cmdBackground)
        Me.Controls.Add(Me.lblLink)
        Me.Controls.Add(Me.cmdLink)
        Me.Controls.Add(Me.chkUseDefaultColours)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "OptionsDialog"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Options"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents chkUseDefaultColours As System.Windows.Forms.CheckBox
    Friend WithEvents lblForeground As System.Windows.Forms.Label
    Friend WithEvents lblBackground As System.Windows.Forms.Label
    Friend WithEvents dlgColor As System.Windows.Forms.ColorDialog
    Friend WithEvents cmdForeground As System.Windows.Forms.Button
    Friend WithEvents cmdBackground As System.Windows.Forms.Button
    Friend WithEvents cmdCancel As System.Windows.Forms.Button
    Friend WithEvents cmdOK As System.Windows.Forms.Button
    Friend WithEvents lblLink As System.Windows.Forms.Label
    Friend WithEvents cmdLink As System.Windows.Forms.Button
    Friend WithEvents chkUseDefaultFont As System.Windows.Forms.CheckBox
    Friend WithEvents dlgFont As System.Windows.Forms.FontDialog
    Friend WithEvents lblFontSample As System.Windows.Forms.Label
    Friend WithEvents cmdFont As System.Windows.Forms.Button
End Class
