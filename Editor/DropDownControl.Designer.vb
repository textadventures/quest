<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class DropDownControl
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
        Me.lstDropdown = New System.Windows.Forms.ComboBox()
        Me.SuspendLayout()
        '
        'lstDropdown
        '
        Me.lstDropdown.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.lstDropdown.FormattingEnabled = True
        Me.lstDropdown.Location = New System.Drawing.Point(0, 0)
        Me.lstDropdown.Name = "lstDropdown"
        Me.lstDropdown.Size = New System.Drawing.Size(190, 21)
        Me.lstDropdown.TabIndex = 0
        '
        'DropDownControl
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.lstDropdown)
        Me.Name = "DropDownControl"
        Me.Size = New System.Drawing.Size(190, 23)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents lstDropdown As System.Windows.Forms.ComboBox

End Class
