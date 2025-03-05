using QuestViva.Common;

namespace QuestViva.Legacy;

public partial class V4Game
{
    private class WalkthroughList : IWalkthroughs
    {
        private readonly Dictionary<string, IWalkthrough> _walkthroughs = new();
        
        public WalkthroughList(V4Game game)
        {
            foreach (var section in game._defineBlocks.Skip(1))
            {
                var startLine = game._lines[section.StartLine];
                
                if (!startLine.StartsWith("define text <walkthrough"))
                {
                    continue;
                }

                var name = game.GetParameter(startLine, game._nullContext, false);

                var steps = new List<string>();
                for (var i = section.StartLine + 1; i < section.EndLine; i++)
                {
                    steps.Add(game._lines[i].Trim());
                }
                
                var walkthrough = new Walkthrough(steps.ToArray());
                
                _walkthroughs.Add(name, walkthrough);
            }
        }
        
        public IDictionary<string, IWalkthrough> Walkthroughs => _walkthroughs;
    }

    private class Walkthrough(string[] steps) : IWalkthrough
    {
        public string[] Steps => steps;
    }
    
    public bool DebugEnabled => true;
    public event EventHandler<ObjectsUpdatedEventArgs> ObjectsUpdated;
    
    public List<string> GetObjects(string type)
    {
        switch (type)
        {
            case "Variables":
                return ["Strings", "Numerics"];
            case "Game":
                return [_objs[1].ObjectName];
            case "Rooms":
                return _objs.Skip(2).Where(o => o.IsRoom).Select(r => r.ObjectName).ToList();
            case "Objects":
                return _objs.Skip(2).Where(o => !o.IsRoom && !o.IsExit).Select(r => r.ObjectName).ToList();
            case "Exits":
                return _objs.Skip(2).Where(o => o.IsExit).Select(r => r.ObjectName).ToList();
            case "Timers":
                return _timers.Skip(1).Select(t => t.TimerName).ToList();
            default:
                throw new NotImplementedException();
        }
    }

    public DebugData GetDebugData(string tab, string obj)
    {
        switch (tab)
        {
            case "Variables":
                return obj switch
                {
                    "Strings" => GetVariableDebugData(_stringVariable),
                    "Numerics" => GetVariableDebugData(_numericVariable),
                    _ => throw new NotImplementedException()
                };
            case "Game":
            case "Rooms":
            case "Objects":
            case "Exits":
                return GetObjectDebugData(_objs.Skip(1).First(o => o.ObjectName == obj));
            case "Timers":
                return GetTimerDebugData(_timers.Skip(1).First(t => t.TimerName == obj));
            default:
                throw new NotImplementedException();
        }
    }

    private DebugData GetVariableDebugData(VariableType[] variable)
    {
        if (variable == null || variable.Length == 0)
        {
            return DebugData.Empty;
        }
        
        var data = variable
            .Skip(1)
            .Select(v => new KeyValuePair<string, DebugDataItem>(v.VariableName,
                new DebugDataItem(string.Join(", ", v.VariableContents))));

        return new DebugData
        {
            Data = data.ToDictionary()
        };
    }

    private DebugData GetObjectDebugData(ObjectType obj)
    {
        if (obj.Properties == null)
        {
            return DebugData.Empty;
        }
        
        var data = obj
            .Properties
            .Skip(1)
            .Select(p => new KeyValuePair<string, DebugDataItem>(p.PropertyName,
                new DebugDataItem(p.PropertyValue)));

        return new DebugData
        {
            Data = data.ToDictionary()
        };
    }

    private DebugData GetTimerDebugData(TimerType timer)
    {
        return new DebugData
        {
            Data = new Dictionary<string, DebugDataItem>
            {
                ["Ticks"] = new(timer.TimerTicks.ToString()),
                ["Interval"] = new(timer.TimerInterval.ToString()),
                ["Active"] = new(timer.TimerActive ? "true" : "false")
            }
        };
    }

    public List<string> DebuggerObjectTypes => [
        "Variables",
        "Game",
        "Rooms",
        "Objects",
        "Exits",
        "Timers"
    ];

    private IWalkthroughs _walkthroughs;
    public IWalkthroughs Walkthroughs => _walkthroughs ??= new WalkthroughList(this);

    public bool Assert(string expression)
    {
        throw new NotImplementedException();
    }
}