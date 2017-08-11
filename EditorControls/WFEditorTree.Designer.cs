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
            this.txtSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSearch.ForeColor = System.Drawing.Color.Black;
            this.txtSearch.Location = new System.Drawing.Point(0, 0);
            this.txtSearch.Margin = new System.Windows.Forms.Padding(4);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(385, 22);
            this.txtSearch.TabIndex = 1;
            // 
            // ctlToolStrip
            // 
            this.ctlToolStrip.AutoSize = false;
            this.ctlToolStrip.BackColor = System.Drawing.Color.GhostWhite;
            this.ctlToolStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ctlToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.ctlToolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ctlToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFilter});
            this.ctlToolStrip.Location = new System.Drawing.Point(0, 390);
            this.ctlToolStrip.Name = "ctlToolStrip";
            this.ctlToolStrip.Padding = new System.Windows.Forms.Padding(0);
            this.ctlToolStrip.Size = new System.Drawing.Size(385, 25);
            this.ctlToolStrip.TabIndex = 5;
            this.ctlToolStrip.Text = "ToolStrip1";
            // 
            // mnuFilter
            // 
            this.mnuFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.mnuFilter.Image = ((System.Drawing.Image)(resources.GetObject("mnuFilter.Image")));
            this.mnuFilter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnuFilter.Name = "mnuFilter";
            this.mnuFilter.Size = new System.Drawing.Size(56, 22);
            this.mnuFilter.Text = "Filter";
            // 
            // lstSearchResults
            // 
            this.lstSearchResults.BackColor = System.Drawing.Color.White;
            this.lstSearchResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colSearchResults});
            this.lstSearchResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstSearchResults.ForeColor = System.Drawing.Color.Black;
            this.lstSearchResults.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lstSearchResults.HideSelection = false;
            this.lstSearchResults.Location = new System.Drawing.Point(0, 20);
            this.lstSearchResults.Margin = new System.Windows.Forms.Padding(4);
            this.lstSearchResults.MultiSelect = false;
            this.lstSearchResults.Name = "lstSearchResults";
            this.lstSearchResults.Size = new System.Drawing.Size(385, 395);
            this.lstSearchResults.TabIndex = 8;
            this.lstSearchResults.UseCompatibleStateImageBehavior = false;
            this.lstSearchResults.View = System.Windows.Forms.View.Details;
            this.lstSearchResults.Visible = false;
            // 
            // colSearchResults
            // 
            this.colSearchResults.Text = "Search Results";
            // 
            // pnlSearchContainer
            // 
            this.pnlSearchContainer.Controls.Add(this.cmdClose);
            this.pnlSearchContainer.Controls.Add(this.cmdSearch);
            this.pnlSearchContainer.Controls.Add(this.txtSearch);
            this.pnlSearchContainer.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSearchContainer.Location = new System.Drawing.Point(0, 0);
            this.pnlSearchContainer.Margin = new System.Windows.Forms.Padding(4);
            this.pnlSearchContainer.Name = "pnlSearchContainer";
            this.pnlSearchContainer.Size = new System.Drawing.Size(385, 20);
            this.pnlSearchContainer.TabIndex = 6;
            // 
            // cmdClose
            // 
            this.cmdClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(27)))), ((int)(((byte)(97)))));
            this.cmdClose.BackgroundImage = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Löschen_16__1_;
            this.cmdClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.cmdClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.cmdClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdClose.Location = new System.Drawing.Point(345, 0);
            this.cmdClose.Margin = new System.Windows.Forms.Padding(4);
            this.cmdClose.Name = "cmdClose";
            this.cmdClose.Size = new System.Drawing.Size(20, 20);
            this.cmdClose.TabIndex = 2;
            this.cmdClose.UseVisualStyleBackColor = false;
            this.cmdClose.Visible = false;
            // 
            // cmdSearch
            // 
            this.cmdSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(29)))), ((int)(((byte)(27)))), ((int)(((byte)(97)))));
            this.cmdSearch.BackgroundImage = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Suche_16;
            this.cmdSearch.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.cmdSearch.Dock = System.Windows.Forms.DockStyle.Right;
            this.cmdSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmdSearch.Location = new System.Drawing.Point(365, 0);
            this.cmdSearch.Margin = new System.Windows.Forms.Padding(4);
            this.cmdSearch.Name = "cmdSearch";
            this.cmdSearch.Size = new System.Drawing.Size(20, 20);
            this.cmdSearch.TabIndex = 3;
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
            this.ctlContextMenu.Size = new System.Drawing.Size(283, 580);
            // 
            // expandAllToolStripMenuItem
            // 
            this.expandAllToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Erweitern_32;
            this.expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
            this.expandAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Right)));
            this.expandAllToolStripMenuItem.Size = new System.Drawing.Size(282, 26);
            this.expandAllToolStripMenuItem.Tag = "expandall";
            this.expandAllToolStripMenuItem.Text = "Expand All";
            // 
            // collapseAllToolStripMenuItem
            // 
            this.collapseAllToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Verkleinern_32;
            this.collapseAllToolStripMenuItem.Name = "collapseAllToolStripMenuItem";
            this.collapseAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Left)));
            this.collapseAllToolStripMenuItem.Size = new System.Drawing.Size(282, 26);
            this.collapseAllToolStripMenuItem.Tag = "collapseall";
            this.collapseAllToolStripMenuItem.Text = "Collapse All";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(279, 6);
            this.toolStripSeparator1.Tag = "separator1";
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Löschen_32;
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(282, 26);
            this.deleteToolStripMenuItem.Tag = "delete";
            this.deleteToolStripMenuItem.Text = "Delete";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(279, 6);
            this.toolStripSeparator2.Tag = "separator2";
            // 
            // AddVerbToolStripMenuItem
            // 
            this.AddVerbToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Verb_32;
            this.AddVerbToolStripMenuItem.Name = "AddVerbToolStripMenuItem";
            this.AddVerbToolStripMenuItem.Size = new System.Drawing.Size(282, 26);
            this.AddVerbToolStripMenuItem.Tag = "addverb";
            this.AddVerbToolStripMenuItem.Text = "+ Verb";
            // 
            // AddCommandToolStripMenuItem
            // 
            this.AddCommandToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Konsole_32;
            this.AddCommandToolStripMenuItem.Name = "AddCommandToolStripMenuItem";
            this.AddCommandToolStripMenuItem.Size = new System.Drawing.Size(282, 26);
            this.AddCommandToolStripMenuItem.Tag = "addcommand";
            this.AddCommandToolStripMenuItem.Text = "+ Command";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(279, 6);
            this.toolStripSeparator3.Tag = "separator3";
            // 
            // addPageToolStripMenuItem
            // 
            this.addPageToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Datei_hinzufügen_32;
            this.addPageToolStripMenuItem.Name = "addPageToolStripMenuItem";
            this.addPageToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Insert;
            this.addPageToolStripMenuItem.Size = new System.Drawing.Size(282, 26);
            this.addPageToolStripMenuItem.Tag = "addpage";
            this.addPageToolStripMenuItem.Text = "+ Site";
            this.addPageToolStripMenuItem.Visible = false;
            // 
            // AddEditorToolStripMenuItem
            // 
            this.AddEditorToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Bearbeiten_32;
            this.AddEditorToolStripMenuItem.Name = "AddEditorToolStripMenuItem";
            this.AddEditorToolStripMenuItem.Size = new System.Drawing.Size(282, 26);
            this.AddEditorToolStripMenuItem.Tag = "addeditor";
            this.AddEditorToolStripMenuItem.Text = "+ Editor";
            this.AddEditorToolStripMenuItem.Visible = false;
            // 
            // AddRoomToolStripMenuItem
            // 
            this.AddRoomToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Zimmer_32;
            this.AddRoomToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Silver;
            this.AddRoomToolStripMenuItem.Name = "AddRoomToolStripMenuItem";
            this.AddRoomToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Insert)));
            this.AddRoomToolStripMenuItem.Size = new System.Drawing.Size(282, 26);
            this.AddRoomToolStripMenuItem.Tag = "addroom";
            this.AddRoomToolStripMenuItem.Text = "+ Room";
            // 
            // AddObjectToolStripMenuItem
            // 
            this.AddObjectToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Topfpflanze_32;
            this.AddObjectToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Silver;
            this.AddObjectToolStripMenuItem.Name = "AddObjectToolStripMenuItem";
            this.AddObjectToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Insert;
            this.AddObjectToolStripMenuItem.Size = new System.Drawing.Size(282, 26);
            this.AddObjectToolStripMenuItem.Tag = "addobject";
            this.AddObjectToolStripMenuItem.Text = "+ Object";
            // 
            // AddExitToolStripMenuItem
            // 
            this.AddExitToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Ausgang_32;
            this.AddExitToolStripMenuItem.Name = "AddExitToolStripMenuItem";
            this.AddExitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Insert)));
            this.AddExitToolStripMenuItem.Size = new System.Drawing.Size(282, 26);
            this.AddExitToolStripMenuItem.Tag = "addexit";
            this.AddExitToolStripMenuItem.Text = "+ Exit";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(279, 6);
            this.toolStripSeparator4.Tag = "separator4";
            // 
            // addTurnScriptToolStripMenuItem
            // 
            this.addTurnScriptToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Bauer_32;
            this.addTurnScriptToolStripMenuItem.Name = "addTurnScriptToolStripMenuItem";
            this.addTurnScriptToolStripMenuItem.Size = new System.Drawing.Size(282, 26);
            this.addTurnScriptToolStripMenuItem.Tag = "addturnscript";
            this.addTurnScriptToolStripMenuItem.Text = "+ Turn Script";
            // 
            // AddFunctionToolStripMenuItem
            // 
            this.AddFunctionToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Variable_32;
            this.AddFunctionToolStripMenuItem.Name = "AddFunctionToolStripMenuItem";
            this.AddFunctionToolStripMenuItem.Size = new System.Drawing.Size(282, 26);
            this.AddFunctionToolStripMenuItem.Tag = "addfunction";
            this.AddFunctionToolStripMenuItem.Text = "+ Function";
            // 
            // addTimerToolStripMenuItem
            // 
            this.addTimerToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Timer_32;
            this.addTimerToolStripMenuItem.Name = "addTimerToolStripMenuItem";
            this.addTimerToolStripMenuItem.Size = new System.Drawing.Size(282, 26);
            this.addTimerToolStripMenuItem.Tag = "addtimer";
            this.addTimerToolStripMenuItem.Text = "+ Timer";
            // 
            // AddWalkthroughToolStripMenuItem
            // 
            this.AddWalkthroughToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Gehen_32;
            this.AddWalkthroughToolStripMenuItem.Name = "AddWalkthroughToolStripMenuItem";
            this.AddWalkthroughToolStripMenuItem.Size = new System.Drawing.Size(282, 26);
            this.AddWalkthroughToolStripMenuItem.Tag = "addwalkthrough";
            this.AddWalkthroughToolStripMenuItem.Text = "+ Walkthrough";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(279, 6);
            this.toolStripSeparator5.Tag = "separator5";
            // 
            // AddLibraryToolStripMenuItem
            // 
            this.AddLibraryToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Bücherregal_32;
            this.AddLibraryToolStripMenuItem.Name = "AddLibraryToolStripMenuItem";
            this.AddLibraryToolStripMenuItem.Size = new System.Drawing.Size(282, 26);
            this.AddLibraryToolStripMenuItem.Tag = "addlibrary";
            this.AddLibraryToolStripMenuItem.Text = "+ Library";
            // 
            // AddTemplateToolStripMenuItem
            // 
            this.AddTemplateToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Lebenslauf_Muster_32;
            this.AddTemplateToolStripMenuItem.Name = "AddTemplateToolStripMenuItem";
            this.AddTemplateToolStripMenuItem.Size = new System.Drawing.Size(282, 26);
            this.AddTemplateToolStripMenuItem.Tag = "addtemplate";
            this.AddTemplateToolStripMenuItem.Text = "+ Template";
            // 
            // AddDynamicTemplateToolStripMenuItem
            // 
            this.AddDynamicTemplateToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Lebenslauf_Vorlage_laden_32;
            this.AddDynamicTemplateToolStripMenuItem.Name = "AddDynamicTemplateToolStripMenuItem";
            this.AddDynamicTemplateToolStripMenuItem.Size = new System.Drawing.Size(282, 26);
            this.AddDynamicTemplateToolStripMenuItem.Tag = "adddynamictemplate";
            this.AddDynamicTemplateToolStripMenuItem.Text = "+ Dynamic Template";
            // 
            // AddObjectTypeToolStripMenuItem
            // 
            this.AddObjectTypeToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Typ_32;
            this.AddObjectTypeToolStripMenuItem.Name = "AddObjectTypeToolStripMenuItem";
            this.AddObjectTypeToolStripMenuItem.Size = new System.Drawing.Size(282, 26);
            this.AddObjectTypeToolStripMenuItem.Tag = "addobjecttype";
            this.AddObjectTypeToolStripMenuItem.Text = "+ Object Type";
            // 
            // AddJavascriptToolStripMenuItem
            // 
            this.AddJavascriptToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_JS_32;
            this.AddJavascriptToolStripMenuItem.Name = "AddJavascriptToolStripMenuItem";
            this.AddJavascriptToolStripMenuItem.Size = new System.Drawing.Size(282, 26);
            this.AddJavascriptToolStripMenuItem.Tag = "addjavascript";
            this.AddJavascriptToolStripMenuItem.Text = "+ Javascript";
            // 
            // AddImpliedTypeToolStripMenuItem
            // 
            this.AddImpliedTypeToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Gleich_32;
            this.AddImpliedTypeToolStripMenuItem.Name = "AddImpliedTypeToolStripMenuItem";
            this.AddImpliedTypeToolStripMenuItem.Size = new System.Drawing.Size(282, 26);
            this.AddImpliedTypeToolStripMenuItem.Tag = "addimpliedtype";
            this.AddImpliedTypeToolStripMenuItem.Text = "+ Implied Type";
            this.AddImpliedTypeToolStripMenuItem.Visible = false;
            // 
            // AddDelegateToolStripMenuItem
            // 
            this.AddDelegateToolStripMenuItem.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Helfende_Hand_32;
            this.AddDelegateToolStripMenuItem.Name = "AddDelegateToolStripMenuItem";
            this.AddDelegateToolStripMenuItem.Size = new System.Drawing.Size(282, 26);
            this.AddDelegateToolStripMenuItem.Tag = "adddelegate";
            this.AddDelegateToolStripMenuItem.Text = "+ Delegate";
            this.AddDelegateToolStripMenuItem.Visible = false;
            // 
            // ctlTreeView
            // 
            this.ctlTreeView.AllowDrop = true;
            this.ctlTreeView.BackColor = System.Drawing.Color.White;
            this.ctlTreeView.ContextMenuStrip = this.ctlContextMenu;
            this.ctlTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlTreeView.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ctlTreeView.ForeColor = System.Drawing.Color.Black;
            this.ctlTreeView.HideSelection = false;
            this.ctlTreeView.ImageIndex = 0;
            this.ctlTreeView.ImageList = this.ctlImageList;
            this.ctlTreeView.LineColor = System.Drawing.Color.Silver;
            this.ctlTreeView.Location = new System.Drawing.Point(0, 20);
            this.ctlTreeView.Margin = new System.Windows.Forms.Padding(4);
            this.ctlTreeView.Name = "ctlTreeView";
            this.ctlTreeView.SelectedImageIndex = 1;
            this.ctlTreeView.Size = new System.Drawing.Size(385, 370);
            this.ctlTreeView.TabIndex = 7;
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
            this.roomToolStripMenuItem.Size = new System.Drawing.Size(180, 26);
            this.roomToolStripMenuItem.Text = "Room";
            // 
            // WFEditorTree
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.GhostWhite;
            this.Controls.Add(this.ctlTreeView);
            this.Controls.Add(this.ctlToolStrip);
            this.Controls.Add(this.lstSearchResults);
            this.Controls.Add(this.pnlSearchContainer);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "WFEditorTree";
            this.Size = new System.Drawing.Size(385, 415);
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
