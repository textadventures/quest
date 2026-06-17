#nullable disable
namespace QuestViva.Engine.Scripts;

internal interface IFirstTimeScript
{
    void SetOtherwiseScript(IScript script);
}

public class FirstTimeScriptConstructor : IScriptConstructor
{
    public string Keyword => "firsttime";

    public IScript Create(string script, ScriptContext scriptContext)
    {
        // Get script after "firsttime" keyword
        script = script.Substring(9).Trim();
        var firstTime = Utility.GetScript(script);
        var firstTimeScript = ScriptFactory.CreateScript(firstTime);

        return new FirstTimeScript(WorldModel, ScriptFactory, firstTimeScript);
    }

    public IScriptFactory ScriptFactory { get; set; }

    public WorldModel WorldModel { get; set; }

    public static void AddOtherwiseScript(IScript firstTimeScript, string script, IScriptFactory scriptFactory)
    {
        // Get script after "otherwise" keyword
        script = script.Substring(9).Trim();
        var otherwise = Utility.GetScript(script);
        var otherwiseScript = scriptFactory.CreateScript(otherwise);
        ((IFirstTimeScript) firstTimeScript).SetOtherwiseScript(otherwiseScript);
    }
}

public class FirstTimeScript : ScriptBase, IFirstTimeScript
{
    private readonly IScript m_firstTimeScript;
    private readonly IScriptFactory m_scriptFactory;
    private readonly WorldModel m_worldModel;
    private bool m_hasRun;
    private IScript m_otherwiseScript;

    public FirstTimeScript(WorldModel worldModel, IScriptFactory scriptFactory, IScript firstTimeScript)
    {
        m_worldModel = worldModel;
        m_scriptFactory = scriptFactory;
        m_firstTimeScript = firstTimeScript;
    }

    public override string Keyword => "firsttime";

    public void SetOtherwiseScript(IScript script)
    {
        m_otherwiseScript = script;
    }

    protected override ScriptBase CloneScript()
    {
        var result = new FirstTimeScript(m_worldModel, m_scriptFactory, (IScript) m_firstTimeScript.Clone());
        if (m_otherwiseScript != null)
        {
            result.m_otherwiseScript = (IScript) m_otherwiseScript.Clone();
        }

        return result;
    }

    protected override void ParentUpdated()
    {
        m_firstTimeScript.Parent = Parent;
    }

    public override void Execute(Context c)
    {
        if (!m_hasRun)
        {
            m_hasRun = true;
            m_worldModel.UndoLogger.AddUndoAction(() => new UndoFirstTime(this));
            m_firstTimeScript.Execute(c);
        }
        else
        {
            if (m_otherwiseScript != null)
            {
                m_otherwiseScript.Execute(c);
            }
        }
    }

    public override string Save()
    {
        if (m_worldModel.EditMode || !m_hasRun)
        {
            if (m_otherwiseScript == null)
            {
                return SaveScript("firsttime", m_firstTimeScript);
            }

            return SaveScript("firsttime", m_firstTimeScript) + Environment.NewLine +
                   SaveScript("otherwise", m_otherwiseScript);
        }

        if (m_otherwiseScript == null)
        {
            return string.Empty;
        }

        return m_otherwiseScript.Save();
    }

    public override object GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return m_firstTimeScript;
            case 1:
                return m_otherwiseScript;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object value)
    {
        switch (index)
        {
            case 0:
                // any updates to the script should change the script itself - nothing should cause SetParameter to be triggered.
                throw new InvalidOperationException(
                    "Attempt to use SetParameter to change the script of a 'firsttime' script");
            case 1:
                m_otherwiseScript = (IScript) value;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private class UndoFirstTime : UndoLogger.IUndoAction
    {
        private readonly FirstTimeScript m_parent;

        public UndoFirstTime(FirstTimeScript parent)
        {
            m_parent = parent;
        }

        public void DoUndo(WorldModel worldModel)
        {
            m_parent.m_hasRun = false;
        }

        public void DoRedo(WorldModel worldModel)
        {
            m_parent.m_hasRun = true;
        }
    }
}