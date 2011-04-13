using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using AxeSoftware.Quest.Scripts;

// AttributeLoaders load properties which are stored in the XML as simple values.
// ExtendedAttributeLoaders load properties which are stored with nested XML.

namespace AxeSoftware.Quest
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
            foreach (Type t in AxeSoftware.Utility.Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
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

                int nestCount = 1;
                string key = null;

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            nestCount++;
                            if (reader.Name == "item")
                            {
                                key = reader.GetAttribute("key");
                            }
                            else
                            {
                                throw new InvalidOperationException(string.Format("Invalid element '{0}' in scriptdictionary block for '{1}.{2}' - expected only 'item' elements", reader.Name, current.Name, xmlElementName));
                            }
                            break;
                        case XmlNodeType.EndElement:
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
                            string value = reader.Value;
                            newDictionary.Add(key, value);
                            break;
                    }
                }

                throw new Exception("Unexpected end of XML data");
            }
        }
    }
}
