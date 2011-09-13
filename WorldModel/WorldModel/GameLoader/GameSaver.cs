using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using AxeSoftware.Quest.Scripts;

namespace AxeSoftware.Quest
{
    public enum SaveMode
    {
        SavedGame,
        Editor,
        Package
    }

    internal partial class GameSaver
    {
        private WorldModel m_worldModel;
        private Dictionary<ElementType, IElementsSaver> m_elementsSavers = new Dictionary<ElementType, IElementsSaver>();
        private Dictionary<ElementType, IElementSaver> m_elementSavers = new Dictionary<ElementType, IElementSaver>();
        private Dictionary<string, string> m_impliedTypes;
        private SaveMode m_mode;

        public GameSaver(WorldModel worldModel)
        {
            m_worldModel = worldModel;

            // Use Reflection to create instances of all IElementSavers (save individual elements)
            foreach (Type t in AxeSoftware.Utility.Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                typeof(IElementSaver)))
            {
                AddElementSaver((IElementSaver)Activator.CreateInstance(t));
            }

            // Use Reflection to create instances of all IElementsSavers (save all elements of a type)
            foreach (Type t in AxeSoftware.Utility.Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                typeof(IElementsSaver)))
            {
                AddElementsSaver((IElementsSaver)Activator.CreateInstance(t));
            }
        }

        private void AddElementSaver(IElementSaver saver)
        {
            saver.GameSaver = this;
            m_elementSavers.Add(saver.AppliesTo, saver);
        }

        private void AddElementsSaver(IElementsSaver saver)
        {
            saver.GameSaver = this;
            m_elementsSavers.Add(saver.AppliesTo, saver);
        }

        private IElementSaver GetElementSaver(ElementType t)
        {
            return m_elementSavers[t];
        }

        public string Save(SaveMode mode)
        {
            m_mode = mode;
            GameXmlWriter writer = new GameXmlWriter(mode);

            UpdateImpliedTypesCache();

            Version ver = new Version(System.Windows.Forms.Application.ProductVersion);
            writer.WriteComment(string.Format("Saved by Quest {0}", ver));
            writer.WriteStartElement("asl");
            writer.WriteAttributeString("version", "500");
            if (mode == SaveMode.SavedGame)
            {
                writer.WriteAttributeString("original", m_worldModel.Filename);
            }

            foreach (ElementType t in Enum.GetValues(typeof(ElementType)))
            {
                if (m_elementsSavers.ContainsKey(t))
                {
                    // We have an IElementsSaver which saves all elements of a particular type at once
                    m_elementsSavers[t].Save(writer, m_worldModel);
                }
                else
                {
                    // Save the elements individually
                    IElementSaver saver;
                    if (m_elementSavers.TryGetValue(t, out saver))
                    {
                        if (saver.AutoSave)
                        {
                            foreach (Element e in m_worldModel.Elements.GetElements(t).Where(e => CanSave(e)))
                            {
                                saver.Save(writer, e);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("ERROR: No ElementSaver for type " + t.ToString());
                    }
                }
            }

            writer.WriteEndElement();
            writer.Close();

            return writer.ToString();
        }

        private bool CanSave(Element e)
        {
            switch (m_mode)
            {
                case SaveMode.SavedGame:
                    return true;
                case SaveMode.Editor:
                    return !e.MetaFields[MetaFieldDefinitions.Library];
                case SaveMode.Package:
                    if (e.ElemType == ElementType.IncludedLibrary) return false;
                    if (e.MetaFields[MetaFieldDefinitions.EditorLibrary]) return false;
                    return true;
                default:
                    throw new Exception("SaveMode not implemented");
            }
        }

        public string SaveScript(GameXmlWriter writer, IScript script, int indent)
        {
            return IndentScript(script.Save(), writer.IndentLevel + indent, writer.IndentChars);
        }

        private string IndentScript(string script, int indentLevel, string indentChars)
        {
            List<string> lines = Utility.SplitIntoLines(script);
            string result = Environment.NewLine;

            foreach (string line in lines)
            {
                AddLine(ref result, ref indentLevel, line, indentChars);
            }

            return result;
        }

        private void AddLine(ref string result, ref int indentLevel, string line, string indentChars)
        {
            if (line.Length == 0) return;

            if (line.StartsWith("}"))
            {
                // if line starts with closing brace, de-indent, put the brace on a line on its own,
                // then resume with the rest of the line.
                indentLevel--;
                result += GetIndentChars(indentLevel, indentChars) + "}" + Environment.NewLine;
                AddLine(ref result, ref indentLevel, line.Substring(1), indentChars);
                return;
            }

            // Add this line at the current indent level
            result += GetIndentChars(indentLevel, indentChars) + line + Environment.NewLine;

            // Now work out the indent level for the following line
            for (int i = 0; i < line.Length; i++)
            {
                string curChar = line.Substring(i, 1);
                if (curChar == "{") indentLevel++;
                if (curChar == "}") indentLevel--;
            }
        }

        private string GetIndentChars(int indentLevel, string indentChars)
        {
            string indentString = string.Empty;

            for (int i = 0; i < indentLevel; i++)
            {
                indentString += indentChars;
            }
            return indentString;
        }

        private void UpdateImpliedTypesCache()
        {
            m_impliedTypes = new Dictionary<string, string>();
            foreach (Element impliedType in m_worldModel.Elements.GetElements(ElementType.ImpliedType))
            {
                string element = impliedType.Fields[FieldDefinitions.Element];
                string property = impliedType.Fields[FieldDefinitions.Property];
                string type = impliedType.Fields[FieldDefinitions.Type];
                m_impliedTypes.Add(GetImpliedTypeKey(element, property), type);
            }
        }

        internal bool IsImpliedType(Element element, string attribute, string type)
        {
            string impliedType;
            string elementType;
            if (element.ElemType == ElementType.Object)
            {
                elementType = element.TypeString;
            }
            else
            {
                elementType = element.ElementTypeString;
            }

            if (m_impliedTypes.TryGetValue(GetImpliedTypeKey(elementType, attribute), out impliedType))
            {
                return (type == impliedType);
            }
            return (type == "string");
        }

        private string GetImpliedTypeKey(string element, string property)
        {
            return string.Concat(element, "~", property);
        }

        private interface IElementsSaver
        {
            ElementType AppliesTo { get; }
            void Save(GameXmlWriter writer, WorldModel worldModel);
            GameSaver GameSaver { set; }
        }

        private interface IElementSaver
        {
            ElementType AppliesTo { get; }
            void Save(GameXmlWriter writer, Element e);
            GameSaver GameSaver { set; }
            bool AutoSave { get; }
        }

        private abstract class ElementSaverBase : IElementSaver
        {
            private FieldSaver m_fieldSaver;
            private GameSaver m_gameSaver;
            private List<string> m_ignoreFields = new List<string>();

            public ElementSaverBase()
            {
                AddIgnoreField("name");
                AddIgnoreField("elementtype");
            }

            protected void AddIgnoreField(string name)
            {
                m_ignoreFields.Add(name);
            }

            protected void SaveFields(GameXmlWriter writer, Element e)
            {
                foreach (Element includedType in e.Fields.Types)
                {
                    if (CanSaveTypeName(writer, includedType.Name, e))
                    {
                        writer.WriteStartElement("inherit");
                        writer.WriteAttributeString("name", includedType.Name);
                        writer.WriteEndElement();
                    }
                }

                IEnumerable<string> fieldNames = e.Fields.FieldNames;
                if (writer.Mode != SaveMode.Editor)
                {
                    fieldNames = fieldNames.Union(e.Fields.FieldExtensionNames);
                }

                foreach (string attribute in fieldNames)
                {
                    if (CanSaveAttribute(attribute, e))
                    {
                        object value = e.Fields.Get(attribute);
                        m_fieldSaver.Save(writer, e, attribute, value);
                    }
                }
            }

            protected virtual void Initialise()
            {
            }

            protected virtual bool CanSaveAttribute(string attribute, Element e)
            {
                return !m_ignoreFields.Contains(attribute);
            }

            protected virtual bool CanSaveTypeName(GameXmlWriter writer, string type, Element e)
            {
                if (writer.Mode == SaveMode.Package)
                {
                    if (e.WorldModel.Elements.Get(ElementType.ObjectType, type).MetaFields[MetaFieldDefinitions.EditorLibrary])
                    {
                        return false;
                    }
                }
                return !WorldModel.DefaultTypeNames.ContainsValue(type);
            }

            #region IElementSaver Members

            public abstract ElementType AppliesTo { get; }

            public abstract void Save(GameXmlWriter writer, Element e);

            public GameSaver GameSaver
            {
                get { return m_gameSaver; }
                set
                {
                    m_gameSaver = value;
                    m_fieldSaver = new FieldSaver(m_gameSaver);
                    Initialise();
                }
            }

            public virtual bool AutoSave
            {
                get
                {
                    return true;
                }
            }

            #endregion
        }
    }
}
