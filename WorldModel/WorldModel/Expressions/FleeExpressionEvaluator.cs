using System;
using System.Collections.Generic;
using Ciloci.Flee;
using TextAdventures.Quest.Scripts;

namespace TextAdventures.Quest.Expressions;

public class FleeExpressionEvaluator<T>(string expression, ScriptContext scriptContext) : IExpressionEvaluator<T>
{
    private IGenericExpression<T> _compiledExpression = null;
    private readonly Dictionary<string, Type> _types = new();

    public T Evaluate(Context c)
    {
        scriptContext.ExecutionContext = c;
        if (_compiledExpression == null || scriptContext.HaveVariableTypesChanged(_compiledExpression.Info.GetReferencedVariables(), _types))
        {
            // Lazy compilation since when the game is loaded, we don't know what types of
            // variables we have.
            try
            {
                _compiledExpression = scriptContext.FleeExpressionContext.CompileGeneric<T>(expression);
                scriptContext.PopulateVariableTypesCache(_compiledExpression.Info.GetReferencedVariables(), _types);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error compiling expression '{Utility.ConvertFleeFormatToVariables(expression)}': {ex.Message}", ex);
            }
        }
        try
        {
            return _compiledExpression.Evaluate();
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Error evaluating expression '{Utility.ConvertFleeFormatToVariables(expression)}': {ex.Message}", ex);
        }
    }
}

public class FleeDynamicExpressionEvaluator(string expression, ScriptContext scriptContext) : IDynamicExpressionEvaluator
{
    private IDynamicExpression _compiledExpression = null;
    private readonly Dictionary<string, Type> _types = new();

    public object Evaluate(Context c)
    {
        scriptContext.ExecutionContext = c;
        if (_compiledExpression == null || scriptContext.HaveVariableTypesChanged(_compiledExpression.Info.GetReferencedVariables(), _types))
        {
            // Lazy compilation since when the game is loaded, we don't know what types of
            // variables we have.
            try
            {
                _compiledExpression = scriptContext.FleeExpressionContext.CompileDynamic(expression);
                scriptContext.PopulateVariableTypesCache(_compiledExpression.Info.GetReferencedVariables(), _types);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error compiling expression '{Utility.ConvertFleeFormatToVariables(expression)}': {ex.Message}", ex);
            }
        }
        try
        {
            return _compiledExpression.Evaluate();
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Error evaluating expression '{Utility.ConvertFleeFormatToVariables(expression)}': {ex.Message}", ex);
        }
    }
}