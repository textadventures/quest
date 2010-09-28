<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Editor
    Inherits System.Windows.Forms.UserControl

    'UserControl1 overrides dispose to clean up the component list.
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
        Me.splitMain = New System.Windows.Forms.SplitContainer
        Me.ctlSaveFile = New System.Windows.Forms.SaveFileDialog
        Me.ctlTree = New AxeSoftware.Quest.EditorTree
        Me.ctlToolbar = New AxeSoftware.Quest.MainToolbar
        Me.splitMain.Panel1.SuspendLayout()
        Me.splitMain.SuspendLayout()
        Me.SuspendLayout()
        '
        'splitMain
        '
        Me.splitMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.splitMain.Location = New System.Drawing.Point(0, 25)
        Me.splitMain.Name = "splitMain"
        '
        'splitMain.Panel1
        '
        Me.splitMain.Panel1.Controls.Add(Me.ctlTree)
        Me.splitMain.Size = New System.Drawing.Size(618, 304)
        Me.splitMain.SplitterDistance = 206
        Me.splitMain.TabIndex = 0
        '
        'ctlSaveFile
        '
        Me.ctlSaveFile.DefaultExt = "aslx"
        Me.ctlSaveFile.Filter = "Quest Games|*.aslx|All files|*.*"
        '
        'ctlTree
        '
        Me.ctlTree.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlTree.Location = New System.Drawing.Point(0, 0)
        Me.ctlTree.Name = "ctlTree"
        Me.ctlTree.Size = New System.Drawing.Size(206, 304)
        Me.ctlTree.TabIndex = 0
        '
        'ctlToolbar
        '
        Me.ctlToolbar.Dock = System.Windows.Forms.DockStyle.Top
        Me.ctlToolbar.Location = New System.Drawing.Point(0, 0)
        Me.ctlToolbar.Name = "ctlToolbar"
        Me.ctlToolbar.Size = New System.Drawing.Size(618, 25)
        Me.ctlToolbar.TabIndex = 2
        '
        'Editor
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.splitMain)
        Me.Controls.Add(Me.ctlToolbar)
        Me.Name = "Editor"
        Me.Size = New System.Drawing.Size(618, 329)
        Me.splitMain.Panel1.ResumeLayout(False)
        Me.splitMain.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents splitMain As System.Windows.Forms.SplitContainer
    Friend WithEvents ctlTree As EditorTree
    Friend WithEvents ctlToolbar As AxeSoftware.Quest.MainToolbar
    Private WithEvents ctlSaveFile As System.Windows.Forms.SaveFileDialog

End Class
