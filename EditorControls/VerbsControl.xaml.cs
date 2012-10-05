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
    [ControlType("verbs")]
    public partial class VerbsControl : UserControl, IMultiAttributeElementEditorControl, IControlDataHelper
    {
        public event EventHandler<DataModifiedEventArgs> Dirty;
        public event Action RequestParentElementEditorSave;
        private ControlDataOptions m_options = new ControlDataOptions();
        private IEditorData m_data;
        private EditorController m_controller;
        private IEditorControl m_definition;

        public VerbsControl()
        {
            InitializeComponent();
            m_options.MultipleAttributes = true;
            ctlAttributes.Dirty += ctlAttributes_Dirty;
            ctlAttributes.RequestParentElementEditorSave += ctlAttributes_RequestParentElementEditorSave;
        }

        void ctlAttributes_RequestParentElementEditorSave()
        {
            RequestParentElementEditorSave();
        }

        void ctlAttributes_Dirty(object sender, DataModifiedEventArgs args)
        {
            Dirty(sender, args);
        }

        private WFAttributesControl ctlAttributes
        {
            get { return (WFAttributesControl)attributesHost.Child; }
        }

        public IControlDataHelper Helper
        {
            get { return this; }
        }

        public void Populate(IEditorData data)
        {
            m_data = data;
            ctlAttributes.Populate(data);
        }

        public void Save()
        {
            ctlAttributes.Save();
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
            m_definition = definition;
            ctlAttributes.Initialise(controller, definition);
        }

        public void DoUninitialise()
        {
            m_controller = null;
            m_definition = null;
            ctlAttributes.Initialise(null, null);
        }

        public void AttributeChanged(string attribute, object value)
        {
            ctlAttributes.AttributeChanged(attribute, value);
        }

        public Control FocusableControl
        {
            get { return null; }
        }
    }
}