namespace QuestViva.EditorCore;

public class EditableScriptsUpdatedEventArgs : EventArgs
{
    internal EditableScriptsUpdatedEventArgs()
    {
    }

    internal EditableScriptsUpdatedEventArgs(IEditableScript updatedScript, EditableScriptUpdatedEventArgs args)
    {
        UpdatedScript = updatedScript;
        UpdatedScriptEventArgs = args;
    }

    public IEditableScript? UpdatedScript { get; private set; }
    public EditableScriptUpdatedEventArgs? UpdatedScriptEventArgs { get; private set; }
}

public interface IEditableScripts : IDataWrapper
{
    IEnumerable<IEditableScript> Scripts { get; }
    IEditableScript this[int index] { get; }
    int Count { get; }
    string? Owner { get; }
    string Code { get; set; }
    // index: where to insert the new script; null (or past the end) adds it at the end.
    void AddNew(string keyword, string elementName, int? index = null);
    event EventHandler<EditableScriptsUpdatedEventArgs> Updated;
    void Remove(int[] indexes);
    string DisplayString(int index, string newValue);
    void Swap(int index1, int index2);
    void Cut(int[] indexes);
    void Copy(int[] indexes);
    void Paste(int index, bool useTransaction);
    IEditableScripts Clone(string parent, string attribute);
}