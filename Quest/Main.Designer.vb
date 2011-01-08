Imports AxeSoftware.Quest

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Main
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Main))
        Me.dlgOpenFile = New System.Windows.Forms.OpenFileDialog()
        Me.ctlPlayer = New AxeSoftware.Quest.Player()
        Me.ctlEditor = New AxeSoftware.Quest.Editor()
        Me.ctlMenu = New AxeSoftware.Quest.Controls.Menu()
        Me.ctlLauncher = New GameBrowser.Launcher()
        Me.SuspendLayout()
        '
        'dlgOpenFile
        '
        Me.dlgOpenFile.FileName = "OpenFileDialog1"
        '
        'ctlPlayer
        '
        Me.ctlPlayer.Bold = False
        Me.ctlPlayer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlPlayer.FontName = "Arial"
        Me.ctlPlayer.FontSize = 9
        Me.ctlPlayer.Foreground = Nothing
        Me.ctlPlayer.ForegroundOverride = Nothing
        Me.ctlPlayer.Italic = False
        Me.ctlPlayer.LinkForeground = Nothing
        Me.ctlPlayer.Location = New System.Drawing.Point(0, 24)
        Me.ctlPlayer.Name = "ctlPlayer"
        Me.ctlPlayer.PanesVisible = True
        Me.ctlPlayer.Size = New System.Drawing.Size(734, 464)
        Me.ctlPlayer.TabIndex = 0
        Me.ctlPlayer.Underline = False
        '
        'ctlEditor
        '
        Me.ctlEditor.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlEditor.Location = New System.Drawing.Point(0, 24)
        Me.ctlEditor.Name = "ctlEditor"
        Me.ctlEditor.Size = New System.Drawing.Size(734, 464)
        Me.ctlEditor.TabIndex = 3
        Me.ctlEditor.Visible = False
        '
        'ctlMenu
        '
        Me.ctlMenu.Dock = System.Windows.Forms.DockStyle.Top
        Me.ctlMenu.Location = New System.Drawing.Point(0, 0)
        Me.ctlMenu.Mode = AxeSoftware.Quest.Controls.Menu.MenuMode.GameBrowser
        Me.ctlMenu.Name = "ctlMenu"
        Me.ctlMenu.Size = New System.Drawing.Size(734, 24)
        Me.ctlMenu.TabIndex = 5
        '
        'ctlLauncher
        '
        Me.ctlLauncher.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlLauncher.Location = New System.Drawing.Point(0, 24)
        Me.ctlLauncher.Name = "ctlLauncher"
        Me.ctlLauncher.Padding = New System.Windows.Forms.Padding(3)
        Me.ctlLauncher.Size = New System.Drawing.Size(734, 464)
        Me.ctlLauncher.TabIndex = 6
        '
        'Main
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(734, 488)
        Me.Controls.Add(Me.ctlLauncher)
        Me.Controls.Add(Me.ctlEditor)
        Me.Controls.Add(Me.ctlPlayer)
        Me.Controls.Add(Me.ctlMenu)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.KeyPreview = True
        Me.Name = "Main"
        Me.Text = "Quest"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ctlPlayer As AxeSoftware.Quest.Player
    Friend WithEvents dlgOpenFile As System.Windows.Forms.OpenFileDialog
    Friend WithEvents ctlEditor As Editor
    Friend WithEvents ctlMenu As AxeSoftware.Quest.Controls.Menu
    Friend WithEvents ctlLauncher As GameBrowser.Launcher

End Class
