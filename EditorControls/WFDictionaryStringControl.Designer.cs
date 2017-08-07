namespace TextAdventures.Quest.EditorControls
{
    partial class WFDictionaryStringControl
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
            this.ctlListEditor = new TextAdventures.Quest.EditorControls.WFListEditor();
            this.SuspendLayout();
            // 
            // ctlListEditor
            // 
            this.ctlListEditor.BackColor = System.Drawing.Color.White;
            this.ctlListEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlListEditor.EditorDelegate = null;
            this.ctlListEditor.ForeColor = System.Drawing.Color.Black;
            this.ctlListEditor.IsReadOnly = false;
            this.ctlListEditor.Location = new System.Drawing.Point(0, 0);
            this.ctlListEditor.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.ctlListEditor.Name = "ctlListEditor";
            this.ctlListEditor.Size = new System.Drawing.Size(511, 315);
            this.ctlListEditor.Style = TextAdventures.Quest.EditorControls.WFListEditor.ColumnStyle.OneColumn;
            this.ctlListEditor.TabIndex = 0;
            // 
            // WFDictionaryStringControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ctlListEditor);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "WFDictionaryStringControl";
            this.Size = new System.Drawing.Size(511, 315);
            this.ResumeLayout(false);

        }

        #endregion

        private WFListEditor ctlListEditor;
    }
}
