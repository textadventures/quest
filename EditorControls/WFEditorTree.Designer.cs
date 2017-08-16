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
            this.cmdClose = new System.Windows.Forms.Button();
            this.cmdSearch = new System.Windows.Forms.Button();
            this.ctlContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.expandAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.AddVerbToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddCommandToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.addPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddRoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.addTurnScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddFunctionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addTimerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddWalkthroughToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
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
            resources.ApplyResources(this.txtSearch, "txtSearch");
            this.txtSearch.ForeColor = System.Drawing.Color.Black;
            this.txtSearch.Name = "txtSearch";
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
            this.lstSearchResults.BackColor = System.Drawing.Color.White;
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
            this.pnlSearchContainer.Controls.Add(this.cmdClose);
            this.pnlSearchContainer.Controls.Add(this.cmdSearch);
            this.pnlSearchContainer.Controls.Add(this.txtSearch);
            resources.ApplyResources(this.pnlSearchContainer, "pnlSearchContainer");
            this.pnlSearchContainer.Name = "pnlSearchContainer";
            // 
            // cmdClose
            // 
            this.cmdClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(27)))), ((int)(((byte)(97)))));
            this.cmdClose.BackgroundImage = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Löschen_16__1_;
            resources.ApplyResources(this.cmdClose, "cmdClose");
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.UseVisualStyleBackColor = false;
            // 
            // cmdSearch
            // 
            this.cmdSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(27)))), ((int)(((byte)(97)))));
            this.cmdSearch.BackgroundImage = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Suche_16;
            resources.ApplyResources(this.cmdSearch, "cmdSearch");
            this.cmdSearch.Name = "cmdSearch";
            this.cmdSearch.UseVisualStyleBackColor = false;
            // 
            // ctlContextMenu
            // 
            this.ctlContextMenu.BackColor = System.Drawing.Color.White;
            this.ctlContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ctlContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.expandAllToolStripMenuItem,
            this.collapseAllToolStripMenuItem,
            this.toolStripSeparator1,
            this.deleteToolStripMenuItem,
            this.toolStripSeparator2,
            this.AddVerbToolStripMenuItem,
            this.AddCommandToolStripMenuItem,
            this.toolStripSeparator3,
            this.addPageToolStripMenuItem,
            this.AddEditorToolStripMenuItem,
            this.AddRoomToolStripMenuItem,
            this.AddObjectToolStripMenuItem,
            this.AddExitToolStripMenuItem,
            this.toolStripSeparator4,
            this.addTurnScriptToolStripMenuItem,
            this.AddFunctionToolStripMenuItem,
            this.addTimerToolStripMenuItem,
            this.AddWalkthroughToolStripMenuItem,
            this.toolStripSeparator5,
            this.AddLibraryToolStripMenuItem,
            this.AddTemplateToolStripMenuItem,
            this.AddDynamicTemplateToolStripMenuItem,
            this.AddObjectTypeToolStripMenuItem,
            this.AddJavascriptToolStripMenuItem,
            this.AddImpliedTypeToolStripMenuItem,
            this.AddDelegateToolStripMenuItem});
            this.ctlContextMenu.Name = "ctlContextMenu";
            resources.ApplyResources(this.ctlContextMenu, "ctlContextMenu");
            // 
            // expandAllToolStripMenuItem
            // 
            this.expandAllToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Erweitern_32;
            this.expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
            resources.ApplyResources(this.expandAllToolStripMenuItem, "expandAllToolStripMenuItem");
            this.expandAllToolStripMenuItem.Tag = "expandall";
            // 
            // collapseAllToolStripMenuItem
            // 
            this.collapseAllToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Verkleinern_32;
            this.collapseAllToolStripMenuItem.Name = "collapseAllToolStripMenuItem";
            resources.ApplyResources(this.collapseAllToolStripMenuItem, "collapseAllToolStripMenuItem");
            this.collapseAllToolStripMenuItem.Tag = "collapseall";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            this.toolStripSeparator1.Tag = "separator1";
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Löschen_32;
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            resources.ApplyResources(this.deleteToolStripMenuItem, "deleteToolStripMenuItem");
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
            this.AddVerbToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Verb_32;
            this.AddVerbToolStripMenuItem.Name = "AddVerbToolStripMenuItem";
            resources.ApplyResources(this.AddVerbToolStripMenuItem, "AddVerbToolStripMenuItem");
            this.AddVerbToolStripMenuItem.Tag = "addverb";
            // 
            // AddCommandToolStripMenuItem
            // 
            this.AddCommandToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Konsole_32;
            this.AddCommandToolStripMenuItem.Name = "AddCommandToolStripMenuItem";
            resources.ApplyResources(this.AddCommandToolStripMenuItem, "AddCommandToolStripMenuItem");
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
            this.addPageToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Datei_hinzufügen_32;
            this.addPageToolStripMenuItem.Name = "addPageToolStripMenuItem";
            resources.ApplyResources(this.addPageToolStripMenuItem, "addPageToolStripMenuItem");
            this.addPageToolStripMenuItem.Tag = "addpage";
            // 
            // AddEditorToolStripMenuItem
            // 
            this.AddEditorToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Bearbeiten_32;
            this.AddEditorToolStripMenuItem.Name = "AddEditorToolStripMenuItem";
            resources.ApplyResources(this.AddEditorToolStripMenuItem, "AddEditorToolStripMenuItem");
            this.AddEditorToolStripMenuItem.Tag = "addeditor";
            // 
            // AddRoomToolStripMenuItem
            // 
            this.AddRoomToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Zimmer_32;
            resources.ApplyResources(this.AddRoomToolStripMenuItem, "AddRoomToolStripMenuItem");
            this.AddRoomToolStripMenuItem.Name = "AddRoomToolStripMenuItem";
            this.AddRoomToolStripMenuItem.Tag = "addroom";
            // 
            // AddObjectToolStripMenuItem
            // 
            this.AddObjectToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Topfpflanze_32;
            resources.ApplyResources(this.AddObjectToolStripMenuItem, "AddObjectToolStripMenuItem");
            this.AddObjectToolStripMenuItem.Name = "AddObjectToolStripMenuItem";
            this.AddObjectToolStripMenuItem.Tag = "addobject";
            // 
            // AddExitToolStripMenuItem
            // 
            this.AddExitToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Ausgang_32;
            this.AddExitToolStripMenuItem.Name = "AddExitToolStripMenuItem";
            resources.ApplyResources(this.AddExitToolStripMenuItem, "AddExitToolStripMenuItem");
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
            this.addTurnScriptToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Bauer_32;
            this.addTurnScriptToolStripMenuItem.Name = "addTurnScriptToolStripMenuItem";
            resources.ApplyResources(this.addTurnScriptToolStripMenuItem, "addTurnScriptToolStripMenuItem");
            this.addTurnScriptToolStripMenuItem.Tag = "addturnscript";
            // 
            // AddFunctionToolStripMenuItem
            // 
            this.AddFunctionToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Variable_32;
            this.AddFunctionToolStripMenuItem.Name = "AddFunctionToolStripMenuItem";
            resources.ApplyResources(this.AddFunctionToolStripMenuItem, "AddFunctionToolStripMenuItem");
            this.AddFunctionToolStripMenuItem.Tag = "addfunction";
            // 
            // addTimerToolStripMenuItem
            // 
            this.addTimerToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Timer_32;
            this.addTimerToolStripMenuItem.Name = "addTimerToolStripMenuItem";
            resources.ApplyResources(this.addTimerToolStripMenuItem, "addTimerToolStripMenuItem");
            this.addTimerToolStripMenuItem.Tag = "addtimer";
            // 
            // AddWalkthroughToolStripMenuItem
            // 
            this.AddWalkthroughToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Gehen_32;
            this.AddWalkthroughToolStripMenuItem.Name = "AddWalkthroughToolStripMenuItem";
            resources.ApplyResources(this.AddWalkthroughToolStripMenuItem, "AddWalkthroughToolStripMenuItem");
            this.AddWalkthroughToolStripMenuItem.Tag = "addwalkthrough";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
            this.toolStripSeparator5.Tag = "separator5";
            // 
            // AddLibraryToolStripMenuItem
            // 
            this.AddLibraryToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Bücherregal_32;
            this.AddLibraryToolStripMenuItem.Name = "AddLibraryToolStripMenuItem";
            resources.ApplyResources(this.AddLibraryToolStripMenuItem, "AddLibraryToolStripMenuItem");
            this.AddLibraryToolStripMenuItem.Tag = "addlibrary";
            // 
            // AddTemplateToolStripMenuItem
            // 
            this.AddTemplateToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Lebenslauf_Muster_32;
            this.AddTemplateToolStripMenuItem.Name = "AddTemplateToolStripMenuItem";
            resources.ApplyResources(this.AddTemplateToolStripMenuItem, "AddTemplateToolStripMenuItem");
            this.AddTemplateToolStripMenuItem.Tag = "addtemplate";
            // 
            // AddDynamicTemplateToolStripMenuItem
            // 
            this.AddDynamicTemplateToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Lebenslauf_Vorlage_laden_32;
            this.AddDynamicTemplateToolStripMenuItem.Name = "AddDynamicTemplateToolStripMenuItem";
            resources.ApplyResources(this.AddDynamicTemplateToolStripMenuItem, "AddDynamicTemplateToolStripMenuItem");
            this.AddDynamicTemplateToolStripMenuItem.Tag = "adddynamictemplate";
            // 
            // AddObjectTypeToolStripMenuItem
            // 
            this.AddObjectTypeToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Typ_32;
            this.AddObjectTypeToolStripMenuItem.Name = "AddObjectTypeToolStripMenuItem";
            resources.ApplyResources(this.AddObjectTypeToolStripMenuItem, "AddObjectTypeToolStripMenuItem");
            this.AddObjectTypeToolStripMenuItem.Tag = "addobjecttype";
            // 
            // AddJavascriptToolStripMenuItem
            // 
            this.AddJavascriptToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_JS_32;
            this.AddJavascriptToolStripMenuItem.Name = "AddJavascriptToolStripMenuItem";
            resources.ApplyResources(this.AddJavascriptToolStripMenuItem, "AddJavascriptToolStripMenuItem");
            this.AddJavascriptToolStripMenuItem.Tag = "addjavascript";
            // 
            // AddImpliedTypeToolStripMenuItem
            // 
            this.AddImpliedTypeToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Gleich_32;
            this.AddImpliedTypeToolStripMenuItem.Name = "AddImpliedTypeToolStripMenuItem";
            resources.ApplyResources(this.AddImpliedTypeToolStripMenuItem, "AddImpliedTypeToolStripMenuItem");
            this.AddImpliedTypeToolStripMenuItem.Tag = "addimpliedtype";
            // 
            // AddDelegateToolStripMenuItem
            // 
            this.AddDelegateToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Helfende_Hand_32;
            this.AddDelegateToolStripMenuItem.Name = "AddDelegateToolStripMenuItem";
            resources.ApplyResources(this.AddDelegateToolStripMenuItem, "AddDelegateToolStripMenuItem");
            this.AddDelegateToolStripMenuItem.Tag = "adddelegate";
            // 
            // ctlTreeView
            // 
            this.ctlTreeView.AllowDrop = true;
            this.ctlTreeView.BackColor = System.Drawing.Color.White;
            this.ctlTreeView.ContextMenuStrip = this.ctlContextMenu;
            resources.ApplyResources(this.ctlTreeView, "ctlTreeView");
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
            this.ctlImageList.Images.SetKeyName(0, "icons8-Mappe-16.png");
            this.ctlImageList.Images.SetKeyName(1, "icons8-Ordner öffnen-16.png");
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
            this.Controls.Add(this.ctlToolStrip);
            this.Controls.Add(this.lstSearchResults);
            this.Controls.Add(this.pnlSearchContainer);
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
        internal System.Windows.Forms.Button cmdClose;
        internal System.Windows.Forms.Button cmdSearch;
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
    }
}
