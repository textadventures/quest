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
    [ControlType("list")]
    public partial class ListStringControl : UserControl, IElementEditorControl
    {
        private ControlDataHelper<IEditableList<string>> m_helper;
        private IEditableList<string> m_list;
        private IEditorData m_data;

        public ListStringControl()
        {
            InitializeComponent();
            toolbar.IsItemSelected = false;
            m_helper = new ControlDataHelper<IEditableList<string>>(this);
            m_helper.Options.Resizable = true;
            m_helper.Options.Scrollable = true;
            m_helper.Initialise += m_helper_Initialise;
        }

        void m_helper_Initialise()
        {
            toolbar.ShowPlayRecord = m_helper.ControlDefinition.GetBool("iswalkthrough");
            toolbar.ShowMoveButtons = false;
        }

        public IControlDataHelper Helper
        {
            get { return m_helper; }
        }

        public void Populate(IEditorData data)
        {
            m_data = data;

            if (data == null)
            {
                CurrentList = null;
                return;
            }

            m_helper.StartPopulating();

            CurrentList = m_helper.Populate(data);
            listBox.IsEnabled = !data.ReadOnly && m_helper.CanEdit(data);
            toolbar.ReadOnly = !listBox.IsEnabled;

            m_helper.FinishedPopulating();
        }

        public void Save()
        {
        }

        private void PrepareForEditing()
        {
            // If we're currently displaying a list which belongs to a type we inherit from,
            // we must clone the list before we can edit it.

            if (m_list == null) return;
            if (m_list.Owner != m_data.Name)
            {
                CurrentList = m_list.Clone(m_data.Name, m_helper.ControlDefinition.Attribute);
            }
        }

        private IEditableList<string> CurrentList
        {
            get { return m_list; }
            set
            {
                m_list = value;
                listBox.ItemsSource = m_list;
            }
        }

        private List<IEditableListItem<string>> CurrentSelection
        {
            get
            {
                return listBox.SelectedItems.Cast<IEditableListItem<string>>().ToList();
            }
        }

        private void toolbar_AddClicked()
        {
            if (m_data.ReadOnly) return;
            var result = PopupEditors.EditString(m_helper.ControlDefinition.GetString("editprompt"), string.Empty);
            if (result.Cancelled) return;
            if (!ValidateInput(result.Result)) return;

            if (m_list == null)
            {
                CurrentList = m_helper.Controller.CreateNewEditableList(m_data.Name, m_helper.ControlDefinition.Attribute, result.Result, true);
            }
            else
            {
                PrepareForEditing();
                m_list.Add(result.Result);
            }
        }

        private void toolbar_EditClicked()
        {
            EditCurrentSelection();
        }

        private void EditCurrentSelection()
        {
            if (listBox.SelectedItem == null) return;
            if (m_data.ReadOnly) return;
            EditableListItem<string> currentSelection = (EditableListItem<string>)listBox.SelectedItem;
            int index = listBox.SelectedIndex;
            var result = PopupEditors.EditString(m_helper.ControlDefinition.GetString("editprompt"), currentSelection.Value);
            if (result.Cancelled) return;
            if (result.Result == currentSelection.Value) return;
            if (!ValidateInput(result.Result)) return;

            PrepareForEditing();
            m_list.Update(index, result.Result);
        }

        private void toolbar_DeleteClicked()
        {
            if (m_data.ReadOnly) return;
            string[] keys = listBox.SelectedItems.Cast<IEditableListItem<string>>().Select(i => i.Key).ToArray();
            PrepareForEditing();
            m_list.Remove(keys);
        }

        private void toolbar_PlayClicked()
        {
            m_helper.Controller.BeginWalkthrough(m_data.Name, false);
        }

        private void toolbar_RecordClicked()
        {
            m_helper.Controller.BeginWalkthrough(m_data.Name, true);
        }

        private void ListItemDoubleClick(object sender, RoutedEventArgs e)
        {
            EditCurrentSelection();
        }

        private bool ValidateInput(string input)
        {
            if (m_list == null) return true;
            ValidationResult result = m_list.CanAdd(input);
            if (result.Valid) return true;

            PopupEditors.DisplayValidationError(result, input, "Unable to add item");
            return false;
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            toolbar.IsItemSelected = (listBox.SelectedItems.Count > 0);
        }

        private WFToolbar toolbar
        {
            get { return (WFToolbar)toolbarHost.Child; }
        }

        public Control FocusableControl
        {
            get { return null; }
        }
    }
}
