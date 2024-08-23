using TextAdventures.Quest.Scripts;

namespace TextAdventures.Quest.Expressions;

public class NcalcExpressionEvaluator<T>: IExpressionEvaluator<T>, IDynamicExpressionEvaluator
{
    private readonly NCalc.Expression _nCalcExpression;

    public NcalcExpressionEvaluator(string expression, ScriptContext scriptContext)
    {
        _nCalcExpression = new NCalc.Expression(expression);
    }

    object IDynamicExpressionEvaluator.Evaluate(Context c)
    {
        return Evaluate(c);
    }

    public T Evaluate(Context c)
    {
        return (T)_nCalcExpression.Evaluate();
    }
}