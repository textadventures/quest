<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CheckBoxControl
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
        Me.chkCheckBox = New System.Windows.Forms.CheckBox()
        Me.SuspendLayout()
        '
        'chkCheckBox
        '
        Me.chkCheckBox.AutoSize = True
        Me.chkCheckBox.Dock = System.Windows.Forms.DockStyle.Fill
        Me.chkCheckBox.Location = New System.Drawing.Point(0, 0)
        Me.chkCheckBox.Name = "chkCheckBox"
        Me.chkCheckBox.Padding = New System.Windows.Forms.Padding(3, 0, 0, 0)
        Me.chkCheckBox.Size = New System.Drawing.Size(368, 20)
        Me.chkCheckBox.TabIndex = 0
        Me.chkCheckBox.UseVisualStyleBackColor = True
        '
        'CheckBoxControl
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.chkCheckBox)
        Me.Name = "CheckBoxControl"
        Me.Size = New System.Drawing.Size(368, 20)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents chkCheckBox As System.Windows.Forms.CheckBox

End Class
