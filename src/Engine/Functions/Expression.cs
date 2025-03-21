using QuestViva.Engine.Expressions;
using QuestViva.Engine.Scripts;

namespace QuestViva.Engine.Functions;

public abstract class ExpressionBase
{
    protected readonly ScriptContext ScriptContext;
    protected readonly string OriginalExpression;
    protected readonly string Expression;

    protected ExpressionBase(string expression, ScriptContext scriptContext)
    {
        ScriptContext = scriptContext;

        OriginalExpression = expression;
        if (!scriptContext.WorldModel.EditMode)
        {
            expression = Utility.ConvertVariablesToFleeFormat(expression);
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
        if (scriptContext.WorldModel.UseNCalc)
        {
            _expressionEvaluator = new NcalcExpressionEvaluator<T>(Expression, ScriptContext);
        }
        else
        {
            _expressionEvaluator = new FleeExpressionEvaluator<T>(Expression, ScriptContext);
        }
    }

    public IFunction<T> Clone()
    {
        return new Expression<T>(Expression, ScriptContext);
    }

    public T Execute(Context c)
    {
        return _expressionEvaluator.Evaluate(c);
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
        if (scriptContext.WorldModel.UseNCalc)
        {
            _dynamicExpressionEvaluator = new NcalcExpressionEvaluator<object>(Expression, ScriptContext);
        }
        else
        {
            _dynamicExpressionEvaluator = new FleeDynamicExpressionEvaluator(Expression, ScriptContext);    
        }
    }

    public IFunctionDynamic Clone()
    {
        return new ExpressionDynamic(Expression, ScriptContext);
    }

    public object Execute(Context c)
    {
        return _dynamicExpressionEvaluator.Evaluate(c);
    }

    public string Save()
    {
        return OriginalExpression;
    }
}