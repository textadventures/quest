#nullable disable
namespace QuestViva.Engine.Scripts;

internal class FailedScript : ScriptBase
{
    private string _script;

    public FailedScript(string script)
    {
        _script = script;
    }

    public override string Keyword => "@failed";

    protected override ScriptBase CloneScript()
    {
        return new FailedScript(_script);
    }

    public override Task ExecuteAsync(Context c)
    {
        throw new NotImplementedException();
    }

    public override string Save()
    {
        return _script;
    }

    protected override void SetParameterInternal(int index, object value)
    {
        if (index != 0)
        {
            throw new ArgumentOutOfRangeException();
        }

        _script = (string) value;
    }

    public override object GetParameter(int index)
    {
        if (index != 0)
        {
            throw new ArgumentOutOfRangeException();
        }

        return _script;
    }
}