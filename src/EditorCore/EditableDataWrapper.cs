namespace QuestViva.EditorCore;

internal class EditableDataWrapper<TSource, TWrapped>
{
    private readonly Func<EditorController, TSource, TWrapped> GetNewWrappedInstance;
    private readonly Dictionary<TSource, TWrapped> s_instances = new();

    public EditableDataWrapper(Func<EditorController, TSource, TWrapped> instanceCreator)
    {
        GetNewWrappedInstance = instanceCreator;
    }

    public TWrapped GetInstance(EditorController controller, TSource source)
    {
        TWrapped instance;
        if (s_instances.TryGetValue(source, out instance))
        {
            return instance;
        }

        instance = GetNewWrappedInstance(controller, source);
        s_instances.Add(source, instance);
        return instance;
    }

    internal void Clear()
    {
        s_instances.Clear();
    }
}