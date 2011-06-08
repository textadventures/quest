using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AxeSoftware.Quest.EditorControls
{
    public partial class WFToolbar : UserControl
    {
        public event Action AddClicked;
        public event Action EditClicked;
        public event Action DeleteClicked;

        private bool m_readOnly;
        private bool m_isItemSelected;

        public WFToolbar()
        {
            InitializeComponent();
        }

        private void cmdAdd_Click(object sender, EventArgs e)
        {
            AddClicked();
        }

        private void cmdEdit_Click(object sender, EventArgs e)
        {
            EditClicked();
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            DeleteClicked();
        }

        public bool ReadOnly
        {
            get { return m_readOnly; }
            set
            {
                m_readOnly = value;
                UpdateEnabledState();
            }
        }

        public bool IsItemSelected
        {
            get { return m_isItemSelected; }
            set
            {
                m_isItemSelected = value;
                UpdateEnabledState();
            }
        }

        private void UpdateEnabledState()
        {
            cmdAdd.Enabled = !ReadOnly;
            cmdEdit.Enabled = !ReadOnly && IsItemSelected;
            cmdDelete.Enabled = !ReadOnly && IsItemSelected;
        }
    }
}
