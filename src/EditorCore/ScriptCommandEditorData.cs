namespace QuestViva.EditorCore;

public class ScriptCommandEditorData : IEditorData
{
    private readonly EditorController m_controller;
    private readonly IEditableScript m_script;

    public ScriptCommandEditorData(EditorController controller, IEditableScript script)
    {
        m_controller = controller;
        m_script = script;

        m_script.Updated += m_script_Updated;
    }

    public event EventHandler Changed;

    public string Name => null;

    public object GetAttribute(string attribute)
    {
        return m_controller.WrapValue(m_script.GetParameter(attribute));
    }

    public ValidationResult SetAttribute(string attribute, object value)
    {
        m_script.SetParameter(attribute, value);

        return new ValidationResult {Valid = true};
    }

    public IEnumerable<string> GetAffectedRelatedAttributes(string attribute)
    {
        return null;
    }

    public string GetSelectedFilter(string filterGroup)
    {
        return null;
    }

    public void SetSelectedFilter(string filterGroup, string filter)
    {
    }

    public bool ReadOnly { get; set; }

    public IEnumerable<string> GetVariablesInScope()
    {
        return m_script.GetVariablesInScope();
    }

    public bool IsDirectlySaveable => true;

    private void m_script_Updated(object sender, EditableScriptUpdatedEventArgs e)
    {
        if (Changed != null)
        {
            Changed(this, new EventArgs());
        }
    }
}