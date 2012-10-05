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
    [ControlType("dropdowntypes")]
    public partial class DropDownTypesControl : UserControl, IElementEditorControl
    {
        private bool m_populating = false;
        private ControlDataHelper<string> m_helper;
        private IDictionary<string, string> m_dropDownValues;
        private string m_currentType;
        private IEditorData m_data;

        private const string k_noType = "*";

        public DropDownTypesControl()
        {
            InitializeComponent();

            m_helper = new ControlDataHelper<string>(this);
            m_helper.Initialise += m_helper_Initialise;
            m_helper.Uninitialise += m_helper_Uninitialise;
        }

        void m_helper_Initialise()
        {
            m_dropDownValues = m_helper.ControlDefinition.GetDictionary("types");
            m_populating = true;
            foreach (string item in m_dropDownValues.Values)
            {
                lstTypes.Items.Add(item);
            }
            m_populating = false;
        }

        void m_helper_Uninitialise()
        {
            m_dropDownValues = null;
        }

        private void lstTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // only respond to user changes
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
            List<string> inheritedTypes = new List<string>();

            // The inherited types look like:
            // *=default; typename1=Type 1; typename2=Type2

            // Find out which of the handled types are inherited by the object

            foreach (var item in m_dropDownValues.Where(i => i.Key != k_noType))
            {
                if (m_helper.Controller.DoesElementInheritType(data.Name, item.Key))
                {
                    inheritedTypes.Add(item.Key);
                }
            }

            // if more than one type is inherited by the object, disable the control

            lstTypes.IsEnabled = (inheritedTypes.Count <= 1);

            m_populating = true;

            switch (inheritedTypes.Count)
            {
                case 0:
                    // Default - no types inherited
                    lstTypes.SelectedItem = m_dropDownValues[k_noType];
                    m_currentType = k_noType;
                    break;
                case 1:
                    lstTypes.SelectedItem = m_dropDownValues[inheritedTypes[0]];
                    m_currentType = inheritedTypes[0];
                    break;
                default:
                    lstTypes.Text = "";
                    m_currentType = null;
                    break;
            }

            m_populating = false;
        }

        public void Save()
        {
            string selectedType = GetSelectedType();
            if (selectedType == null) return;
            if (m_currentType == null) return;
            if (selectedType == m_currentType) return;

            m_helper.Controller.StartTransaction(String.Format("Change type from '{0}' to '{1}'", m_dropDownValues[m_currentType], m_dropDownValues[selectedType]));

            if (m_currentType != k_noType)
            {
                m_helper.Controller.RemoveInheritedTypeFromElement(m_data.Name, m_currentType, false);
            }

            if (selectedType != k_noType)
            {
                m_helper.Controller.AddInheritedTypeToElement(m_data.Name, selectedType, false);
            }

            m_helper.Controller.EndTransaction();
        }

        private string GetSelectedType()
        {
            if (lstTypes.Text.Length == 0 || lstTypes.SelectedIndex == -1) return null;
            return m_dropDownValues.Keys.ToArray()[lstTypes.SelectedIndex];
        }

        public Control FocusableControl
        {
            get { return lstTypes; }
        }
    }
}
