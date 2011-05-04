using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest;
using AxeSoftware.Quest.Scripts;

namespace AxeSoftware.Quest
{
    internal class EditorAttributeData : IEditorAttributeData
    {
        public EditorAttributeData(string attributeName, bool isInherited, string source, bool isDefaultType)
        {
            AttributeName = attributeName;
            IsInherited = isInherited;
            Source = source;
            IsDefaultType = isDefaultType;
        }

        public string AttributeName
        {
            get;
            private set;
        }

        public bool IsInherited
        {
            get;
            private set;
        }

        public string Source
        {
            get;
            set;
        }

        public bool IsDefaultType
        {
            get;
            set;
        }
    }

    internal class EditorData : IEditorDataExtendedAttributeInfo
    {
        private Element m_element;
        private EditorController m_controller;
        public event EventHandler Changed;

        public EditorData(Element element, EditorController controller)
        {
            m_element = element;
            m_controller = controller;

            element.Fields.AttributeChanged += Fields_AttributeChanged;
            element.Fields.AttributeChangedSilent += Fields_AttributeChanged;
        }

        void Fields_AttributeChanged(object sender, AttributeChangedEventArgs e)
        {
            if (Changed != null) Changed(this, new EventArgs());
        }

        public string Name
        {
            get { return m_element.Name; }
        }

        public object GetAttribute(string attribute)
        {
            if (attribute == "name" && m_element.MetaFields[MetaFieldDefinitions.Anonymous])
            {
                return string.Empty;
            }
            return m_controller.WrapValue(m_element.Fields.Get(attribute), m_element, attribute);
        }

        public void SetAttribute(string attribute, object value)
        {
            if (attribute == "name" && m_element.MetaFields[MetaFieldDefinitions.Anonymous])
            {
                m_element.MetaFields[MetaFieldDefinitions.Anonymous] = false;
            }
            IDataWrapper wrapper = value as IDataWrapper;
            if (wrapper != null)
            {
                value = wrapper.GetUnderlyingValue();
            }
            m_element.Fields.Set(attribute, value);
        }

        public IEnumerable<IEditorAttributeData> GetAttributeData()
        {
            DebugData data = m_controller.WorldModel.GetDebugData(m_element.Name);
            return ConvertDebugDataToEditorAttributeData(data);
        }

        public IEditorAttributeData GetAttributeData(string attribute)
        {
            DebugDataItem data = m_controller.WorldModel.GetDebugDataItem(m_element.Name, attribute);
            return new EditorAttributeData(attribute, data.IsInherited, data.Source, data.IsDefaultType);
        }

        public void RemoveAttribute(string attribute)
        {
            m_element.Fields.RemoveField(attribute);
        }

        public IEnumerable<IEditorAttributeData> GetInheritedTypes()
        {
            DebugData data = m_controller.WorldModel.GetInheritedTypesDebugData(m_element.Name);
            return ConvertDebugDataToEditorAttributeData(data);
        }

        private IEnumerable<IEditorAttributeData> ConvertDebugDataToEditorAttributeData(DebugData data)
        {
            List<EditorAttributeData> result = new List<EditorAttributeData>();
            foreach (var item in data.Data)
            {
                result.Add(new EditorAttributeData(item.Key, item.Value.IsInherited, item.Value.Source, item.Value.IsDefaultType));
            }
            return result;
        }
    }
}
