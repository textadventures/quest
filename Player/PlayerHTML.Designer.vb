<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class PlayerHTML
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
        Me.wbOutput = New System.Windows.Forms.WebBrowser()
        Me.SuspendLayout()
        '
        'wbOutput
        '
        Me.wbOutput.AllowWebBrowserDrop = False
        Me.wbOutput.Dock = System.Windows.Forms.DockStyle.Fill
        Me.wbOutput.IsWebBrowserContextMenuEnabled = False
        Me.wbOutput.Location = New System.Drawing.Point(0, 0)
        Me.wbOutput.Margin = New System.Windows.Forms.Padding(0)
        Me.wbOutput.MinimumSize = New System.Drawing.Size(20, 20)
        Me.wbOutput.Name = "wbOutput"
        Me.wbOutput.Size = New System.Drawing.Size(635, 400)
        Me.wbOutput.TabIndex = 7
        Me.wbOutput.WebBrowserShortcutsEnabled = False
        '
        'PlayerHTML
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.wbOutput)
        Me.Name = "PlayerHTML"
        Me.Size = New System.Drawing.Size(635, 400)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents wbOutput As System.Windows.Forms.WebBrowser

End Class
