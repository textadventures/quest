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
            ctlListEditor.EditorDelegate = this;
            ctlListEditor.Style = WFListEditor.ColumnStyle.OneColumn;
            ctlListEditor.UpdateList(null);
            m_controller = controller;
            m_controller.ElementsUpdated += m_controller_ElementsUpdated;
            m_controlData = controlData;

            m_elementType = controlData.GetString("elementtype");
            m_objectType = controlData.GetString("objecttype");
            m_filter = controlData.GetString("listfilter");

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

        public void Populate(IEditorData data)
        {
            m_data = data;

            IEnumerable<string> elements = null;
            if (m_elementType == "object")
            {
                string parent = m_data == null ? null : m_data.Name;
                elements = Controller.GetObjectNames(m_objectType, false, parent);
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
            Controller.StartTransaction("Reorder elements");
            Controller.SwapElements(key1, key2);
            Controller.EndTransaction();
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

        private void m_controller_ElementsUpdated()
        {
            Populate(m_data);
        }
    }
}
