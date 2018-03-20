namespace TextAdventures.Quest.EditorControls
{
    partial class WFListEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WFListEditor));
            this.lstList = new System.Windows.Forms.ListView();
            this.ctlToolStrip = new System.Windows.Forms.ToolStrip();
            this.cmdAdd = new System.Windows.Forms.ToolStripButton();
            this.cmdAddNewPage = new System.Windows.Forms.ToolStripButton();
            this.cmdDelete = new System.Windows.Forms.ToolStripButton();
            this.cmdLink = new System.Windows.Forms.ToolStripButton();
            this.cmdEditKey = new System.Windows.Forms.ToolStripButton();
            this.cmdEdit = new System.Windows.Forms.ToolStripButton();
            this.cmdGoToPage = new System.Windows.Forms.ToolStripButton();
            this.cmdMoveUp = new System.Windows.Forms.ToolStripButton();
            this.cmdMoveDown = new System.Windows.Forms.ToolStripButton();
            this.ctlToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstList
            // 
            this.lstList.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this.lstList, "lstList");
            this.lstList.ForeColor = System.Drawing.Color.Black;
            this.lstList.GridLines = true;
            this.lstList.HideSelection = false;
            this.lstList.Name = "lstList";
            this.lstList.UseCompatibleStateImageBehavior = false;
            this.lstList.View = System.Windows.Forms.View.Details;
            // 
            // ctlToolStrip
            // 
            this.ctlToolStrip.BackColor = System.Drawing.Color.White;
            this.ctlToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.ctlToolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ctlToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmdAdd,
            this.cmdAddNewPage,
            this.cmdDelete,
            this.cmdEditKey,
            this.cmdEdit,
            this.cmdLink,
            this.cmdGoToPage,
            this.cmdMoveUp,
            this.cmdMoveDown});
            resources.ApplyResources(this.ctlToolStrip, "ctlToolStrip");
            this.ctlToolStrip.Name = "ctlToolStrip";
            // 
            // cmdAdd
            // 
            this.cmdAdd.AutoToolTip = false;
            this.cmdAdd.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_wf_add;
            resources.ApplyResources(this.cmdAdd, "cmdAdd");
            this.cmdAdd.Name = "cmdAdd";
            // 
            // cmdAddNewPage
            // 
            this.cmdAddNewPage.AutoToolTip = false;
            this.cmdAddNewPage.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_wf_add_page;
            resources.ApplyResources(this.cmdAddNewPage, "cmdAddNewPage");
            this.cmdAddNewPage.Name = "cmdAddNewPage";
            this.cmdAddNewPage.Tag = "addpage";
            // 
            // cmdDelete
            // 
            this.cmdDelete.AutoToolTip = false;
            this.cmdDelete.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_wf_delete;
            resources.ApplyResources(this.cmdDelete, "cmdDelete");
            this.cmdDelete.Name = "cmdDelete";
            // 
            // cmdLink
            // 
            this.cmdLink.AutoToolTip = false;
            this.cmdLink.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_wf_link;
            resources.ApplyResources(this.cmdLink, "cmdLink");
            this.cmdLink.Name = "cmdLink";
            this.cmdLink.Tag = "link";
            // 
            // cmdEditKey
            // 
            this.cmdEditKey.AutoToolTip = false;
            this.cmdEditKey.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_wf_rename_key;
            resources.ApplyResources(this.cmdEditKey, "cmdEditKey");
            this.cmdEditKey.Name = "cmdEditKey";
            // 
            // cmdEdit
            // 
            this.cmdEdit.AutoToolTip = false;
            this.cmdEdit.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_wf_options;
            resources.ApplyResources(this.cmdEdit, "cmdEdit");
            this.cmdEdit.Name = "cmdEdit";
            // 
            // cmdGoToPage
            // 
            this.cmdGoToPage.AutoToolTip = false;
            this.cmdGoToPage.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_wf_domain;
            resources.ApplyResources(this.cmdGoToPage, "cmdGoToPage");
            this.cmdGoToPage.Name = "cmdGoToPage";
            this.cmdGoToPage.Tag = "goto";
            // 
            // cmdMoveUp
            // 
            this.cmdMoveUp.AutoToolTip = false;
            this.cmdMoveUp.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_wf_up;
            resources.ApplyResources(this.cmdMoveUp, "cmdMoveUp");
            this.cmdMoveUp.Name = "cmdMoveUp";
            this.cmdMoveUp.Click += new System.EventHandler(this.cmdMoveUp_Click);
            // 
            // cmdMoveDown
            // 
            this.cmdMoveDown.AutoToolTip = false;
            this.cmdMoveDown.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.s_wf_down;
            resources.ApplyResources(this.cmdMoveDown, "cmdMoveDown");
            this.cmdMoveDown.Name = "cmdMoveDown";
            this.cmdMoveDown.Click += new System.EventHandler(this.cmdMoveDown_Click);
            // 
            // WFListEditor
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lstList);
            this.Controls.Add(this.ctlToolStrip);
            this.Name = "WFListEditor";
            this.ctlToolStrip.ResumeLayout(false);
            this.ctlToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.ListView lstList;
        internal System.Windows.Forms.ToolStrip ctlToolStrip;
        internal System.Windows.Forms.ToolStripButton cmdAdd;
        internal System.Windows.Forms.ToolStripButton cmdEdit;
        internal System.Windows.Forms.ToolStripButton cmdDelete;
        private System.Windows.Forms.ToolStripButton cmdMoveUp;
        private System.Windows.Forms.ToolStripButton cmdMoveDown;
        internal System.Windows.Forms.ToolStripButton cmdEditKey;
        private System.Windows.Forms.ToolStripButton cmdLink;
        private System.Windows.Forms.ToolStripButton cmdAddNewPage;
        private System.Windows.Forms.ToolStripButton cmdGoToPage;
    }
}
