#nullable disable
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace QuestViva.Engine.Scripts;

public class NoReturnValue
{
}

public class Context
{
    public Context()
    {
        ReturnValue = new NoReturnValue();
    }

    public Parameters Parameters { get; set; }
    public object ReturnValue { get; set; }
    public bool IsReturned { get; set; }
}

[SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
public class Parameters : Dictionary<string, object>
{
    public Parameters()
    {
    }

    public Parameters(string key, object value)
    {
        Add(key, value);
    }

    public Parameters(IDictionary parameters)
    {
        foreach (var key in parameters.Keys)
        {
            if (key is string)
            {
                Add((string) key, parameters[key]);
            }
        }
    }
}