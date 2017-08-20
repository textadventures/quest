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
using Microsoft.Win32;

namespace TextAdventures.Quest.EditorControls
{
    [ControlType("exits")]
    public partial class ExitsControl : UserControl, IElementEditorControl, IControlDataHelper
    {
        private ControlDataOptions m_options = new ControlDataOptions();
        private IEditorData m_data;
        private EditorController m_controller;
        private List<string> m_directionNames;
        private IDictionary<string, string> m_compassTypes;
        private Dictionary<string, int> m_directionListIndexes = new Dictionary<string, int>();
        private bool m_selectionChanging;

        public event EventHandler<DataModifiedEventArgs> Dirty { add { } remove { } }
        public event Action RequestParentElementEditorSave { add { } remove { } }

        public ExitsControl()
        {
            InitializeComponent();
            toolbar.ShowPlayRecord = false;
            toolbar.ShowMoveButtons = true;
            compassControl.HyperlinkClicked += compassControl_HyperlinkClicked;
            compassControl.SelectionChanged += compassControl_SelectionChanged;
            CompassEditor.EditExit += CompassEditor_EditExit;
            CompassEditor.CreateExit += CompassEditor_CreateExit;
            CompassEditor.CreateInverseExit += CompassEditor_CreateInverseExit;
            CompassEditor.CheckInverseExit += CompassEditor_CheckInverseExit;
        }

        public IControlDataHelper Helper
        {
            get { return this; }
        }

        public void DoInitialise(EditorController controller, IEditorControl definition)
        {
            m_controller = controller;
            m_controller.ElementsUpdated += m_controller_ElementsUpdated;
            m_controller.ElementMoved += m_controller_ElementMoved;
            m_controller.SimpleModeChanged += m_controller_SimpleModeChanged;

            m_directionNames = new List<string>(definition.GetListString("compass"));
            m_compassTypes = definition.GetDictionary("compasstypes");
        }

        public void DoUninitialise()
        {
            m_controller.ElementsUpdated -= m_controller_ElementsUpdated;
            m_controller.ElementMoved -= m_controller_ElementMoved;
            m_controller.SimpleModeChanged -= m_controller_SimpleModeChanged;
            m_controller = null;
            m_directionNames = null;
            m_compassTypes = null;
        }

        void m_controller_ElementsUpdated(object sender, EventArgs e)
        {
            Populate(m_data);
        }

        void m_controller_ElementMoved(object sender, TextAdventures.Quest.EditorController.ElementMovedEventArgs e)
        {
            Populate(m_data);
        }

        void m_controller_SimpleModeChanged(object sender, EventArgs e)
        {
            CompassEditor.SimpleMode = m_controller.SimpleMode;
        }

        private class ExitListData
        {
            public string Name { get; set; }
            public IEditableObjectReference ToRoom { get; set; }
            public string Alias { get; set; }
            public bool LookOnly { get; set; }
            public string To
            {
                get
                {
                    if (LookOnly) return "(look)";
                    return ToRoom == null ? null : ToRoom.Reference;
                }
            }
        }

        public void Populate(IEditorData data)
        {
            m_data = data;

            listView.Items.Clear();
            compassControl.Clear();
            m_directionListIndexes.Clear();
            CompassEditor.SimpleMode = m_controller.SimpleMode;
            PopulateCompassEditor(null);

            if (data != null)
            {
                IEnumerable<string> exits = m_controller.GetObjectNames("exit", data.Name, true);
                foreach (string exit in exits)
                {
                    IEditorData exitData = m_controller.GetEditorData(exit);

                    ExitListData exitListData = new ExitListData
                    {
                        Name = exitData.Name,
                        ToRoom = exitData.GetAttribute("to") as IEditableObjectReference,
                        Alias = exitData.GetAttribute("alias") as string,
                        LookOnly = (exitData.GetAttribute("lookonly") as bool? == true)
                    };

                    int addedIndex = listView.Items.Add(exitListData);

                    if (m_directionNames.Contains(exitListData.Alias))
                    {
                        int direction = m_directionNames.IndexOf(exitListData.Alias);
                        compassControl.Populate(direction, exitListData.To, exitListData.LookOnly);
                        m_directionListIndexes[exitListData.Alias] = addedIndex;
                    }
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
            EditSelectedItem();
        }

        private void EditSelectedItem()
        {
            if (listView.SelectedItem == null) return;
            m_controller.UIRequestEditElement(SelectedExit.Name);
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

        private void toolbar_MoveUpClicked()
        {
            if (listView.SelectedItem == null) return;
            int index = listView.SelectedIndex;
            Swap(index, index - 1);
        }

        private void toolbar_MoveDownClicked()
        {
            if (listView.SelectedItem == null) return;
            int index = listView.SelectedIndex;
            Swap(index, index + 1);
        }

        private void Swap(int index1, int index2)
        {
            string key1 = ((ExitListData)listView.Items[index1]).Name;
            string key2 = ((ExitListData)listView.Items[index2]).Name;

            m_controller.StartTransaction("Reorder elements");
            m_controller.SwapElements(key1, key2);
            m_controller.EndTransaction();
            Populate(m_data);
            listView.SelectedItem = listView.Items[index2];
        }

        private ExitListData SelectedExit
        {
            get
            {
                if (listView.SelectedItem == null) return null;
                return (ExitListData)listView.SelectedItem;
            }
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            toolbar.IsItemSelected = (listView.SelectedItems.Count > 0);
            if (listView.SelectedItems.Count == 1)
            {
                toolbar.CanMoveUp = listView.SelectedIndex > 0;
                toolbar.CanMoveDown = listView.SelectedIndex < listView.Items.Count - 1;
            }
            else
            {
                toolbar.CanMoveUp = false;
                toolbar.CanMoveDown = false;
            }

            if (m_selectionChanging) return;
            if (SelectedExit == null)
            {
                compassControl.SelectedDirection = null;
                PopulateCompassEditor(null);
            }
            else
            {
                int? dirIndex = m_directionNames.IndexOf(SelectedExit.Alias);
                if (dirIndex == -1) dirIndex = null;
                compassControl.SelectedDirection = dirIndex;
                PopulateCompassEditor(SelectedExit.Alias ?? string.Empty);
            }
        }

        private void compassControl_HyperlinkClicked(string destination)
        {
            m_controller.UIRequestEditElement(destination);
        }

        private void compassControl_SelectionChanged(int dirIndex)
        {
            string direction = m_directionNames[dirIndex];
            m_selectionChanging = true;
            listView.SelectedItems.Clear();
            if (m_directionListIndexes.ContainsKey(direction))
            {
                int listIndex = m_directionListIndexes[direction];
                listView.SelectedItem = listView.Items[listIndex];
            }
            m_selectionChanging = false;
            PopulateCompassEditor(direction);
        }

        private void PopulateCompassEditor(string direction)
        {
            // Populate the Compass Editor (top-right) in response to a selection change in
            // either the compass or exits list. There are four possibilities for direction:
            //   - an existing compass direction
            //   - a new compass direction
            //   - not a compass direction
            //   - null

            if (direction == null)
            {
                CompassEditor.Mode = CompassEditorControl.CompassEditorMode.NoSelection;
                return;
            }
            else if (!m_directionNames.Contains(direction))
            {
                CompassEditor.Mode = CompassEditorControl.CompassEditorMode.NotACompassExit;
                return;
            }
            else if (m_directionListIndexes.ContainsKey(direction))
            {
                CompassEditor.Mode = CompassEditorControl.CompassEditorMode.ExistingCompassExit;
                ExitListData data = (ExitListData)listView.Items[m_directionListIndexes[direction]];
                CompassEditor.toName.Text = data.To;
                CompassEditor.ExitID = data.Name;
                CompassEditor.Destination = data.To;
                CompassEditor.DirectionName = direction;

                if (data.LookOnly)
                {
                    CompassEditor.CorrespondingExitExists = false;
                    CompassEditor.AllowCreateInverseExit = false;
                }
                else
                {
                    // See if a corresponding exit exists in the "to" room
                    bool exitsInverseExists = ExitsInverseExists(CompassEditor.Destination, direction);
                    CompassEditor.CorrespondingExitExists = exitsInverseExists;
                    CompassEditor.AllowCreateInverseExit = !exitsInverseExists;
                }
            }
            else
            {
                CompassEditor.Mode = CompassEditorControl.CompassEditorMode.NewCompassExit;
                CompassEditor.to.Items.Clear();
                foreach (string objectName in m_controller.GetObjectNames("object")
                    .Where(n => n != m_data.Name)
                    .OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase))
                {
                    CompassEditor.to.Items.Add(objectName);
                }
                CompassEditor.create.IsEnabled = false;
                CompassEditor.to.IsEnabled = true;
                CompassEditor.DirectionName = direction;
                CompassEditor.AllowCreateInverseExit = true;
            }

            CompassEditor.direction.Text = TextAdventures.Utility.Strings.CapFirst(direction);
            CompassEditor.chkCorresponding.IsChecked = DefaultCreateInverseSetting;
        }

        private CompassEditorControl CompassEditor
        {
            get { return compassControl.compassEditor; }
        }

        private void CompassEditor_EditExit(string exitName)
        {
            m_controller.UIRequestEditElement(exitName);
        }

        private void CompassEditor_CreateExit(object sender, CompassEditorControl.CreateExitEventArgs e)
        {
            string newExit;
            if (CompassEditor.AllowCreateInverseExit) DefaultCreateInverseSetting = e.CreateInverse;
            if (e.CreateInverse)
            {
                newExit = m_controller.CreateNewExit(m_data.Name, e.To, e.Direction, GetInverseDirection(e.Direction), GetDirectionType(e.Direction), GetDirectionType(GetInverseDirection(e.Direction)));
            }
            else
            {
                newExit = m_controller.CreateNewExit(m_data.Name, e.To, e.Direction, GetDirectionType(e.Direction), e.LookOnly);
            }
        }

        void CompassEditor_CreateInverseExit()
        {
            string inverseDir = GetInverseDirection(CompassEditor.DirectionName);
            string from = CompassEditor.Destination;
            string to = m_data.Name;
            string newExit = m_controller.CreateNewExit(from, to, inverseDir, GetDirectionType(inverseDir), false);
            m_controller.UIRequestEditElement(newExit);
        }

        private string GetDirectionType(string direction)
        {
            if (m_compassTypes.ContainsKey(direction))
            {
                return m_compassTypes[direction];
            }
            throw new ArgumentOutOfRangeException(string.Format("Unknown direction {0}", direction));
        }

        private static List<int> s_oppositeDirs = new List<int> { 7, 6, 5, 4, 3, 2, 1, 0, 9, 8, 11, 10 };

        private string GetInverseDirection(string direction)
        {
            // 0 = NW, 1 = N, 2 = NE
            // 3 = W ,        4 = E     8 = U, 10 = In
            // 5 = SW, 6 = S, 7 = SE    9 = D, 11 = Out

            // So opposites are:

            //   0 <--> 7
            //   1 <--> 6
            //   2 <--> 5
            //   3 <--> 4
            //   4 <--> 3
            //   5 <--> 2
            //   6 <--> 1
            //   7 <--> 0
            //   8 <--> 9
            //   9 <--> 8
            //  10 <--> 11
            //  11 <--> 10

            int dirIndex = m_directionNames.IndexOf(direction);
            int opposite = s_oppositeDirs[dirIndex];
            if (opposite == -1) return null;
            return m_directionNames[opposite];
        }

        private const string k_regPath = @"Software\Quest\Settings";
        private const string k_regName = "DefaultCreateInverseSetting";

        private bool DefaultCreateInverseSetting
        {
            get
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(k_regPath);
                int? value = key.GetValue(k_regName) as int?;
                return (value ?? 1) == 1;
            }
            set
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(k_regPath);
                key.SetValue(k_regName, value ? 1 : 0);
            }
        }

        private void listView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditSelectedItem();
        }

        private void CompassEditor_CheckInverseExit(object sender, CompassEditorControl.CreateExitEventArgs e)
        {
            if (e.To != null)
            {
                bool exitsInverseExists = ExitsInverseExists(e.To, e.Direction);
                CompassEditor.CorrespondingExitExists = exitsInverseExists;
                CompassEditor.AllowCreateInverseExit = !exitsInverseExists;
            }
        }

        private bool ExitsInverseExists(string to, string direction)
        {
            string inverseExit = GetInverseDirection(direction);
            bool inverseExitExists = false;

            foreach (string exitName in m_controller.GetObjectNames("exit", to, true))
            {
                IEditorData otherRoomExitData = m_controller.GetEditorData(exitName);
                bool otherRoomLookOnly = otherRoomExitData.GetAttribute("lookonly") as bool? == true;
                if (!otherRoomLookOnly)
                {
                    string otherRoomExitAlias = otherRoomExitData.GetAttribute("alias") as string;
                    inverseExitExists = (inverseExit == otherRoomExitAlias);
                    if (inverseExitExists) break;
                }
            }
            return inverseExitExists;
        }
    }
}
