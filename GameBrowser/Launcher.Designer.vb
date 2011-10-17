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
        Me.ElementHost1 = New System.Windows.Forms.Integration.ElementHost()
        Me.ctlPlayBrowser = New GameBrowser.PlayBrowser()
        Me.tabEdit = New System.Windows.Forms.TabPage()
        Me.ElementHost2 = New System.Windows.Forms.Integration.ElementHost()
        Me.ctlEditBrowser = New GameBrowser.EditBrowser()
        Me.ElementHost3 = New System.Windows.Forms.Integration.ElementHost()
        Me.ctlVersionInfo = New GameBrowser.VersionInfo()
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
        Me.ctlTabs.Size = New System.Drawing.Size(663, 357)
        Me.ctlTabs.TabIndex = 0
        '
        'tabPlay
        '
        Me.tabPlay.Controls.Add(Me.ElementHost1)
        Me.tabPlay.Location = New System.Drawing.Point(4, 22)
        Me.tabPlay.Name = "tabPlay"
        Me.tabPlay.Size = New System.Drawing.Size(655, 331)
        Me.tabPlay.TabIndex = 0
        Me.tabPlay.Tag = ""
        Me.tabPlay.Text = "Play"
        Me.tabPlay.UseVisualStyleBackColor = True
        '
        'ElementHost1
        '
        Me.ElementHost1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ElementHost1.Location = New System.Drawing.Point(0, 0)
        Me.ElementHost1.Name = "ElementHost1"
        Me.ElementHost1.Size = New System.Drawing.Size(655, 331)
        Me.ElementHost1.TabIndex = 0
        Me.ElementHost1.Text = "ElementHost1"
        Me.ElementHost1.Child = Me.ctlPlayBrowser
        '
        'tabEdit
        '
        Me.tabEdit.Controls.Add(Me.ElementHost2)
        Me.tabEdit.Location = New System.Drawing.Point(4, 22)
        Me.tabEdit.Name = "tabEdit"
        Me.tabEdit.Size = New System.Drawing.Size(655, 345)
        Me.tabEdit.TabIndex = 1
        Me.tabEdit.Tag = ""
        Me.tabEdit.Text = "Edit"
        Me.tabEdit.UseVisualStyleBackColor = True
        '
        'ElementHost2
        '
        Me.ElementHost2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ElementHost2.Location = New System.Drawing.Point(0, 0)
        Me.ElementHost2.Name = "ElementHost2"
        Me.ElementHost2.Size = New System.Drawing.Size(655, 345)
        Me.ElementHost2.TabIndex = 0
        Me.ElementHost2.Text = "ElementHost2"
        Me.ElementHost2.Child = Me.ctlEditBrowser
        '
        'ElementHost3
        '
        Me.ElementHost3.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.ElementHost3.Location = New System.Drawing.Point(0, 357)
        Me.ElementHost3.Name = "ElementHost3"
        Me.ElementHost3.Size = New System.Drawing.Size(663, 100)
        Me.ElementHost3.TabIndex = 1
        Me.ElementHost3.Text = "ElementHost3"
        Me.ElementHost3.Child = Me.ctlVersionInfo
        '
        'Launcher
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.ctlTabs)
        Me.Controls.Add(Me.ElementHost3)
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
    Friend WithEvents ElementHost1 As System.Windows.Forms.Integration.ElementHost
    Friend ctlPlayBrowser As GameBrowser.PlayBrowser
    Friend WithEvents ElementHost2 As System.Windows.Forms.Integration.ElementHost
    Friend ctlEditBrowser As GameBrowser.EditBrowser
    Friend WithEvents ElementHost3 As System.Windows.Forms.Integration.ElementHost
    Friend ctlVersionInfo As GameBrowser.VersionInfo

End Class
