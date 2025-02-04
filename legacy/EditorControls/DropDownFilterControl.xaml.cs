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
    [ControlType("filter")]
    public partial class DropDownFilterControl : UserControl, IElementEditorControl
    {
        private ControlDataHelper<string> m_helper;
        private string m_filterGroup;
        private IDictionary<string, string> m_dropDownValues;
        private bool m_populating;
        private IEditorData m_data;

        public DropDownFilterControl()
        {
            InitializeComponent();
            m_helper = new ControlDataHelper<string>(this);
            m_helper.Initialise += m_helper_Initialise;
            m_helper.Uninitialise += m_helper_Uninitialise;
        }

        void m_helper_Initialise()
        {
            m_populating = true;
            m_dropDownValues = m_helper.ControlDefinition.GetDictionary("filters");
            foreach (string value in m_dropDownValues.Values.ToArray())
            {
                lstDropdown.Items.Add(value);
            }
            m_filterGroup = m_helper.ControlDefinition.GetString("filtergroupname");
            m_populating = false;
        }

        void m_helper_Uninitialise()
        {
            m_dropDownValues = null;
        }

        private void lstDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_populating) return;
            Save();
        }

        public IControlDataHelper Helper
        {
            get { return m_helper; }
        }

        public void Populate(IEditorData data)
        {
            m_data = data;
            if (data == null) return;
            m_populating = true;

            string selectedItem = data.GetSelectedFilter(m_filterGroup);
            if (selectedItem == null) selectedItem = m_helper.ControlDefinition.Parent.GetDefaultFilterName(m_filterGroup, data);
            lstDropdown.Text = m_dropDownValues[selectedItem];
            
            m_populating = false;
        }

        public void Save()
        {
            string filter = GetSelectedFilter();
            if (filter == null) return;
            m_data.SetSelectedFilter(m_filterGroup, filter);
        }

        private string GetSelectedFilter()
        {
            string selection = (string)lstDropdown.SelectedItem;
            if (selection.Length == 0) return null;
            return m_dropDownValues.Keys.ToArray()[lstDropdown.SelectedIndex];
        }

        public Control FocusableControl
        {
            get { return lstDropdown; }
        }
    }
}
