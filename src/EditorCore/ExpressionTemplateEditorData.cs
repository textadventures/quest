namespace QuestViva.EditorCore;

internal class ExpressionTemplateEditorData : IEditorData
{
    private readonly string _originalPattern;
    private readonly IDictionary<string, string> _parameters;
    private readonly IEditorData _parentData;

    public ExpressionTemplateEditorData(string expression, EditorDefinition definition, IEditorData parentData)
    {
        // We get passed in an expression like "Got(myobject)"
        // The definition has pattern like "Got(#object#)" (as a regex)
        // We create the parameter dictionary in the same way as the command parser, so
        // we end up with a dictionary like "object=myobject".

        // Expression template editors always define a pattern and its original form
        _parameters = Engine.Utility.Populate(definition.Pattern!, expression);
        _originalPattern = definition.OriginalPattern!;
        _parentData = parentData;
    }

    public event EventHandler? Changed;

    public string Name => throw new NotImplementedException();

    public object? GetAttribute(string attribute)
    {
        return _parameters[attribute];
    }

    public ValidationResult SetAttribute(string attribute, object? value)
    {
        _parameters[attribute] = (string) value!;
        if (Changed != null)
        {
            Changed(this, new EventArgs());
        }

        return new ValidationResult {Valid = true};
    }

    public string? GetSelectedFilter(string filterGroup)
    {
        throw new NotImplementedException();
    }

    public void SetSelectedFilter(string filterGroup, string filter)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<string>? GetVariablesInScope()
    {
        return _parentData.GetVariablesInScope();
    }
}