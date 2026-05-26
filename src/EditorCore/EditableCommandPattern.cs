using QuestViva.Engine;
using QuestViva.Engine.Types;

namespace QuestViva.EditorCore;

public class EditableCommandPattern : IEditableCommandPattern
{
    private readonly string m_attribute;
    private readonly Element m_parent;
    private readonly EditorCommandPattern m_pattern;
    private EditorController m_controller;

    public EditableCommandPattern(EditorController controller, EditorCommandPattern pattern, Element parent,
        string attribute)
    {
        m_pattern = pattern;
        m_controller = controller;
        m_parent = parent;
        m_attribute = attribute;
    }

    public event EventHandler<DataWrapperUpdatedEventArgs> UnderlyingValueUpdated
    {
        add { }
        remove { }
    }

    public object GetUnderlyingValue()
    {
        return m_pattern;
    }

    public string DisplayString()
    {
        return m_pattern.Pattern;
    }

    public string Pattern
    {
        get => m_pattern.Pattern;
        set => m_parent.Fields.Set(m_attribute, new EditorCommandPattern(value));
    }
}