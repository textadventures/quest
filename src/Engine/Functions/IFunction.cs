using QuestViva.Engine.Scripts;

namespace QuestViva.Engine.Functions;

public interface IFunction<T>
{
    Task<T> ExecuteAsync(Context c);
    string Save();
    IFunction<T> Clone();
}

public interface IFunctionDynamic
{
    Task<object> ExecuteAsync(Context c);
    string Save();
    IFunctionDynamic Clone();
}