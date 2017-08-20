using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TextAdventures.Quest.EditorControls
{
    public partial class WFAttributesControl : UserControl
    {
        public WFAttributesControl()
        {
            InitializeComponent();

            ctlMultiControl.Dirty += ctlMultiControl_Dirty;
            ctlMultiControl.RequestParentElementEditorSave += ctlMultiControl_RequestParentElementEditorSave;

            lstAttributes.ListViewItemSorter = m_attributesListSorter;
        }

        private EditorController m_controller;
        private IEditorControl m_controlData;
        private IEditorDataExtendedAttributeInfo m_data;
        private Dictionary<string, IEditorAttributeData> m_inheritedTypeData = new Dictionary<string, IEditorAttributeData>();
        private Dictionary<IDataWrapper, string> m_listeningToAttributes = new Dictionary<IDataWrapper, string>();

        private bool m_readOnly;
        public delegate void DirtyEventHandler(object sender, DataModifiedEventArgs args);
        public event DirtyEventHandler Dirty;
        public delegate void RequestParentElementEditorSaveEventHandler();
        public event RequestParentElementEditorSaveEventHandler RequestParentElementEditorSave;

        private Utility.ListViewColumnSorter m_attributesListSorter = new Utility.ListViewColumnSorter();

        public EditorController Controller
        {
            get { return m_controller; }
        }

        public string AttributeName
        {
            get { return null; }
        }

        public void Save()
        {
            ctlMultiControl.Save();
        }

        public void Populate(IEditorData data)
        {
            m_readOnly = data != null && data.ReadOnly;
            cmdAdd.Enabled = !m_readOnly;
            cmdAddType.Enabled = !m_readOnly;

            lstAttributes.Items.Clear();
            lstTypes.Items.Clear();
            m_inheritedTypeData.Clear();
            ClearListenedToAttributes();
            cmdDelete.Enabled = false;
            cmdOnChange.Enabled = false;
            cmdDeleteType.Enabled = false;
            ctlMultiControl.Visible = false;
            m_data = (IEditorDataExtendedAttributeInfo)data;

            if (data != null)
            {
                foreach (var type in m_data.GetInheritedTypes())
                {
                    m_inheritedTypeData.Add(type.AttributeName, type);
                    AddListItem(lstTypes, type, GetTypeDisplayString);
                }

                foreach (var attr in m_data.GetAttributeData())
                {
                    if (CanDisplayAttribute(attr.AttributeName, m_data.GetAttribute(attr.AttributeName), attr.Source != m_data.Name))
                    {
                        AddListItem(lstAttributes, attr, GetAttributeDisplayString);
                        ListenToAttribute(attr);
                    }
                }
            }
        }

        private void ClearListenedToAttributes()
        {
            foreach (IDataWrapper value in m_listeningToAttributes.Keys)
            {
                value.UnderlyingValueUpdated -= ListenedToValue_UnderlyingValueUpdated;
            }

            m_listeningToAttributes.Clear();
        }

        private void ListenToAttribute(IEditorAttributeData attr)
        {
            object value = m_data.GetAttribute(attr.AttributeName);
            IDataWrapper dataWrapperValue = value as IDataWrapper;
            if (dataWrapperValue != null)
            {
                m_listeningToAttributes.Add(dataWrapperValue, attr.AttributeName);
                dataWrapperValue.UnderlyingValueUpdated += ListenedToValue_UnderlyingValueUpdated;
            }
        }

        void ListenedToValue_UnderlyingValueUpdated(object sender, DataWrapperUpdatedEventArgs e)
        {
            IDataWrapper wrappedValue = (IDataWrapper)sender;
            string attribute = m_listeningToAttributes[wrappedValue];
            AttributeChangedInternal(attribute, wrappedValue, false);
        }

        protected virtual bool CanDisplayAttribute(string attribute, object value, bool isInherited)
        {
            return true;
        }

        private void AddListItem(IEditorAttributeData attr)
        {
            AddListItem(lstAttributes, attr, GetAttributeDisplayString);
        }

        private void AddListItem(ListView listView, IEditorAttributeData attr, Func<IEditorAttributeData, string> displayStringFunction)
        {
            ListViewItem newItem = listView.Items.Add(attr.AttributeName, GetAttributeDisplayName(attr), 0);
            newItem.ForeColor = GetAttributeColour(attr);
            string displayValue = displayStringFunction(attr);
            newItem.SubItems.Add(displayValue);
            newItem.SubItems.Add(attr.Source);
        }

        private string GetDisplayString(object value)
        {
            IEditableScripts scriptValue = value as IEditableScripts;
            IEditableList<string> listStringValue = value as IEditableList<string>;
            IEditableDictionary<string> dictionaryStringValue = value as IEditableDictionary<string>;
            IEditableDictionary<IEditableScripts> dictionaryScriptValue = value as IEditableDictionary<IEditableScripts>;
            IDataWrapper wrappedValue = value as IDataWrapper;
            string result = null;

            if (scriptValue != null)
            {
                result = scriptValue.DisplayString();
            }
            else if (listStringValue != null)
            {
                result = GetListDisplayString(listStringValue.DisplayItems);
            }
            else if (dictionaryStringValue != null)
            {
                result = GetDictionaryDisplayString(dictionaryStringValue.DisplayItems);
            }
            else if (dictionaryScriptValue != null)
            {
                result = GetDictionaryDisplayString(dictionaryScriptValue.DisplayItems);
            }
            else if (wrappedValue != null)
            {
                result = wrappedValue.DisplayString();
            }
            else if (value == null)
            {
                result = "(null)";
            }
            else
            {
                result = value.ToString();
            }

            return EditorUtility.FormatAsOneLine(result);
        }

        private string GetAttributeDisplayString(IEditorAttributeData attr)
        {
            return GetDisplayString(m_data.GetAttribute(attr.AttributeName));
        }

        private string GetTypeDisplayString(IEditorAttributeData attr)
        {
            return attr.AttributeName;
        }

        private string GetListDisplayString(IEnumerable<KeyValuePair<string, string>> items)
        {
            string result = string.Empty;

            foreach (var item in items)
            {
                if (result.Length > 0)
                    result += ", ";
                result += item.Value;
            }

            return result;
        }

        private string GetDictionaryDisplayString(IEnumerable<KeyValuePair<string, string>> items)
        {
            string result = string.Empty;

            foreach (var item in items)
            {
                if (result.Length > 0)
                    result += ", ";
                result += item.Key + "=" + item.Value;
            }

            return result;
        }

        public virtual void Initialise(EditorController controller, IEditorControl controlData)
        {
            m_controller = controller;
            m_controlData = controlData;
        }

        private void lstAttributes_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            string selectedAttribute = GetSelectedAttribute();
            cmdDelete.Enabled = DeleteAllowed(selectedAttribute);
            cmdOnChange.Enabled = AddChangeScriptAllowed(selectedAttribute);
            EditItem(selectedAttribute);
        }

        private string GetSelectedAttribute()
        {
            if (lstAttributes.SelectedItems.Count == 0) return null;
            return lstAttributes.SelectedItems[0].Name;
        }

        private bool DeleteAllowed(string attribute)
        {
            if (m_readOnly) return false;
            if (string.IsNullOrEmpty(attribute)) return false;
            if (attribute == "name" || attribute == "type" || attribute == "elementtype") return false;
            return !m_data.GetAttributeData(attribute).IsInherited;
        }

        private bool AddChangeScriptAllowed(string attribute)
        {
            if (m_readOnly) return false;
            if (string.IsNullOrEmpty(attribute)) return false;
            return true;
        }

        private void EditItem(string attribute)
        {
            if ((string.IsNullOrEmpty(attribute)))
            {
                ctlMultiControl.Visible = false;
                ctlMultiControl.Populate(null);
            }
            else
            {
                ctlMultiControl.Visible = true;
                IEditorControl controlData = GetControlData(attribute);
                ctlMultiControl.DoInitialise(m_controller, controlData);
                ctlMultiControl.Populate(m_data);
            }
        }

        protected virtual IEditorControl GetControlData(string attribute)
        {
            return new AttributeSubEditorControlData(attribute);
        }

        public void AttributeChanged(string attribute, object value)
        {
            AttributeChangedInternal(attribute, value, true);
        }

        private void AttributeChangedInternal(string attribute, object value, bool updateMultiControl)
        {
            ListViewItem listViewItem = lstAttributes.Items[attribute];

            if (value == null || !CanDisplayAttribute(attribute, value, false))
            {
                // Remove attribute
                lstAttributes.Items.Remove(listViewItem);
            }
            else
            {
                // Add or update attribute
                if (listViewItem == null)
                {
                    AddListItem(m_data.GetAttributeData(attribute));
                }
                else
                {
                    listViewItem.SubItems[1].Text = GetDisplayString(value);
                    IEditorAttributeData data = m_data.GetAttributeData(attribute);
                    listViewItem.SubItems[2].Text = data.Source;
                    listViewItem.ForeColor = GetAttributeColour(data);
                    if (updateMultiControl)
                    {
                        if (attribute == GetSelectedAttribute())
                        {
                            ctlMultiControl.Populate(m_data);
                        }
                    }
                }
            }
        }

        private void ctlMultiControl_Dirty(object sender, DataModifiedEventArgs args)
        {
            args.Attribute = GetSelectedAttribute();
            if (args.Attribute == null) return;
            AttributeChangedInternal(args.Attribute, args.NewValue, false);
            if (Dirty != null)
            {
                Dirty(this, args);
            }
        }

        void ctlMultiControl_RequestParentElementEditorSave()
        {
            RequestParentElementEditorSave();
        }

        private Color GetAttributeColour(IEditorAttributeData data)
        {
            return data.IsInherited || data.IsDefaultType ? Color.Gray : SystemColors.WindowText;
        }

        private void cmdAdd_Click(System.Object sender, System.EventArgs e)
        {
            if (m_readOnly) return;
            Add();
        }

        protected virtual void Add()
        {
            Add(string.Empty, () => string.Empty);
        }

        protected virtual void Add(string attributeName, Func<object> createAttributeValue)
        {
            if (attributeName.Length == 0)
            {
                PopupEditors.EditStringResult result = PopupEditors.EditString("Please enter a name for the new attribute", string.Empty);
                if (result.Cancelled) return;
                attributeName = result.Result;
            }

            bool setSelection = true;

            if (!lstAttributes.Items.ContainsKey(attributeName))
            {
                m_controller.StartTransaction(string.Format("Add '{0}' attribute", attributeName));

                ValidationResult setAttrResult = m_data.SetAttribute(attributeName, createAttributeValue());
                if (!setAttrResult.Valid)
                {
                    PopupEditors.DisplayValidationError(setAttrResult, attributeName, "Unable to add attribute");
                    setSelection = false;
                }

                m_controller.EndTransaction();
            }

            if (setSelection)
            {
                lstAttributes.Items[attributeName].Selected = true;
                lstAttributes.SelectedItems[0].EnsureVisible();
            }
        }

        private void cmdDelete_Click(System.Object sender, System.EventArgs e)
        {
            if (m_readOnly) return;
            string selectedAttribute = GetSelectedAttribute();
            m_controller.StartTransaction(string.Format("Remove '{0}' attribute", selectedAttribute));
            m_data.RemoveAttribute(selectedAttribute);
            m_controller.EndTransaction();
        }

        public System.Type ExpectedType
        {
            get { return null; }
        }

        private void cmdAddType_Click(object sender, EventArgs e)
        {
            if (m_readOnly) return;

            var availableTypes = m_controller.GetElementNames("type")
                                             .Where(t => !lstTypes.Items.ContainsKey(t))
                                             .Where(t => !m_controller.IsDefaultTypeName(t))
                                             .OrderBy(t => t);

            var result = PopupEditors.EditStringWithDropdown("Please choose a type to add", string.Empty, null, null,
                                                             string.Empty, availableTypes);

            if (result.Cancelled) return;

            if (!availableTypes.Contains(result.Result))
            {
                if (lstTypes.Items.ContainsKey(result.Result))
                {
                    MessageBox.Show(string.Format("Type '{0}' is already inherited", result.Result), "Invalid type",
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    MessageBox.Show(string.Format("Type '{0}' does not exist", result.Result), "Invalid type",
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                return;
            }

            var addResult = m_controller.AddInheritedTypeToElement(m_data.Name, result.Result, true);
            if (!addResult.Valid) PopupEditors.DisplayValidationError(addResult, null, "Unable to add type");
        }

        private void lstTypes_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            string selectedAttribute = GetSelectedType();
            cmdDeleteType.Enabled = DeleteTypeAllowed(selectedAttribute);
        }

        private string GetSelectedType()
        {
            if (lstTypes.SelectedItems.Count == 0) return null;
            return lstTypes.SelectedItems[0].Text;
        }

        private bool DeleteTypeAllowed(string type)
        {
            if (m_readOnly) return false;
            if (string.IsNullOrEmpty(type)) return false;
            IEditorAttributeData typeData = m_inheritedTypeData[type];
            return !(typeData.IsInherited || typeData.IsDefaultType);
        }

        private void cmdDeleteType_Click(System.Object sender, System.EventArgs e)
        {
            if (m_readOnly) return;
            string selectedType = GetSelectedType();
            m_controller.RemoveInheritedTypeFromElement(m_data.Name, selectedType, true);
        }

        protected IEditorData Data
        {
            get { return m_data; }
        }

        protected virtual string GetAttributeDisplayName(IEditorAttributeData attr)
        {
            return attr.AttributeName;
        }

        private void cmdOnChange_Click(object sender, EventArgs e)
        {
            if (m_readOnly) return;
            string selectedAttribute = GetSelectedAttribute();
            Add("changed" + selectedAttribute, () => m_controller.CreateNewEditableScripts(m_data.Name, m_controlData.Attribute, null, false));
        }

        private void lstAttributes_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            Utility.ListViewColumnSorter.SortList(lstAttributes, m_attributesListSorter, e.Column);
        }
    }
}
