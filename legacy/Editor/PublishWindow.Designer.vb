<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class PublishWindow
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(PublishWindow))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtFilename = New System.Windows.Forms.TextBox()
        Me.cmdBrowse = New System.Windows.Forms.Button()
        Me.chkIncludeWalkthrough = New System.Windows.Forms.CheckBox()
        Me.cmdCancel = New System.Windows.Forms.Button()
        Me.cmdSave = New System.Windows.Forms.Button()
        Me.ctlSaveDialog = New System.Windows.Forms.SaveFileDialog()
        Me.SuspendLayout()
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'txtFilename
        '
        resources.ApplyResources(Me.txtFilename, "txtFilename")
        Me.txtFilename.Name = "txtFilename"
        '
        'cmdBrowse
        '
        resources.ApplyResources(Me.cmdBrowse, "cmdBrowse")
        Me.cmdBrowse.Name = "cmdBrowse"
        Me.cmdBrowse.UseVisualStyleBackColor = True
        '
        'chkIncludeWalkthrough
        '
        resources.ApplyResources(Me.chkIncludeWalkthrough, "chkIncludeWalkthrough")
        Me.chkIncludeWalkthrough.Name = "chkIncludeWalkthrough"
        Me.chkIncludeWalkthrough.UseVisualStyleBackColor = True
        '
        'cmdCancel
        '
        resources.ApplyResources(Me.cmdCancel, "cmdCancel")
        Me.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.UseVisualStyleBackColor = True
        '
        'cmdSave
        '
        resources.ApplyResources(Me.cmdSave, "cmdSave")
        Me.cmdSave.Name = "cmdSave"
        Me.cmdSave.UseVisualStyleBackColor = True
        '
        'ctlSaveDialog
        '
        Me.ctlSaveDialog.DefaultExt = "quest"
        resources.ApplyResources(Me.ctlSaveDialog, "ctlSaveDialog")
        '
        'PublishWindow
        '
        Me.AcceptButton = Me.cmdSave
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.GhostWhite
        Me.CancelButton = Me.cmdCancel
        Me.Controls.Add(Me.cmdSave)
        Me.Controls.Add(Me.cmdCancel)
        Me.Controls.Add(Me.chkIncludeWalkthrough)
        Me.Controls.Add(Me.cmdBrowse)
        Me.Controls.Add(Me.txtFilename)
        Me.Controls.Add(Me.Label1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "PublishWindow"
        Me.ShowIcon = False
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents txtFilename As System.Windows.Forms.TextBox
    Friend WithEvents cmdBrowse As System.Windows.Forms.Button
    Friend WithEvents chkIncludeWalkthrough As System.Windows.Forms.CheckBox
    Friend WithEvents cmdCancel As System.Windows.Forms.Button
    Friend WithEvents cmdSave As System.Windows.Forms.Button
    Friend WithEvents ctlSaveDialog As System.Windows.Forms.SaveFileDialog
End Class
