<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ScriptEditorPopOut
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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
        Me.ctlScriptEditor = New AxeSoftware.Quest.ScriptEditor()
        Me.SuspendLayout()
        '
        'ctlScriptEditor
        '
        Me.ctlScriptEditor.AttributeName = Nothing
        Me.ctlScriptEditor.Controller = Nothing
        Me.ctlScriptEditor.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlScriptEditor.ElementName = Nothing
        Me.ctlScriptEditor.Location = New System.Drawing.Point(0, 0)
        Me.ctlScriptEditor.Name = "ctlScriptEditor"
        Me.ctlScriptEditor.Size = New System.Drawing.Size(570, 302)
        Me.ctlScriptEditor.TabIndex = 0
        Me.ctlScriptEditor.Value = Nothing
        '
        'ScriptEditorPopOut
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(570, 302)
        Me.Controls.Add(Me.ctlScriptEditor)
        Me.KeyPreview = True
        Me.Name = "ScriptEditorPopOut"
        Me.ShowIcon = False
        Me.Text = "Script Editor"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ctlScriptEditor As AxeSoftware.Quest.ScriptEditor
End Class
