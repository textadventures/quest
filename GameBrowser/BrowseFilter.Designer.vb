<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class BrowseFilter
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
        Me.lstCategories = New System.Windows.Forms.ComboBox()
        Me.SuspendLayout()
        '
        'lstCategories
        '
        Me.lstCategories.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstCategories.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.lstCategories.FormattingEnabled = True
        Me.lstCategories.Location = New System.Drawing.Point(0, 0)
        Me.lstCategories.Name = "lstCategories"
        Me.lstCategories.Size = New System.Drawing.Size(187, 21)
        Me.lstCategories.TabIndex = 0
        '
        'BrowseFilter
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.lstCategories)
        Me.Name = "BrowseFilter"
        Me.Size = New System.Drawing.Size(187, 22)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents lstCategories As System.Windows.Forms.ComboBox

End Class
