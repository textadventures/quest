<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class TextEditorControl
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
        Me.ctlElementHost = New System.Windows.Forms.Integration.ElementHost()
        Me.wpfTextEditor = New TextAdventures.Quest.EditorControls.TextEditorControl()
        Me.SuspendLayout()
        '
        'ctlElementHost
        '
        Me.ctlElementHost.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlElementHost.Location = New System.Drawing.Point(0, 0)
        Me.ctlElementHost.Name = "ctlElementHost"
        Me.ctlElementHost.Size = New System.Drawing.Size(150, 150)
        Me.ctlElementHost.TabIndex = 0
        Me.ctlElementHost.Text = "ElementHost1"
        Me.ctlElementHost.Child = Me.wpfTextEditor
        '
        'TextEditorControl
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.ctlElementHost)
        Me.Name = "TextEditorControl"
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents ctlElementHost As System.Windows.Forms.Integration.ElementHost
    Private wpfTextEditor As TextAdventures.Quest.EditorControls.TextEditorControl

End Class
