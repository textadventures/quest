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
            this.lstInputAutoComplete.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstInputAutoComplete.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.lstInputAutoComplete.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.lstInputAutoComplete.FormattingEnabled = true;
            this.lstInputAutoComplete.Location = new System.Drawing.Point(11, 29);
            this.lstInputAutoComplete.Name = "lstInputAutoComplete";
            this.lstInputAutoComplete.Size = new System.Drawing.Size(334, 21);
            this.lstInputAutoComplete.TabIndex = 6;
            this.lstInputAutoComplete.Visible = false;
            // 
            // lblDropdownCaption
            // 
            this.lblDropdownCaption.AutoSize = true;
            this.lblDropdownCaption.Location = new System.Drawing.Point(8, 61);
            this.lblDropdownCaption.Name = "lblDropdownCaption";
            this.lblDropdownCaption.Size = new System.Drawing.Size(56, 13);
            this.lblDropdownCaption.TabIndex = 10;
            this.lblDropdownCaption.Text = "Dropdown";
            // 
            // txtInput
            // 
            this.txtInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInput.Location = new System.Drawing.Point(11, 29);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(334, 20);
            this.txtInput.TabIndex = 4;
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(270, 58);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(75, 23);
            this.cmdCancel.TabIndex = 9;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.Location = new System.Drawing.Point(189, 58);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(75, 23);
            this.cmdOK.TabIndex = 8;
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // lblPrompt
            // 
            this.lblPrompt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPrompt.Location = new System.Drawing.Point(8, 7);
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Size = new System.Drawing.Size(337, 40);
            this.lblPrompt.TabIndex = 5;
            this.lblPrompt.Text = "Prompt";
            // 
            // lstDropdown
            // 
            this.lstDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.lstDropdown.FormattingEnabled = true;
            this.lstDropdown.Location = new System.Drawing.Point(71, 58);
            this.lstDropdown.Name = "lstDropdown";
            this.lstDropdown.Size = new System.Drawing.Size(274, 21);
            this.lstDropdown.TabIndex = 7;
            // 
            // InputWindow
            // 
            this.AcceptButton = this.cmdOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cmdCancel;
            this.ClientSize = new System.Drawing.Size(352, 89);
            this.Controls.Add(this.lstInputAutoComplete);
            this.Controls.Add(this.lblDropdownCaption);
            this.Controls.Add(this.txtInput);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.Controls.Add(this.lblPrompt);
            this.Controls.Add(this.lstDropdown);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputWindow";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Quest";
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