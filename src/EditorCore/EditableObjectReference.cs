using QuestViva.Engine;

namespace QuestViva.EditorCore;

public class EditableObjectReference : IEditableObjectReference
{
    private readonly string _attribute;
    private readonly EditorController _controller;
    private readonly Element _parent;
    private Element? _object;

    public EditableObjectReference(EditorController controller, Element obj, Element parent, string attribute)
    {
        _object = obj;
        _controller = controller;
        _parent = parent;
        _attribute = attribute;
    }

    public event EventHandler<DataWrapperUpdatedEventArgs> UnderlyingValueUpdated
    {
        add { }
        remove { }
    }

    public object? GetUnderlyingValue()
    {
        return _object;
    }

    public string DisplayString()
    {
        return "Object: " + _object!.Name;
    }

    public string Reference
    {
        get => _object!.Name;
        set
        {
            _object = string.IsNullOrEmpty(value) ? null : _controller.WorldModel.Elements.Get(value);
            _parent.Fields.Set(_attribute, _object);
        }
    }
}