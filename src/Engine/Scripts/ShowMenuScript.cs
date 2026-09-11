using QuestViva.Common;
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class ShowMenuScriptConstructor : IScriptConstructor
{
    public string Keyword => "show menu";

    public IScript Create(string script, ScriptContext scriptContext)
    {
        string? afterExpr;
        var param = Utility.GetParameter(script, out afterExpr);
        var callback = Utility.GetScript(afterExpr!);

        var parameters = Utility.SplitParameter(param!).ToArray();
        if (parameters.Count() != 3)
        {
            throw new Exception(string.Format("'show menu' script should have 3 parameters: 'show menu ({0})'", param));
        }

        var callbackScript = ScriptFactory.CreateScript(callback);

        return new ShowMenuScript(scriptContext, ScriptFactory, new Expression<string>(parameters[0], scriptContext),
            new ExpressionDynamic(parameters[1], scriptContext), new Expression<bool>(parameters[2], scriptContext),
            callbackScript);
    }

    public IScriptFactory ScriptFactory { get; set; } = null!;

    public WorldModel WorldModel { get; set; } = null!;
}

public class ShowMenuScript : ScriptBase
{
    private readonly IScript _callbackScript;
    private readonly ScriptContext _scriptContext;
    private readonly IScriptFactory _scriptFactory;
    private readonly WorldModel _worldModel;
    private IFunction<bool> _allowCancel;
    private IFunction<string> _caption;
    private IFunctionDynamic _options;

    public ShowMenuScript(ScriptContext scriptContext, IScriptFactory scriptFactory, IFunction<string> caption,
        IFunctionDynamic options, IFunction<bool> allowCancel, IScript callbackScript)
    {
        _scriptContext = scriptContext;
        _worldModel = scriptContext.WorldModel;
        _scriptFactory = scriptFactory;
        _caption = caption;
        _options = options;
        _allowCancel = allowCancel;
        _callbackScript = callbackScript;
    }

    public override string Keyword => "show menu";

    protected override ScriptBase CloneScript()
    {
        return new ShowMenuScript(_scriptContext, _scriptFactory, _caption.Clone(), _options.Clone(),
            _allowCancel.Clone(), (IScript) _callbackScript.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var caption = await _caption.ExecuteAsync(c);
        var options = await _options.ExecuteAsync(c);
        var allowCancel = await _allowCancel.ExecuteAsync(c);

        IDictionary<string, string> optionsDictionary;
        if (options is IList<string> stringListOptions)
        {
            if (stringListOptions.Count == 0) throw new Exception("No menu options specified");
            optionsDictionary = stringListOptions.ToDictionary(o => o);
        }
        else if (options is IDictionary<string, string> stringDictionaryOptions)
        {
            if (stringDictionaryOptions.Count == 0) throw new Exception("No menu options specified");
            optionsDictionary = stringDictionaryOptions;
        }
        else
        {
            throw new Exception("Unknown menu options type");
        }

        await _worldModel.PrintAsync(caption);
        var menuData = new MenuData(caption, optionsDictionary, allowCancel);
        _worldModel.PlayerUi.ShowMenu(menuData);

        WorldModel.BeginPrompt(ref _worldModel._menuTcs);
        _worldModel.BeginDormantSuspension();
        _worldModel.SignalTurnSuspended();
        _ = AwaitResponseAndRunCallbackAsync(c, optionsDictionary);
    }

    private async Task AwaitResponseAndRunCallbackAsync(Context c, IDictionary<string, string> optionsDictionary)
    {
        var resolved = false;
        try
        {
            var response = await _worldModel._menuTcs!.Task;
            resolved = true;
            _worldModel.SignalCallbackResolving();
            if (response != null)
                await _worldModel.PrintAsync(" - " + optionsDictionary[response]);
            c.Parameters!["result"] = response;
            await _worldModel.RunScriptAsync(_callbackScript, c);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { _worldModel.LogException(ex); }
        finally
        {
            if (!resolved) _worldModel.SignalCallbackResolving();
            await _worldModel.EndPendingCallbackAsync();
            _worldModel.SignalTurnSuspended();
        }
    }

    public override string Save()
    {
        return SaveScript("show menu", _callbackScript, _caption.Save(), _options.Save(), _allowCancel.Save());
    }

    public override object? GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return _caption.Save();
            case 1:
                return _options.Save();
            case 2:
                return _allowCancel.Save();
            case 3:
                return _callbackScript;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object? value)
    {
        switch (index)
        {
            case 0:
                _caption = new Expression<string>((string) value!, _scriptContext);
                break;
            case 1:
                _options = new ExpressionDynamic((string) value!, _scriptContext);
                break;
            case 2:
                _allowCancel = new Expression<bool>((string) value!, _scriptContext);
                break;
            case 3:
                throw new InvalidOperationException(
                    "Attempt to use SetParameter to change the script of a 'show menu' command");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
