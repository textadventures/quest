#nullable disable
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class InsertScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "insert";

    protected override int[] ExpectedParameters
    {
        get { return new[] {1}; }
    }

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new InsertScript(scriptContext, new Expression<string>(parameters[0], scriptContext));
    }
}

public class InsertScript : ScriptBase
{
    private readonly ScriptContext m_scriptContext;
    private readonly WorldModel m_worldModel;
    private IFunction<string> m_filename;

    public InsertScript(ScriptContext scriptContext, IFunction<string> filename)
    {
        m_scriptContext = scriptContext;
        m_worldModel = scriptContext.WorldModel;
        m_filename = filename;
    }

    public override string Keyword => "insert";

    protected override ScriptBase CloneScript()
    {
        return new InsertScript(m_scriptContext, m_filename.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        if (m_worldModel.Version >= WorldModelVersion.v540)
        {
            throw new Exception(
                "The 'insert' script command is not supported for games written for Quest 5.4 or later. You can output HTML directly using the 'msg' command instead.");
        }

        var filename = await m_filename.ExecuteAsync(c);
        if (m_worldModel.Version == WorldModelVersion.v500)
        {
            // v500 games used Frame.htm for static panel feature. This is now implemented natively
            // in Player and WebPlayer.
            if (filename.ToLower() == "frame.htm")
            {
                return;
            }
        }

        var stream = m_worldModel.GetResourceStream(filename);
        if (stream == null)
        {
            return;
        }

        using var reader = new StreamReader(stream);
        var html = reader.ReadToEnd();
        m_worldModel.PlayerUi.WriteHTML(html);
    }

    public override string Save()
    {
        return SaveScript("insert", m_filename.Save());
    }

    public override object GetParameter(int index)
    {
        return m_filename.Save();
    }

    protected override void SetParameterInternal(int index, object value)
    {
        m_filename = new Expression<string>((string) value, m_scriptContext);
    }
}