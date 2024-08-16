using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest;
using TextAdventures.Quest.Scripts;

namespace TextAdventures.Quest
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
        private Dictionary<string, string> m_filters = new Dictionary<string, string>();

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
            if (attribute == "name" && m_element.Fields[FieldDefinitions.Anonymous])
            {
                return string.Empty;
            }
            return m_controller.WrapValue(m_element.Fields.Get(attribute), m_element, attribute);
        }

        public ValidationResult SetAttribute(string attribute, object value)
        {
            if (attribute == "name")
            {
                if (!(value is string))
                {
                    return new ValidationResult { Valid = false, Message = ValidationMessage.InvalidAttributeName };
                }

                ValidationResult result;

                result = m_controller.CanRename(m_element, (string)value);
                if (!result.Valid) return result;

                if (m_element.Fields[FieldDefinitions.Anonymous])
                {
                    m_element.Fields[FieldDefinitions.Anonymous] = false;
                }
            }

            if (!Utility.IsValidAttributeName(attribute))
            {
                return new ValidationResult { Valid = false, Message = ValidationMessage.InvalidAttributeName };
            }

            IDataWrapper wrapper = value as IDataWrapper;
            if (wrapper != null)
            {
                value = wrapper.GetUnderlyingValue();
            }

            string oldName = null;
            if (attribute == "name")
            {
                oldName = m_element.Name;
            }

            m_element.Fields.Set(attribute, value);

            if (attribute == "name")
            {
                m_controller.UpdateDictionariesReferencingRenamedObject(oldName, (string)value);
            }

            return new ValidationResult { Valid = true };
        }

        // When the "anonymous" field is updated, that affects how the "name" attribute is displayed.

        public IEnumerable<string> GetAffectedRelatedAttributes(string attribute)
        {
            if (attribute == "anonymous")
            {
                return new List<string> { "name" };
            }
            else
            {
                return null;
            }
        }

        public string GetSelectedFilter(string filterGroup)
        {
            string result;
            m_filters.TryGetValue(filterGroup, out result);
            return result;
        }

        public void SetSelectedFilter(string filterGroup, string filter)
        {
            m_filters[filterGroup] = filter;
            if (Changed != null) Changed(this, new EventArgs());
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

        public bool IsLibraryElement
        {
            get
            {
                return m_element.MetaFields[MetaFieldDefinitions.Library];
            }
        }

        public string Filename
        {
            get
            {
                return m_element.MetaFields[MetaFieldDefinitions.Filename];
            }
        }

        public void MakeElementLocal()
        {
            if (!IsLibraryElement)
            {
                throw new InvalidOperationException("Element is not defined in a library");
            }

            m_element.MetaFields[MetaFieldDefinitions.Library] = false;
            m_element.MetaFields[MetaFieldDefinitions.Filename] = null;
        }

        public bool ReadOnly
        {
            get { return IsLibraryElement; }
            set { }
        }

        public IEnumerable<string> GetVariablesInScope()
        {
            return null;
        }

        public bool IsDirectlySaveable { get { return true; } }
    }
}
