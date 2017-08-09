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
            this.cmdAddType = new System.Windows.Forms.ToolStripButton();
            this.cmdDeleteType = new System.Windows.Forms.ToolStripButton();
            this.ctlSplitContainer = new System.Windows.Forms.SplitContainer();
            this.lstAttributes = new System.Windows.Forms.ListView();
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSource = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ctlToolStrip = new System.Windows.Forms.ToolStrip();
            this.lblAttributesTitle = new System.Windows.Forms.ToolStripLabel();
            this.cmdAdd = new System.Windows.Forms.ToolStripButton();
            this.cmdDelete = new System.Windows.Forms.ToolStripButton();
            this.cmdOnChange = new System.Windows.Forms.ToolStripButton();
            this.ctlMultiControl = new TextAdventures.Quest.EditorControls.WFMultiControl();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
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
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // ctlSplitContainerMain
            // 
            resources.ApplyResources(this.ctlSplitContainerMain, "ctlSplitContainerMain");
            this.ctlSplitContainerMain.Name = "ctlSplitContainerMain";
            // 
            // ctlSplitContainerMain.Panel1
            // 
            resources.ApplyResources(this.ctlSplitContainerMain.Panel1, "ctlSplitContainerMain.Panel1");
            this.ctlSplitContainerMain.Panel1.Controls.Add(this.lstTypes);
            this.ctlSplitContainerMain.Panel1.Controls.Add(this.ctlTypesToolStrip);
            // 
            // ctlSplitContainerMain.Panel2
            // 
            resources.ApplyResources(this.ctlSplitContainerMain.Panel2, "ctlSplitContainerMain.Panel2");
            this.ctlSplitContainerMain.Panel2.Controls.Add(this.ctlSplitContainer);
            // 
            // lstTypes
            // 
            resources.ApplyResources(this.lstTypes, "lstTypes");
            this.lstTypes.BackColor = System.Drawing.Color.White;
            this.lstTypes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnHeader1,
            this.ColumnHeader2,
            this.ColumnHeader3});
            this.lstTypes.ForeColor = System.Drawing.Color.Black;
            this.lstTypes.FullRowSelect = true;
            this.lstTypes.GridLines = true;
            this.lstTypes.HideSelection = false;
            this.lstTypes.MultiSelect = false;
            this.lstTypes.Name = "lstTypes";
            this.lstTypes.UseCompatibleStateImageBehavior = false;
            this.lstTypes.View = System.Windows.Forms.View.Details;
            this.lstTypes.SelectedIndexChanged += new System.EventHandler(this.lstTypes_SelectedIndexChanged);
            // 
            // ColumnHeader1
            // 
            resources.ApplyResources(this.ColumnHeader1, "ColumnHeader1");
            // 
            // ColumnHeader2
            // 
            resources.ApplyResources(this.ColumnHeader2, "ColumnHeader2");
            // 
            // ColumnHeader3
            // 
            resources.ApplyResources(this.ColumnHeader3, "ColumnHeader3");
            // 
            // ctlTypesToolStrip
            // 
            resources.ApplyResources(this.ctlTypesToolStrip, "ctlTypesToolStrip");
            this.ctlTypesToolStrip.BackColor = System.Drawing.Color.Transparent;
            this.ctlTypesToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.ctlTypesToolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ctlTypesToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripLabel2,
            this.cmdAddType,
            this.cmdDeleteType});
            this.ctlTypesToolStrip.Name = "ctlTypesToolStrip";
            // 
            // ToolStripLabel2
            // 
            resources.ApplyResources(this.ToolStripLabel2, "ToolStripLabel2");
            this.ToolStripLabel2.Name = "ToolStripLabel2";
            // 
            // cmdAddType
            // 
            resources.ApplyResources(this.cmdAddType, "cmdAddType");
            this.cmdAddType.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Plus_16;
            this.cmdAddType.Name = "cmdAddType";
            this.cmdAddType.Click += new System.EventHandler(this.cmdAddType_Click);
            // 
            // cmdDeleteType
            // 
            resources.ApplyResources(this.cmdDeleteType, "cmdDeleteType");
            this.cmdDeleteType.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Minus_16;
            this.cmdDeleteType.Name = "cmdDeleteType";
            this.cmdDeleteType.Click += new System.EventHandler(this.cmdDeleteType_Click);
            // 
            // ctlSplitContainer
            // 
            resources.ApplyResources(this.ctlSplitContainer, "ctlSplitContainer");
            this.ctlSplitContainer.Name = "ctlSplitContainer";
            // 
            // ctlSplitContainer.Panel1
            // 
            resources.ApplyResources(this.ctlSplitContainer.Panel1, "ctlSplitContainer.Panel1");
            this.ctlSplitContainer.Panel1.Controls.Add(this.lstAttributes);
            this.ctlSplitContainer.Panel1.Controls.Add(this.ctlToolStrip);
            // 
            // ctlSplitContainer.Panel2
            // 
            resources.ApplyResources(this.ctlSplitContainer.Panel2, "ctlSplitContainer.Panel2");
            this.ctlSplitContainer.Panel2.Controls.Add(this.ctlMultiControl);
            this.ctlSplitContainer.Panel2.Controls.Add(this.toolStrip);
            // 
            // lstAttributes
            // 
            resources.ApplyResources(this.lstAttributes, "lstAttributes");
            this.lstAttributes.BackColor = System.Drawing.Color.White;
            this.lstAttributes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName,
            this.colValue,
            this.colSource});
            this.lstAttributes.ForeColor = System.Drawing.Color.Black;
            this.lstAttributes.FullRowSelect = true;
            this.lstAttributes.GridLines = true;
            this.lstAttributes.HideSelection = false;
            this.lstAttributes.MultiSelect = false;
            this.lstAttributes.Name = "lstAttributes";
            this.lstAttributes.UseCompatibleStateImageBehavior = false;
            this.lstAttributes.View = System.Windows.Forms.View.Details;
            this.lstAttributes.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lstAttributes_ColumnClick);
            this.lstAttributes.SelectedIndexChanged += new System.EventHandler(this.lstAttributes_SelectedIndexChanged);
            // 
            // colName
            // 
            resources.ApplyResources(this.colName, "colName");
            // 
            // colValue
            // 
            resources.ApplyResources(this.colValue, "colValue");
            // 
            // colSource
            // 
            resources.ApplyResources(this.colSource, "colSource");
            // 
            // ctlToolStrip
            // 
            resources.ApplyResources(this.ctlToolStrip, "ctlToolStrip");
            this.ctlToolStrip.BackColor = System.Drawing.Color.Transparent;
            this.ctlToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.ctlToolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ctlToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblAttributesTitle,
            this.cmdAdd,
            this.cmdDelete,
            this.cmdOnChange});
            this.ctlToolStrip.Name = "ctlToolStrip";
            // 
            // lblAttributesTitle
            // 
            resources.ApplyResources(this.lblAttributesTitle, "lblAttributesTitle");
            this.lblAttributesTitle.Name = "lblAttributesTitle";
            // 
            // cmdAdd
            // 
            resources.ApplyResources(this.cmdAdd, "cmdAdd");
            this.cmdAdd.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Plus_16;
            this.cmdAdd.Name = "cmdAdd";
            this.cmdAdd.Click += new System.EventHandler(this.cmdAdd_Click);
            // 
            // cmdDelete
            // 
            resources.ApplyResources(this.cmdDelete, "cmdDelete");
            this.cmdDelete.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Minus_16;
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // cmdOnChange
            // 
            resources.ApplyResources(this.cmdOnChange, "cmdOnChange");
            this.cmdOnChange.Image = global::TextAdventures.Quest.EditorControls.Properties.Resources.icons8_Joyent;
            this.cmdOnChange.Name = "cmdOnChange";
            this.cmdOnChange.Click += new System.EventHandler(this.cmdOnChange_Click);
            // 
            // ctlMultiControl
            // 
            resources.ApplyResources(this.ctlMultiControl, "ctlMultiControl");
            this.ctlMultiControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ctlMultiControl.Name = "ctlMultiControl";
            // 
            // toolStrip
            // 
            resources.ApplyResources(this.toolStrip, "toolStrip");
            this.toolStrip.BackColor = System.Drawing.Color.Transparent;
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1});
            this.toolStrip.Name = "toolStrip";
            // 
            // toolStripLabel1
            // 
            resources.ApplyResources(this.toolStripLabel1, "toolStripLabel1");
            this.toolStripLabel1.Name = "toolStripLabel1";
            // 
            // WFAttributesControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.ctlSplitContainerMain);
            this.Name = "WFAttributesControl";
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
            this.ctlSplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ctlSplitContainer)).EndInit();
            this.ctlSplitContainer.ResumeLayout(false);
            this.ctlToolStrip.ResumeLayout(false);
            this.ctlToolStrip.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
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
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
    }
}
