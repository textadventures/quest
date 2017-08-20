using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.EditorControls
{
    internal class WFStringDictionaryEditorManager : WFDictionaryEditorManager<string>
    {
        public WFStringDictionaryEditorManager(WFListEditor listEditor)
            : base(listEditor, false)
        {
        }

        protected override string GetDisplayString(string data)
        {
            return data;
        }

        protected override void AddNewValue(PopupEditors.EditStringResult addKey)
        {
            var addValue = PopupEditors.EditString(ControlData.GetString("valueprompt"), string.Empty, allowEmptyString: true);
            if (addValue.Cancelled) return;

            PrepareForEditing();

            if (List == null)
            {
                Value = Controller.CreateNewEditableStringDictionary(ElementName, AttributeName, addKey.Result, addValue.Result, true);
            }
            else
            {
                List.Add(addKey.Result, addValue.Result);
            }
        }

        protected override void EditValue(string key)
        {
            var result = PopupEditors.EditString(ControlData.GetString("valueprompt"), List[key], allowEmptyString: true);
            if (result.Cancelled) return;
            if (result.Result == List[key]) return;

            PrepareForEditing();

            Controller.StartTransaction(string.Format("Update '{0}='{1}'", key, result.Result));
            List.Update(key, result.Result);
            Controller.EndTransaction();
        }
    }

    internal class WFScriptDictionaryEditorManager : WFDictionaryEditorManager<IEditableScripts>
    {
        public WFScriptDictionaryEditorManager(WFListEditor listEditor)
            : base(listEditor, true)
        {
        }

        protected override string GetDisplayString(IEditableScripts data)
        {
            return data.DisplayString();
        }

        protected override void AddNewValue(PopupEditors.EditStringResult addKey)
        {
            IEditableScripts script = Controller.CreateNewEditableScripts(null, null, null, true);

            if (List == null)
            {
                Value = Controller.CreateNewEditableScriptDictionary(ElementName, AttributeName, addKey.Result, script, true);

                // Script will have been cloned, so ensure we use a reference to the script that actually appears in the dictionary
                script = List[addKey.Result];
            }
            else
            {
                List.Add(addKey.Result, script);
            }

            PopupEditors.EditScript(Controller, ref script, null, null, false, () => RaiseDirty(new DataModifiedEventArgs(null, List)));
        }

        protected override void EditValue(string key)
        {
            IEditableScripts script = List[key];
            PopupEditors.EditScript(Controller, ref script, null, null, false, () => RaiseDirty(new DataModifiedEventArgs(null, List)));
        }
    }

    internal abstract class WFDictionaryEditorManager<T> : IListEditorDelegate
    {
        private string m_attributeName;
        private string m_elementName;
        private EditorController m_controller;
        private IEditorData m_data;
        private IEditorControl m_controlData;
        private WFListEditor ctlListEditor;
        private bool m_canEditKey;

        public WFDictionaryEditorManager(WFListEditor listEditor, bool canEditKey)
        {
            ctlListEditor = listEditor;
            m_canEditKey = canEditKey;
        }

        protected abstract string GetDisplayString(T data);
        protected IEditableDictionary<T> List { get { return m_list; } }
        protected IEditorControl ControlData { get { return m_controlData; } }
        protected string ElementName { get { return m_elementName; } }
        protected EditorController Controller { get { return m_controller; } }

        private IEditableDictionary<T> withEventsField_m_list;
        private IEditableDictionary<T> m_list
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

        public string AttributeName
        {
            get { return m_attributeName; }
        }

        public event DirtyEventHandler Dirty;
        public delegate void DirtyEventHandler(object sender, DataModifiedEventArgs args);
        public event RequestParentElementEditorSaveEventHandler RequestParentElementEditorSave { add { } remove { } }
        public delegate void RequestParentElementEditorSaveEventHandler();
        public event ExtraToolbarItemClickHandler ExtraToolbarItemClicked;
        public delegate void ExtraToolbarItemClickHandler(string action, string key);

        public void Initialise(EditorController controller, IEditorControl controlData, string keyHeader, string valueHeader)
        {
            m_controller = controller;
            ctlListEditor.EditorDelegate = this;
            ctlListEditor.Style = WFListEditor.ColumnStyle.TwoColumns;
            ctlListEditor.SetHeader(1, keyHeader);
            ctlListEditor.SetHeader(2, valueHeader);
            ctlListEditor.UpdateList(null);
            m_controller = controller;
            m_attributeName = controlData == null ? null : controlData.Attribute;
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
                m_list = value as IEditableDictionary<T>;
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
            DoAddKeyAction((ignore) => true, string.Empty);
        }

        public void DoAddKeyAction(Func<string, bool> keyAction, string suggestedNewKey)
        {
            var addKey = PopupEditors.EditString(m_controlData.GetString("keyprompt"), suggestedNewKey, GetAutoCompleteList());
            if (addKey.Cancelled) return;
            if (!ValidateInput(addKey.Result)) return;

            if (keyAction(addKey.Result))
            {
                AddNewValue(addKey);
            }
        }

        protected abstract void AddNewValue(PopupEditors.EditStringResult addKey);

        public void DoEdit(string key, int index)
        {
            EditValue(key);
        }

        protected abstract void EditValue(string key);

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

        private void m_list_Added(object sender, EditableListUpdatedEventArgs<T> e)
        {
            ctlListEditor.AddListItem(new KeyValuePair<string, string>(e.UpdatedItem.Key, GetDisplayString(e.UpdatedItem.Value)), e.Index);

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

        private void m_list_Removed(object sender, EditableListUpdatedEventArgs<T> e)
        {
            ctlListEditor.RemoveListItem(e.UpdatedItem.Key);
            if (Dirty != null)
            {
                Dirty(this, new DataModifiedEventArgs(null, m_list));
            }
        }

        private void m_list_Updated(object sender, EditableListUpdatedEventArgs<T> e)
        {
            var updatedIndex = e.Index;
            var newDisplayString = GetDisplayString(e.UpdatedItem.Value);
            ctlListEditor.UpdateValue(updatedIndex, newDisplayString);
            if (Dirty != null)
            {
                Dirty(this, new DataModifiedEventArgs(null, m_list));
            }
        }

        protected void PrepareForEditing()
        {
            // If we're currently displaying a dictionary which belongs to a type we inherit from,
            // we must clone the dictionary before we can edit it.

            if (m_list == null) return;

            if (m_list.Owner != m_data.Name)
            {
                Value = m_list.Clone(m_elementName, m_attributeName);
            }
        }

        public bool CanEditKey
        {
            get { return m_canEditKey; }
        }

        public void DoEditKey(string key, int index)
        {
            var newKey = PopupEditors.EditString(m_controlData.GetString("keyprompt"), key, GetAutoCompleteList());
            if (newKey.Cancelled || newKey.Result == key) return;
            if (!ValidateInput(newKey.Result)) return;
            m_controller.StartTransaction(string.Format("Update key '{0}' to '{1}'", key, newKey.Result));
            m_list.ChangeKey(key, newKey.Result);
            m_controller.EndTransaction();
        }

        public void DoAction(string action, string key)
        {
            if (ExtraToolbarItemClicked != null)
            {
                ExtraToolbarItemClicked(action, key);
            }
        }

        private IEnumerable<string> GetAutoCompleteList()
        {
            string listsource = m_controlData.GetString("source");
            if (listsource == null) return null;
            IEnumerable<string> result = (listsource == "object") ? m_controller.GetObjectNames("object") : m_controller.GetElementNames(listsource);
            string sourceexclude = m_controlData.GetString("sourceexclude");
            if (sourceexclude != null)
            {
                result = result.Where(s => s != sourceexclude);
            }
            return result.OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase);
        }

        protected void RaiseDirty(DataModifiedEventArgs args)
        {
            if (Dirty != null)
            {
                Dirty(this, args);
            }
        }

        public bool CanRemove(string[] keys)
        {
            return true;
        }
    }
}
