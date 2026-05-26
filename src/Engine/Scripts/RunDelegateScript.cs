#nullable disable
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class RunDelegateScriptConstructor : ScriptConstructorBase
{
    public override string Keyword => "rundelegate";

    protected override int[] ExpectedParameters
    {
        get { return new int[] { }; }
    }

    protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
    {
        if (parameters.Count < 2)
        {
            throw new Exception("Expected at least 2 parameters in rundelegate call");
        }

        var paramExpressions = new List<IFunction<object>>();
        IFunction<Element> obj = null;
        var cnt = 0;
        IFunction<string> delegateName = null;

        foreach (var param in parameters)
        {
            cnt++;
            switch (cnt)
            {
                case 1:
                    obj = new Expression<Element>(param, scriptContext);
                    break;
                case 2:
                    delegateName = new Expression<string>(param, scriptContext);
                    break;
                default:
                    paramExpressions.Add(new Expression<object>(param, scriptContext));
                    break;
            }
        }

        return new RunDelegateScript(scriptContext, obj, delegateName, paramExpressions);
    }
}

public class RunDelegateScript : ScriptBase
{
    private readonly FunctionCallParameters m_parameters;
    private readonly ScriptContext m_scriptContext;
    private readonly WorldModel m_worldModel;
    private IFunction<Element> m_appliesTo;
    private IFunction<string> m_delegate;

    public RunDelegateScript(ScriptContext scriptContext, IFunction<Element> obj, IFunction<string> del,
        IList<IFunction<object>> parameters)
    {
        m_scriptContext = scriptContext;
        m_worldModel = scriptContext.WorldModel;
        m_delegate = del;
        m_parameters = new FunctionCallParameters(m_worldModel, parameters);
        m_appliesTo = obj;
    }

    public override string Keyword => "rundelegate";

    protected override ScriptBase CloneScript()
    {
        return new RunDelegateScript(m_scriptContext, m_appliesTo.Clone(), m_delegate.Clone(), m_parameters.Parameters);
    }

    public override void Execute(Context c)
    {
        if (m_parameters == null)
        {
            throw new NotImplementedException();
        }

        var obj = m_appliesTo.Execute(c);
        var delName = m_delegate.Execute(c);
        var impl = obj.Fields.Get(delName) as DelegateImplementation;

        if (impl == null)
        {
            throw new Exception(
                string.Format("Object '{0}' has no delegate implementation '{1}'", obj.Name, m_delegate));
        }

        var paramValues = new Parameters();

        var cnt = 0;
        foreach (var f in m_parameters.Parameters)
        {
            paramValues.Add((string) impl.Definition.Fields[FieldDefinitions.ParamNames][cnt], f.Execute(c));
            cnt++;
        }

        m_worldModel.RunScript(impl.Implementation.Fields[FieldDefinitions.Script], paramValues, obj);
    }

    public override string Save()
    {
        var saveParameters = new List<string>();
        saveParameters.Add(m_appliesTo.Save());
        saveParameters.Add(m_delegate.Save());
        foreach (var p in m_parameters.ParametersAsQuestList)
        {
            saveParameters.Add(p);
        }

        return SaveScript("rundelegate", saveParameters.ToArray());
    }

    public override object GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return m_appliesTo.Save();
            case 1:
                return m_delegate.Save();
            case 2:
                return m_parameters.ParametersAsQuestList;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override void SetParameterInternal(int index, object value)
    {
        switch (index)
        {
            case 0:
                m_appliesTo = new Expression<Element>((string) value, m_scriptContext);
                break;
            case 1:
                m_delegate = new Expression<string>((string) value, m_scriptContext);
                break;
            case 2:
                // any updates to the parameters should change the list itself - nothing should cause SetParameter to be triggered.
                throw new InvalidOperationException(
                    "Attempt to use SetParameter to change the parameters of a 'rundelegate'");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}