using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public class EditableListUpdatedEventArgs<T> : EventArgs
    {
        public IEditableListItem<T> UpdatedItem { get; set; }
        public int Index { get; set; }
        public EditorUpdateSource Source { get; set; }
    }

    public interface IEditableList<T>
    {
        event EventHandler<EditableListUpdatedEventArgs<T>> Added;
        event EventHandler<EditableListUpdatedEventArgs<T>> Removed;

        IDictionary<string, IEditableListItem<T>> Items { get; }
        void Add(T item);
        void Remove(params T[] items);
        void Update(int index, T item);
        IEnumerable<KeyValuePair<string, string>> DisplayItems { get; }
        ValidationResult CanAdd(T item);
        bool Locked { get; }
        IEditableList<T> Clone(string parent, string attribute);
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

        IDictionary<string, IEditableListItem<T>> Items { get; }
        IEnumerable<KeyValuePair<string, string>> DisplayItems { get; }
        void Add(string key, T value);
        void Remove(params string[] keys);
        void Update(string key, T value);
        ValidationResult CanAdd(string key);
        T this[string key] { get; }
        bool Locked { get; }
        IEditableDictionary<T> Clone(string parent, string attribute);
    }
}
