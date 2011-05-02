using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    internal class EditorControl : IEditorControl
    {
        private WorldModel m_worldModel;
        private string m_controlType;
        private string m_caption;
        private int? m_height = null;
        private int? m_width = null;
        private int? m_paddingTop = null;
        private string m_attribute;
        private bool m_expand = false;
        private Element m_source;
        private bool m_alwaysVisible = true;
        private string m_relatedAttribute = null;
        private string m_visibleIfRelatedAttributeIsType = null;
        private string m_visibleIfElementInheritsType = null;
        private Element m_visibleIfElementInheritsTypeElement = null;

        public EditorControl(WorldModel worldModel, Element source)
        {
            m_worldModel = worldModel;
            m_source = source;
            m_controlType = source.Fields.GetString("controltype");
            m_caption = source.Fields.GetString("caption");
            m_attribute = source.Fields.GetString("attribute");
            if (source.Fields.HasType<int>("height")) m_height = source.Fields.GetAsType<int>("height");
            if (source.Fields.HasType<int>("width")) m_width = source.Fields.GetAsType<int>("width");
            if (source.Fields.HasType<int>("paddingtop")) m_paddingTop = source.Fields.GetAsType<int>("paddingtop");
            if (source.Fields.HasType<bool>("expand")) m_expand = source.Fields.GetAsType<bool>("expand");
            m_relatedAttribute = source.Fields.GetString("relatedattribute");
            if (m_relatedAttribute != null) m_alwaysVisible = false;
            m_visibleIfRelatedAttributeIsType = source.Fields.GetString("relatedattributedisplaytype");
            m_visibleIfElementInheritsType = source.Fields.GetString("mustinherit");
            if (m_visibleIfElementInheritsType != null) m_alwaysVisible = false;
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

        public int? PaddingTop
        {
            get { return m_paddingTop; }
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

        public int GetInt(string tag)
        {
            return m_source.Fields.GetAsType<int>(tag);
        }

        public bool IsControlVisible(IEditorData data)
        {
            if (m_alwaysVisible) return true;

            if (m_relatedAttribute != null)
            {
                object relatedAttributeValue = data.GetAttribute(m_relatedAttribute);
                if (relatedAttributeValue is IDataWrapper) relatedAttributeValue = ((IDataWrapper)relatedAttributeValue).GetUnderlyingValue();

                string relatedAttributeType = relatedAttributeValue == null ? "null" : WorldModel.ConvertTypeToTypeName(relatedAttributeValue.GetType());
                return relatedAttributeType == m_visibleIfRelatedAttributeIsType;
            }

            if (m_visibleIfElementInheritsType != null)
            {
                if (m_visibleIfElementInheritsTypeElement == null)
                {
                    m_visibleIfElementInheritsTypeElement = m_worldModel.Elements.Get(ElementType.ObjectType, m_visibleIfElementInheritsType);
                }
                return m_worldModel.Elements.Get(data.Name).Fields.InheritsType(m_visibleIfElementInheritsTypeElement);
            }

            return false;
        }
    }
}
