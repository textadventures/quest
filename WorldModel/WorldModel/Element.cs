using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Scripts;

namespace TextAdventures.Quest
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ElementTypeInfo : Attribute
    {
        public ElementTypeInfo(string name)
        {
            Name = name;
        }
        public string Name;
    }

    public enum ElementType
    {
        [ElementTypeInfo("include")]
        IncludedLibrary,
        [ElementTypeInfo("implied")]
        ImpliedType,
        [ElementTypeInfo("template")]
        Template,
        [ElementTypeInfo("dynamictemplate")]
        DynamicTemplate,
        [ElementTypeInfo("delegate")]
        Delegate,
        [ElementTypeInfo("object")]
        Object,
        [ElementTypeInfo("type")]
        ObjectType,
        [ElementTypeInfo("function")]
        Function,
        [ElementTypeInfo("editor")]
        Editor,
        [ElementTypeInfo("tab")]
        EditorTab,
        [ElementTypeInfo("control")]
        EditorControl,
        [ElementTypeInfo("walkthrough")]
        Walkthrough,
        [ElementTypeInfo("javascript")]
        Javascript,
        [ElementTypeInfo("timer")]
        Timer,
        [ElementTypeInfo("resource")]
        Resource,
        [ElementTypeInfo("output")]
        Output,
    }

    // These are all sub-types of the "Object" element type
    public enum ObjectType
    {
        Object,
        Exit,
        Command,
        Game,
        TurnScript
    }

    public class Element
    {
        private Fields m_fields;
        private Fields m_metaFields;
        internal WorldModel m_worldModel;

        private ObjectType m_type;
        private ElementType m_elemType;
        private bool m_initialised = false;

        private static Dictionary<ObjectType, string> s_typeStrings;
        private static Dictionary<string, ObjectType> s_mapObjectTypeStringsToElementType;
        private static Dictionary<ElementType, string> s_elemTypeStrings;
        private static Dictionary<string, ElementType> s_mapElemTypeStringsToElementType;

        static Element()
        {
            s_typeStrings = new Dictionary<ObjectType, string>();
            s_typeStrings.Add(ObjectType.Object, "object");
            s_typeStrings.Add(ObjectType.Exit, "exit");
            s_typeStrings.Add(ObjectType.Command, "command");
            s_typeStrings.Add(ObjectType.Game, "game");
            s_typeStrings.Add(ObjectType.TurnScript, "turnscript");

            s_mapObjectTypeStringsToElementType = new Dictionary<string, ObjectType>();
            foreach (var item in s_typeStrings)
            {
                s_mapObjectTypeStringsToElementType.Add(item.Value, item.Key);
            }

            s_elemTypeStrings = new Dictionary<ElementType, string>();
            foreach (ElementType t in Enum.GetValues(typeof(ElementType)))
            {
                s_elemTypeStrings.Add(t, ((ElementTypeInfo)(typeof(ElementType).GetField(t.ToString()).GetCustomAttributes(typeof(ElementTypeInfo), false)[0])).Name);
            }

            s_mapElemTypeStringsToElementType = new Dictionary<string, ElementType>();
            foreach (var item in s_elemTypeStrings)
            {
                s_mapElemTypeStringsToElementType.Add(item.Value, item.Key);
            }
        }

        internal static ElementType GetElementTypeForTypeString(string typeString)
        {
            return s_mapElemTypeStringsToElementType[typeString];
        }

        internal static ObjectType GetObjectTypeForTypeString(string typeString)
        {
            return s_mapObjectTypeStringsToElementType[typeString];
        }

        internal static string GetTypeStringForElementType(ElementType type)
        {
            return s_elemTypeStrings[type];
        }

        internal static string GetTypeStringForObjectType(ObjectType type)
        {
            return s_typeStrings[type];
        }

        internal Element(WorldModel worldModel)
            : this(worldModel, null)
        {
        }

        internal Element(WorldModel worldModel, Element element)
        {
            m_worldModel = worldModel;

            if (element == null)
            {
                // New element
                m_fields = new Fields(worldModel, this, false);
                m_metaFields = new Fields(worldModel, this, true);
            }
            else
            {
                // Clone element
                m_fields = element.Fields.Clone(this);
                m_metaFields = element.MetaFields.Clone(this);
            }

            Fields.AttributeChanged += Fields_AttributeChanged;
            Fields.AttributeChangedSilent += Fields_AttributeChangedSilent;
            m_metaFields.AttributeChanged += MetaFields_AttributeChanged;
            m_metaFields.AttributeChangedSilent += MetaFields_AttributeChangedSilent;

        }

        void Fields_AttributeChangedSilent(object sender, AttributeChangedEventArgs e)
        {
            // used by the Editor to receive notifications of updates when undoing
            if (e.InheritedTypesSet)
            {
                m_worldModel.NotifyElementRefreshed(this);
            }
            else
            {
                m_worldModel.NotifyElementFieldUpdate(this, e.Property, e.Value, true);
            }
        }

        void Fields_AttributeChanged(object sender, AttributeChangedEventArgs e)
        {
            m_worldModel.NotifyElementFieldUpdate(this, e.Property, e.Value, false);
            if (!m_worldModel.EditMode)
            {
                string changedScript = "changed" + e.Property;
                if (Fields.HasType<IScript>(changedScript))
                {
                    Parameters parameters = new Parameters("oldvalue", e.OldValue);
                    m_worldModel.RunScript(Fields.GetAsType<IScript>(changedScript), parameters, this);
                }
            }
        }

        void MetaFields_AttributeChanged(object sender, AttributeChangedEventArgs e)
        {
            m_worldModel.NotifyElementMetaFieldUpdate(this, e.Property, e.Value, false);
        }

        void MetaFields_AttributeChangedSilent(object sender, AttributeChangedEventArgs e)
        {
            m_worldModel.NotifyElementMetaFieldUpdate(this, e.Property, e.Value, true);
        }

        public IScript GetAction(string action)
        {
            return Fields.GetAsType<IScript>(action);
        }

        public Fields Fields
        {
            get { return m_fields; }
        }

        public Fields MetaFields
        {
            get { return m_metaFields; }
        }

        public string Name
        {
            get { return Fields.GetString("name"); }
            set { Fields.Set("name", value); }
        }

        public Element Parent
        {
            get { return Fields.GetObject("parent"); }
            set { Fields.Set("parent", value); }
        }

        internal void AddType(Element addType)
        {
            m_fields.AddType(addType);
        }

        internal DebugData GetDebugData()
        {
            return m_fields.GetDebugData();
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", TextAdventures.Utility.Strings.CapFirst(TypeString), Name);
        }

        public ObjectType Type
        {
            get
            {
                return m_type;
            }
            set
            {
                m_type = value;
                Fields.Set("type", TypeString);
            }
        }

        public ElementType ElemType
        {
            get
            {
                return m_elemType;
            }
            set
            {
                m_elemType = value;
                Fields.Set("elementtype", ElementTypeString);
            }
        }

        internal string TypeString
        {
            get
            {
                return s_typeStrings[m_type];
            }
        }

        internal string ElementTypeString
        {
            get
            {
                return s_elemTypeStrings[m_elemType];
            }
        }

        internal bool Initialised
        {
            get { return m_initialised; }
        }

        internal void FinishedInitialisation()
        {
            m_initialised = true;
        }

        public Element Clone()
        {
            return Clone(e => true);
        }

        public Element Clone(Func<Element, bool> canCloneChild)
        {
            Element newElement = m_worldModel.GetElementFactory(m_elemType).CloneElement(this, m_worldModel.GetUniqueElementName(Name));

            if (this.MetaFields[MetaFieldDefinitions.Library])
            {
                newElement.MetaFields[MetaFieldDefinitions.Library] = false;
                newElement.MetaFields[MetaFieldDefinitions.Filename] = null;
            }

            // Pre-fetch all children of this element
            var children = m_worldModel.Elements.GetDirectChildren(this).ToList();

            foreach (Element child in children.Where(e => canCloneChild(e)))
            {
                Element cloneChild = child.Clone();
                cloneChild.Parent = newElement;
            }

            return newElement;
        }

        internal WorldModel WorldModel
        {
            get { return m_worldModel; }
        }
    }
}
