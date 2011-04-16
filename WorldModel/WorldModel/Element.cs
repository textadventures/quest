using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Scripts;

namespace AxeSoftware.Quest
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
    }

    // These are all sub-types of the "Object" element type
    public enum ObjectType
    {
        Object,
        Exit,
        Command,
        Game
    }

    public class Element
    {
        private Fields m_fields;
        private Fields m_metaFields;
        internal WorldModel m_worldModel;

        private ObjectType m_type;
        private ElementType m_elemType;
        private static Dictionary<ObjectType, string> s_typeStrings;
        private static Dictionary<ElementType, string> s_elemTypeStrings;

        static Element()
        {
            s_typeStrings = new Dictionary<ObjectType, string>();
            s_typeStrings.Add(ObjectType.Object, "object");
            s_typeStrings.Add(ObjectType.Exit, "exit");
            s_typeStrings.Add(ObjectType.Command, "command");
            s_typeStrings.Add(ObjectType.Game, "game");

            s_elemTypeStrings = new Dictionary<ElementType, string>();
            foreach (ElementType t in Enum.GetValues(typeof(ElementType)))
            {
                s_elemTypeStrings.Add(t, ((ElementTypeInfo)(typeof(ElementType).GetField(t.ToString()).GetCustomAttributes(typeof(ElementTypeInfo), false)[0])).Name);
            }
        }
        
        internal Element(WorldModel worldModel)
        {
            m_fields = new Fields(worldModel, this);
            m_metaFields = new Fields(null, this);
            Fields.AttributeChanged += Fields_AttributeChanged;
            Fields.AttributeChangedSilent += Fields_AttributeChangedSilent;
            m_worldModel = worldModel;
        }

        void Fields_AttributeChangedSilent(object sender, AttributeChangedEventArgs e)
        {
            // used by the Editor to receive notifications of updates when undoing
            m_worldModel.NotifyElementFieldUpdate(this, e.Property, e.Value, true);
        }

        void Fields_AttributeChanged(object sender, AttributeChangedEventArgs e)
        {
            m_worldModel.NotifyElementFieldUpdate(this, e.Property, e.Value, false);
            string changedScript = "changed" + e.Property;
            if (Fields.HasType<IScript>(changedScript))
            {
                m_worldModel.RunScript(Fields.GetAsType<IScript>(changedScript));
            }
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
            return string.Format("{0}: {1}", Utility.CapFirst(TypeString), Name);
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
    }
}
