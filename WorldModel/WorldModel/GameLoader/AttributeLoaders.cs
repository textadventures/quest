using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    partial class GameLoader
    {
        private delegate void AddErrorHandler(string error);

        private Dictionary<string, IAttributeLoader> m_attributeLoaders = new Dictionary<string, IAttributeLoader>();

        private void AddLoaders(LoadMode mode)
        {
            foreach (Type t in TextAdventures.Utility.Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                typeof(IAttributeLoader)))
            {
                AddLoader((IAttributeLoader)Activator.CreateInstance(t), mode);
            }
        }

        private void AddLoader(IAttributeLoader loader, LoadMode mode)
        {
            if (loader.SupportsMode(mode))
            {
                m_attributeLoaders.Add(loader.AppliesTo, loader);
                loader.GameLoader = this;
            }
        }

        private interface IAttributeLoader
        {
            string AppliesTo { get; }
            void Load(Element element, string attribute, string value);
            GameLoader GameLoader { set; }
            bool SupportsMode(LoadMode mode);
        }

        private abstract class AttributeLoaderBase : IAttributeLoader
        {
            #region IAttributeLoader Members

            public abstract string AppliesTo { get; }
            public abstract void Load(Element element, string attribute, string value);

            public GameLoader GameLoader { set; protected get; }

            public virtual bool SupportsMode(LoadMode mode)
            {
                return true;
            }

            #endregion
        }

        private class ListLoader : AttributeLoaderBase
        {
            public override string AppliesTo
            {
                get { return "list"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                string[] values = GetValues(value);
                element.Fields.Set(attribute, new QuestList<string>(values));
            }

            protected string[] GetValues(string value)
            {
                string[] values;
                if (value.IndexOf("\n") >= 0)
                {
                    values = Utility.SplitIntoLines(value).ToArray();
                }
                else
                {
                    values = Utility.ListSplit(value);
                }
                return values;
            }
        }

        private class ListExtensionLoader : ListLoader
        {
            public override string AppliesTo
            {
                get { return "listextend"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                string[] values = GetValues(value);
                element.Fields.AddFieldExtension(attribute, new QuestList<string>(values, true));
            }
        }

        private class ObjectListLoader : AttributeLoaderBase
        {
            public override string AppliesTo
            {
                get { return "objectlist"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                IEnumerable<string> values = GetValues(value);
                element.Fields.LazyFields.AddObjectList(attribute, values);
            }

            protected IEnumerable<string> GetValues(string value)
            {
                string[] values;
                if (value.IndexOf("\n") >= 0)
                {
                    values = Utility.SplitIntoLines(value).ToArray();
                }
                else
                {
                    values = Utility.ListSplit(value);
                }
                return values.Where(v => v.Length > 0);
            }
        }

        private class ScriptLoader : AttributeLoaderBase
        {
            public override string AppliesTo
            {
                get { return "script"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                element.Fields.LazyFields.AddScript(attribute, value);
            }
        }

        private class StringLoader : AttributeLoaderBase
        {
            public override string AppliesTo
            {
                get { return "string"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                element.Fields.Set(attribute, value);
            }
        }

        private class DoubleLoader : AttributeLoaderBase
        {
            public override string AppliesTo
            {
                get { return "double"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                double num;
                if (double.TryParse(value, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out num))
                {
                    element.Fields.Set(attribute, num);
                }
                else
                {
                    GameLoader.AddError(string.Format("Invalid number specified '{0}.{1} = {2}'", element.Name, attribute, value));
                }
            }
        }

        private class IntLoader : AttributeLoaderBase
        {
            public override string AppliesTo
            {
                get { return "int"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                int num;
                if (int.TryParse(value, out num))
                {
                    element.Fields.Set(attribute, num);
                }
                else
                {
                    GameLoader.AddError(string.Format("Invalid number specified '{0}.{1} = {2}'", element.Name, attribute, value));
                }
            }
        }

        private class BooleanLoader : AttributeLoaderBase
        {
            public override string AppliesTo
            {
                get { return "boolean"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                switch (value)
                {
                    case "":
                    case "true":
                        element.Fields.Set(attribute, true);
                        break;
                    case "false":
                        element.Fields.Set(attribute, false);
                        break;
                    default:
                        GameLoader.AddError(string.Format("Invalid boolean specified '{0}.{1} = {2}'", element.Name, attribute, value));
                        break;
                }
            }
        }

        private class SimplePatternLoader : AttributeLoaderBase
        {
            // TO DO: It would be nice if we could also specify optional text in square brackets
            // e.g. ask man about[ the] #subject#

            private System.Text.RegularExpressions.Regex m_regex = new System.Text.RegularExpressions.Regex(
                "#([A-Za-z]\\w+)#");

            public override string AppliesTo
            {
                get { return "simplepattern"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                if (element.Fields.GetAsType<bool>("isverb"))
                {
                    element.Fields.LazyFields.AddAction(() =>
                    {
                        // use LazyField as we need the separator attribute to exist to create
                        // the correct regex
                        LoadVerb(element, attribute, value);
                    });
                }
                else
                {
                    LoadCommand(element, attribute, value);
                }
            }

            private void LoadCommand(Element element, string attribute, string value)
            {
                value = value.Replace("(", @"\(").Replace(")", @"\)").Replace(".", @"\.").Replace("?", @"\?");
                value = m_regex.Replace(value, MatchReplace);

                if (value.Contains("#"))
                {
                    GameLoader.AddError(string.Format("Invalid command pattern '{0}.{1} = {2}'", element.Name, attribute, value));
                }

                // Now split semi-colon separated command patterns
                string[] patterns = Utility.ListSplit(value);
                string result = string.Empty;
                foreach (string pattern in patterns)
                {
                    if (result.Length > 0) result += "|";
                    result += "^" + pattern + "$";
                }

                element.Fields.Set(attribute, result);
            }

            private string MatchReplace(System.Text.RegularExpressions.Match m)
            {
                // "#blah#" needs to be converted to "(?<blah>.*)"
                return "(?<" + m.Groups[1].Value + ">.*)";
            }

            private void LoadVerb(Element element, string attribute, string value)
            {
                element.Fields.Set(attribute, Utility.ConvertVerbSimplePattern(value, element.Fields[FieldDefinitions.Separator]));
            }

            public override bool SupportsMode(LoadMode mode)
            {
                return (mode == LoadMode.Play);
            }
        }

        private class EditorSimplePatternLoader : AttributeLoaderBase
        {
            private SimplePatternLoader m_patternLoader = new SimplePatternLoader();

            public override string AppliesTo
            {
                get { return "simplepattern"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                if (element.ElemType == ElementType.Editor)
                {
                    // For Editor elements, use the normal pattern loader to convert this
                    // simple pattern to a regex.
                    m_patternLoader.Load(element, attribute, value);

                    if (attribute == FieldDefinitions.Pattern.Property)
                    {
                        // Save the original pattern for convenience so we can generate expression easily
                        element.Fields[FieldDefinitions.OriginalPattern] = value;
                    }
                }
                else
                {
                    // For all other element types, create an EditorCommandPattern so the pattern
                    // can be edited as a simple string
                    element.Fields.Set(attribute, new EditorCommandPattern(value));
                }
            }

            public override bool SupportsMode(LoadMode mode)
            {
                return (mode == LoadMode.Edit);
            }
        }

        private class StringDictionaryLoader : AttributeLoaderBase
        {
            public override string AppliesTo
            {
                get { return "stringdictionary"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                QuestDictionary<string> result = new QuestDictionary<string>();

                string[] values = Utility.ListSplit(value);
                foreach (string pair in values)
                {
                    if (pair.Length > 0)
                    {
                        string trimmedPair = pair.Trim();
                        int splitPos = trimmedPair.IndexOf('=');
                        if (splitPos == -1)
                        {
                            GameLoader.AddError(string.Format("Missing '=' in dictionary element '{0}' in '{1}.{2}'", trimmedPair, element.Name, attribute));
                            return;
                        }
                        string key = trimmedPair.Substring(0, splitPos).Trim();
                        string dictValue = trimmedPair.Substring(splitPos + 1).Trim();
                        result.Add(key, dictValue);
                    }
                }

                element.Fields.Set(attribute, result);
            }
        }

        private class ObjectDictionaryLoader : AttributeLoaderBase
        {
            public override string AppliesTo
            {
                get { return "objectdictionary"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                Dictionary<string, string> result = new Dictionary<string, string>();

                string[] values = Utility.ListSplit(value);
                foreach (string pair in values)
                {
                    if (pair.Length > 0)
                    {
                        string trimmedPair = pair.Trim();
                        int splitPos = trimmedPair.IndexOf('=');
                        if (splitPos == -1)
                        {
                            GameLoader.AddError(string.Format("Missing '=' in dictionary element '{0}' in '{1}.{2}'", trimmedPair, element.Name, attribute));
                            return;
                        }
                        string key = trimmedPair.Substring(0, splitPos).Trim();
                        string dictValue = trimmedPair.Substring(splitPos + 1).Trim();
                        result.Add(key, dictValue);
                    }
                }

                element.Fields.LazyFields.AddObjectDictionary(attribute, result);
            }
        }

        private class ObjectReferenceLoader : AttributeLoaderBase
        {
            public override string AppliesTo
            {
                get { return "object"; }
            }

            public override void Load(Element element, string attribute, string value)
            {
                element.Fields.LazyFields.AddObjectField(attribute, value);   
            }
        }
    }
}
