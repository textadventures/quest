<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class InputWindow
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
        Me.lblPrompt = New System.Windows.Forms.Label()
        Me.cmdOK = New System.Windows.Forms.Button()
        Me.cmdCancel = New System.Windows.Forms.Button()
        Me.txtInput = New System.Windows.Forms.TextBox()
        Me.lblDropdownCaption = New System.Windows.Forms.Label()
        Me.lstDropdown = New System.Windows.Forms.ComboBox()
        Me.lstInputAutoComplete = New System.Windows.Forms.ComboBox()
        Me.SuspendLayout()
        '
        'lblPrompt
        '
        Me.lblPrompt.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblPrompt.Location = New System.Drawing.Point(13, 13)
        Me.lblPrompt.Name = "lblPrompt"
        Me.lblPrompt.Size = New System.Drawing.Size(327, 40)
        Me.lblPrompt.TabIndex = 0
        Me.lblPrompt.Text = "Prompt"
        '
        'cmdOK
        '
        Me.cmdOK.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdOK.Location = New System.Drawing.Point(184, 64)
        Me.cmdOK.Name = "cmdOK"
        Me.cmdOK.Size = New System.Drawing.Size(75, 23)
        Me.cmdOK.TabIndex = 2
        Me.cmdOK.Text = "OK"
        Me.cmdOK.UseVisualStyleBackColor = True
        '
        'cmdCancel
        '
        Me.cmdCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.cmdCancel.Location = New System.Drawing.Point(265, 64)
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.Size = New System.Drawing.Size(75, 23)
        Me.cmdCancel.TabIndex = 3
        Me.cmdCancel.Text = "Cancel"
        Me.cmdCancel.UseVisualStyleBackColor = True
        '
        'txtInput
        '
        Me.txtInput.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtInput.Location = New System.Drawing.Point(16, 35)
        Me.txtInput.Name = "txtInput"
        Me.txtInput.Size = New System.Drawing.Size(324, 20)
        Me.txtInput.TabIndex = 0
        '
        'lblDropdownCaption
        '
        Me.lblDropdownCaption.AutoSize = True
        Me.lblDropdownCaption.Location = New System.Drawing.Point(13, 67)
        Me.lblDropdownCaption.Name = "lblDropdownCaption"
        Me.lblDropdownCaption.Size = New System.Drawing.Size(56, 13)
        Me.lblDropdownCaption.TabIndex = 3
        Me.lblDropdownCaption.Text = "Dropdown"
        '
        'lstDropdown
        '
        Me.lstDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.lstDropdown.FormattingEnabled = True
        Me.lstDropdown.Location = New System.Drawing.Point(76, 64)
        Me.lstDropdown.Name = "lstDropdown"
        Me.lstDropdown.Size = New System.Drawing.Size(264, 21)
        Me.lstDropdown.TabIndex = 1
        '
        'lstInputAutoComplete
        '
        Me.lstInputAutoComplete.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lstInputAutoComplete.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest
        Me.lstInputAutoComplete.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems
        Me.lstInputAutoComplete.FormattingEnabled = True
        Me.lstInputAutoComplete.Location = New System.Drawing.Point(16, 35)
        Me.lstInputAutoComplete.Name = "lstInputAutoComplete"
        Me.lstInputAutoComplete.Size = New System.Drawing.Size(324, 21)
        Me.lstInputAutoComplete.TabIndex = 0
        Me.lstInputAutoComplete.Visible = False
        '
        'InputWindow
        '
        Me.AcceptButton = Me.cmdOK
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.cmdCancel
        Me.ClientSize = New System.Drawing.Size(352, 99)
        Me.Controls.Add(Me.lstInputAutoComplete)
        Me.Controls.Add(Me.lblDropdownCaption)
        Me.Controls.Add(Me.txtInput)
        Me.Controls.Add(Me.cmdCancel)
        Me.Controls.Add(Me.cmdOK)
        Me.Controls.Add(Me.lblPrompt)
        Me.Controls.Add(Me.lstDropdown)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "InputWindow"
        Me.ShowIcon = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Quest"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblPrompt As System.Windows.Forms.Label
    Friend WithEvents cmdOK As System.Windows.Forms.Button
    Friend WithEvents cmdCancel As System.Windows.Forms.Button
    Friend WithEvents txtInput As System.Windows.Forms.TextBox
    Friend WithEvents lblDropdownCaption As System.Windows.Forms.Label
    Friend WithEvents lstDropdown As System.Windows.Forms.ComboBox
    Friend WithEvents lstInputAutoComplete As System.Windows.Forms.ComboBox
End Class
