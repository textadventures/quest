#nullable disable
using QuestViva.Engine.Scripts;

namespace QuestViva.Engine.Functions
{
    public interface IFunction<T>
    {
        T Execute(Context c);
        string Save();
        IFunction<T> Clone();
    }

    public interface IFunctionDynamic
    {
        object Execute(Context c);
        string Save();
        IFunctionDynamic Clone();
    }
}
