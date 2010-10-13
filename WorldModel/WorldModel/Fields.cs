using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Scripts;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest
{
    public class AttributeChangedEventArgs : EventArgs
    {
        internal AttributeChangedEventArgs(string property, object value)
        {
            Property = property;
            Value = value;
        }

        public string Property { get; private set; }
        public object Value { get; private set; }
    }

    public interface IMutableField
    {
        /// <summary>
        /// This is used PURELY so the field can get hold of the UndoLogger - why not just pass it in directly then!
        /// </summary>
        Fields Parent { get; set; }

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

    internal interface IField<T>
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

    internal static class FieldDefinitions
    {
        public static IField<string> Alias = new FieldDef<string>("alias");
        public static IField<QuestList<string>> DisplayVerbs = new FieldDef<QuestList<string>>("displayverbs");
        public static IField<QuestList<string>> InventoryVerbs = new FieldDef<QuestList<string>>("displayverbs");
        public static IField<Element> To = new FieldDef<Element>("to");
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
        public static IField<string> Interface = new FieldDef<string>("interface");
        public static IField<string> Filename = new FieldDef<string>("filename");
        public static IField<QuestList<string>> Steps = new FieldDef<QuestList<string>>("steps");
        public static IField<string> Element = new FieldDef<string>("element");
        public static IField<string> Type = new FieldDef<string>("type");
    }

    internal static class MetaFieldDefinitions
    {
        public static IField<bool> Anonymous = new FieldDef<bool>("anonymous");
        public static IField<string> Filename = new FieldDef<string>("filename");
        public static IField<bool> Library = new FieldDef<bool>("library");
        public static IField<bool> DelegateImplementation = new FieldDef<bool>("delegateimplementation");
    }

    public class Fields
    {
        private delegate string DebugFormatDelegate(object input);
        private WorldModel m_worldModel;
        private Element m_element;
        private Dictionary<string, object> m_attributes = new Dictionary<string, object>();
        private Dictionary<Type, DebugFormatDelegate> m_formatters = new Dictionary<Type, DebugFormatDelegate>();
        private Stack<Element> m_types = new Stack<Element>();
        private LazyFields m_lazyFields;
        public event EventHandler<AttributeChangedEventArgs> AttributeChanged;
        public event EventHandler<AttributeChangedEventArgs> AttributeChangedSilent;
        private bool m_mutableFieldsLocked = false;

        public Fields(WorldModel worldModel, Element element)
        {
            m_worldModel = worldModel;
            m_element = element;
            m_formatters.Add(typeof(List<string>), ListFormatter);
            m_lazyFields = new LazyFields(worldModel, this);
        }

        #region Indexed Properties
        internal string this[IField<string> field]
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

        internal QuestList<string> this[IField<QuestList<string>> field]
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

        internal IScript this[IField<IScript> field]
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

        internal Element this[IField<Element> field]
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

        internal bool this[IField<bool> field]
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

        internal IFunction<string> this[IField<IFunction<string>> field]
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
        #endregion

        internal bool MutableFieldsLocked
        {
            get { return m_mutableFieldsLocked; }
            set { m_mutableFieldsLocked = value; }
        }

        private DebugFormatDelegate GetFormatter(Type type)
        {
            if (type == null) return DefaultFormatter;
            if (m_formatters.ContainsKey(type)) return m_formatters[type];
            return DefaultFormatter;
        }

        private string DefaultFormatter(object input)
        {
            if (input == null) return "(null)";
            return input.ToString();
        }

        private string ListFormatter(object input)
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

        private void UndoLog(string property, object oldValue, object newValue)
        {
            if (m_worldModel != null) m_worldModel.UndoLogger.AddUndoAction(new UndoFieldSet(m_element.Name, property, oldValue, newValue));
        }

        internal void UndoLog(AxeSoftware.Quest.UndoLogger.IUndoAction action)
        {
            if (m_worldModel != null) m_worldModel.UndoLogger.AddUndoAction(action);
        }

        public void Set(string name, object value)
        {
            Set(name, value, true);
        }

        internal void SetSilent(string name, object value)
        {
            Set(name, value, false);
        }

        private void Set(string name, object value, bool raiseEvent)
        {
            bool changed = false;
            object oldValue = null;
            if (m_attributes.ContainsKey(name)) oldValue = Get(name);

            if (value == null)
            {
                changed = (oldValue != null);
            }
            else
            {
                changed = !value.Equals(oldValue);
            }

            if (changed)
            {
                IMutableField mutableOldValue = oldValue as IMutableField;
                IMutableField mutableNewValue = value as IMutableField;
                if (mutableOldValue != null)
                {
                    mutableOldValue.Parent = null;
                    if (mutableOldValue.RequiresCloning) oldValue = mutableOldValue.Clone();
                }
                if (mutableNewValue != null)
                {
                    if (mutableNewValue.RequiresCloning) value = mutableNewValue.Clone();
                    ((IMutableField)value).Parent = this;
                    ((IMutableField)value).Locked = m_mutableFieldsLocked;
                }
            }

            m_attributes[name] = value;

            if (raiseEvent)
            {
                if (changed) UndoLog(name, oldValue, value);
                if (changed && AttributeChanged != null) AttributeChanged(this, new AttributeChangedEventArgs(name, value));
            }
            else
            {
                // when undoing, the editor still needs a notification of a changed field
                if (changed && AttributeChangedSilent != null) AttributeChangedSilent(this, new AttributeChangedEventArgs(name, value));
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
            if (m_attributes.ContainsKey(attribute))
            {
                return m_attributes[attribute];
            }
            else
            {
                foreach (Element type in m_types)
                {
                    if (type.Fields.Exists(attribute)) return type.Fields.Get(attribute);
                }
            }

            return null;
        }

        internal bool Exists(string attribute)
        {
            if (m_attributes.ContainsKey(attribute)) return true;

            foreach (Element type in m_types)
            {
                if (type.Fields.Exists(attribute)) return true;
            }
            return false;
        }

        internal void AddType(Element addType)
        {
            m_types.Push(addType);
        }

        internal DebugData GetDebugData()
        {
            DebugData result = new DebugData();

            foreach (string attribute in m_attributes.Keys)
            {
                result.Data.Add(attribute, new DebugDataItem(GetFormatter(m_attributes[attribute] == null ? null : m_attributes[attribute].GetType()).Invoke(m_attributes[attribute])));
            }

            foreach (Element type in m_types)
            {
                DebugData inheritedData = type.Fields.GetDebugData();

                foreach (string attribute in inheritedData.Data.Keys)
                {
                    if (!result.Data.ContainsKey(attribute))
                    {
                        DebugDataItem item = inheritedData.Data[attribute];
                        item.IsInherited = true;
                        result.Data.Add(attribute, item);
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

        internal IEnumerable<Element> Types
        {
            get { return m_types; }
        }
    }

    public class LazyFields
    {
        private Fields m_fields;
        private List<string> m_types = new List<string>();
        private List<string> m_defaultTypes = new List<string>();
        private Dictionary<string, string> m_objectFields = new Dictionary<string, string>();
        private Dictionary<string, string> m_scripts = new Dictionary<string, string>();
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
            foreach (string typename in m_types)
            {
                m_fields.AddType(m_worldModel.GetObjectType(typename));
            }
            foreach (string property in m_objectFields.Keys)
            {
                m_fields.Set(property, m_worldModel.Object(m_objectFields[property]));
            }
            foreach (string property in m_scripts.Keys)
            {
                m_fields.Set(property, scriptFactory.CreateScript(m_scripts[property]));
            }
            m_resolved = true;
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

        private void CheckNotResolved()
        {
            if (m_resolved) throw new Exception("LazyFields instance already resolved.");
        }
    }

    public class UndoFieldSet : AxeSoftware.Quest.UndoLogger.IUndoAction
    {
        private string m_appliesTo;
        private string m_property;
        private object m_oldValue;
        private object m_newValue;

        public UndoFieldSet(string appliesTo, string property, object oldValue, object newValue)
        {
            System.Diagnostics.Debug.Assert(appliesTo != null);
            m_appliesTo = appliesTo;
            m_property = property;
            m_oldValue = oldValue;
            m_newValue = newValue;
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
            get { return m_oldValue; }
        }

        public object NewValue
        {
            get { return m_newValue; }
        }

        public void DoUndo(WorldModel worldModel)
        {
            worldModel.Object(m_appliesTo).Fields.SetSilent(Property, OldValue);
        }

        public void DoRedo(WorldModel worldModel)
        {
            worldModel.Object(m_appliesTo).Fields.SetSilent(Property, NewValue);
        }
    }
}