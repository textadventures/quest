namespace AxeSoftware.Quest.EditorControls
{
    partial class WFEditorTree
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WFEditorTree));
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.AddJavascriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddObjectTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddDelegateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctlToolStrip = new System.Windows.Forms.ToolStrip();
            this.mnuFilter = new System.Windows.Forms.ToolStripDropDownButton();
            this.lstSearchResults = new System.Windows.Forms.ListView();
            this.colSearchResults = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmdClose = new System.Windows.Forms.Button();
            this.cmdSearch = new System.Windows.Forms.Button();
            this.pnlSearchContainer = new System.Windows.Forms.Panel();
            this.AddDynamicTemplateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddTemplateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddImpliedTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddRoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctlContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.AddExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddVerbToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddCommandToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddFunctionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddWalkthroughToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctlTreeView = new System.Windows.Forms.TreeView();
            this.ctlToolStrip.SuspendLayout();
            this.pnlSearchContainer.SuspendLayout();
            this.ctlContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(0, 0);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(269, 20);
            this.txtSearch.TabIndex = 1;
            // 
            // AddJavascriptToolStripMenuItem
            // 
            this.AddJavascriptToolStripMenuItem.Name = "AddJavascriptToolStripMenuItem";
            this.AddJavascriptToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.AddJavascriptToolStripMenuItem.Tag = "addjavascript";
            this.AddJavascriptToolStripMenuItem.Text = "Add Javascript";
            // 
            // AddEditorToolStripMenuItem
            // 
            this.AddEditorToolStripMenuItem.Name = "AddEditorToolStripMenuItem";
            this.AddEditorToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.AddEditorToolStripMenuItem.Tag = "addeditor";
            this.AddEditorToolStripMenuItem.Text = "Add Editor";
            this.AddEditorToolStripMenuItem.Visible = false;
            // 
            // AddObjectTypeToolStripMenuItem
            // 
            this.AddObjectTypeToolStripMenuItem.Name = "AddObjectTypeToolStripMenuItem";
            this.AddObjectTypeToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.AddObjectTypeToolStripMenuItem.Tag = "addobjecttype";
            this.AddObjectTypeToolStripMenuItem.Text = "Add Object Type";
            // 
            // AddDelegateToolStripMenuItem
            // 
            this.AddDelegateToolStripMenuItem.Name = "AddDelegateToolStripMenuItem";
            this.AddDelegateToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.AddDelegateToolStripMenuItem.Tag = "adddelegate";
            this.AddDelegateToolStripMenuItem.Text = "Add Delegate";
            this.AddDelegateToolStripMenuItem.Visible = false;
            // 
            // ctlToolStrip
            // 
            this.ctlToolStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ctlToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFilter});
            this.ctlToolStrip.Location = new System.Drawing.Point(0, 312);
            this.ctlToolStrip.Name = "ctlToolStrip";
            this.ctlToolStrip.Size = new System.Drawing.Size(289, 25);
            this.ctlToolStrip.TabIndex = 5;
            this.ctlToolStrip.Text = "ToolStrip1";
            // 
            // mnuFilter
            // 
            this.mnuFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.mnuFilter.Image = ((System.Drawing.Image)(resources.GetObject("mnuFilter.Image")));
            this.mnuFilter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnuFilter.Name = "mnuFilter";
            this.mnuFilter.Size = new System.Drawing.Size(46, 22);
            this.mnuFilter.Text = "Filter";
            // 
            // lstSearchResults
            // 
            this.lstSearchResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colSearchResults});
            this.lstSearchResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstSearchResults.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lstSearchResults.HideSelection = false;
            this.lstSearchResults.Location = new System.Drawing.Point(0, 20);
            this.lstSearchResults.MultiSelect = false;
            this.lstSearchResults.Name = "lstSearchResults";
            this.lstSearchResults.Size = new System.Drawing.Size(289, 317);
            this.lstSearchResults.TabIndex = 8;
            this.lstSearchResults.UseCompatibleStateImageBehavior = false;
            this.lstSearchResults.View = System.Windows.Forms.View.Details;
            this.lstSearchResults.Visible = false;
            // 
            // colSearchResults
            // 
            this.colSearchResults.Text = "Search Results";
            // 
            // cmdClose
            // 
            this.cmdClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdClose.Image = ((System.Drawing.Image)(resources.GetObject("cmdClose.Image")));
            this.cmdClose.Location = new System.Drawing.Point(249, 0);
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.Size = new System.Drawing.Size(20, 20);
            this.cmdClose.TabIndex = 2;
            this.cmdClose.UseVisualStyleBackColor = true;
            this.cmdClose.Visible = false;
            // 
            // cmdSearch
            // 
            this.cmdSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdSearch.Image = ((System.Drawing.Image)(resources.GetObject("cmdSearch.Image")));
            this.cmdSearch.Location = new System.Drawing.Point(269, 0);
            this.cmdSearch.Name = "cmdSearch";
            this.cmdSearch.Size = new System.Drawing.Size(20, 20);
            this.cmdSearch.TabIndex = 3;
            this.cmdSearch.UseVisualStyleBackColor = true;
            // 
            // pnlSearchContainer
            // 
            this.pnlSearchContainer.Controls.Add(this.cmdClose);
            this.pnlSearchContainer.Controls.Add(this.cmdSearch);
            this.pnlSearchContainer.Controls.Add(this.txtSearch);
            this.pnlSearchContainer.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSearchContainer.Location = new System.Drawing.Point(0, 0);
            this.pnlSearchContainer.Name = "pnlSearchContainer";
            this.pnlSearchContainer.Size = new System.Drawing.Size(289, 20);
            this.pnlSearchContainer.TabIndex = 6;
            // 
            // AddDynamicTemplateToolStripMenuItem
            // 
            this.AddDynamicTemplateToolStripMenuItem.Name = "AddDynamicTemplateToolStripMenuItem";
            this.AddDynamicTemplateToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.AddDynamicTemplateToolStripMenuItem.Tag = "adddynamictemplate";
            this.AddDynamicTemplateToolStripMenuItem.Text = "Add Dynamic Template";
            // 
            // AddTemplateToolStripMenuItem
            // 
            this.AddTemplateToolStripMenuItem.Name = "AddTemplateToolStripMenuItem";
            this.AddTemplateToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.AddTemplateToolStripMenuItem.Tag = "addtemplate";
            this.AddTemplateToolStripMenuItem.Text = "Add Template";
            // 
            // AddImpliedTypeToolStripMenuItem
            // 
            this.AddImpliedTypeToolStripMenuItem.Name = "AddImpliedTypeToolStripMenuItem";
            this.AddImpliedTypeToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.AddImpliedTypeToolStripMenuItem.Tag = "addimpliedtype";
            this.AddImpliedTypeToolStripMenuItem.Text = "Add Implied Type";
            this.AddImpliedTypeToolStripMenuItem.Visible = false;
            // 
            // AddRoomToolStripMenuItem
            // 
            this.AddRoomToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.AddRoomToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Silver;
            this.AddRoomToolStripMenuItem.Name = "AddRoomToolStripMenuItem";
            this.AddRoomToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.AddRoomToolStripMenuItem.Tag = "addroom";
            this.AddRoomToolStripMenuItem.Text = "Add Room";
            // 
            // AddObjectToolStripMenuItem
            // 
            this.AddObjectToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.AddObjectToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Silver;
            this.AddObjectToolStripMenuItem.Name = "AddObjectToolStripMenuItem";
            this.AddObjectToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.AddObjectToolStripMenuItem.Tag = "addobject";
            this.AddObjectToolStripMenuItem.Text = "Add Object";
            // 
            // ctlContextMenu
            // 
            this.ctlContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddObjectToolStripMenuItem,
            this.AddRoomToolStripMenuItem,
            this.AddExitToolStripMenuItem,
            this.AddVerbToolStripMenuItem,
            this.AddCommandToolStripMenuItem,
            this.AddFunctionToolStripMenuItem,
            this.AddWalkthroughToolStripMenuItem,
            this.AddLibraryToolStripMenuItem,
            this.AddImpliedTypeToolStripMenuItem,
            this.AddTemplateToolStripMenuItem,
            this.AddDynamicTemplateToolStripMenuItem,
            this.AddDelegateToolStripMenuItem,
            this.AddObjectTypeToolStripMenuItem,
            this.AddEditorToolStripMenuItem,
            this.AddJavascriptToolStripMenuItem});
            this.ctlContextMenu.Name = "ctlContextMenu";
            this.ctlContextMenu.Size = new System.Drawing.Size(200, 334);
            // 
            // AddExitToolStripMenuItem
            // 
            this.AddExitToolStripMenuItem.Name = "AddExitToolStripMenuItem";
            this.AddExitToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.AddExitToolStripMenuItem.Tag = "addexit";
            this.AddExitToolStripMenuItem.Text = "Add Exit";
            // 
            // AddVerbToolStripMenuItem
            // 
            this.AddVerbToolStripMenuItem.Name = "AddVerbToolStripMenuItem";
            this.AddVerbToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.AddVerbToolStripMenuItem.Tag = "addverb";
            this.AddVerbToolStripMenuItem.Text = "Add Verb";
            // 
            // AddCommandToolStripMenuItem
            // 
            this.AddCommandToolStripMenuItem.Name = "AddCommandToolStripMenuItem";
            this.AddCommandToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.AddCommandToolStripMenuItem.Tag = "addcommand";
            this.AddCommandToolStripMenuItem.Text = "Add Command";
            // 
            // AddFunctionToolStripMenuItem
            // 
            this.AddFunctionToolStripMenuItem.Name = "AddFunctionToolStripMenuItem";
            this.AddFunctionToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.AddFunctionToolStripMenuItem.Tag = "addfunction";
            this.AddFunctionToolStripMenuItem.Text = "Add Function";
            // 
            // AddWalkthroughToolStripMenuItem
            // 
            this.AddWalkthroughToolStripMenuItem.Name = "AddWalkthroughToolStripMenuItem";
            this.AddWalkthroughToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.AddWalkthroughToolStripMenuItem.Tag = "addwalkthrough";
            this.AddWalkthroughToolStripMenuItem.Text = "Add Walkthrough";
            // 
            // AddLibraryToolStripMenuItem
            // 
            this.AddLibraryToolStripMenuItem.Name = "AddLibraryToolStripMenuItem";
            this.AddLibraryToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.AddLibraryToolStripMenuItem.Tag = "addlibrary";
            this.AddLibraryToolStripMenuItem.Text = "Add Library";
            // 
            // ctlTreeView
            // 
            this.ctlTreeView.AllowDrop = true;
            this.ctlTreeView.ContextMenuStrip = this.ctlContextMenu;
            this.ctlTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlTreeView.HideSelection = false;
            this.ctlTreeView.Location = new System.Drawing.Point(0, 20);
            this.ctlTreeView.Name = "ctlTreeView";
            this.ctlTreeView.Size = new System.Drawing.Size(289, 292);
            this.ctlTreeView.TabIndex = 7;
            // 
            // WFEditorTree
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ctlTreeView);
            this.Controls.Add(this.ctlToolStrip);
            this.Controls.Add(this.lstSearchResults);
            this.Controls.Add(this.pnlSearchContainer);
            this.Name = "WFEditorTree";
            this.Size = new System.Drawing.Size(289, 337);
            this.ctlToolStrip.ResumeLayout(false);
            this.ctlToolStrip.PerformLayout();
            this.pnlSearchContainer.ResumeLayout(false);
            this.pnlSearchContainer.PerformLayout();
            this.ctlContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.TextBox txtSearch;
        internal System.Windows.Forms.ToolStripMenuItem AddJavascriptToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddEditorToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddObjectTypeToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddDelegateToolStripMenuItem;
        internal System.Windows.Forms.ToolStrip ctlToolStrip;
        internal System.Windows.Forms.ToolStripDropDownButton mnuFilter;
        internal System.Windows.Forms.ListView lstSearchResults;
        internal System.Windows.Forms.ColumnHeader colSearchResults;
        internal System.Windows.Forms.Button cmdClose;
        internal System.Windows.Forms.Button cmdSearch;
        internal System.Windows.Forms.Panel pnlSearchContainer;
        internal System.Windows.Forms.ToolStripMenuItem AddDynamicTemplateToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddTemplateToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddImpliedTypeToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddRoomToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddObjectToolStripMenuItem;
        internal System.Windows.Forms.ContextMenuStrip ctlContextMenu;
        internal System.Windows.Forms.ToolStripMenuItem AddExitToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddVerbToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddCommandToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddFunctionToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddWalkthroughToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddLibraryToolStripMenuItem;
        internal System.Windows.Forms.TreeView ctlTreeView;
    }
}
