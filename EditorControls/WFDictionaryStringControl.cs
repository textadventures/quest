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
    public partial class WFDictionaryStringControl : UserControl, IListEditorDelegate
    {
        private string m_attributeName;
        private string m_elementName;
        private EditorController m_controller;
        private IEditorData m_data;
        private IEditorControl m_controlData;

        public WFDictionaryStringControl()
        {
            InitializeComponent();
        }

        private IEditableDictionary<string> withEventsField_m_list;
        private IEditableDictionary<string> m_list
        {
            get { return withEventsField_m_list; }
            set
            {
                if (withEventsField_m_list != null)
                {
                    withEventsField_m_list.Added -= m_list_Added;
                    withEventsField_m_list.Removed -= m_list_Removed;
                }
                withEventsField_m_list = value;
                if (withEventsField_m_list != null)
                {
                    withEventsField_m_list.Added += m_list_Added;
                    withEventsField_m_list.Removed += m_list_Removed;
                }
            }
        }

        public string AttributeName
        {
            get { return m_attributeName; }
        }

        public event DirtyEventHandler Dirty;
        public delegate void DirtyEventHandler(object sender, DataModifiedEventArgs args);
        public event RequestParentElementEditorSaveEventHandler RequestParentElementEditorSave;
        public delegate void RequestParentElementEditorSaveEventHandler();

        public void Initialise(EditorController controller, IEditorControl controlData)
        {
            m_controller = controller;
            ctlListEditor.EditorDelegate = this;
            ctlListEditor.Style = WFListEditor.ColumnStyle.TwoColumns;
            ctlListEditor.SetHeader(1, "Key");
            ctlListEditor.SetHeader(2, "Value");
            ctlListEditor.UpdateList(null);
            m_controller = controller;
            m_attributeName = controlData.Attribute;
            m_controlData = controlData;
        }

        public void Populate(IEditorData data)
        {
            m_data = data;
            if (data != null)
            {
                Value = data.GetAttribute(m_attributeName);
                m_elementName = data.Name;
                ctlListEditor.IsReadOnly = data.ReadOnly;
            }
            else
            {
                Value = null;
                m_elementName = null;
            }
        }

        public void Save(IEditorData data)
        {
        }

        public object Value
        {
            get { return m_list; }
            set
            {
                m_list = value as IEditableDictionary<string>;
                if (m_list == null)
                {
                    ctlListEditor.UpdateList(null);
                }
                else
                {
                    ctlListEditor.UpdateList(m_list == null ? null : m_list.DisplayItems);
                }
            }
        }

        public void DoAdd()
        {
            var addKey = PopupEditors.EditString(m_controlData.GetString("keyprompt"), string.Empty);
            if (addKey.Cancelled) return;
            if (!ValidateInput(addKey.Result)) return;

            var addValue = PopupEditors.EditString(m_controlData.GetString("valueprompt"), string.Empty);
            if (addValue.Cancelled) return;

            PrepareForEditing();

            if (m_list == null)
            {
                Value = m_controller.CreateNewEditableStringDictionary(m_elementName, m_attributeName, addKey.Result, addValue.Result, true);
            }
            else
            {
                m_list.Add(addKey.Result, addValue.Result);
            }
        }

        public void DoEdit(string key, int index)
        {
            var result = PopupEditors.EditString(m_controlData.GetString("valueprompt"), m_list[key]);
            if (result.Cancelled) return;
            if (result.Result == m_list[key]) return;

            PrepareForEditing();
            m_list.Update(key, result.Result);
        }

        public void DoRemove(string[] keys)
        {
            PrepareForEditing();
            m_list.Remove(keys);
        }

        private bool ValidateInput(string input)
        {
            if (m_list == null) return true;
            var result = m_list.CanAdd(input);
            if (result.Valid) return true;

            PopupEditors.DisplayValidationError(result, input, "Unable to add item");
            return false;
        }

        private void m_list_Added(object sender, EditableListUpdatedEventArgs<string> e)
        {
            ctlListEditor.AddListItem(new KeyValuePair<string, string>(e.UpdatedItem.Key, e.UpdatedItem.Value), e.Index);

            if ((e.Source == EditorUpdateSource.User))
            {
                ctlListEditor.SetSelectedItem(e.UpdatedItem.Key);
                ctlListEditor.Focus();
            }

            if (Dirty != null)
            {
                Dirty(this, new DataModifiedEventArgs(null, m_list));
            }
        }

        private void m_list_Removed(object sender, EditableListUpdatedEventArgs<string> e)
        {
            ctlListEditor.RemoveListItem(e.UpdatedItem.Key);
            if (Dirty != null)
            {
                Dirty(this, new DataModifiedEventArgs(null, m_list));
            }
        }

        private void ctlListEditor_ToolbarClicked()
        {
            if (RequestParentElementEditorSave != null)
            {
                RequestParentElementEditorSave();
            }
        }

        private void PrepareForEditing()
        {
            // If we're currently displaying a dictionary which belongs to a type we inherit from,
            // we must clone the dictionary before we can edit it.

            if (m_list == null) return;

            if (m_list.Owner != m_data.Name)
            {
                Value = m_list.Clone(m_elementName, m_attributeName);
            }
        }
    }
}
