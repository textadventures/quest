<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class IfEditor
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
        Me.ctlSplitContainer = New System.Windows.Forms.SplitContainer()
        Me.ctlChild = New AxeSoftware.Quest.IfEditorChild()
        Me.ctlSplitContainer.Panel1.SuspendLayout()
        Me.ctlSplitContainer.SuspendLayout()
        Me.SuspendLayout()
        '
        'ctlSplitContainer
        '
        Me.ctlSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlSplitContainer.Location = New System.Drawing.Point(0, 0)
        Me.ctlSplitContainer.Name = "ctlSplitContainer"
        Me.ctlSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'ctlSplitContainer.Panel1
        '
        Me.ctlSplitContainer.Panel1.Controls.Add(Me.ctlChild)
        Me.ctlSplitContainer.Size = New System.Drawing.Size(414, 360)
        Me.ctlSplitContainer.SplitterDistance = 280
        Me.ctlSplitContainer.TabIndex = 4
        '
        'ctlChild
        '
        Me.ctlChild.Controller = Nothing
        Me.ctlChild.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlChild.Location = New System.Drawing.Point(0, 0)
        Me.ctlChild.Name = "ctlChild"
        Me.ctlChild.Size = New System.Drawing.Size(414, 280)
        Me.ctlChild.TabIndex = 0
        '
        'IfEditor
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.ctlSplitContainer)
        Me.Name = "IfEditor"
        Me.Size = New System.Drawing.Size(414, 360)
        Me.ctlSplitContainer.Panel1.ResumeLayout(False)
        Me.ctlSplitContainer.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ctlSplitContainer As System.Windows.Forms.SplitContainer
    Friend WithEvents ctlChild As AxeSoftware.Quest.IfEditorChild

End Class
