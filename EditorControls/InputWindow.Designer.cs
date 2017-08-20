namespace TextAdventures.Quest.EditorControls
{
    partial class InputWindow
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InputWindow));
            this.lstInputAutoComplete = new System.Windows.Forms.ComboBox();
            this.lblDropdownCaption = new System.Windows.Forms.Label();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.cmdOK = new System.Windows.Forms.Button();
            this.lblPrompt = new System.Windows.Forms.Label();
            this.lstDropdown = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // lstInputAutoComplete
            // 
            resources.ApplyResources(this.lstInputAutoComplete, "lstInputAutoComplete");
            this.lstInputAutoComplete.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.lstInputAutoComplete.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.lstInputAutoComplete.FormattingEnabled = true;
            this.lstInputAutoComplete.Name = "lstInputAutoComplete";
            // 
            // lblDropdownCaption
            // 
            resources.ApplyResources(this.lblDropdownCaption, "lblDropdownCaption");
            this.lblDropdownCaption.Name = "lblDropdownCaption";
            // 
            // txtInput
            // 
            resources.ApplyResources(this.txtInput, "txtInput");
            this.txtInput.Name = "txtInput";
            // 
            // cmdCancel
            // 
            resources.ApplyResources(this.cmdCancel, "cmdCancel");
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            resources.ApplyResources(this.cmdOK, "cmdOK");
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblPrompt
            // 
            resources.ApplyResources(this.lblPrompt, "lblPrompt");
            this.lblPrompt.Name = "lblPrompt";
            // 
            // lstDropdown
            // 
            resources.ApplyResources(this.lstDropdown, "lstDropdown");
            this.lstDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.lstDropdown.FormattingEnabled = true;
            this.lstDropdown.Name = "lstDropdown";
            // 
            // InputWindow
            // 
            this.AcceptButton = this.cmdOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.GhostWhite;
            this.CancelButton = this.cmdCancel;
            this.Controls.Add(this.lstInputAutoComplete);
            this.Controls.Add(this.lblDropdownCaption);
            this.Controls.Add(this.txtInput);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.lblPrompt);
            this.Controls.Add(this.lstDropdown);
            this.ForeColor = System.Drawing.Color.Black;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputWindow";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.ComboBox lstInputAutoComplete;
        internal System.Windows.Forms.Label lblDropdownCaption;
        internal System.Windows.Forms.TextBox txtInput;
        internal System.Windows.Forms.Button cmdCancel;
        internal System.Windows.Forms.Button cmdOK;
        internal System.Windows.Forms.Label lblPrompt;
        internal System.Windows.Forms.ComboBox lstDropdown;

    }
}