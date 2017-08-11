namespace TextAdventures.Quest.EditorControls
{
    partial class WFToolbar
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WFToolbar));
            this.ctlToolStrip = new System.Windows.Forms.ToolStrip();
            this.cmdAdd = new System.Windows.Forms.ToolStripButton();
            this.cmdDelete = new System.Windows.Forms.ToolStripButton();
            this.cmdEdit = new System.Windows.Forms.ToolStripButton();
            this.cmdPlay = new System.Windows.Forms.ToolStripButton();
            this.cmdRecord = new System.Windows.Forms.ToolStripButton();
            this.cmdMoveUp = new System.Windows.Forms.ToolStripButton();
            this.cmdMoveDown = new System.Windows.Forms.ToolStripButton();
            this.ctlToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // ctlToolStrip
            // 
            this.ctlToolStrip.BackColor = System.Drawing.Color.White;
            this.ctlToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.ctlToolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ctlToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmdAdd,
            this.cmdDelete,
            this.cmdEdit,
            this.cmdPlay,
            this.cmdRecord,
            this.cmdMoveUp,
            this.cmdMoveDown});
            resources.ApplyResources(this.ctlToolStrip, "ctlToolStrip");
            this.ctlToolStrip.Name = "ctlToolStrip";
            // 
            // cmdAdd
            // 
            this.cmdAdd.AutoToolTip = false;
            this.cmdAdd.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Plus_16;
            resources.ApplyResources(this.cmdAdd, "cmdAdd");
            this.cmdAdd.Name = "cmdAdd";
            this.cmdAdd.Click += new System.EventHandler(this.cmdAdd_Click);
            // 
            // cmdDelete
            // 
            this.cmdDelete.AutoToolTip = false;
            this.cmdDelete.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Minus_16;
            resources.ApplyResources(this.cmdDelete, "cmdDelete");
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // cmdEdit
            // 
            this.cmdEdit.AutoToolTip = false;
            this.cmdEdit.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Eigenschaft_bearbeiten_16;
            resources.ApplyResources(this.cmdEdit, "cmdEdit");
            this.cmdEdit.Name = "cmdEdit";
            this.cmdEdit.Click += new System.EventHandler(this.cmdEdit_Click);
            // 
            // cmdPlay
            // 
            this.cmdPlay.AutoToolTip = false;
            this.cmdPlay.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Spielen_16;
            resources.ApplyResources(this.cmdPlay, "cmdPlay");
            this.cmdPlay.Name = "cmdPlay";
            this.cmdPlay.Click += new System.EventHandler(this.cmdPlay_Click);
            // 
            // cmdRecord
            // 
            this.cmdRecord.AutoToolTip = false;
            this.cmdRecord.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Aufzeichnen_16;
            resources.ApplyResources(this.cmdRecord, "cmdRecord");
            this.cmdRecord.Name = "cmdRecord";
            this.cmdRecord.Click += new System.EventHandler(this.cmdRecord_Click);
            // 
            // cmdMoveUp
            // 
            this.cmdMoveUp.AutoToolTip = false;
            this.cmdMoveUp.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Oben_16;
            resources.ApplyResources(this.cmdMoveUp, "cmdMoveUp");
            this.cmdMoveUp.Name = "cmdMoveUp";
            this.cmdMoveUp.Click += new System.EventHandler(this.cmdMoveUp_Click);
            // 
            // cmdMoveDown
            // 
            this.cmdMoveDown.AutoToolTip = false;
            this.cmdMoveDown.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Unten_16;
            resources.ApplyResources(this.cmdMoveDown, "cmdMoveDown");
            this.cmdMoveDown.Name = "cmdMoveDown";
            this.cmdMoveDown.Click += new System.EventHandler(this.cmdMoveDown_Click);
            // 
            // WFToolbar
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ctlToolStrip);
            this.Name = "WFToolbar";
            this.ctlToolStrip.ResumeLayout(false);
            this.ctlToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.ToolStrip ctlToolStrip;
        internal System.Windows.Forms.ToolStripButton cmdAdd;
        internal System.Windows.Forms.ToolStripButton cmdEdit;
        internal System.Windows.Forms.ToolStripButton cmdDelete;
        private System.Windows.Forms.ToolStripButton cmdPlay;
        private System.Windows.Forms.ToolStripButton cmdRecord;
        private System.Windows.Forms.ToolStripButton cmdMoveUp;
        private System.Windows.Forms.ToolStripButton cmdMoveDown;

    }
}
