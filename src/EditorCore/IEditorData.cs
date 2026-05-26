namespace QuestViva.EditorCore;

public interface IEditorAttributeData
{
    string AttributeName { get; }
    bool IsInherited { get; }
    string Source { get; }
    bool IsDefaultType { get; }
}

public interface IEditorData
{
    string Name { get; }
    bool ReadOnly { get; set; }

    // Usually set to True, but set to false for ExpressionTemplateEditorData as any update
    // to an expression template parameter can only be persisted by saving the entire parent
    // expression.
    bool IsDirectlySaveable { get; }
    object GetAttribute(string attribute);
    ValidationResult SetAttribute(string attribute, object value);
    IEnumerable<string> GetAffectedRelatedAttributes(string attribute);
    string GetSelectedFilter(string filterGroup);
    void SetSelectedFilter(string filterGroup, string filter);
    IEnumerable<string> GetVariablesInScope();

    event EventHandler Changed;
}

public interface IEditorDataExtendedAttributeInfo : IEditorData
{
    bool IsLibraryElement { get; }
    string Filename { get; }
    IEnumerable<IEditorAttributeData> GetAttributeData();
    IEditorAttributeData GetAttributeData(string attribute);
    void RemoveAttribute(string attribute);
    IEnumerable<IEditorAttributeData> GetInheritedTypes();
    void MakeElementLocal();
}