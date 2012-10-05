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
    public partial class IfEditorChild : UserControl
    {
        public enum IfEditorChildMode
        {
            If,
            ElseIf,
            Else
        }

        private IfEditorChildMode m_mode;
        private bool m_readOnly;

        public event Action Dirty;
        public event Action RequestParentElementEditorSave;
        public event Action<IfEditorChild> Delete;

        public IfEditorChild()
        {
            InitializeComponent();
            ctlExpression.IsSimpleModeAvailable = false;
            ctlExpression.UseExpressionTemplates = true;
            ctlExpression.ExpressionTypeTemplateFilter = "if";
            Mode = IfEditorChildMode.If;
        }

        public void Initialise(EditorController controller)
        {
            ctlExpression.Helper.DoInitialise(controller, IfExpressionControlDefinition.Instance);
            ctlScriptExpander.DoInitialise(controller, null);
            ctlScriptExpander.Delete += ctlScriptExpander_Delete;
            ctlExpression.Helper.Dirty += RaiseDirty;
            ctlExpression.Helper.RequestParentElementEditorSave += RaiseRequestParentElementEditorSave;
            ctlScriptExpander.Helper.Dirty += RaiseDirty;
            ctlScriptExpander.Helper.RequestParentElementEditorSave += RaiseRequestParentElementEditorSave;
        }

        public void Uninitialise()
        {
            ctlExpression.Helper.DoUninitialise();
            ctlScriptExpander.DoUninitialise();
            ctlScriptExpander.Delete -= ctlScriptExpander_Delete;
            ctlExpression.Helper.Dirty -= RaiseDirty;
            ctlExpression.Helper.RequestParentElementEditorSave -= RaiseRequestParentElementEditorSave;
            ctlScriptExpander.Helper.Dirty -= RaiseDirty;
            ctlScriptExpander.Helper.RequestParentElementEditorSave -= RaiseRequestParentElementEditorSave;
        }

        void ctlScriptExpander_Delete()
        {
            RaiseDeleteEvent();
        }

        void RaiseDirty(object sender, DataModifiedEventArgs e)
        {
            Dirty();
        }

        void RaiseRequestParentElementEditorSave()
        {
            RequestParentElementEditorSave();
        }

        public void Populate(IEditorData data, IEditableScripts script)
        {
            if (data != null)
            {
                data.ReadOnly = m_readOnly;
            }
            ctlExpression.Populate(data);

            ctlScriptExpander.ReadOnly = m_readOnly;
            ctlScriptExpander.Populate(data, script);

            cmdDelete.IsEnabled = !m_readOnly;
        }

        public void RefreshExpression(IEditorData data)
        {
            ctlExpression.Populate(data);
        }

        public void Save()
        {
            ctlExpression.Save();
            ctlScriptExpander.Save();
        }

        public IfEditorChildMode Mode
        {
            get { return m_mode; }
            set
            {
                m_mode = value;

                switch (m_mode)
                {
                    case IfEditorChildMode.If:
                        lblIf.Content = "If:";
                        ctlScriptExpander.Header = "Then:";
                        break;
                    case IfEditorChildMode.ElseIf:
                        lblIf.Content = "Else if:";
                        ctlScriptExpander.Header = "Then:";
                        break;
                    case IfEditorChildMode.Else:
                        ctlScriptExpander.Header = "Else:";
                        break;
                }

                Visibility expressionVisibility = (m_mode == IfEditorChildMode.Else) ? Visibility.Collapsed : Visibility.Visible;
                lblIf.Visibility = expressionVisibility;
                ctlExpression.Visibility = expressionVisibility;

                cmdDelete.Visibility = (m_mode == IfEditorChildMode.ElseIf) ? Visibility.Visible : Visibility.Collapsed;
                ctlScriptExpander.ShowDeleteButton = (m_mode == IfEditorChildMode.Else);
            }
        }

        private void cmdDelete_Click(object sender, RoutedEventArgs e)
        {
            RaiseDeleteEvent();
        }

        private void RaiseDeleteEvent()
        {
            Delete(this);
        }

        public EditableIfScript.EditableElseIf ElseIfData { get; set; }

        public Control FocusableControl
        {
            get
            {
                return ctlExpression.FocusableControl;
            }
        }

        public bool ReadOnly
        {
            get { return m_readOnly; }
            set { m_readOnly = value; }
        }
    }
}
