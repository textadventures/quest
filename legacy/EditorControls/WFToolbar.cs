using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TextAdventures.Quest.EditorControls
{
    public partial class WFToolbar : UserControl
    {
        public event Action AddClicked;
        public event Action EditClicked;
        public event Action DeleteClicked;
        public event Action PlayClicked;
        public event Action RecordClicked;
        public event Action MoveUpClicked;
        public event Action MoveDownClicked;

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

        private void cmdPlay_Click(object sender, EventArgs e)
        {
            PlayClicked();
        }

        private void cmdRecord_Click(object sender, EventArgs e)
        {
            RecordClicked();
        }

        private void cmdMoveUp_Click(object sender, EventArgs e)
        {
            MoveUpClicked();
        }

        private void cmdMoveDown_Click(object sender, EventArgs e)
        {
            MoveDownClicked();
        }

        public bool ReadOnly
        {
            get { return m_readOnly; }
            set
            {
                m_readOnly = value;
                UpdateEnabledState();
                if (value)
                {
                    cmdMoveUp.Enabled = false;
                    cmdMoveDown.Enabled = false;
                }
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

        public bool ShowPlayRecord
        {
            get { return cmdPlay.Available; }
            set
            {
                cmdPlay.Available = value;
                cmdRecord.Available = value;
            }
        }

        public bool ShowMoveButtons
        {
            get { return cmdMoveUp.Available; }
            set
            {
                cmdMoveUp.Available = value;
                cmdMoveDown.Available = value;
            }
        }

        public bool CanMoveUp
        {
            get { return cmdMoveUp.Enabled; }
            set { cmdMoveUp.Enabled = !ReadOnly && value; }
        }

        public bool CanMoveDown
        {
            get { return cmdMoveDown.Enabled; }
            set { cmdMoveDown.Enabled = !ReadOnly && value; }
        }
    }
}
