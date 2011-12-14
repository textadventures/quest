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
        Me.dlgColor = New System.Windows.Forms.ColorDialog()
        Me.cmdCancel = New System.Windows.Forms.Button()
        Me.cmdOK = New System.Windows.Forms.Button()
        Me.dlgFont = New System.Windows.Forms.FontDialog()
        Me.ctlTabs = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.chkPlaySounds = New System.Windows.Forms.CheckBox()
        Me.cmdFont = New System.Windows.Forms.Button()
        Me.lblFontSample = New System.Windows.Forms.Label()
        Me.chkUseDefaultFont = New System.Windows.Forms.CheckBox()
        Me.lblForeground = New System.Windows.Forms.Label()
        Me.cmdForeground = New System.Windows.Forms.Button()
        Me.lblBackground = New System.Windows.Forms.Label()
        Me.cmdBackground = New System.Windows.Forms.Button()
        Me.lblLink = New System.Windows.Forms.Label()
        Me.cmdLink = New System.Windows.Forms.Button()
        Me.chkUseDefaultColours = New System.Windows.Forms.CheckBox()
        Me.TabPage2 = New System.Windows.Forms.TabPage()
        Me.chkShowSandpit = New System.Windows.Forms.CheckBox()
        Me.cmdGamesFolder = New System.Windows.Forms.Button()
        Me.txtGamesFolder = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.dlgFolderBrowser = New System.Windows.Forms.FolderBrowserDialog()
        Me.chkShowAdult = New System.Windows.Forms.CheckBox()
        Me.ctlTabs.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.TabPage2.SuspendLayout()
        Me.SuspendLayout()
        '
        'cmdCancel
        '
        Me.cmdCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.cmdCancel.Location = New System.Drawing.Point(418, 357)
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.Size = New System.Drawing.Size(75, 23)
        Me.cmdCancel.TabIndex = 9
        Me.cmdCancel.Text = "Cancel"
        Me.cmdCancel.UseVisualStyleBackColor = True
        '
        'cmdOK
        '
        Me.cmdOK.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdOK.Location = New System.Drawing.Point(337, 357)
        Me.cmdOK.Name = "cmdOK"
        Me.cmdOK.Size = New System.Drawing.Size(75, 23)
        Me.cmdOK.TabIndex = 8
        Me.cmdOK.Text = "OK"
        Me.cmdOK.UseVisualStyleBackColor = True
        '
        'dlgFont
        '
        Me.dlgFont.ScriptsOnly = True
        Me.dlgFont.ShowEffects = False
        '
        'ctlTabs
        '
        Me.ctlTabs.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ctlTabs.Controls.Add(Me.TabPage1)
        Me.ctlTabs.Controls.Add(Me.TabPage2)
        Me.ctlTabs.Location = New System.Drawing.Point(12, 12)
        Me.ctlTabs.Name = "ctlTabs"
        Me.ctlTabs.SelectedIndex = 0
        Me.ctlTabs.Size = New System.Drawing.Size(481, 316)
        Me.ctlTabs.TabIndex = 12
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.chkPlaySounds)
        Me.TabPage1.Controls.Add(Me.cmdFont)
        Me.TabPage1.Controls.Add(Me.lblFontSample)
        Me.TabPage1.Controls.Add(Me.chkUseDefaultFont)
        Me.TabPage1.Controls.Add(Me.lblForeground)
        Me.TabPage1.Controls.Add(Me.cmdForeground)
        Me.TabPage1.Controls.Add(Me.lblBackground)
        Me.TabPage1.Controls.Add(Me.cmdBackground)
        Me.TabPage1.Controls.Add(Me.lblLink)
        Me.TabPage1.Controls.Add(Me.cmdLink)
        Me.TabPage1.Controls.Add(Me.chkUseDefaultColours)
        Me.TabPage1.Location = New System.Drawing.Point(4, 22)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(473, 290)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "Player"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'chkPlaySounds
        '
        Me.chkPlaySounds.AutoSize = True
        Me.chkPlaySounds.Location = New System.Drawing.Point(6, 257)
        Me.chkPlaySounds.Name = "chkPlaySounds"
        Me.chkPlaySounds.Size = New System.Drawing.Size(83, 17)
        Me.chkPlaySounds.TabIndex = 22
        Me.chkPlaySounds.Text = "Play &sounds"
        Me.chkPlaySounds.UseVisualStyleBackColor = True
        '
        'cmdFont
        '
        Me.cmdFont.Location = New System.Drawing.Point(124, 147)
        Me.cmdFont.Name = "cmdFont"
        Me.cmdFont.Size = New System.Drawing.Size(75, 23)
        Me.cmdFont.TabIndex = 21
        Me.cmdFont.Text = "&Modify..."
        Me.cmdFont.UseVisualStyleBackColor = True
        '
        'lblFontSample
        '
        Me.lblFontSample.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.lblFontSample.Location = New System.Drawing.Point(124, 191)
        Me.lblFontSample.Name = "lblFontSample"
        Me.lblFontSample.Size = New System.Drawing.Size(256, 47)
        Me.lblFontSample.TabIndex = 20
        Me.lblFontSample.Text = "Sample Text"
        Me.lblFontSample.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'chkUseDefaultFont
        '
        Me.chkUseDefaultFont.AutoSize = True
        Me.chkUseDefaultFont.Location = New System.Drawing.Point(6, 123)
        Me.chkUseDefaultFont.Name = "chkUseDefaultFont"
        Me.chkUseDefaultFont.Size = New System.Drawing.Size(130, 17)
        Me.chkUseDefaultFont.TabIndex = 19
        Me.chkUseDefaultFont.Text = "Use default game &font"
        Me.chkUseDefaultFont.UseVisualStyleBackColor = True
        '
        'lblForeground
        '
        Me.lblForeground.AutoSize = True
        Me.lblForeground.Location = New System.Drawing.Point(121, 30)
        Me.lblForeground.Name = "lblForeground"
        Me.lblForeground.Size = New System.Drawing.Size(31, 13)
        Me.lblForeground.TabIndex = 13
        Me.lblForeground.Text = "&Text:"
        '
        'cmdForeground
        '
        Me.cmdForeground.BackColor = System.Drawing.Color.White
        Me.cmdForeground.Location = New System.Drawing.Point(191, 25)
        Me.cmdForeground.Name = "cmdForeground"
        Me.cmdForeground.Size = New System.Drawing.Size(42, 23)
        Me.cmdForeground.TabIndex = 14
        Me.cmdForeground.UseVisualStyleBackColor = False
        '
        'lblBackground
        '
        Me.lblBackground.AutoSize = True
        Me.lblBackground.Location = New System.Drawing.Point(121, 59)
        Me.lblBackground.Name = "lblBackground"
        Me.lblBackground.Size = New System.Drawing.Size(68, 13)
        Me.lblBackground.TabIndex = 15
        Me.lblBackground.Text = "&Background:"
        '
        'cmdBackground
        '
        Me.cmdBackground.BackColor = System.Drawing.Color.Black
        Me.cmdBackground.Location = New System.Drawing.Point(191, 54)
        Me.cmdBackground.Name = "cmdBackground"
        Me.cmdBackground.Size = New System.Drawing.Size(42, 23)
        Me.cmdBackground.TabIndex = 16
        Me.cmdBackground.UseVisualStyleBackColor = False
        '
        'lblLink
        '
        Me.lblLink.AutoSize = True
        Me.lblLink.Location = New System.Drawing.Point(121, 88)
        Me.lblLink.Name = "lblLink"
        Me.lblLink.Size = New System.Drawing.Size(30, 13)
        Me.lblLink.TabIndex = 17
        Me.lblLink.Text = "&Link:"
        '
        'cmdLink
        '
        Me.cmdLink.BackColor = System.Drawing.Color.Blue
        Me.cmdLink.Location = New System.Drawing.Point(191, 83)
        Me.cmdLink.Name = "cmdLink"
        Me.cmdLink.Size = New System.Drawing.Size(42, 23)
        Me.cmdLink.TabIndex = 18
        Me.cmdLink.UseVisualStyleBackColor = False
        '
        'chkUseDefaultColours
        '
        Me.chkUseDefaultColours.AutoSize = True
        Me.chkUseDefaultColours.Location = New System.Drawing.Point(6, 6)
        Me.chkUseDefaultColours.Name = "chkUseDefaultColours"
        Me.chkUseDefaultColours.Size = New System.Drawing.Size(146, 17)
        Me.chkUseDefaultColours.TabIndex = 12
        Me.chkUseDefaultColours.Text = "Use default game &colours"
        Me.chkUseDefaultColours.UseVisualStyleBackColor = True
        '
        'TabPage2
        '
        Me.TabPage2.Controls.Add(Me.chkShowAdult)
        Me.TabPage2.Controls.Add(Me.chkShowSandpit)
        Me.TabPage2.Controls.Add(Me.cmdGamesFolder)
        Me.TabPage2.Controls.Add(Me.txtGamesFolder)
        Me.TabPage2.Controls.Add(Me.Label1)
        Me.TabPage2.Location = New System.Drawing.Point(4, 22)
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage2.Size = New System.Drawing.Size(473, 290)
        Me.TabPage2.TabIndex = 1
        Me.TabPage2.Text = "Game Browser"
        Me.TabPage2.UseVisualStyleBackColor = True
        '
        'chkShowSandpit
        '
        Me.chkShowSandpit.AutoSize = True
        Me.chkShowSandpit.Location = New System.Drawing.Point(10, 59)
        Me.chkShowSandpit.Name = "chkShowSandpit"
        Me.chkShowSandpit.Size = New System.Drawing.Size(134, 17)
        Me.chkShowSandpit.TabIndex = 3
        Me.chkShowSandpit.Text = "Show sandpit category"
        Me.chkShowSandpit.UseVisualStyleBackColor = True
        '
        'cmdGamesFolder
        '
        Me.cmdGamesFolder.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdGamesFolder.Location = New System.Drawing.Point(392, 22)
        Me.cmdGamesFolder.Name = "cmdGamesFolder"
        Me.cmdGamesFolder.Size = New System.Drawing.Size(75, 23)
        Me.cmdGamesFolder.TabIndex = 2
        Me.cmdGamesFolder.Text = "Change..."
        Me.cmdGamesFolder.UseVisualStyleBackColor = True
        '
        'txtGamesFolder
        '
        Me.txtGamesFolder.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtGamesFolder.Location = New System.Drawing.Point(10, 24)
        Me.txtGamesFolder.Name = "txtGamesFolder"
        Me.txtGamesFolder.ReadOnly = True
        Me.txtGamesFolder.Size = New System.Drawing.Size(376, 20)
        Me.txtGamesFolder.TabIndex = 1
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(7, 7)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(104, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Download games to:"
        '
        'chkShowAdult
        '
        Me.chkShowAdult.AutoSize = True
        Me.chkShowAdult.Location = New System.Drawing.Point(10, 82)
        Me.chkShowAdult.Name = "chkShowAdult"
        Me.chkShowAdult.Size = New System.Drawing.Size(113, 17)
        Me.chkShowAdult.TabIndex = 4
        Me.chkShowAdult.Text = "Show adult games"
        Me.chkShowAdult.UseVisualStyleBackColor = True
        '
        'OptionsDialog
        '
        Me.AcceptButton = Me.cmdOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.cmdCancel
        Me.ClientSize = New System.Drawing.Size(505, 392)
        Me.Controls.Add(Me.ctlTabs)
        Me.Controls.Add(Me.cmdOK)
        Me.Controls.Add(Me.cmdCancel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "OptionsDialog"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Options"
        Me.ctlTabs.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.TabPage1.PerformLayout()
        Me.TabPage2.ResumeLayout(False)
        Me.TabPage2.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents dlgColor As System.Windows.Forms.ColorDialog
    Friend WithEvents cmdCancel As System.Windows.Forms.Button
    Friend WithEvents cmdOK As System.Windows.Forms.Button
    Friend WithEvents dlgFont As System.Windows.Forms.FontDialog
    Friend WithEvents ctlTabs As System.Windows.Forms.TabControl
    Friend WithEvents TabPage1 As System.Windows.Forms.TabPage
    Friend WithEvents cmdFont As System.Windows.Forms.Button
    Friend WithEvents lblFontSample As System.Windows.Forms.Label
    Friend WithEvents chkUseDefaultFont As System.Windows.Forms.CheckBox
    Friend WithEvents lblForeground As System.Windows.Forms.Label
    Friend WithEvents cmdForeground As System.Windows.Forms.Button
    Friend WithEvents lblBackground As System.Windows.Forms.Label
    Friend WithEvents cmdBackground As System.Windows.Forms.Button
    Friend WithEvents lblLink As System.Windows.Forms.Label
    Friend WithEvents cmdLink As System.Windows.Forms.Button
    Friend WithEvents chkUseDefaultColours As System.Windows.Forms.CheckBox
    Friend WithEvents TabPage2 As System.Windows.Forms.TabPage
    Friend WithEvents txtGamesFolder As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents cmdGamesFolder As System.Windows.Forms.Button
    Friend WithEvents chkShowSandpit As System.Windows.Forms.CheckBox
    Friend WithEvents dlgFolderBrowser As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents chkPlaySounds As System.Windows.Forms.CheckBox
    Friend WithEvents chkShowAdult As System.Windows.Forms.CheckBox
End Class
