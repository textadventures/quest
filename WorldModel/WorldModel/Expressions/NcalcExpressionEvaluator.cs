using System;
using System.Linq;
using NCalc.Handlers;
using TextAdventures.Quest.Functions;
using TextAdventures.Quest.Scripts;

namespace TextAdventures.Quest.Expressions;

public class NcalcExpressionEvaluator<T>: IExpressionEvaluator<T>, IDynamicExpressionEvaluator
{
    private readonly ScriptContext _scriptContext;
    private readonly NCalc.Expression _nCalcExpression;
    private readonly ExpressionOwner _expressionOwner;

    public NcalcExpressionEvaluator(string expression, ScriptContext scriptContext)
    {
        _scriptContext = scriptContext;
        _expressionOwner = new ExpressionOwner(scriptContext.WorldModel);
        _nCalcExpression = new NCalc.Expression(expression);
        _nCalcExpression.EvaluateFunction += EvaluateFunction;
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

    private void EvaluateFunction(string name, FunctionArgs args)
    {
        var expressionOwnerCandidates = _expressionOwner.GetFunction(name);
        if (expressionOwnerCandidates.Length != 0)
        {
            var evaluatedArgs = args.Parameters.Select(p => p.Evaluate()).ToArray();
            var types = evaluatedArgs.Select(a => a.GetType()).ToArray();

            var filteredExpressionOwnerCandidates = expressionOwnerCandidates
                .Where(
                    c => IsFunctionCallableWithTypes(
                        c.GetParameters().Select(p => p.ParameterType).ToArray(),
                        types))
                .ToArray();

            if (filteredExpressionOwnerCandidates.Length == 0)
            {
                throw new Exception($"{name} function does not handle parameters of types {string.Join(", ", types.Select(t => t.Name))}");
            }

            if (filteredExpressionOwnerCandidates.Length != 1)
            {
                throw new Exception($"Ambiguous function call to {name}");
            }

            args.Result = filteredExpressionOwnerCandidates[0].Invoke(_expressionOwner, evaluatedArgs);
            return;
        }
        args.Result = EvaluateAslFunction(name, args);
    }

    private bool IsFunctionCallableWithTypes(Type[] functionTypes, Type[] inputTypes)
    {
        return functionTypes.Length == inputTypes.Length && functionTypes.Zip(inputTypes).All(t => t.First.IsAssignableFrom(t.Second));
    }

    private object? EvaluateAslFunction(string name, FunctionArgs args)
    {
        if (name == "IsDefined")
        {
            if (args.Parameters.Length != 1)
            {
                throw new Exception("IsDefined function expects 1 parameter");
            }

            if (args.Parameters[0].Evaluate() is not string variableName)
            {
                throw new Exception("IsDefined function expects a string parameter");
            }

            return _context.Parameters.ContainsKey(variableName);
        }

        var proc = _scriptContext.WorldModel.Procedure(name);

        if (proc == null)
        {
            throw new Exception($"Unknown function '{name}'");
        }

        var parameters = new Parameters();
        var cnt = 0;

        foreach (var val in args.Parameters)
        {
            parameters.Add((string)proc.Fields[FieldDefinitions.ParamNames][cnt], val.Evaluate());
            cnt++;
        }

        return _scriptContext.WorldModel.RunProcedure(name, parameters, true);
    }
}