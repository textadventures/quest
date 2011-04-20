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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(AttributesControl))
        Me.ctlSplitContainerMain = New System.Windows.Forms.SplitContainer()
        Me.ctlSplitContainer = New System.Windows.Forms.SplitContainer()
        Me.lstAttributes = New System.Windows.Forms.ListView()
        Me.colName = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colValue = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colSource = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ctlToolStrip = New System.Windows.Forms.ToolStrip()
        Me.cmdAdd = New System.Windows.Forms.ToolStripButton()
        Me.cmdDelete = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripLabel1 = New System.Windows.Forms.ToolStripLabel()
        Me.ctlTypesToolStrip = New System.Windows.Forms.ToolStrip()
        Me.ToolStripLabel2 = New System.Windows.Forms.ToolStripLabel()
        Me.ToolStripButton1 = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton2 = New System.Windows.Forms.ToolStripButton()
        Me.ctlMultiControl = New AxeSoftware.Quest.MultiControl()
        Me.lstTypes = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        CType(Me.ctlSplitContainerMain, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ctlSplitContainerMain.Panel1.SuspendLayout()
        Me.ctlSplitContainerMain.Panel2.SuspendLayout()
        Me.ctlSplitContainerMain.SuspendLayout()
        CType(Me.ctlSplitContainer, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ctlSplitContainer.Panel1.SuspendLayout()
        Me.ctlSplitContainer.Panel2.SuspendLayout()
        Me.ctlSplitContainer.SuspendLayout()
        Me.ctlToolStrip.SuspendLayout()
        Me.ctlTypesToolStrip.SuspendLayout()
        Me.SuspendLayout()
        '
        'ctlSplitContainerMain
        '
        Me.ctlSplitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlSplitContainerMain.Location = New System.Drawing.Point(0, 0)
        Me.ctlSplitContainerMain.Name = "ctlSplitContainerMain"
        Me.ctlSplitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal
        '
        'ctlSplitContainerMain.Panel1
        '
        Me.ctlSplitContainerMain.Panel1.Controls.Add(Me.lstTypes)
        Me.ctlSplitContainerMain.Panel1.Controls.Add(Me.ctlTypesToolStrip)
        '
        'ctlSplitContainerMain.Panel2
        '
        Me.ctlSplitContainerMain.Panel2.Controls.Add(Me.ctlSplitContainer)
        Me.ctlSplitContainerMain.Size = New System.Drawing.Size(657, 456)
        Me.ctlSplitContainerMain.SplitterDistance = 106
        Me.ctlSplitContainerMain.TabIndex = 1
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
        Me.ctlSplitContainer.Panel1.Controls.Add(Me.ctlToolStrip)
        '
        'ctlSplitContainer.Panel2
        '
        Me.ctlSplitContainer.Panel2.Controls.Add(Me.ctlMultiControl)
        Me.ctlSplitContainer.Size = New System.Drawing.Size(657, 346)
        Me.ctlSplitContainer.SplitterDistance = 165
        Me.ctlSplitContainer.TabIndex = 1
        '
        'lstAttributes
        '
        Me.lstAttributes.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colName, Me.colValue, Me.colSource})
        Me.lstAttributes.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstAttributes.FullRowSelect = True
        Me.lstAttributes.HideSelection = False
        Me.lstAttributes.Location = New System.Drawing.Point(0, 25)
        Me.lstAttributes.MultiSelect = False
        Me.lstAttributes.Name = "lstAttributes"
        Me.lstAttributes.Size = New System.Drawing.Size(657, 140)
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
        'colSource
        '
        Me.colSource.Text = "Source"
        Me.colSource.Width = 115
        '
        'ctlToolStrip
        '
        Me.ctlToolStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripLabel1, Me.cmdAdd, Me.cmdDelete})
        Me.ctlToolStrip.Location = New System.Drawing.Point(0, 0)
        Me.ctlToolStrip.Name = "ctlToolStrip"
        Me.ctlToolStrip.Size = New System.Drawing.Size(657, 25)
        Me.ctlToolStrip.TabIndex = 3
        Me.ctlToolStrip.Text = "ToolStrip1"
        '
        'cmdAdd
        '
        Me.cmdAdd.Image = CType(resources.GetObject("cmdAdd.Image"), System.Drawing.Image)
        Me.cmdAdd.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.cmdAdd.Name = "cmdAdd"
        Me.cmdAdd.Size = New System.Drawing.Size(49, 22)
        Me.cmdAdd.Text = "Add"
        '
        'cmdDelete
        '
        Me.cmdDelete.Image = CType(resources.GetObject("cmdDelete.Image"), System.Drawing.Image)
        Me.cmdDelete.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.cmdDelete.Name = "cmdDelete"
        Me.cmdDelete.Size = New System.Drawing.Size(60, 22)
        Me.cmdDelete.Text = "Delete"
        '
        'ToolStripLabel1
        '
        Me.ToolStripLabel1.AutoSize = False
        Me.ToolStripLabel1.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ToolStripLabel1.Name = "ToolStripLabel1"
        Me.ToolStripLabel1.Size = New System.Drawing.Size(100, 22)
        Me.ToolStripLabel1.Text = "Attributes"
        Me.ToolStripLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'ctlTypesToolStrip
        '
        Me.ctlTypesToolStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripLabel2, Me.ToolStripButton1, Me.ToolStripButton2})
        Me.ctlTypesToolStrip.Location = New System.Drawing.Point(0, 0)
        Me.ctlTypesToolStrip.Name = "ctlTypesToolStrip"
        Me.ctlTypesToolStrip.Size = New System.Drawing.Size(657, 25)
        Me.ctlTypesToolStrip.TabIndex = 4
        Me.ctlTypesToolStrip.Text = "ToolStrip1"
        '
        'ToolStripLabel2
        '
        Me.ToolStripLabel2.AutoSize = False
        Me.ToolStripLabel2.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ToolStripLabel2.Name = "ToolStripLabel2"
        Me.ToolStripLabel2.Size = New System.Drawing.Size(100, 22)
        Me.ToolStripLabel2.Text = "Inherited Types"
        Me.ToolStripLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'ToolStripButton1
        '
        Me.ToolStripButton1.Image = CType(resources.GetObject("ToolStripButton1.Image"), System.Drawing.Image)
        Me.ToolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton1.Name = "ToolStripButton1"
        Me.ToolStripButton1.Size = New System.Drawing.Size(49, 22)
        Me.ToolStripButton1.Text = "Add"
        '
        'ToolStripButton2
        '
        Me.ToolStripButton2.Image = CType(resources.GetObject("ToolStripButton2.Image"), System.Drawing.Image)
        Me.ToolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButton2.Name = "ToolStripButton2"
        Me.ToolStripButton2.Size = New System.Drawing.Size(60, 22)
        Me.ToolStripButton2.Text = "Delete"
        '
        'ctlMultiControl
        '
        Me.ctlMultiControl.BackColor = System.Drawing.Color.Transparent
        Me.ctlMultiControl.Controller = Nothing
        Me.ctlMultiControl.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlMultiControl.Location = New System.Drawing.Point(0, 0)
        Me.ctlMultiControl.Name = "ctlMultiControl"
        Me.ctlMultiControl.Padding = New System.Windows.Forms.Padding(10)
        Me.ctlMultiControl.Size = New System.Drawing.Size(657, 177)
        Me.ctlMultiControl.TabIndex = 0
        Me.ctlMultiControl.Value = Nothing
        Me.ctlMultiControl.Visible = False
        '
        'lstTypes
        '
        Me.lstTypes.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2, Me.ColumnHeader3})
        Me.lstTypes.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstTypes.FullRowSelect = True
        Me.lstTypes.HideSelection = False
        Me.lstTypes.Location = New System.Drawing.Point(0, 25)
        Me.lstTypes.MultiSelect = False
        Me.lstTypes.Name = "lstTypes"
        Me.lstTypes.Size = New System.Drawing.Size(657, 81)
        Me.lstTypes.TabIndex = 5
        Me.lstTypes.UseCompatibleStateImageBehavior = False
        Me.lstTypes.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Text = "Name"
        Me.ColumnHeader1.Width = 138
        '
        'ColumnHeader2
        '
        Me.ColumnHeader2.Text = "Value"
        Me.ColumnHeader2.Width = 354
        '
        'ColumnHeader3
        '
        Me.ColumnHeader3.Text = "Source"
        Me.ColumnHeader3.Width = 115
        '
        'AttributesControl
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.Transparent
        Me.Controls.Add(Me.ctlSplitContainerMain)
        Me.Name = "AttributesControl"
        Me.Size = New System.Drawing.Size(657, 456)
        Me.ctlSplitContainerMain.Panel1.ResumeLayout(False)
        Me.ctlSplitContainerMain.Panel1.PerformLayout()
        Me.ctlSplitContainerMain.Panel2.ResumeLayout(False)
        CType(Me.ctlSplitContainerMain, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ctlSplitContainerMain.ResumeLayout(False)
        Me.ctlSplitContainer.Panel1.ResumeLayout(False)
        Me.ctlSplitContainer.Panel1.PerformLayout()
        Me.ctlSplitContainer.Panel2.ResumeLayout(False)
        CType(Me.ctlSplitContainer, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ctlSplitContainer.ResumeLayout(False)
        Me.ctlToolStrip.ResumeLayout(False)
        Me.ctlToolStrip.PerformLayout()
        Me.ctlTypesToolStrip.ResumeLayout(False)
        Me.ctlTypesToolStrip.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ctlSplitContainerMain As System.Windows.Forms.SplitContainer
    Friend WithEvents ctlSplitContainer As System.Windows.Forms.SplitContainer
    Friend WithEvents lstAttributes As System.Windows.Forms.ListView
    Friend WithEvents colName As System.Windows.Forms.ColumnHeader
    Friend WithEvents colValue As System.Windows.Forms.ColumnHeader
    Friend WithEvents colSource As System.Windows.Forms.ColumnHeader
    Friend WithEvents ctlToolStrip As System.Windows.Forms.ToolStrip
    Friend WithEvents cmdAdd As System.Windows.Forms.ToolStripButton
    Friend WithEvents cmdDelete As System.Windows.Forms.ToolStripButton
    Friend WithEvents ctlMultiControl As AxeSoftware.Quest.MultiControl
    Friend WithEvents ToolStripLabel1 As System.Windows.Forms.ToolStripLabel
    Friend WithEvents ctlTypesToolStrip As System.Windows.Forms.ToolStrip
    Friend WithEvents ToolStripLabel2 As System.Windows.Forms.ToolStripLabel
    Friend WithEvents ToolStripButton1 As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripButton2 As System.Windows.Forms.ToolStripButton
    Friend WithEvents lstTypes As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader2 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader3 As System.Windows.Forms.ColumnHeader

End Class
