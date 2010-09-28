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
    }

    public interface IFunctionGeneric
    {
        object Execute(Context c);
        string Save();
    }
}
