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
    public partial class WFDictionaryStringControl : UserControl
    {
        public event DirtyEventHandler Dirty;
        public delegate void DirtyEventHandler(object sender, DataModifiedEventArgs args);
        public event RequestParentElementEditorSaveEventHandler RequestParentElementEditorSave;
        public delegate void RequestParentElementEditorSaveEventHandler();

        private WFStringDictionaryEditorManager m_manager;

        public WFDictionaryStringControl()
        {
            InitializeComponent();
            m_manager = new WFStringDictionaryEditorManager(ctlListEditor);
            m_manager.Dirty += m_manager_Dirty;
            m_manager.RequestParentElementEditorSave += m_manager_RequestParentElementEditorSave;
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
            m_manager.Initialise(controller, controlData, "Key", "Value");
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
