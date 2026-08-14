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

    // Debugger attribute override — runs `{element}.{attribute} = {valueExpression}`
    // through the game's own script engine, so it gets the same type coercion
    // (quoted strings, numbers, booleans, bare-identifier object references)
    // as a script the game itself could run. Returns null on success, or an
    // error message (bad syntax, unknown element, ...) on failure.
    Task<string?> SetAttributeAsync(string element, string attribute, string valueExpression);
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

    // A version of Value pre-formatted as valid script syntax (quoted string,
    // lowercase true/false, bare object name, ...) rather than a
    // human-readable label — what the WasmPlayer debugger's attribute
    // override pre-fills its input with, so most edits are a direct Apply
    // instead of the user having to hand-quote/rewrite Value themselves. Null
    // for callers that don't populate it (e.g. Legacy's V4Game), in which
    // case Value is the only thing available to pre-fill with.
    public string? EditValue { get; set; }

    // Whether the WasmPlayer debugger should offer an override control for
    // this attribute at all — false for value kinds with no simple literal
    // syntax to write back (lists, dictionaries, scripts, ...), where an
    // override textbox would just be a dead end. True by default so callers
    // that don't populate it (e.g. Legacy's V4Game) keep the old behaviour.
    public bool CanOverride { get; set; } = true;

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