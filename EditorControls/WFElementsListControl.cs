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
    public partial class WFElementsListControl : UserControl, IRearrangeableListEditorDelegate
    {
        private EditorController m_controller;
        private IEditorData m_data;
        private IEditorControl m_controlData;
        private string m_elementType;
        private string m_objectType;
        private string m_typeDesc;
        private string m_filter;

        public WFElementsListControl()
        {
            InitializeComponent();

            ctlListEditor.ToolbarClicked += ctlListEditor_ToolbarClicked;
        }

        public EditorController Controller
        {
            get { return m_controller; }
            set { m_controller = value; }
        }

        public event RequestParentElementEditorSaveEventHandler RequestParentElementEditorSave;
        public delegate void RequestParentElementEditorSaveEventHandler();

        public void Initialise(EditorController controller, IEditorControl controlData)
        {
            m_elementType = controlData.GetString("elementtype");
            m_objectType = controlData.GetString("objecttype");
            m_filter = controlData.GetString("listfilter");

            ctlListEditor.EditorDelegate = this;
            ctlListEditor.Style = WFListEditor.ColumnStyle.OneColumn;
            ctlListEditor.UpdateList(null);
            m_controller = controller;
            m_controller.ElementsUpdated += m_controller_ElementsUpdated;
            m_controller.ElementMoved += m_controller_ElementMoved;
            m_controlData = controlData;

            if (m_filter != null)
            {
                m_typeDesc = m_filter;
            }
            else if (m_objectType != null)
            {
                m_typeDesc = m_objectType;
            }
            else
            {
                m_typeDesc = m_elementType;
            }
        }

        public void Uninitialise()
        {
            m_controller.ElementsUpdated -= m_controller_ElementsUpdated;
            m_controller.ElementMoved -= m_controller_ElementMoved;
            m_controller = null;
            m_controlData = null;
            m_data = null;
        }

        public void Populate(IEditorData data)
        {
            m_data = data;

            IEnumerable<string> elements = null;
            if (m_elementType == "object")
            {
                string parent = m_data == null ? null : m_data.Name;
                if (parent == "game") parent = null;
                elements = Controller.GetObjectNames(m_objectType, false, parent, true);
            }
            else
            {
                elements = Controller.GetElementNames(m_elementType, false);
            }

            Dictionary<string, string> listItems = new Dictionary<string, string>();

            foreach (var element in elements.Where(e => Filter(e)))
            {
                listItems.Add(element, Controller.GetDisplayName(element));
            }

            ctlListEditor.UpdateList(listItems);
        }

        private bool Filter(string element)
        {
            if (m_filter == null) return true;
            return Controller.ElementIsVerb(element) == (m_filter == "verb");
        }

        public void Save(IEditorData data)
        {
        }

        public void DoAdd()
        {
            Controller.UIRequestAddElement(m_elementType, m_objectType, m_filter);
        }

        public void DoEdit(string key, int index)
        {
            Controller.UIRequestEditElement(key);
        }

        public void DoRemove(string[] keys)
        {
            Controller.StartTransaction(string.Format("Delete {0} {1}s", keys.Length, m_typeDesc));

            foreach (var key in keys)
            {
                // Deleting an element will delete any children, so we need to check that we've not already
                // deleted the element if the user selected multiple elements to delete
                if (Controller.ElementExists(key))
                {
                    Controller.DeleteElement(key, false);
                }
            }

            Controller.EndTransaction();
        }

        public void DoSwap(string key1, string key2)
        {
            string selectedItem = ctlListEditor.SelectedItem;
            Controller.StartTransaction("Reorder elements");
            Controller.SwapElements(key1, key2);
            Controller.EndTransaction();
            Populate(m_data);
            ctlListEditor.SetSelectedItem(selectedItem);
        }

        private void ctlListEditor_ToolbarClicked()
        {
            if (RequestParentElementEditorSave != null)
            {
                RequestParentElementEditorSave();
            }
        }

        public System.Type ExpectedType
        {
            get { return null; }
        }

        private void m_controller_ElementsUpdated(object sender, EventArgs e)
        {
            Populate(m_data);
        }

        private void m_controller_ElementMoved(object sender, TextAdventures.Quest.EditorController.ElementMovedEventArgs e)
        {
            Populate(m_data);
        }

        public bool CanRearrange
        {
            get
            {
                return m_elementType == "object";
            }
        }

        public bool CanEditKey
        {
            get { return false; }
        }

        public void DoEditKey(string key, int index)
        {
        }

        public bool CanRemove(string[] keys)
        {
            return keys.All(k => m_controller.CanDelete(k));
        }

        public void DoAction(string action, string key)
        {
        }
    }
}
