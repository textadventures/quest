using QuestViva.Engine.Scripts;

namespace QuestViva.Engine.Functions;

public interface IFunction<T>
{
    T Execute(Context c);
    Task<T> ExecuteAsync(Context c);
    string Save();
    IFunction<T> Clone();
}

public interface IFunctionDynamic
{
    object Execute(Context c);
    Task<object> ExecuteAsync(Context c);
    string Save();
    IFunctionDynamic Clone();
}