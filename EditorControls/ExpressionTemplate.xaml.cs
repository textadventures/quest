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
        public ExpressionTemplate()
        {
            InitializeComponent();
        }

        public EditorController Controller { get; set; }

        public void Initialise(IEditorDefinition definition, string expression)
        {
            stackPanel.Children.Clear();

            // TO DO: Unhook event handlers

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
                elementEditor.Populate(Controller.GetExpressionEditorData(expression));
            }

            // TO DO: Hook up event handlers Dirty etc
        }
    }
}
