namespace TextAdventures.Quest.EditorControls
{
    partial class WFRichTextControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WFRichTextControl));
            this.ctlWebBrowser = new System.Windows.Forms.WebBrowser();
            this.ctlToolbar = new System.Windows.Forms.ToolStrip();
            this.butBold = new System.Windows.Forms.ToolStripButton();
            this.butItalic = new System.Windows.Forms.ToolStripButton();
            this.butUnderline = new System.Windows.Forms.ToolStripButton();
            this.ctlToolbar.SuspendLayout();
            this.SuspendLayout();
            // 
            // ctlWebBrowser
            // 
            this.ctlWebBrowser.AllowWebBrowserDrop = false;
            this.ctlWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlWebBrowser.IsWebBrowserContextMenuEnabled = false;
            this.ctlWebBrowser.Location = new System.Drawing.Point(0, 25);
            this.ctlWebBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.ctlWebBrowser.Name = "ctlWebBrowser";
            this.ctlWebBrowser.Size = new System.Drawing.Size(381, 220);
            this.ctlWebBrowser.TabIndex = 4;
            // 
            // ctlToolbar
            // 
            this.ctlToolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.butBold,
            this.butItalic,
            this.butUnderline});
            this.ctlToolbar.Location = new System.Drawing.Point(0, 0);
            this.ctlToolbar.Name = "ctlToolbar";
            this.ctlToolbar.Size = new System.Drawing.Size(381, 25);
            this.ctlToolbar.TabIndex = 5;
            this.ctlToolbar.Text = "ToolStrip1";
            // 
            // butBold
            // 
            this.butBold.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butBold.Image = ((System.Drawing.Image)(resources.GetObject("butBold.Image")));
            this.butBold.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butBold.Name = "butBold";
            this.butBold.Size = new System.Drawing.Size(23, 22);
            this.butBold.Text = "Bold";
            // 
            // butItalic
            // 
            this.butItalic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butItalic.Image = ((System.Drawing.Image)(resources.GetObject("butItalic.Image")));
            this.butItalic.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butItalic.Name = "butItalic";
            this.butItalic.Size = new System.Drawing.Size(23, 22);
            this.butItalic.Text = "Italic";
            // 
            // butUnderline
            // 
            this.butUnderline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butUnderline.Image = ((System.Drawing.Image)(resources.GetObject("butUnderline.Image")));
            this.butUnderline.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butUnderline.Name = "butUnderline";
            this.butUnderline.Size = new System.Drawing.Size(23, 22);
            this.butUnderline.Text = "Underline";
            // 
            // WFRichTextControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ctlWebBrowser);
            this.Controls.Add(this.ctlToolbar);
            this.Name = "WFRichTextControl";
            this.Size = new System.Drawing.Size(381, 245);
            this.ctlToolbar.ResumeLayout(false);
            this.ctlToolbar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.WebBrowser ctlWebBrowser;
        internal System.Windows.Forms.ToolStrip ctlToolbar;
        internal System.Windows.Forms.ToolStripButton butBold;
        internal System.Windows.Forms.ToolStripButton butItalic;
        internal System.Windows.Forms.ToolStripButton butUnderline;
    }
}
