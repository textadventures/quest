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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(AlertBanner))
        Me.lblAlertText = New System.Windows.Forms.Label()
        Me.cmdAction = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'lblAlertText
        '
        resources.ApplyResources(Me.lblAlertText, "lblAlertText")
        Me.lblAlertText.ForeColor = System.Drawing.Color.Black
        Me.lblAlertText.Name = "lblAlertText"
        '
        'cmdAction
        '
        resources.ApplyResources(Me.cmdAction, "cmdAction")
        Me.cmdAction.BackColor = System.Drawing.SystemColors.Control
        Me.cmdAction.Name = "cmdAction"
        Me.cmdAction.UseVisualStyleBackColor = False
        '
        'AlertBanner
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.Controls.Add(Me.cmdAction)
        Me.Controls.Add(Me.lblAlertText)
        Me.Name = "AlertBanner"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents lblAlertText As System.Windows.Forms.Label
    Private WithEvents cmdAction As System.Windows.Forms.Button

End Class
