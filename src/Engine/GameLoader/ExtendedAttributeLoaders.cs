using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using QuestViva.Engine.Types;
using QuestViva.Utility;
// ReSharper disable UnusedType.Local

// AttributeLoaders load properties which are stored in the XML as simple values.
// ExtendedAttributeLoaders load properties which are stored with nested XML.

namespace QuestViva.Engine.GameLoader;

internal partial class GameLoader
{
    private interface IExtendedAttributeLoader
    {
        string AppliesTo { get; }
        void Load(XmlReader reader, Element current);
        GameLoader GameLoader { set; }
        bool SupportsMode(LoadMode mode);
    }

    private void AddExtendedAttributeLoaders(LoadMode mode)
    {
        foreach (var t in Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                     typeof(IExtendedAttributeLoader)))
        {
            AddExtendedAttributeLoader((IExtendedAttributeLoader)Activator.CreateInstance(t)!, mode);
        }
    }

    private void AddExtendedAttributeLoader(IExtendedAttributeLoader loader, LoadMode mode)
    {
        if (!loader.SupportsMode(mode))
        {
            return;
        }

        ExtendedAttributeLoaders.Add(loader.AppliesTo, loader);
        loader.GameLoader = this;
    }

    private abstract class ExtendedAttributeLoaderBase : IExtendedAttributeLoader
    {
        public abstract string AppliesTo { get; }

        public abstract void Load(XmlReader reader, Element current);

        public required GameLoader GameLoader
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
        public override string AppliesTo => "scriptdictionary";

        public override void Load(XmlReader reader, Element current)
        {
            var currentXmlElementName = reader.Name;
            var result = LoadScriptDictionary(reader, current, currentXmlElementName);
            current.Fields.LazyFields.AddScriptDictionary(currentXmlElementName, result);
        }

        private Dictionary<string, string> LoadScriptDictionary(XmlReader reader, Element? current, string xmlElementName)
        {
            var newDictionary = new Dictionary<string, string>();
            if (reader.IsEmptyElement) return newDictionary;

            var nestCount = 1;
            string? key = null;
            var value = string.Empty;

            while (reader.Read())
            {
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
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
                            throw new InvalidOperationException(
                                $"Invalid element '{reader.Name}' in scriptdictionary block for '{(current == null ? "(nested)" : current.Name)}.{xmlElementName}' - expected only 'item' elements");
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name == "item")
                        {
                            newDictionary.Add(key!, value);
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

                            throw new InvalidOperationException($"Expected closing tag '{xmlElementName}'");
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
            return new LazyScriptDictionary(LoadScriptDictionary(xmlReader, null, "value"));
        }
    }

    private class StringListLoader : ExtendedAttributeLoaderBase, IValueLoader
    {
        public override string AppliesTo => "stringlist";

        public override void Load(XmlReader reader, Element current)
        {
            var xml = XElement.Load(reader.ReadSubtree());
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
        public override string AppliesTo => "list";

        public override void Load(XmlReader reader, Element current)
        {
            var xml = XElement.Load(reader.ReadSubtree());
            var result = LoadQuestList(xml);
            current.Fields.Set(reader.Name, result);
        }

        private QuestList<object> LoadQuestList(XElement xml)
        {
            var result = new QuestList<object>();

            foreach (var xmlValue in xml.Elements("value"))
            {
                var typeAttr = xmlValue.Attribute("type");
                var type = typeAttr?.Value;
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
        protected IDictionary<string, T?> LoadDictionary(XmlReader reader, string xmlElementName)
        {
            var xml = XElement.Load(reader.ReadSubtree());

            return LoadDictionaryFromXElement(xml, $"{xmlElementName}.{reader.Name}");
        }

        protected IDictionary<string, T?> LoadDictionaryFromXElement(XElement xml, string errorSource)
        {
            var result = new Dictionary<string, T?>();

            var items = xml.Elements("item");
            foreach (var item in items)
            {
                var key = item.Element("key");
                var value = item.Element("value");

                if (key == null)
                {
                    GameLoader.AddError($"Missing key in dictionary for '{errorSource}'");
                }
                else if (value == null)
                {
                    GameLoader.AddError($"Missing value in dictionary for '{errorSource}'");
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

        protected abstract void AddResultToDictionary(IDictionary<string, T?> dictionary, string key, XElement xmlValue);
    }

    private class StringDictionaryLoader : DictionaryLoaderBase<string>, IValueLoader
    {
        public override string AppliesTo => "stringdictionary";

        public override void Load(XmlReader reader, Element current)
        {
            var result = LoadDictionary(reader, current.Name);
            current.Fields.Set(reader.Name, new QuestDictionary<string?>(result));
        }

        public object GetValue(XElement xml)
        {
            return new QuestDictionary<string?>(LoadDictionaryFromXElement(xml, "(nested stringdictionary)"));
        }

        protected override void AddResultToDictionary(IDictionary<string, string?> dictionary, string key, XElement xmlValue)
        {
            var value = GameLoader.GetTemplate(xmlValue.Value);
            dictionary.Add(key, value);
        }
    }

    private class ObjectDictionaryLoader : DictionaryLoaderBase<string>, IValueLoader
    {
        public override string AppliesTo => "objectdictionary";

        public override void Load(XmlReader reader, Element current)
        {
            var result = LoadDictionary(reader, current.Name);
            current.Fields.LazyFields.AddObjectDictionary(reader.Name, result);
        }

        public object GetValue(XElement xml)
        {
            return new LazyObjectDictionary(LoadDictionaryFromXElement(xml, "(nested objectdictionary)"));
        }

        protected override void AddResultToDictionary(IDictionary<string, string?> dictionary, string key, XElement xmlValue)
        {
            dictionary.Add(key, xmlValue.Value);
        }
    }

    private class DictionaryLoader : DictionaryLoaderBase<object>, IValueLoader
    {
        public override string AppliesTo => "dictionary";

        public override void Load(XmlReader reader, Element current)
        {
            var result = LoadDictionary(reader, current.Name);
            current.Fields.Set(reader.Name, new QuestDictionary<object?>(result));
        }

        protected override void AddResultToDictionary(IDictionary<string, object?> dictionary, string key, XElement xmlValue)
        {
            var typeAttr = xmlValue.Attribute("type");
            var type = typeAttr?.Value;
            var value = GameLoader.ReadXmlValue(type, xmlValue);
            dictionary.Add(key, value);
        }

        public object GetValue(XElement xml)
        {
            return new QuestDictionary<object?>(LoadDictionaryFromXElement(xml, "(nested dictionary)"));
        }
    }
}