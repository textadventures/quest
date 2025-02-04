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
    [ControlType("dropdown")]
    public partial class DropDownControl : UserControl, IElementEditorControl
    {
        private ControlDataHelper<string> m_helper;
        private IDictionary<string, string> m_dictionary;
        private Dictionary<string, string> m_dictionaryValuesToKeys;
        private string m_source;
        private bool m_initialised = false;

        public DropDownControl()
        {
            InitializeComponent();
            m_helper = new ControlDataHelper<string>(this);
            m_helper.Initialise += m_helper_Initialise;
            m_helper.Uninitialise += m_helper_Uninitialise;
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
            m_source = m_helper.ControlDefinition.GetString("source");

            if (valuesList != null)
            {
                SetListItems(valuesList.ToArray());
                m_initialised = true;
            }
            else if (valuesDictionary != null)
            {
                SetListItems(valuesDictionary.Values.ToArray());
                InitialiseDictionary(valuesDictionary);
                m_initialised = true;
            }
            else if (m_source == "basefonts")
            {
                SetListItems(m_helper.Controller.AvailableBaseFonts());
                m_initialised = true;
            }
            else if (m_source == "webfonts")
            {
                InitialiseWebFonts();
            }
            else
            {
                throw new Exception("Unknown source list for dropdown");
            }
        }

        void m_helper_Uninitialise()
        {
            m_dictionary = null;
            m_dictionaryValuesToKeys = null;
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

        private void InitialiseWebFonts()
        {
            var fonts = m_helper.Controller.AvailableWebFonts();
            if (fonts.Count() > 1)
            {
                SetListItems(fonts);
                m_initialised = true;
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
            if (m_dictionary != null)
            {
                string value = m_helper.Populate(data) ?? string.Empty;
                string dropdownValue;
                if (m_dictionary.TryGetValue(value, out dropdownValue))
                {
                    lstDropdown.Text = dropdownValue;
                }
                else
                {
                    if (lstDropdown.IsEditable)
                    {
                        lstDropdown.Text = value;
                    }
                    else
                    {
                        lstDropdown.Text = string.Empty;
                    }
                }
            }
            else
            {
                lstDropdown.Text = m_helper.Populate(data);
            }
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
            string selectedValue = (string)lstDropdown.SelectedItem ?? lstDropdown.Text;
            if (m_dictionary == null)
            {
                return selectedValue;
            }
            else
            {
                if (selectedValue.Length == 0) return null;
                return m_dictionaryValuesToKeys[selectedValue];
            }
        }

        public Control FocusableControl
        {
            get { return lstDropdown; }
        }

        private void lstDropdown_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_helper.SetDirty(lstDropdown.Text);
        }

        public string SelectedItem
        {
            get { return (string)lstDropdown.SelectedItem ?? lstDropdown.Text; }
            set
            {
                lstDropdown.Text = value;
            }
        }

        public bool IsUpdatingList
        {
            get { return m_helper.IsPopulating; }
        }

        private void lstDropdown_DropDownOpened(object sender, EventArgs e)
        {
            if (m_source == "webfonts" && !m_initialised)
            {
                InitialiseWebFonts();
            }
        }
    }
}
