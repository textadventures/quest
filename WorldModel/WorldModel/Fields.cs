using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Scripts;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest
{
    public class AttributeChangedEventArgs : EventArgs
    {
        internal AttributeChangedEventArgs(string property, object value, object oldValue)
        {
            Property = property;
            Value = value;
            OldValue = oldValue;
        }

        internal AttributeChangedEventArgs(bool inheritedTypesSet)
        {
            InheritedTypesSet = inheritedTypesSet;
        }

        public string Property { get; private set; }
        public object Value { get; private set; }
        public object OldValue { get; private set; }
        public bool InheritedTypesSet { get; private set; }
    }

    public class NameChangedEventArgs : EventArgs
    {
        public string OldName { get; set; }
        public Element Element { get; set; }
    }

    public interface IMutableField
    {
        UndoLogger UndoLog { get; set; }

        Element Owner { get; set; }

        /// <summary>
        /// True if we're in an inherited type, so we must be unmodifable. Implementors must check this and throw an exception
        /// if an attempt is made to change a locked object
        /// </summary>
        bool Locked { get; set; }

        // QuestLists etc. require cloning as you want to be able to assign a.list = b.list and modify
        // the two lists separately. Scripts don't require cloning as they cannot be modified during the
        // game, we just want the Undo behaviour to work in the editor.

        bool RequiresCloning { get; }
        IMutableField Clone();
    }

    public interface IExtendableField
    {
        bool Extended { get; }
        IExtendableField Merge(IExtendableField parent);
    }

    public interface IField<T>
    {
        string Property { get; }
    }

    internal class FieldDef<T> : IField<T>
    {
        private string m_property;

        public FieldDef(string property)
        {
            m_property = property;
        }

        public string Property { get { return m_property; } }
    }

    public static class FieldDefinitions
    {
        public static IField<string> Alias = new FieldDef<string>("alias");
        public static IField<QuestList<string>> DisplayVerbs = new FieldDef<QuestList<string>>("displayverbs");
        public static IField<QuestList<string>> InventoryVerbs = new FieldDef<QuestList<string>>("inventoryverbs");
        public static IField<Element> To = new FieldDef<Element>("to");
        public static IField<bool> LookOnly = new FieldDef<bool>("lookonly");
        public static IField<QuestList<string>> ParamNames = new FieldDef<QuestList<string>>("paramnames");
        public static IField<IScript> Script = new FieldDef<IScript>("script");
        public static IField<string> ReturnType = new FieldDef<string>("returntype");
        public static IField<string> Pattern = new FieldDef<string>("pattern");
        public static IField<string> Unresolved = new FieldDef<string>("unresolved");
        public static IField<string> Property = new FieldDef<string>("property");
        public static IField<string> DefaultTemplate = new FieldDef<string>("defaulttemplate");
        public static IField<bool> IsVerb = new FieldDef<bool>("isverb");
        public static IField<string> DefaultText = new FieldDef<string>("defaulttext");
        public static IField<string> GameName = new FieldDef<string>("gamename");
        public static IField<string> Text = new FieldDef<string>("text");
        public static IField<IFunction<string>> Function = new FieldDef<IFunction<string>>("text");
        public static IField<string> Filename = new FieldDef<string>("filename");
        public static IField<QuestList<string>> Steps = new FieldDef<QuestList<string>>("steps");
        public static IField<string> Element = new FieldDef<string>("element");
        public static IField<string> Type = new FieldDef<string>("type");
        public static IField<string> Src = new FieldDef<string>("src");
        public static IField<bool> Anonymous = new FieldDef<bool>("anonymous");
        public static IField<string> TemplateName = new FieldDef<string>("templatename");
        public static IField<string> OriginalPattern = new FieldDef<string>("originalpattern");
        public static IField<int> TimeElapsed = new FieldDef<int>("timeelapsed");
        public static IField<int> Trigger = new FieldDef<int>("trigger");
        public static IField<int> Interval = new FieldDef<int>("interval");
        public static IField<bool> Enabled = new FieldDef<bool>("enabled");
        public static IField<string> Separator = new FieldDef<string>("separator");
        public static IField<string> GameID = new FieldDef<string>("gameid");
        public static IField<string> Category = new FieldDef<string>("category");
        public static IField<string> Description = new FieldDef<string>("description");
        public static IField<string> DefaultWebFont = new FieldDef<string>("defaultwebfont");
        public static IField<bool> IsBaseTemplate = new FieldDef<bool>("isbasetemplate");
        public static IField<string> DisplayVerb = new FieldDef<string>("displayverb");
        public static IField<string> Cover = new FieldDef<string>("cover");
        public static IField<string> PublishFileExtensions = new FieldDef<string>("publishfileextensions");
    }

    public static class MetaFieldDefinitions
    {
        public static IField<string> Filename = new FieldDef<string>("filename");
        public static IField<bool> Library = new FieldDef<bool>("library");
        public static IField<bool> EditorLibrary = new FieldDef<bool>("editorlibrary");
        public static IField<bool> DelegateImplementation = new FieldDef<bool>("delegateimplementation");
        public static IField<int> SortIndex = new FieldDef<int>("sortindex");
    }

    public class Fields
    {
        private static Dictionary<Type, DebugFormatDelegate> s_formatters = new Dictionary<Type, DebugFormatDelegate>
        {
            { typeof(List<string>), ListFormatter }
        };

        private delegate string DebugFormatDelegate(object input);
        private WorldModel m_worldModel;
        private Element m_element;
        private Dictionary<string, object> m_attributes = new Dictionary<string, object>();
        private Dictionary<string, IExtendableField> m_extendableFields = new Dictionary<string, IExtendableField>();
        private Stack<Element> m_types = new Stack<Element>();
        private LazyFields m_lazyFields;
        public event EventHandler<AttributeChangedEventArgs> AttributeChanged;
        public event EventHandler<AttributeChangedEventArgs> AttributeChangedSilent;
        internal event EventHandler<NameChangedEventArgs> NameChanged;
        private bool m_mutableFieldsLocked = false;
        private bool m_isMeta;

        public Fields(WorldModel worldModel, Element element, bool isMeta)
        {
            m_worldModel = worldModel;
            m_element = element;
            m_lazyFields = new LazyFields(worldModel, this);
            m_isMeta = isMeta;
        }

        internal Fields Clone(Element newElement)
        {
            Fields clone = new Fields(m_worldModel, newElement, m_isMeta);

            foreach (Element type in m_types.Reverse())
            {
                clone.m_types.Push(type);
            }

            foreach (var attribute in m_attributes)
            {
                if (attribute.Key != "name")
                {
                    clone.Set(attribute.Key, attribute.Value, false, true);
                }
            }

            return clone;
        }

        #region Indexed Properties
        public string this[IField<string> field]
        {
            get
            {
                return GetAsType<string>(field.Property);
            }
            set
            {
                Set(field.Property, value);
            }
        }

        public QuestList<string> this[IField<QuestList<string>> field]
        {
            get
            {
                return GetAsType<QuestList<string>>(field.Property);
            }
            set
            {
                Set(field.Property, value);
            }
        }

        public IScript this[IField<IScript> field]
        {
            get
            {
                return GetAsType<IScript>(field.Property);
            }
            set
            {
                Set(field.Property, value);
            }
        }

        public Element this[IField<Element> field]
        {
            get
            {
                return GetAsType<Element>(field.Property);
            }
            set
            {
                Set(field.Property, value);
            }
        }

        public bool this[IField<bool> field]
        {
            get
            {
                return GetAsType<bool>(field.Property);
            }
            set
            {
                Set(field.Property, value);
            }
        }

        public int this[IField<int> field]
        {
            get
            {
                return GetAsType<int>(field.Property);
            }
            set
            {
                Set(field.Property, value);
            }
        }

        public IFunction<string> this[IField<IFunction<string>> field]
        {
            get
            {
                return GetAsType<IFunction<string>>(field.Property);
            }
            set
            {
                Set(field.Property, value);
            }
        }

        public QuestList<object> this[IField<QuestList<object>> field]
        {
            get
            {
                return GetAsType<QuestList<object>>(field.Property);
            }
            set
            {
                Set(field.Property, value);
            }
        }
        #endregion

        internal bool MutableFieldsLocked
        {
            get { return m_mutableFieldsLocked; }
            set { m_mutableFieldsLocked = value; }
        }

        private DebugFormatDelegate GetFormatter(Type type)
        {
            if (type == null) return DefaultFormatter;
            if (s_formatters.ContainsKey(type)) return s_formatters[type];
            return DefaultFormatter;
        }

        private string DefaultFormatter(object input)
        {
            if (input == null) return "(null)";
            return input.ToString();
        }

        private static string ListFormatter(object input)
        {
            List<string> list = (List<string>)input;
            string output = string.Empty;
            if (list.Count == 0) return output;

            foreach (string item in list)
            {
                output += item + ", ";
            }
            return output.Substring(0, output.Length - 2);
        }

        private void UndoLog(string property, object oldValue, object newValue, bool added)
        {
            if (m_worldModel != null) m_worldModel.UndoLogger.AddUndoAction(new UndoFieldSet(m_worldModel, m_element.Name, property, oldValue, newValue, added, m_isMeta));
        }

        internal void UndoLog(TextAdventures.Quest.UndoLogger.IUndoAction action)
        {
            if (m_worldModel != null) m_worldModel.UndoLogger.AddUndoAction(action);
        }

        public void Set(string name, object value)
        {
            Set(name, value, true, true);
        }

        internal void SetFromUndo(string name, object value)
        {
            Set(name, value, false, false, allowUpdateSortOrder: false);
        }

        internal void RemoveFieldInternal(string name)
        {
            object oldValue = Get(name);
            m_attributes.Remove(name);

            if (AttributeChangedSilent != null && name != "name")
            {
                AttributeChangedSilent(this, new AttributeChangedEventArgs(name, Get(name), oldValue));
            }
        }

        public void RemoveField(string name)
        {
            UndoLog(new UndoFieldRemove(m_element.Name, name, m_attributes[name]));
            RemoveFieldInternal(name);
        }

        public void AddFieldExtension(string name, IExtendableField value)
        {
            if (value.Extended)
            {
                // extendable fields should only ever exist in types, and be set at the start of the game,
                // so we should never need to worry about adding them to the undo log etc.
                m_extendableFields[name] = value;
            }
            else
            {
                Set(name, value);
            }
        }

        private void Set(string name, object value, bool raiseEvent, bool cloneClonableValues, bool allowUpdateSortOrder = true)
        {
            bool changed;
            bool added = true;
            object oldValue = null;

            if (m_attributes.ContainsKey(name))
            {
                added = false;
                oldValue = Get(name);
            }
            else
            {
                if (!Utility.IsValidAttributeName(name))
                {
                    throw new Exception(string.Format("Invalid attribute name '{0}'", name));
                }
            }

            if (value == null)
            {
                changed = (oldValue != null);
            }
            else
            {
                changed = !value.Equals(oldValue);
            }

            if (changed && cloneClonableValues)
            {
                IMutableField mutableOldValue = oldValue as IMutableField;
                IMutableField mutableNewValue = value as IMutableField;
                if (mutableOldValue != null)
                {
                    if (m_worldModel.EditMode || mutableOldValue.RequiresCloning) oldValue = mutableOldValue.Clone();
                }
                if (mutableNewValue != null)
                {
                    if (m_worldModel.EditMode || mutableNewValue.RequiresCloning) value = mutableNewValue.Clone();
                    IMutableField mutableValue = (IMutableField)value;
                    mutableValue.Locked = m_mutableFieldsLocked;
                    mutableValue.UndoLog = m_worldModel.UndoLogger;
                    mutableValue.Owner = m_element;
                }
            }

            if (name == "name" && !(value is string))
            {
                throw new ArgumentException("Invalid data type for 'name'");
            }

            if (name == "parent" && value == m_element)
            {
                throw new ArgumentException(string.Format("Parent of element '{0}' cannot be set to itself", m_element.Name));
            }

            if (m_worldModel.Version >= WorldModelVersion.v530 && value == null)
            {
                m_attributes.Remove(name);
            }
            else
            {
                m_attributes[name] = value;
            }

            if (changed && allowUpdateSortOrder && name == "parent" && m_element.Initialised)
            {
                m_worldModel.UpdateElementSortOrder(m_element);
            }

            if (name == "name" && changed && value != null && !added && NameChanged != null)
            {
                if (!m_worldModel.EditMode)
                {
                    // Actually we could allow this I suppose but I don't think it's sensible.
                    throw new InvalidOperationException("Cannot change name of element when not in Edit mode");
                }
                NameChanged(this, new NameChangedEventArgs
                {
                    OldName = (string)oldValue,
                    Element = m_element
                });
            }

            if (raiseEvent)
            {
                if (changed) UndoLog(name, oldValue, value, added);
                if (changed && AttributeChanged != null) AttributeChanged(this, new AttributeChangedEventArgs(name, value, oldValue));
            }
            else
            {
                // when undoing, the editor still needs a notification of a changed field
                if (changed && AttributeChangedSilent != null) AttributeChangedSilent(this, new AttributeChangedEventArgs(name, value, oldValue));
            }
        }

        public Type GetCurrentType(string attribute)
        {
            object value = Get(attribute);
            if (value == null) return null;
            return value.GetType();
        }

        public object Get(string attribute)
        {
            return Get(attribute, false).Value;
        }

        private struct AttributeData
        {
            public object Value;
            public string Source;
            public bool IsInherited;
        }

        private AttributeData Get(string attribute, bool withSource)
        {
            AttributeData result = new AttributeData();

            if (m_attributes.TryGetValue(attribute, out result.Value))
            {
                if (withSource)
                {
                    result.Source = m_element.Name;
                    result.IsInherited = false;
                }
                return result;
            }

            foreach (Element type in m_types)
            {
                if (type.Fields.Exists(attribute, false))
                {
                    result.Value = type.Fields.Get(attribute);
                    if (withSource)
                    {
                        result.Source = type.Name;
                        result.IsInherited = true;
                    }
                    break;
                }
            }

            // if for example we have a "listextend" field in the type hierarchy, we need to merge
            // that field with the base field

            if (HasExtendableField(attribute))
            {
                return GetMergedResult(attribute, result);
            }

            return result;
        }

        private AttributeData GetMergedResult(string attribute, AttributeData baseField)
        {
            List<string> source = new List<string>();
            IExtendableField extendableBaseField = baseField.Value as IExtendableField;
            IExtendableField mergedResult = GetExtendableField(attribute, source);

            if (extendableBaseField == null) return new AttributeData
            {
                Value = mergedResult,
                Source = string.Join(",", source)
            };

            if (mergedResult == null) return baseField;

            return new AttributeData
            {
                Value = mergedResult.Merge(extendableBaseField),
                Source = baseField.Source + "," + string.Join(",", source),
                IsInherited = true
            };
        }

        private bool HasExtendableField(string attribute)
        {
            if (m_extendableFields.ContainsKey(attribute)) return true;

            foreach (Element type in m_types)
            {
                if (type.Fields.HasExtendableField(attribute)) return true;
            }
            return false;
        }

        private IExtendableField GetExtendableField(string attribute, List<string> source)
        {
            IExtendableField result = null;

            if (m_extendableFields.ContainsKey(attribute))
            {
                result = MergeExtendableFields(result, m_extendableFields[attribute]);
                source.Add(m_element.Name);
            }

            foreach (Element type in m_types)
            {
                if (type.Fields.HasExtendableField(attribute))
                {
                    result = MergeExtendableFields(result, type.Fields.GetExtendableField(attribute, source));
                    // Don't need to add to source here as the call to GetExtendableField will do that automatically
                }
            }

            return result;
        }

        private IExtendableField MergeExtendableFields(IExtendableField field, IExtendableField parent)
        {
            if (field == null) return parent;
            return field.Merge(parent);
        }

        internal DebugDataItem GetDebugDataItem(string attribute)
        {
            DebugDataItem result = new DebugDataItem(FormatDebugData(Get(attribute)));

            string source = null;
            bool isInherited = false;

            if (m_attributes.ContainsKey(attribute))
            {
                source = m_element.Name;
            }
            else
            {
                foreach (Element type in m_types)
                {
                    if (type.Fields.Exists(attribute, true))
                    {
                        source = type.Name;
                        isInherited = true;
                        break;
                    }
                }
            }

            result.IsInherited = isInherited;
            result.Source = source;
            return result;
        }

        internal bool Exists(string attribute, bool includeExtendableFields)
        {
            if (m_attributes.ContainsKey(attribute)) return true;

            foreach (Element type in m_types)
            {
                if (type.Fields.Exists(attribute, includeExtendableFields)) return true;
            }

            if (includeExtendableFields && HasExtendableField(attribute)) return true;

            return false;
        }

        public void AddType(Element addType)
        {
            if (m_element.ElemType == ElementType.ObjectType &&
                (addType == m_element || addType.Fields.InheritsTypeRecursive(m_element)))
            {
                throw new Exception("Circular type reference");
            }
            m_types.Push(addType);
        }

        public void AddTypeUndoable(Element addType)
        {
            Stack<Element> oldValue = CloneStack(m_types);
            AddType(addType);
            Stack<Element> newValue = CloneStack(m_types);
            m_worldModel.UndoLogger.AddUndoAction(new UndoAddRemoveType(m_element.Name, oldValue, newValue));
            if (AttributeChangedSilent != null) AttributeChangedSilent(this, new AttributeChangedEventArgs(true));
        }

        public void RemoveTypeUndoable(Element removeType)
        {
            Stack<Element> oldValue = CloneStack(m_types);
            m_types = CloneStackAndDelete(m_types, removeType);
            Stack<Element> newValue = CloneStack(m_types);
            m_worldModel.UndoLogger.AddUndoAction(new UndoAddRemoveType(m_element.Name, oldValue, newValue));
            if (AttributeChangedSilent != null) AttributeChangedSilent(this, new AttributeChangedEventArgs(true));
        }

        private string FormatDebugData(object value)
        {
            return GetFormatter(value == null ? null : value.GetType()).Invoke(value);
        }

        internal DebugData GetDebugData()
        {
            DebugData result = new DebugData();

            foreach (string attribute in GetAttributeNames(true))
            {
                var attributeData = Get(attribute, true);
                var debugDataItem = new DebugDataItem(FormatDebugData(attributeData.Value));
                debugDataItem.Source = attributeData.Source;
                debugDataItem.IsInherited = attributeData.IsInherited;

                result.Data.Add(attribute, debugDataItem);
            }

            return result;
        }

        internal DebugData GetInheritedTypesDebugData()
        {
            DebugData result = new DebugData();

            // First we want all the types we include directly
            foreach (Element type in m_types)
            {
                if (!result.Data.ContainsKey(type.Name))
                {
                    DebugDataItem newItem = new DebugDataItem(type.Name);
                    newItem.Source = m_element.Name;
                    newItem.IsDefaultType = WorldModel.DefaultTypeNames.ContainsValue(type.Name);
                    result.Data.Add(type.Name, newItem);
                }
                else
                {
                    // This element directly inherits a type which has also been included
                    // via inheriting another type

                    result.Data[type.Name].Source += "," + m_element.Name;
                }

                // Next we want all the types that are inherited from those, as attributes
                // from these will take priority over any other inherited types

                DebugData inheritedData = type.Fields.GetInheritedTypesDebugData();

                foreach (string typeName in inheritedData.Data.Keys)
                {
                    if (!result.Data.ContainsKey(typeName))
                    {
                        DebugDataItem item = inheritedData.Data[typeName];
                        item.IsInherited = true;
                        result.Data.Add(typeName, item);
                    }
                }
            }

            return result;
        }

        public T GetAsType<T>(string attribute)
        {
            object value = Get(attribute);
            if (value is T) return (T)value;
            return default(T);
        }

        public bool HasType<T>(string attribute)
        {
            object value = Get(attribute);
            return (value is T);
        }

        public string GetString(string attribute)
        {
            return GetAsType<string>(attribute);
        }

        public bool HasString(string attribute)
        {
            return HasType<string>(attribute);
        }

        public Element GetObject(string attribute)
        {
            return GetAsType<Element>(attribute);
        }

        public bool HasObject(string attribute)
        {
            return HasType<Element>(attribute);
        }

        public Dictionary<string, object> GetAllAttributes()
        {
            // return a clone of the attributes dictionary as we don't
            // want external code changing any.
            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (string key in m_attributes.Keys)
            {
                // ok so it's not strictly a clone for things like Lists
                result.Add(key, m_attributes[key]);
            }
            return result;
        }

        internal LazyFields LazyFields
        {
            get { return m_lazyFields; }
        }

        internal IEnumerable<string> FieldNames
        {
            get { return m_attributes.Keys; }
        }

        internal IEnumerable<string> FieldExtensionNames
        {
            get
            {
                List<string> result = new List<string>(m_extendableFields.Keys);

                foreach (Element type in m_types)
                {
                    result.AddRange(type.Fields.FieldExtensionNames);
                }

                return result;
            }
        }

        internal IEnumerable<Element> Types
        {
            get { return m_types; }
        }

        internal void DoUndoAddRemoveType(Stack<Element> newValue)
        {
            m_types = newValue;
            if (AttributeChangedSilent != null) AttributeChangedSilent(this, new AttributeChangedEventArgs(true));
        }

        private Stack<Element> CloneStack(Stack<Element> input)
        {
            return CloneStackAndDelete(input, null);
        }

        private Stack<Element> CloneStackAndDelete(Stack<Element> input, Element elementToDelete)
        {
            Stack<Element> result = new Stack<Element>();
            foreach (var item in input.Reverse())
            {
                if (item != elementToDelete)
                {
                    result.Push(item);
                }
            }
            return result;
        }

        public void Resolve(ScriptFactory factory)
        {
            LazyFields.Resolve(factory);
        }

        public bool InheritsType(Element type)
        {
            return m_types.Contains(type);
        }

        public bool InheritsTypeRecursive(Element type)
        {
            if (m_types.Contains(type))
            {
                return true;
            }

            foreach (Element inheritedType in m_types)
            {
                if (inheritedType.Fields.InheritsTypeRecursive(type))
                {
                    return true;
                }
            }

            return false;
        }

        internal void RemoveReferencesTo(Element e)
        {
            List<string> nullifyAttributes = new List<string>();

            foreach (var attribute in m_attributes)
            {
                Element elementValue = attribute.Value as Element;
                if (elementValue != null)
                {
                    if (elementValue == e)
                    {
                        nullifyAttributes.Add(attribute.Key);
                    }
                    continue;
                }

                QuestList<Element> listValue = attribute.Value as QuestList<Element>;
                if (listValue != null)
                {
                    while (listValue.Contains(e))
                    {
                        listValue.Remove(e);
                    }
                    continue;
                }

                QuestDictionary<Element> dictionaryValue = attribute.Value as QuestDictionary<Element>;
                if (dictionaryValue != null)
                {
                    List<string> keysToRemove = new List<string>();
                    foreach (var item in dictionaryValue)
                    {
                        if (item.Value == e)
                        {
                            keysToRemove.Add(item.Key);
                        }
                    }
                    foreach (string key in keysToRemove)
                    {
                        dictionaryValue.Remove(key);
                    }
                    continue;
                }
            }

            foreach (string attribute in nullifyAttributes)
            {
                Set(attribute, null);
            }
        }

        public IEnumerable<string> GetAttributeNames(bool includeInheritedAttributes)
        {
            List<string> result = new List<string>(m_attributes.Select(a => a.Key));
            if (includeInheritedAttributes)
            {
                foreach (Element type in m_types)
                {
                    result.AddRange(type.Fields.GetAttributeNames(true).Where(a => !result.Contains(a)));
                }
            }
            return result;
        }
    }

    public class LazyFields
    {
        private Fields m_fields;
        private List<string> m_types = new List<string>();
        private List<string> m_defaultTypes = new List<string>();
        private Dictionary<string, string> m_objectFields = new Dictionary<string, string>();
        private Dictionary<string, string> m_scripts = new Dictionary<string, string>();
        private Dictionary<string, IDictionary<string, string>> m_scriptDictionaries = new Dictionary<string, IDictionary<string, string>>();
        private Dictionary<string, IEnumerable<string>> m_objectLists = new Dictionary<string, IEnumerable<string>>();
        private Dictionary<string, IDictionary<string, string>> m_objectDictionaries = new Dictionary<string, IDictionary<string, string>>();
        private List<Action> m_actions = new List<Action>();
        private WorldModel m_worldModel;
        private bool m_resolved = false;

        internal LazyFields(WorldModel worldModel, Fields fields)
        {
            m_worldModel = worldModel;
            m_fields = fields;
        }

        public void Resolve(ScriptFactory scriptFactory)
        {
            CheckNotResolved();
            foreach (string typename in m_defaultTypes)
            {
                // It is legitimate for a default type not to exist
                if (m_worldModel.Elements.ContainsKey(ElementType.ObjectType, typename))
                {
                    m_fields.AddType(m_worldModel.GetObjectType(typename));
                }
            }
            m_defaultTypes = null;
            foreach (string typename in m_types)
            {
                try
                {
                    m_fields.AddType(m_worldModel.GetObjectType(typename));
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error adding type '{0}' to element '{1}': {2}", typename, m_fields.Get("name"), ex.Message), ex);
                }
            }
            m_types = null;
            foreach (string property in m_objectFields.Keys)
            {
                try
                {
                    m_fields.Set(property, m_worldModel.Elements.Get(m_objectFields[property]));
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error adding attribute '{0}' to element '{1}': {2}", property, m_fields.Get("name"), ex.Message), ex);
                }
            }
            m_objectFields = null;
            foreach (string property in m_scripts.Keys)
            {
                try
                {
                    m_fields.Set(property, scriptFactory.CreateScript(m_scripts[property]));
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error adding script attribute '{0}' to element '{1}': {2}", property, m_fields.Get("name"), ex.Message), ex);
                }
            }
            m_scripts = null;
            foreach (string property in m_scriptDictionaries.Keys)
            {
                try
                {
                    m_fields.Set(property, ConvertToScriptDictionary(m_scriptDictionaries[property], scriptFactory));
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error adding script dictionary '{0}' to element '{1}': {2}", property, m_fields.Get("name"), ex.Message), ex);
                }
            }
            m_scriptDictionaries = null;
            foreach (string property in m_objectLists.Keys)
            {
                try
                {
                    m_fields.Set(property, new QuestList<Element>(m_objectLists[property].Select(n => m_worldModel.Elements.Get(n))));
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error adding object list '{0}' to element '{1}': {2}", property, m_fields.Get("name"), ex.Message), ex);
                }

            }
            m_objectLists = null;
            foreach (string property in m_objectDictionaries.Keys)
            {
                try
                {
                    m_fields.Set(property, ConvertToObjectDictionary(m_objectDictionaries[property]));
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error adding object dictionary '{0}' to element '{1}': {2}", property, m_fields.Get("name"), ex.Message), ex);
                }
            }
            m_objectDictionaries = null;
            foreach (Action action in m_actions)
            {
                action();
            }
            m_actions = null;
            foreach (var field in m_fields.FieldNames)
            {
                var attribute = m_fields.Get(field);
                var objectList = attribute as QuestList<object>;
                if (objectList != null)
                {
                    ResolveObjectList(objectList, scriptFactory);
                }

                var objectDictionary = attribute as QuestDictionary<object>;
                if (objectDictionary != null)
                {
                    ResolveObjectDictionary(objectDictionary, scriptFactory);
                }
            }
            m_resolved = true;
        }

        private QuestDictionary<IScript> ConvertToScriptDictionary(IDictionary<string, string> dictionary, ScriptFactory scriptFactory)
        {
            QuestDictionary<IScript> newDictionary = new QuestDictionary<IScript>();
            foreach (var item in dictionary)
            {
                IScript newScript = scriptFactory.CreateScript(item.Value);
                newDictionary.Add(item.Key, newScript);
            }
            return newDictionary;
        }

        private QuestDictionary<Element> ConvertToObjectDictionary(IDictionary<string, string> dictionary)
        {
            QuestDictionary<Element> newDictionary = new QuestDictionary<Element>();
            foreach (var item in dictionary)
            {
                Element element = m_worldModel.Elements.Get(item.Value);
                newDictionary.Add(item.Key, element);
            }
            return newDictionary;
        }

        public void AddType(string typename)
        {
            CheckNotResolved();
            m_types.Add(typename);
        }

        public void AddDefaultType(string typename)
        {
            CheckNotResolved();
            m_defaultTypes.Add(typename);
        }

        public void AddObjectField(string property, string value)
        {
            CheckNotResolved();
            m_objectFields.Add(property, value);
        }

        public void AddScript(string property, string value)
        {
            CheckNotResolved();
            m_scripts.Add(property, value);
        }

        public void AddScriptDictionary(string property, IDictionary<string, string> value)
        {
            CheckNotResolved();
            m_scriptDictionaries.Add(property, value);
        }

        public void AddObjectList(string property, IEnumerable<string> value)
        {
            CheckNotResolved();
            m_objectLists.Add(property, value);
        }

        public void AddObjectDictionary(string property, IDictionary<string, string> value)
        {
            CheckNotResolved();
            m_objectDictionaries.Add(property, value);
        }

        public void AddAction(Action action)
        {
            CheckNotResolved();
            m_actions.Add(action);
        }

        private void ResolveObjectList(QuestList<object> list, ScriptFactory scriptFactory)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var value = list[i];

                object replacement;
                var replace = ReplaceValue(value, scriptFactory, out replacement);

                if (replace)
                {
                    list.RemoveAt(i);
                    list.Insert(i, replacement);
                }
            }
        }

        private void ResolveObjectDictionary(QuestDictionary<object> dictionary, ScriptFactory scriptFactory)
        {
            var copy = new Dictionary<string, object>(dictionary);

            foreach (var item in copy)
            {
                object replacement;
                var replace = ReplaceValue(item.Value, scriptFactory, out replacement);

                if (replace)
                {
                    dictionary[item.Key] = replacement;
                }
            }
        }

        private bool ReplaceValue(object value, ScriptFactory scriptFactory, out object replacement)
        {
            replacement = null;
            
            var genericList = value as QuestList<object>;
            if (genericList != null)
            {
                ResolveObjectList(genericList, scriptFactory);
                return false;
            }

            var genericDictionary = value as QuestDictionary<object>;
            if (genericDictionary != null)
            {
                ResolveObjectDictionary(genericDictionary, scriptFactory);
                return false;
            }

            var objRef = value as Types.LazyObjectReference;
            if (objRef != null)
            {
                replacement = m_worldModel.Elements.Get(objRef.ObjectName);
                return true;
            }

            var objList = value as Types.LazyObjectList;
            if (objList != null)
            {
                replacement = new QuestList<Element>(objList.Objects.Select(o => m_worldModel.Elements.Get(o)));
                return true;
            }

            var objDictionary = value as Types.LazyObjectDictionary;
            if (objDictionary != null)
            {
                var newDictionary = new QuestDictionary<Element>();
                foreach (var kvp in objDictionary.Dictionary)
                {
                    newDictionary.Add(kvp.Key, m_worldModel.Elements.Get(kvp.Value));
                }
                replacement = newDictionary;
                return true;
            }

            var script = value as Types.LazyScript;
            if (script != null)
            {
                replacement = scriptFactory.CreateScript(script.Script);
                return true;
            }

            var scriptDictionary = value as Types.LazyScriptDictionary;
            if (scriptDictionary != null)
            {
                replacement = ConvertToScriptDictionary(scriptDictionary.Dictionary, scriptFactory);
                return true;
            }

            return false;
        }

        private void CheckNotResolved()
        {
            if (m_resolved) throw new Exception("LazyFields instance already resolved.");
        }
    }

    public class UndoFieldSet : TextAdventures.Quest.UndoLogger.IUndoAction
    {
        private string m_appliesTo;
        private string m_property;
        private object m_oldValue;
        private object m_newValue;
        private bool m_added;
        private string m_oldValueElementName;
        private string m_newValueElementName;
        private WorldModel m_worldModel;
        private bool m_useMetaFields;

        public UndoFieldSet(WorldModel worldModel, string appliesTo, string property, object oldValue, object newValue, bool added, bool useMetaFields)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(appliesTo));
            m_worldModel = worldModel;
            m_appliesTo = appliesTo;
            m_property = property;
            m_useMetaFields = useMetaFields;

            if (oldValue is Element)
            {
                m_oldValueElementName = ((Element)oldValue).Name;
            }
            else
            {
                m_oldValue = oldValue;
            }

            if (newValue is Element)
            {
                m_newValueElementName = ((Element)newValue).Name;
            }
            else
            {
                m_newValue = newValue;
            }

            m_added = added;

            //System.Diagnostics.Debug.Print("UndoFieldSet: {0}.{1} from '{2}' to '{3}'", appliesTo, property, oldValue, newValue);
        }

        public string AppliesTo
        {
            get { return m_appliesTo; }
        }

        public string Property
        {
            get { return m_property; }
        }

        public object OldValue
        {
            get
            {
                if (m_oldValueElementName != null)
                {
                    if (m_worldModel.Elements.ContainsKey(m_oldValueElementName))
                    {
                        return m_worldModel.Elements.Get(m_oldValueElementName);
                    }
                    // element may have been deleted
                    return null;
                }
                return m_oldValue;
            }
        }

        public object NewValue
        {
            get
            {
                if (m_newValueElementName != null) return m_worldModel.Elements.Get(m_newValueElementName);
                return m_newValue;
            }
        }

        public void DoUndo(WorldModel worldModel)
        {
            Fields fields = GetFields(m_appliesTo);
            if (m_added)
            {
                fields.RemoveFieldInternal(Property);
            }
            else
            {
                fields.SetFromUndo(Property, OldValue);
            }
        }

        public void DoRedo(WorldModel worldModel)
        {
            if (Property != "name" || OldValue == null)
            {
                GetFields(m_appliesTo).SetFromUndo(Property, NewValue);
            }
            else
            {
                // When redoing a name change, m_appliesTo will be incorrect as it will be the new object name.
                // So in this specific case we get the appliesTo name from the old property value.
                // (If OldValue is null then this is just setting the name property for a brand new object,
                // so the above comment doesn't apply, and this case is handled in the above "if")
                GetFields((string)OldValue).SetFromUndo(Property, NewValue);
            }
        }

        private Fields GetFields(string elementName)
        {
            Element element = m_worldModel.Elements.Get(elementName);
            return m_useMetaFields ? element.MetaFields : element.Fields;
        }
    }

    public class UndoFieldRemove : TextAdventures.Quest.UndoLogger.IUndoAction
    {
        private string m_appliesTo;
        private string m_property;
        private object m_oldValue;

        public UndoFieldRemove(string appliesTo, string property, object oldValue)
        {
            m_appliesTo = appliesTo;
            m_property = property;
            m_oldValue = oldValue;
        }

        public void DoUndo(WorldModel worldModel)
        {
            worldModel.Object(m_appliesTo).Fields.SetFromUndo(m_property, m_oldValue);
        }

        public void DoRedo(WorldModel worldModel)
        {
            worldModel.Object(m_appliesTo).Fields.RemoveFieldInternal(m_property);
        }
    }

    public class UndoAddRemoveType : TextAdventures.Quest.UndoLogger.IUndoAction
    {
        private string m_appliesTo;
        private Stack<Element> m_oldValue;
        private Stack<Element> m_newValue;

        public UndoAddRemoveType(string appliesTo, Stack<Element> oldValue, Stack<Element> newValue)
        {
            m_appliesTo = appliesTo;
            m_oldValue = oldValue;
            m_newValue = newValue;
        }

        public void DoUndo(WorldModel worldModel)
        {
            worldModel.Object(m_appliesTo).Fields.DoUndoAddRemoveType(m_oldValue);
        }

        public void DoRedo(WorldModel worldModel)
        {
            worldModel.Object(m_appliesTo).Fields.DoUndoAddRemoveType(m_newValue);
        }
    }
}