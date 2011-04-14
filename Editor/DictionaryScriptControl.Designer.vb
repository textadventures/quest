<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class DictionaryScriptControl
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
        Me.ctlListEditor = New AxeSoftware.Quest.ListEditor()
        Me.SuspendLayout()
        '
        'ctlListEditor
        '
        Me.ctlListEditor.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlListEditor.EditorDelegate = Nothing
        Me.ctlListEditor.Location = New System.Drawing.Point(0, 0)
        Me.ctlListEditor.Name = "ctlListEditor"
        Me.ctlListEditor.Size = New System.Drawing.Size(386, 134)
        Me.ctlListEditor.Style = AxeSoftware.Quest.ListEditor.ColumnStyle.OneColumn
        Me.ctlListEditor.TabIndex = 0
        '
        'DictionaryScriptControl
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.ctlListEditor)
        Me.Name = "DictionaryScriptControl"
        Me.Size = New System.Drawing.Size(386, 134)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ctlListEditor As AxeSoftware.Quest.ListEditor

End Class
