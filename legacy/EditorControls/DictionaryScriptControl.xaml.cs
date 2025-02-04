using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TextAdventures.Quest.EditorControls
{
    [ControlType("scriptdictionary")]
    public partial class DictionaryScriptControl : UserControl, IElementEditorControl
    {
        private ControlDataHelper<IEditableDictionary<IEditableScripts>> m_helper;
        private IEditorData m_data;

        public DictionaryScriptControl()
        {
            InitializeComponent();
            m_helper = new ControlDataHelper<IEditableDictionary<IEditableScripts>>(this);
            m_helper.Options.Resizable = true;
            m_helper.Initialise += m_helper_Initialise;
            m_helper.Uninitialise += m_helper_Uninitialise;

            ctlDictionaryScript.Dirty += ctlDictionaryScript_Dirty;
            ctlDictionaryScript.RequestParentElementEditorSave += ctlDictionaryScript_RequestParentElementEditorSave;
        }

        void ctlDictionaryScript_RequestParentElementEditorSave()
        {
            m_helper.RaiseRequestParentElementEditorSaveEvent();
        }

        void ctlDictionaryScript_Dirty(object sender, DataModifiedEventArgs args)
        {
            m_helper.RaiseDirtyEvent(args.NewValue);
        }

        void m_helper_Initialise()
        {
            ctlDictionaryScript.Initialise(m_helper.Controller, m_helper.ControlDefinition);
        }

        void m_helper_Uninitialise()
        {
            ctlDictionaryScript.Initialise(null, null);
        }

        private WFDictionaryScriptControl ctlDictionaryScript
        {
            get { return (WFDictionaryScriptControl)listHost.Child; }
        }

        public IControlDataHelper Helper
        {
            get { return m_helper; }
        }

        public void Populate(IEditorData data)
        {
            m_data = data;
            ctlDictionaryScript.Populate(data);
        }

        public void Save()
        {
            ctlDictionaryScript.Save(m_data);
        }

        public Control FocusableControl
        {
            get { return null; }
        }
    }
}
