using TextAdventures.Quest.Scripts;

namespace TextAdventures.Quest.Expressions;

public interface IExpressionEvaluator<out T>
{
    T Evaluate(Context c);
}

public interface IDynamicExpressionEvaluator
{
    object Evaluate(Context c);
}