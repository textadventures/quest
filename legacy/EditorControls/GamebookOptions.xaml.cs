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
    [ControlType("gamebookoptions")]
    public partial class GamebookOptions : UserControl, IElementEditorControl
    {
        private ControlDataHelper<IEditableDictionary<string>> m_helper;
        private IEditorData m_data;

        public GamebookOptions()
        {
            InitializeComponent();
            m_helper = new ControlDataHelper<IEditableDictionary<string>>(this);
            // "Resizable" option doesn't work well for WinForms usercontrols - this should be made resizable
            // when the control is fully converted to WPF.
            //m_helper.Options.Resizable = true;
            m_helper.Initialise += m_helper_Initialise;
            m_helper.Uninitialise += m_helper_Uninitialise;
            ctlGamebookOptions.Dirty += ctlDictionaryString_Dirty;
            ctlGamebookOptions.RequestParentElementEditorSave += ctlDictionaryString_RequestParentElementEditorSave;
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
            ctlGamebookOptions.Initialise(m_helper.Controller, m_helper.ControlDefinition);
        }

        void m_helper_Uninitialise()
        {
            ctlGamebookOptions.Initialise(null, null);
        }

        private WFGamebookOptions ctlGamebookOptions
        {
            get { return (WFGamebookOptions)listHost.Child; }
        }

        public IControlDataHelper Helper
        {
            get { return m_helper; }
        }

        public void Populate(IEditorData data)
        {
            m_data = data;
            ctlGamebookOptions.Populate(data);
        }

        public void Save()
        {
            ctlGamebookOptions.Save(m_data);
        }

        public Control FocusableControl
        {
            get { return null; }
        }
    }
}
