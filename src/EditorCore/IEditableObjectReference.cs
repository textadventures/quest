namespace QuestViva.EditorCore
{
    public interface IEditableObjectReference : IDataWrapper
    {
        string Reference { get; set; }
    }
}
