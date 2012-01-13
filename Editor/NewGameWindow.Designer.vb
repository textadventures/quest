<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class NewGameWindow
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(NewGameWindow))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lstTemplate = New System.Windows.Forms.ComboBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtGameName = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtFilename = New System.Windows.Forms.TextBox()
        Me.cmdOK = New System.Windows.Forms.Button()
        Me.cmdCancel = New System.Windows.Forms.Button()
        Me.cmdBrowse = New System.Windows.Forms.Button()
        Me.ctlSaveDialog = New System.Windows.Forms.SaveFileDialog()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.optTextAdventure = New System.Windows.Forms.RadioButton()
        Me.optGamebook = New System.Windows.Forms.RadioButton()
        Me.picTextAdventureBorder = New System.Windows.Forms.PictureBox()
        Me.picGamebookBorder = New System.Windows.Forms.PictureBox()
        Me.picTextAdventure = New System.Windows.Forms.PictureBox()
        Me.picGamebook = New System.Windows.Forms.PictureBox()
        CType(Me.picTextAdventureBorder, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.picGamebookBorder, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.picTextAdventure, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.picGamebook, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(104, 136)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(58, 13)
        Me.Label1.TabIndex = 4
        Me.Label1.Text = "&Language:"
        '
        'lstTemplate
        '
        Me.lstTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.lstTemplate.FormattingEnabled = True
        Me.lstTemplate.Location = New System.Drawing.Point(107, 152)
        Me.lstTemplate.Name = "lstTemplate"
        Me.lstTemplate.Size = New System.Drawing.Size(182, 21)
        Me.lstTemplate.TabIndex = 5
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(16, 209)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(38, 13)
        Me.Label2.TabIndex = 6
        Me.Label2.Text = "&Name:"
        '
        'txtGameName
        '
        Me.txtGameName.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtGameName.Location = New System.Drawing.Point(77, 206)
        Me.txtGameName.Name = "txtGameName"
        Me.txtGameName.Size = New System.Drawing.Size(484, 20)
        Me.txtGameName.TabIndex = 7
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(16, 240)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(55, 13)
        Me.Label3.TabIndex = 8
        Me.Label3.Text = "File name:"
        '
        'txtFilename
        '
        Me.txtFilename.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtFilename.Location = New System.Drawing.Point(77, 237)
        Me.txtFilename.Name = "txtFilename"
        Me.txtFilename.ReadOnly = True
        Me.txtFilename.Size = New System.Drawing.Size(403, 20)
        Me.txtFilename.TabIndex = 9
        '
        'cmdOK
        '
        Me.cmdOK.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdOK.Enabled = False
        Me.cmdOK.Location = New System.Drawing.Point(405, 273)
        Me.cmdOK.Name = "cmdOK"
        Me.cmdOK.Size = New System.Drawing.Size(75, 23)
        Me.cmdOK.TabIndex = 11
        Me.cmdOK.Text = "OK"
        Me.cmdOK.UseVisualStyleBackColor = True
        '
        'cmdCancel
        '
        Me.cmdCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.cmdCancel.Location = New System.Drawing.Point(486, 273)
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.Size = New System.Drawing.Size(75, 23)
        Me.cmdCancel.TabIndex = 12
        Me.cmdCancel.Text = "Cancel"
        Me.cmdCancel.UseVisualStyleBackColor = True
        '
        'cmdBrowse
        '
        Me.cmdBrowse.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdBrowse.Location = New System.Drawing.Point(486, 235)
        Me.cmdBrowse.Name = "cmdBrowse"
        Me.cmdBrowse.Size = New System.Drawing.Size(75, 23)
        Me.cmdBrowse.TabIndex = 10
        Me.cmdBrowse.Text = "&Browse..."
        Me.cmdBrowse.UseVisualStyleBackColor = True
        '
        'ctlSaveDialog
        '
        Me.ctlSaveDialog.DefaultExt = "aslx"
        Me.ctlSaveDialog.Filter = "Quest Games|*.aslx"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(15, 13)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(61, 13)
        Me.Label4.TabIndex = 1
        Me.Label4.Text = "Game type:"
        '
        'optTextAdventure
        '
        Me.optTextAdventure.AutoSize = True
        Me.optTextAdventure.Checked = True
        Me.optTextAdventure.Location = New System.Drawing.Point(82, 11)
        Me.optTextAdventure.Name = "optTextAdventure"
        Me.optTextAdventure.Size = New System.Drawing.Size(97, 17)
        Me.optTextAdventure.TabIndex = 2
        Me.optTextAdventure.TabStop = True
        Me.optTextAdventure.Text = "&Text adventure"
        Me.optTextAdventure.UseVisualStyleBackColor = True
        '
        'optGamebook
        '
        Me.optGamebook.AutoSize = True
        Me.optGamebook.Location = New System.Drawing.Point(324, 11)
        Me.optGamebook.Name = "optGamebook"
        Me.optGamebook.Size = New System.Drawing.Size(77, 17)
        Me.optGamebook.TabIndex = 3
        Me.optGamebook.Text = "&Gamebook"
        Me.optGamebook.UseVisualStyleBackColor = True
        '
        'picTextAdventureBorder
        '
        Me.picTextAdventureBorder.BackColor = System.Drawing.Color.White
        Me.picTextAdventureBorder.Location = New System.Drawing.Point(107, 34)
        Me.picTextAdventureBorder.Name = "picTextAdventureBorder"
        Me.picTextAdventureBorder.Size = New System.Drawing.Size(182, 89)
        Me.picTextAdventureBorder.TabIndex = 11
        Me.picTextAdventureBorder.TabStop = False
        '
        'picGamebookBorder
        '
        Me.picGamebookBorder.BackColor = System.Drawing.Color.White
        Me.picGamebookBorder.Location = New System.Drawing.Point(346, 34)
        Me.picGamebookBorder.Name = "picGamebookBorder"
        Me.picGamebookBorder.Size = New System.Drawing.Size(182, 89)
        Me.picGamebookBorder.TabIndex = 12
        Me.picGamebookBorder.TabStop = False
        '
        'picTextAdventure
        '
        Me.picTextAdventure.BackColor = System.Drawing.Color.White
        Me.picTextAdventure.Image = CType(resources.GetObject("picTextAdventure.Image"), System.Drawing.Image)
        Me.picTextAdventure.Location = New System.Drawing.Point(117, 44)
        Me.picTextAdventure.Name = "picTextAdventure"
        Me.picTextAdventure.Size = New System.Drawing.Size(122, 50)
        Me.picTextAdventure.TabIndex = 13
        Me.picTextAdventure.TabStop = False
        '
        'picGamebook
        '
        Me.picGamebook.BackColor = System.Drawing.Color.White
        Me.picGamebook.Image = CType(resources.GetObject("picGamebook.Image"), System.Drawing.Image)
        Me.picGamebook.Location = New System.Drawing.Point(360, 44)
        Me.picGamebook.Name = "picGamebook"
        Me.picGamebook.Size = New System.Drawing.Size(142, 74)
        Me.picGamebook.TabIndex = 14
        Me.picGamebook.TabStop = False
        '
        'NewGameWindow
        '
        Me.AcceptButton = Me.cmdOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.cmdCancel
        Me.ClientSize = New System.Drawing.Size(573, 308)
        Me.Controls.Add(Me.picGamebook)
        Me.Controls.Add(Me.picTextAdventure)
        Me.Controls.Add(Me.picGamebookBorder)
        Me.Controls.Add(Me.picTextAdventureBorder)
        Me.Controls.Add(Me.optGamebook)
        Me.Controls.Add(Me.optTextAdventure)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.cmdBrowse)
        Me.Controls.Add(Me.cmdCancel)
        Me.Controls.Add(Me.cmdOK)
        Me.Controls.Add(Me.txtFilename)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.txtGameName)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.lstTemplate)
        Me.Controls.Add(Me.Label1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(1200, 400)
        Me.MinimizeBox = False
        Me.MinimumSize = New System.Drawing.Size(400, 190)
        Me.Name = "NewGameWindow"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Create New Game"
        CType(Me.picTextAdventureBorder, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.picGamebookBorder, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.picTextAdventure, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.picGamebook, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lstTemplate As System.Windows.Forms.ComboBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtGameName As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents txtFilename As System.Windows.Forms.TextBox
    Friend WithEvents cmdOK As System.Windows.Forms.Button
    Friend WithEvents cmdCancel As System.Windows.Forms.Button
    Friend WithEvents cmdBrowse As System.Windows.Forms.Button
    Friend WithEvents ctlSaveDialog As System.Windows.Forms.SaveFileDialog
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents optTextAdventure As System.Windows.Forms.RadioButton
    Friend WithEvents optGamebook As System.Windows.Forms.RadioButton
    Friend WithEvents picTextAdventureBorder As System.Windows.Forms.PictureBox
    Friend WithEvents picGamebookBorder As System.Windows.Forms.PictureBox
    Friend WithEvents picTextAdventure As System.Windows.Forms.PictureBox
    Friend WithEvents picGamebook As System.Windows.Forms.PictureBox
End Class
