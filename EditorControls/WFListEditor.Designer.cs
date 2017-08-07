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
            this.lstList = new System.Windows.Forms.ListView();
            this.ctlToolStrip = new System.Windows.Forms.ToolStrip();
            this.cmdAdd = new System.Windows.Forms.ToolStripButton();
            this.cmdDelete = new System.Windows.Forms.ToolStripButton();
            this.cmdAddNewPage = new System.Windows.Forms.ToolStripButton();
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
            this.lstList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstList.ForeColor = System.Drawing.Color.Black;
            this.lstList.GridLines = true;
            this.lstList.HideSelection = false;
            this.lstList.Location = new System.Drawing.Point(0, 25);
            this.lstList.Margin = new System.Windows.Forms.Padding(4);
            this.lstList.Name = "lstList";
            this.lstList.Size = new System.Drawing.Size(1076, 310);
            this.lstList.TabIndex = 5;
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
            this.cmdDelete,
            this.cmdAddNewPage,
            this.cmdLink,
            this.cmdEditKey,
            this.cmdEdit,
            this.cmdGoToPage,
            this.cmdMoveUp,
            this.cmdMoveDown});
            this.ctlToolStrip.Location = new System.Drawing.Point(0, 0);
            this.ctlToolStrip.Name = "ctlToolStrip";
            this.ctlToolStrip.Size = new System.Drawing.Size(1076, 25);
            this.ctlToolStrip.TabIndex = 4;
            this.ctlToolStrip.Text = "ToolStrip1";
            // 
            // cmdAdd
            // 
            this.cmdAdd.AutoToolTip = false;
            this.cmdAdd.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Plus_16;
            this.cmdAdd.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.cmdAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdAdd.Name = "cmdAdd";
            this.cmdAdd.Size = new System.Drawing.Size(23, 22);
            this.cmdAdd.ToolTipText = "Add";
            // 
            // cmdDelete
            // 
            this.cmdDelete.AutoToolTip = false;
            this.cmdDelete.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Minus_16;
            this.cmdDelete.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.cmdDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(23, 22);
            this.cmdDelete.ToolTipText = "Delete";
            // 
            // cmdAddNewPage
            // 
            this.cmdAddNewPage.AutoToolTip = false;
            this.cmdAddNewPage.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Datei_hinzufügen_16;
            this.cmdAddNewPage.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.cmdAddNewPage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdAddNewPage.Name = "cmdAddNewPage";
            this.cmdAddNewPage.Size = new System.Drawing.Size(23, 22);
            this.cmdAddNewPage.Tag = "addpage";
            this.cmdAddNewPage.ToolTipText = "Add New Page";
            // 
            // cmdLink
            // 
            this.cmdLink.AutoToolTip = false;
            this.cmdLink.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Link_hinzufügen_16;
            this.cmdLink.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.cmdLink.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdLink.Name = "cmdLink";
            this.cmdLink.Size = new System.Drawing.Size(23, 22);
            this.cmdLink.Tag = "link";
            this.cmdLink.ToolTipText = "Link to Existing Page";
            // 
            // cmdEditKey
            // 
            this.cmdEditKey.AutoToolTip = false;
            this.cmdEditKey.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Umbenennen_16;
            this.cmdEditKey.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.cmdEditKey.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdEditKey.Name = "cmdEditKey";
            this.cmdEditKey.Size = new System.Drawing.Size(23, 22);
            this.cmdEditKey.ToolTipText = "Edit Key";
            // 
            // cmdEdit
            // 
            this.cmdEdit.AutoToolTip = false;
            this.cmdEdit.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Eigenschaft_bearbeiten_16;
            this.cmdEdit.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.cmdEdit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdEdit.Name = "cmdEdit";
            this.cmdEdit.Size = new System.Drawing.Size(23, 22);
            this.cmdEdit.ToolTipText = "Edit";
            // 
            // cmdGoToPage
            // 
            this.cmdGoToPage.AutoToolTip = false;
            this.cmdGoToPage.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Domain_16;
            this.cmdGoToPage.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.cmdGoToPage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdGoToPage.Name = "cmdGoToPage";
            this.cmdGoToPage.Size = new System.Drawing.Size(23, 22);
            this.cmdGoToPage.Tag = "goto";
            this.cmdGoToPage.ToolTipText = "Go to Page";
            // 
            // cmdMoveUp
            // 
            this.cmdMoveUp.AutoToolTip = false;
            this.cmdMoveUp.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Oben_16;
            this.cmdMoveUp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.cmdMoveUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdMoveUp.Name = "cmdMoveUp";
            this.cmdMoveUp.Size = new System.Drawing.Size(23, 22);
            this.cmdMoveUp.ToolTipText = "Move Up";
            this.cmdMoveUp.Click += new System.EventHandler(this.cmdMoveUp_Click);
            // 
            // cmdMoveDown
            // 
            this.cmdMoveDown.AutoToolTip = false;
            this.cmdMoveDown.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Unten_16;
            this.cmdMoveDown.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.cmdMoveDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdMoveDown.Name = "cmdMoveDown";
            this.cmdMoveDown.Size = new System.Drawing.Size(23, 22);
            this.cmdMoveDown.ToolTipText = "Move Down";
            this.cmdMoveDown.Click += new System.EventHandler(this.cmdMoveDown_Click);
            // 
            // WFListEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lstList);
            this.Controls.Add(this.ctlToolStrip);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "WFListEditor";
            this.Size = new System.Drawing.Size(1076, 335);
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
