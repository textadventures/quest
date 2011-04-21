<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class NumberControl
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
        Me.ctlUpDown = New System.Windows.Forms.NumericUpDown()
        CType(Me.ctlUpDown, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'ctlUpDown
        '
        Me.ctlUpDown.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlUpDown.Location = New System.Drawing.Point(0, 0)
        Me.ctlUpDown.Name = "ctlUpDown"
        Me.ctlUpDown.Size = New System.Drawing.Size(120, 20)
        Me.ctlUpDown.TabIndex = 0
        '
        'NumberControl
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.ctlUpDown)
        Me.Name = "NumberControl"
        Me.Size = New System.Drawing.Size(120, 20)
        CType(Me.ctlUpDown, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ctlUpDown As System.Windows.Forms.NumericUpDown

End Class
