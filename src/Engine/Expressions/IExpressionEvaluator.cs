using QuestViva.Engine.Scripts;

namespace QuestViva.Engine.Expressions;

public interface IExpressionEvaluator<T>
{
    T Evaluate(Context c);
    Task<T> EvaluateAsync(Context c);
}

public interface IDynamicExpressionEvaluator
{
    object Evaluate(Context c);
    Task<object> EvaluateAsync(Context c);
}