<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class OptionsDialog
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.dlgColor = New System.Windows.Forms.ColorDialog()
        Me.cmdCancel = New System.Windows.Forms.Button()
        Me.cmdOK = New System.Windows.Forms.Button()
        Me.dlgFont = New System.Windows.Forms.FontDialog()
        Me.ctlTabs = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.TabPage2 = New System.Windows.Forms.TabPage()
        Me.lnkShowAdultHelp = New System.Windows.Forms.LinkLabel()
        Me.chkShowAdult = New System.Windows.Forms.CheckBox()
        Me.chkShowSandpit = New System.Windows.Forms.CheckBox()
        Me.cmdGamesFolder = New System.Windows.Forms.Button()
        Me.txtGamesFolder = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.dlgFolderBrowser = New System.Windows.Forms.FolderBrowserDialog()
        Me.chkPlaySounds = New System.Windows.Forms.CheckBox()
        Me.chkUseSAPI = New System.Windows.Forms.CheckBox()
        Me.chkUseDefaultFont = New System.Windows.Forms.CheckBox()
        Me.lblFontSample = New System.Windows.Forms.Label()
        Me.cmdFont = New System.Windows.Forms.Button()
        Me.chkUseDefaultColours = New System.Windows.Forms.CheckBox()
        Me.cmdLink = New System.Windows.Forms.Button()
        Me.lblLink = New System.Windows.Forms.Label()
        Me.cmdBackground = New System.Windows.Forms.Button()
        Me.lblBackground = New System.Windows.Forms.Label()
        Me.cmdForeground = New System.Windows.Forms.Button()
        Me.lblForeground = New System.Windows.Forms.Label()
        Me.ctlTabs.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.TabPage2.SuspendLayout()
        Me.SuspendLayout()
        '
        'cmdCancel
        '
        Me.cmdCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdCancel.BackColor = System.Drawing.SystemColors.Control
        Me.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.cmdCancel.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdCancel.Location = New System.Drawing.Point(519, 377)
        Me.cmdCancel.Margin = New System.Windows.Forms.Padding(4)
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.Size = New System.Drawing.Size(100, 28)
        Me.cmdCancel.TabIndex = 9
        Me.cmdCancel.Text = "Cancel"
        Me.cmdCancel.UseVisualStyleBackColor = False
        '
        'cmdOK
        '
        Me.cmdOK.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdOK.BackColor = System.Drawing.SystemColors.Control
        Me.cmdOK.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdOK.Location = New System.Drawing.Point(411, 377)
        Me.cmdOK.Margin = New System.Windows.Forms.Padding(4)
        Me.cmdOK.Name = "cmdOK"
        Me.cmdOK.Size = New System.Drawing.Size(100, 28)
        Me.cmdOK.TabIndex = 8
        Me.cmdOK.Text = "OK"
        Me.cmdOK.UseVisualStyleBackColor = False
        '
        'dlgFont
        '
        Me.dlgFont.Color = System.Drawing.SystemColors.ControlText
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
        Me.ctlTabs.Location = New System.Drawing.Point(16, 15)
        Me.ctlTabs.Margin = New System.Windows.Forms.Padding(4)
        Me.ctlTabs.Name = "ctlTabs"
        Me.ctlTabs.SelectedIndex = 0
        Me.ctlTabs.Size = New System.Drawing.Size(603, 352)
        Me.ctlTabs.TabIndex = 12
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.chkUseSAPI)
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
        Me.TabPage1.Location = New System.Drawing.Point(4, 25)
        Me.TabPage1.Margin = New System.Windows.Forms.Padding(4)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(4)
        Me.TabPage1.Size = New System.Drawing.Size(595, 323)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "Player"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'TabPage2
        '
        Me.TabPage2.Controls.Add(Me.lnkShowAdultHelp)
        Me.TabPage2.Controls.Add(Me.chkShowAdult)
        Me.TabPage2.Controls.Add(Me.chkShowSandpit)
        Me.TabPage2.Controls.Add(Me.cmdGamesFolder)
        Me.TabPage2.Controls.Add(Me.txtGamesFolder)
        Me.TabPage2.Controls.Add(Me.Label1)
        Me.TabPage2.Location = New System.Drawing.Point(4, 25)
        Me.TabPage2.Margin = New System.Windows.Forms.Padding(4)
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.Padding = New System.Windows.Forms.Padding(4)
        Me.TabPage2.Size = New System.Drawing.Size(595, 323)
        Me.TabPage2.TabIndex = 1
        Me.TabPage2.Text = "Game Browser"
        Me.TabPage2.UseVisualStyleBackColor = True
        '
        'lnkShowAdultHelp
        '
        Me.lnkShowAdultHelp.AutoSize = True
        Me.lnkShowAdultHelp.Location = New System.Drawing.Point(179, 100)
        Me.lnkShowAdultHelp.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lnkShowAdultHelp.Name = "lnkShowAdultHelp"
        Me.lnkShowAdultHelp.Size = New System.Drawing.Size(171, 17)
        Me.lnkShowAdultHelp.TabIndex = 5
        Me.lnkShowAdultHelp.TabStop = True
        Me.lnkShowAdultHelp.Text = "How to remove this option"
        '
        'chkShowAdult
        '
        Me.chkShowAdult.AutoSize = True
        Me.chkShowAdult.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.chkShowAdult.Location = New System.Drawing.Point(11, 98)
        Me.chkShowAdult.Margin = New System.Windows.Forms.Padding(4)
        Me.chkShowAdult.Name = "chkShowAdult"
        Me.chkShowAdult.Size = New System.Drawing.Size(148, 22)
        Me.chkShowAdult.TabIndex = 4
        Me.chkShowAdult.Text = "Show adult games"
        Me.chkShowAdult.UseVisualStyleBackColor = True
        '
        'chkShowSandpit
        '
        Me.chkShowSandpit.AutoSize = True
        Me.chkShowSandpit.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.chkShowSandpit.Location = New System.Drawing.Point(11, 68)
        Me.chkShowSandpit.Margin = New System.Windows.Forms.Padding(4)
        Me.chkShowSandpit.Name = "chkShowSandpit"
        Me.chkShowSandpit.Size = New System.Drawing.Size(176, 22)
        Me.chkShowSandpit.TabIndex = 3
        Me.chkShowSandpit.Text = "Show sandpit category"
        Me.chkShowSandpit.UseVisualStyleBackColor = True
        '
        'cmdGamesFolder
        '
        Me.cmdGamesFolder.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdGamesFolder.BackColor = System.Drawing.SystemColors.Control
        Me.cmdGamesFolder.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdGamesFolder.Location = New System.Drawing.Point(487, 35)
        Me.cmdGamesFolder.Margin = New System.Windows.Forms.Padding(4)
        Me.cmdGamesFolder.Name = "cmdGamesFolder"
        Me.cmdGamesFolder.Size = New System.Drawing.Size(100, 28)
        Me.cmdGamesFolder.TabIndex = 2
        Me.cmdGamesFolder.Text = "Change..."
        Me.cmdGamesFolder.UseVisualStyleBackColor = False
        '
        'txtGamesFolder
        '
        Me.txtGamesFolder.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtGamesFolder.BackColor = System.Drawing.SystemColors.Control
        Me.txtGamesFolder.Location = New System.Drawing.Point(11, 38)
        Me.txtGamesFolder.Margin = New System.Windows.Forms.Padding(4)
        Me.txtGamesFolder.Name = "txtGamesFolder"
        Me.txtGamesFolder.ReadOnly = True
        Me.txtGamesFolder.Size = New System.Drawing.Size(468, 22)
        Me.txtGamesFolder.TabIndex = 1
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(8, 17)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(136, 17)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Download games to:"
        '
        'chkPlaySounds
        '
        Me.chkPlaySounds.AutoSize = True
        Me.chkPlaySounds.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.chkPlaySounds.Location = New System.Drawing.Point(8, 265)
        Me.chkPlaySounds.Margin = New System.Windows.Forms.Padding(4)
        Me.chkPlaySounds.Name = "chkPlaySounds"
        Me.chkPlaySounds.Size = New System.Drawing.Size(110, 22)
        Me.chkPlaySounds.TabIndex = 22
        Me.chkPlaySounds.Text = "Play &sounds"
        Me.chkPlaySounds.UseVisualStyleBackColor = True
        '
        'chkUseSAPI
        '
        Me.chkUseSAPI.AutoSize = True
        Me.chkUseSAPI.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.chkUseSAPI.Location = New System.Drawing.Point(8, 295)
        Me.chkUseSAPI.Margin = New System.Windows.Forms.Padding(4)
        Me.chkUseSAPI.Name = "chkUseSAPI"
        Me.chkUseSAPI.Size = New System.Drawing.Size(183, 22)
        Me.chkUseSAPI.TabIndex = 23
        Me.chkUseSAPI.Text = "Speak all text (via SAPI)"
        Me.chkUseSAPI.UseVisualStyleBackColor = True
        '
        'chkUseDefaultFont
        '
        Me.chkUseDefaultFont.AutoSize = True
        Me.chkUseDefaultFont.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.chkUseDefaultFont.Location = New System.Drawing.Point(8, 152)
        Me.chkUseDefaultFont.Margin = New System.Windows.Forms.Padding(4)
        Me.chkUseDefaultFont.Name = "chkUseDefaultFont"
        Me.chkUseDefaultFont.Size = New System.Drawing.Size(172, 22)
        Me.chkUseDefaultFont.TabIndex = 19
        Me.chkUseDefaultFont.Text = "Use default game &font"
        Me.chkUseDefaultFont.UseVisualStyleBackColor = True
        '
        'lblFontSample
        '
        Me.lblFontSample.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblFontSample.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.lblFontSample.Location = New System.Drawing.Point(286, 180)
        Me.lblFontSample.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblFontSample.Name = "lblFontSample"
        Me.lblFontSample.Size = New System.Drawing.Size(298, 58)
        Me.lblFontSample.TabIndex = 20
        Me.lblFontSample.Text = "Sample Text"
        Me.lblFontSample.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'cmdFont
        '
        Me.cmdFont.BackColor = System.Drawing.SystemColors.Control
        Me.cmdFont.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdFont.Location = New System.Drawing.Point(286, 148)
        Me.cmdFont.Margin = New System.Windows.Forms.Padding(4)
        Me.cmdFont.Name = "cmdFont"
        Me.cmdFont.Size = New System.Drawing.Size(100, 28)
        Me.cmdFont.TabIndex = 21
        Me.cmdFont.Text = "&Modify..."
        Me.cmdFont.UseVisualStyleBackColor = False
        '
        'chkUseDefaultColours
        '
        Me.chkUseDefaultColours.AutoSize = True
        Me.chkUseDefaultColours.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.chkUseDefaultColours.Location = New System.Drawing.Point(8, 17)
        Me.chkUseDefaultColours.Margin = New System.Windows.Forms.Padding(4)
        Me.chkUseDefaultColours.Name = "chkUseDefaultColours"
        Me.chkUseDefaultColours.Size = New System.Drawing.Size(194, 22)
        Me.chkUseDefaultColours.TabIndex = 12
        Me.chkUseDefaultColours.Text = "Use default game &colours"
        Me.chkUseDefaultColours.UseVisualStyleBackColor = True
        '
        'cmdLink
        '
        Me.cmdLink.BackColor = System.Drawing.Color.Blue
        Me.cmdLink.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdLink.Location = New System.Drawing.Point(377, 85)
        Me.cmdLink.Margin = New System.Windows.Forms.Padding(4)
        Me.cmdLink.Name = "cmdLink"
        Me.cmdLink.Size = New System.Drawing.Size(207, 28)
        Me.cmdLink.TabIndex = 18
        Me.cmdLink.UseVisualStyleBackColor = True
        '
        'lblLink
        '
        Me.lblLink.AutoSize = True
        Me.lblLink.Location = New System.Drawing.Point(283, 91)
        Me.lblLink.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblLink.Name = "lblLink"
        Me.lblLink.Size = New System.Drawing.Size(38, 17)
        Me.lblLink.TabIndex = 17
        Me.lblLink.Text = "&Link:"
        '
        'cmdBackground
        '
        Me.cmdBackground.BackColor = System.Drawing.Color.Black
        Me.cmdBackground.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdBackground.Location = New System.Drawing.Point(377, 49)
        Me.cmdBackground.Margin = New System.Windows.Forms.Padding(4)
        Me.cmdBackground.Name = "cmdBackground"
        Me.cmdBackground.Size = New System.Drawing.Size(207, 28)
        Me.cmdBackground.TabIndex = 16
        Me.cmdBackground.UseVisualStyleBackColor = True
        '
        'lblBackground
        '
        Me.lblBackground.AutoSize = True
        Me.lblBackground.Location = New System.Drawing.Point(283, 55)
        Me.lblBackground.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblBackground.Name = "lblBackground"
        Me.lblBackground.Size = New System.Drawing.Size(88, 17)
        Me.lblBackground.TabIndex = 15
        Me.lblBackground.Text = "&Background:"
        '
        'cmdForeground
        '
        Me.cmdForeground.BackColor = System.Drawing.Color.White
        Me.cmdForeground.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdForeground.Location = New System.Drawing.Point(377, 13)
        Me.cmdForeground.Margin = New System.Windows.Forms.Padding(4)
        Me.cmdForeground.Name = "cmdForeground"
        Me.cmdForeground.Size = New System.Drawing.Size(207, 28)
        Me.cmdForeground.TabIndex = 14
        Me.cmdForeground.UseVisualStyleBackColor = True
        '
        'lblForeground
        '
        Me.lblForeground.AutoSize = True
        Me.lblForeground.Location = New System.Drawing.Point(283, 17)
        Me.lblForeground.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblForeground.Name = "lblForeground"
        Me.lblForeground.Size = New System.Drawing.Size(39, 17)
        Me.lblForeground.TabIndex = 13
        Me.lblForeground.Text = "&Text:"
        '
        'OptionsDialog
        '
        Me.AcceptButton = Me.cmdOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.GhostWhite
        Me.CancelButton = Me.cmdCancel
        Me.ClientSize = New System.Drawing.Size(635, 418)
        Me.Controls.Add(Me.ctlTabs)
        Me.Controls.Add(Me.cmdOK)
        Me.Controls.Add(Me.cmdCancel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Margin = New System.Windows.Forms.Padding(4)
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
    Friend WithEvents TabPage2 As System.Windows.Forms.TabPage
    Friend WithEvents txtGamesFolder As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents cmdGamesFolder As System.Windows.Forms.Button
    Friend WithEvents chkShowSandpit As System.Windows.Forms.CheckBox
    Friend WithEvents dlgFolderBrowser As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents chkShowAdult As System.Windows.Forms.CheckBox
    Friend WithEvents lnkShowAdultHelp As System.Windows.Forms.LinkLabel
    Friend WithEvents chkUseSAPI As CheckBox
    Friend WithEvents chkPlaySounds As CheckBox
    Friend WithEvents cmdFont As Button
    Friend WithEvents lblFontSample As Label
    Friend WithEvents chkUseDefaultFont As CheckBox
    Friend WithEvents lblForeground As Label
    Friend WithEvents cmdForeground As Button
    Friend WithEvents lblBackground As Label
    Friend WithEvents cmdBackground As Button
    Friend WithEvents lblLink As Label
    Friend WithEvents cmdLink As Button
    Friend WithEvents chkUseDefaultColours As CheckBox
End Class
