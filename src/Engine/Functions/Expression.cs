using QuestViva.Engine.Expressions;
using QuestViva.Engine.Scripts;

namespace QuestViva.Engine.Functions;

public abstract class ExpressionBase
{
    protected readonly string Expression;
    protected readonly string OriginalExpression;
    protected readonly ScriptContext ScriptContext;

    protected ExpressionBase(string expression, ScriptContext scriptContext)
    {
        ScriptContext = scriptContext;

        OriginalExpression = expression;
        if (!scriptContext.WorldModel.EditMode)
        {
            expression = Utility.EncodeIdentifierSpaces(expression);
        }

        Expression = expression;
    }

    public override string ToString()
    {
        return "Expression: " + OriginalExpression;
    }
}

public class Expression<T> : ExpressionBase, IFunction<T>
{
    private readonly IExpressionEvaluator<T> _expressionEvaluator;

    public Expression(string expression, ScriptContext scriptContext)
        : base(expression, scriptContext)
    {
        _expressionEvaluator = new NcalcExpressionEvaluator<T>(Expression, ScriptContext);
    }

    public IFunction<T> Clone()
    {
        return new Expression<T>(Expression, ScriptContext);
    }

    public Task<T> ExecuteAsync(Context c)
    {
        return _expressionEvaluator.EvaluateAsync(c);
    }

    public string Save()
    {
        return OriginalExpression;
    }
}

public class ExpressionDynamic : ExpressionBase, IFunctionDynamic
{
    private readonly IDynamicExpressionEvaluator _dynamicExpressionEvaluator;

    public ExpressionDynamic(string expression, ScriptContext scriptContext)
        : base(expression, scriptContext)
    {
        _dynamicExpressionEvaluator = new NcalcExpressionEvaluator<object>(Expression, ScriptContext);
    }

    public IFunctionDynamic Clone()
    {
        return new ExpressionDynamic(Expression, ScriptContext);
    }

    public Task<object> ExecuteAsync(Context c)
    {
        return _dynamicExpressionEvaluator.EvaluateAsync(c);
    }

    public string Save()
    {
        return OriginalExpression;
    }
}