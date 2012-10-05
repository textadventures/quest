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
    public partial class WFDictionaryScriptControl : UserControl
    {
        public event DirtyEventHandler Dirty;
        public delegate void DirtyEventHandler(object sender, DataModifiedEventArgs args);
        public event RequestParentElementEditorSaveEventHandler RequestParentElementEditorSave;
        public delegate void RequestParentElementEditorSaveEventHandler();

        private WFScriptDictionaryEditorManager m_manager;

        public WFDictionaryScriptControl()
        {
            InitializeComponent();
            m_manager = new WFScriptDictionaryEditorManager(ctlListEditor);
            m_manager.Dirty += m_manager_Dirty;
            m_manager.RequestParentElementEditorSave += m_manager_RequestParentElementEditorSave;
            ctlListEditor.ToolbarClicked += ctlListEditor_ToolbarClicked;
        }

        private void m_manager_RequestParentElementEditorSave()
        {
            if (RequestParentElementEditorSave != null) RequestParentElementEditorSave();
        }

        private void m_manager_Dirty(object sender, DataModifiedEventArgs args)
        {
            if (Dirty != null) Dirty(sender, args);
        }

        public void Initialise(EditorController controller, IEditorControl controlData)
        {
            string keyName = "Key";
            if (controlData != null) keyName = controlData.GetString("keyname") ?? keyName;
            m_manager.Initialise(controller, controlData, keyName, "Script");
            ctlListEditor.SetEditKeyButtonText("Edit " + keyName);
            ctlListEditor.SetEditButtonText("Edit Script");
        }

        public void Populate(IEditorData data)
        {
            m_manager.Populate(data);
        }

        public void Save(IEditorData data)
        {
        }

        public object Value
        {
            get { return m_manager.Value; }
            set
            {
                m_manager.Value = value;
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
