using System;
using System.Collections.Generic;
using Ciloci.Flee;
using QuestViva.Engine.Scripts;

namespace QuestViva.Engine.Expressions;

public class FleeExpressionEvaluator<T>(string expression, ScriptContext scriptContext) : IExpressionEvaluator<T>
{
    private IGenericExpression<T> _compiledExpression = null;
    private readonly Dictionary<string, Type> _types = new();

    public T Evaluate(Context c)
    {
        scriptContext.FleeExpressionContext.ExecutionContext = c;
        if (_compiledExpression == null || scriptContext.FleeExpressionContext.HaveVariableTypesChanged(_compiledExpression.Info.GetReferencedVariables(), _types))
        {
            // Lazy compilation since when the game is loaded, we don't know what types of
            // variables we have.
            try
            {
                _compiledExpression = scriptContext.FleeExpressionContext.ExpressionContext.CompileGeneric<T>(expression);
                scriptContext.FleeExpressionContext.PopulateVariableTypesCache(_compiledExpression.Info.GetReferencedVariables(), _types);
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
        scriptContext.FleeExpressionContext.ExecutionContext = c;
        if (_compiledExpression == null || scriptContext.FleeExpressionContext.HaveVariableTypesChanged(_compiledExpression.Info.GetReferencedVariables(), _types))
        {
            // Lazy compilation since when the game is loaded, we don't know what types of
            // variables we have.
            try
            {
                _compiledExpression = scriptContext.FleeExpressionContext.ExpressionContext.CompileDynamic(expression);
                scriptContext.FleeExpressionContext.PopulateVariableTypesCache(_compiledExpression.Info.GetReferencedVariables(), _types);
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