using QuestViva.Engine;
using QuestViva.Engine.Types;

namespace QuestViva.EditorCore;

public class EditableCommandPattern : IEditableCommandPattern
{
    private readonly string _attribute;
    private readonly Element _parent;
    private readonly EditorCommandPattern _pattern;
    private EditorController _controller;

    public EditableCommandPattern(EditorController controller, EditorCommandPattern pattern, Element parent,
        string attribute)
    {
        _pattern = pattern;
        _controller = controller;
        _parent = parent;
        _attribute = attribute;
    }

    public event EventHandler<DataWrapperUpdatedEventArgs> UnderlyingValueUpdated
    {
        add { }
        remove { }
    }

    public object GetUnderlyingValue()
    {
        return _pattern;
    }

    public string DisplayString()
    {
        return _pattern.Pattern;
    }

    public string Pattern
    {
        get => _pattern.Pattern;
        set => _parent.Fields.Set(_attribute, new EditorCommandPattern(value));
    }
}