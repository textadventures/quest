using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using QuestViva.Engine.Scripts;
using QuestViva.Engine.Types;
using QuestViva.Utility;
// ReSharper disable UnusedType.Local

namespace QuestViva.Engine.GameLoader;

internal partial class FieldSaver
{
    private readonly Dictionary<Type, IFieldSaver> _savers = new();
    private readonly GameSaver _saver;

    public FieldSaver(GameSaver saver)
    {
        _saver = saver;

        // Use Reflection to create instances of all IFieldSavers
        foreach (var t in Classes.GetImplementations(System.Reflection.Assembly.GetExecutingAssembly(),
                     typeof(IFieldSaver)))
        {
            AddSaver((IFieldSaver)Activator.CreateInstance(t)!);
        }
    }

    private void AddSaver(IFieldSaver saver)
    {
        var ignore = (saver.MinVersion.HasValue && saver.MinVersion.Value > _saver.Version)
                     || (saver.MaxVersion.HasValue && saver.MaxVersion.Value < _saver.Version);

        if (ignore) return;

        _savers.Add(saver.AppliesTo, saver);
        saver.GameSaver = _saver;
        saver.FieldSaver = this;
    }

    public void Save(GameXmlWriter writer, Element element, string attribute, object? value)
    {
        if (value == null) return;
        if (TryGetSaver(value.GetType(), out var saver))
        {
            try
            {
                saver.Save(writer, element, attribute, value);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to save field '{element.Name}.{attribute}' with value '{value}' - {ex.Message}", ex);
            }
        }
        else
        {
            throw new Exception($"ERROR: No FieldSaver for attribute {attribute}, type: {value.GetType()}");
        }
    }

    private void SaveValue(GameXmlWriter writer, string xmlElementName, object? value)
    {
        if (value == null) return;
        if (TryGetSaver(value.GetType(), out var saver))
        {
            saver.Save(writer, xmlElementName, value);
        }
        else
        {
            throw new Exception($"ERROR: No ValueSaver for XML element {xmlElementName}, type: {value.GetType()}");
        }
    }

    private bool TryGetSaver(Type type, out IFieldSaver saver)
    {
        if (_savers.TryGetValue(type, out saver!)) return true;

        foreach (var s in _savers.Where(s => s.Key.IsAssignableFrom(type)))
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

    private abstract partial class FieldSaverBase : IFieldSaver
    {
        public abstract Type AppliesTo { get; }
        public abstract void Save(GameXmlWriter writer, Element? element, string attribute, object value);
        
        [GeneratedRegex("^[A-Za-z0-9]*$")]
        private static partial Regex OnlyLettersAndNumbers();

        protected void WriteAttribute(GameXmlWriter writer, Element? element, string attribute, string type, string value)
        {
            if (!OnlyLettersAndNumbers().IsMatch(attribute))
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

        public required GameSaver GameSaver { get; set; }
        public required FieldSaver FieldSaver { get; set; }

        public virtual WorldModelVersion? MinVersion => null;
        public virtual WorldModelVersion? MaxVersion => null;
    }

    private class StringSaver : FieldSaverBase
    {
        public override Type AppliesTo => typeof(string);

        public override void Save(GameXmlWriter writer, Element? element, string attribute, object value)
        {
            var strValue = (string)value;
            WriteAttribute(writer, element, attribute, "string", strValue);
        }
    }

    private class BooleanSaver : FieldSaverBase
    {
        public override Type AppliesTo => typeof(bool);

        public override void Save(GameXmlWriter writer, Element? element, string attribute, object value)
        {
            var boolVal = (bool)value;
            if (boolVal)
            {
                if (attribute.Contains(' '))
                {
                    WriteAttribute(writer, element, attribute, "boolean", "true");
                }
                else
                {
                    writer.WriteElementString(attribute, null);
                }
            }
            else
            {
                WriteAttribute(writer, element, attribute, "boolean", "false");
            }
        }
    }

    private class LegacyStringListSaver : FieldSaverBase
    {
        public override Type AppliesTo => typeof(QuestList<string>);

        public override WorldModelVersion? MaxVersion => WorldModelVersion.v530;

        public override void Save(GameXmlWriter writer, Element? element, string attribute, object value)
        {
            var list = (QuestList<string>)value;
            var saveString = string.Join("; ", list.ToArray());
            WriteAttribute(writer, element, attribute, "list", saveString);
        }
    }

    private class StringListSaver : FieldSaverBase
    {
        public override Type AppliesTo => typeof(QuestList<string>);

        public override WorldModelVersion? MinVersion => WorldModelVersion.v540;

        public override void Save(GameXmlWriter writer, Element? element, string attribute, object value)
        {
            writer.WriteStartElement(attribute);
            if (element == null || !GameSaver.IsImpliedType(element, attribute, "stringlist"))
            {
                writer.WriteAttributeString("type", "stringlist");
            }

            var list = (QuestList<string>)value;

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
        public override Type AppliesTo => typeof (QuestList<object>);

        public override void Save(GameXmlWriter writer, Element? element, string attribute, object value)
        {
            writer.WriteStartElement(attribute);
            if (element == null || !GameSaver.IsImpliedType(element, attribute, "list"))
            {
                writer.WriteAttributeString("type", "list");
            }

            var list = (QuestList<object>)value;

            foreach (var item in list)
            {
                FieldSaver.SaveValue(writer, "value", item);
            }
            writer.WriteEndElement();
        }
    }

    private class LegacyStringDictionarySaver : FieldSaverBase
    {
        public override Type AppliesTo => typeof(QuestDictionary<string>);

        public override void Save(GameXmlWriter writer, Element? element, string attribute, object value)
        {
            var dictionary = (QuestDictionary<string>)value;
            WriteAttribute(writer, element, attribute, "stringdictionary", dictionary.SaveString());
        }

        public override WorldModelVersion? MaxVersion => WorldModelVersion.v530;
    }

    private class ObjectListSaver : FieldSaverBase
    {
        public override Type AppliesTo => typeof(QuestList<Element>);

        public override void Save(GameXmlWriter writer, Element? element, string attribute, object value)
        {
            var list = (QuestList<Element>)value;
            var saveString = string.Join("; ", list.Select(e => e.Name));
            WriteAttribute(writer, element, attribute, "objectlist", saveString);
        }
    }

    private class LegacyObjectDictionarySaver : FieldSaverBase
    {
        public override Type AppliesTo => typeof(QuestDictionary<Element>);

        public override void Save(GameXmlWriter writer, Element? element, string attribute, object value)
        {
            var dictionary = (QuestDictionary<Element>)value;
            WriteAttribute(writer, element, attribute, "objectdictionary", dictionary.SaveString(o => o.Name));
        }

        public override WorldModelVersion? MaxVersion
        {
            get { return WorldModelVersion.v530; }
        }
    }

    private abstract class DictionarySaverBase<T> : FieldSaverBase
    {
        public override WorldModelVersion? MinVersion => WorldModelVersion.v540;

        protected abstract string TypeName { get; }

        protected abstract string GetValueString(T value);

        public override void Save(GameXmlWriter writer, Element? element, string attribute, object value)
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
        public override Type AppliesTo => typeof(QuestDictionary<string>);

        protected override string TypeName => "stringdictionary";

        protected override string GetValueString(string value)
        {
            return value;
        }
    }

    private class ObjectDictionarySaver : DictionarySaverBase<Element>
    {
        public override Type AppliesTo => typeof(QuestDictionary<Element>);

        protected override string TypeName => "objectdictionary";

        protected override string GetValueString(Element value)
        {
            return value.Name;
        }
    }

    private class DictionarySaver : DictionarySaverBase<object>
    {
        public override Type AppliesTo => typeof(QuestDictionary<object>);

        protected override string TypeName => "dictionary";

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
        public override Type AppliesTo => typeof(QuestDictionary<object>);

        public override void Save(GameXmlWriter writer, Element? element, string attribute, object value)
        {
            // Do nothing - objectdictionaries are not saved for ASL 530 and earlier.
        }

        public override WorldModelVersion? MaxVersion => WorldModelVersion.v530;
    }

    private class ScriptSaver : FieldSaverBase
    {
        public override Type AppliesTo => typeof(IScript);

        public override void Save(GameXmlWriter writer, Element? element, string attribute, object value)
        {
            var script = (IScript)value;
            var savedScript = GameSaver.SaveScript(writer, script, 1);
            WriteAttribute(writer, element, attribute, "script", savedScript);
        }
    }

    private class IntSaver : FieldSaverBase
    {
        public override Type AppliesTo => typeof(int);

        public override void Save(GameXmlWriter writer, Element? element, string attribute, object value)
        {
            var number = (int)value;
            WriteAttribute(writer, element, attribute, "int", number.ToString());
        }
    }

    private class DoubleSaver : FieldSaverBase
    {
        public override Type AppliesTo => typeof(double);

        public override void Save(GameXmlWriter writer, Element? element, string attribute, object value)
        {
            var number = (double)value;
            WriteAttribute(writer, element, attribute, "double", number.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
    }

    private class DelegateImplementationSaver : FieldSaverBase
    {
        public override Type AppliesTo => typeof(DelegateImplementation);

        public override void Save(GameXmlWriter writer, Element? element, string attribute, object value)
        {
            var impl = (DelegateImplementation)value;
            WriteAttribute(writer, element, attribute, impl.Definition.Name, GameSaver.SaveScript(writer, impl.Implementation.Fields[FieldDefinitions.Script], 1));
        }
    }

    private class SimplePatternSaver : FieldSaverBase
    {
        public override Type AppliesTo => typeof(EditorCommandPattern);

        public override void Save(GameXmlWriter writer, Element? element, string attribute, object value)
        {
            var pattern = (EditorCommandPattern)value;
            WriteAttribute(writer, element, attribute, "simplepattern", pattern.Pattern);
        }
    }

    // TO DO: Would be good for ScriptDictionary to use the same <item><key>...</key><value>...</value></item>
    // format as the other dictionary types, then this can simply derive from DictionarySaverBase
    private class ScriptDictionarySaver : IFieldSaver
    {
        public Type AppliesTo => typeof(QuestDictionary<IScript>);

        public void Save(GameXmlWriter writer, Element? element, string attribute, object value)
        {
            writer.WriteStartElement(attribute);
            if (element == null || !GameSaver.IsImpliedType(element, attribute, "scriptdictionary"))
            {
                writer.WriteAttributeString("type", "scriptdictionary");
            }

            var dictionary = (QuestDictionary<IScript>)value;

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

        public required GameSaver GameSaver { get; set; }
        public required FieldSaver FieldSaver { get; set; }

        public WorldModelVersion? MinVersion => null;
        public WorldModelVersion? MaxVersion => null;
    }

    private class ObjectReferenceSaver : FieldSaverBase
    {
        public override Type AppliesTo => typeof(Element);

        public override void Save(GameXmlWriter writer, Element? element, string attribute, object value)
        {
            WriteAttribute(writer, element, attribute, "object", ((Element)value).Name);
        }
    }
}