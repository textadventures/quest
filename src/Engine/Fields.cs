using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using QuestViva.Common;
using QuestViva.Engine.Functions;
using QuestViva.Engine.Scripts;
using QuestViva.Engine.Types;

namespace QuestViva.Engine;

public class AttributeChangedEventArgs : EventArgs
{
    internal AttributeChangedEventArgs(string property, object? value, object? oldValue)
    {
        Property = property;
        Value = value;
        OldValue = oldValue;
    }

    internal AttributeChangedEventArgs(bool inheritedTypesSet)
    {
        InheritedTypesSet = inheritedTypesSet;
    }

    // Property is only null when InheritedTypesSet is true (the element's inherited types changed)
    public string? Property { get; private set; }
    public object? Value { get; private set; }
    public object? OldValue { get; private set; }
    public bool InheritedTypesSet { get; private set; }
}

public class NameChangedEventArgs : EventArgs
{
    public required string OldName { get; set; }
    public required Element Element { get; set; }
}

public interface IMutableField
{
    UndoLogger? UndoLog { get; set; }

    Element? Owner { get; set; }

    /// <summary>
    ///     True if we're in an inherited type, so we must be unmodifable. Implementors must check this and throw an exception
    ///     if an attempt is made to change a locked object
    /// </summary>
    bool Locked { get; set; }

    // QuestLists etc. require cloning as you want to be able to assign a.list = b.list and modify
    // the two lists separately. Scripts don't require cloning as they cannot be modified during the
    // game, we just want the Undo behaviour to work in the editor.

    bool RequiresCloning { get; }
    IMutableField Clone();
}

public interface IExtendableField
{
    bool Extended { get; }
    IExtendableField Merge(IExtendableField parent);
}

public interface IField<T>
{
    string Property { get; }
}

internal class FieldDef<T> : IField<T>
{
    public FieldDef(string property)
    {
        Property = property;
    }

    public string Property { get; }
}

public static class FieldDefinitions
{
    public static IField<string> Alias = new FieldDef<string>("alias");
    public static IField<QuestList<string>> DisplayVerbs = new FieldDef<QuestList<string>>("displayverbs");
    public static IField<QuestList<string>> InventoryVerbs = new FieldDef<QuestList<string>>("inventoryverbs");
    public static IField<Element> To = new FieldDef<Element>("to");
    public static IField<bool> LookOnly = new FieldDef<bool>("lookonly");
    public static IField<QuestList<string>> ParamNames = new FieldDef<QuestList<string>>("paramnames");
    public static IField<IScript> Script = new FieldDef<IScript>("script");
    public static IField<string> ReturnType = new FieldDef<string>("returntype");
    // Editor-only organisational folder name for a user-authored Function — purely a display
    // grouping in the editor tree/list, has no effect on the game itself. See ElementsList.svelte
    // and TreePanel.svelte's folder grouping.
    public static IField<string> EditorFolder = new FieldDef<string>("editorfolder");
    public static IField<string> Pattern = new FieldDef<string>("pattern");
    public static IField<string> Unresolved = new FieldDef<string>("unresolved");
    public static IField<string> Property = new FieldDef<string>("property");
    public static IField<string> DefaultTemplate = new FieldDef<string>("defaulttemplate");
    public static IField<bool> IsVerb = new FieldDef<bool>("isverb");
    public static IField<string> DefaultText = new FieldDef<string>("defaulttext");
    public static IField<string> GameName = new FieldDef<string>("gamename");
    public static IField<string> Text = new FieldDef<string>("text");
    public static IField<IFunction<string>> Function = new FieldDef<IFunction<string>>("text");
    public static IField<string> Filename = new FieldDef<string>("filename");
    public static IField<QuestList<string>> Steps = new FieldDef<QuestList<string>>("steps");
    public static IField<string> Element = new FieldDef<string>("element");
    public static IField<string> Type = new FieldDef<string>("type");
    public static IField<string> Src = new FieldDef<string>("src");
    public static IField<bool> Anonymous = new FieldDef<bool>("anonymous");
    public static IField<string> TemplateName = new FieldDef<string>("templatename");
    public static IField<string> OriginalPattern = new FieldDef<string>("originalpattern");
    public static IField<int> TimeElapsed = new FieldDef<int>("timeelapsed");
    public static IField<int> Trigger = new FieldDef<int>("trigger");
    public static IField<int> Interval = new FieldDef<int>("interval");
    public static IField<bool> Enabled = new FieldDef<bool>("enabled");
    public static IField<string> Separator = new FieldDef<string>("separator");
    public static IField<string> GameID = new FieldDef<string>("gameid");
    public static IField<string> Category = new FieldDef<string>("category");
    public static IField<string> Description = new FieldDef<string>("description");
    public static IField<string> DefaultWebFont = new FieldDef<string>("defaultwebfont");
    public static IField<bool> IsBaseTemplate = new FieldDef<bool>("isbasetemplate");
    public static IField<string> DisplayVerb = new FieldDef<string>("displayverb");
    public static IField<string> Cover = new FieldDef<string>("cover");
    public static IField<string> EditorStyle = new FieldDef<string>("_editorstyle");
}

public static class MetaFieldDefinitions
{
    public static IField<string> Filename = new FieldDef<string>("filename");
    public static IField<bool> Library = new FieldDef<bool>("library");
    public static IField<bool> EditorLibrary = new FieldDef<bool>("editorlibrary");
    public static IField<bool> DelegateImplementation = new FieldDef<bool>("delegateimplementation");
    public static IField<int> SortIndex = new FieldDef<int>("sortindex");
}

public class Fields
{
    private static readonly Dictionary<Type, DebugFormatDelegate> Formatters = new()
    {
        {typeof(List<string>), ListFormatter}
    };

    // Debugger attribute-override pre-fill (see WasmPlayerBridge's debugger):
    // unlike Formatters above (a human-readable *display* string — an
    // Element shows as "Object: kitchen", a string has no quotes, a bool
    // reads "True"), this produces a value already written as valid script
    // syntax, ready to feed straight into ScriptFactory.CreateScript for
    // "element.attribute = <this>". Types with no entry here (lists, dicts,
    // scripts, ...) fall back to the same display string, which isn't
    // generally re-enterable — there's no simple literal syntax for those,
    // so the debugger's override hint still covers that residual case.
    private static readonly Dictionary<Type, DebugFormatDelegate> EditFormatters = new()
    {
        {typeof(bool), v => (bool) v! ? "true" : "false"},
        {typeof(string), v => "\"" + ((string) v!).Replace("\\", "\\\\").Replace("\"", "\\\"") + "\""},
        {typeof(Element), v => ((Element) v!).Name}
    };

    // Which value kinds the debugger's override control should even offer —
    // the types above (their ToString() already needs the EditFormatters
    // rewrite to become valid script syntax) plus plain numeric types (whose
    // ToString() already *is* valid script syntax, no rewrite needed). Lists,
    // dictionaries, scripts etc. have no simple literal syntax to write back,
    // so there's deliberately no entry for them here — an override textbox
    // for those would just be a dead end.
    private static readonly HashSet<Type> OverridableValueTypes =
    [
        typeof(bool), typeof(string), typeof(Element),
        typeof(int), typeof(double), typeof(long), typeof(float), typeof(decimal), typeof(short)
    ];

    private static bool CanOverrideValue(object? value)
    {
        return value == null || OverridableValueTypes.Contains(value.GetType());
    }

    private readonly Dictionary<string, object?> _attributes = [];
    private readonly Element _element;
    private readonly Dictionary<string, IExtendableField> _extendableFields = [];
    private readonly bool _isMeta;
    private readonly WorldModel _worldModel;
    private Stack<Element> _types = new();

    public Fields(WorldModel worldModel, Element element, bool isMeta)
    {
        _worldModel = worldModel;
        _element = element;
        LazyFields = new LazyFields(worldModel, this);
        _isMeta = isMeta;
    }

    internal bool MutableFieldsLocked { get; set; }

    internal LazyFields LazyFields { get; }

    internal IEnumerable<string> FieldNames => _attributes.Keys;

    internal IEnumerable<string> FieldExtensionNames
    {
        get
        {
            var result = new List<string>(_extendableFields.Keys);

            foreach (var type in _types)
            {
                result.AddRange(type.Fields.FieldExtensionNames);
            }

            return result;
        }
    }

    internal IEnumerable<Element> Types => _types;
    public event EventHandler<AttributeChangedEventArgs>? AttributeChanged;
    public event EventHandler<AttributeChangedEventArgs>? AttributeChangedSilent;
    internal event EventHandler<NameChangedEventArgs>? NameChanged;

    internal Fields Clone(Element newElement)
    {
        var clone = new Fields(_worldModel, newElement, _isMeta);

        foreach (var type in _types.Reverse())
        {
            clone._types.Push(type);
        }

        foreach (var attribute in _attributes)
        {
            if (attribute.Key != "name")
            {
                clone.Set(attribute.Key, attribute.Value, false, true);
            }
        }

        return clone;
    }

    private DebugFormatDelegate GetFormatter(Type? type)
    {
        if (type == null)
        {
            return DefaultFormatter;
        }

        if (Formatters.ContainsKey(type))
        {
            return Formatters[type];
        }

        return DefaultFormatter;
    }

    private string DefaultFormatter(object? input)
    {
        if (input == null)
        {
            return "(null)";
        }

        return input.ToString() ?? string.Empty;
    }

    private static string ListFormatter(object? input)
    {
        var list = (List<string>) input!;
        var output = string.Empty;
        if (list.Count == 0)
        {
            return output;
        }

        foreach (var item in list)
        {
            output += item + ", ";
        }

        return output.Substring(0, output.Length - 2);
    }

    private void UndoLog(string property, object? oldValue, object? newValue, bool added)
    {
        if (_worldModel != null)
        {
            _worldModel.UndoLogger.AddUndoAction(() =>
                new UndoFieldSet(_worldModel, _element.Name, property, oldValue, newValue, added, _isMeta));
        }
    }

    internal void UndoLog(UndoLogger.IUndoAction action)
    {
        if (_worldModel != null)
        {
            _worldModel.UndoLogger.AddUndoAction(() => action);
        }
    }

    public void Set(string name, object? value)
    {
        Set(name, value, true, true);
    }

    internal void SetFromUndo(string name, object? value)
    {
        Set(name, value, false, false, false);
    }

    internal void RemoveFieldInternal(string name)
    {
        var oldValue = Get(name);
        _attributes.Remove(name);

        if (AttributeChangedSilent != null && name != "name")
        {
            AttributeChangedSilent(this, new AttributeChangedEventArgs(name, Get(name), oldValue));
        }
    }

    public void RemoveField(string name)
    {
        UndoLog(new UndoFieldRemove(_element.Name, name, _attributes[name]));
        RemoveFieldInternal(name);
    }

    public void AddFieldExtension(string name, IExtendableField value)
    {
        if (value.Extended)
        {
            // extendable fields should only ever exist in types, and be set at the start of the game,
            // so we should never need to worry about adding them to the undo log etc.
            _extendableFields[name] = value;
        }
        else
        {
            Set(name, value);
        }
    }

    private void Set(string name, object? value, bool raiseEvent, bool cloneClonableValues,
        bool allowUpdateSortOrder = true)
    {
        bool changed;
        var added = true;
        object? oldValue = null;

        if (_attributes.ContainsKey(name))
        {
            added = false;
            oldValue = Get(name);
        }
        else
        {
            if (!Utility.IsValidFieldName(name))
            {
                throw new Exception(string.Format("Invalid attribute name '{0}'", name));
            }
        }

        if (value == null)
        {
            changed = oldValue != null;
        }
        else
        {
            changed = !value.Equals(oldValue);
        }

        if (changed && cloneClonableValues)
        {
            if (oldValue is IMutableField mutableOldValue)
            {
                if (_worldModel.EditMode || mutableOldValue.RequiresCloning)
                {
                    oldValue = mutableOldValue.Clone();
                }
            }

            if (value is IMutableField mutableNewValue)
            {
                if (_worldModel.EditMode || mutableNewValue.RequiresCloning)
                {
                    mutableNewValue = mutableNewValue.Clone();
                    value = mutableNewValue;
                }

                mutableNewValue.Locked = MutableFieldsLocked;
                mutableNewValue.UndoLog = _worldModel.UndoLogger;
                mutableNewValue.Owner = _element;
            }
        }

        switch (name)
        {
            case "name":
            {
                if (value is not string strValue)
                {
                    throw new ArgumentException("Invalid data type for 'name'");
                }

                _element.SetNameFromFields(strValue);
                break;
            }
            case "parent":
                _element.SetParentFromFields(value as Element);
                break;
            case "text":
                _element.SetTextFromFields(value as string);
                break;
        }

        if (_worldModel.Version >= WorldModelVersion.v530 && value == null)
        {
            _attributes.Remove(name);
        }
        else
        {
            _attributes[name] = value;
        }

        if (changed && allowUpdateSortOrder && name == "parent" && _element.Initialised)
        {
            _worldModel.UpdateElementSortOrder(_element);
        }

        if (name == "name" && changed && value != null && !added && NameChanged != null)
        {
            if (!_worldModel.EditMode)
            {
                // Actually we could allow this I suppose but I don't think it's sensible.
                throw new InvalidOperationException("Cannot change name of element when not in Edit mode");
            }

            NameChanged(this, new NameChangedEventArgs
            {
                OldName = (string) oldValue!,
                Element = _element
            });
        }

        if (raiseEvent)
        {
            if (changed)
            {
                UndoLog(name, oldValue, value, added);
            }

            if (changed && AttributeChanged != null)
            {
                AttributeChanged(this, new AttributeChangedEventArgs(name, value, oldValue));
            }
        }
        else
        {
            // when undoing, the editor still needs a notification of a changed field
            if (changed && AttributeChangedSilent != null)
            {
                AttributeChangedSilent(this, new AttributeChangedEventArgs(name, value, oldValue));
            }
        }
    }

    public object? Get(string attribute)
    {
        return Get(attribute, false).Value;
    }

    private AttributeData Get(string attribute, bool withSource)
    {
        var result = new AttributeData();

        if (_attributes.TryGetValue(attribute, out result.Value))
        {
            if (withSource)
            {
                result.Source = _element.Name;
                result.IsInherited = false;
            }

            return result;
        }

        foreach (var type in _types)
        {
            if (type.Fields.Exists(attribute, false))
            {
                result.Value = type.Fields.Get(attribute);
                if (withSource)
                {
                    result.Source = type.Name;
                    result.IsInherited = true;
                }

                break;
            }
        }

        // if for example we have a "listextend" field in the type hierarchy, we need to merge
        // that field with the base field

        if (HasExtendableField(attribute))
        {
            return GetMergedResult(attribute, result);
        }

        return result;
    }

    private AttributeData GetMergedResult(string attribute, AttributeData baseField)
    {
        var source = new List<string>();
        var mergedResult = GetExtendableField(attribute, source);

        if (baseField.Value is not IExtendableField extendableBaseField)
        {
            return new AttributeData
            {
                Value = mergedResult,
                Source = string.Join(",", source)
            };
        }

        if (mergedResult == null)
        {
            return baseField;
        }

        return new AttributeData
        {
            Value = mergedResult.Merge(extendableBaseField),
            Source = baseField.Source + "," + string.Join(",", source),
            IsInherited = true
        };
    }

    private bool HasExtendableField(string attribute)
    {
        if (_extendableFields.ContainsKey(attribute))
        {
            return true;
        }

        foreach (var type in _types)
        {
            if (type.Fields.HasExtendableField(attribute))
            {
                return true;
            }
        }

        return false;
    }

    private IExtendableField? GetExtendableField(string attribute, List<string> source)
    {
        IExtendableField? result = null;

        if (_extendableFields.ContainsKey(attribute))
        {
            result = MergeExtendableFields(result, _extendableFields[attribute]);
            source.Add(_element.Name);
        }

        foreach (var type in _types)
        {
            if (type.Fields.HasExtendableField(attribute))
            {
                result = MergeExtendableFields(result, type.Fields.GetExtendableField(attribute, source)!);
                // Don't need to add to source here as the call to GetExtendableField will do that automatically
            }
        }

        return result;
    }

    private IExtendableField MergeExtendableFields(IExtendableField? field, IExtendableField parent)
    {
        if (field == null)
        {
            return parent;
        }

        return field.Merge(parent);
    }

    internal bool Exists(string attribute, bool includeExtendableFields)
    {
        if (_attributes.ContainsKey(attribute))
        {
            return true;
        }

        foreach (var type in _types)
        {
            if (type.Fields.Exists(attribute, includeExtendableFields))
            {
                return true;
            }
        }

        if (includeExtendableFields && HasExtendableField(attribute))
        {
            return true;
        }

        return false;
    }

    public void AddType(Element addType)
    {
        if (_element.ElemType == ElementType.ObjectType &&
            (addType == _element || addType.Fields.InheritsTypeRecursive(_element)))
        {
            throw new Exception("Circular type reference");
        }

        _types.Push(addType);
    }

    public void AddTypeUndoable(Element addType)
    {
        var oldValue = CloneStack(_types);
        AddType(addType);
        var newValue = CloneStack(_types);
        _worldModel.UndoLogger.AddUndoAction(() => new UndoAddRemoveType(_element.Name, oldValue, newValue));
        AttributeChangedSilent?.Invoke(this, new AttributeChangedEventArgs(true));
    }

    public void RemoveTypeUndoable(Element removeType)
    {
        var oldValue = CloneStack(_types);
        _types = CloneStackAndDelete(_types, removeType);
        var newValue = CloneStack(_types);
        _worldModel.UndoLogger.AddUndoAction(() => new UndoAddRemoveType(_element.Name, oldValue, newValue));
        AttributeChangedSilent?.Invoke(this, new AttributeChangedEventArgs(true));
    }

    private string FormatDebugData(object? value)
    {
        return GetFormatter(value == null ? null : value.GetType()).Invoke(value);
    }

    private string FormatEditValue(object? value)
    {
        if (value == null)
        {
            return "null";
        }

        return EditFormatters.TryGetValue(value.GetType(), out var formatter)
            ? formatter.Invoke(value)
            : FormatDebugData(value);
    }

    internal DebugData GetDebugData()
    {
        var result = new DebugData();

        foreach (var attribute in GetAttributeNames(true))
        {
            var attributeData = Get(attribute, true);
            var debugDataItem = new DebugDataItem(FormatDebugData(attributeData.Value));
            debugDataItem.EditValue = FormatEditValue(attributeData.Value);
            debugDataItem.CanOverride = CanOverrideValue(attributeData.Value);
            debugDataItem.Source = attributeData.Source;
            debugDataItem.IsInherited = attributeData.IsInherited;

            result.Data.Add(attribute, debugDataItem);
        }

        return result;
    }

    internal DebugData GetInheritedTypesDebugData()
    {
        var result = new DebugData();

        // First we want all the types we include directly
        foreach (var type in _types)
        {
            if (!result.Data.ContainsKey(type.Name))
            {
                var newItem = new DebugDataItem(type.Name);
                newItem.Source = _element.Name;
                newItem.IsDefaultType = WorldModel.DefaultTypeNames.ContainsValue(type.Name);
                result.Data.Add(type.Name, newItem);
            }
            else
            {
                // This element directly inherits a type which has also been included
                // via inheriting another type

                result.Data[type.Name].Source += "," + _element.Name;
            }

            // Next we want all the types that are inherited from those, as attributes
            // from these will take priority over any other inherited types

            var inheritedData = type.Fields.GetInheritedTypesDebugData();

            foreach (var typeName in inheritedData.Data.Keys)
            {
                if (!result.Data.ContainsKey(typeName))
                {
                    var item = inheritedData.Data[typeName];
                    item.IsInherited = true;
                    result.Data.Add(typeName, item);
                }
            }
        }

        return result;
    }

    public T? GetAsType<T>(string attribute)
    {
        var value = Get(attribute);
        if (value is T)
        {
            return (T) value;
        }

        return default;
    }

    public bool HasType<T>(string attribute)
    {
        var value = Get(attribute);
        return value is T;
    }

    public string? GetString(string attribute)
    {
        return GetAsType<string>(attribute);
    }

    public bool HasString(string attribute)
    {
        return HasType<string>(attribute);
    }

    internal void DoUndoAddRemoveType(Stack<Element> newValue)
    {
        _types = newValue;
        AttributeChangedSilent?.Invoke(this, new AttributeChangedEventArgs(true));
    }

    private Stack<Element> CloneStack(Stack<Element> input)
    {
        return CloneStackAndDelete(input, null);
    }

    private Stack<Element> CloneStackAndDelete(Stack<Element> input, Element? elementToDelete)
    {
        var result = new Stack<Element>();
        foreach (var item in input.Reverse())
        {
            if (item != elementToDelete)
            {
                result.Push(item);
            }
        }

        return result;
    }

    public void Resolve(ScriptFactory factory)
    {
        LazyFields.Resolve(factory);
    }

    public bool InheritsType(Element type)
    {
        return _types.Contains(type);
    }

    public bool InheritsTypeRecursive(Element type)
    {
        if (_types.Contains(type))
        {
            return true;
        }

        foreach (var inheritedType in _types)
        {
            if (inheritedType.Fields.InheritsTypeRecursive(type))
            {
                return true;
            }
        }

        return false;
    }

    internal void RemoveReferencesTo(Element e)
    {
        var nullifyAttributes = new List<string>();

        foreach (var attribute in _attributes)
        {
            if (attribute.Value is Element elementValue)
            {
                if (elementValue == e)
                {
                    nullifyAttributes.Add(attribute.Key);
                }

                continue;
            }

            if (attribute.Value is QuestList<Element> listValue)
            {
                while (listValue.Contains(e))
                {
                    listValue.Remove(e);
                }

                continue;
            }

            if (attribute.Value is QuestDictionary<Element> dictionaryValue)
            {
                var keysToRemove = new List<string>();
                foreach (var item in dictionaryValue)
                {
                    if (item.Value == e)
                    {
                        keysToRemove.Add(item.Key);
                    }
                }

                foreach (var key in keysToRemove)
                {
                    dictionaryValue.Remove(key);
                }
            }
        }

        foreach (var attribute in nullifyAttributes)
        {
            Set(attribute, null);
        }
    }

    public IEnumerable<string> GetAttributeNames(bool includeInheritedAttributes)
    {
        var result = new List<string>(_attributes.Select(a => a.Key));
        if (includeInheritedAttributes)
        {
            foreach (var type in _types)
            {
                result.AddRange(type.Fields.GetAttributeNames(true).Where(a => !result.Contains(a)));
            }
        }

        return result;
    }

    private delegate string DebugFormatDelegate(object? input);

    private struct AttributeData
    {
        public object? Value;
        public string? Source;
        public bool IsInherited;
    }

    #region Indexed Properties

    public string? this[IField<string> field]
    {
        get => GetAsType<string>(field.Property);
        set => Set(field.Property, value);
    }

    public QuestList<string>? this[IField<QuestList<string>> field]
    {
        get => GetAsType<QuestList<string>>(field.Property);
        set => Set(field.Property, value);
    }

    public IScript? this[IField<IScript> field]
    {
        get => GetAsType<IScript>(field.Property);
        set => Set(field.Property, value);
    }

    public Element? this[IField<Element> field]
    {
        get => GetAsType<Element>(field.Property);
        set => Set(field.Property, value);
    }

    public bool this[IField<bool> field]
    {
        get => GetAsType<bool>(field.Property);
        set => Set(field.Property, value);
    }

    public int this[IField<int> field]
    {
        get => GetAsType<int>(field.Property);
        set => Set(field.Property, value);
    }

    public IFunction<string>? this[IField<IFunction<string>> field]
    {
        get => GetAsType<IFunction<string>>(field.Property);
        set => Set(field.Property, value);
    }

    #endregion
}

public class LazyFields
{
    private readonly Fields _fields;
    private readonly WorldModel _worldModel;
    private List<Action> _actions = [];
    private List<string> _defaultTypes = [];
    private Dictionary<string, IDictionary<string, string?>> _objectDictionaries = [];
    private Dictionary<string, string> _objectFields = [];
    private Dictionary<string, IEnumerable<string>> _objectLists = [];
    private bool _resolved;
    private Dictionary<string, IDictionary<string, string>> _scriptDictionaries = [];
    private Dictionary<string, string> _scripts = [];
    private List<string> _types = [];

    internal LazyFields(WorldModel worldModel, Fields fields)
    {
        _worldModel = worldModel;
        _fields = fields;
    }

    public void Resolve(ScriptFactory scriptFactory)
    {
        CheckNotResolved();
        foreach (var typename in _defaultTypes)
        {
            // It is legitimate for a default type not to exist
            if (_worldModel.Elements.ContainsKey(ElementType.ObjectType, typename))
            {
                _fields.AddType(_worldModel.GetObjectType(typename));
            }
        }

        // Each pending collection is released once resolved - CheckNotResolved stops any later use
        _defaultTypes = null!;
        foreach (var typename in _types)
        {
            try
            {
                _fields.AddType(_worldModel.GetObjectType(typename));
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Error adding type '{0}' to element '{1}': {2}", typename, _fields.Get("name"),
                        ex.Message), ex);
            }
        }

        _types = null!;
        foreach (var property in _objectFields.Keys)
        {
            try
            {
                _fields.Set(property, _worldModel.Elements.Get(_objectFields[property]));
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Error adding attribute '{0}' to element '{1}': {2}", property, _fields.Get("name"),
                        ex.Message), ex);
            }
        }

        _objectFields = null!;
        foreach (var property in _scripts.Keys)
        {
            try
            {
                _fields.Set(property, scriptFactory.CreateScript(_scripts[property]));
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Error adding script attribute '{0}' to element '{1}': {2}", property,
                        _fields.Get("name"), ex.Message), ex);
            }
        }

        _scripts = null!;
        foreach (var property in _scriptDictionaries.Keys)
        {
            try
            {
                _fields.Set(property, ConvertToScriptDictionary(_scriptDictionaries[property], scriptFactory));
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Error adding script dictionary '{0}' to element '{1}': {2}", property,
                        _fields.Get("name"), ex.Message), ex);
            }
        }

        _scriptDictionaries = null!;
        foreach (var property in _objectLists.Keys)
        {
            try
            {
                _fields.Set(property,
                    new QuestList<Element>(_objectLists[property].Select(n => _worldModel.Elements.Get(n))));
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Error adding object list '{0}' to element '{1}': {2}", property,
                        _fields.Get("name"), ex.Message), ex);
            }
        }

        _objectLists = null!;
        foreach (var property in _objectDictionaries.Keys)
        {
            try
            {
                _fields.Set(property, ConvertToObjectDictionary(_objectDictionaries[property]));
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format("Error adding object dictionary '{0}' to element '{1}': {2}", property,
                        _fields.Get("name"), ex.Message), ex);
            }
        }

        _objectDictionaries = null!;
        foreach (var action in _actions)
        {
            action();
        }

        _actions = null!;
        foreach (var field in _fields.FieldNames)
        {
            var attribute = _fields.Get(field);
            if (attribute is QuestList<object> objectList)
            {
                ResolveObjectList(objectList, scriptFactory);
            }

            if (attribute is QuestDictionary<object> objectDictionary)
            {
                ResolveObjectDictionary(objectDictionary, scriptFactory);
            }
        }

        _resolved = true;
    }

    private QuestDictionary<IScript> ConvertToScriptDictionary(IDictionary<string, string> dictionary,
        ScriptFactory scriptFactory)
    {
        var newDictionary = new QuestDictionary<IScript>();
        foreach (var item in dictionary)
        {
            var newScript = scriptFactory.CreateScript(item.Value);
            newDictionary.Add(item.Key, newScript);
        }

        return newDictionary;
    }

    private QuestDictionary<Element> ConvertToObjectDictionary(IDictionary<string, string?> dictionary)
    {
        var newDictionary = new QuestDictionary<Element>();
        foreach (var item in dictionary)
        {
            var element = _worldModel.Elements.Get(item.Value!);
            newDictionary.Add(item.Key, element);
        }

        return newDictionary;
    }

    public void AddType(string typename)
    {
        CheckNotResolved();
        _types.Add(typename);
    }

    public void AddDefaultType(string typename)
    {
        CheckNotResolved();
        _defaultTypes.Add(typename);
    }

    public void AddObjectField(string property, string value)
    {
        CheckNotResolved();
        _objectFields.Add(property, value);
    }

    public void AddScript(string property, string value)
    {
        CheckNotResolved();
        _scripts.Add(property, value);
    }

    public void AddScriptDictionary(string property, IDictionary<string, string> value)
    {
        CheckNotResolved();
        _scriptDictionaries.Add(property, value);
    }

    public void AddObjectList(string property, IEnumerable<string> value)
    {
        CheckNotResolved();
        _objectLists.Add(property, value);
    }

    public void AddObjectDictionary(string property, IDictionary<string, string?> value)
    {
        CheckNotResolved();
        _objectDictionaries.Add(property, value);
    }

    public void AddAction(Action action)
    {
        CheckNotResolved();
        _actions.Add(action);
    }

    private void ResolveObjectList(QuestList<object> list, ScriptFactory scriptFactory)
    {
        for (var i = 0; i < list.Count; i++)
        {
            var value = list[i];

            if (ReplaceValue(value, scriptFactory, out var replacement))
            {
                list.RemoveAt(i);
                list.Insert(i, replacement);
            }
        }
    }

    private void ResolveObjectDictionary(QuestDictionary<object> dictionary, ScriptFactory scriptFactory)
    {
        var copy = new Dictionary<string, object>(dictionary);

        foreach (var item in copy)
        {
            if (ReplaceValue(item.Value, scriptFactory, out var replacement))
            {
                dictionary[item.Key] = replacement;
            }
        }
    }

    private bool ReplaceValue(object? value, ScriptFactory scriptFactory, [NotNullWhen(true)] out object? replacement)
    {
        replacement = null;

        if (value is QuestList<object> genericList)
        {
            ResolveObjectList(genericList, scriptFactory);
            return false;
        }

        if (value is QuestDictionary<object> genericDictionary)
        {
            ResolveObjectDictionary(genericDictionary, scriptFactory);
            return false;
        }

        if (value is LazyObjectReference objRef)
        {
            replacement = _worldModel.Elements.Get(objRef.ObjectName);
            return true;
        }

        if (value is LazyObjectList objList)
        {
            replacement = new QuestList<Element>(objList.Objects.Select(o => _worldModel.Elements.Get(o)));
            return true;
        }

        if (value is LazyObjectDictionary objDictionary)
        {
            var newDictionary = new QuestDictionary<Element>();
            foreach (var kvp in objDictionary.Dictionary)
            {
                newDictionary.Add(kvp.Key, _worldModel.Elements.Get(kvp.Value!));
            }

            replacement = newDictionary;
            return true;
        }

        if (value is LazyScript script)
        {
            replacement = scriptFactory.CreateScript(script.Script);
            return true;
        }

        if (value is LazyScriptDictionary scriptDictionary)
        {
            replacement = ConvertToScriptDictionary(scriptDictionary.Dictionary, scriptFactory);
            return true;
        }

        return false;
    }

    private void CheckNotResolved()
    {
        if (_resolved)
        {
            throw new Exception("LazyFields instance already resolved.");
        }
    }
}

public class UndoFieldSet : UndoLogger.IUndoAction
{
    private readonly bool _added;
    private readonly object? _newValue;
    private readonly string? _newValueElementName;
    private readonly object? _oldValue;
    private readonly string? _oldValueElementName;
    private readonly bool _useMetaFields;
    private readonly WorldModel _worldModel;

    public UndoFieldSet(WorldModel worldModel, string appliesTo, string property, object? oldValue, object? newValue,
        bool added, bool useMetaFields)
    {
        Debug.Assert(!string.IsNullOrEmpty(appliesTo));
        _worldModel = worldModel;
        AppliesTo = appliesTo;
        Property = property;
        _useMetaFields = useMetaFields;

        if (oldValue is Element)
        {
            _oldValueElementName = ((Element) oldValue).Name;
        }
        else
        {
            _oldValue = oldValue;
        }

        if (newValue is Element)
        {
            _newValueElementName = ((Element) newValue).Name;
        }
        else
        {
            _newValue = newValue;
        }

        _added = added;

        //System.Diagnostics.Debug.Print("UndoFieldSet: {0}.{1} from '{2}' to '{3}'", appliesTo, property, oldValue, newValue);
    }

    public string AppliesTo { get; }

    public string Property { get; }

    public object? OldValue
    {
        get
        {
            if (_oldValueElementName != null)
            {
                if (_worldModel.Elements.ContainsKey(_oldValueElementName))
                {
                    return _worldModel.Elements.Get(_oldValueElementName);
                }

                // element may have been deleted
                return null;
            }

            return _oldValue;
        }
    }

    public object? NewValue
    {
        get
        {
            if (_newValueElementName != null)
            {
                return _worldModel.Elements.Get(_newValueElementName);
            }

            return _newValue;
        }
    }

    public void DoUndo(WorldModel worldModel)
    {
        var fields = GetFields(AppliesTo);
        if (_added)
        {
            fields.RemoveFieldInternal(Property);
        }
        else
        {
            fields.SetFromUndo(Property, OldValue);
        }
    }

    public void DoRedo(WorldModel worldModel)
    {
        var oldValue = OldValue;
        if (Property != "name" || oldValue == null)
        {
            GetFields(AppliesTo).SetFromUndo(Property, NewValue);
        }
        else
        {
            // When redoing a name change, _appliesTo will be incorrect as it will be the new object name.
            // So in this specific case we get the appliesTo name from the old property value.
            // (If OldValue is null then this is just setting the name property for a brand new object,
            // so the above comment doesn't apply, and this case is handled in the above "if")
            GetFields((string) oldValue).SetFromUndo(Property, NewValue);
        }
    }

    private Fields GetFields(string elementName)
    {
        var element = _worldModel.Elements.Get(elementName);
        return _useMetaFields ? element.MetaFields : element.Fields;
    }
}

public class UndoFieldRemove : UndoLogger.IUndoAction
{
    private readonly string _appliesTo;
    private readonly object? _oldValue;
    private readonly string _property;

    public UndoFieldRemove(string appliesTo, string property, object? oldValue)
    {
        _appliesTo = appliesTo;
        _property = property;
        _oldValue = oldValue;
    }

    public void DoUndo(WorldModel worldModel)
    {
        worldModel.Object(_appliesTo).Fields.SetFromUndo(_property, _oldValue);
    }

    public void DoRedo(WorldModel worldModel)
    {
        worldModel.Object(_appliesTo).Fields.RemoveFieldInternal(_property);
    }
}

public class UndoAddRemoveType : UndoLogger.IUndoAction
{
    private readonly string _appliesTo;
    private readonly Stack<Element> _newValue;
    private readonly Stack<Element> _oldValue;

    public UndoAddRemoveType(string appliesTo, Stack<Element> oldValue, Stack<Element> newValue)
    {
        _appliesTo = appliesTo;
        _oldValue = oldValue;
        _newValue = newValue;
    }

    public void DoUndo(WorldModel worldModel)
    {
        worldModel.Object(_appliesTo).Fields.DoUndoAddRemoveType(_oldValue);
    }

    public void DoRedo(WorldModel worldModel)
    {
        worldModel.Object(_appliesTo).Fields.DoUndoAddRemoveType(_newValue);
    }
}