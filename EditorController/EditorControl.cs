using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    internal class EditorControl : IEditorControl
    {
        private WorldModel m_worldModel;
        private string m_controlType;
        private string m_caption;
        private int? m_height = null;
        private int? m_width = null;
        private string m_attribute;
        private bool m_expand = false;
        private Element m_source;
        private EditorVisibilityHelper m_visibilityHelper;
        private EditorDefinition m_parent;
        private string m_id;

        public EditorControl(EditorDefinition parent, WorldModel worldModel, Element source)
        {
            m_parent = parent;
            m_worldModel = worldModel;
            m_source = source;
            m_controlType = source.Fields.GetString("controltype");
            m_caption = source.Fields.GetString("caption");
            m_attribute = source.Fields.GetString("attribute");
            if (source.Fields.HasType<int>("height")) m_height = source.Fields.GetAsType<int>("height");
            if (source.Fields.HasType<int>("width")) m_width = source.Fields.GetAsType<int>("width");
            if (source.Fields.HasType<bool>("expand")) m_expand = source.Fields.GetAsType<bool>("expand");            
            m_visibilityHelper = new EditorVisibilityHelper(parent, worldModel, source);
            IsControlVisibleInSimpleMode = !source.Fields.GetAsType<bool>("advanced");
            m_id = source.Name;

            if (source.Fields.HasString("filtergroup"))
            {
                parent.RegisterFilter(source.Fields.GetString("filtergroup"), source.Fields.GetString("filter"), m_attribute);
            }
        }

        public string ControlType
        {
            get { return m_controlType; }
        }

        public string Caption
        {
            get { return m_caption; }
        }

        public int? Height
        {
            get { return m_height; }
        }

        public int? Width
        {
            get { return m_width; }
        }

        public string Attribute
        {
            get { return m_attribute; }
        }

        public bool Expand
        {
            get { return m_expand; }
        }

        public string GetString(string tag)
        {
            return m_source.Fields.GetString(tag);
        }

        public IEnumerable<string> GetListString(string tag)
        {
            return m_source.Fields.GetAsType<QuestList<string>>(tag);
        }

        public IDictionary<string, string> GetDictionary(string tag)
        {
            return m_source.Fields.GetAsType<QuestDictionary<string>>(tag);
        }

        public bool GetBool(string tag)
        {
            return m_source.Fields.GetAsType<bool>(tag);
        }

        public int? GetInt(string tag)
        {
            if (!m_source.Fields.HasType<int>(tag)) return null;
            return m_source.Fields.GetAsType<int>(tag);
        }

        public double? GetDouble(string tag)
        {
            if (!m_source.Fields.HasType<double>(tag)) return null;
            return m_source.Fields.GetAsType<double>(tag);
        }

        public bool IsControlVisible(IEditorData data)
        {
            return m_visibilityHelper.IsVisible(data);
        }

        public IEditorDefinition Parent
        {
            get { return m_parent; }
        }

        public bool IsControlVisibleInSimpleMode
        {
            get;
            private set;
        }

        public string Id
        {
            get { return m_id; }
        }
    }
}
