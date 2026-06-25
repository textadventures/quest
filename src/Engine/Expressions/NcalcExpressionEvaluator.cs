#nullable disable
using System.Reflection;
using NCalc;
using NCalc.Cache;
using NCalc.Factories;
using NCalc.Handlers;
using QuestViva.Engine;
using QuestViva.Engine.Functions;
using QuestViva.Engine.Scripts;

namespace QuestViva.Engine.Expressions;

public class NcalcExpressionEvaluator<T> : IExpressionEvaluator<T>, IDynamicExpressionEvaluator
{
    private readonly ScriptContext _scriptContext;
    private readonly Expression _nCalcExpression;
    private readonly ExpressionOwner _expressionOwner;
    private readonly string _expression;

    public NcalcExpressionEvaluator(string expression, ScriptContext scriptContext)
    {
        _scriptContext = scriptContext;
        _expressionOwner = new ExpressionOwner(scriptContext.WorldModel);
        _expression = Utility.ConvertFleeFormatToVariables(expression);

        _nCalcExpression = new Expression(expression,
            new ExpressionContext { Options = ExpressionOptions.NoStringTypeCoercion },
            QuestNCalcExpressionFactory.GetInstance(),
            LogicalExpressionCache.GetInstance());

        _nCalcExpression.EvaluateFunction += EvaluateFunction;
        _nCalcExpression.EvaluateParameter += EvaluateParameter;
        _nCalcExpression.EvaluateBinary += EvaluateBinary;
        _nCalcExpression.EvaluateAsyncFunction += EvaluateFunctionAsync;
        _nCalcExpression.EvaluateBinaryAsync += EvaluateBinaryAsync;
    }

    object IDynamicExpressionEvaluator.Evaluate(Context c)
    {
        return Evaluate(c);
    }

    async Task<object> IDynamicExpressionEvaluator.EvaluateAsync(Context c)
    {
        return await EvaluateAsync(c);
    }

    public T Evaluate(Context c)
    {
        _context = c;
        try
        {
            var result = CoerceLong(_nCalcExpression.Evaluate());

            // Converting ints to generic doubles is fun
            if (typeof(T) == typeof(double) && result is int i)
            {
                return (T) (object) (double) i;
            }

            return (T) result;
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            throw new Exception($"Error evaluating expression '{_expression}': {innerMessage}", ex);
        }
    }

    public async Task<T> EvaluateAsync(Context c)
    {
        _context = c;
        try
        {
            var result = CoerceLong(await _nCalcExpression.EvaluateAsync());

            if (typeof(T) == typeof(double) && result is int i)
            {
                return (T) (object) (double) i;
            }

            return (T) result;
        }
        catch (Exception ex)
        {
            var innerMessage = ex.InnerException?.Message ?? ex.Message;
            throw new Exception($"Error evaluating expression '{_expression}': {innerMessage}", ex);
        }
    }

    // NCalc returns Int64 for integer literals; coerce to Int32 to match engine expectations.
    private static object CoerceLong(object value)
    {
        return value is long l ? (int) l : value;
    }

    private Context _context;

    // Set to true while evaluating cast()'s type argument so EvaluateParameter
    // returns the identifier name as a string rather than trying to resolve it as a variable.
    private bool _evaluatingCastType;

    private void EvaluateParameter(string name, ParameterEventArgs args)
    {
        var tryMath = EvaluateVariableFromType(typeof(Math), name);
        if (tryMath.handled)
        {
            args.Result = tryMath.result;
            return;
        }

        if (_evaluatingCastType)
        {
            args.Result = name.ToLowerInvariant();
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
            if (obj == null)
            {
                throw new Exception($"Unknown object or variable '{name}'");
            }

            var value = ResolveVariable(obj);
            if (value is not Element instance)
            {
                throw new Exception($"Variable does not refer to an object: '{obj}'");
            }

            fields = instance.Fields;
        }
    }

    private void EvaluateFunction(string name, FunctionEventArgs args)
    {
        var tryExpressionOwner =
            EvaluateFunctionFromType(typeof(ExpressionOwner), _expressionOwner, name, args.Parameters);
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

        var tryDateTimeFunctions = EvaluateFunctionFromType(typeof(DateTimeFunctions), null, name, args.Parameters);
        if (tryDateTimeFunctions.handled)
        {
            args.Result = tryDateTimeFunctions.result;
            return;
        }

        args.Result = EvaluateAslFunction(name, args);
    }

    private async Task EvaluateFunctionAsync(string name, FunctionEventArgs args)
    {
        var tryExpressionOwner =
            await EvaluateFunctionFromTypeAsync(typeof(ExpressionOwner), _expressionOwner, name, args.Parameters);
        if (tryExpressionOwner.handled)
        {
            args.Result = tryExpressionOwner.result;
            return;
        }

        var tryMath = await EvaluateFunctionFromTypeAsync(typeof(Math), null, name, args.Parameters);
        if (tryMath.handled)
        {
            args.Result = tryMath.result;
            return;
        }

        var tryStringFunctions = await EvaluateFunctionFromTypeAsync(typeof(StringFunctions), null, name, args.Parameters);
        if (tryStringFunctions.handled)
        {
            args.Result = tryStringFunctions.result;
            return;
        }

        var tryDateTimeFunctions = await EvaluateFunctionFromTypeAsync(typeof(DateTimeFunctions), null, name, args.Parameters);
        if (tryDateTimeFunctions.handled)
        {
            args.Result = tryDateTimeFunctions.result;
            return;
        }

        args.Result = await EvaluateAslFunctionAsync(name, args);
    }

#nullable enable
    private static (bool handled, object? result) EvaluateFunctionFromType(Type type, object? instance, string name,
        FunctionData parameters)
    {
        var methods = GetPublicMethodsByName(type, name);
        if (methods == null) return (false, null);

        var evaluatedArgs = Enumerable.Range(0, parameters.Count)
            .Select(i => CoerceLong(parameters.Evaluate(i)))
            .ToArray();

        return DispatchToMethod(methods, instance, name, evaluatedArgs);
    }

    private static async Task<(bool handled, object? result)> EvaluateFunctionFromTypeAsync(Type type, object? instance,
        string name, FunctionData parameters)
    {
        var methods = GetPublicMethodsByName(type, name);
        if (methods == null) return (false, null);

        var evaluatedArgs = await Task.WhenAll(
            Enumerable.Range(0, parameters.Count)
                .Select(async i => CoerceLong(await parameters.EvaluateAsync(i))));

        return DispatchToMethod(methods, instance, name, evaluatedArgs);
    }

    private static MethodBase[]? GetPublicMethodsByName(Type type, string name)
    {
        var methods = type.GetMethods()
            .Where(m => m.IsPublic && m.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
            .ToArray<MethodBase>();
        return methods.Length == 0 ? null : methods;
    }

    private static (bool handled, object? result) DispatchToMethod(MethodBase[] methods, object? instance, string name,
        object?[] evaluatedArgs)
    {
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
            {
                continue;
            }

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
            {
                continue;
            }

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
            {
                continue;
            }

            // Build the new arguments list: fixed parameters + one array for the params parameter.
            var newArgs = evaluatedArgs.Take(fixedParamCount)
                .Concat([paramsArray])
                .ToArray();
            return (true, method.Invoke(instance, newArgs));
        }

        // If no params-method was found that can handle these arguments, try a normal method.
        var types = evaluatedArgs.Select(a => a?.GetType() ?? typeof(object)).ToArray();
        var methodNoParams = Type.DefaultBinder!.SelectMethod(BindingFlags.Default, methods, types, null);

        // DefaultBinder can't match when null args produce typeof(object) as a stand-in type.
        // Fall back to the first overload where all non-null args are type-compatible.
        if (methodNoParams == null && evaluatedArgs.Any(a => a == null))
        {
            methodNoParams = methods.OfType<MethodBase>().FirstOrDefault(m =>
            {
                var ps = m.GetParameters();
                if (ps.Length != evaluatedArgs.Length) return false;
                for (var i = 0; i < evaluatedArgs.Length; i++)
                {
                    if (evaluatedArgs[i] == null)
                    {
                        if (ps[i].ParameterType.IsValueType &&
                            Nullable.GetUnderlyingType(ps[i].ParameterType) == null)
                            return false;
                        continue;
                    }
                    if (!ps[i].ParameterType.IsInstanceOfType(evaluatedArgs[i])) return false;
                }
                return true;
            });
        }

        if (methodNoParams == null)
        {
            throw new Exception(
                $"{name} function does not handle parameters of types {string.Join(", ", types.Select(t => t.Name))}");
        }

        return (true, methodNoParams.Invoke(instance, evaluatedArgs));
    }
#nullable disable

    // FLEE compiled expressions to IL so C# operator overloads (e.g. QuestList<T> - Element)
    // resolved automatically. NCalc evaluates at runtime and doesn't use operator overloads,
    // so we intercept binary operations on non-standard types and dispatch via reflection.
    private void EvaluateBinary(BinaryEventArgs args)
    {
        // Check operator type BEFORE evaluating args to preserve short-circuit evaluation
        // for And/Or: calling LeftValue()/RightValue() forces eager evaluation of both sides.
        var isEquality = args.BinaryExpression.Type is BinaryExpressionType.Equal or BinaryExpressionType.NotEqual;

        var operatorName = args.BinaryExpression.Type switch
        {
            BinaryExpressionType.Plus => "op_Addition",
            BinaryExpressionType.Minus => "op_Subtraction",
            BinaryExpressionType.Times => "op_Multiply",
            BinaryExpressionType.Div => "op_Division",
            BinaryExpressionType.Modulo => "op_Modulus",
            _ => null
        };

        if (operatorName == null && !isEquality) return;

        var left = args.LeftValue();
        var right = args.RightValue();

        HandleBinaryResult(args, isEquality, operatorName, left, right);
    }

    private async Task EvaluateBinaryAsync(BinaryEventArgs args)
    {
        var isEquality = args.BinaryExpression.Type is BinaryExpressionType.Equal or BinaryExpressionType.NotEqual;

        var operatorName = args.BinaryExpression.Type switch
        {
            BinaryExpressionType.Plus => "op_Addition",
            BinaryExpressionType.Minus => "op_Subtraction",
            BinaryExpressionType.Times => "op_Multiply",
            BinaryExpressionType.Div => "op_Division",
            BinaryExpressionType.Modulo => "op_Modulus",
            _ => null
        };

        if (operatorName == null && !isEquality) return;

        var left = await args.LeftValueAsync();
        var right = await args.RightValueAsync();

        HandleBinaryResult(args, isEquality, operatorName, left, right);
    }

    private static void HandleBinaryResult(BinaryEventArgs args, bool isEquality, string operatorName, object left, object right)
    {
        // NCalc's internal equality logic calls Convert.ChangeType() which requires IConvertible.
        // Element doesn't implement IConvertible, so intercept equality/inequality for non-standard
        // types and use Equals() instead — matching FLEE's behaviour of returning false for
        // cross-type comparisons (e.g. string vs Element, or Element vs string).
        if (isEquality && (!IsStandardNCalcType(left) || !IsStandardNCalcType(right)))
        {
            var equal = Equals(left, right);
            args.Result = args.BinaryExpression.Type == BinaryExpressionType.Equal ? equal : !equal;
            return;
        }

        // NCalc always returns Double for /, but FLEE compiled to IL where int/int = int.
        // Intercept integer division to match FLEE's behavior.
        if (args.BinaryExpression.Type == BinaryExpressionType.Div
            && IsIntegerType(left) && IsIntegerType(right))
        {
            args.Result = Convert.ToInt64(left) / Convert.ToInt64(right);
            return;
        }

        // Let NCalc handle standard numeric/bool/string/null operations natively
        if (IsStandardNCalcType(left) && IsStandardNCalcType(right))
            return;

        var leftType = left?.GetType();
        var rightType = right?.GetType();

        foreach (var declaringType in new[] { leftType, rightType }.Where(t => t != null).Distinct())
        {
            var method = TryFindOperatorOverload(declaringType, operatorName, leftType, rightType);
            if (method != null)
            {
                args.Result = method.Invoke(null, new[] { left, right });
                return;
            }
        }
    }

    private static bool IsStandardNCalcType(object value) =>
        value is null or int or long or double or float or decimal or byte or short or uint or ulong or ushort or sbyte or bool or string;

    private static bool IsIntegerType(object value) =>
        value is int or long or byte or short or uint or ulong or ushort or sbyte;

    private static MethodInfo TryFindOperatorOverload(Type declaringType, string operatorName, Type leftType, Type rightType)
    {
        foreach (var method in declaringType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == operatorName))
        {
            var ps = method.GetParameters();
            if (ps.Length != 2) continue;
            if (leftType != null && !ps[0].ParameterType.IsAssignableFrom(leftType)) continue;
            if (rightType != null && !ps[1].ParameterType.IsAssignableFrom(rightType)) continue;
            return method;
        }
        return null;
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

    private object EvaluateAslFunction(string name, FunctionEventArgs args)
    {
        if (name == "__Quest_MethodCall__")
        {
            if (args.Parameters.Count < 2)
                throw new Exception("__Quest_MethodCall__ requires at least 2 arguments");
            var receiver = CoerceLong(args.Parameters.Evaluate(0));
            var methodName = args.Parameters.Evaluate(1) as string
                ?? throw new Exception("__Quest_MethodCall__ second argument must be a string method name");
            var methodArgs = Enumerable.Range(2, args.Parameters.Count - 2)
                .Select(i => CoerceLong(args.Parameters.Evaluate(i)))
                .ToArray();
            return DispatchMethodCall(receiver, methodName, methodArgs);
        }

        if (name == "__Quest_Index__")
        {
            if (args.Parameters.Count != 2)
                throw new Exception("Subscript operator requires exactly 2 operands");
            var collection = args.Parameters.Evaluate(0);
            var key = CoerceLong(args.Parameters.Evaluate(1));
            if (collection is IQuestList)
                return _expressionOwner.ListItem(collection, (int) key);
            return _expressionOwner.DictionaryItem(collection, key?.ToString());
        }

        if (name.Equals("cast", StringComparison.InvariantCultureIgnoreCase))
        {
            if (args.Parameters.Count != 2)
                throw new Exception("cast() expects 2 parameters: value and type");
            var value = CoerceLong(args.Parameters.Evaluate(0));
            _evaluatingCastType = true;
            object typeArg;
            try { typeArg = args.Parameters.Evaluate(1); }
            finally { _evaluatingCastType = false; }
            var typeName = typeArg as string
                ?? throw new Exception("cast() second parameter must be a type name (int, double, string, bool)");
            return CastValue(value, typeName);
        }

        if (name.Equals("if", StringComparison.InvariantCultureIgnoreCase))
        {
            if (args.Parameters.Count != 3)
            {
                throw new Exception("'if' function expects 3 parameters: condition, trueValue, falseValue");
            }
            var condition = args.Parameters.Evaluate(0);
            return condition is true ? args.Parameters.Evaluate(1) : args.Parameters.Evaluate(2);
        }

        if (name == "IsDefined")
        {
            if (args.Parameters.Count != 1)
            {
                throw new Exception("IsDefined function expects 1 parameter");
            }

            if (args.Parameters.Evaluate(0) is not string variableName)
            {
                throw new Exception("IsDefined function expects a string parameter");
            }

            return _context.Parameters.ContainsKey(variableName);
        }

        return RunQuestProcedure(name, args.Parameters.Count, i => args.Parameters.Evaluate(i));
    }

    private async Task<object> EvaluateAslFunctionAsync(string name, FunctionEventArgs args)
    {
        if (name == "__Quest_MethodCall__")
        {
            if (args.Parameters.Count < 2)
                throw new Exception("__Quest_MethodCall__ requires at least 2 arguments");
            var receiver = CoerceLong(await args.Parameters.EvaluateAsync(0));
            var methodName = await args.Parameters.EvaluateAsync(1) as string
                ?? throw new Exception("__Quest_MethodCall__ second argument must be a string method name");
            var methodArgs = await Task.WhenAll(Enumerable.Range(2, args.Parameters.Count - 2)
                .Select(async i => CoerceLong(await args.Parameters.EvaluateAsync(i))));
            return DispatchMethodCall(receiver, methodName, methodArgs);
        }

        if (name == "__Quest_Index__")
        {
            if (args.Parameters.Count != 2)
                throw new Exception("Subscript operator requires exactly 2 operands");
            var collection = await args.Parameters.EvaluateAsync(0);
            var key = CoerceLong(await args.Parameters.EvaluateAsync(1));
            if (collection is IQuestList)
                return _expressionOwner.ListItem(collection, (int) key);
            return _expressionOwner.DictionaryItem(collection, key?.ToString());
        }

        if (name.Equals("cast", StringComparison.InvariantCultureIgnoreCase))
        {
            if (args.Parameters.Count != 2)
                throw new Exception("cast() expects 2 parameters: value and type");
            var value = CoerceLong(await args.Parameters.EvaluateAsync(0));
            _evaluatingCastType = true;
            object typeArg;
            try { typeArg = await args.Parameters.EvaluateAsync(1); }
            finally { _evaluatingCastType = false; }
            var typeName = typeArg as string
                ?? throw new Exception("cast() second parameter must be a type name (int, double, string, bool)");
            return CastValue(value, typeName);
        }

        if (name.Equals("if", StringComparison.InvariantCultureIgnoreCase))
        {
            if (args.Parameters.Count != 3)
                throw new Exception("'if' function expects 3 parameters: condition, trueValue, falseValue");
            var condition = await args.Parameters.EvaluateAsync(0);
            return condition is true
                ? await args.Parameters.EvaluateAsync(1)
                : await args.Parameters.EvaluateAsync(2);
        }

        if (name == "IsDefined")
        {
            if (args.Parameters.Count != 1)
                throw new Exception("IsDefined function expects 1 parameter");
            if (await args.Parameters.EvaluateAsync(0) is not string variableName)
                throw new Exception("IsDefined function expects a string parameter");
            return _context.Parameters.ContainsKey(variableName);
        }

        return RunQuestProcedure(name, args.Parameters.Count,
            i => args.Parameters.EvaluateAsync(i).GetAwaiter().GetResult());
    }

    private object RunQuestProcedure(string name, int argCount, Func<int, object> evaluateArg)
    {
        var proc = _scriptContext.WorldModel.Procedure(name);

        if (proc == null)
        {
            throw new Exception($"Unknown function '{name}'");
        }

        var parameters = new Parameters();

        for (var cnt = 0; cnt < argCount; cnt++)
        {
            parameters.Add((string) proc.Fields[FieldDefinitions.ParamNames][cnt], evaluateArg(cnt));
        }

        return _scriptContext.WorldModel.RunProcedure(name, parameters, true);
    }

    private static object DispatchMethodCall(object receiver, string methodName, object[] methodArgs)
    {
        var argTypes = methodArgs.Select(a => a?.GetType() ?? typeof(object)).ToArray();
        var method = receiver?.GetType().GetMethod(methodName, argTypes);

        if (method == null && methodArgs.Any(a => a == null))
        {
            method = receiver?.GetType().GetMethods()
                .Where(m => m.Name == methodName)
                .FirstOrDefault(m =>
                {
                    var ps = m.GetParameters();
                    if (ps.Length != methodArgs.Length) return false;
                    for (var i = 0; i < methodArgs.Length; i++)
                    {
                        if (methodArgs[i] == null)
                        {
                            if (ps[i].ParameterType.IsValueType &&
                                Nullable.GetUnderlyingType(ps[i].ParameterType) == null)
                                return false;
                            continue;
                        }
                        if (!ps[i].ParameterType.IsInstanceOfType(methodArgs[i])) return false;
                    }
                    return true;
                });
        }

        if (method == null)
            throw new Exception($"Method '{methodName}' not found on '{receiver?.GetType().Name}'");
        return method.Invoke(receiver, methodArgs);
    }

    private static object CastValue(object value, string typeName) => typeName switch
    {
        "boolean" => Convert.ToBoolean(value),
        "byte" => Convert.ToByte(value),
        "sbyte" => Convert.ToSByte(value),
        "short" => Convert.ToInt16(value),
        "ushort" => Convert.ToUInt16(value),
        "int" => (int) Convert.ToDouble(value),
        "uint" => Convert.ToUInt32(value),
        "long" => Convert.ToInt64(value),
        "ulong" => Convert.ToUInt64(value),
        "single" => Convert.ToSingle(value),
        "double" => Convert.ToDouble(value),
        "decimal" => Convert.ToDecimal(value),
        "char" => Convert.ToChar(value),
        "object" => value,
        "string" => Convert.ToString(value),
        _ => throw new Exception($"cast(): unknown type '{typeName}'")
    };
}
