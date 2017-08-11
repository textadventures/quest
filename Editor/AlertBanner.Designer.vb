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
        Me.lblAlertText.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblAlertText.ForeColor = System.Drawing.Color.Black
        Me.lblAlertText.Location = New System.Drawing.Point(1, 1)
        Me.lblAlertText.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblAlertText.Name = "lblAlertText"
        Me.lblAlertText.Size = New System.Drawing.Size(306, 28)
        Me.lblAlertText.TabIndex = 0
        Me.lblAlertText.Text = "Text"
        Me.lblAlertText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'cmdAction
        '
        Me.cmdAction.BackColor = System.Drawing.SystemColors.Control
        Me.cmdAction.Dock = System.Windows.Forms.DockStyle.Right
        Me.cmdAction.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdAction.Location = New System.Drawing.Point(307, 1)
        Me.cmdAction.Margin = New System.Windows.Forms.Padding(4)
        Me.cmdAction.Name = "cmdAction"
        Me.cmdAction.Size = New System.Drawing.Size(100, 28)
        Me.cmdAction.TabIndex = 1
        Me.cmdAction.Text = "Action"
        Me.cmdAction.UseVisualStyleBackColor = False
        '
        'AlertBanner
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.Controls.Add(Me.lblAlertText)
        Me.Controls.Add(Me.cmdAction)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "AlertBanner"
        Me.Padding = New System.Windows.Forms.Padding(1, 1, 5, 1)
        Me.Size = New System.Drawing.Size(412, 30)
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents lblAlertText As System.Windows.Forms.Label
    Private WithEvents cmdAction As System.Windows.Forms.Button

End Class
