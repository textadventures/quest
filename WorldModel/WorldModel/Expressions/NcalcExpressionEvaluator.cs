using System;
using NCalc.Handlers;
using TextAdventures.Quest.Scripts;

namespace TextAdventures.Quest.Expressions;

public class NcalcExpressionEvaluator<T>: IExpressionEvaluator<T>, IDynamicExpressionEvaluator
{
    private readonly ScriptContext _scriptContext;
    private readonly NCalc.Expression _nCalcExpression;

    public NcalcExpressionEvaluator(string expression, ScriptContext scriptContext)
    {
        _scriptContext = scriptContext;
        _nCalcExpression = new NCalc.Expression(expression);
        _nCalcExpression.EvaluateFunction += EvaluateAslFunction;
        _nCalcExpression.EvaluateParameter += EvaluateParameter;
    }

    object IDynamicExpressionEvaluator.Evaluate(Context c)
    {
        return Evaluate(c);
    }

    public T Evaluate(Context c)
    {
        _context = c;
        return (T)_nCalcExpression.Evaluate();
    }

    private Context _context;

    private void EvaluateParameter(string name, ParameterArgs args)
    {
        args.Result = ResolveVariable(name);
    }

    private object ResolveVariable(string name)
    {
        if (_context.Parameters?.ContainsKey(name) == true)
        {
            return _context.Parameters[name];
        }

        if (_scriptContext.WorldModel.TryResolveExpressionElement(Utility.ResolveElementName(name), out var result))
        {
            return result;
        }

        ResolveVariableName(name, out var fields, out var variable);

        do
        {
            if (!Utility.ContainsUnresolvedDotNotation(variable))
            {
                continue;
            }

            // We may have been passed in something like someobj.parent.someproperty
            Utility.ResolveVariableName(ref variable, out var nestedObj, out variable);
            fields = fields.GetObject(nestedObj).Fields;
        } while (Utility.ContainsUnresolvedDotNotation(variable));

        return !fields.Exists(variable, true) ? null : fields.Get(variable);
    }

    private void ResolveVariableName(string name, out Fields fields, out string variable)
    {
        Utility.ResolveVariableName(ref name, out var obj, out variable);

        if (_scriptContext.WorldModel.TryResolveExpressionElement(name, out var result))
        {
            fields = result.Fields;
        }
        else
        {
            if (obj == null) throw new Exception($"Unknown object or variable '{name}'");

            var value = ResolveVariable(obj);
            if (value is not Element instance)
            {
                throw new Exception($"Variable does not refer to an object: '{obj}'");
            }
            fields = instance.Fields;
        }
    }

    private void EvaluateAslFunction(string name, FunctionArgs args)
    {
        if (name == "IsDefined")
        {
            args.Result = _context.Parameters.ContainsKey((string)args.Parameters[0].Evaluate());
        }

        var proc = _scriptContext.WorldModel.Procedure(name);
        var parameters = new Parameters();
        var cnt = 0;

        foreach (var val in args.Parameters)
        {
            parameters.Add((string)proc.Fields[FieldDefinitions.ParamNames][cnt], val.Evaluate());
            cnt++;
        }

        args.Result = _scriptContext.WorldModel.RunProcedure(name, parameters, true);
    }
}