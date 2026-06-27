namespace QuestViva.Common;

public class ObjectsUpdatedEventArgs : EventArgs
{
    public string? Added { get; set; }
    public string? Removed { get; set; }
}

public interface IGameDebug
{
    bool DebugEnabled { get; }
    List<string> DebuggerObjectTypes { get; }
    IWalkthroughs Walkthroughs { get; }
    event EventHandler<ObjectsUpdatedEventArgs>? ObjectsUpdated;
    List<string> GetObjects(string type);
    DebugData GetDebugData(string tab, string obj);
    Task<bool> AssertAsync(string expression);
}

public class DebugDataItem
{
    public DebugDataItem(string value)
        : this(value, false)
    {
    }

    public DebugDataItem(string value, bool isInherited)
        : this(value, isInherited, null)
    {
    }

    public DebugDataItem(string value, bool isInherited, string? source)
    {
        Value = value;
        IsInherited = isInherited;
        Source = source;
    }

    public string Value { get; private set; }

    public bool IsInherited { get; set; }

    public string? Source { get; set; }

    public bool IsDefaultType { get; set; }
}

public class DebugData
{
    public static readonly DebugData Empty = new();

    public Dictionary<string, DebugDataItem> Data { get; set; } = new();
}

public interface IWalkthroughs
{
    IDictionary<string, IWalkthrough> Walkthroughs { get; }
}

public interface IWalkthrough
{
    string[] Steps { get; }
}