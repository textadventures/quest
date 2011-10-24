using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    internal class EditorTab : IEditorTab
    {
        private Dictionary<string, IEditorControl> m_controls = null;
        private string m_caption;
        private EditorVisibilityHelper m_visibilityHelper;

        public EditorTab(EditorDefinition parent, WorldModel worldModel, Element source)
        {
            m_controls = new Dictionary<string, IEditorControl>();
            m_caption = source.Fields.GetString("caption");
            IsTabVisibleInSimpleMode = !source.Fields.GetAsType<bool>("advanced");

            foreach (Element e in worldModel.Elements.GetElements(ElementType.EditorControl))
            {
                if (e.Parent == source)
                {
                    m_controls.Add(e.Name, new EditorControl(parent, worldModel, e));
                }
            }
            m_visibilityHelper = new EditorVisibilityHelper(parent, worldModel, source);
        }

        public string Caption
        {
            get
            {
                return m_caption;
            }
        }

        public IEnumerable<IEditorControl> Controls
        {
            get { return m_controls.Values; }
        }

        public bool IsTabVisible(IEditorData data)
        {
            return m_visibilityHelper.IsVisible(data);
        }

        public bool IsTabVisibleInSimpleMode
        {
            get;
            private set;
        }
    }
}
