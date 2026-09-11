using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class CreateScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "create";

    protected override int[] ExpectedParameters => [1, 2];

    protected override IScript? CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        switch (parameters.Count)
        {
            case 1:
                return new CreateScript(scriptContext, new Expression<string>(parameters[0], scriptContext));
            case 2:
                return new CreateScript(scriptContext, new Expression<string>(parameters[0], scriptContext),
                    new Expression<string>(parameters[1], scriptContext));
        }

        return null;
    }
}

public class CreateScript : ScriptBase
{
    private readonly ScriptContext _scriptContext;
    private readonly WorldModel _worldModel;
    private IFunction<string> _expr;
    private IFunction<string>? _type;

    public CreateScript(ScriptContext scriptContext, IFunction<string> expr)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _expr = expr;
    }

    public CreateScript(ScriptContext scriptContext, IFunction<string> expr, IFunction<string> type)
        : this(scriptContext, expr)
    {
        _type = type;
    }

    public override string Keyword => "create";

    protected override ScriptBase CloneScript()
    {
        if (_type == null)
        {
            return new CreateScript(_scriptContext, _expr.Clone());
        }

        return new CreateScript(_scriptContext, _expr.Clone(), _type.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        if (_type == null)
        {
            _worldModel.ObjectFactory.CreateObject(await _expr.ExecuteAsync(c));
        }
        else
        {
            _worldModel.ObjectFactory.CreateObject(await _expr.ExecuteAsync(c), ObjectType.Object, true,
                [await _type.ExecuteAsync(c)]);
        }
    }

    public override string Save()
    {
        if (_type == null)
        {
            return SaveScript("create", _expr.Save());
        }

        return SaveScript("create", _expr.Save(), _type.Save());
    }

    public override object? GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return _expr.Save();
            case 1:
                return _type == null ? null : _type.Save();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        switch (index)
        {
            case 0:
                _expr = new Expression<string>((string) value!, _scriptContext);
                break;
            case 1:
                _type = value == null ? null : new Expression<string>((string) value, _scriptContext);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public class CreateExitScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "create exit";

    protected override int[] ExpectedParameters => [3, 4, 5];

    protected override IScript? CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        switch (parameters.Count)
        {
            case 3:
                return new CreateExitScript(scriptContext, new Expression<string>(parameters[0], scriptContext),
                    new Expression<Element>(parameters[1], scriptContext),
                    new Expression<Element>(parameters[2], scriptContext));
            case 4:
                return new CreateExitScript(scriptContext, new Expression<string>(parameters[0], scriptContext),
                    new Expression<Element>(parameters[1], scriptContext),
                    new Expression<Element>(parameters[2], scriptContext),
                    new Expression<string>(parameters[3], scriptContext));
            case 5:
                return new CreateExitScript(scriptContext, new Expression<string>(parameters[1], scriptContext),
                    new Expression<Element>(parameters[2], scriptContext),
                    new Expression<Element>(parameters[3], scriptContext),
                    new Expression<string>(parameters[4], scriptContext),
                    new Expression<string>(parameters[0], scriptContext));
        }

        return null;
    }
}

public class CreateExitScript : ScriptBase
{
    private readonly ScriptContext _scriptContext;
    private readonly WorldModel _worldModel;
    private IFunction<Element> _from;
    private IFunction<string>? _id;
    private IFunction<string>? _initialType;
    private IFunction<string> _name;
    private IFunction<Element> _to;

    public CreateExitScript(ScriptContext scriptContext, IFunction<string> name, IFunction<Element> from,
        IFunction<Element> to)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _name = name;
        _from = from;
        _to = to;
    }

    public CreateExitScript(ScriptContext scriptContext, IFunction<string> name, IFunction<Element> from,
        IFunction<Element> to, IFunction<string> initialType)
        : this(scriptContext, name, from, to)
    {
        _initialType = initialType;
    }

    public CreateExitScript(ScriptContext scriptContext, IFunction<string> name, IFunction<Element> from,
        IFunction<Element> to, IFunction<string> initialType, IFunction<string> id)
        : this(scriptContext, name, from, to, initialType)
    {
        _id = id;
    }

    public override string Keyword => "create exit";

    protected override ScriptBase CloneScript()
    {
        if (_initialType == null)
        {
            return new CreateExitScript(_scriptContext, _name.Clone(), _from.Clone(), _to.Clone());
        }

        if (_id == null)
        {
            return new CreateExitScript(_scriptContext, _name.Clone(), _from.Clone(), _to.Clone(),
                _initialType.Clone());
        }

        return new CreateExitScript(_scriptContext, _name.Clone(), _from.Clone(), _to.Clone(),
            _initialType.Clone(), _id.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        _worldModel.ObjectFactory.CreateExit(_id == null ? null : await _id.ExecuteAsync(c), await _name.ExecuteAsync(c),
            await _from.ExecuteAsync(c), await _to.ExecuteAsync(c), _initialType == null ? null : await _initialType.ExecuteAsync(c));
    }

    public override string Save()
    {
        if (_initialType == null)
        {
            return SaveScript("create exit", _name.Save(), _from.Save(), _to.Save());
        }

        if (_id == null)
        {
            return SaveScript("create exit", _name.Save(), _from.Save(), _to.Save(), _initialType.Save());
        }

        return SaveScript("create exit", _id.Save(), _name.Save(), _from.Save(), _to.Save(), _initialType.Save());
    }

    public override object? GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return _id == null ? null : _id.Save();
            case 1:
                return _name.Save();
            case 2:
                return _from.Save();
            case 3:
                return _to.Save();
            case 4:
                return _initialType == null ? null : _initialType.Save();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        switch (index)
        {
            case 0:
                _id = value == null ? null : new Expression<string>((string) value, _scriptContext);
                break;
            case 1:
                _name = new Expression<string>((string) value!, _scriptContext);
                break;
            case 2:
                _from = new Expression<Element>((string) value!, _scriptContext);
                break;
            case 3:
                _to = new Expression<Element>((string) value!, _scriptContext);
                break;
            case 4:
                _initialType = value == null ? null : new Expression<string>((string) value, _scriptContext);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public class CreateTimerScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "create timer";

    protected override int[] ExpectedParameters => [1];

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new CreateTimerScript(scriptContext, new Expression<string>(parameters[0], scriptContext));
    }
}

public class CreateTimerScript : ScriptBase
{
    private readonly ScriptContext _scriptContext;
    private readonly WorldModel _worldModel;
    private IFunction<string> _expr;

    public CreateTimerScript(ScriptContext scriptContext, IFunction<string> expr)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _expr = expr;
    }

    public override string Keyword => "create timer";

    protected override ScriptBase CloneScript()
    {
        return new CreateTimerScript(_scriptContext, _expr.Clone());
    }


    public override async Task ExecuteAsync(Context c)
    {
        _worldModel.GetElementFactory(ElementType.Timer).Create(await _expr.ExecuteAsync(c));
    }

    public override string Save()
    {
        return SaveScript("create timer", _expr.Save());
    }

    public override object? GetParameter(int index)
    {
        return _expr.Save();
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        _expr = new Expression<string>((string) value!, _scriptContext);
    }
}

public class CreateTurnScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "create turnscript";

    protected override int[] ExpectedParameters => [1];

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        return new CreateTurnScript(scriptContext, new Expression<string>(parameters[0], scriptContext));
    }
}

public class CreateTurnScript : ScriptBase
{
    private readonly ScriptContext _scriptContext;
    private readonly WorldModel _worldModel;
    private IFunction<string> _expr;

    public CreateTurnScript(ScriptContext scriptContext, IFunction<string> expr)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _expr = expr;
    }

    public override string Keyword => "create turnscript";

    protected override ScriptBase CloneScript()
    {
        return new CreateTurnScript(_scriptContext, _expr.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        _worldModel.ObjectFactory.CreateTurnScript(await _expr.ExecuteAsync(c), null);
    }

    public override string Save()
    {
        return SaveScript("create turnscript", _expr.Save());
    }

    public override object? GetParameter(int index)
    {
        return _expr.Save();
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        _expr = new Expression<string>((string) value!, _scriptContext);
    }
}