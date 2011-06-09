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

namespace AxeSoftware.Quest.EditorControls
{
    [ControlType("stringdictionary")]
    public partial class DictionaryStringControl : UserControl, IElementEditorControl
    {
        private ControlDataHelper<IEditableDictionary<string>> m_helper;
        private IEditorData m_data;

        public DictionaryStringControl()
        {
            InitializeComponent();
            m_helper = new ControlDataHelper<IEditableDictionary<string>>(this);
            m_helper.Options.Resizable = true;
            m_helper.Initialise += m_helper_Initialise;
            ctlDictionaryString.Dirty += ctlDictionaryString_Dirty;
            ctlDictionaryString.RequestParentElementEditorSave += ctlDictionaryString_RequestParentElementEditorSave;
        }

        void ctlDictionaryString_RequestParentElementEditorSave()
        {
            m_helper.RaiseRequestParentElementEditorSaveEvent();
        }

        void ctlDictionaryString_Dirty(object sender, DataModifiedEventArgs args)
        {
            m_helper.RaiseDirtyEvent(args.NewValue);
        }

        void m_helper_Initialise()
        {
            ctlDictionaryString.Initialise(m_helper.Controller, m_helper.ControlDefinition);
        }

        private WFDictionaryStringControl ctlDictionaryString
        {
            get { return (WFDictionaryStringControl)listHost.Child; }
        }

        public IControlDataHelper Helper
        {
            get { return m_helper; }
        }

        public void Populate(IEditorData data)
        {
            m_data = data;
            ctlDictionaryString.Populate(data);
        }

        public void Save()
        {
            ctlDictionaryString.Save(m_data);
        }

        public Control FocusableControl
        {
            get { return null; }
        }
    }
}
