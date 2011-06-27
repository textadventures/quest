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
    [ControlType("exits")]
    public partial class ExitsControl : UserControl, IElementEditorControl, IControlDataHelper
    {
        public event EventHandler<DataModifiedEventArgs> Dirty;
        public event Action RequestParentElementEditorSave;
        private ControlDataOptions m_options = new ControlDataOptions();

        public ExitsControl()
        {
            InitializeComponent();
        }

        public IControlDataHelper Helper
        {
            get { return this; }
        }

        public void Populate(IEditorData data)
        {
        }

        public void Save()
        {
        }

        public Control FocusableControl
        {
            get { return null; }
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
        }
    }
}
