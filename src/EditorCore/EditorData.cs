using QuestViva.Common;
using QuestViva.Engine;

namespace QuestViva.EditorCore;

internal class EditorAttributeData : IEditorAttributeData
{
    public EditorAttributeData(string attributeName, bool isInherited, string? source, bool isDefaultType)
    {
        AttributeName = attributeName;
        IsInherited = isInherited;
        Source = source;
        IsDefaultType = isDefaultType;
    }

    public string AttributeName { get; }

    public bool IsInherited { get; }

    public string? Source { get; set; }

    public bool IsDefaultType { get; set; }
}

internal class EditorData : IEditorDataExtendedAttributeInfo
{
    private readonly EditorController _controller;
    private readonly Element _element;
    private readonly Dictionary<string, string> _filters = [];

    public EditorData(Element element, EditorController controller)
    {
        _element = element;
        _controller = controller;

        element.Fields.AttributeChanged += Fields_AttributeChanged;
        element.Fields.AttributeChangedSilent += Fields_AttributeChanged;
    }

    public event EventHandler? Changed;

    public string Name => _element.Name;

    public object? GetAttribute(string attribute)
    {
        if (attribute == "name" && _element.Fields[FieldDefinitions.Anonymous])
        {
            return string.Empty;
        }

        return _controller.WrapValue(_element.Fields.Get(attribute), _element, attribute);
    }

    public ValidationResult SetAttribute(string attribute, object? value)
    {
        if (attribute == "name")
        {
            if (!(value is string))
            {
                return new ValidationResult {Valid = false, Message = ValidationMessage.InvalidAttributeName};
            }

            ValidationResult result;

            result = _controller.CanRename(_element, (string) value);
            if (!result.Valid)
            {
                return result;
            }

            if (_element.Fields[FieldDefinitions.Anonymous])
            {
                _element.Fields[FieldDefinitions.Anonymous] = false;
            }
        }

        if (!Engine.Utility.IsValidAttributeName(attribute))
        {
            return new ValidationResult {Valid = false, Message = ValidationMessage.InvalidAttributeName};
        }

        if (value is IDataWrapper wrapper)
        {
            value = wrapper.GetUnderlyingValue();
        }

        string? oldName = null;
        if (attribute == "name")
        {
            oldName = _element.Name;
        }

        _element.Fields.Set(attribute, value);

        if (attribute == "name")
        {
            _controller.UpdateDictionariesReferencingRenamedObject(oldName!, (string) value!);
        }

        return new ValidationResult {Valid = true};
    }

    public string? GetSelectedFilter(string filterGroup)
    {
        _filters.TryGetValue(filterGroup, out var result);
        return result;
    }

    public void SetSelectedFilter(string filterGroup, string filter)
    {
        _filters[filterGroup] = filter;
        Changed?.Invoke(this, new EventArgs());
    }

    public IEnumerable<IEditorAttributeData> GetAttributeData()
    {
        var data = _controller.WorldModel.GetDebugData(_element.Name);
        return ConvertDebugDataToEditorAttributeData(data);
    }

    public void RemoveAttribute(string attribute)
    {
        _element.Fields.RemoveField(attribute);
    }

    public IEnumerable<IEditorAttributeData> GetInheritedTypes()
    {
        var data = _controller.WorldModel.GetInheritedTypesDebugData(_element.Name);
        return ConvertDebugDataToEditorAttributeData(data);
    }

    public bool IsLibraryElement => _element.MetaFields[MetaFieldDefinitions.Library];

    public string? Filename => _element.MetaFields[MetaFieldDefinitions.Filename];

    public void MakeElementLocal()
    {
        if (!IsLibraryElement)
        {
            throw new InvalidOperationException("Element is not defined in a library");
        }

        _element.MetaFields[MetaFieldDefinitions.Library] = false;
        _element.MetaFields[MetaFieldDefinitions.Filename] = null;
    }

    public IEnumerable<string>? GetVariablesInScope()
    {
        return null;
    }

    private void Fields_AttributeChanged(object? sender, AttributeChangedEventArgs e)
    {
        Changed?.Invoke(this, new EventArgs());
    }

    private IEnumerable<IEditorAttributeData> ConvertDebugDataToEditorAttributeData(DebugData data)
    {
        var result = new List<EditorAttributeData>();
        foreach (var item in data.Data)
        {
            result.Add(new EditorAttributeData(item.Key, item.Value.IsInherited, item.Value.Source,
                item.Value.IsDefaultType));
        }

        return result;
    }
}