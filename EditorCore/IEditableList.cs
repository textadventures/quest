using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace TextAdventures.Quest
{
    public class EditableListUpdatedEventArgs<T> : EventArgs
    {
        public IEditableListItem<T> UpdatedItem { get; set; }
        public int Index { get; set; }
        public EditorUpdateSource Source { get; set; }
    }

    public interface IEditableList<T> : IEnumerable
    {
        event EventHandler<EditableListUpdatedEventArgs<T>> Added;
        event EventHandler<EditableListUpdatedEventArgs<T>> Removed;

        IDictionary<string, IEditableListItem<T>> Items { get; }
        void Add(T item);
        void Remove(params string[] keys);
        void Update(int index, T item);
        void Update(string key, T item);
        IEnumerable<KeyValuePair<string, string>> DisplayItems { get; }
        ValidationResult CanAdd(T item);
        bool Locked { get; }
        IEditableList<T> Clone(string parent, string attribute);
        string Owner { get; }
        IEnumerable<IEditableListItem<T>> ItemsList { get; }
    }

    public interface IEditableListItem<T>
    {
        string Key { get; }
        T Value { get; set; }
    }

    public interface IEditableDictionary<T>
    {
        event EventHandler<EditableListUpdatedEventArgs<T>> Added;
        event EventHandler<EditableListUpdatedEventArgs<T>> Removed;
        event EventHandler<EditableListUpdatedEventArgs<T>> Updated;

        string Id { get; }
        IDictionary<string, IEditableListItem<T>> Items { get; }
        IEnumerable<KeyValuePair<string, string>> DisplayItems { get; }
        void Add(string key, T value);
        void Remove(params string[] keys);
        void Update(string key, T value);
        void ChangeKey(string oldKey, string newKey);
        ValidationResult CanAdd(string key);
        T this[string key] { get; }
        bool Locked { get; }
        IEditableDictionary<T> Clone(string parent, string attribute);
        string Owner { get; }
    }
}
