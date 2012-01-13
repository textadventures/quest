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
            m_manager.Initialise(controller, controlData, "Value");
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

        public void DoAdd()
        {
            m_manager.DoAdd();
        }

        public void DoEdit(string key, int index)
        {
            m_manager.DoEdit(key, index);
        }

        public void DoRemove(string[] keys)
        {
            m_manager.DoRemove(keys);
        }

        public bool CanEditKey
        {
            get { return m_manager.CanEditKey; }
        }

        public void DoEditKey(string key, int index)
        {
            m_manager.DoEditKey(key, index);
        }
    }
}
