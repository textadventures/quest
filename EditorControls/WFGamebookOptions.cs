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
    public partial class WFGamebookOptions : UserControl
    {
        public event DirtyEventHandler Dirty;
        public delegate void DirtyEventHandler(object sender, DataModifiedEventArgs args);
        public event RequestParentElementEditorSaveEventHandler RequestParentElementEditorSave;
        public delegate void RequestParentElementEditorSaveEventHandler();

        private WFStringDictionaryEditorManager m_manager;

        public WFGamebookOptions()
        {
            InitializeComponent();
            m_manager = new WFStringDictionaryEditorManager(ctlListEditor);
            m_manager.Dirty += m_manager_Dirty;
            m_manager.RequestParentElementEditorSave += m_manager_RequestParentElementEditorSave;
            m_manager.ExtraToolbarItemClicked += m_manager_ExtraToolbarItemClicked;
            ctlListEditor.ShowExtraToolstripItems(new[] { "addpage", "link" });
            ctlListEditor.HideAddButton();
        }

        private void m_manager_RequestParentElementEditorSave()
        {
            if (RequestParentElementEditorSave != null) RequestParentElementEditorSave();
        }

        private void m_manager_Dirty(object sender, DataModifiedEventArgs args)
        {
            if (Dirty != null) Dirty(sender, args);
        }

        private void m_manager_ExtraToolbarItemClicked(string item)
        {
            switch (item)
            {
                case "addpage":
                    AddNewPage();
                    break;
                case "link":
                    LinkExistingPage();
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        private void AddNewPage()
        {
            string result = m_manager.DoAddResult();
            if (result != null)
            {
                MessageBox.Show("Add new page... " + result);
            }
        }

        private void LinkExistingPage()
        {
            m_manager.DoAdd();
        }

        public void Initialise(EditorController controller, IEditorControl controlData)
        {
            m_manager.Initialise(controller, controlData, "Value");
        }

        public void Populate(IEditorData data)
        {
            m_manager.Populate(data);
        }

        public void Save(IEditorData data)
        {
        }
    }
}
