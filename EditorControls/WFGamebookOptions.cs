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
    public partial class WFGamebookOptions : UserControl
    {
        public event DirtyEventHandler Dirty;
        public delegate void DirtyEventHandler(object sender, DataModifiedEventArgs args);
        public event RequestParentElementEditorSaveEventHandler RequestParentElementEditorSave;
        public delegate void RequestParentElementEditorSaveEventHandler();

        private EditorController m_controller;
        private WFStringDictionaryEditorManager m_manager;

        public WFGamebookOptions()
        {
            InitializeComponent();
            m_manager = new WFStringDictionaryEditorManager(ctlListEditor);
            m_manager.Dirty += m_manager_Dirty;
            m_manager.RequestParentElementEditorSave += m_manager_RequestParentElementEditorSave;
            m_manager.ExtraToolbarItemClicked += m_manager_ExtraToolbarItemClicked;
            ctlListEditor.ShowExtraToolstripItems(new[] { "addpage", "link", "goto" });
            ctlListEditor.HideAddButton();
            ctlListEditor.SetEditButtonText("Edit Link Text");
        }

        private void m_manager_RequestParentElementEditorSave()
        {
            if (RequestParentElementEditorSave != null) RequestParentElementEditorSave();
        }

        private void m_manager_Dirty(object sender, DataModifiedEventArgs args)
        {
            if (Dirty != null) Dirty(sender, args);
        }

        private void m_manager_ExtraToolbarItemClicked(string action, string key)
        {
            switch (action)
            {
                case "addpage":
                    AddNewPage();
                    break;
                case "link":
                    LinkExistingPage();
                    break;
                case "goto":
                    GoToPage(key);
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        private void AddNewPage()
        {
            m_manager.DoAddKeyAction((newKey) =>
            {
                ValidationResult result = m_controller.CanAdd(newKey);
                if (!result.Valid)
                {
                    PopupEditors.DisplayValidationError(result, newKey, "Unable to add page");
                    return false;
                }
                m_controller.CreateNewObject(newKey, null, null);
                return true;
            }, m_controller.GetUniqueElementName("Page1"));
        }

        private void LinkExistingPage()
        {
            m_manager.DoAdd();
        }

        private void GoToPage(string key)
        {
            m_controller.UIRequestEditElement(key);
        }

        public void Initialise(EditorController controller, IEditorControl controlData)
        {
            m_controller = controller;
            m_manager.Initialise(controller, controlData, "Page", "Link text");
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
