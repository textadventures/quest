<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FileControl
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
        Me.ctlDropDown = New AxeSoftware.Quest.DropDownFileControl()
        Me.cmdBrowse = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'ctlDropDown
        '
        Me.ctlDropDown.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ctlDropDown.Controller = Nothing
        Me.ctlDropDown.Location = New System.Drawing.Point(0, 1)
        Me.ctlDropDown.Name = "ctlDropDown"
        Me.ctlDropDown.Size = New System.Drawing.Size(243, 23)
        Me.ctlDropDown.TabIndex = 0
        Me.ctlDropDown.Value = ""
        '
        'cmdBrowse
        '
        Me.cmdBrowse.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdBrowse.Location = New System.Drawing.Point(249, 0)
        Me.cmdBrowse.Name = "cmdBrowse"
        Me.cmdBrowse.Size = New System.Drawing.Size(75, 23)
        Me.cmdBrowse.TabIndex = 1
        Me.cmdBrowse.Text = "Browse..."
        Me.cmdBrowse.UseVisualStyleBackColor = True
        '
        'FileControl
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.cmdBrowse)
        Me.Controls.Add(Me.ctlDropDown)
        Me.Name = "FileControl"
        Me.Size = New System.Drawing.Size(324, 24)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ctlDropDown As AxeSoftware.Quest.DropDownFileControl
    Friend WithEvents cmdBrowse As System.Windows.Forms.Button

End Class
