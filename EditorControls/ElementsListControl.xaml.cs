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
    [ControlType("elementslist")]
    public partial class ElementsListControl : UserControl, IElementEditorControl, IControlDataHelper
    {
        private ControlDataOptions m_options = new ControlDataOptions();
        private EditorController m_controller;
        private IEditorData m_data;

        public event EventHandler<DataModifiedEventArgs> Dirty { add { } remove { } }
        public event Action RequestParentElementEditorSave;

        public ElementsListControl()
        {
            InitializeComponent();
            ctlElementsList.RequestParentElementEditorSave += ctlElementsList_RequestParentElementEditorSave;
        }

        void ctlElementsList_RequestParentElementEditorSave()
        {
            RequestParentElementEditorSave();
        }

        public IControlDataHelper Helper
        {
            get { return this; }
        }

        public void Populate(IEditorData data)
        {
            m_data = data;
            ctlElementsList.Populate(data);
        }

        public void Save()
        {
            ctlElementsList.Save(m_data);
        }

        public Type ExpectedType
        {
            get { return null; }
        }

        public string Attribute
        {
            get { return null; }
        }

        public ControlDataOptions Options
        {
            get { return m_options; }
        }

        public void DoInitialise(EditorController controller, IEditorControl definition)
        {
            m_controller = controller;
            ctlElementsList.Initialise(controller, definition);
        }

        public void DoUninitialise()
        {
            m_controller = null;
            ctlElementsList.Uninitialise();
        }

        private WFElementsListControl ctlElementsList
        {
            get { return (WFElementsListControl)elementsListHost.Child; }
        }

        public Control FocusableControl
        {
            get { return null; }
        }
    }
}
