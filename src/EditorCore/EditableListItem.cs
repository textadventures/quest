
namespace QuestViva.EditorCore
{
    public class EditableListItem<T> : IEditableListItem<T>
    {
        public EditableListItem(string key, T value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; private set; }
        public T Value { get; set; }
    }
}
