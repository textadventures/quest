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
        private List<string> m_directionNames;

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
            m_controller.ElementsUpdated += m_controller_ElementsUpdated;

            m_directionNames = new List<string>(definition.GetListString("compass"));
        }

        void m_controller_ElementsUpdated()
        {
            Populate(m_data);
        }

        private class ExitListData
        {
            public string Name { get; set; }
            public IEditableObjectReference ToRoom { get; set; }
            public string Alias { get; set; }
            public string To
            {
                get { return ToRoom == null ? null : ToRoom.Reference; }
            }
        }

        public void Populate(IEditorData data)
        {
            m_data = data;

            listView.Items.Clear();
            compassControl.Clear();

            IEnumerable<string> exits = m_controller.GetObjectNames("exit", data.Name);
            foreach (string exit in exits)
            {
                IEditorData exitData = m_controller.GetEditorData(exit);

                ExitListData exitListData = new ExitListData
                {
                    Name = exitData.Name,
                    ToRoom = exitData.GetAttribute("to") as IEditableObjectReference,
                    Alias = exitData.GetAttribute("alias") as string,
                };

                listView.Items.Add(exitListData);

                if (m_directionNames.Contains(exitListData.Alias))
                {
                    int direction = m_directionNames.IndexOf(exitListData.Alias);
                    compassControl.Populate(direction, exitListData.To);
                }
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

        private WFToolbar toolbar
        {
            get { return (WFToolbar)toolbarHost.Child; }
        }

        private void toolbar_AddClicked()
        {
            m_controller.UIRequestAddElement("object", "exit", "");
        }

        private void toolbar_EditClicked()
        {
            if (listView.SelectedItem == null) return;
            ExitListData currentSelection = (ExitListData)listView.SelectedItem;
            m_controller.UIRequestEditElement(currentSelection.Name);
        }

        private void toolbar_DeleteClicked()
        {
            string[] keys = listView.SelectedItems.Cast<ExitListData>().Select(i => i.Name).ToArray();
            if (keys.Length == 0) return;

            m_controller.StartTransaction(string.Format("Delete {0} exits", keys.Length));

            foreach (var key in keys)
            {
                m_controller.DeleteElement(key, false);
            }

            m_controller.EndTransaction();
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            toolbar.IsItemSelected = (listView.SelectedItems.Count > 0);
        }
    }
}
