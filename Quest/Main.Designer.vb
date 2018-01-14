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
    <System.Diagnostics.DebuggerStepThrough()>
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
        resources.ApplyResources(Me.dlgOpenFile, "dlgOpenFile")
        '
        'ctlPlayer
        '
        resources.ApplyResources(Me.ctlPlayer, "ctlPlayer")
        Me.ctlPlayer.Name = "ctlPlayer"
        Me.ctlPlayer.PlaySounds = True
        Me.ctlPlayer.PostLaunchAction = Nothing
        Me.ctlPlayer.PreLaunchAction = Nothing
        Me.ctlPlayer.RecordWalkthrough = Nothing
        Me.ctlPlayer.UseGameColours = True
        Me.ctlPlayer.UseGameFont = True
        Me.ctlPlayer.UseSAPI = False
        '
        'ctlEditor
        '
        resources.ApplyResources(Me.ctlEditor, "ctlEditor")
        Me.ctlEditor.BackColor = System.Drawing.Color.GhostWhite
        Me.ctlEditor.Name = "ctlEditor"
        Me.ctlEditor.SimpleMode = False
        '
        'ctlMenu
        '
        resources.ApplyResources(Me.ctlMenu, "ctlMenu")
        Me.ctlMenu.BackColor = System.Drawing.Color.GhostWhite
        Me.ctlMenu.ForeColor = System.Drawing.SystemColors.ControlText
        Me.ctlMenu.Mode = TextAdventures.Quest.Controls.Menu.MenuMode.GameBrowser
        Me.ctlMenu.Name = "ctlMenu"
        '
        'ctlLauncherHost
        '
        resources.ApplyResources(Me.ctlLauncherHost, "ctlLauncherHost")
        Me.ctlLauncherHost.BackColor = System.Drawing.Color.GhostWhite
        Me.ctlLauncherHost.Name = "ctlLauncherHost"
        Me.ctlLauncherHost.Child = Me.ctlLauncher
        '
        'Main
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.ctlLauncherHost)
        Me.Controls.Add(Me.ctlEditor)
        Me.Controls.Add(Me.ctlPlayer)
        Me.Controls.Add(Me.ctlMenu)
        Me.KeyPreview = True
        Me.Name = "Main"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ctlPlayer As TextAdventures.Quest.Player
    Friend WithEvents dlgOpenFile As System.Windows.Forms.OpenFileDialog
    Friend WithEvents ctlEditor As Editor
    Friend WithEvents ctlMenu As TextAdventures.Quest.Controls.Menu
    Friend WithEvents ctlLauncherHost As System.Windows.Forms.Integration.ElementHost
    Friend ctlLauncher As GameBrowser.Launcher

End Class
