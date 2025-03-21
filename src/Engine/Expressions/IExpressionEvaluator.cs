#nullable disable
using QuestViva.Engine.Scripts;

namespace QuestViva.Engine.Expressions;

public interface IExpressionEvaluator<out T>
{
    T Evaluate(Context c);
}

public interface IDynamicExpressionEvaluator
{
    object Evaluate(Context c);
}