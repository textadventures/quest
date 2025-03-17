using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using QuestViva.Engine.Types;
using QuestViva.Utility;
// ReSharper disable UnusedType.Local

namespace QuestViva.Engine.GameLoader;

internal partial class GameLoader
{
    private delegate void AddErrorHandler(string error);

    private readonly Dictionary<string, IAttributeLoader> _attributeLoaders = new();
    private readonly Dictionary<string, IValueLoader> _valueLoaders = new();

    private void AddLoaders(LoadMode mode)
    {
        foreach (var t in Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                     typeof(IAttributeLoader)))
        {
            AddLoader((IAttributeLoader)Activator.CreateInstance(t)!, mode);
        }

        foreach (var t in Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                     typeof(IValueLoader)))
        {
            AddValueLoader((IValueLoader)Activator.CreateInstance(t)!);
        }
    }

    private void AddLoader(IAttributeLoader loader, LoadMode mode)
    {
        if (!loader.SupportsMode(mode))
        {
            return;
        }

        _attributeLoaders.Add(loader.AppliesTo, loader);
        loader.GameLoader = this;
    }

    private void AddValueLoader(IValueLoader loader)
    {
        _valueLoaders.Add(loader.AppliesTo, loader);
        loader.GameLoader = this;
    }

    private object? ReadXmlValue(string? type, XElement xml)
    {
        type ??= xml.IsEmpty ? "boolean" : "string";

        if (_valueLoaders.TryGetValue(type, out var loader))
        {
            return loader.GetValue(xml);
        }

        AddError($"Unrecognised nested attribute type '{type}'");

        return null;
    }

    private interface IAttributeLoader
    {
        string AppliesTo { get; }
        void Load(Element element, string attribute, string value);
        GameLoader GameLoader { set; }
        bool SupportsMode(LoadMode mode);
    }

    private interface IValueLoader
    {
        string AppliesTo { get; }
        object? GetValue(XElement xml);
        GameLoader GameLoader { set; }
    }

    private abstract class AttributeLoaderBase : IAttributeLoader
    {
        public abstract string AppliesTo { get; }
        public abstract void Load(Element element, string attribute, string value);

        public GameLoader GameLoader { set; protected get; } = null!;

        public virtual bool SupportsMode(LoadMode mode)
        {
            return true;
        }
    }

    private abstract class BasicAttributeLoaderBase : AttributeLoaderBase, IValueLoader
    {
        protected abstract record AttributeLoadResult;
        protected record InvalidAttributeLoadResult : AttributeLoadResult;
        protected record ValidAttributeLoadResult(object Value) : AttributeLoadResult;

        public object? GetValue(XElement xml)
        {
            var value = GetValueFromString(xml.Value, "(nested)");
            if (value is ValidAttributeLoadResult validResult)
            {
                return validResult.Value;
            }

            return null;
        }

        public override void Load(Element element, string attribute, string value)
        {
            var result = GetValueFromString(value, $"{element.Name}.{attribute}");
            if (result is ValidAttributeLoadResult validResult)
            {
                element.Fields.Set(attribute, validResult.Value);
            }
        }

        protected abstract AttributeLoadResult GetValueFromString(string s, string errorSource);
    }

    private class SimpleStringListLoader : AttributeLoaderBase
    {
        public override string AppliesTo => "simplestringlist";

        public override void Load(Element element, string attribute, string value)
        {
            var values = GetValues(value);
            element.Fields.Set(attribute, new QuestList<string>(values));
        }

        protected static string[] GetValues(string value)
        {
            var values = value.Contains('\n')
                ? Utility.SplitIntoLines(value).ToArray()
                : Utility.ListSplit(value);
            return values;
        }
    }

    private class ListExtensionLoader : SimpleStringListLoader
    {
        public override string AppliesTo => "listextend";

        public override void Load(Element element, string attribute, string value)
        {
            var values = GetValues(value);
            element.Fields.AddFieldExtension(attribute, new QuestList<string>(values, true));
        }
    }

    private class ObjectListLoader : AttributeLoaderBase, IValueLoader
    {
        public override string AppliesTo => "objectlist";

        public override void Load(Element element, string attribute, string value)
        {
            var values = GetValues(value);
            element.Fields.LazyFields.AddObjectList(attribute, values);
        }

        private static IEnumerable<string> GetValues(string value)
        {
            var values = value.Contains('\n')
                ? Utility.SplitIntoLines(value).ToArray()
                : Utility.ListSplit(value);
            return values.Where(v => v.Length > 0);
        }

        public object GetValue(XElement xml)
        {
            return new LazyObjectList(GetValues(xml.Value));
        }
    }

    private class ScriptLoader : AttributeLoaderBase, IValueLoader
    {
        public override string AppliesTo => "script";

        public override void Load(Element element, string attribute, string value)
        {
            element.Fields.LazyFields.AddScript(attribute, value);
        }

        public object GetValue(XElement xml)
        {
            return new LazyScript(xml.Value);
        }
    }

    private class StringLoader : AttributeLoaderBase, IValueLoader
    {
        public override string AppliesTo => "string";

        public override void Load(Element element, string attribute, string value)
        {
            element.Fields.Set(attribute, value);
        }

        public object GetValue(XElement xml)
        {
            return xml.Value;
        }
    }

    private class DoubleLoader : BasicAttributeLoaderBase
    {
        public override string AppliesTo => "double";

        protected override AttributeLoadResult GetValueFromString(string s, string errorSource)
        {
            if (double.TryParse(s,
                    System.Globalization.NumberStyles.AllowDecimalPoint |
                    System.Globalization.NumberStyles.AllowLeadingSign,
                    System.Globalization.CultureInfo.InvariantCulture, out var num))
            {
                return new ValidAttributeLoadResult(num);
            }

            GameLoader.AddError($"Invalid number specified '{errorSource} = {s}'");
            return new InvalidAttributeLoadResult();
        }
    }

    private class IntLoader : BasicAttributeLoaderBase
    {
        public override string AppliesTo => "int";

        protected override AttributeLoadResult GetValueFromString(string s, string errorSource)
        {
            if (int.TryParse(s, out var num))
            {
                return new ValidAttributeLoadResult(num);
            }

            GameLoader.AddError($"Invalid number specified '{errorSource} = {s}'");
            return new InvalidAttributeLoadResult();
        }
    }

    private class BooleanLoader : BasicAttributeLoaderBase
    {
        public override string AppliesTo => "boolean";

        protected override AttributeLoadResult GetValueFromString(string s, string errorSource)
        {
            switch (s)
            {
                case "":
                case "true":
                    return new ValidAttributeLoadResult(true);
                case "false":
                    return new ValidAttributeLoadResult(false);
                default:
                    GameLoader.AddError($"Invalid boolean specified '{errorSource} = {s}'");
                    return new InvalidAttributeLoadResult();
            }
        }
    }

    private partial class SimplePatternLoader : AttributeLoaderBase
    {
        // TO DO: It would be nice if we could also specify optional text in square brackets
        // e.g. ask man about[ the] #subject#

        [GeneratedRegex("#([A-Za-z]\\w+)#")]
        private partial Regex m_regex();

        public override string AppliesTo => "simplepattern";

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
            value = m_regex().Replace(value, MatchReplace);

            if (value.Contains('#'))
            {
                GameLoader.AddError($"Invalid command pattern '{element.Name}.{attribute} = {value}'");
            }

            // Now split semicolon separated command patterns
            var patterns = Utility.ListSplit(value);
            var result = string.Empty;
            foreach (var pattern in patterns)
            {
                if (result.Length > 0) result += "|";
                result += "^" + pattern + "$";
            }

            element.Fields.Set(attribute, result);
        }

        private static string MatchReplace(Match m)
        {
            // "#blah#" needs to be converted to "(?<blah>.*)"
            return "(?<" + m.Groups[1].Value + ">.*)";
        }

        private static void LoadVerb(Element element, string attribute, string value)
        {
            element.Fields.Set(attribute, Utility.ConvertVerbSimplePattern(value, element.Fields[FieldDefinitions.Separator]));

            var verbs = value.Split(';');
            element.Fields[FieldDefinitions.DisplayVerb] = verbs[0].Replace("#object#", string.Empty).Trim();
        }

        public override bool SupportsMode(LoadMode mode)
        {
            return mode == LoadMode.Play;
        }
    }

    private class EditorSimplePatternLoader : AttributeLoaderBase
    {
        private readonly SimplePatternLoader _patternLoader = new();

        public override string AppliesTo => "simplepattern";

        public override void Load(Element element, string attribute, string value)
        {
            if (element.ElemType == ElementType.Editor)
            {
                // For Editor elements, use the normal pattern loader to convert this
                // simple pattern to a regex.
                _patternLoader.Load(element, attribute, value);

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
            return mode == LoadMode.Edit;
        }
    }

    private class SimpleStringDictionaryLoader : AttributeLoaderBase
    {
        public override string AppliesTo => "simplestringdictionary";

        public override void Load(Element element, string attribute, string value)
        {
            var result = new QuestDictionary<string>();

            var values = Utility.ListSplit(value);
            foreach (var pair in values)
            {
                if (pair.Length <= 0)
                {
                    continue;
                }

                var trimmedPair = pair.Trim();
                var splitPos = trimmedPair.IndexOf('=');
                if (splitPos == -1)
                {
                    GameLoader.AddError(
                        $"Missing '=' in dictionary element '{trimmedPair}' in '{element.Name}.{attribute}'");
                    return;
                }
                var key = trimmedPair[..splitPos].Trim();
                var dictValue = trimmedPair[(splitPos + 1)..].Trim();
                result.Add(key, dictValue);
            }

            element.Fields.Set(attribute, result);
        }
    }

    private class SimpleObjectDictionaryLoader : AttributeLoaderBase
    {
        public override string AppliesTo => "simpleobjectdictionary";

        public override void Load(Element element, string attribute, string value)
        {
            var result = new Dictionary<string, string>();

            var values = Utility.ListSplit(value);
            foreach (var pair in values)
            {
                if (pair.Length <= 0)
                {
                    continue;
                }

                var trimmedPair = pair.Trim();
                var splitPos = trimmedPair.IndexOf('=');
                if (splitPos == -1)
                {
                    GameLoader.AddError(
                        $"Missing '=' in dictionary element '{trimmedPair}' in '{element.Name}.{attribute}'");
                    return;
                }
                var key = trimmedPair[..splitPos].Trim();
                var dictValue = trimmedPair[(splitPos + 1)..].Trim();
                result.Add(key, dictValue);
            }

            element.Fields.LazyFields.AddObjectDictionary(attribute, result);
        }
    }

    private class ObjectReferenceLoader : AttributeLoaderBase, IValueLoader
    {
        public override string AppliesTo => "object";

        public override void Load(Element element, string attribute, string value)
        {
            element.Fields.LazyFields.AddObjectField(attribute, value);   
        }

        public object GetValue(XElement xml)
        {
            return new LazyObjectReference(xml.Value);
        }
    }
}