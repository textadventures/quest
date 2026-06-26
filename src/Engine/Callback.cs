#nullable disable
using QuestViva.Engine.Scripts;

namespace QuestViva.Engine;

public class Callback
{
    public Callback(IScript script, Context context)
    {
        Script = script;
        Context = context;
    }

    public IScript Script { get; private set; }
    public Context Context { get; private set; }
}

internal class CallbackManager
{
    private List<Callback> m_onReadyCallbacks = new();

    public void AddOnReadyCallback(Callback callback)
    {
        m_onReadyCallbacks.Add(callback);
    }

    public IEnumerable<Callback> FlushOnReadyCallbacks()
    {
        var currentList = m_onReadyCallbacks;
        m_onReadyCallbacks = new List<Callback>();
        return currentList;
    }
}
