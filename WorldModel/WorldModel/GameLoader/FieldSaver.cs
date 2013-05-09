using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using TextAdventures.Quest.Scripts;

namespace TextAdventures.Quest
{
    internal class FieldSaver
    {
        private Dictionary<Type, IFieldSaver> m_savers = new Dictionary<Type, IFieldSaver>();
        private GameSaver m_saver;

        public FieldSaver(GameSaver saver)
        {
            m_saver = saver;

            // Use Reflection to create instances of all IFieldSavers
            foreach (Type t in TextAdventures.Utility.Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                typeof(IFieldSaver)))
            {
                AddSaver((IFieldSaver)Activator.CreateInstance(t));
            }
        }

        private void AddSaver(IFieldSaver saver)
        {
            bool ignore = (saver.MinVersion.HasValue && saver.MinVersion.Value > m_saver.Version)
                          || (saver.MaxVersion.HasValue && saver.MaxVersion.Value < m_saver.Version);

            if (ignore) return;

            m_savers.Add(saver.AppliesTo, saver);
            saver.GameSaver = m_saver;
            saver.FieldSaver = this;
        }

        public void Save(GameXmlWriter writer, Element element, string attribute, object value)
        {
            if (value == null) return;
            IFieldSaver saver;
            if (TryGetSaver(value.GetType(), out saver))
            {
                try
                {
                    saver.Save(writer, element, attribute, value);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Failed to save field '{0}.{1}' with value '{2}' - {3}",
                                                      element.Name, attribute, value, ex.Message), ex);
                }
            }
            else
            {
                throw new Exception(string.Format("ERROR: No FieldSaver for attribute {0}, type: {1}", attribute, value.GetType()));
            }
        }

        public void SaveValue(GameXmlWriter writer, string xmlElementName, object value)
        {
            if (value == null) return;
            IFieldSaver saver;
            if (TryGetSaver(value.GetType(), out saver))
            {
                saver.Save(writer, xmlElementName, value);
            }
            else
            {
                throw new Exception(string.Format("ERROR: No ValueSaver for XML element {0}, type: {1}", xmlElementName, value.GetType()));
            }
        }

        private bool TryGetSaver(Type type, out IFieldSaver saver)
        {
            if (m_savers.TryGetValue(type, out saver)) return true;

            foreach (var s in m_savers.Where(s => s.Key.IsAssignableFrom(type)))
            {
                saver = s.Value;
                return true;
            }

            return false;
        }

        private interface IFieldSaver
        {
            Type AppliesTo { get; }
            void Save(GameXmlWriter writer, Element element, string attribute, object value);
            void Save(GameXmlWriter writer, string xmlElementName, object value);
            GameSaver GameSaver { set; }
            FieldSaver FieldSaver { set; }
            WorldModelVersion? MinVersion { get; }
            WorldModelVersion? MaxVersion { get; }
        }

        private abstract class FieldSaverBase : IFieldSaver
        {
            public abstract Type AppliesTo { get; }
            public abstract void Save(GameXmlWriter writer, Element element, string attribute, object value);

            private static Regex s_regex = new Regex("^[A-Za-z0-9]*$");

            protected void WriteAttribute(GameXmlWriter writer, Element element, string attribute, string type, string value)
            {
                if (!s_regex.IsMatch(attribute))
                {
                    // For attribute names with spaces or accented characters, we output
                    //      <attr name="my attribute" ... />
                    writer.WriteStartElement("attr");
                    writer.WriteAttributeString("name", attribute);
                }
                else
                {
                    // For attribute names without spaces, we output
                    //      <myattribute ... />
                    writer.WriteStartElement(attribute);
                }
                if (element == null || !GameSaver.IsImpliedType(element, attribute, type) || value.Length == 0)
                {
                    writer.WriteAttributeString("type", type);
                }
                writer.WriteString(value);
                writer.WriteEndElement();
            }

            public void Save(GameXmlWriter writer, string xmlElementName, object value)
            {
                Save(writer, null, xmlElementName, value);
            }

            public GameSaver GameSaver { get; set; }
            public FieldSaver FieldSaver { get; set; }

            public virtual WorldModelVersion? MinVersion { get { return null; } }
            public virtual WorldModelVersion? MaxVersion { get { return null; } }
        }

        private class StringSaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(string); }
            }

            public override void Save(GameXmlWriter writer, Element element, string attribute, object value)
            {
                string strValue = (string)value;
                base.WriteAttribute(writer, element, attribute, "string", strValue);
            }
        }

        private class BooleanSaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(bool); }
            }

            public override void Save(GameXmlWriter writer, Element element, string attribute, object value)
            {
                bool boolVal = (bool)value;
                if (boolVal)
                {
                    if (attribute.Contains(" "))
                    {
                        base.WriteAttribute(writer, element, attribute, "boolean", "true");
                    }
                    else
                    {
                        writer.WriteElementString(attribute, null);
                    }
                }
                else
                {
                    base.WriteAttribute(writer, element, attribute, "boolean", "false");
                }
            }
        }

        private class LegacyStringListSaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(QuestList<string>); }
            }

            public override WorldModelVersion? MaxVersion
            {
                get { return WorldModelVersion.v530; }
            }

            public override void Save(GameXmlWriter writer, Element element, string attribute, object value)
            {
                QuestList<string> list = (QuestList<string>)value;
                string saveString = String.Join("; ", list.ToArray());
                base.WriteAttribute(writer, element, attribute, "list", saveString);
            }
        }

        private class StringListSaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(QuestList<string>); }
            }

            public override WorldModelVersion? MinVersion
            {
                get { return WorldModelVersion.v540; }
            }

            public override void Save(GameXmlWriter writer, Element element, string attribute, object value)
            {
                writer.WriteStartElement(attribute);
                if (element == null || !GameSaver.IsImpliedType(element, attribute, "stringlist"))
                {
                    writer.WriteAttributeString("type", "stringlist");
                }

                QuestList<string> list = (QuestList<string>)value;

                foreach (var item in list)
                {
                    writer.WriteStartElement("value");
                    writer.WriteString(item);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
        }

        private class ListSaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof (QuestList<object>); }
            }

            public override void Save(GameXmlWriter writer, Element element, string attribute, object value)
            {
                writer.WriteStartElement(attribute);
                if (element == null || !GameSaver.IsImpliedType(element, attribute, "list"))
                {
                    writer.WriteAttributeString("type", "list");
                }

                QuestList<object> list = (QuestList<object>)value;

                foreach (var item in list)
                {
                    FieldSaver.SaveValue(writer, "value", item);
                }
                writer.WriteEndElement();
            }
        }

        private class LegacyStringDictionarySaver : FieldSaverBase
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

            public override WorldModelVersion? MaxVersion
            {
                get { return WorldModelVersion.v530; }
            }
        }

        private class ObjectListSaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(QuestList<Element>); }
            }

            public override void Save(GameXmlWriter writer, Element element, string attribute, object value)
            {
                QuestList<Element> list = (QuestList<Element>)value;
                string saveString = String.Join("; ", list.Select(e => e.Name));
                base.WriteAttribute(writer, element, attribute, "objectlist", saveString);
            }
        }

        private class LegacyObjectDictionarySaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(QuestDictionary<Element>); }
            }

            public override void Save(GameXmlWriter writer, Element element, string attribute, object value)
            {
                QuestDictionary<Element> dictionary = (QuestDictionary<Element>)value;
                base.WriteAttribute(writer, element, attribute, "objectdictionary", dictionary.SaveString(o => o.Name));
            }

            public override WorldModelVersion? MaxVersion
            {
                get { return WorldModelVersion.v530; }
            }
        }

        private abstract class DictionarySaverBase<T> : FieldSaverBase
        {
            public override WorldModelVersion? MinVersion
            {
                get { return WorldModelVersion.v540; }
            }

            protected abstract string TypeName { get; }

            protected abstract string GetValueString(T value);

            public override void Save(GameXmlWriter writer, Element element, string attribute, object value)
            {
                writer.WriteStartElement(attribute);
                if (element == null || !GameSaver.IsImpliedType(element, attribute, TypeName))
                {
                    writer.WriteAttributeString("type", TypeName);
                }

                var dictionary = (QuestDictionary<T>)value;

                foreach (var item in dictionary)
                {
                    writer.WriteStartElement("item");
                    writer.WriteStartElement("key");
                    writer.WriteString(item.Key);
                    writer.WriteEndElement();
                    WriteXml(writer, item.Value);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            protected virtual void WriteXml(GameXmlWriter writer, T value)
            {
                writer.WriteStartElement("value");
                writer.WriteString(GetValueString(value));
                writer.WriteEndElement();
            }
        }

        private class StringDictionarySaver : DictionarySaverBase<string>
        {
            public override Type AppliesTo
            {
                get { return typeof(QuestDictionary<string>); }
            }

            protected override string TypeName
            {
                get { return "stringdictionary"; }
            }

            protected override string GetValueString(string value)
            {
                return value;
            }
        }

        private class ObjectDictionarySaver : DictionarySaverBase<Element>
        {
            public override Type AppliesTo
            {
                get { return typeof(QuestDictionary<Element>); }
            }

            protected override string TypeName
            {
                get { return "objectdictionary"; }
            }

            protected override string GetValueString(Element value)
            {
                return value.Name;
            }
        }

        private class DictionarySaver : DictionarySaverBase<object>
        {
            public override Type AppliesTo
            {
                get { return typeof(QuestDictionary<object>); }
            }

            protected override string TypeName
            {
                get { return "dictionary"; }
            }

            protected override string GetValueString(object value)
            {
                throw new NotImplementedException();
            }

            protected override void WriteXml(GameXmlWriter writer, object value)
            {
                FieldSaver.SaveValue(writer, "value", value);
            }
        }

        private class LegacyDictionarySaver : FieldSaverBase
        {
            public override Type AppliesTo
            {
                get { return typeof(QuestDictionary<object>); }
            }

            public override void Save(GameXmlWriter writer, Element element, string attribute, object value)
            {
                // Do nothing - objectdictionaries are not saved for ASL 530 and earlier.
            }

            public override WorldModelVersion? MaxVersion
            {
                get { return WorldModelVersion.v530; }
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
                base.WriteAttribute(writer, element, attribute, "script", savedScript);
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
                base.WriteAttribute(writer, element, attribute, "double", number.ToString(System.Globalization.CultureInfo.InvariantCulture));
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

        // TO DO: Would be good for ScriptDictionary to use the same <item><key>...</key><value>...</value></item>
        // format as the other dictionary types, then this can simply derive from DictionarySaverBase
        private class ScriptDictionarySaver : IFieldSaver
        {
            public Type AppliesTo
            {
                get { return typeof(QuestDictionary<IScript>); }
            }

            public void Save(GameXmlWriter writer, Element element, string attribute, object value)
            {
                writer.WriteStartElement(attribute);
                if (element == null || !GameSaver.IsImpliedType(element, attribute, "scriptdictionary"))
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

            public void Save(GameXmlWriter writer, string xmlElementName, object value)
            {
                Save(writer, null, xmlElementName, value);
            }

            public GameSaver GameSaver { get; set; }
            public FieldSaver FieldSaver { get; set; }

            public WorldModelVersion? MinVersion { get { return null; } }
            public WorldModelVersion? MaxVersion { get { return null; } }
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
