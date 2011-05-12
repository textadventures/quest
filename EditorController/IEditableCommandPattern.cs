using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxeSoftware.Quest
{
    public interface IEditableCommandPattern : IDataWrapper
    {
        string Pattern { get; set; }
    }
}
