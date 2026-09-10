#nullable disable
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class UndoScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "undo";

    protected override int[] ExpectedParameters
    {
        get { return new[] {0}; }
    }

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new UndoScript(WorldModel);
    }
}

public class UndoScript : ScriptBase
{
    private readonly WorldModel _worldModel;

    public UndoScript(WorldModel worldModel)
    {
        _worldModel = worldModel;
    }

    public override string Keyword => "undo";

    protected override ScriptBase CloneScript()
    {
        return new UndoScript(_worldModel);
    }

    public override Task ExecuteAsync(Context c)
    {
        return _worldModel.UndoLogger.RollbackTransaction();
    }

    public override string Save()
    {
        return "undo";
    }

    public override object GetParameter(int index)
    {
        throw new ArgumentOutOfRangeException();
    }

    protected override void SetParameterInternal(int index, object value)
    {
        throw new ArgumentOutOfRangeException();
    }
}

public class StartTransactionConstructor : ScriptConstructorBase
{
    public override string Keyword => "start transaction";

    protected override int[] ExpectedParameters
    {
        get { return new[] {1}; }
    }

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new StartTransactionScript(scriptContext, new Expression<string>(parameters[0], scriptContext));
    }
}

public class StartTransactionScript : ScriptBase
{
    private readonly ScriptContext _scriptContext;
    private readonly WorldModel _worldModel;
    private IFunction<string> _command;

    public StartTransactionScript(ScriptContext scriptContext, IFunction<string> command)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _command = command;
    }

    public override string Keyword => "start transaction";

    protected override ScriptBase CloneScript()
    {
        return new StartTransactionScript(_scriptContext, _command.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        _worldModel.UndoLogger.RollTransaction(await _command.ExecuteAsync(c));
    }

    public override string Save()
    {
        return SaveScript("start transaction", _command.Save());
    }

    public override object GetParameter(int index)
    {
        return _command.Save();
    }

    protected override void SetParameterInternal(int index, object value)
    {
        _command = new Expression<string>((string) value, _scriptContext);
    }
}