#nullable disable
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class PictureScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "picture";

    protected override int[] ExpectedParameters
    {
        get { return new[] {1}; }
    }

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new PictureScript(scriptContext, new Expression<string>(parameters[0], scriptContext));
    }
}

public class PictureScript : ScriptBase
{
    private readonly ScriptContext m_scriptContext;
    private readonly WorldModel m_worldModel;
    private IFunction<string> m_filename;

    public PictureScript(ScriptContext scriptContext, IFunction<string> function)
    {
        m_scriptContext = scriptContext;
        m_worldModel = scriptContext.WorldModel;
        m_filename = function;
    }

    public override string Keyword => "picture";

    protected override ScriptBase CloneScript()
    {
        return new PictureScript(m_scriptContext, m_filename.Clone());
    }

    public override void Execute(Context c)
    {
        var filename = m_filename.Execute(c);

        if (m_worldModel.Version >= WorldModelVersion.v540)
        {
            m_worldModel.Print("<img src=\"" + m_worldModel.ExpressionOwner.GetFileURL(filename) + "\" />");
        }
        else
        {
            m_worldModel.PlayerUi.ShowPicture(filename);
            ((LegacyOutputLogger) m_worldModel.OutputLogger).AddPicture(filename);
        }
    }

    public override string Save()
    {
        return SaveScript("picture", m_filename.Save());
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