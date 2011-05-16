<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class RichTextControl
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(RichTextControl))
        Me.ctlToolbar = New System.Windows.Forms.ToolStrip()
        Me.ToolStripButton1 = New System.Windows.Forms.ToolStripButton()
        Me.ctlWebBrowser = New System.Windows.Forms.WebBrowser()
        Me.ctlToolbar.SuspendLayout()
        Me.SuspendLayout()
        '
        'ctlToolbar
        '
        Me.ctlToolbar.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripButton1})
        Me.ctlToolbar.Location = New System.Drawing.Point(0, 0)
        Me.ctlToolbar.Name = "ctlToolbar"
        Me.ctlToolbar.Size = New System.Drawing.Size(469, 25)
        Me.ctlToolbar.TabIndex = 0
        Me.ctlToolbar.Text = "ToolStrip1"
        '
        'ToolStripButton1
        '
        Me.ToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton1.Image = CType(resources.GetObject("ToolStripButton1.Image"), System.Drawing.Image)
        Me.ToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton1.Name = "ToolStripButton1"
        Me.ToolStripButton1.Size = New System.Drawing.Size(23, 22)
        Me.ToolStripButton1.Text = "ToolStripButton1"
        '
        'ctlWebBrowser
        '
        Me.ctlWebBrowser.AllowWebBrowserDrop = False
        Me.ctlWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlWebBrowser.IsWebBrowserContextMenuEnabled = False
        Me.ctlWebBrowser.Location = New System.Drawing.Point(0, 25)
        Me.ctlWebBrowser.MinimumSize = New System.Drawing.Size(20, 20)
        Me.ctlWebBrowser.Name = "ctlWebBrowser"
        Me.ctlWebBrowser.Size = New System.Drawing.Size(469, 238)
        Me.ctlWebBrowser.TabIndex = 1
        '
        'RichTextControl
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.ctlWebBrowser)
        Me.Controls.Add(Me.ctlToolbar)
        Me.Name = "RichTextControl"
        Me.Size = New System.Drawing.Size(469, 263)
        Me.ctlToolbar.ResumeLayout(False)
        Me.ctlToolbar.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ctlToolbar As System.Windows.Forms.ToolStrip
    Friend WithEvents ToolStripButton1 As System.Windows.Forms.ToolStripButton
    Friend WithEvents ctlWebBrowser As System.Windows.Forms.WebBrowser

End Class
