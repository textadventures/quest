namespace TextAdventures.Quest.EditorControls
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
      this.ctlToolStrip = new System.Windows.Forms.ToolStrip();
      this.mnuFilter = new System.Windows.Forms.ToolStripDropDownButton();
      this.lstSearchResults = new System.Windows.Forms.ListView();
      this.colSearchResults = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.pnlSearchContainer = new System.Windows.Forms.Panel();
      this.cmdSearch = new System.Windows.Forms.Button();
      this.cmdClose = new System.Windows.Forms.Button();
      this.ctlContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.expandAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.collapseAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
      this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
      this.AddVerbToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.AddCommandToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
      this.addPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.AddRoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.AddObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.AddExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
      this.addTurnScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.AddFunctionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.addTimerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.AddWalkthroughToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
      this.AddEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.AddLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.AddTemplateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.AddDynamicTemplateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.AddObjectTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.AddJavascriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.AddImpliedTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.AddDelegateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.ctlTreeView = new System.Windows.Forms.TreeView();
      this.ctlImageList = new System.Windows.Forms.ImageList(this.components);
      this.roomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.ctlToolStrip.SuspendLayout();
      this.pnlSearchContainer.SuspendLayout();
      this.ctlContextMenu.SuspendLayout();
      this.SuspendLayout();
      // 
      // txtSearch
      // 
      this.txtSearch.BackColor = System.Drawing.Color.White;
      this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.None;
      resources.ApplyResources(this.txtSearch, "txtSearch");
      this.txtSearch.ForeColor = System.Drawing.Color.Black;
      this.txtSearch.Name = "txtSearch";
      this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
      // 
      // ctlToolStrip
      // 
      resources.ApplyResources(this.ctlToolStrip, "ctlToolStrip");
      this.ctlToolStrip.BackColor = System.Drawing.Color.GhostWhite;
      this.ctlToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      this.ctlToolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
      this.ctlToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFilter});
      this.ctlToolStrip.Name = "ctlToolStrip";
      // 
      // mnuFilter
      // 
      this.mnuFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      resources.ApplyResources(this.mnuFilter, "mnuFilter");
      this.mnuFilter.Name = "mnuFilter";
      // 
      // lstSearchResults
      // 
      this.lstSearchResults.BackColor = System.Drawing.Color.GhostWhite;
      this.lstSearchResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.lstSearchResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colSearchResults});
      resources.ApplyResources(this.lstSearchResults, "lstSearchResults");
      this.lstSearchResults.ForeColor = System.Drawing.Color.Black;
      this.lstSearchResults.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
      this.lstSearchResults.HideSelection = false;
      this.lstSearchResults.MultiSelect = false;
      this.lstSearchResults.Name = "lstSearchResults";
      this.lstSearchResults.UseCompatibleStateImageBehavior = false;
      this.lstSearchResults.View = System.Windows.Forms.View.Details;
      // 
      // colSearchResults
      // 
      resources.ApplyResources(this.colSearchResults, "colSearchResults");
      // 
      // pnlSearchContainer
      // 
      this.pnlSearchContainer.BackColor = System.Drawing.Color.GhostWhite;
      this.pnlSearchContainer.Controls.Add(this.cmdSearch);
      this.pnlSearchContainer.Controls.Add(this.cmdClose);
      this.pnlSearchContainer.Controls.Add(this.txtSearch);
      resources.ApplyResources(this.pnlSearchContainer, "pnlSearchContainer");
      this.pnlSearchContainer.Name = "pnlSearchContainer";
      // 
      // cmdSearch
      // 
      this.cmdSearch.BackColor = System.Drawing.Color.White;
      resources.ApplyResources(this.cmdSearch, "cmdSearch");
      this.cmdSearch.FlatAppearance.BorderSize = 0;
      this.cmdSearch.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_tv_search;
      this.cmdSearch.Name = "cmdSearch";
      this.cmdSearch.TabStop = false;
      this.cmdSearch.UseVisualStyleBackColor = false;
      // 
      // cmdClose
      // 
      this.cmdClose.BackColor = System.Drawing.Color.White;
      resources.ApplyResources(this.cmdClose, "cmdClose");
      this.cmdClose.FlatAppearance.BorderSize = 0;
      this.cmdClose.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_tv_delete;
      this.cmdClose.Name = "cmdClose";
      this.cmdClose.TabStop = false;
      this.cmdClose.UseVisualStyleBackColor = false;
      // 
      // ctlContextMenu
      // 
      this.ctlContextMenu.BackColor = System.Drawing.Color.White;
      this.ctlContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
      this.ctlContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.expandAllToolStripMenuItem,
            this.collapseAllToolStripMenuItem,
            this.toolStripSeparator1,
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.toolStripSeparator2,
            this.AddVerbToolStripMenuItem,
            this.AddCommandToolStripMenuItem,
            this.toolStripSeparator3,
            this.addPageToolStripMenuItem,
            this.AddRoomToolStripMenuItem,
            this.AddObjectToolStripMenuItem,
            this.AddExitToolStripMenuItem,
            this.toolStripSeparator4,
            this.addTurnScriptToolStripMenuItem,
            this.AddFunctionToolStripMenuItem,
            this.addTimerToolStripMenuItem,
            this.AddWalkthroughToolStripMenuItem,
            this.toolStripSeparator5,
            this.AddEditorToolStripMenuItem,
            this.AddLibraryToolStripMenuItem,
            this.AddTemplateToolStripMenuItem,
            this.AddDynamicTemplateToolStripMenuItem,
            this.AddObjectTypeToolStripMenuItem,
            this.AddJavascriptToolStripMenuItem,
            this.AddImpliedTypeToolStripMenuItem,
            this.AddDelegateToolStripMenuItem});
      this.ctlContextMenu.Name = "ctlContextMenu";
      resources.ApplyResources(this.ctlContextMenu, "ctlContextMenu");
      this.ctlContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.ctlContextMenu_Opening);
      // 
      // expandAllToolStripMenuItem
      // 
      resources.ApplyResources(this.expandAllToolStripMenuItem, "expandAllToolStripMenuItem");
      this.expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
      this.expandAllToolStripMenuItem.Tag = "expandall";
      // 
      // collapseAllToolStripMenuItem
      // 
      resources.ApplyResources(this.collapseAllToolStripMenuItem, "collapseAllToolStripMenuItem");
      this.collapseAllToolStripMenuItem.Name = "collapseAllToolStripMenuItem";
      this.collapseAllToolStripMenuItem.Tag = "collapseall";
      // 
      // toolStripSeparator1
      // 
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
      this.toolStripSeparator1.Tag = "separator1";
      // 
      // cutToolStripMenuItem
      // 
      this.cutToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_cut;
      resources.ApplyResources(this.cutToolStripMenuItem, "cutToolStripMenuItem");
      this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
      this.cutToolStripMenuItem.Tag = "cut";
      // 
      // copyToolStripMenuItem
      // 
      this.copyToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_copy;
      resources.ApplyResources(this.copyToolStripMenuItem, "copyToolStripMenuItem");
      this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
      this.copyToolStripMenuItem.Tag = "copy";
      // 
      // pasteToolStripMenuItem
      // 
      this.pasteToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_paste;
      resources.ApplyResources(this.pasteToolStripMenuItem, "pasteToolStripMenuItem");
      this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
      this.pasteToolStripMenuItem.Tag = "paste";
      // 
      // deleteToolStripMenuItem
      // 
      this.deleteToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_delete;
      resources.ApplyResources(this.deleteToolStripMenuItem, "deleteToolStripMenuItem");
      this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
      this.deleteToolStripMenuItem.Tag = "delete";
      // 
      // toolStripSeparator2
      // 
      this.toolStripSeparator2.Name = "toolStripSeparator2";
      resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
      this.toolStripSeparator2.Tag = "separator2";
      // 
      // AddVerbToolStripMenuItem
      // 
      this.AddVerbToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_verb;
      resources.ApplyResources(this.AddVerbToolStripMenuItem, "AddVerbToolStripMenuItem");
      this.AddVerbToolStripMenuItem.Name = "AddVerbToolStripMenuItem";
      this.AddVerbToolStripMenuItem.Tag = "addverb";
      // 
      // AddCommandToolStripMenuItem
      // 
      this.AddCommandToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_command;
      resources.ApplyResources(this.AddCommandToolStripMenuItem, "AddCommandToolStripMenuItem");
      this.AddCommandToolStripMenuItem.Name = "AddCommandToolStripMenuItem";
      this.AddCommandToolStripMenuItem.Tag = "addcommand";
      // 
      // toolStripSeparator3
      // 
      this.toolStripSeparator3.Name = "toolStripSeparator3";
      resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
      this.toolStripSeparator3.Tag = "separator3";
      // 
      // addPageToolStripMenuItem
      // 
      this.addPageToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_add_page;
      resources.ApplyResources(this.addPageToolStripMenuItem, "addPageToolStripMenuItem");
      this.addPageToolStripMenuItem.Name = "addPageToolStripMenuItem";
      this.addPageToolStripMenuItem.Tag = "addpage";
      // 
      // AddRoomToolStripMenuItem
      // 
      this.AddRoomToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_room;
      resources.ApplyResources(this.AddRoomToolStripMenuItem, "AddRoomToolStripMenuItem");
      this.AddRoomToolStripMenuItem.Name = "AddRoomToolStripMenuItem";
      this.AddRoomToolStripMenuItem.Tag = "addroom";
      // 
      // AddObjectToolStripMenuItem
      // 
      this.AddObjectToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_object;
      resources.ApplyResources(this.AddObjectToolStripMenuItem, "AddObjectToolStripMenuItem");
      this.AddObjectToolStripMenuItem.Name = "AddObjectToolStripMenuItem";
      this.AddObjectToolStripMenuItem.Tag = "addobject";
      // 
      // AddExitToolStripMenuItem
      // 
      this.AddExitToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_exit;
      resources.ApplyResources(this.AddExitToolStripMenuItem, "AddExitToolStripMenuItem");
      this.AddExitToolStripMenuItem.Name = "AddExitToolStripMenuItem";
      this.AddExitToolStripMenuItem.Tag = "addexit";
      // 
      // toolStripSeparator4
      // 
      this.toolStripSeparator4.Name = "toolStripSeparator4";
      resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
      this.toolStripSeparator4.Tag = "separator4";
      // 
      // addTurnScriptToolStripMenuItem
      // 
      this.addTurnScriptToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_turn;
      resources.ApplyResources(this.addTurnScriptToolStripMenuItem, "addTurnScriptToolStripMenuItem");
      this.addTurnScriptToolStripMenuItem.Name = "addTurnScriptToolStripMenuItem";
      this.addTurnScriptToolStripMenuItem.Tag = "addturnscript";
      // 
      // AddFunctionToolStripMenuItem
      // 
      this.AddFunctionToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_function;
      resources.ApplyResources(this.AddFunctionToolStripMenuItem, "AddFunctionToolStripMenuItem");
      this.AddFunctionToolStripMenuItem.Name = "AddFunctionToolStripMenuItem";
      this.AddFunctionToolStripMenuItem.Tag = "addfunction";
      // 
      // addTimerToolStripMenuItem
      // 
      this.addTimerToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_timer;
      resources.ApplyResources(this.addTimerToolStripMenuItem, "addTimerToolStripMenuItem");
      this.addTimerToolStripMenuItem.Name = "addTimerToolStripMenuItem";
      this.addTimerToolStripMenuItem.Tag = "addtimer";
      // 
      // AddWalkthroughToolStripMenuItem
      // 
      this.AddWalkthroughToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_walk;
      resources.ApplyResources(this.AddWalkthroughToolStripMenuItem, "AddWalkthroughToolStripMenuItem");
      this.AddWalkthroughToolStripMenuItem.Name = "AddWalkthroughToolStripMenuItem";
      this.AddWalkthroughToolStripMenuItem.Tag = "addwalkthrough";
      // 
      // toolStripSeparator5
      // 
      this.toolStripSeparator5.Name = "toolStripSeparator5";
      resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
      this.toolStripSeparator5.Tag = "separator5";
      // 
      // AddEditorToolStripMenuItem
      // 
      resources.ApplyResources(this.AddEditorToolStripMenuItem, "AddEditorToolStripMenuItem");
      this.AddEditorToolStripMenuItem.Name = "AddEditorToolStripMenuItem";
      this.AddEditorToolStripMenuItem.Tag = "addeditor";
      // 
      // AddLibraryToolStripMenuItem
      // 
      resources.ApplyResources(this.AddLibraryToolStripMenuItem, "AddLibraryToolStripMenuItem");
      this.AddLibraryToolStripMenuItem.Name = "AddLibraryToolStripMenuItem";
      this.AddLibraryToolStripMenuItem.Tag = "addlibrary";
      // 
      // AddTemplateToolStripMenuItem
      // 
      resources.ApplyResources(this.AddTemplateToolStripMenuItem, "AddTemplateToolStripMenuItem");
      this.AddTemplateToolStripMenuItem.Name = "AddTemplateToolStripMenuItem";
      this.AddTemplateToolStripMenuItem.Tag = "addtemplate";
      // 
      // AddDynamicTemplateToolStripMenuItem
      // 
      resources.ApplyResources(this.AddDynamicTemplateToolStripMenuItem, "AddDynamicTemplateToolStripMenuItem");
      this.AddDynamicTemplateToolStripMenuItem.Name = "AddDynamicTemplateToolStripMenuItem";
      this.AddDynamicTemplateToolStripMenuItem.Tag = "adddynamictemplate";
      // 
      // AddObjectTypeToolStripMenuItem
      // 
      resources.ApplyResources(this.AddObjectTypeToolStripMenuItem, "AddObjectTypeToolStripMenuItem");
      this.AddObjectTypeToolStripMenuItem.Name = "AddObjectTypeToolStripMenuItem";
      this.AddObjectTypeToolStripMenuItem.Tag = "addobjecttype";
      // 
      // AddJavascriptToolStripMenuItem
      // 
      resources.ApplyResources(this.AddJavascriptToolStripMenuItem, "AddJavascriptToolStripMenuItem");
      this.AddJavascriptToolStripMenuItem.Name = "AddJavascriptToolStripMenuItem";
      this.AddJavascriptToolStripMenuItem.Tag = "addjavascript";
      // 
      // AddImpliedTypeToolStripMenuItem
      // 
      resources.ApplyResources(this.AddImpliedTypeToolStripMenuItem, "AddImpliedTypeToolStripMenuItem");
      this.AddImpliedTypeToolStripMenuItem.Name = "AddImpliedTypeToolStripMenuItem";
      this.AddImpliedTypeToolStripMenuItem.Tag = "addimpliedtype";
      // 
      // AddDelegateToolStripMenuItem
      // 
      resources.ApplyResources(this.AddDelegateToolStripMenuItem, "AddDelegateToolStripMenuItem");
      this.AddDelegateToolStripMenuItem.Name = "AddDelegateToolStripMenuItem";
      this.AddDelegateToolStripMenuItem.Tag = "adddelegate";
      // 
      // ctlTreeView
      // 
      this.ctlTreeView.AllowDrop = true;
      this.ctlTreeView.BackColor = System.Drawing.Color.GhostWhite;
      this.ctlTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.ctlTreeView.ContextMenuStrip = this.ctlContextMenu;
      resources.ApplyResources(this.ctlTreeView, "ctlTreeView");
      this.ctlTreeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
      this.ctlTreeView.ForeColor = System.Drawing.Color.Black;
      this.ctlTreeView.HideSelection = false;
      this.ctlTreeView.ImageList = this.ctlImageList;
      this.ctlTreeView.LineColor = System.Drawing.Color.Silver;
      this.ctlTreeView.Name = "ctlTreeView";
      // 
      // ctlImageList
      // 
      this.ctlImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ctlImageList.ImageStream")));
      this.ctlImageList.TransparentColor = System.Drawing.Color.Transparent;
      this.ctlImageList.Images.SetKeyName(0, "s_object_close.png");
      this.ctlImageList.Images.SetKeyName(1, "s_object_open.png");
      // 
      // roomToolStripMenuItem
      // 
      this.roomToolStripMenuItem.Name = "roomToolStripMenuItem";
      resources.ApplyResources(this.roomToolStripMenuItem, "roomToolStripMenuItem");
      // 
      // WFEditorTree
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.GhostWhite;
      this.Controls.Add(this.ctlTreeView);
      this.Controls.Add(this.lstSearchResults);
      this.Controls.Add(this.ctlToolStrip);
      this.Controls.Add(this.pnlSearchContainer);
      this.ForeColor = System.Drawing.Color.Black;
      this.Name = "WFEditorTree";
      this.ctlToolStrip.ResumeLayout(false);
      this.ctlToolStrip.PerformLayout();
      this.pnlSearchContainer.ResumeLayout(false);
      this.pnlSearchContainer.PerformLayout();
      this.ctlContextMenu.ResumeLayout(false);
      this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.TextBox txtSearch;
        internal System.Windows.Forms.ToolStrip ctlToolStrip;
        internal System.Windows.Forms.ToolStripDropDownButton mnuFilter;
        internal System.Windows.Forms.ListView lstSearchResults;
        internal System.Windows.Forms.ColumnHeader colSearchResults;
        internal System.Windows.Forms.Panel pnlSearchContainer;
        internal System.Windows.Forms.TreeView ctlTreeView;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem expandAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collapseAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        internal System.Windows.Forms.ToolStripMenuItem AddWalkthroughToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addPageToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddEditorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem roomToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        internal System.Windows.Forms.ToolStripMenuItem AddCommandToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddVerbToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddRoomToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddObjectToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddExitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        internal System.Windows.Forms.ToolStripMenuItem AddFunctionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addTimerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addTurnScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        internal System.Windows.Forms.ToolStripMenuItem AddLibraryToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddTemplateToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddDynamicTemplateToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddObjectTypeToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddJavascriptToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddImpliedTypeToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem AddDelegateToolStripMenuItem;
        internal System.Windows.Forms.ContextMenuStrip ctlContextMenu;
        private System.Windows.Forms.ImageList ctlImageList;
        internal System.Windows.Forms.Button cmdSearch;
        internal System.Windows.Forms.Button cmdClose;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
    }
}
