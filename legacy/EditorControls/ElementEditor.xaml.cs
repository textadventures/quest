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
    public partial class ElementEditor : UserControl
    {
        private EditorController m_controller;
        private List<IElementEditorControl> m_controls = new List<IElementEditorControl>();
        private Dictionary<IEditorControl, List<UIElement>> m_controlUIElements = new Dictionary<IEditorControl, List<UIElement>>();
        private Dictionary<IEditorTab, TabItem> m_tabs = new Dictionary<IEditorTab, TabItem>();
        private IEditorData m_data;
        private bool m_lastRowIsResizable = false;
        private bool m_lastRowIsScrollableAndExpands = false;
        private bool m_simpleMode = false;

        public event EventHandler<DataModifiedEventArgs> Dirty;
        public event Action RequestParentElementEditorSave;

        public static void InitialiseEditorControls(EditorController controller)
        {
            foreach (Type t in TextAdventures.Utility.Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(), typeof(IElementEditorControl)))
            {
                ControlTypeAttribute controlType = (ControlTypeAttribute)Attribute.GetCustomAttribute(t, typeof(ControlTypeAttribute));
                if (controlType != null)
                {
                    controller.AddControlType(controlType.ControlType, t);
                }
            }
        }

        public ElementEditor()
        {
            InitializeComponent();
        }

        public void Initialise(EditorController controller, IEditorDefinition definition)
        {
            m_controller = controller;

            foreach (IEditorTab tabDefinition in definition.Tabs.Values)
            {
                InitialiseTab(tabDefinition);
            }
        }

        private void InitialiseTab(IEditorTab tab)
        {
            TabItem newTab = new TabItem();
            m_tabs.Add(tab, newTab);
            newTab.Header = tab.Caption;
            newTab.Padding = new Thickness(8, 4, 8, 4);
            tabControl.Items.Add(newTab);

            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.Padding = new Thickness(5);
            newTab.Content = scrollViewer;

            Grid controlGrid = new Grid();

            ColumnDefinition labelColumn = new ColumnDefinition();
            labelColumn.Width = new GridLength(1, GridUnitType.Auto);
            ColumnDefinition controlColumn = new ColumnDefinition();
            controlColumn.Width = new GridLength(1, GridUnitType.Star);
            controlGrid.ColumnDefinitions.Add(labelColumn);
            controlGrid.ColumnDefinitions.Add(controlColumn);

            scrollViewer.Content = controlGrid;

            foreach (IEditorControl ctl in tab.Controls)
            {
                AddControlToGrid(controlGrid, ctl);
            }

            if (m_lastRowIsResizable)
            {
                // If final grid row is resizable, add a dummy row underneath so it can be expanded
                AddRowToGrid(controlGrid);
            }

            if (m_lastRowIsScrollableAndExpands)
            {
                // If final grid row is scrollable and expands, disable the parent scrollviewer, otherwise
                // the final grid row will simply expand until its own content doesn't need to scroll.
                // For example, the TextEditorControl.
                scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
        }

        private List<Control> m_titleControls = new List<Control>();

        private void AddControlToGrid(Grid grid, IEditorControl ctl)
        {
            m_controlUIElements.Add(ctl, new List<UIElement>());

            // Add row
            int currentRow = AddRowToGrid(grid, ctl.Expand);
            bool controlDisplaysOwnCaption = false;
            bool resizableRow = false;
            bool scrollableControl = false;

            Control newControl = InitialiseEditorControl(ctl);
            m_controlUIElements[ctl].Add(newControl);
            newControl.Padding = new Thickness(6);

            if (ctl.ControlType == "title")
            {
                m_titleControls.Add(newControl);
            }

            IElementEditorControl elementEditorControl = newControl as IElementEditorControl;
            if (elementEditorControl != null)
            {
                controlDisplaysOwnCaption = elementEditorControl.Helper.Options.DisplaysOwnCaption;
                resizableRow = elementEditorControl.Helper.Options.Resizable;
                scrollableControl = elementEditorControl.Helper.Options.Scrollable;
            }

            if (!controlDisplaysOwnCaption && !string.IsNullOrEmpty(ctl.Caption))
            {
                Label newLabel = new Label();
                m_controlUIElements[ctl].Add(newLabel);
                newLabel.Content = ctl.Caption + ":";
                newLabel.Target = newControl;

                if (ctl.Caption.Length > 17)
                {
                    newLabel.Padding = new Thickness(5, 5, 5, 0);
                    newControl.Padding = new Thickness(5, 3, 5, 5);

                    // Create StackPanel, label at top and control underneath
                    Grid subGrid = new Grid();
                    AddRowToGrid(subGrid, false);
                    AddRowToGrid(subGrid, ctl.Expand || resizableRow);

                    m_controlUIElements[ctl].Add(subGrid);

                    // Add label and new control to subgrid
                    Grid.SetRow(newLabel, 0);
                    Grid.SetRow(newControl, 1);
                    subGrid.Children.Add(newLabel);
                    subGrid.Children.Add(newControl);

                    // Add subgrid to parent editor grid
                    Grid.SetColumn(subGrid, 0);
                    Grid.SetRow(subGrid, currentRow);
                    Grid.SetColumnSpan(subGrid, 2);
                    grid.Children.Add(subGrid);
                }
                else
                {
                    newLabel.Padding = new Thickness(5, 7, 5, 5);

                    // Add label to first column
                    Grid.SetColumn(newLabel, 0);
                    Grid.SetRow(newLabel, currentRow);
                    grid.Children.Add(newLabel);

                    // Add control to second column
                    Grid.SetColumn(newControl, 1);
                    Grid.SetRow(newControl, currentRow);
                    grid.Children.Add(newControl);
                }
            }
            else
            {
                // Add control to grid row, colspan=2
                Grid.SetColumn(newControl, 0);
                Grid.SetRow(newControl, currentRow);
                Grid.SetColumnSpan(newControl, 2);
                grid.Children.Add(newControl);
            }

            if (resizableRow)
            {
                // TO DO: Enforce minimum height for resizable rows

                int gridRow = AddRowToGrid(grid);
                GridSplitter splitter = new GridSplitter();
                splitter.Height = 3;
                splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
                splitter.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetColumnSpan(splitter, 2);
                Grid.SetRow(splitter, gridRow);
                grid.Children.Add(splitter);
                m_controlUIElements[ctl].Add(splitter);
            }

            m_lastRowIsResizable = resizableRow;
            m_lastRowIsScrollableAndExpands = scrollableControl && ctl.Expand;
        }

        private int AddRowToGrid(Grid grid, bool takeAllSpace = false)
        {
            RowDefinition newRowDefinition = new RowDefinition();
            GridUnitType gridUnitType = takeAllSpace ? GridUnitType.Star : GridUnitType.Auto;
            newRowDefinition.Height = new GridLength(1, gridUnitType);
            grid.RowDefinitions.Add(newRowDefinition);
            return grid.RowDefinitions.Count - 1;
        }

        private Control InitialiseEditorControl(IEditorControl ctl)
        {
            Control newControl = ControlFactory.CreateEditorControl(m_controller, ctl.ControlType);

            IElementEditorControl newElementEditorControl = newControl as IElementEditorControl;
            if (newElementEditorControl != null)
            {
                m_controls.Add(newElementEditorControl);
                newElementEditorControl.Helper.Dirty += Control_Dirty;
                newElementEditorControl.Helper.RequestParentElementEditorSave += Control_RequestParentElementEditorSave;
                newElementEditorControl.Helper.DoInitialise(m_controller, ctl);
            }

            return (Control)newControl;
        }

        void Control_Dirty(object sender, DataModifiedEventArgs e)
        {
            Dirty(sender, e);
        }

        void Control_RequestParentElementEditorSave()
        {
            RequestParentElementEditorSave();
        }

        public void Populate(IEditorData data)
        {
            if (m_data != null)
            {
                m_data.Changed -= m_data_Changed;
            }

            m_data = data;

            if (m_data != null)
            {
                m_data.Changed += m_data_Changed;
            }

            foreach (IElementEditorControl ctl in m_controls)
            {
                ctl.Populate(data);
            }

            UpdateControlVisibility();
        }

        void m_data_Changed(object sender, EventArgs e)
        {
            UpdateControlVisibility();
        }

        private void UpdateControlVisibility()
        {
            if (m_data == null || m_data.Name == null) return;    // might be in the middle of a delete

            // if the currently selected tab gets hidden, switch to the first visible tab
            bool switchToFirstVisibleTab = false;
            bool anyTabIsSelected = false;
            TabItem firstVisibleTab = null;

            foreach (var tab in m_tabs)
            {
                bool visible;
                if (SimpleMode && !tab.Key.IsTabVisibleInSimpleMode)
                {
                    visible = false;
                }
                else
                {
                    visible = tab.Key.IsTabVisible(m_data);
                }
                if (visible && firstVisibleTab == null) firstVisibleTab = tab.Value;
                if (!visible && tab.Value.IsSelected) switchToFirstVisibleTab = true;
                if (tab.Value.IsSelected) anyTabIsSelected = true;
                tab.Value.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            }

            if (switchToFirstVisibleTab || !anyTabIsSelected)
            {
                firstVisibleTab.IsSelected = true;
            }

            foreach (var ctl in m_controlUIElements)
            {
                bool visible = IsControlVisible(ctl.Key);
                Visibility visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                foreach (UIElement element in ctl.Value)
                {
                    element.Visibility = visibility;
                }
            }

            foreach (var titleControl in m_titleControls)
            {
                titleControl.Margin = new Thickness(0, 15, 0, 0);
            }

            foreach (var tab in m_tabs)
            {
                var firstVisibleControl = tab.Key.Controls.FirstOrDefault(IsControlVisible);
                if (firstVisibleControl == null) continue;
                if (firstVisibleControl.ControlType != "title") continue;
                var uiElement = m_controlUIElements[firstVisibleControl].FirstOrDefault() as Control;
                if (uiElement == null) continue;
                uiElement.Margin = new Thickness(0, 0, 0, 0);
            }
        }

        private bool IsControlVisible(IEditorControl ctl)
        {
            bool visible;
            if (SimpleMode && !ctl.IsControlVisibleInSimpleMode)
            {
                visible = false;
            }
            else
            {
                visible = ctl.IsControlVisible(m_data);
            }
            return visible;
        }

        public void Save()
        {
            foreach (IElementEditorControl ctl in m_controls)
            {
                ctl.Save();
            }
        }

        public void UpdateField(string attribute, bool setFocus)
        {
            var affectedControls = from ctl in m_controls where ctl.Helper.Attribute == attribute select ctl;
            foreach (var ctl in affectedControls)
            {
                ctl.Populate(m_data);
            }

            var multiAttributeControls = from ctl in m_controls where ctl.Helper.Options.MultipleAttributes select ctl;
            foreach (var ctl in multiAttributeControls)
            {
                ((IMultiAttributeElementEditorControl)ctl).AttributeChanged(attribute, m_data.GetAttribute(attribute));
            }
        }

        public void Uninitialise()
        {
            foreach (List<UIElement> ctlList in m_controlUIElements.Values)
            {
                foreach (UIElement element in ctlList)
                {
                    IElementEditorControl elementEditor = element as IElementEditorControl;
                    if (elementEditor != null)
                    {
                        elementEditor.Helper.Dirty -= Control_Dirty;
                        elementEditor.Helper.RequestParentElementEditorSave -= Control_RequestParentElementEditorSave;
                        elementEditor.Helper.DoUninitialise();
                    }

                    Grid grid = element as Grid;
                    if (grid != null)
                    {
                        grid.Children.Clear();
                    }

                    IDisposable disposableElement = element as IDisposable;
                }
                ctlList.Clear();
            }

            m_controlUIElements.Clear();

            foreach (TabItem tab in m_tabs.Values)
            {
                tab.Content = null;
            }

            tabControl.Items.Clear();

            m_tabs.Clear();
            m_controls.Clear();
            m_controller = null;
        }

        public bool SimpleMode
        {
            get { return m_simpleMode; }
            set
            {
                if (m_simpleMode != value)
                {
                    m_simpleMode = value;
                    UpdateControlVisibility();
                }
            }
        }
    }
}
