<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MultiControl
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
        Me.lstTypes = New System.Windows.Forms.ComboBox()
        Me.SuspendLayout()
        '
        'lstTypes
        '
        Me.lstTypes.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lstTypes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.lstTypes.FormattingEnabled = True
        Me.lstTypes.Location = New System.Drawing.Point(0, 0)
        Me.lstTypes.Name = "lstTypes"
        Me.lstTypes.Size = New System.Drawing.Size(519, 21)
        Me.lstTypes.TabIndex = 0
        '
        'MultiControl
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.lstTypes)
        Me.Name = "MultiControl"
        Me.Size = New System.Drawing.Size(519, 60)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents lstTypes As System.Windows.Forms.ComboBox

End Class
