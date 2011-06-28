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
        private ControlDataOptions m_options = new ControlDataOptions();
        private IEditorData m_data;
        private EditorController m_controller;

        public event EventHandler<DataModifiedEventArgs> Dirty { add { } remove { } }
        public event Action RequestParentElementEditorSave { add { } remove { } }

        public ExitsControl()
        {
            InitializeComponent();
        }

        public IControlDataHelper Helper
        {
            get { return this; }
        }

        public void DoInitialise(EditorController controller, IEditorControl definition)
        {
            m_controller = controller;
        }

        private class ExitListData
        {
            public IEditableObjectReference ToRoom { get; set; }
            public string Alias { get; set; }
            public string To
            {
                get { return ToRoom == null ? null : ToRoom.Reference; }
            }
        }

        public void Populate(IEditorData data)
        {
            // TO DO: listen to Controller.ElementsUpdated and repopulate

            m_data = data;

            listView.Items.Clear();
            IEnumerable<string> exits = m_controller.GetObjectNames("exit", data.Name);
            foreach (string exit in exits)
            {
                IEditorData exitData = m_controller.GetEditorData(exit);

                ExitListData exitListData = new ExitListData
                {
                    ToRoom = exitData.GetAttribute("to") as IEditableObjectReference,
                    Alias = exitData.GetAttribute("alias") as string,
                };

                listView.Items.Add(exitListData);
            }
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
    }
}
