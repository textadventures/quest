namespace AxeSoftware.Quest.EditorControls
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
            this.cmdEdit = new System.Windows.Forms.ToolStripButton();
            this.cmdDelete = new System.Windows.Forms.ToolStripButton();
            this.cmdPlay = new System.Windows.Forms.ToolStripButton();
            this.cmdRecord = new System.Windows.Forms.ToolStripButton();
            this.ctlToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // ctlToolStrip
            // 
            this.ctlToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmdAdd,
            this.cmdEdit,
            this.cmdDelete,
            this.cmdPlay,
            this.cmdRecord});
            this.ctlToolStrip.Location = new System.Drawing.Point(0, 0);
            this.ctlToolStrip.Name = "ctlToolStrip";
            this.ctlToolStrip.Size = new System.Drawing.Size(404, 25);
            this.ctlToolStrip.TabIndex = 3;
            this.ctlToolStrip.Text = "ToolStrip1";
            // 
            // cmdAdd
            // 
            this.cmdAdd.Image = ((System.Drawing.Image)(resources.GetObject("cmdAdd.Image")));
            this.cmdAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdAdd.Name = "cmdAdd";
            this.cmdAdd.Size = new System.Drawing.Size(49, 22);
            this.cmdAdd.Text = "Add";
            this.cmdAdd.Click += new System.EventHandler(this.cmdAdd_Click);
            // 
            // cmdEdit
            // 
            this.cmdEdit.Image = ((System.Drawing.Image)(resources.GetObject("cmdEdit.Image")));
            this.cmdEdit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdEdit.Name = "cmdEdit";
            this.cmdEdit.Size = new System.Drawing.Size(47, 22);
            this.cmdEdit.Text = "Edit";
            this.cmdEdit.Click += new System.EventHandler(this.cmdEdit_Click);
            // 
            // cmdDelete
            // 
            this.cmdDelete.Image = ((System.Drawing.Image)(resources.GetObject("cmdDelete.Image")));
            this.cmdDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(60, 22);
            this.cmdDelete.Text = "Delete";
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // cmdPlay
            // 
            this.cmdPlay.Image = ((System.Drawing.Image)(resources.GetObject("cmdPlay.Image")));
            this.cmdPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdPlay.Name = "cmdPlay";
            this.cmdPlay.Size = new System.Drawing.Size(49, 22);
            this.cmdPlay.Text = "Play";
            this.cmdPlay.Click += new System.EventHandler(this.cmdPlay_Click);
            // 
            // cmdRecord
            // 
            this.cmdRecord.Image = ((System.Drawing.Image)(resources.GetObject("cmdRecord.Image")));
            this.cmdRecord.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdRecord.Name = "cmdRecord";
            this.cmdRecord.Size = new System.Drawing.Size(64, 22);
            this.cmdRecord.Text = "Record";
            this.cmdRecord.Click += new System.EventHandler(this.cmdRecord_Click);
            // 
            // WFToolbar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ctlToolStrip);
            this.Name = "WFToolbar";
            this.Size = new System.Drawing.Size(404, 26);
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

    }
}
