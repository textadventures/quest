<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ElementList
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
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.tlbToolbar = New System.Windows.Forms.ToolStrip()
        Me.ctlSplitContainer = New System.Windows.Forms.SplitContainer()
        Me.lstList = New System.Windows.Forms.ListView()
        Me.colItems = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.txtStatus = New System.Windows.Forms.TextBox()
        CType(Me.ctlSplitContainer, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ctlSplitContainer.Panel1.SuspendLayout()
        Me.ctlSplitContainer.Panel2.SuspendLayout()
        Me.ctlSplitContainer.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblTitle
        '
        Me.lblTitle.BackColor = System.Drawing.SystemColors.InactiveCaption
        Me.lblTitle.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblTitle.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTitle.ForeColor = System.Drawing.SystemColors.InactiveCaptionText
        Me.lblTitle.Location = New System.Drawing.Point(0, 0)
        Me.lblTitle.Margin = New System.Windows.Forms.Padding(0)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Padding = New System.Windows.Forms.Padding(4, 2, 0, 2)
        Me.lblTitle.Size = New System.Drawing.Size(360, 20)
        Me.lblTitle.TabIndex = 8
        Me.lblTitle.Text = "Title"
        '
        'tlbToolbar
        '
        Me.tlbToolbar.Location = New System.Drawing.Point(0, 20)
        Me.tlbToolbar.Name = "tlbToolbar"
        Me.tlbToolbar.Size = New System.Drawing.Size(360, 25)
        Me.tlbToolbar.TabIndex = 11
        Me.tlbToolbar.Text = "ToolStrip1"
        '
        'ctlSplitContainer
        '
        Me.ctlSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlSplitContainer.Location = New System.Drawing.Point(0, 45)
        Me.ctlSplitContainer.Name = "ctlSplitContainer"
        Me.ctlSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'ctlSplitContainer.Panel1
        '
        Me.ctlSplitContainer.Panel1.Controls.Add(Me.lstList)
        '
        'ctlSplitContainer.Panel2
        '
        Me.ctlSplitContainer.Panel2.Controls.Add(Me.txtStatus)
        Me.ctlSplitContainer.Panel2Collapsed = True
        Me.ctlSplitContainer.Size = New System.Drawing.Size(360, 238)
        Me.ctlSplitContainer.SplitterDistance = 120
        Me.ctlSplitContainer.TabIndex = 12
        '
        'lstList
        '
        Me.lstList.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colItems})
        Me.lstList.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstList.HideSelection = False
        Me.lstList.Location = New System.Drawing.Point(0, 0)
        Me.lstList.Margin = New System.Windows.Forms.Padding(0)
        Me.lstList.MultiSelect = False
        Me.lstList.Name = "lstList"
        Me.lstList.Size = New System.Drawing.Size(360, 238)
        Me.lstList.TabIndex = 11
        Me.lstList.UseCompatibleStateImageBehavior = False
        Me.lstList.View = System.Windows.Forms.View.List
        '
        'colItems
        '
        Me.colItems.Text = "Items"
        Me.colItems.Width = 214
        '
        'txtStatus
        '
        Me.txtStatus.BackColor = System.Drawing.SystemColors.Window
        Me.txtStatus.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtStatus.Location = New System.Drawing.Point(0, 0)
        Me.txtStatus.Multiline = True
        Me.txtStatus.Name = "txtStatus"
        Me.txtStatus.ReadOnly = True
        Me.txtStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtStatus.Size = New System.Drawing.Size(150, 46)
        Me.txtStatus.TabIndex = 0
        '
        'ElementList
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.ctlSplitContainer)
        Me.Controls.Add(Me.tlbToolbar)
        Me.Controls.Add(Me.lblTitle)
        Me.Name = "ElementList"
        Me.Size = New System.Drawing.Size(360, 283)
        Me.ctlSplitContainer.Panel1.ResumeLayout(False)
        Me.ctlSplitContainer.Panel2.ResumeLayout(False)
        Me.ctlSplitContainer.Panel2.PerformLayout()
        CType(Me.ctlSplitContainer, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ctlSplitContainer.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Friend WithEvents tlbToolbar As System.Windows.Forms.ToolStrip
    Friend WithEvents ctlSplitContainer As System.Windows.Forms.SplitContainer
    Friend WithEvents lstList As System.Windows.Forms.ListView
    Friend WithEvents colItems As System.Windows.Forms.ColumnHeader
    Friend WithEvents txtStatus As System.Windows.Forms.TextBox

End Class
