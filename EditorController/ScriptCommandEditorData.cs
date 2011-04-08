using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Scripts;

namespace AxeSoftware.Quest
{
    internal class ScriptCommandEditorData : IEditorData
    {
        private IEditableScript m_script;
        private EditorController m_controller;

        public ScriptCommandEditorData(EditorController controller, IEditableScript script)
        {
            m_controller = controller;
            m_script = script;
        }

        #region IEditorData Members

        public string Name
        {
            get { return null; }
        }

        public object GetAttribute(string attribute)
        {
            return m_controller.WrapValue(m_script.GetParameter(int.Parse(attribute)));
        }

        public void SetAttribute(string attribute, object value)
        {
            m_script.SetParameter(int.Parse(attribute), (string)value);
        }

        #endregion
    }
}
