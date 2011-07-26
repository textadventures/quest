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
    public partial class WFDictionaryScriptControl : UserControl, IListEditorDelegate
    {
        private string m_attributeName;
        private string m_elementName;
        private EditorController m_controller;
        private IEditorData m_data;
        private IEditorControl m_controlData;

        public WFDictionaryScriptControl()
        {
            InitializeComponent();
            ctlListEditor.ToolbarClicked += ctlListEditor_ToolbarClicked;
        }

        private IEditableDictionary<IEditableScripts> withEventsField_m_list;
        
        private IEditableDictionary<IEditableScripts> m_list
        {
            get { return withEventsField_m_list; }
            set
            {
                if (withEventsField_m_list != null)
                {
                    withEventsField_m_list.Added -= m_list_Added;
                    withEventsField_m_list.Removed -= m_list_Removed;
                    withEventsField_m_list.Updated -= m_list_Updated;
                }
                withEventsField_m_list = value;
                if (withEventsField_m_list != null)
                {
                    withEventsField_m_list.Added += m_list_Added;
                    withEventsField_m_list.Removed += m_list_Removed;
                    withEventsField_m_list.Updated += m_list_Updated;
                }
            }
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
            ctlListEditor.SetHeader(2, "Script");
            ctlListEditor.UpdateList(null);
            m_controller = controller;
            if (controlData != null)
            {
                m_attributeName = controlData.Attribute;
            }
            else
            {
                m_attributeName = null;
            }
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
                m_list = value as IEditableDictionary<IEditableScripts>;
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
            var addKey = PopupEditors.EditString(m_controlData.GetString("keyprompt"), string.Empty, GetAutoCompleteList());
            if (addKey.Cancelled) return;
            if (!ValidateInput(addKey.Result)) return;

            IEditableScripts script = m_controller.CreateNewEditableScripts(null, null, null, true);

            if (m_list == null)
            {
                Value = m_controller.CreateNewEditableScriptDictionary(m_elementName, m_attributeName, addKey.Result, script, true);
                
                // Script will have been cloned, so ensure we use a reference to the script that actually appears in the dictionary
                script = m_list[addKey.Result];
            }
            else
            {
                m_list.Add(addKey.Result, script);
            }

            PopupEditors.EditScript(m_controller, ref script, null, null, false, () => Dirty(this, new DataModifiedEventArgs(null, m_list)));
        }

        public void DoEdit(string key, int index)
        {
            IEditableScripts script = m_list[key];

            PopupEditors.EditScript(m_controller, ref script, null, null, false, () => Dirty(this, new DataModifiedEventArgs(null, m_list)));
        }

        public void DoRemove(string[] keys)
        {
            m_list.Remove(keys);
        }

        private IEnumerable<string> GetAutoCompleteList()
        {
            string listsource = m_controlData.GetString("source");
            if (listsource == null) return null;
            return (listsource == "object") ? m_controller.GetObjectNames("object") : m_controller.GetElementNames(listsource);
        }

        private bool ValidateInput(string input)
        {
            if (m_list == null) return true;
            var result = m_list.CanAdd(input);
            if (result.Valid) return true;

            PopupEditors.DisplayValidationError(result, input, "Unable to add item");
            return false;
        }

        private void m_list_Added(object sender, EditableListUpdatedEventArgs<IEditableScripts> e)
        {
            ctlListEditor.AddListItem(new KeyValuePair<string, string>(e.UpdatedItem.Key, e.UpdatedItem.Value.DisplayString()), e.Index);

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

        private void m_list_Removed(object sender, EditableListUpdatedEventArgs<IEditableScripts> e)
        {
            ctlListEditor.RemoveListItem(e.UpdatedItem.Key);
            if (Dirty != null)
            {
                Dirty(this, new DataModifiedEventArgs(null, m_list));
            }
        }

        private void m_list_Updated(object sender, EditableListUpdatedEventArgs<IEditableScripts> e)
        {
            var updatedIndex = e.Index;
            var newDisplayString = e.UpdatedItem.Value.DisplayString();
            ctlListEditor.UpdateValue(updatedIndex, newDisplayString);
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
    }
}
