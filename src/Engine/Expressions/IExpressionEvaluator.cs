using QuestViva.Engine.Scripts;

namespace QuestViva.Engine.Expressions;

public interface IExpressionEvaluator<T>
{
    Task<T> EvaluateAsync(Context c);
}

public interface IDynamicExpressionEvaluator
{
    Task<object> EvaluateAsync(Context c);
}