<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class AttributesControl
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
        Me.ctlSplitContainer = New System.Windows.Forms.SplitContainer()
        Me.lstAttributes = New System.Windows.Forms.ListView()
        Me.colName = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colValue = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ctlMultiControl = New AxeSoftware.Quest.MultiControl()
        CType(Me.ctlSplitContainer, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ctlSplitContainer.Panel1.SuspendLayout()
        Me.ctlSplitContainer.Panel2.SuspendLayout()
        Me.ctlSplitContainer.SuspendLayout()
        Me.SuspendLayout()
        '
        'ctlSplitContainer
        '
        Me.ctlSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlSplitContainer.Location = New System.Drawing.Point(0, 0)
        Me.ctlSplitContainer.Name = "ctlSplitContainer"
        Me.ctlSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'ctlSplitContainer.Panel1
        '
        Me.ctlSplitContainer.Panel1.Controls.Add(Me.lstAttributes)
        '
        'ctlSplitContainer.Panel2
        '
        Me.ctlSplitContainer.Panel2.Controls.Add(Me.ctlMultiControl)
        Me.ctlSplitContainer.Size = New System.Drawing.Size(657, 456)
        Me.ctlSplitContainer.SplitterDistance = 219
        Me.ctlSplitContainer.TabIndex = 0
        '
        'lstAttributes
        '
        Me.lstAttributes.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colName, Me.colValue})
        Me.lstAttributes.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstAttributes.FullRowSelect = True
        Me.lstAttributes.HideSelection = False
        Me.lstAttributes.Location = New System.Drawing.Point(0, 0)
        Me.lstAttributes.Name = "lstAttributes"
        Me.lstAttributes.Size = New System.Drawing.Size(657, 219)
        Me.lstAttributes.TabIndex = 0
        Me.lstAttributes.UseCompatibleStateImageBehavior = False
        Me.lstAttributes.View = System.Windows.Forms.View.Details
        '
        'colName
        '
        Me.colName.Text = "Name"
        Me.colName.Width = 138
        '
        'colValue
        '
        Me.colValue.Text = "Value"
        Me.colValue.Width = 354
        '
        'ctlMultiControl
        '
        Me.ctlMultiControl.Controller = Nothing
        Me.ctlMultiControl.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlMultiControl.Location = New System.Drawing.Point(0, 0)
        Me.ctlMultiControl.Name = "ctlMultiControl"
        Me.ctlMultiControl.Padding = New System.Windows.Forms.Padding(10)
        Me.ctlMultiControl.Size = New System.Drawing.Size(657, 233)
        Me.ctlMultiControl.TabIndex = 0
        Me.ctlMultiControl.Value = Nothing
        '
        'AttributesControl
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.Control
        Me.Controls.Add(Me.ctlSplitContainer)
        Me.Name = "AttributesControl"
        Me.Size = New System.Drawing.Size(657, 456)
        Me.ctlSplitContainer.Panel1.ResumeLayout(False)
        Me.ctlSplitContainer.Panel2.ResumeLayout(False)
        CType(Me.ctlSplitContainer, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ctlSplitContainer.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ctlSplitContainer As System.Windows.Forms.SplitContainer
    Friend WithEvents lstAttributes As System.Windows.Forms.ListView
    Friend WithEvents colName As System.Windows.Forms.ColumnHeader
    Friend WithEvents colValue As System.Windows.Forms.ColumnHeader
    Friend WithEvents ctlMultiControl As AxeSoftware.Quest.MultiControl

End Class
