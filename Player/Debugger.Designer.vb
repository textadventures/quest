<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Debugger
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Debugger))
        Me.tabsMain = New System.Windows.Forms.TabControl()
        Me.SuspendLayout()
        '
        'tabsMain
        '
        Me.tabsMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tabsMain.Location = New System.Drawing.Point(0, 0)
        Me.tabsMain.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.tabsMain.Name = "tabsMain"
        Me.tabsMain.SelectedIndex = 0
        Me.tabsMain.Size = New System.Drawing.Size(663, 428)
        Me.tabsMain.TabIndex = 1
        '
        'Debugger
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(663, 428)
        Me.Controls.Add(Me.tabsMain)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.Name = "Debugger"
        Me.ShowIcon = False
        Me.Text = "Debugger"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents tabsMain As System.Windows.Forms.TabControl
End Class
