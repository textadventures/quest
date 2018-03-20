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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(OptionsDialog))
        Me.dlgColor = New System.Windows.Forms.ColorDialog()
        Me.cmdCancel = New System.Windows.Forms.Button()
        Me.cmdOK = New System.Windows.Forms.Button()
        Me.dlgFont = New System.Windows.Forms.FontDialog()
        Me.ctlTabs = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.chkUseSAPI = New System.Windows.Forms.CheckBox()
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
        Me.lnkShowAdultHelp = New System.Windows.Forms.LinkLabel()
        Me.chkShowAdult = New System.Windows.Forms.CheckBox()
        Me.chkShowSandpit = New System.Windows.Forms.CheckBox()
        Me.cmdGamesFolder = New System.Windows.Forms.Button()
        Me.txtGamesFolder = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.dlgFolderBrowser = New System.Windows.Forms.FolderBrowserDialog()
        Me.ctlTabs.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.TabPage2.SuspendLayout()
        Me.SuspendLayout()
        '
        'cmdCancel
        '
        resources.ApplyResources(Me.cmdCancel, "cmdCancel")
        Me.cmdCancel.BackColor = System.Drawing.SystemColors.Control
        Me.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.UseVisualStyleBackColor = False
        '
        'cmdOK
        '
        resources.ApplyResources(Me.cmdOK, "cmdOK")
        Me.cmdOK.BackColor = System.Drawing.SystemColors.Control
        Me.cmdOK.Name = "cmdOK"
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
        resources.ApplyResources(Me.ctlTabs, "ctlTabs")
        Me.ctlTabs.Controls.Add(Me.TabPage1)
        Me.ctlTabs.Controls.Add(Me.TabPage2)
        Me.ctlTabs.Name = "ctlTabs"
        Me.ctlTabs.SelectedIndex = 0
        '
        'TabPage1
        '
        resources.ApplyResources(Me.TabPage1, "TabPage1")
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
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'chkUseSAPI
        '
        resources.ApplyResources(Me.chkUseSAPI, "chkUseSAPI")
        Me.chkUseSAPI.Name = "chkUseSAPI"
        Me.chkUseSAPI.UseVisualStyleBackColor = True
        '
        'chkPlaySounds
        '
        resources.ApplyResources(Me.chkPlaySounds, "chkPlaySounds")
        Me.chkPlaySounds.Name = "chkPlaySounds"
        Me.chkPlaySounds.UseVisualStyleBackColor = True
        '
        'cmdFont
        '
        resources.ApplyResources(Me.cmdFont, "cmdFont")
        Me.cmdFont.BackColor = System.Drawing.SystemColors.Control
        Me.cmdFont.Name = "cmdFont"
        Me.cmdFont.UseVisualStyleBackColor = False
        '
        'lblFontSample
        '
        resources.ApplyResources(Me.lblFontSample, "lblFontSample")
        Me.lblFontSample.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lblFontSample.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.lblFontSample.Name = "lblFontSample"
        '
        'chkUseDefaultFont
        '
        resources.ApplyResources(Me.chkUseDefaultFont, "chkUseDefaultFont")
        Me.chkUseDefaultFont.Name = "chkUseDefaultFont"
        Me.chkUseDefaultFont.UseVisualStyleBackColor = True
        '
        'lblForeground
        '
        resources.ApplyResources(Me.lblForeground, "lblForeground")
        Me.lblForeground.Name = "lblForeground"
        '
        'cmdForeground
        '
        resources.ApplyResources(Me.cmdForeground, "cmdForeground")
        Me.cmdForeground.BackColor = System.Drawing.Color.White
        Me.cmdForeground.Name = "cmdForeground"
        Me.cmdForeground.UseVisualStyleBackColor = True
        '
        'lblBackground
        '
        resources.ApplyResources(Me.lblBackground, "lblBackground")
        Me.lblBackground.Name = "lblBackground"
        '
        'cmdBackground
        '
        resources.ApplyResources(Me.cmdBackground, "cmdBackground")
        Me.cmdBackground.BackColor = System.Drawing.Color.Black
        Me.cmdBackground.Name = "cmdBackground"
        Me.cmdBackground.UseVisualStyleBackColor = True
        '
        'lblLink
        '
        resources.ApplyResources(Me.lblLink, "lblLink")
        Me.lblLink.Name = "lblLink"
        '
        'cmdLink
        '
        resources.ApplyResources(Me.cmdLink, "cmdLink")
        Me.cmdLink.BackColor = System.Drawing.Color.Blue
        Me.cmdLink.Name = "cmdLink"
        Me.cmdLink.UseVisualStyleBackColor = True
        '
        'chkUseDefaultColours
        '
        resources.ApplyResources(Me.chkUseDefaultColours, "chkUseDefaultColours")
        Me.chkUseDefaultColours.Name = "chkUseDefaultColours"
        Me.chkUseDefaultColours.UseVisualStyleBackColor = True
        '
        'TabPage2
        '
        resources.ApplyResources(Me.TabPage2, "TabPage2")
        Me.TabPage2.Controls.Add(Me.lnkShowAdultHelp)
        Me.TabPage2.Controls.Add(Me.chkShowAdult)
        Me.TabPage2.Controls.Add(Me.chkShowSandpit)
        Me.TabPage2.Controls.Add(Me.cmdGamesFolder)
        Me.TabPage2.Controls.Add(Me.txtGamesFolder)
        Me.TabPage2.Controls.Add(Me.Label1)
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.UseVisualStyleBackColor = True
        '
        'lnkShowAdultHelp
        '
        resources.ApplyResources(Me.lnkShowAdultHelp, "lnkShowAdultHelp")
        Me.lnkShowAdultHelp.Name = "lnkShowAdultHelp"
        Me.lnkShowAdultHelp.TabStop = True
        '
        'chkShowAdult
        '
        resources.ApplyResources(Me.chkShowAdult, "chkShowAdult")
        Me.chkShowAdult.Name = "chkShowAdult"
        Me.chkShowAdult.UseVisualStyleBackColor = True
        '
        'chkShowSandpit
        '
        resources.ApplyResources(Me.chkShowSandpit, "chkShowSandpit")
        Me.chkShowSandpit.Name = "chkShowSandpit"
        Me.chkShowSandpit.UseVisualStyleBackColor = True
        '
        'cmdGamesFolder
        '
        resources.ApplyResources(Me.cmdGamesFolder, "cmdGamesFolder")
        Me.cmdGamesFolder.BackColor = System.Drawing.SystemColors.Control
        Me.cmdGamesFolder.Name = "cmdGamesFolder"
        Me.cmdGamesFolder.UseVisualStyleBackColor = False
        '
        'txtGamesFolder
        '
        resources.ApplyResources(Me.txtGamesFolder, "txtGamesFolder")
        Me.txtGamesFolder.BackColor = System.Drawing.SystemColors.Control
        Me.txtGamesFolder.Name = "txtGamesFolder"
        Me.txtGamesFolder.ReadOnly = True
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'dlgFolderBrowser
        '
        resources.ApplyResources(Me.dlgFolderBrowser, "dlgFolderBrowser")
        '
        'OptionsDialog
        '
        Me.AcceptButton = Me.cmdOK
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.GhostWhite
        Me.CancelButton = Me.cmdCancel
        Me.Controls.Add(Me.ctlTabs)
        Me.Controls.Add(Me.cmdOK)
        Me.Controls.Add(Me.cmdCancel)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "OptionsDialog"
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
