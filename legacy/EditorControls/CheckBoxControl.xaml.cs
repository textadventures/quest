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
    [ControlType("checkbox")]
    public partial class CheckBoxControl : UserControl, IElementEditorControl
    {
        private ControlDataHelper<bool> m_helper;

        public CheckBoxControl()
        {
            InitializeComponent();
            m_helper = new ControlDataHelper<bool>(this);
            m_helper.Options.DisplaysOwnCaption = true;
            m_helper.Initialise += m_helper_Initialise;
        }

        void m_helper_Initialise()
        {
            SetCaption(m_helper.ControlDefinition.Caption);
        }

        public IControlDataHelper Helper
        {
            get { return m_helper; }
        }

        public void Populate(IEditorData data)
        {
            if (data == null) return;
            m_helper.StartPopulating();
            checkBox.IsChecked = m_helper.Populate(data);
            checkBox.IsEnabled = !data.ReadOnly && m_helper.CanEdit(data);
            m_helper.FinishedPopulating();
        }

        public void Save()
        {
            m_helper.Save(checkBox.IsChecked.Value);
        }

        private void checkBox_Click(object sender, RoutedEventArgs e)
        {
            m_helper.SetDirty(checkBox.IsChecked.Value);
            Save();
        }

        public void SetCaption(string caption)
        {
            checkBox.Content = caption;
        }

        public Control FocusableControl
        {
            get { return checkBox; }
        }
    }
}
