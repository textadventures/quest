<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Menu
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
        Me.lblCaption = New System.Windows.Forms.Label
        Me.cmdSelect = New System.Windows.Forms.Button
        Me.cmdCancel = New System.Windows.Forms.Button
        Me.lstOptions = New System.Windows.Forms.ListView
        Me.colName = New System.Windows.Forms.ColumnHeader
        Me.SuspendLayout()
        '
        'lblCaption
        '
        Me.lblCaption.AutoSize = True
        Me.lblCaption.Location = New System.Drawing.Point(13, 13)
        Me.lblCaption.Name = "lblCaption"
        Me.lblCaption.Size = New System.Drawing.Size(28, 13)
        Me.lblCaption.TabIndex = 0
        Me.lblCaption.Text = "Text"
        '
        'cmdSelect
        '
        Me.cmdSelect.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdSelect.Location = New System.Drawing.Point(266, 253)
        Me.cmdSelect.Name = "cmdSelect"
        Me.cmdSelect.Size = New System.Drawing.Size(75, 23)
        Me.cmdSelect.TabIndex = 1
        Me.cmdSelect.Text = "Select"
        Me.cmdSelect.UseVisualStyleBackColor = True
        '
        'cmdCancel
        '
        Me.cmdCancel.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.cmdCancel.Location = New System.Drawing.Point(185, 253)
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.Size = New System.Drawing.Size(75, 23)
        Me.cmdCancel.TabIndex = 2
        Me.cmdCancel.Text = "Cancel"
        Me.cmdCancel.UseVisualStyleBackColor = True
        '
        'lstOptions
        '
        Me.lstOptions.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lstOptions.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colName})
        Me.lstOptions.HideSelection = False
        Me.lstOptions.Location = New System.Drawing.Point(16, 30)
        Me.lstOptions.Name = "lstOptions"
        Me.lstOptions.Size = New System.Drawing.Size(325, 216)
        Me.lstOptions.TabIndex = 0
        Me.lstOptions.UseCompatibleStateImageBehavior = False
        Me.lstOptions.View = System.Windows.Forms.View.List
        '
        'colName
        '
        Me.colName.Text = "Name"
        '
        'Menu
        '
        Me.AcceptButton = Me.cmdSelect
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CancelButton = Me.cmdCancel
        Me.ClientSize = New System.Drawing.Size(353, 284)
        Me.Controls.Add(Me.lstOptions)
        Me.Controls.Add(Me.cmdCancel)
        Me.Controls.Add(Me.cmdSelect)
        Me.Controls.Add(Me.lblCaption)
        Me.Name = "Menu"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Menu"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents colName As System.Windows.Forms.ColumnHeader
    Private WithEvents lblCaption As System.Windows.Forms.Label
    Private WithEvents cmdSelect As System.Windows.Forms.Button
    Private WithEvents cmdCancel As System.Windows.Forms.Button
    Private WithEvents lstOptions As System.Windows.Forms.ListView
End Class
