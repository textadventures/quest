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
    public partial class IfEditor : UserControl, IElementEditorControl, IControlDataHelper
    {
        private ControlDataOptions m_options = new ControlDataOptions();
        private EditableIfScript m_data;
        private bool m_hasElse = false;
        private EditorController m_controller;
        private Dictionary<string, IfEditorChild> m_elseIfEditors = new Dictionary<string, IfEditorChild>();
        private List<RowDefinition> m_elseIfGridRows = new List<RowDefinition>();
        private bool m_readOnly;

        public event EventHandler<DataModifiedEventArgs> Dirty;
        public event Action RequestParentElementEditorSave;

        public IfEditor()
        {
            InitializeComponent();

            ctlChild.Dirty += RaiseDirtyEvent;
            ctlChild.RequestParentElementEditorSave += RaiseRequestParentElementEditorSaveEvent;
            ctlElse.Dirty += RaiseDirtyEvent;
            ctlElse.RequestParentElementEditorSave += RaiseRequestParentElementEditorSaveEvent;
            ctlElse.Delete += IfEditorChild_Delete;
            ctlElse.Mode = IfEditorChild.IfEditorChildMode.Else;
        }

        void RaiseDirtyEvent()
        {
            Dirty(this, new DataModifiedEventArgs(null, null));
        }

        void RaiseRequestParentElementEditorSaveEvent()
        {
            RequestParentElementEditorSave();
        }

        public IControlDataHelper Helper
        {
            get { return this; }
        }

        public void Populate(IEditorData data)
        {
            if (data == null)
            {
                Populate((EditableIfScript)null);
                return;
            }
            throw new NotImplementedException();
        }

        public void Populate(EditableIfScript data)
        {
            if (data == m_data)
            {
                // If repopulating with the same editable "if" script, we only
                // need to refresh the conditions, not the "Then", "Else" scripts etc.
                ctlChild.RefreshExpression(data);
                foreach (var elseIf in data.ElseIfScripts)
                {
                    m_elseIfEditors[elseIf.Id].RefreshExpression(elseIf);
                }
                return;
            }

            if (m_data != null)
            {
                m_data.AddedElse -= AddedElse;
                m_data.AddedElseIf -= AddedElseIf;
                m_data.RemovedElse -= RemovedElse;
                m_data.RemovedElseIf -= RemovedElseIf;
            }

            m_data = data;

            if (data != null)
            {
                data.AddedElse += AddedElse;
                data.AddedElseIf += AddedElseIf;
                data.RemovedElse += RemovedElse;
                data.RemovedElseIf += RemovedElseIf;
            }

            // remove all existing "else if" and "else" child controls
            RemoveElseChildControl();
            RemoveAllElseIfChildControls();

            // The expression is contained in the "expression" attribute of the data IEditorData
            ctlChild.ReadOnly = m_readOnly;
            ctlChild.Populate(data, data == null ? null : data.ThenScript);

            if (data != null)
            {
                // add else if controls
                foreach (var elseIfScript in data.ElseIfScripts)
                {
                    AddElseIfChildControl(elseIfScript);
                }

                // add else control
                if (data.ElseScript != null)
                {
                    AddElseChildControl();
                }
            }

            if (m_readOnly)
            {
                cmdAddElse.Visibility = Visibility.Collapsed;
                cmdAddElseIf.Visibility = Visibility.Collapsed;
            }
        }

        void AddedElse(object sender, EventArgs e)
        {
            AddElseChildControl();
        }

        void AddedElseIf(object sender, EditableIfScript.ElseIfEventArgs e)
        {
            AddElseIfChildControl(e.Script);
        }

        void RemovedElse(object sender, EventArgs e)
        {
            RemoveElseChildControl();
        }

        void RemovedElseIf(object sender, EditableIfScript.ElseIfEventArgs e)
        {
            RemoveElseIfChildControl(e.Script);
        }

        private void AddElseChildControl()
        {
            m_hasElse = true;

            // add to grid
            ctlElse.Visibility = Visibility.Visible;

            ctlElse.ReadOnly = m_readOnly;
            ctlElse.Populate(null, m_data.ElseScript);

            // remove "Add Else" button
            cmdAddElse.Visibility = Visibility.Collapsed;
        }

        private void RemoveElseChildControl()
        {
            m_hasElse = false;
            ctlElse.Populate(null, null);

            // remove from grid
            ctlElse.Visibility = Visibility.Collapsed;

            // add "Add Else" button
            cmdAddElse.Visibility = Visibility.Visible;
        }

        private void RemoveAllElseIfChildControls()
        {
            foreach (IfEditorChild child in m_elseIfEditors.Values)
            {
                RemoveElseIfEditor(child);
            }
            m_elseIfEditors.Clear();
        }

        private void RemoveElseIfEditor(IfEditorChild child)
        {
            child.Populate(null, null);
            child.ElseIfData = null;
            child.Uninitialise();
            child.Visibility = Visibility.Collapsed;
            grid.Children.Remove(child);
            child.Dirty -= RaiseDirtyEvent;
            child.RequestParentElementEditorSave -= RaiseRequestParentElementEditorSaveEvent;
            child.Delete -= IfEditorChild_Delete;
        }

        private void AddElseIfChildControl(EditableIfScript.EditableElseIf elseIfData)
        {
            IfEditorChild newChild = new IfEditorChild();
            newChild.Mode = IfEditorChild.IfEditorChildMode.ElseIf;
            newChild.Margin = new Thickness(0, 10, 0, 0);
            newChild.Dirty += RaiseDirtyEvent;
            newChild.RequestParentElementEditorSave += RaiseRequestParentElementEditorSaveEvent;
            newChild.Delete += IfEditorChild_Delete;
            newChild.Initialise(m_controller);
            newChild.ReadOnly = m_readOnly;
            newChild.Populate(elseIfData, elseIfData.EditableScripts);
            newChild.ElseIfData = elseIfData;

            m_elseIfEditors.Add(elseIfData.Id, newChild);

            // First ElseIf script is at Row 1
            int rowIndex = m_elseIfEditors.Count;

            if (m_elseIfEditors.Count > m_elseIfGridRows.Count)
            {
                RowDefinition newRow = new RowDefinition();
                newRow.Height = GridLength.Auto;
                grid.RowDefinitions.Insert(rowIndex, newRow);
                m_elseIfGridRows.Add(newRow);

                Grid.SetRow(cmdAddElseIf, Grid.GetRow(cmdAddElseIf) + 1);
                Grid.SetRow(cmdAddElse, Grid.GetRow(cmdAddElse) + 1);
                Grid.SetRow(ctlElse, Grid.GetRow(ctlElse) + 1);
            }

            Grid.SetRow(newChild, rowIndex);
            grid.Children.Add(newChild);
        }

        private void RemoveElseIfChildControl(EditableIfScript.EditableElseIf elseIfData)
        {
            RemoveElseIfEditor(m_elseIfEditors[elseIfData.Id]);
            m_elseIfEditors.Remove(elseIfData.Id);
        }

        public void Save()
        {
            ctlChild.Save();
            foreach (IfEditorChild elseIfEditor in m_elseIfEditors.Values)
            {
                elseIfEditor.Save();
            }
            if (m_hasElse)
            {
                ctlElse.Save();
            }
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
            ctlChild.Initialise(controller);
            ctlElse.Initialise(controller);
        }

        public void DoUninitialise()
        {
            m_controller = null;
            ctlChild.Uninitialise();
            ctlElse.Uninitialise();
        }

        private void cmdAddElseIf_Click(object sender, RoutedEventArgs e)
        {
            m_data.AddElseIf();
            RaiseDirtyEvent();
        }

        private void cmdAddElse_Click(object sender, RoutedEventArgs e)
        {
            m_data.AddElse();
            RaiseDirtyEvent();
        }

        private void IfEditorChild_Delete(IfEditorChild sender)
        {
            if (sender == ctlElse)
            {
                m_data.RemoveElse();
            }
            else
            {
                m_data.RemoveElseIf(sender.ElseIfData);
            }
        }

        public Control FocusableControl
        {
            get { return ctlChild.FocusableControl; }
        }

        public bool ReadOnly
        {
            get { return m_readOnly; }
            set { m_readOnly = value; }
        }
    }
}
