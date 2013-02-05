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
        private class ScriptDictionaryLoader : ExtendedAttributeLoaderBase
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
                                throw new InvalidOperationException(string.Format("Invalid element '{0}' in scriptdictionary block for '{1}.{2}' - expected only 'item' elements", reader.Name, current.Name, xmlElementName));
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
        }

        private class StringListLoader : ExtendedAttributeLoaderBase
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
        }

        private class GenericDictionaryLoader : ExtendedAttributeLoaderBase
        {
            public override string AppliesTo
            {
                get { return "dictionary"; }
            }

            public override void Load(XmlReader reader, Element current)
            {
                // TO DO: Implement
            }
        }

        private abstract class DictionaryLoaderBase : ExtendedAttributeLoaderBase
        {
            protected IDictionary<string, string> LoadDictionary(XmlReader reader, Element current)
            {
                XElement xml = XElement.Load(reader.ReadSubtree());

                var result = new Dictionary<string, string>();

                var items = xml.Elements("item");
                foreach (var item in items)
                {
                    var key = item.Element("key");
                    var value = item.Element("value");

                    if (key == null)
                    {
                        GameLoader.AddError(string.Format("Missing key in dictionary for '{0}.{1}'", current.Name, reader.Name));
                    }
                    else if (value == null)
                    {
                        GameLoader.AddError(string.Format("Missing value in dictionary for '{0}.{1}'", current.Name, reader.Name));
                    }
                    else if (result.ContainsKey(key.Value))
                    {
                        GameLoader.AddError(string.Format("Duplicate key '{2}' in dictionary for '{0}.{1}'", current.Name, reader.Name, key.Value));
                    }
                    else
                    {
                        try
                        {
                            result.Add(key.Value, value.Value);
                        }
                        catch (Exception ex)
                        {
                            GameLoader.AddError(string.Format("Error adding key '{2}' to dictionary for '{0}.{1}': {3}", current.Name, reader.Name, key.Value, ex.Message));
                        }
                    }
                }

                return result;
            }
        }

        private class StringDictionaryLoader : DictionaryLoaderBase
        {
            public override string AppliesTo
            {
                get { return "stringdictionary"; }
            }

            public override void Load(XmlReader reader, Element current)
            {
                var result = LoadDictionary(reader, current);
                current.Fields.Set(reader.Name, new QuestDictionary<string>(result));
            }
        }

        private class ObjectDictionaryLoader : DictionaryLoaderBase
        {
            public override string AppliesTo
            {
                get { return "objectdictionary"; }
            }

            public override void Load(XmlReader reader, Element current)
            {
                var result = LoadDictionary(reader, current);
                current.Fields.LazyFields.AddObjectDictionary(reader.Name, result);
            }
        }
    }
}
