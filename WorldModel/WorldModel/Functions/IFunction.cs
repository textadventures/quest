using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Scripts;

namespace AxeSoftware.Quest.Functions
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
