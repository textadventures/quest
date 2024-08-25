using System;
using System.Linq;
using System.Reflection;
using NCalc;
using NCalc.Cache;
using NCalc.Factories;
using NCalc.Handlers;
using NCalc.Services;
using TextAdventures.Quest.Functions;
using TextAdventures.Quest.Scripts;

namespace TextAdventures.Quest.Expressions;

public class NcalcExpressionEvaluator<T>: IExpressionEvaluator<T>, IDynamicExpressionEvaluator
{
    private readonly ScriptContext _scriptContext;
    private readonly Expression _nCalcExpression;
    private readonly ExpressionOwner _expressionOwner;

    public NcalcExpressionEvaluator(string expression, ScriptContext scriptContext)
    {
        _scriptContext = scriptContext;
        _expressionOwner = new ExpressionOwner(scriptContext.WorldModel);

        // TODO: Implement our own ILogicalExpressionFactory, based on the default NCalc one
        _nCalcExpression = new Expression(expression,
            new ExpressionContext(ExpressionOptions.NoStringTypeCoercion, null),
            new LogicalExpressionFactory(),
            LogicalExpressionCache.GetInstance(),
            new EvaluationService());
        
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
        var result = _nCalcExpression.Evaluate();

        // Converting ints to generic doubles is fun
        if (typeof(T) == typeof(double) && result is int i)
        {
            return (T)(object)(double)i;
        }

        return (T)result;
    }

    private Context _context;

    private void EvaluateParameter(string name, ParameterArgs args)
    {
        var tryMath = EvaluateVariableFromType(typeof(Math), name);
        if (tryMath.handled)
        {
            args.Result = tryMath.result;
            return;
        }
        
        args.Result = ResolveVariable(name);
    }

    private object ResolveVariable(string name)
    {
        if (name == "null")
        {
            return null;
        }

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
        var tryExpressionOwner = EvaluateFunctionFromType(typeof(ExpressionOwner), _expressionOwner, name, args.Parameters);
        if (tryExpressionOwner.handled)
        {
            args.Result = tryExpressionOwner.result;
            return;
        }

        var tryMath = EvaluateFunctionFromType(typeof(Math), null, name, args.Parameters);
        if (tryMath.handled)
        {
            args.Result = tryMath.result;
            return;
        }

        var tryStringFunctions = EvaluateFunctionFromType(typeof(StringFunctions), null, name, args.Parameters);
        if (tryStringFunctions.handled)
        {
            args.Result = tryStringFunctions.result;
            return;
        }

        args.Result = EvaluateAslFunction(name, args);
    }

    private static (bool handled, object? result) EvaluateFunctionFromType(Type type, object instance, string name, Expression[] parameters)
    {
        var methods = type
            .GetMethods()
            .Cast<MethodBase>()
            .Where(m => m.IsPublic && m.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
            .ToArray();

        if (methods.Length == 0)
        {
            return (false, null);
        }

        var evaluatedArgs = parameters.Select(p => p.Evaluate()).ToArray();
        var types = evaluatedArgs.Select(a => a.GetType()).ToArray();

        var method = Type.DefaultBinder!.SelectMethod(BindingFlags.Default, methods, types, null);

        if (method == null)
        {
            throw new Exception($"{name} function does not handle parameters of types {string.Join(", ", types.Select(t => t.Name))}");
        }

        return (true, method.Invoke(instance, evaluatedArgs));
    }
    
    private static (bool handled, object result) EvaluateVariableFromType(Type type, string name)
    {
        var fields = type
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && !f.IsInitOnly)
            .ToArray();
        
        var field = fields.FirstOrDefault(f => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        
        if (field == null)
        {
            return (false, null);
        }
        
        return (true, field.GetRawConstantValue());
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