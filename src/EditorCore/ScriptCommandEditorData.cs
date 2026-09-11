namespace QuestViva.EditorCore;

public class ScriptCommandEditorData : IEditorData
{
    private readonly EditorController _controller;
    private readonly IEditableScript _script;

    public ScriptCommandEditorData(EditorController controller, IEditableScript script)
    {
        _controller = controller;
        _script = script;

        _script.Updated += OnScriptUpdated;
    }

    public event EventHandler? Changed;

    public string? Name => null;

    public object? GetAttribute(string attribute)
    {
        return _controller.WrapValue(_script.GetParameter(attribute));
    }

    public ValidationResult SetAttribute(string attribute, object? value)
    {
        _script.SetParameter(attribute, value);

        return new ValidationResult {Valid = true};
    }

    public string? GetSelectedFilter(string filterGroup)
    {
        return null;
    }

    public void SetSelectedFilter(string filterGroup, string filter)
    {
    }

    public IEnumerable<string>? GetVariablesInScope()
    {
        return _script.GetVariablesInScope();
    }

    private void OnScriptUpdated(object? sender, EditableScriptUpdatedEventArgs e)
    {
        Changed?.Invoke(this, new EventArgs());
    }
}