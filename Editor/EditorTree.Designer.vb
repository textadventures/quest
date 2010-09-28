<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class EditorTree
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(EditorTree))
        Me.txtSearch = New System.Windows.Forms.TextBox
        Me.ctlTreeView = New System.Windows.Forms.TreeView
        Me.ctlToolStrip = New System.Windows.Forms.ToolStrip
        Me.mnuFilter = New System.Windows.Forms.ToolStripDropDownButton
        Me.ctlToolStrip.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtSearch
        '
        Me.txtSearch.Dock = System.Windows.Forms.DockStyle.Top
        Me.txtSearch.Location = New System.Drawing.Point(0, 0)
        Me.txtSearch.Name = "txtSearch"
        Me.txtSearch.Size = New System.Drawing.Size(204, 20)
        Me.txtSearch.TabIndex = 0
        '
        'ctlTreeView
        '
        Me.ctlTreeView.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlTreeView.HideSelection = False
        Me.ctlTreeView.Location = New System.Drawing.Point(0, 20)
        Me.ctlTreeView.Name = "ctlTreeView"
        Me.ctlTreeView.Size = New System.Drawing.Size(204, 282)
        Me.ctlTreeView.TabIndex = 1
        '
        'ctlToolStrip
        '
        Me.ctlToolStrip.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.ctlToolStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuFilter})
        Me.ctlToolStrip.Location = New System.Drawing.Point(0, 302)
        Me.ctlToolStrip.Name = "ctlToolStrip"
        Me.ctlToolStrip.Size = New System.Drawing.Size(204, 25)
        Me.ctlToolStrip.TabIndex = 2
        Me.ctlToolStrip.Text = "ToolStrip1"
        '
        'mnuFilter
        '
        Me.mnuFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
        Me.mnuFilter.Image = CType(resources.GetObject("mnuFilter.Image"), System.Drawing.Image)
        Me.mnuFilter.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.mnuFilter.Name = "mnuFilter"
        Me.mnuFilter.Size = New System.Drawing.Size(46, 22)
        Me.mnuFilter.Text = "Filter"
        '
        'EditorTree
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.ctlTreeView)
        Me.Controls.Add(Me.ctlToolStrip)
        Me.Controls.Add(Me.txtSearch)
        Me.Name = "EditorTree"
        Me.Size = New System.Drawing.Size(204, 327)
        Me.ctlToolStrip.ResumeLayout(False)
        Me.ctlToolStrip.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtSearch As System.Windows.Forms.TextBox
    Friend WithEvents ctlTreeView As System.Windows.Forms.TreeView
    Friend WithEvents ctlToolStrip As System.Windows.Forms.ToolStrip
    Friend WithEvents mnuFilter As System.Windows.Forms.ToolStripDropDownButton

End Class
