using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    internal class EditorDefinition : IEditorDefinition
    {
        private Dictionary<string, IEditorTab> m_tabs = null;
        private Dictionary<string, IEditorControl> m_controls = null;
        private string m_appliesTo = null;

        public EditorDefinition(WorldModel worldModel, Element source)
        {
            m_tabs = new Dictionary<string, IEditorTab>();
            m_controls = new Dictionary<string, IEditorControl>();
            m_appliesTo = source.Fields.GetString("appliesto");

            foreach (Element e in worldModel.Elements.GetElements(ElementType.EditorTab))
            {
                if (e.Parent == source)
                {
                    m_tabs.Add(e.Name, new EditorTab(worldModel, e));
                }
            }

            foreach (Element e in worldModel.Elements.GetElements(ElementType.EditorControl))
            {
                if (e.Parent == source)
                {
                    m_controls.Add(e.Name, new EditorControl(worldModel, e));
                }
            }
        }

        public string AppliesTo
        {
            get { return m_appliesTo; }
        }

        public IDictionary<string, IEditorTab> Tabs
        {
            get { return m_tabs; }
        }

        public IEnumerable<IEditorControl> Controls
        {
            get { return m_controls.Values; }
        }
    }
}
