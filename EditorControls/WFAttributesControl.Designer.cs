namespace TextAdventures.Quest.EditorControls
{
    partial class WFAttributesControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WFAttributesControl));
            this.ctlSplitContainerMain = new System.Windows.Forms.SplitContainer();
            this.lstTypes = new System.Windows.Forms.ListView();
            this.ColumnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ctlTypesToolStrip = new System.Windows.Forms.ToolStrip();
            this.ToolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.cmdDeleteType = new System.Windows.Forms.ToolStripButton();
            this.ctlSplitContainer = new System.Windows.Forms.SplitContainer();
            this.lstAttributes = new System.Windows.Forms.ListView();
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSource = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ctlToolStrip = new System.Windows.Forms.ToolStrip();
            this.lblAttributesTitle = new System.Windows.Forms.ToolStripLabel();
            this.cmdAdd = new System.Windows.Forms.ToolStripButton();
            this.cmdOnChange = new System.Windows.Forms.ToolStripButton();
            this.cmdDelete = new System.Windows.Forms.ToolStripButton();
            this.cmdAddType = new System.Windows.Forms.ToolStripButton();
            this.ctlMultiControl = new TextAdventures.Quest.EditorControls.WFMultiControl();
            ((System.ComponentModel.ISupportInitialize)(this.ctlSplitContainerMain)).BeginInit();
            this.ctlSplitContainerMain.Panel1.SuspendLayout();
            this.ctlSplitContainerMain.Panel2.SuspendLayout();
            this.ctlSplitContainerMain.SuspendLayout();
            this.ctlTypesToolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ctlSplitContainer)).BeginInit();
            this.ctlSplitContainer.Panel1.SuspendLayout();
            this.ctlSplitContainer.Panel2.SuspendLayout();
            this.ctlSplitContainer.SuspendLayout();
            this.ctlToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // ctlSplitContainerMain
            // 
            this.ctlSplitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlSplitContainerMain.Location = new System.Drawing.Point(0, 0);
            this.ctlSplitContainerMain.Name = "ctlSplitContainerMain";
            this.ctlSplitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // ctlSplitContainerMain.Panel1
            // 
            this.ctlSplitContainerMain.Panel1.Controls.Add(this.lstTypes);
            this.ctlSplitContainerMain.Panel1.Controls.Add(this.ctlTypesToolStrip);
            // 
            // ctlSplitContainerMain.Panel2
            // 
            this.ctlSplitContainerMain.Panel2.Controls.Add(this.ctlSplitContainer);
            this.ctlSplitContainerMain.Size = new System.Drawing.Size(699, 482);
            this.ctlSplitContainerMain.SplitterDistance = 163;
            this.ctlSplitContainerMain.TabIndex = 0;
            // 
            // lstTypes
            // 
            this.lstTypes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnHeader1,
            this.ColumnHeader2,
            this.ColumnHeader3});
            this.lstTypes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstTypes.FullRowSelect = true;
            this.lstTypes.HideSelection = false;
            this.lstTypes.Location = new System.Drawing.Point(0, 25);
            this.lstTypes.MultiSelect = false;
            this.lstTypes.Name = "lstTypes";
            this.lstTypes.Size = new System.Drawing.Size(699, 138);
            this.lstTypes.TabIndex = 6;
            this.lstTypes.UseCompatibleStateImageBehavior = false;
            this.lstTypes.View = System.Windows.Forms.View.Details;
            this.lstTypes.SelectedIndexChanged += new System.EventHandler(this.lstTypes_SelectedIndexChanged);
            // 
            // ColumnHeader1
            // 
            this.ColumnHeader1.Text = "Name";
            this.ColumnHeader1.Width = 138;
            // 
            // ColumnHeader2
            // 
            this.ColumnHeader2.Text = "Value";
            this.ColumnHeader2.Width = 354;
            // 
            // ColumnHeader3
            // 
            this.ColumnHeader3.Text = "Source";
            this.ColumnHeader3.Width = 115;
            // 
            // ctlTypesToolStrip
            // 
            this.ctlTypesToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripLabel2,
            this.cmdAddType,
            this.cmdDeleteType});
            this.ctlTypesToolStrip.Location = new System.Drawing.Point(0, 0);
            this.ctlTypesToolStrip.Name = "ctlTypesToolStrip";
            this.ctlTypesToolStrip.Size = new System.Drawing.Size(699, 25);
            this.ctlTypesToolStrip.TabIndex = 5;
            this.ctlTypesToolStrip.Text = "ToolStrip1";
            // 
            // ToolStripLabel2
            // 
            this.ToolStripLabel2.AutoSize = false;
            this.ToolStripLabel2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ToolStripLabel2.Name = "ToolStripLabel2";
            this.ToolStripLabel2.Size = new System.Drawing.Size(100, 22);
            this.ToolStripLabel2.Text = "Inherited Types";
            this.ToolStripLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmdDeleteType
            // 
            this.cmdDeleteType.Image = ((System.Drawing.Image)(resources.GetObject("cmdDeleteType.Image")));
            this.cmdDeleteType.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdDeleteType.Name = "cmdDeleteType";
            this.cmdDeleteType.Size = new System.Drawing.Size(60, 22);
            this.cmdDeleteType.Text = "Delete";
            this.cmdDeleteType.Click += new System.EventHandler(this.cmdDeleteType_Click);
            // 
            // ctlSplitContainer
            // 
            this.ctlSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.ctlSplitContainer.Name = "ctlSplitContainer";
            this.ctlSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // ctlSplitContainer.Panel1
            // 
            this.ctlSplitContainer.Panel1.Controls.Add(this.lstAttributes);
            this.ctlSplitContainer.Panel1.Controls.Add(this.ctlToolStrip);
            // 
            // ctlSplitContainer.Panel2
            // 
            this.ctlSplitContainer.Panel2.Controls.Add(this.ctlMultiControl);
            this.ctlSplitContainer.Size = new System.Drawing.Size(699, 315);
            this.ctlSplitContainer.SplitterDistance = 156;
            this.ctlSplitContainer.TabIndex = 0;
            // 
            // lstAttributes
            // 
            this.lstAttributes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colValue,
            this.colSource});
            this.lstAttributes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstAttributes.FullRowSelect = true;
            this.lstAttributes.HideSelection = false;
            this.lstAttributes.Location = new System.Drawing.Point(0, 25);
            this.lstAttributes.MultiSelect = false;
            this.lstAttributes.Name = "lstAttributes";
            this.lstAttributes.Size = new System.Drawing.Size(699, 131);
            this.lstAttributes.TabIndex = 5;
            this.lstAttributes.UseCompatibleStateImageBehavior = false;
            this.lstAttributes.View = System.Windows.Forms.View.Details;
            this.lstAttributes.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lstAttributes_ColumnClick);
            this.lstAttributes.SelectedIndexChanged += new System.EventHandler(this.lstAttributes_SelectedIndexChanged);
            // 
            // colName
            // 
            this.colName.Text = "Name";
            this.colName.Width = 138;
            // 
            // colValue
            // 
            this.colValue.Text = "Value";
            this.colValue.Width = 354;
            // 
            // colSource
            // 
            this.colSource.Text = "Source";
            this.colSource.Width = 115;
            // 
            // ctlToolStrip
            // 
            this.ctlToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblAttributesTitle,
            this.cmdAdd,
            this.cmdOnChange,
            this.cmdDelete});
            this.ctlToolStrip.Location = new System.Drawing.Point(0, 0);
            this.ctlToolStrip.Name = "ctlToolStrip";
            this.ctlToolStrip.Size = new System.Drawing.Size(699, 25);
            this.ctlToolStrip.TabIndex = 4;
            this.ctlToolStrip.Text = "ToolStrip1";
            // 
            // lblAttributesTitle
            // 
            this.lblAttributesTitle.AutoSize = false;
            this.lblAttributesTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAttributesTitle.Name = "lblAttributesTitle";
            this.lblAttributesTitle.Size = new System.Drawing.Size(100, 22);
            this.lblAttributesTitle.Text = "Attributes";
            this.lblAttributesTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            // cmdOnChange
            // 
            this.cmdOnChange.Image = ((System.Drawing.Image)(resources.GetObject("cmdOnChange.Image")));
            this.cmdOnChange.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdOnChange.Name = "cmdOnChange";
            this.cmdOnChange.Size = new System.Drawing.Size(126, 22);
            this.cmdOnChange.Text = "Add Change Script";
            this.cmdOnChange.Click += new System.EventHandler(this.cmdOnChange_Click);
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
            // cmdAddType
            // 
            this.cmdAddType.Image = ((System.Drawing.Image)(resources.GetObject("cmdAddType.Image")));
            this.cmdAddType.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cmdAddType.Name = "cmdAddType";
            this.cmdAddType.Size = new System.Drawing.Size(49, 22);
            this.cmdAddType.Text = "Add";
            this.cmdAddType.Click += new System.EventHandler(this.cmdAddType_Click);
            // 
            // ctlMultiControl
            // 
            this.ctlMultiControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlMultiControl.Location = new System.Drawing.Point(0, 0);
            this.ctlMultiControl.Name = "ctlMultiControl";
            this.ctlMultiControl.Size = new System.Drawing.Size(699, 155);
            this.ctlMultiControl.TabIndex = 0;
            // 
            // WFAttributesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ctlSplitContainerMain);
            this.Name = "WFAttributesControl";
            this.Size = new System.Drawing.Size(699, 482);
            this.ctlSplitContainerMain.Panel1.ResumeLayout(false);
            this.ctlSplitContainerMain.Panel1.PerformLayout();
            this.ctlSplitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ctlSplitContainerMain)).EndInit();
            this.ctlSplitContainerMain.ResumeLayout(false);
            this.ctlTypesToolStrip.ResumeLayout(false);
            this.ctlTypesToolStrip.PerformLayout();
            this.ctlSplitContainer.Panel1.ResumeLayout(false);
            this.ctlSplitContainer.Panel1.PerformLayout();
            this.ctlSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ctlSplitContainer)).EndInit();
            this.ctlSplitContainer.ResumeLayout(false);
            this.ctlToolStrip.ResumeLayout(false);
            this.ctlToolStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.ToolStrip ctlTypesToolStrip;
        internal System.Windows.Forms.ToolStripLabel ToolStripLabel2;
        internal System.Windows.Forms.ToolStripButton cmdDeleteType;
        internal System.Windows.Forms.ListView lstTypes;
        internal System.Windows.Forms.ColumnHeader ColumnHeader1;
        internal System.Windows.Forms.ColumnHeader ColumnHeader2;
        internal System.Windows.Forms.ColumnHeader ColumnHeader3;
        internal System.Windows.Forms.ToolStrip ctlToolStrip;
        internal System.Windows.Forms.ToolStripLabel lblAttributesTitle;
        internal System.Windows.Forms.ToolStripButton cmdAdd;
        internal System.Windows.Forms.ToolStripButton cmdDelete;
        internal System.Windows.Forms.ListView lstAttributes;
        internal System.Windows.Forms.ColumnHeader colName;
        internal System.Windows.Forms.ColumnHeader colValue;
        internal System.Windows.Forms.ColumnHeader colSource;
        private WFMultiControl ctlMultiControl;
        protected System.Windows.Forms.SplitContainer ctlSplitContainerMain;
        protected System.Windows.Forms.ToolStripButton cmdOnChange;
        protected System.Windows.Forms.SplitContainer ctlSplitContainer;
        private System.Windows.Forms.ToolStripButton cmdAddType;

    }
}
