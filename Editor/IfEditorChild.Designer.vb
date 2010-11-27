<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class IfEditorChild
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
        Me.lblThen = New System.Windows.Forms.Label()
        Me.lblIf = New System.Windows.Forms.Label()
        Me.cmdExpand = New System.Windows.Forms.Button()
        Me.ctlThenScript = New AxeSoftware.Quest.ScriptControl()
        Me.ctlExpression = New AxeSoftware.Quest.TextBoxControl()
        Me.SuspendLayout()
        '
        'lblThen
        '
        Me.lblThen.AutoSize = True
        Me.lblThen.Location = New System.Drawing.Point(4, 28)
        Me.lblThen.Name = "lblThen"
        Me.lblThen.Size = New System.Drawing.Size(35, 13)
        Me.lblThen.TabIndex = 6
        Me.lblThen.Text = "Then:"
        '
        'lblIf
        '
        Me.lblIf.AutoSize = True
        Me.lblIf.Location = New System.Drawing.Point(3, 0)
        Me.lblIf.Name = "lblIf"
        Me.lblIf.Size = New System.Drawing.Size(16, 13)
        Me.lblIf.TabIndex = 4
        Me.lblIf.Text = "If:"
        '
        'cmdExpand
        '
        Me.cmdExpand.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdExpand.Location = New System.Drawing.Point(332, 0)
        Me.cmdExpand.Name = "cmdExpand"
        Me.cmdExpand.Size = New System.Drawing.Size(23, 23)
        Me.cmdExpand.TabIndex = 8
        Me.cmdExpand.Text = "v"
        Me.cmdExpand.UseVisualStyleBackColor = True
        '
        'ctlThenScript
        '
        Me.ctlThenScript.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ctlThenScript.Controller = Nothing
        Me.ctlThenScript.Location = New System.Drawing.Point(46, 28)
        Me.ctlThenScript.Name = "ctlThenScript"
        Me.ctlThenScript.Size = New System.Drawing.Size(280, 306)
        Me.ctlThenScript.TabIndex = 7
        Me.ctlThenScript.Value = Nothing
        '
        'ctlExpression
        '
        Me.ctlExpression.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.ctlExpression.Controller = Nothing
        Me.ctlExpression.Location = New System.Drawing.Point(46, 0)
        Me.ctlExpression.Name = "ctlExpression"
        Me.ctlExpression.Size = New System.Drawing.Size(280, 20)
        Me.ctlExpression.TabIndex = 5
        Me.ctlExpression.Value = ""
        '
        'IfEditorChild
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.cmdExpand)
        Me.Controls.Add(Me.ctlThenScript)
        Me.Controls.Add(Me.lblThen)
        Me.Controls.Add(Me.ctlExpression)
        Me.Controls.Add(Me.lblIf)
        Me.Name = "IfEditorChild"
        Me.Size = New System.Drawing.Size(358, 337)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ctlThenScript As AxeSoftware.Quest.ScriptControl
    Friend WithEvents lblThen As System.Windows.Forms.Label
    Friend WithEvents ctlExpression As AxeSoftware.Quest.TextBoxControl
    Friend WithEvents lblIf As System.Windows.Forms.Label
    Friend WithEvents cmdExpand As System.Windows.Forms.Button

End Class
