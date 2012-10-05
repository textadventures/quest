using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TextAdventures.Quest.EditorControls
{
    [ControlType("pattern")]
    public partial class PatternControl : UserControl, IElementEditorControl
    {
        private ControlDataHelper<IEditableCommandPattern> m_helper;
        private IEditableCommandPattern m_value;
        private string m_oldValue;
        private IEditorData m_data;

        public PatternControl()
        {
            InitializeComponent();
            m_helper = new ControlDataHelper<IEditableCommandPattern>(this);
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_helper.RaiseDirtyEvent(textBox.Text);
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Save();
        }

        public IControlDataHelper Helper
        {
            get { return m_helper;  }
        }

        public void Populate(IEditorData data)
        {
            m_data = data;
            if (data == null)
            {
                m_value = null;
                return;
            }
            m_helper.StartPopulating();
            m_value = m_helper.Populate(data);
            textBox.Text = (m_value != null) ? m_value.Pattern : string.Empty;
            m_oldValue = textBox.Text;
            textBox.IsEnabled = m_helper.CanEdit(data);
            textBox.IsReadOnly = data.ReadOnly;
            m_helper.FinishedPopulating();
        }

        public void Save()
        {
            SetValue();
        }

        private void SetValue()
        {
            string newValue = textBox.Text;
            if (m_oldValue == newValue) return;

            string description = string.Format("Set {0} to '{1}'", m_helper.ControlDefinition.Attribute, newValue);
            m_helper.Controller.StartTransaction(description);
            m_value = m_helper.Controller.CreateNewEditableCommandPattern(m_data.Name, m_helper.ControlDefinition.Attribute, newValue, false);
            m_helper.Controller.EndTransaction();
        }

        public Control FocusableControl
        {
            get { return textBox; }
        }
    }
}
