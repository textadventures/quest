Imports TextAdventures.Quest

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
        Me.ctlPlayer = New TextAdventures.Quest.Player()
        Me.ctlEditor = New TextAdventures.Quest.Editor()
        Me.ctlMenu = New TextAdventures.Quest.Controls.Menu()
        Me.ctlLauncherHost = New System.Windows.Forms.Integration.ElementHost()
        Me.ctlLauncher = New GameBrowser.Launcher()
        Me.SuspendLayout()
        '
        'dlgOpenFile
        '
        Me.dlgOpenFile.FileName = "OpenFileDialog1"
        '
        'ctlPlayer
        '
        Me.ctlPlayer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlPlayer.Location = New System.Drawing.Point(0, 24)
        Me.ctlPlayer.Name = "ctlPlayer"
        Me.ctlPlayer.PostLaunchAction = Nothing
        Me.ctlPlayer.PreLaunchAction = Nothing
        Me.ctlPlayer.RecordWalkthrough = Nothing
        Me.ctlPlayer.Size = New System.Drawing.Size(734, 464)
        Me.ctlPlayer.TabIndex = 0
        Me.ctlPlayer.UseGameColours = True
        Me.ctlPlayer.UseGameFont = True
        '
        'ctlEditor
        '
        Me.ctlEditor.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlEditor.Location = New System.Drawing.Point(0, 24)
        Me.ctlEditor.Name = "ctlEditor"
        Me.ctlEditor.SimpleMode = False
        Me.ctlEditor.Size = New System.Drawing.Size(734, 464)
        Me.ctlEditor.TabIndex = 3
        Me.ctlEditor.Visible = False
        '
        'ctlMenu
        '
        Me.ctlMenu.Dock = System.Windows.Forms.DockStyle.Top
        Me.ctlMenu.Location = New System.Drawing.Point(0, 0)
        Me.ctlMenu.Mode = TextAdventures.Quest.Controls.Menu.MenuMode.GameBrowser
        Me.ctlMenu.Name = "ctlMenu"
        Me.ctlMenu.Size = New System.Drawing.Size(734, 24)
        Me.ctlMenu.TabIndex = 5
        '
        'ctlLauncherHost
        '
        Me.ctlLauncherHost.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlLauncherHost.Location = New System.Drawing.Point(0, 24)
        Me.ctlLauncherHost.Name = "ctlLauncherHost"
        Me.ctlLauncherHost.Size = New System.Drawing.Size(734, 464)
        Me.ctlLauncherHost.TabIndex = 6
        Me.ctlLauncherHost.Text = "ElementHost1"
        Me.ctlLauncherHost.Child = Me.ctlLauncher
        '
        'Main
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(734, 488)
        Me.Controls.Add(Me.ctlLauncherHost)
        Me.Controls.Add(Me.ctlEditor)
        Me.Controls.Add(Me.ctlPlayer)
        Me.Controls.Add(Me.ctlMenu)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.KeyPreview = True
        Me.Name = "Main"
        Me.Text = "Quest"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ctlPlayer As TextAdventures.Quest.Player
    Friend WithEvents dlgOpenFile As System.Windows.Forms.OpenFileDialog
    Friend WithEvents ctlEditor As Editor
    Friend WithEvents ctlMenu As TextAdventures.Quest.Controls.Menu
    Friend WithEvents ctlLauncherHost As System.Windows.Forms.Integration.ElementHost
    Friend ctlLauncher As GameBrowser.Launcher

End Class
