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
        Me.ctlTreeView = New System.Windows.Forms.TreeView()
        Me.ctlToolStrip = New System.Windows.Forms.ToolStrip()
        Me.mnuFilter = New System.Windows.Forms.ToolStripDropDownButton()
        Me.pnlSearchContainer = New System.Windows.Forms.Panel()
        Me.txtSearch = New System.Windows.Forms.TextBox()
        Me.cmdClose = New System.Windows.Forms.Button()
        Me.cmdSearch = New System.Windows.Forms.Button()
        Me.lstSearchResults = New System.Windows.Forms.ListView()
        Me.colSearchResults = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ctlToolStrip.SuspendLayout()
        Me.pnlSearchContainer.SuspendLayout()
        Me.SuspendLayout()
        '
        'ctlTreeView
        '
        Me.ctlTreeView.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ctlTreeView.HideSelection = False
        Me.ctlTreeView.Location = New System.Drawing.Point(0, 20)
        Me.ctlTreeView.Name = "ctlTreeView"
        Me.ctlTreeView.Size = New System.Drawing.Size(204, 282)
        Me.ctlTreeView.TabIndex = 4
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
        'pnlSearchContainer
        '
        Me.pnlSearchContainer.Controls.Add(Me.txtSearch)
        Me.pnlSearchContainer.Controls.Add(Me.cmdClose)
        Me.pnlSearchContainer.Controls.Add(Me.cmdSearch)
        Me.pnlSearchContainer.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlSearchContainer.Location = New System.Drawing.Point(0, 0)
        Me.pnlSearchContainer.Name = "pnlSearchContainer"
        Me.pnlSearchContainer.Size = New System.Drawing.Size(204, 20)
        Me.pnlSearchContainer.TabIndex = 3
        '
        'txtSearch
        '
        Me.txtSearch.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtSearch.Location = New System.Drawing.Point(0, 0)
        Me.txtSearch.Name = "txtSearch"
        Me.txtSearch.Size = New System.Drawing.Size(184, 20)
        Me.txtSearch.TabIndex = 1
        '
        'cmdClose
        '
        Me.cmdClose.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdClose.Image = CType(resources.GetObject("cmdClose.Image"), System.Drawing.Image)
        Me.cmdClose.Location = New System.Drawing.Point(166, 0)
        Me.cmdClose.Name = "cmdClose"
        Me.cmdClose.Size = New System.Drawing.Size(20, 20)
        Me.cmdClose.TabIndex = 2
        Me.cmdClose.UseVisualStyleBackColor = True
        Me.cmdClose.Visible = False
        '
        'cmdSearch
        '
        Me.cmdSearch.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdSearch.Image = CType(resources.GetObject("cmdSearch.Image"), System.Drawing.Image)
        Me.cmdSearch.Location = New System.Drawing.Point(184, 0)
        Me.cmdSearch.Name = "cmdSearch"
        Me.cmdSearch.Size = New System.Drawing.Size(20, 20)
        Me.cmdSearch.TabIndex = 3
        Me.cmdSearch.UseVisualStyleBackColor = True
        '
        'lstSearchResults
        '
        Me.lstSearchResults.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colSearchResults})
        Me.lstSearchResults.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstSearchResults.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None
        Me.lstSearchResults.HideSelection = False
        Me.lstSearchResults.Location = New System.Drawing.Point(0, 20)
        Me.lstSearchResults.MultiSelect = False
        Me.lstSearchResults.Name = "lstSearchResults"
        Me.lstSearchResults.Size = New System.Drawing.Size(204, 282)
        Me.lstSearchResults.TabIndex = 4
        Me.lstSearchResults.UseCompatibleStateImageBehavior = False
        Me.lstSearchResults.View = System.Windows.Forms.View.Details
        Me.lstSearchResults.Visible = False
        '
        'colSearchResults
        '
        Me.colSearchResults.Text = "Search Results"
        '
        'EditorTree
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.lstSearchResults)
        Me.Controls.Add(Me.ctlTreeView)
        Me.Controls.Add(Me.ctlToolStrip)
        Me.Controls.Add(Me.pnlSearchContainer)
        Me.Name = "EditorTree"
        Me.Size = New System.Drawing.Size(204, 327)
        Me.ctlToolStrip.ResumeLayout(False)
        Me.ctlToolStrip.PerformLayout()
        Me.pnlSearchContainer.ResumeLayout(False)
        Me.pnlSearchContainer.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ctlTreeView As System.Windows.Forms.TreeView
    Friend WithEvents ctlToolStrip As System.Windows.Forms.ToolStrip
    Friend WithEvents mnuFilter As System.Windows.Forms.ToolStripDropDownButton
    Friend WithEvents pnlSearchContainer As System.Windows.Forms.Panel
    Friend WithEvents txtSearch As System.Windows.Forms.TextBox
    Friend WithEvents cmdSearch As System.Windows.Forms.Button
    Friend WithEvents cmdClose As System.Windows.Forms.Button
    Friend WithEvents lstSearchResults As System.Windows.Forms.ListView
    Friend WithEvents colSearchResults As System.Windows.Forms.ColumnHeader

End Class
