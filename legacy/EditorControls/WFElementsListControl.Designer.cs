namespace TextAdventures.Quest.EditorControls
{
    partial class WFElementsListControl
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
            this.ctlListEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlListEditor.EditorDelegate = null;
            this.ctlListEditor.IsReadOnly = false;
            this.ctlListEditor.Location = new System.Drawing.Point(0, 0);
            this.ctlListEditor.Name = "ctlListEditor";
            this.ctlListEditor.Size = new System.Drawing.Size(387, 229);
            this.ctlListEditor.Style = TextAdventures.Quest.EditorControls.WFListEditor.ColumnStyle.OneColumn;
            this.ctlListEditor.TabIndex = 0;
            // 
            // WFElementsListControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ctlListEditor);
            this.Name = "WFElementsListControl";
            this.Size = new System.Drawing.Size(387, 229);
            this.ResumeLayout(false);

        }

        #endregion

        private WFListEditor ctlListEditor;
    }
}
