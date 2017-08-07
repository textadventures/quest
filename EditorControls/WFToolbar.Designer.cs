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
            this.ctlToolStrip.Location = new System.Drawing.Point(0, 0);
            this.ctlToolStrip.Name = "ctlToolStrip";
            this.ctlToolStrip.Size = new System.Drawing.Size(660, 25);
            this.ctlToolStrip.TabIndex = 3;
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
            this.cmdAdd.Click += new System.EventHandler(this.cmdAdd_Click);
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
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
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
            this.cmdEdit.Click += new System.EventHandler(this.cmdEdit_Click);
            // 
            // cmdPlay
            // 
            this.cmdPlay.AutoToolTip = false;
            this.cmdPlay.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Spielen_16;
            this.cmdPlay.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.cmdPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdPlay.Name = "cmdPlay";
            this.cmdPlay.Size = new System.Drawing.Size(23, 22);
            this.cmdPlay.ToolTipText = "Play";
            this.cmdPlay.Click += new System.EventHandler(this.cmdPlay_Click);
            // 
            // cmdRecord
            // 
            this.cmdRecord.AutoToolTip = false;
            this.cmdRecord.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Aufzeichnen_16;
            this.cmdRecord.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.cmdRecord.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdRecord.Name = "cmdRecord";
            this.cmdRecord.Size = new System.Drawing.Size(23, 22);
            this.cmdRecord.ToolTipText = "Record";
            this.cmdRecord.Click += new System.EventHandler(this.cmdRecord_Click);
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
            // WFToolbar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ctlToolStrip);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "WFToolbar";
            this.Size = new System.Drawing.Size(660, 32);
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
