using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AxeSoftware.Quest.EditorControls
{
    public partial class InputWindow : Form
    {
        private Control m_activeControl;

        public InputWindow()
        {
            InitializeComponent();

            ShowDropdown(false);
            m_activeControl = txtInput;
        }

        private void cmdOK_Click(System.Object sender, System.EventArgs e)
        {
            this.Hide();
        }

        private void cmdCancel_Click(System.Object sender, System.EventArgs e)
        {
            m_activeControl.Text = "";
            this.Hide();
        }

        private void ShowDropdown(bool visible)
        {
            if (visible)
            {
                this.Height = 162;
            }
            else
            {
                this.Height = 127;
            }

            lblDropdownCaption.Visible = visible;
            lstDropdown.Visible = visible;
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

    }
}
