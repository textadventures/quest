<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class VersionInfo
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
        Me.pnlNewVersion = New System.Windows.Forms.Panel()
        Me.cmdDownload = New System.Windows.Forms.Button()
        Me.lblNewVersionDesc = New System.Windows.Forms.Label()
        Me.lblNewVersion = New System.Windows.Forms.Label()
        Me.pnlNewVersion.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlNewVersion
        '
        Me.pnlNewVersion.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.pnlNewVersion.Controls.Add(Me.cmdDownload)
        Me.pnlNewVersion.Controls.Add(Me.lblNewVersionDesc)
        Me.pnlNewVersion.Controls.Add(Me.lblNewVersion)
        Me.pnlNewVersion.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlNewVersion.Location = New System.Drawing.Point(0, 0)
        Me.pnlNewVersion.Name = "pnlNewVersion"
        Me.pnlNewVersion.Size = New System.Drawing.Size(670, 89)
        Me.pnlNewVersion.TabIndex = 5
        '
        'cmdDownload
        '
        Me.cmdDownload.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdDownload.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdDownload.Location = New System.Drawing.Point(65, 49)
        Me.cmdDownload.Name = "cmdDownload"
        Me.cmdDownload.Size = New System.Drawing.Size(542, 24)
        Me.cmdDownload.TabIndex = 2
        Me.cmdDownload.Text = "Click here to update"
        Me.cmdDownload.UseVisualStyleBackColor = True
        '
        'lblNewVersionDesc
        '
        Me.lblNewVersionDesc.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblNewVersionDesc.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblNewVersionDesc.Location = New System.Drawing.Point(4, 27)
        Me.lblNewVersionDesc.Name = "lblNewVersionDesc"
        Me.lblNewVersionDesc.Size = New System.Drawing.Size(663, 22)
        Me.lblNewVersionDesc.TabIndex = 1
        Me.lblNewVersionDesc.Text = "Quest 5.x.x"
        Me.lblNewVersionDesc.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'lblNewVersion
        '
        Me.lblNewVersion.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblNewVersion.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblNewVersion.ForeColor = System.Drawing.Color.Green
        Me.lblNewVersion.Location = New System.Drawing.Point(4, 4)
        Me.lblNewVersion.Name = "lblNewVersion"
        Me.lblNewVersion.Size = New System.Drawing.Size(663, 23)
        Me.lblNewVersion.TabIndex = 0
        Me.lblNewVersion.Text = "New version available"
        Me.lblNewVersion.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'VersionInfo
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.pnlNewVersion)
        Me.Name = "VersionInfo"
        Me.Size = New System.Drawing.Size(670, 89)
        Me.pnlNewVersion.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents pnlNewVersion As System.Windows.Forms.Panel
    Friend WithEvents cmdDownload As System.Windows.Forms.Button
    Friend WithEvents lblNewVersionDesc As System.Windows.Forms.Label
    Friend WithEvents lblNewVersion As System.Windows.Forms.Label

End Class
