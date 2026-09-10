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

    public WorldModel WorldModel { get; set; } = null!;
}

public class CommentScript : ScriptBase
{
    private string _comment;

    public CommentScript(string comment)
    {
        _comment = comment;
    }

    public override string Keyword => "//";

    protected override ScriptBase CloneScript()
    {
        return new CommentScript(_comment);
    }

    public override Task ExecuteAsync(Context c)
    {
        return Task.CompletedTask;
    }

    public override string Save()
    {
        return "// " + string.Join(Environment.NewLine + "// ",
            _comment.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries));
    }

    public override object? GetParameter(int index)
    {
        return _comment;
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        _comment = (string) value!;
    }

    public void AddLine(string line)
    {
        if (!line.StartsWith("//"))
        {
            throw new ArgumentException("Expected comment line: " + line);
        }

        _comment += Environment.NewLine + line.Substring(2).Trim();
    }
}