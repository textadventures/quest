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
        value = value.Trim();
        var dotPos = value.LastIndexOf('.');
        if (dotPos == -1)
        {
            variable = Utility.ResolveElementName(value);
            return null;
        }
        variable = value[(dotPos + 1)..];
        return new Expression<Element>(value[..dotPos], scriptContext);
    }
}

public abstract class SetScriptBase : ScriptBase
{
    private IFunction<Element> _appliesTo;
    private string _property;
    protected ScriptContext _scriptContext;

    internal SetScriptBase(SetScriptConstructor constructor, ScriptContext scriptContext, IFunction<Element> appliesTo,
        string property)
    {
        Constructor = constructor;
        WorldModel = constructor.WorldModel;
        _scriptContext = scriptContext;
        AppliesTo = appliesTo;
        Property = property;
    }

    protected IFunction<Element> AppliesTo
    {
        get => _appliesTo;
        private set
        {
            _appliesTo = value;
            AddAttributeNameToWorldModel();
        }
    }

    protected string Property
    {
        get => _property;
        private set
        {
            _property = value;
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
                AppliesTo = Constructor.GetAppliesTo(_scriptContext, (string) value, out variable);
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
    private Expression<object> _expr;

    public SetExpressionScript(SetScriptConstructor constructor, ScriptContext scriptContext,
        IFunction<Element> appliesTo, string property, Expression<object> expr)
        : base(constructor, scriptContext, appliesTo, property)
    {
        _expr = expr;
    }

    protected override string GetEqualsString => " = ";

    public override string Keyword => "=";

    protected override ScriptBase CloneScript()
    {
        return new SetExpressionScript(Constructor, _scriptContext, AppliesTo == null ? null : AppliesTo.Clone(),
            Property, (Expression<object>) _expr.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        var result = await _expr.ExecuteAsync(c);
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
        return _expr.Save();
    }

    protected override void SetValue(string newValue)
    {
        _expr = new Expression<object>(newValue, _scriptContext);
    }

    protected override object GetValue()
    {
        return _expr.Save();
    }
}

public class SetScriptScript : SetScriptBase
{
    private readonly IScriptFactory _scriptFactory;
    private IScript _script;

    public SetScriptScript(SetScriptConstructor constructor, ScriptContext scriptContext, IFunction<Element> appliesTo,
        string property, IScript script)
        : base(constructor, scriptContext, appliesTo, property)
    {
        _script = script;
        _scriptFactory = constructor.ScriptFactory;
    }

    protected override string GetEqualsString => " => ";

    public override string Keyword => "=>";

    protected override ScriptBase CloneScript()
    {
        return new SetScriptScript(Constructor, _scriptContext, AppliesTo == null ? null : AppliesTo.Clone(), Property,
            (IScript) _script.Clone());
    }

    public override async Task ExecuteAsync(Context c)
    {
        if (AppliesTo != null)
        {
            // we're setting an object property
            var obj = await AppliesTo.ExecuteAsync(c);
            await obj.SetFieldAsync(Property, _script);
        }
        else
        {
            // we're setting a local variable
            c.Parameters[Property] = _script;
        }
    }

    protected override string GetSaveString()
    {
        return SaveScript("", _script).Trim();
    }

    protected override void SetValue(string newValue)
    {
        _script = _scriptFactory.CreateScript(newValue);
    }

    protected override object GetValue()
    {
        return _script;
    }
}