<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class BetaInfo
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
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.LinkLabel1 = New System.Windows.Forms.LinkLabel()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.LinkLabel2 = New System.Windows.Forms.LinkLabel()
        Me.pnlNewVersion = New System.Windows.Forms.Panel()
        Me.cmdDownload = New System.Windows.Forms.Button()
        Me.lblNewVersionDesc = New System.Windows.Forms.Label()
        Me.lblNewVersion = New System.Windows.Forms.Label()
        Me.pnlNewVersion.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.Red
        Me.Label1.Location = New System.Drawing.Point(4, 4)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(139, 16)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Under Construction"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(4, 31)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(218, 13)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "Please send all feedback and bug reports to:"
        '
        'LinkLabel1
        '
        Me.LinkLabel1.AutoSize = True
        Me.LinkLabel1.Location = New System.Drawing.Point(228, 31)
        Me.LinkLabel1.Name = "LinkLabel1"
        Me.LinkLabel1.Size = New System.Drawing.Size(89, 13)
        Me.LinkLabel1.TabIndex = 2
        Me.LinkLabel1.TabStop = True
        Me.LinkLabel1.Text = "alex@axeuk.com"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(4, 54)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(109, 13)
        Me.Label3.TabIndex = 3
        Me.Label3.Text = "Check for updates at:"
        '
        'LinkLabel2
        '
        Me.LinkLabel2.AutoSize = True
        Me.LinkLabel2.Location = New System.Drawing.Point(119, 54)
        Me.LinkLabel2.Name = "LinkLabel2"
        Me.LinkLabel2.Size = New System.Drawing.Size(84, 13)
        Me.LinkLabel2.TabIndex = 4
        Me.LinkLabel2.TabStop = True
        Me.LinkLabel2.Text = "www.quest5.net"
        '
        'pnlNewVersion
        '
        Me.pnlNewVersion.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pnlNewVersion.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.pnlNewVersion.Controls.Add(Me.cmdDownload)
        Me.pnlNewVersion.Controls.Add(Me.lblNewVersionDesc)
        Me.pnlNewVersion.Controls.Add(Me.lblNewVersion)
        Me.pnlNewVersion.Location = New System.Drawing.Point(417, 4)
        Me.pnlNewVersion.Name = "pnlNewVersion"
        Me.pnlNewVersion.Size = New System.Drawing.Size(250, 82)
        Me.pnlNewVersion.TabIndex = 5
        Me.pnlNewVersion.Visible = False
        '
        'cmdDownload
        '
        Me.cmdDownload.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdDownload.Location = New System.Drawing.Point(65, 49)
        Me.cmdDownload.Name = "cmdDownload"
        Me.cmdDownload.Size = New System.Drawing.Size(122, 24)
        Me.cmdDownload.TabIndex = 2
        Me.cmdDownload.Text = "Update Now"
        Me.cmdDownload.UseVisualStyleBackColor = True
        '
        'lblNewVersionDesc
        '
        Me.lblNewVersionDesc.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblNewVersionDesc.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblNewVersionDesc.Location = New System.Drawing.Point(4, 27)
        Me.lblNewVersionDesc.Name = "lblNewVersionDesc"
        Me.lblNewVersionDesc.Size = New System.Drawing.Size(243, 22)
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
        Me.lblNewVersion.Size = New System.Drawing.Size(243, 23)
        Me.lblNewVersion.TabIndex = 0
        Me.lblNewVersion.Text = "New version available"
        Me.lblNewVersion.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'BetaInfo
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.pnlNewVersion)
        Me.Controls.Add(Me.LinkLabel2)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.LinkLabel1)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Name = "BetaInfo"
        Me.Size = New System.Drawing.Size(670, 89)
        Me.pnlNewVersion.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents LinkLabel1 As System.Windows.Forms.LinkLabel
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents LinkLabel2 As System.Windows.Forms.LinkLabel
    Friend WithEvents pnlNewVersion As System.Windows.Forms.Panel
    Friend WithEvents lblNewVersion As System.Windows.Forms.Label
    Friend WithEvents lblNewVersionDesc As System.Windows.Forms.Label
    Friend WithEvents cmdDownload As System.Windows.Forms.Button

End Class
