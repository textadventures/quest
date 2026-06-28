#nullable disable
using QuestViva.Engine.Functions;

namespace QuestViva.Engine.Scripts;

public class SetScriptConstructor : IScriptConstructor
{
    public string Keyword => "=";

    public IScript Create(string script, ScriptContext scriptContext)
    {
        var isScript = false;
        var offset = 0;
        int eqPos;

        // hide text within string expressions
        var obscuredScript = Utility.ObscureStrings(script);
        var bracePos = obscuredScript.IndexOf('{');
        if (bracePos != -1)
        {
            // only want to look for = and => before any other scripts which may
            // be defined on the same line, for example procedure calls of type
            //     MyProcedureCall (5) { some other script }

            obscuredScript = obscuredScript.Substring(0, bracePos);
        }

        eqPos = obscuredScript.IndexOf("=>");
        if (eqPos != -1)
        {
            isScript = true;
            offset = 1;
        }
        else
        {
            eqPos = obscuredScript.IndexOf('=');
        }

        if (eqPos != -1)
        {
            var appliesTo = script.Substring(0, eqPos);
            var value = script.Substring(eqPos + 1 + offset).Trim();

            string variable;
            var expr = GetAppliesTo(scriptContext, appliesTo, out variable);

            if (!WorldModel.EditMode && WorldModel.Version >= WorldModelVersion.v530)
            {
                if (expr == null)
                {
                    if (!Utility.IsValidFieldName(variable))
                    {
                        var error = string.Format("Invalid variable name '{0}' in '{1}'", variable, script);
                        throw new Exception(error);
                    }
                }
                else
                {
                    if (!Utility.IsValidAttributeName(variable))
                    {
                        var error = string.Format("Invalid attribute name '{0}' in '{1}'", variable, script);
                        throw new Exception(error);
                    }
                }
            }

            if (!isScript)
            {
                return new SetExpressionScript(this, scriptContext, expr, variable,
                    new Expression<object>(value, scriptContext));
            }

            return new SetScriptScript(this, scriptContext, expr, variable, ScriptFactory.CreateScript(value));
        }

        return null;
    }

    public IScriptFactory ScriptFactory { get; set; }

    public WorldModel WorldModel { get; set; }

    internal IFunction<Element> GetAppliesTo(ScriptContext scriptContext, string value, out string variable)
    {
        var var = Utility.ConvertVariablesToFleeFormat(value).Trim();
        string obj;
        Utility.ResolveObjectDotAttribute(var, out obj, out variable);
        return obj == null ? null : new Expression<Element>(obj, scriptContext);
    }
}

public abstract class SetScriptBase : ScriptBase
{
    private IFunction<Element> m_appliesTo;
    private string m_property;
    protected ScriptContext m_scriptContext;

    internal SetScriptBase(SetScriptConstructor constructor, ScriptContext scriptContext, IFunction<Element> appliesTo,
        string property)
    {
        Constructor = constructor;
        WorldModel = constructor.WorldModel;
        m_scriptContext = scriptContext;
        AppliesTo = appliesTo;
        Property = property;
    }

    protected IFunction<Element> AppliesTo
    {
        get => m_appliesTo;
        private set
        {
            m_appliesTo = value;
            AddAttributeNameToWorldModel();
        }
    }

    protected string Property
    {
        get => m_property;
        private set
        {
            m_property = value;
            AddAttributeNameToWorldModel();
        }
    }

    protected abstract string GetEqualsString { get; }
    protected WorldModel WorldModel { get; }

    protected SetScriptConstructor Constructor { get; }

    private void AddAttributeNameToWorldModel()
    {
        if (AppliesTo != null && Property != null)
        {
            WorldModel.AddAttributeName(Property);
        }
    }

    public override string Save()
    {
        string result;

        if (AppliesTo != null)
        {
            result = AppliesTo.Save() + "." + Property;
        }
        else
        {
            result = Property;
        }

        result += GetEqualsString + GetSaveString();

        return result;
    }

    public override object GetParameter(int index)
    {
        switch (index)
        {
            case 0:
                return AppliesTo == null ? Property : AppliesTo.Save() + "." + Property;
            case 1:
                return GetValue();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void SetParameterInternal(int index, object value)
    {
        switch (index)
        {
            case 0:
                string variable;
                AppliesTo = Constructor.GetAppliesTo(m_scriptContext, (string) value, out variable);
                Property = variable;
                break;
            case 1:
                SetValue((string) value);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override IEnumerable<string> GetDefinedVariables()
    {
        if (AppliesTo == null)
        {
            // If AppliesTo is null, then this is an expression setting a simple variable value
            return new List<string> {Property};
        }

        return base.GetDefinedVariables();
    }

    protected abstract string GetSaveString();
    protected abstract object GetValue();
    protected abstract void SetValue(string newValue);
}

public class SetExpressionScript : SetScriptBase
{
    private Expression<object> m_expr;

    public SetExpressionScript(SetScriptConstructor constructor, ScriptContext scriptContext,
        IFunction<Element> appliesTo, string property, Expression<object> expr)
        : base(constructor, scriptContext, appliesTo, property)
    {
        m_expr = expr;
    }

    protected override string GetEqualsString => " = ";

    public override string Keyword => "=";

    protected override ScriptBase CloneScript()
    {
        return new SetExpressionScript(Constructor, m_scriptContext, AppliesTo == null ? null : AppliesTo.Clone(),
            Property, (Expression<object>) m_expr.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var result = await m_expr.ExecuteAsync(c);
        if (AppliesTo != null)
        {
            // we're setting an object property
            var obj = await AppliesTo.ExecuteAsync(c);
            await obj.SetFieldAsync(Property, result);
        }
        else
        {
            // we're setting a local variable
            c.Parameters[Property] = result;
        }
    }

    protected override string GetSaveString()
    {
        return m_expr.Save();
    }

    protected override void SetValue(string newValue)
    {
        m_expr = new Expression<object>(newValue, m_scriptContext);
    }

    protected override object GetValue()
    {
        return m_expr.Save();
    }
}

public class SetScriptScript : SetScriptBase
{
    private readonly IScriptFactory m_scriptFactory;
    private IScript m_script;

    public SetScriptScript(SetScriptConstructor constructor, ScriptContext scriptContext, IFunction<Element> appliesTo,
        string property, IScript script)
        : base(constructor, scriptContext, appliesTo, property)
    {
        m_script = script;
        m_scriptFactory = constructor.ScriptFactory;
    }

    protected override string GetEqualsString => " => ";

    public override string Keyword => "=>";

    protected override ScriptBase CloneScript()
    {
        return new SetScriptScript(Constructor, m_scriptContext, AppliesTo == null ? null : AppliesTo.Clone(), Property,
            (IScript) m_script.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        if (AppliesTo != null)
        {
            // we're setting an object property
            var obj = await AppliesTo.ExecuteAsync(c);
            await obj.SetFieldAsync(Property, m_script);
        }
        else
        {
            // we're setting a local variable
            c.Parameters[Property] = m_script;
        }
    }

    protected override string GetSaveString()
    {
        return SaveScript("", m_script).Trim();
    }

    protected override void SetValue(string newValue)
    {
        m_script = m_scriptFactory.CreateScript(newValue);
    }

    protected override object GetValue()
    {
        return m_script;
    }
}