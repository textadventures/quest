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
        Me.picGamebook = New System.Windows.Forms.PictureBox()
        Me.picTextAdventure = New System.Windows.Forms.PictureBox()
        Me.picTextAdventureBorder = New System.Windows.Forms.PictureBox()
        Me.picGamebookBorder = New System.Windows.Forms.PictureBox()
        CType(Me.picGamebook, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.picTextAdventure, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.picTextAdventureBorder, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.picGamebookBorder, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'lstTemplate
        '
        resources.ApplyResources(Me.lstTemplate, "lstTemplate")
        Me.lstTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.lstTemplate.FormattingEnabled = True
        Me.lstTemplate.Name = "lstTemplate"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'txtGameName
        '
        resources.ApplyResources(Me.txtGameName, "txtGameName")
        Me.txtGameName.Name = "txtGameName"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'txtFilename
        '
        resources.ApplyResources(Me.txtFilename, "txtFilename")
        Me.txtFilename.Name = "txtFilename"
        Me.txtFilename.ReadOnly = True
        '
        'cmdOK
        '
        resources.ApplyResources(Me.cmdOK, "cmdOK")
        Me.cmdOK.Name = "cmdOK"
        Me.cmdOK.UseVisualStyleBackColor = True
        '
        'cmdCancel
        '
        resources.ApplyResources(Me.cmdCancel, "cmdCancel")
        Me.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.UseVisualStyleBackColor = True
        '
        'cmdBrowse
        '
        resources.ApplyResources(Me.cmdBrowse, "cmdBrowse")
        Me.cmdBrowse.Name = "cmdBrowse"
        Me.cmdBrowse.UseVisualStyleBackColor = True
        '
        'ctlSaveDialog
        '
        Me.ctlSaveDialog.DefaultExt = "aslx"
        resources.ApplyResources(Me.ctlSaveDialog, "ctlSaveDialog")
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'optTextAdventure
        '
        resources.ApplyResources(Me.optTextAdventure, "optTextAdventure")
        Me.optTextAdventure.Checked = True
        Me.optTextAdventure.Name = "optTextAdventure"
        Me.optTextAdventure.TabStop = True
        Me.optTextAdventure.UseVisualStyleBackColor = True
        '
        'optGamebook
        '
        resources.ApplyResources(Me.optGamebook, "optGamebook")
        Me.optGamebook.Name = "optGamebook"
        Me.optGamebook.UseVisualStyleBackColor = True
        '
        'picGamebook
        '
        resources.ApplyResources(Me.picGamebook, "picGamebook")
        Me.picGamebook.BackColor = System.Drawing.Color.White
        Me.picGamebook.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.picGamebook.Name = "picGamebook"
        Me.picGamebook.TabStop = False
        '
        'picTextAdventure
        '
        resources.ApplyResources(Me.picTextAdventure, "picTextAdventure")
        Me.picTextAdventure.BackColor = System.Drawing.Color.White
        Me.picTextAdventure.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.picTextAdventure.Name = "picTextAdventure"
        Me.picTextAdventure.TabStop = False
        '
        'picTextAdventureBorder
        '
        resources.ApplyResources(Me.picTextAdventureBorder, "picTextAdventureBorder")
        Me.picTextAdventureBorder.BackColor = System.Drawing.Color.White
        Me.picTextAdventureBorder.Name = "picTextAdventureBorder"
        Me.picTextAdventureBorder.TabStop = False
        '
        'picGamebookBorder
        '
        resources.ApplyResources(Me.picGamebookBorder, "picGamebookBorder")
        Me.picGamebookBorder.BackColor = System.Drawing.Color.White
        Me.picGamebookBorder.Name = "picGamebookBorder"
        Me.picGamebookBorder.TabStop = False
        '
        'NewGameWindow
        '
        Me.AcceptButton = Me.cmdOK
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.GhostWhite
        Me.CancelButton = Me.cmdCancel
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
        Me.MinimizeBox = False
        Me.Name = "NewGameWindow"
        Me.ShowIcon = False
        CType(Me.picGamebook, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.picTextAdventure, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.picTextAdventureBorder, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.picGamebookBorder, System.ComponentModel.ISupportInitialize).EndInit()
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
    Friend WithEvents picTextAdventure As System.Windows.Forms.PictureBox
    Friend WithEvents picGamebook As System.Windows.Forms.PictureBox
    Friend WithEvents picTextAdventureBorder As PictureBox
    Friend WithEvents picGamebookBorder As PictureBox
End Class
