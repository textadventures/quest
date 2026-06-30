using QuestViva.Common;
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class ShowMenuScriptConstructor : IScriptConstructor
{
    public string Keyword => "show menu";

    public IScript Create(string script, ScriptContext scriptContext)
    {
        string afterExpr;
        var param = Utility.GetParameter(script, out afterExpr);
        var callback = Utility.GetScript(afterExpr);

        var parameters = Utility.SplitParameter(param).ToArray();
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
    private readonly IScript m_callbackScript;
    private readonly ScriptContext m_scriptContext;
    private readonly IScriptFactory m_scriptFactory;
    private readonly WorldModel m_worldModel;
    private IFunction<bool> m_allowCancel;
    private IFunction<string> m_caption;
    private IFunctionDynamic m_options;

    public ShowMenuScript(ScriptContext scriptContext, IScriptFactory scriptFactory, IFunction<string> caption,
        IFunctionDynamic options, IFunction<bool> allowCancel, IScript callbackScript)
    {
        m_scriptContext = scriptContext;
        m_worldModel = scriptContext.WorldModel;
        m_scriptFactory = scriptFactory;
        m_caption = caption;
        m_options = options;
        m_allowCancel = allowCancel;
        m_callbackScript = callbackScript;
    }

    public override string Keyword => "show menu";

    protected override ScriptBase CloneScript()
    {
        return new ShowMenuScript(m_scriptContext, m_scriptFactory, m_caption.Clone(), m_options.Clone(),
            m_allowCancel.Clone(), (IScript) m_callbackScript.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var caption = await m_caption.ExecuteAsync(c);
        var options = await m_options.ExecuteAsync(c);
        var allowCancel = await m_allowCancel.ExecuteAsync(c);

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

        await m_worldModel.PrintAsync(caption);
        var menuData = new MenuData(caption, optionsDictionary, allowCancel);
        m_worldModel.PlayerUi.ShowMenu(menuData);

        m_worldModel._menuTcs = new TaskCompletionSource<string?>();
        m_worldModel.BeginPendingCallback();
        m_worldModel.SignalTurnSuspended();
        _ = AwaitResponseAndRunCallbackAsync(c, optionsDictionary);
    }

    private async Task AwaitResponseAndRunCallbackAsync(Context c, IDictionary<string, string> optionsDictionary)
    {
        try
        {
            var response = await m_worldModel._menuTcs!.Task;
            if (response != null)
                await m_worldModel.PrintAsync(" - " + optionsDictionary[response]);
            c.Parameters["result"] = response;
            await m_worldModel.RunScriptAsync(m_callbackScript, c);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { m_worldModel.LogException(ex); }
        finally
        {
            await m_worldModel.EndPendingCallbackAsync();
            m_worldModel.SignalTurnSuspended();
        }
    }

    public override string Save()
    {
        return SaveScript("show menu", m_callbackScript, m_caption.Save(), m_options.Save(), m_allowCancel.Save());
    }

    public override object GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return m_caption.Save();
            case 1:
                return m_options.Save();
            case 2:
                return m_allowCancel.Save();
            case 3:
                return m_callbackScript;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object value)
    {
        switch (index)
        {
            case 0:
                m_caption = new Expression<string>((string) value, m_scriptContext);
                break;
            case 1:
                m_options = new ExpressionDynamic((string) value, m_scriptContext);
                break;
            case 2:
                m_allowCancel = new Expression<bool>((string) value, m_scriptContext);
                break;
            case 3:
                throw new InvalidOperationException(
                    "Attempt to use SetParameter to change the script of a 'show menu' command");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
