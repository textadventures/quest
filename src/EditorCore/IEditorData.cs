namespace QuestViva.EditorCore;

public interface IEditorAttributeData
{
    string AttributeName { get; }
    bool IsInherited { get; }
    string? Source { get; }
    bool IsDefaultType { get; }
}

public interface IEditorData
{
    string? Name { get; }
    object? GetAttribute(string attribute);
    ValidationResult SetAttribute(string attribute, object? value);
    string? GetSelectedFilter(string filterGroup);
    void SetSelectedFilter(string filterGroup, string filter);
    IEnumerable<string>? GetVariablesInScope();

    event EventHandler Changed;
}

public interface IEditorDataExtendedAttributeInfo : IEditorData
{
    bool IsLibraryElement { get; }
    string? Filename { get; }
    IEnumerable<IEditorAttributeData> GetAttributeData();
    void RemoveAttribute(string attribute);
    IEnumerable<IEditorAttributeData> GetInheritedTypes();
    void MakeElementLocal();
}