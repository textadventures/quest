using System;
using System.Linq;
using System.Reflection;
using NCalc;
using NCalc.Cache;
using NCalc.Handlers;
using NCalc.Services;
using QuestViva.Engine.Functions;
using QuestViva.Engine.Scripts;

namespace QuestViva.Engine.Expressions;

public class NcalcExpressionEvaluator<T>: IExpressionEvaluator<T>, IDynamicExpressionEvaluator
{
    private readonly ScriptContext _scriptContext;
    private readonly Expression _nCalcExpression;
    private readonly ExpressionOwner _expressionOwner;

    public NcalcExpressionEvaluator(string expression, ScriptContext scriptContext)
    {
        _scriptContext = scriptContext;
        _expressionOwner = new ExpressionOwner(scriptContext.WorldModel);
        
        _nCalcExpression = new Expression(expression,
            new ExpressionContext(ExpressionOptions.NoStringTypeCoercion, null),
            QuestNCalcExpressionFactory.GetInstance(),
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

    #nullable enable
    private static (bool handled, object? result) EvaluateFunctionFromType(Type type, object instance, string name, Expression[] parameters)
    {
        var methods = type.GetMethods()
            .Where(m => m.IsPublic && m.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
            .ToArray<MethodBase>();

        if (methods.Length == 0)
        {
            return (false, null);
        }

        var evaluatedArgs = parameters.Select(p => p.Evaluate()).ToArray();

        // First, try to find a method with a params parameter.
        var paramsMethods = methods
            .Where(m =>
            {
                var ps = m.GetParameters();
                return ps.Length > 0 &&
                       ps.Last().IsDefined(typeof(ParamArrayAttribute), false);
            })
            .ToArray();

        foreach (var method in paramsMethods)
        {
            var ps = method.GetParameters();
            var fixedParamCount = ps.Length - 1; // the last parameter is the params array

            // We must have at least as many arguments as the fixed parameters.
            if (evaluatedArgs.Length < fixedParamCount)
                continue;

            var fixedArgsMatch = true;
            for (var i = 0; i < fixedParamCount; i++)
            {
                if (evaluatedArgs[i] == null ||
                    ps[i].ParameterType.IsInstanceOfType(evaluatedArgs[i]))
                {
                    continue;
                }

                fixedArgsMatch = false;
                break;
            }

            if (!fixedArgsMatch)
                continue;

            // We assume here that all extra arguments (if any) can be converted to the element type.
            var elementType = ps.Last().ParameterType.GetElementType()!;
            var paramsCount = evaluatedArgs.Length - fixedParamCount;
            var paramsArray = Array.CreateInstance(elementType, paramsCount);
            for (var i = 0; i < paramsCount; i++)
            {
                var arg = evaluatedArgs[fixedParamCount + i];
                if (arg != null && !elementType.IsInstanceOfType(arg))
                {
                    fixedArgsMatch = false;
                    break;
                }

                paramsArray.SetValue(arg, i);
            }

            if (!fixedArgsMatch)
                continue;

            // Build the new arguments list: fixed parameters + one array for the params parameter.
            var newArgs = evaluatedArgs.Take(fixedParamCount)
                .Concat([paramsArray])
                .ToArray();
            return (true, method.Invoke(instance, newArgs));
        }

        // If no params-method was found that can handle these arguments, try a normal method.
        var types = evaluatedArgs.Select(a => a?.GetType() ?? typeof(object)).ToArray();
        var methodNoParams = Type.DefaultBinder!.SelectMethod(BindingFlags.Default, methods, types, null);

        if (methodNoParams == null)
        {
            throw new Exception(
                $"{name} function does not handle parameters of types {string.Join(", ", types.Select(t => t.Name))}");
        }

        return (true, methodNoParams.Invoke(instance, evaluatedArgs));
    }
    #nullable restore

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