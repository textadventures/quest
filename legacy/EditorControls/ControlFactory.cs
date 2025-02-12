using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace TextAdventures.Quest.EditorControls
{
    internal static class ControlFactory
    {
        public static Control CreateEditorControl(EditorController controller, string controlType)
        {
            Type newControlType = controller.GetControlType(controlType);
            if (newControlType == null)
            {
                Label tempLabel = new Label();
                tempLabel.Content = controlType + " not implemented";
                return tempLabel;
            }

            IElementEditorControl newControl = (IElementEditorControl)Activator.CreateInstance(newControlType);
            return (Control)newControl;
        }
    }
}
