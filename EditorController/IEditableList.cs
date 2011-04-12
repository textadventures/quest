using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public interface IEditableList<T>
    {
        IDictionary<string, IEditableListItem<T>> Items { get; }
    }

    public interface IEditableListItem<T>
    {
        string Key { get; }
        T Value { get; set; }
    }
}
