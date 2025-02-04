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
    [ControlType("objects")]
    public partial class DropDownObjectsControl : UserControl, IElementEditorControl, IControlDataHelper
    {
        private EditorController m_controller;
        private IEditorControl m_definition;
        private ControlDataOptions m_options = new ControlDataOptions();
        private bool m_populating = false;
        private IEditableObjectReference m_value;
        private IEditorData m_data;
        private string m_source;
        private string m_objectType;

        public event EventHandler<DataModifiedEventArgs> Dirty { add { } remove { } }
        public event Action RequestParentElementEditorSave { add { } remove { } }

        public DropDownObjectsControl()
        {
            InitializeComponent();
        }

        private void lstDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_populating) return;
            Save();
        }

        public IControlDataHelper Helper
        {
            get { return this; }
        }

        public void Populate(IEditorData data)
        {
            m_data = data;

            if (data == null)
            {
                m_value = null;
                return;
            }

            m_populating = true;
            PopulateList();

            object attr = data.GetAttribute(m_definition.Attribute);
            IEditableObjectReference value = attr as IEditableObjectReference;

            if (value == null)
            {
                lstDropdown.Text = string.Empty;
                lstDropdown.IsEnabled = (attr == null);
            }
            else
            {
                lstDropdown.SelectedItem = value.Reference;
                lstDropdown.IsEnabled = !data.ReadOnly;
            }

            m_value = value;

            m_populating = false;
        }

        public void Save()
        {
            if (m_data == null) return;
            if (m_populating) return;
            string currentValue = m_value == null ? string.Empty : m_value.Reference;
            string selectedValue = (string)lstDropdown.SelectedItem;
            if (selectedValue == null) selectedValue = string.Empty;
            if (selectedValue != currentValue)
            {
                m_controller.StartTransaction(string.Format("Change '{0}' to '{1}'", m_definition.Attribute, selectedValue));
                m_value = m_controller.CreateNewEditableObjectReference(m_data.Name, m_definition.Attribute, false);
                m_value.Reference = selectedValue;
                m_controller.EndTransaction();
            }
        }

        public void PopulateList()
        {
            bool oldValue = m_populating;
            m_populating = true;
            lstDropdown.Items.Clear();
            IEnumerable<string> allObjects = GetValidNames().OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase);
            foreach (string obj in allObjects)
            {
                lstDropdown.Items.Add(obj);
            }
            m_populating = oldValue;
        }

        public IEnumerable<string> GetValidNames()
        {
            return (m_source == null) ? m_controller.GetObjectNames(m_objectType ?? "object") : m_controller.GetElementNames(m_source);
        }

        public Type ExpectedType
        {
            get { return typeof(IEditableObjectReference); }
        }

        public string Attribute
        {
            get { return m_definition.Attribute; }
        }

        public ControlDataOptions Options
        {
            get { return m_options; }
        }

        public void DoInitialise(EditorController controller, IEditorControl definition)
        {
            m_controller = controller;
            m_definition = definition;
            m_source = definition == null ? null : definition.GetString("source");
            m_objectType = definition == null ? null : definition.GetString("objecttype");
        }

        public void DoUninitialise()
        {
            m_controller = null;
            m_definition = null;
            m_source = null;
            m_objectType = null;
        }

        public string SelectedItem
        {
            get { return (string)lstDropdown.SelectedItem; }
            set
            {
                lstDropdown.SelectedItem = value;
            }
        }

        public bool IsUpdatingList
        {
            get { return m_populating; }
        }

        public Control FocusableControl
        {
            get { return lstDropdown; }
        }
    }
}
