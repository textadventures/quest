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
    [ControlType("dropdown")]
    public partial class DropDownControl : UserControl, IElementEditorControl
    {
        private ControlDataHelper<string> m_helper;
        private IDictionary<string, string> m_dictionary;
        private Dictionary<string, string> m_dictionaryValuesToKeys;

        public DropDownControl()
        {
            InitializeComponent();
            m_helper = new ControlDataHelper<string>(this);
            m_helper.Initialise += m_helper_Initialise;
        }

        void m_helper_Initialise()
        {
            int? width = m_helper.ControlDefinition.Width;
            if (width.HasValue)
            {
                lstDropdown.MinWidth = width.Value;
                lstDropdown.MaxWidth = width.Value;
            }

            lstDropdown.IsEditable = m_helper.ControlDefinition.GetBool("freetext");

            // validvalues may be a simple string list, or a dictionary
            IEnumerable<string> valuesList = m_helper.ControlDefinition.GetListString("validvalues");
            IDictionary<string, string> valuesDictionary = m_helper.ControlDefinition.GetDictionary("validvalues");

            if (valuesList != null)
            {
                SetListItems(valuesList.ToArray());
            }
            else if (valuesDictionary != null)
            {
                SetListItems(valuesDictionary.Values.ToArray());
                InitialiseDictionary(valuesDictionary);
            }
            else
            {
                throw new Exception("Invalid type for validvalues");
            }
        }

        private void InitialiseDictionary(IDictionary<string, string> dictionary)
        {
            m_dictionary = dictionary;
            m_dictionaryValuesToKeys = new Dictionary<string, string>();
            foreach (var item in dictionary)
            {
                m_dictionaryValuesToKeys.Add(item.Value, item.Key);
            }
        }

        private void SetListItems(IEnumerable<string> values)
        {
            lstDropdown.Items.Clear();
            foreach (string value in values)
            {
                lstDropdown.Items.Add(value);
            }
        }

        private void lstDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_helper.SetDirty(GetCurrentValue());
        }

        public IControlDataHelper Helper
        {
            get { return m_helper; }
        }

        public void Populate(IEditorData data)
        {
            if (data == null) return;
            m_helper.StartPopulating();
            lstDropdown.Text = m_helper.Populate(data);
            lstDropdown.IsEnabled = m_helper.CanEdit(data) && !data.ReadOnly;
            m_helper.FinishedPopulating();
        }

        public void Save()
        {
            if (!m_helper.IsDirty) return;
            string saveValue = GetCurrentValue();
            m_helper.Save(saveValue);
        }

        private string GetCurrentValue()
        {
            if (m_dictionary == null)
            {
                return (string)lstDropdown.SelectedItem;
            }
            else
            {
                if (lstDropdown.Text.Length == 0) return null;
                return m_dictionaryValuesToKeys[lstDropdown.Text];
            }
        }
    }
}
