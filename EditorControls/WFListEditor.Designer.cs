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
            this.cmdLink = new System.Windows.Forms.ToolStripButton();
            this.cmdEditKey = new System.Windows.Forms.ToolStripButton();
            this.cmdEdit = new System.Windows.Forms.ToolStripButton();
            this.cmdDelete = new System.Windows.Forms.ToolStripButton();
            this.cmdMoveUp = new System.Windows.Forms.ToolStripButton();
            this.cmdMoveDown = new System.Windows.Forms.ToolStripButton();
            this.cmdGoToPage = new System.Windows.Forms.ToolStripButton();
            this.ctlToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstList
            // 
            this.lstList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstList.HideSelection = false;
            this.lstList.Location = new System.Drawing.Point(0, 25);
            this.lstList.Name = "lstList";
            this.lstList.Size = new System.Drawing.Size(807, 247);
            this.lstList.TabIndex = 5;
            this.lstList.UseCompatibleStateImageBehavior = false;
            this.lstList.View = System.Windows.Forms.View.Details;
            // 
            // ctlToolStrip
            // 
            this.ctlToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmdAdd,
            this.cmdAddNewPage,
            this.cmdLink,
            this.cmdEditKey,
            this.cmdEdit,
            this.cmdDelete,
            this.cmdGoToPage,
            this.cmdMoveUp,
            this.cmdMoveDown});
            this.ctlToolStrip.Location = new System.Drawing.Point(0, 0);
            this.ctlToolStrip.Name = "ctlToolStrip";
            this.ctlToolStrip.Size = new System.Drawing.Size(807, 25);
            this.ctlToolStrip.TabIndex = 4;
            this.ctlToolStrip.Text = "ToolStrip1";
            // 
            // cmdAdd
            // 
            this.cmdAdd.Image = ((System.Drawing.Image)(resources.GetObject("cmdAdd.Image")));
            this.cmdAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdAdd.Name = "cmdAdd";
            this.cmdAdd.Size = new System.Drawing.Size(49, 22);
            this.cmdAdd.Text = "Add";
            // 
            // cmdAddNewPage
            // 
            this.cmdAddNewPage.Image = ((System.Drawing.Image)(resources.GetObject("cmdAddNewPage.Image")));
            this.cmdAddNewPage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdAddNewPage.Name = "cmdAddNewPage";
            this.cmdAddNewPage.Size = new System.Drawing.Size(105, 22);
            this.cmdAddNewPage.Tag = "addpage";
            this.cmdAddNewPage.Text = "Add New Page";
            // 
            // cmdLink
            // 
            this.cmdLink.Image = ((System.Drawing.Image)(resources.GetObject("cmdLink.Image")));
            this.cmdLink.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdLink.Name = "cmdLink";
            this.cmdLink.Size = new System.Drawing.Size(135, 22);
            this.cmdLink.Tag = "link";
            this.cmdLink.Text = "Link to Existing Page";
            // 
            // cmdEditKey
            // 
            this.cmdEditKey.Image = ((System.Drawing.Image)(resources.GetObject("cmdEditKey.Image")));
            this.cmdEditKey.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdEditKey.Name = "cmdEditKey";
            this.cmdEditKey.Size = new System.Drawing.Size(69, 22);
            this.cmdEditKey.Text = "Edit Key";
            // 
            // cmdEdit
            // 
            this.cmdEdit.Image = ((System.Drawing.Image)(resources.GetObject("cmdEdit.Image")));
            this.cmdEdit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdEdit.Name = "cmdEdit";
            this.cmdEdit.Size = new System.Drawing.Size(47, 22);
            this.cmdEdit.Text = "Edit";
            // 
            // cmdDelete
            // 
            this.cmdDelete.Image = ((System.Drawing.Image)(resources.GetObject("cmdDelete.Image")));
            this.cmdDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(60, 22);
            this.cmdDelete.Text = "Delete";
            // 
            // cmdMoveUp
            // 
            this.cmdMoveUp.Image = ((System.Drawing.Image)(resources.GetObject("cmdMoveUp.Image")));
            this.cmdMoveUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdMoveUp.Name = "cmdMoveUp";
            this.cmdMoveUp.Size = new System.Drawing.Size(75, 22);
            this.cmdMoveUp.Text = "Move Up";
            this.cmdMoveUp.Click += new System.EventHandler(this.cmdMoveUp_Click);
            // 
            // cmdMoveDown
            // 
            this.cmdMoveDown.Image = ((System.Drawing.Image)(resources.GetObject("cmdMoveDown.Image")));
            this.cmdMoveDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdMoveDown.Name = "cmdMoveDown";
            this.cmdMoveDown.Size = new System.Drawing.Size(91, 22);
            this.cmdMoveDown.Text = "Move Down";
            this.cmdMoveDown.Click += new System.EventHandler(this.cmdMoveDown_Click);
            // 
            // cmdGoToPage
            // 
            this.cmdGoToPage.Image = ((System.Drawing.Image)(resources.GetObject("cmdGoToPage.Image")));
            this.cmdGoToPage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdGoToPage.Name = "cmdGoToPage";
            this.cmdGoToPage.Size = new System.Drawing.Size(85, 22);
            this.cmdGoToPage.Tag = "goto";
            this.cmdGoToPage.Text = "Go to Page";
            // 
            // WFListEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lstList);
            this.Controls.Add(this.ctlToolStrip);
            this.Name = "WFListEditor";
            this.Size = new System.Drawing.Size(807, 272);
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
