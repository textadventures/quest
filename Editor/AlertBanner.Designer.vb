<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class AlertBanner
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
        Me.lblAlertText = New System.Windows.Forms.Label()
        Me.cmdAction = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'lblAlertText
        '
        Me.lblAlertText.AutoSize = True
        Me.lblAlertText.ForeColor = System.Drawing.Color.Black
        Me.lblAlertText.Location = New System.Drawing.Point(3, 5)
        Me.lblAlertText.Name = "lblAlertText"
        Me.lblAlertText.Size = New System.Drawing.Size(28, 13)
        Me.lblAlertText.TabIndex = 0
        Me.lblAlertText.Text = "Text"
        '
        'cmdAction
        '
        Me.cmdAction.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdAction.BackColor = System.Drawing.SystemColors.Control
        Me.cmdAction.Location = New System.Drawing.Point(231, 0)
        Me.cmdAction.Name = "cmdAction"
        Me.cmdAction.Size = New System.Drawing.Size(75, 23)
        Me.cmdAction.TabIndex = 1
        Me.cmdAction.Text = "Action"
        Me.cmdAction.UseVisualStyleBackColor = True
        '
        'AlertBanner
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.Controls.Add(Me.cmdAction)
        Me.Controls.Add(Me.lblAlertText)
        Me.Name = "AlertBanner"
        Me.Size = New System.Drawing.Size(309, 23)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents lblAlertText As System.Windows.Forms.Label
    Private WithEvents cmdAction As System.Windows.Forms.Button

End Class
