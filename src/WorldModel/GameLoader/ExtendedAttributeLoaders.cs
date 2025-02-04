using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using TextAdventures.Quest.Scripts;

// AttributeLoaders load properties which are stored in the XML as simple values.
// ExtendedAttributeLoaders load properties which are stored with nested XML.

namespace TextAdventures.Quest
{
    partial class GameLoader
    {
        private interface IExtendedAttributeLoader
        {
            string AppliesTo { get; }
            void Load(XmlReader reader, Element current);
            GameLoader GameLoader { set; }
            bool SupportsMode(LoadMode mode);
        }

        private Dictionary<string, IExtendedAttributeLoader> m_extendedAttributeLoaders = new Dictionary<string, IExtendedAttributeLoader>();

        private void AddExtendedAttributeLoaders(LoadMode mode)
        {
            foreach (Type t in TextAdventures.Utility.Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                typeof(IExtendedAttributeLoader)))
            {
                AddExtendedAttributeLoader((IExtendedAttributeLoader)Activator.CreateInstance(t), mode);
            }
        }

        private void AddExtendedAttributeLoader(IExtendedAttributeLoader loader, LoadMode mode)
        {
            if (loader.SupportsMode(mode))
            {
                m_extendedAttributeLoaders.Add(loader.AppliesTo, loader);
                loader.GameLoader = this;
            }
        }

        private abstract class ExtendedAttributeLoaderBase : IExtendedAttributeLoader
        {
            public abstract string AppliesTo { get; }

            public abstract void Load(XmlReader reader, Element current);

            public GameLoader GameLoader
            {
                get;
                set;
            }

            public virtual bool SupportsMode(LoadMode mode)
            {
                return true;
            }
        }

        // TO DO: Would be good for ScriptDictionary to use the same <item><key>...</key><value>...</value></item>
        // format as the other dictionary types, then this can simply derive from DictionaryLoaderBase
        private class ScriptDictionaryLoader : ExtendedAttributeLoaderBase, IValueLoader
        {
            public override string AppliesTo
            {
                get { return "scriptdictionary"; }
            }

            public override void Load(XmlReader reader, Element current)
            {
                string currentXmlElementName = reader.Name;
                Dictionary<string, string> result = LoadScriptDictionary(reader, current, currentXmlElementName);
                current.Fields.LazyFields.AddScriptDictionary(currentXmlElementName, result);
            }

            private Dictionary<string, string> LoadScriptDictionary(XmlReader reader, Element current, string xmlElementName)
            {
                Dictionary<string, string> newDictionary = new Dictionary<string, string>();
                if (reader.IsEmptyElement) return newDictionary;

                int nestCount = 1;
                string key = null;
                string value = string.Empty;

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            nestCount++;
                            if (reader.Name == "item")
                            {
                                key = reader.GetAttribute("key");
                                value = string.Empty;
                            }
                            else
                            {
                                throw new InvalidOperationException(string.Format("Invalid element '{0}' in scriptdictionary block for '{1}.{2}' - expected only 'item' elements", reader.Name, current == null ? "(nested)" : current.Name, xmlElementName));
                            }
                            break;
                        case XmlNodeType.EndElement:
                            if (reader.Name == "item")
                            {
                                newDictionary.Add(key, value);
                            }

                            nestCount--;

                            if (nestCount == 0)
                            {
                                // Finished reading this XML block. Sanity check that we are
                                // closing the element that we opened with.
                                if (reader.Name == xmlElementName)
                                {
                                    return newDictionary;
                                }

                                throw new InvalidOperationException(string.Format("Expected closing tag '{0}'", xmlElementName));
                            }
                            break;
                        case XmlNodeType.Text:
                        case XmlNodeType.CDATA:
                            value = reader.Value;
                            break;
                    }
                }

                throw new Exception("Unexpected end of XML data");
            }

            public object GetValue(XElement xml)
            {
                var xmlReader = xml.CreateReader();
                // move to first sub-element
                xmlReader.Read();
                return new Types.LazyScriptDictionary(LoadScriptDictionary(xmlReader, null, "value"));
            }
        }

        private class StringListLoader : ExtendedAttributeLoaderBase, IValueLoader
        {
            public override string AppliesTo
            {
                get { return "stringlist"; }
            }

            public override void Load(XmlReader reader, Element current)
            {
                XElement xml = XElement.Load(reader.ReadSubtree());
                var values = xml.Elements("value").Select(e => e.Value);
                current.Fields.Set(reader.Name, new QuestList<string>(values));
            }

            public object GetValue(XElement xml)
            {
                return new QuestList<string>(xml.Elements("value").Select(e => e.Value));
            }
        }

        private class ListLoader : ExtendedAttributeLoaderBase, IValueLoader
        {
            public override string AppliesTo
            {
                get { return "list"; }
            }

            public override void Load(XmlReader reader, Element current)
            {
                XElement xml = XElement.Load(reader.ReadSubtree());
                var result = LoadQuestList(xml);
                current.Fields.Set(reader.Name, result);
            }

            private QuestList<object> LoadQuestList(XElement xml)
            {
                var result = new QuestList<object>();

                foreach (var xmlValue in xml.Elements("value"))
                {
                    var typeAttr = xmlValue.Attribute("type");
                    string type = typeAttr != null ? typeAttr.Value : null;
                    var value = GameLoader.ReadXmlValue(type, xmlValue);

                    result.Add(value);
                }
                return result;
            }

            public object GetValue(XElement xml)
            {
                return LoadQuestList(xml);
            }
        }

        private abstract class DictionaryLoaderBase<T> : ExtendedAttributeLoaderBase
        {
            protected IDictionary<string, T> LoadDictionary(XmlReader reader, string xmlElementName)
            {
                XElement xml = XElement.Load(reader.ReadSubtree());

                return LoadDictionaryFromXElement(xml, string.Format("{0}.{1}", xmlElementName, reader.Name));
            }

            protected IDictionary<string, T> LoadDictionaryFromXElement(XElement xml, string errorSource)
            {
                var result = new Dictionary<string, T>();

                var items = xml.Elements("item");
                foreach (var item in items)
                {
                    var key = item.Element("key");
                    var value = item.Element("value");

                    if (key == null)
                    {
                        GameLoader.AddError(string.Format("Missing key in dictionary for '{0}'", errorSource));
                    }
                    else if (value == null)
                    {
                        GameLoader.AddError(string.Format("Missing value in dictionary for '{0}'", errorSource));
                    }
                    else if (result.ContainsKey(key.Value))
                    {
                        GameLoader.AddError(string.Format("Duplicate key '{1}' in dictionary for '{0}'", errorSource, key.Value));
                    }
                    else
                    {
                        try
                        {
                            AddResultToDictionary(result, key.Value, value);
                        }
                        catch (Exception ex)
                        {
                            GameLoader.AddError(string.Format("Error adding key '{1}' to dictionary for '{0}': {2}",
                                                              errorSource, key.Value, ex.Message));
                        }
                    }
                }

                return result;
            }

            protected abstract void AddResultToDictionary(IDictionary<string, T> dictionary, string key, XElement xmlValue);
        }

        private class StringDictionaryLoader : DictionaryLoaderBase<string>, IValueLoader
        {
            public override string AppliesTo
            {
                get { return "stringdictionary"; }
            }

            public override void Load(XmlReader reader, Element current)
            {
                var result = LoadDictionary(reader, current.Name);
                current.Fields.Set(reader.Name, new QuestDictionary<string>(result));
            }

            public object GetValue(XElement xml)
            {
                return new QuestDictionary<string>(LoadDictionaryFromXElement(xml, "(nested stringdictionary)"));
            }

            protected override void AddResultToDictionary(IDictionary<string, string> dictionary, string key, XElement xmlValue)
            {
                string value = GameLoader.GetTemplate(xmlValue.Value);
                dictionary.Add(key, value);
            }
        }

        private class ObjectDictionaryLoader : DictionaryLoaderBase<string>, IValueLoader
        {
            public override string AppliesTo
            {
                get { return "objectdictionary"; }
            }

            public override void Load(XmlReader reader, Element current)
            {
                var result = LoadDictionary(reader, current.Name);
                current.Fields.LazyFields.AddObjectDictionary(reader.Name, result);
            }

            public object GetValue(XElement xml)
            {
                return new Types.LazyObjectDictionary(LoadDictionaryFromXElement(xml, "(nested objectdictionary)"));
            }

            protected override void AddResultToDictionary(IDictionary<string, string> dictionary, string key, XElement xmlValue)
            {
                dictionary.Add(key, xmlValue.Value);
            }
        }

        private class DictionaryLoader : DictionaryLoaderBase<object>, IValueLoader
        {
            public override string AppliesTo
            {
                get { return "dictionary"; }
            }

            public override void Load(XmlReader reader, Element current)
            {
                var result = LoadDictionary(reader, current.Name);
                current.Fields.Set(reader.Name, new QuestDictionary<object>(result));
            }

            protected override void AddResultToDictionary(IDictionary<string, object> dictionary, string key, XElement xmlValue)
            {
                var typeAttr = xmlValue.Attribute("type");
                string type = typeAttr != null ? typeAttr.Value : null;
                var value = GameLoader.ReadXmlValue(type, xmlValue);
                dictionary.Add(key, value);
            }

            public object GetValue(XElement xml)
            {
                return new QuestDictionary<object>(LoadDictionaryFromXElement(xml, "(nested dictionary)"));
            }
        }
    }
}
