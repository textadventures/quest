using QuestViva.Engine.Scripts;

namespace QuestViva.Engine.Functions
{
    public interface IFunction<T>
    {
        T Execute(Context c);
        string Save();
        IFunction<T> Clone();
    }

    public interface IFunctionGeneric
    {
        object Execute(Context c);
        string Save();
        IFunctionGeneric Clone();
    }
}
