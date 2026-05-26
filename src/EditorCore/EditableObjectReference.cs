using QuestViva.Engine;

namespace QuestViva.EditorCore;

public class EditableObjectReference : IEditableObjectReference
{
    private readonly string m_attribute;
    private readonly EditorController m_controller;
    private readonly Element m_parent;
    private Element m_object;

    public EditableObjectReference(EditorController controller, Element obj, Element parent, string attribute)
    {
        m_object = obj;
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
        return m_object;
    }

    public string DisplayString()
    {
        return "Object: " + m_object.Name;
    }

    public string Reference
    {
        get => m_object.Name;
        set
        {
            m_object = string.IsNullOrEmpty(value) ? null : m_controller.WorldModel.Elements.Get(value);
            m_parent.Fields.Set(m_attribute, m_object);
        }
    }
}