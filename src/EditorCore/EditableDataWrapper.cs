namespace QuestViva.EditorCore;

internal class EditableDataWrapper<TSource, TWrapped>
    where TSource : notnull
{
    private readonly Func<EditorController, TSource, TWrapped> _getNewWrappedInstance;
    private readonly Dictionary<TSource, TWrapped> _instances = new();

    public EditableDataWrapper(Func<EditorController, TSource, TWrapped> instanceCreator)
    {
        _getNewWrappedInstance = instanceCreator;
    }

    public TWrapped GetInstance(EditorController controller, TSource source)
    {
        if (_instances.TryGetValue(source, out var existing))
        {
            return existing;
        }

        var instance = _getNewWrappedInstance(controller, source);
        _instances.Add(source, instance);
        return instance;
    }

    internal void Clear()
    {
        _instances.Clear();
    }
}