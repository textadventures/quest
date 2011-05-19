<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Launcher
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
        Me.ctlTabs = New System.Windows.Forms.TabControl()
        Me.tabPlay = New System.Windows.Forms.TabPage()
        Me.tabEdit = New System.Windows.Forms.TabPage()
        Me.ctlPlayBrowser = New GameBrowser.PlayBrowser()
        Me.ctlEditBrowser = New GameBrowser.EditBrowser()
        Me.ctlVersionInfo = New GameBrowser.BetaInfo()
        Me.ctlTabs.SuspendLayout()
        Me.tabPlay.SuspendLayout()
        Me.tabEdit.SuspendLayout()
        Me.SuspendLayout()
        '
        'ctlTabs
        '
        Me.ctlTabs.Controls.Add(Me.tabPlay)
        Me.ctlTabs.Controls.Add(Me.tabEdit)
        Me.ctlTabs.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlTabs.ItemSize = New System.Drawing.Size(62, 18)
        Me.ctlTabs.Location = New System.Drawing.Point(0, 0)
        Me.ctlTabs.Name = "ctlTabs"
        Me.ctlTabs.Padding = New System.Drawing.Point(0, 0)
        Me.ctlTabs.SelectedIndex = 0
        Me.ctlTabs.Size = New System.Drawing.Size(663, 371)
        Me.ctlTabs.TabIndex = 0
        '
        'tabPlay
        '
        Me.tabPlay.Controls.Add(Me.ctlPlayBrowser)
        Me.tabPlay.Location = New System.Drawing.Point(4, 22)
        Me.tabPlay.Name = "tabPlay"
        Me.tabPlay.Size = New System.Drawing.Size(655, 345)
        Me.tabPlay.TabIndex = 0
        Me.tabPlay.Tag = ""
        Me.tabPlay.Text = "Play"
        Me.tabPlay.UseVisualStyleBackColor = True
        '
        'tabEdit
        '
        Me.tabEdit.Controls.Add(Me.ctlEditBrowser)
        Me.tabEdit.Location = New System.Drawing.Point(4, 22)
        Me.tabEdit.Name = "tabEdit"
        Me.tabEdit.Size = New System.Drawing.Size(655, 339)
        Me.tabEdit.TabIndex = 1
        Me.tabEdit.Tag = ""
        Me.tabEdit.Text = "Edit"
        Me.tabEdit.UseVisualStyleBackColor = True
        '
        'ctlPlayBrowser
        '
        Me.ctlPlayBrowser.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlPlayBrowser.Location = New System.Drawing.Point(0, 0)
        Me.ctlPlayBrowser.Margin = New System.Windows.Forms.Padding(0)
        Me.ctlPlayBrowser.Name = "ctlPlayBrowser"
        Me.ctlPlayBrowser.Size = New System.Drawing.Size(655, 345)
        Me.ctlPlayBrowser.TabIndex = 0
        '
        'ctlEditBrowser
        '
        Me.ctlEditBrowser.BackColor = System.Drawing.Color.White
        Me.ctlEditBrowser.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlEditBrowser.Location = New System.Drawing.Point(0, 0)
        Me.ctlEditBrowser.Name = "ctlEditBrowser"
        Me.ctlEditBrowser.Size = New System.Drawing.Size(655, 339)
        Me.ctlEditBrowser.TabIndex = 2
        '
        'ctlVersionInfo
        '
        Me.ctlVersionInfo.BackColor = System.Drawing.Color.White
        Me.ctlVersionInfo.CurrentVersion = Nothing
        Me.ctlVersionInfo.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.ctlVersionInfo.Location = New System.Drawing.Point(0, 371)
        Me.ctlVersionInfo.Name = "ctlVersionInfo"
        Me.ctlVersionInfo.Size = New System.Drawing.Size(663, 86)
        Me.ctlVersionInfo.TabIndex = 2
        Me.ctlVersionInfo.UpdateInfo = Nothing
        '
        'Launcher
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.ctlTabs)
        Me.Controls.Add(Me.ctlVersionInfo)
        Me.Name = "Launcher"
        Me.Size = New System.Drawing.Size(663, 457)
        Me.ctlTabs.ResumeLayout(False)
        Me.tabPlay.ResumeLayout(False)
        Me.tabEdit.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ctlTabs As System.Windows.Forms.TabControl
    Friend WithEvents tabPlay As System.Windows.Forms.TabPage
    Friend WithEvents tabEdit As System.Windows.Forms.TabPage
    Friend WithEvents ctlPlayBrowser As GameBrowser.PlayBrowser
    Friend WithEvents ctlEditBrowser As GameBrowser.EditBrowser
    Friend WithEvents ctlVersionInfo As GameBrowser.BetaInfo

End Class
