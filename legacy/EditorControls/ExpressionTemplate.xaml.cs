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
    /// <summary>
    /// Interaction logic for ExpressionTemplate.xaml
    /// </summary>
    public partial class ExpressionTemplate : UserControl
    {
        private List<IElementEditorControl> m_controls = new List<IElementEditorControl>();
        private IEditorData m_data;
        private string m_templatesFilter;

        public event Action<string> Dirty;
        public event Action RequestSave;

        public ExpressionTemplate()
        {
            InitializeComponent();
        }

        public EditorController Controller { get; set; }

        public void Initialise(IEditorDefinition definition, string expression, IEditorData parentData)
        {
            foreach (IElementEditorControl ctl in m_controls)
            {
                ctl.Helper.Dirty -= SubControl_Dirty;
            }

            m_controls.Clear();
            stackPanel.Children.Clear();

            if (m_data != null)
            {
                m_data.Changed -= m_data_Changed;
            }

            m_data = Controller.GetExpressionEditorData(expression, ExpressionTypeTemplateFilter, parentData);
            m_data.Changed += m_data_Changed;

            foreach (IEditorControl ctl in definition.Controls)
            {
                AddControl(ctl, expression);
            }
        }

        public void Uninitialise()
        {
            foreach (IElementEditorControl ctl in m_controls)
            {
                ctl.Populate(null);
                ctl.Helper.Dirty -= SubControl_Dirty;
                ctl.Helper.RequestParentElementEditorSave -= SubControl_RequestParentElementEditorSave;
                ctl.Helper.DoUninitialise();
            }
            if (m_data != null)
            {
                m_data.Changed -= m_data_Changed;
            }
            m_data = null;
        }

        void m_data_Changed(object sender, EventArgs e)
        {
            RequestSave();
        }

        private void AddControl(IEditorControl ctl, string expression)
        {
            Control newControl = ControlFactory.CreateEditorControl(Controller, ctl.ControlType);
            stackPanel.Children.Add(newControl);

            newControl.VerticalAlignment = VerticalAlignment.Top;
            if (newControl is LabelControl)
            {
                newControl.Padding = new Thickness(3, 3, 3, 0);
            }
            else
            {
                newControl.Padding = new Thickness(3, 0, 3, 0);
            }

            IElementEditorControl elementEditor = newControl as IElementEditorControl;
            if (elementEditor != null)
            {
                elementEditor.Helper.DoInitialise(Controller, ctl);
                elementEditor.Populate(m_data);
                elementEditor.Helper.Dirty += SubControl_Dirty;
                elementEditor.Helper.RequestParentElementEditorSave += SubControl_RequestParentElementEditorSave;
                m_controls.Add(elementEditor);
            }
        }

        void SubControl_RequestParentElementEditorSave()
        {
            RequestSave();
        }

        void SubControl_Dirty(object sender, DataModifiedEventArgs e)
        {
            Dirty(SaveExpression(e.Attribute, (string)e.NewValue));
        }

        public string SaveExpression()
        {
            // Copy control list as saving will cause it to be repopulated
            List<IElementEditorControl> controls = new List<IElementEditorControl>(m_controls);
            foreach (IElementEditorControl ctl in controls)
            {
                ctl.Save();
            }
            return SaveExpression(null, null);
        }

        private string SaveExpression(string changedAttribute, string changedValue)
        {
            return Controller.GetExpression(m_data, changedAttribute, changedValue);
        }

        public string ExpressionTypeTemplateFilter
        {
            get { return m_templatesFilter; }
            set { m_templatesFilter = value; }
        }
    }
}
