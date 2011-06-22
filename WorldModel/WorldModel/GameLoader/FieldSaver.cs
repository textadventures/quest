using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using AxeSoftware.Quest.Scripts;

namespace AxeSoftware.Quest
{
    internal class FieldSaver
    {
        private Dictionary<Type, IFieldSaver> m_savers = new Dictionary<Type, IFieldSaver>();
        private GameSaver m_saver;

        public FieldSaver(GameSaver saver)
        {
            m_saver = saver;

            // Use Reflection to create instances of all IFieldSavers
            foreach (Type t in AxeSoftware.Utility.Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                typeof(IFieldSaver)))
            {
                AddSaver((IFieldSaver)Activator.CreateInstance(t));
            }
        }

        private void AddSaver(IFieldSaver saver)
        {
            m_savers.Add(saver.AppliesTo, saver);
            saver.GameSaver = m_saver;
        }

        public void Save(GameXmlWriter writer, Element element, string attribute, object value)
        {
            if (value == null) return;
            IFieldSaver saver;
            if (TryGetSaver(value.GetType(), out saver))
            {
                saver.Save(writer, element, attribute, value);
            }
            else
            {
                throw new Exception(string.Format("ERROR: No FieldSaver for attribute {0}, type: {1}", attribute, value.GetType().ToString()));
            }
        }

        private bool TryGetSaver(Type type, out IFieldSaver saver)
        {
            if (m_savers.TryGetValue(type, out saver)) return true;

            foreach (KeyValuePair<Type, IFieldSaver> s in m_savers)
            {
                if (s.Key.IsAssignableFrom(type))
                {
                    saver = s.Value;
                    return true;
                }
            }

            return false;
        }

        private interface IFieldSaver
        {
            Type AppliesTo { get; }
            void Save(GameXmlWriter writer, Element element, string attribute, object value);
            GameSaver GameSaver { set; }
        }

        private abstract class FieldSaverBase : IFieldSaver
        {
            public abstract Type AppliesTo { get; }
            public abstract void Save(GameXmlWriter writer, Element element, string attribute, object value);

            protected void WriteAttribute(GameXmlWriter writer, Element element, string attribute, string type, string value)
            {
                writer.WriteStartElement(attribute);
                if (!GameSaver.IsImpliedType(element, attribute, type) || value.Length == 0)
                {
                    writer.WriteAttributeString("type", type);
                }
                writer.WriteString(value);
                writer.WriteEndElement();
            }

            public GameSaver GameSaver { get; set; }
        }

        private class StringSaver : FieldSaverBase
        {
            #region IFieldSaver Members

            public override Type AppliesTo
            {
                get { return typeof(string); }
            }

            public override void Save(GameXmlWriter writer, Element element, string attribute, object value)
            {
                string strValue = (string)value;
                base.WriteAttribute(writer, element, attribute, "string", strValue);
            }

            #endregion
        }

        private class BooleanSaver : FieldSaverBase
        {
            #region IFieldSaver Members

            public override Type AppliesTo
            {
                get { return typeof(bool); }
            }

            public override void Save(GameXmlWriter writer, Element element, string attribute, object value)
            {
                bool boolVal = (bool)value;
                if (boolVal)
                {
                    writer.WriteElementString(attribute, null);
                }
                else
                {
                    base.WriteAttribute(writer, element, attribute, "boolean", "false");
                }
            }

            #endregion
        }

        private class StringListSaver : FieldSaverBase
        {
            #region IFieldSaver Members

            public override Type AppliesTo
            {
                get { return typeof(QuestList<string>); }
            }

            public override void Save(GameXmlWriter writer, Element element, string attribute, object value)
            {
                QuestList<string> list = (QuestList<string>)value;
                string saveString = String.Join("; ", list.ToArray());
                base.WriteAttribute(writer, element, attribute, "list", saveString);
            }

            #endregion
        }

        private class StringDictionarySaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(QuestDictionary<string>); }
            }

            public override void Save(GameXmlWriter writer, Element element, string attribute, object value)
            {
                QuestDictionary<string> dictionary = (QuestDictionary<string>)value;
                base.WriteAttribute(writer, element, attribute, "stringdictionary", dictionary.SaveString());
            }
        }

        private class ScriptSaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(IScript); }
            }

            public override void Save(GameXmlWriter writer, Element element, string attribute, object value)
            {
                IScript script = (IScript)value;
                string savedScript = GameSaver.SaveScript(writer, script, 1);
                if (savedScript.Trim().Length > 0)
                {
                    base.WriteAttribute(writer, element, attribute, "script", savedScript);
                }
            }
        }

        private class IntSaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(int); }
            }

            public override void Save(GameXmlWriter writer, Element element, string attribute, object value)
            {
                int number = (int)value;
                base.WriteAttribute(writer, element, attribute, "int", number.ToString());
            }
        }

        private class DoubleSaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(double); }
            }

            public override void Save(GameXmlWriter writer, Element element, string attribute, object value)
            {
                double number = (double)value;
                base.WriteAttribute(writer, element, attribute, "double", number.ToString());
            }
        }

        private class DelegateImplementationSaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(DelegateImplementation); }
            }

            public override void Save(GameXmlWriter writer, Element element, string attribute, object value)
            {
                DelegateImplementation impl = (DelegateImplementation)value;
                base.WriteAttribute(writer, element, attribute, impl.Definition.Name, GameSaver.SaveScript(writer, impl.Implementation.Fields[FieldDefinitions.Script], 1));
            }
        }

        private class SimplePatternSaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(EditorCommandPattern); }
            }

            public override void Save(GameXmlWriter writer, Element element, string attribute, object value)
            {
                EditorCommandPattern pattern = (EditorCommandPattern)value;
                base.WriteAttribute(writer, element, attribute, "simplepattern", pattern.Pattern);
            }
        }

        private class ScriptDictionarySaver : IFieldSaver
        {
            public Type AppliesTo
            {
                get { return typeof(QuestDictionary<IScript>); }
            }

            public void Save(GameXmlWriter writer, Element element, string attribute, object value)
            {
                writer.WriteStartElement(attribute);
                if (!GameSaver.IsImpliedType(element, attribute, "scriptdictionary"))
                {
                    writer.WriteAttributeString("type", "scriptdictionary");
                }

                QuestDictionary<IScript> dictionary = (QuestDictionary<IScript>)value;

                foreach (var item in dictionary)
                {
                    writer.WriteStartElement("item");
                    writer.WriteAttributeString("key", item.Key);
                    writer.WriteString(GameSaver.SaveScript(writer, item.Value, 0));
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            public GameSaver GameSaver { get; set; }
        }

        private class ObjectReferenceSaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(Element); }
            }

            public override void Save(GameXmlWriter writer, Element element, string attribute, object value)
            {
                base.WriteAttribute(writer, element, attribute, "object", ((Element)value).Name);
            }
        }
    }
}
