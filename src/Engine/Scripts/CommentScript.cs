#nullable disable
namespace QuestViva.Engine.Scripts;

public class CommentScriptConstructor : IScriptConstructor
{
    public string Keyword => "//";

    public IScript Create(string script, ScriptContext scriptContext)
    {
        return new CommentScript(script.Substring(2).Trim());
    }

    public IScriptFactory ScriptFactory
    {
        set { }
    }

    public WorldModel WorldModel { get; set; }
}

public class CommentScript : ScriptBase
{
    private string m_comment;

    public CommentScript(string comment)
    {
        m_comment = comment;
    }

    public override string Keyword => "//";

    protected override ScriptBase CloneScript()
    {
        return new CommentScript(m_comment);
    }

    public override Task ExecuteAsync(Context c)
    {
        return Task.CompletedTask;
    }

    public override string Save()
    {
        return "// " + string.Join(Environment.NewLine + "// ",
            m_comment.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries));
    }

    public override object GetParameter(int index)
    {
        return m_comment;
    }

    protected override void SetParameterInternal(int index, object value)
    {
        m_comment = (string) value;
    }

    public void AddLine(string line)
    {
        if (!line.StartsWith("//"))
        {
            throw new ArgumentException("Expected comment line: " + line);
        }

        m_comment += Environment.NewLine + line.Substring(2).Trim();
    }
}