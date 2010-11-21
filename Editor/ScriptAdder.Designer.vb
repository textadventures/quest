<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ScriptAdder
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
        Me.cmdAdd = New System.Windows.Forms.Button()
        Me.cmdClose = New System.Windows.Forms.Button()
        Me.ctlEditorTree = New AxeSoftware.Quest.EditorTree()
        Me.SuspendLayout()
        '
        'cmdAdd
        '
        Me.cmdAdd.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.cmdAdd.Location = New System.Drawing.Point(3, 178)
        Me.cmdAdd.Name = "cmdAdd"
        Me.cmdAdd.Size = New System.Drawing.Size(75, 23)
        Me.cmdAdd.TabIndex = 3
        Me.cmdAdd.Text = "Add"
        Me.cmdAdd.UseVisualStyleBackColor = True
        '
        'cmdClose
        '
        Me.cmdClose.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdClose.Location = New System.Drawing.Point(281, 178)
        Me.cmdClose.Name = "cmdClose"
        Me.cmdClose.Size = New System.Drawing.Size(75, 23)
        Me.cmdClose.TabIndex = 4
        Me.cmdClose.Text = "Close"
        Me.cmdClose.UseVisualStyleBackColor = True
        '
        'ctlEditorTree
        '
        Me.ctlEditorTree.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ctlEditorTree.Location = New System.Drawing.Point(0, 0)
        Me.ctlEditorTree.Name = "ctlEditorTree"
        Me.ctlEditorTree.ShowFilterBar = False
        Me.ctlEditorTree.Size = New System.Drawing.Size(359, 169)
        Me.ctlEditorTree.TabIndex = 2
        '
        'ScriptAdder
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.cmdClose)
        Me.Controls.Add(Me.cmdAdd)
        Me.Controls.Add(Me.ctlEditorTree)
        Me.Name = "ScriptAdder"
        Me.Size = New System.Drawing.Size(359, 204)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ctlEditorTree As AxeSoftware.Quest.EditorTree
    Friend WithEvents cmdAdd As System.Windows.Forms.Button
    Friend WithEvents cmdClose As System.Windows.Forms.Button

End Class
