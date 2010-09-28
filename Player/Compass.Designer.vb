<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Compass
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
        Me.pnlContainer = New System.Windows.Forms.Panel
        Me.SuspendLayout()
        '
        'pnlContainer
        '
        Me.pnlContainer.Location = New System.Drawing.Point(0, 0)
        Me.pnlContainer.Margin = New System.Windows.Forms.Padding(0)
        Me.pnlContainer.Name = "pnlContainer"
        Me.pnlContainer.Size = New System.Drawing.Size(200, 100)
        Me.pnlContainer.TabIndex = 0
        '
        'Compass
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.pnlContainer)
        Me.Name = "Compass"
        Me.Size = New System.Drawing.Size(553, 231)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents pnlContainer As System.Windows.Forms.Panel

End Class
