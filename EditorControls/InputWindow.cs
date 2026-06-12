using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TextAdventures.Quest.EditorControls
{
    public partial class InputWindow : Form
    {
        private Control m_activeControl;
        private float dpiX, dpiY;

        public InputWindow()
        {
            if (System.Windows.Forms.Application.OpenForms.Count > 0)
                this.Font = System.Windows.Forms.Application.OpenForms[0].Font;

            InitializeComponent();

            using (Graphics g = this.CreateGraphics())
            {
                dpiX = g.DpiX;
                dpiY = g.DpiY;
            }

            this.AutoScaleMode = AutoScaleMode.None;
            ShowDropdown(false);
            m_activeControl = txtInput;
        }

        private void cmdOK_Click(System.Object sender, System.EventArgs e)
        {
            Cancelled = false;
            this.Hide();
        }

        private void cmdCancel_Click(System.Object sender, System.EventArgs e)
        {
            Cancelled = true;
            m_activeControl.Text = "";
            this.Hide();
        }

        private void ShowDropdown(bool visible)
        {
            float s = dpiY / 96f;
            int pad = (int)(10 * s);
            int w = ClientSize.Width - 2 * pad;

            lblPrompt.SetBounds(pad, pad, w, lblPrompt.PreferredHeight);
            txtInput.SetBounds(pad, lblPrompt.Bottom + pad / 2, w, txtInput.Height);
            lstInputAutoComplete.SetBounds(pad, txtInput.Top, w, txtInput.Height);

            lblDropdownCaption.Visible = visible;
            lstDropdown.Visible = visible;

            int contentBottom = txtInput.Bottom;
            if (visible)
            {
                lblDropdownCaption.SetBounds(pad, contentBottom + pad / 2, w, lblDropdownCaption.Height);
                lstDropdown.SetBounds(pad, lblDropdownCaption.Bottom + (int)(4 * s), w, lstDropdown.Height);
                contentBottom = lstDropdown.Bottom;
            }

            int btnH = (int)(28 * s);
            int btnW = (int)(100 * s);
            int formClientH = contentBottom + pad + btnH + pad;
            ClientSize = new Size(ClientSize.Width, formClientH);

            cmdOK.SetBounds(ClientSize.Width - pad - btnW, formClientH - pad - btnH, btnW, btnH);
            cmdCancel.SetBounds(cmdOK.Left - pad / 2 - btnW, cmdOK.Top, btnW, btnH);
        }

        public void SetDropdown(string caption, IEnumerable<string> items, string defaultSelection)
        {
            lblDropdownCaption.Text = caption + ":";
            foreach (var item in items)
            {
                lstDropdown.Items.Add(item);
            }
            lstDropdown.Text = defaultSelection;
            ShowDropdown(true);
        }

        public void SetAutoComplete(IEnumerable<string> items)
        {
            txtInput.Visible = false;
            lstInputAutoComplete.Visible = true;
            m_activeControl = lstInputAutoComplete;
            foreach (var item in items)
            {
                lstInputAutoComplete.Items.Add(item);
            }
        }

        public Control ActiveInputControl
        {
            get { return m_activeControl; }
        }

        public bool Cancelled { get; private set; }
    }
}
