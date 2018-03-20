<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ErrorHandler
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ErrorHandler))
        Me.lblInfo = New System.Windows.Forms.Label()
        Me.txtError = New System.Windows.Forms.TextBox()
        Me.cmdClose = New System.Windows.Forms.Button()
        Me.cmdReport = New System.Windows.Forms.Button()
        Me.cmdCopy = New System.Windows.Forms.Button()
        Me.lblHelp = New System.Windows.Forms.Label()
        Me.lblIssueTracker = New System.Windows.Forms.LinkLabel()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblInfo
        '
        resources.ApplyResources(Me.lblInfo, "lblInfo")
        Me.lblInfo.Name = "lblInfo"
        '
        'txtError
        '
        resources.ApplyResources(Me.txtError, "txtError")
        Me.txtError.Name = "txtError"
        Me.txtError.ReadOnly = True
        '
        'cmdClose
        '
        resources.ApplyResources(Me.cmdClose, "cmdClose")
        Me.cmdClose.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.cmdClose.Name = "cmdClose"
        Me.cmdClose.UseVisualStyleBackColor = True
        '
        'cmdReport
        '
        resources.ApplyResources(Me.cmdReport, "cmdReport")
        Me.cmdReport.Name = "cmdReport"
        Me.cmdReport.UseVisualStyleBackColor = True
        '
        'cmdCopy
        '
        resources.ApplyResources(Me.cmdCopy, "cmdCopy")
        Me.cmdCopy.Name = "cmdCopy"
        Me.cmdCopy.UseVisualStyleBackColor = True
        '
        'lblHelp
        '
        resources.ApplyResources(Me.lblHelp, "lblHelp")
        Me.lblHelp.Name = "lblHelp"
        '
        'lblIssueTracker
        '
        resources.ApplyResources(Me.lblIssueTracker, "lblIssueTracker")
        Me.lblIssueTracker.Name = "lblIssueTracker"
        Me.lblIssueTracker.TabStop = True
        '
        'PictureBox1
        '
        resources.ApplyResources(Me.PictureBox1, "PictureBox1")
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.TabStop = False
        '
        'ErrorHandler
        '
        Me.AcceptButton = Me.cmdReport
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.GhostWhite
        Me.CancelButton = Me.cmdClose
        Me.Controls.Add(Me.lblIssueTracker)
        Me.Controls.Add(Me.lblHelp)
        Me.Controls.Add(Me.cmdCopy)
        Me.Controls.Add(Me.cmdReport)
        Me.Controls.Add(Me.cmdClose)
        Me.Controls.Add(Me.txtError)
        Me.Controls.Add(Me.PictureBox1)
        Me.Controls.Add(Me.lblInfo)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "ErrorHandler"
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblInfo As System.Windows.Forms.Label
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents txtError As System.Windows.Forms.TextBox
    Friend WithEvents cmdClose As System.Windows.Forms.Button
    Friend WithEvents cmdReport As System.Windows.Forms.Button
    Friend WithEvents cmdCopy As System.Windows.Forms.Button
    Friend WithEvents lblHelp As System.Windows.Forms.Label
    Friend WithEvents lblIssueTracker As System.Windows.Forms.LinkLabel
End Class
