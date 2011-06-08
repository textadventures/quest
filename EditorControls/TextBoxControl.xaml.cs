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

namespace AxeSoftware.Quest.EditorControls
{
    [ControlType("textbox")]
    public partial class TextBoxControl : UserControl, IElementEditorControl
    {
        private bool m_nullable = false;
        private ControlDataHelper<string> m_helper;

        public TextBoxControl()
        {
            InitializeComponent();
            m_helper = new ControlDataHelper<string>(this);
            m_helper.Initialise += Initialise;
        }

        void Initialise()
        {
            m_nullable = m_helper.ControlDefinition.GetBool("nullable");
        }

        public IControlDataHelper Helper { get { return m_helper; } }

        public void Populate(IEditorData data)
        {
            if (data == null) return;
            m_helper.StartPopulating();
            textBox.Text = m_helper.Populate(data);
            textBox.IsEnabled = m_helper.CanEdit(data);
            textBox.IsReadOnly = data.ReadOnly;
            m_helper.FinishedPopulating();
        }

        public void Save()
        {
            if (!m_helper.IsDirty) return;
            string saveValue = textBox.Text;
            if (saveValue.Length == 0 && m_nullable) saveValue = null;
            m_helper.Save(saveValue);
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_helper.SetDirty(textBox.Text);
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Save();
        }
    }
}
