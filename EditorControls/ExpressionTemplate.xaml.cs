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
    /// <summary>
    /// Interaction logic for ExpressionTemplate.xaml
    /// </summary>
    public partial class ExpressionTemplate : UserControl
    {
        private List<IElementEditorControl> m_controls = new List<IElementEditorControl>();
        private IEditorData m_data;

        public event Action<string> Dirty;

        public ExpressionTemplate()
        {
            InitializeComponent();
        }

        public EditorController Controller { get; set; }

        public void Initialise(IEditorDefinition definition, string expression)
        {
            foreach (IElementEditorControl ctl in m_controls)
            {
                ctl.Helper.Dirty -= SubControl_Dirty;
            }

            m_controls.Clear();
            stackPanel.Children.Clear();

            m_data = Controller.GetExpressionEditorData(expression);

            foreach (IEditorControl ctl in definition.Controls)
            {
                AddControl(ctl, expression);
            }
        }

        private void AddControl(IEditorControl ctl, string expression)
        {
            Control newControl = ControlFactory.CreateEditorControl(Controller, ctl.ControlType);
            stackPanel.Children.Add(newControl);

            IElementEditorControl elementEditor = newControl as IElementEditorControl;
            if (elementEditor != null)
            {
                elementEditor.Helper.DoInitialise(Controller, ctl);
                elementEditor.Populate(m_data);
                elementEditor.Helper.Dirty += SubControl_Dirty;
                m_controls.Add(elementEditor);
            }
        }

        void SubControl_Dirty(object sender, DataModifiedEventArgs e)
        {
            Dirty(SaveExpression(e.Attribute, (string)e.NewValue));
        }

        public string SaveExpression()
        {
            return SaveExpression(null, null);
        }

        private string SaveExpression(string changedAttribute, string changedValue)
        {
            return Controller.GetExpression(m_data, changedAttribute, changedValue);
        }
    }
}
