using System.Collections;

namespace QuestViva.EditorCore;

public class EditableListUpdatedEventArgs<T> : EventArgs
{
    public IEditableListItem<T> UpdatedItem { get; set; }
    public int Index { get; set; }
    public EditorUpdateSource Source { get; set; }
}

public interface IEditableList<T> : IEnumerable
{
    IDictionary<string, IEditableListItem<T>> Items { get; }
    IEnumerable<KeyValuePair<string, string>> DisplayItems { get; }
    bool Locked { get; }
    string Owner { get; }
    IEnumerable<IEditableListItem<T>> ItemsList { get; }
    event EventHandler<EditableListUpdatedEventArgs<T>> Added;
    event EventHandler<EditableListUpdatedEventArgs<T>> Removed;
    void Add(T item);
    void Remove(params string[] keys);
    void Update(int index, T item);
    void Update(string key, T item);
    ValidationResult CanAdd(T item);
    IEditableList<T> Clone(string parent, string attribute);
}

public interface IEditableListItem<T>
{
    string Key { get; }
    T Value { get; set; }
}

public interface IEditableDictionary<T>
{
    string Id { get; }
    IDictionary<string, IEditableListItem<T>> Items { get; }
    IEnumerable<KeyValuePair<string, string>> DisplayItems { get; }
    T this[string key] { get; }
    bool Locked { get; }
    string Owner { get; }
    event EventHandler<EditableListUpdatedEventArgs<T>> Added;
    event EventHandler<EditableListUpdatedEventArgs<T>> Removed;
    event EventHandler<EditableListUpdatedEventArgs<T>> Updated;
    void Add(string key, T value);
    void Remove(params string[] keys);
    void Update(string key, T value);
    void ChangeKey(string oldKey, string newKey);
    ValidationResult CanAdd(string key);
    IEditableDictionary<T> Clone(string parent, string attribute);
}