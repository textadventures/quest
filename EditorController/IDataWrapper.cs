using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest
{
    public class DataWrapperUpdatedEventArgs : EventArgs
    {
    }

    public interface IDataWrapper
    {
        event EventHandler<DataWrapperUpdatedEventArgs> UnderlyingValueUpdated;
        object GetUnderlyingValue();
        string DisplayString();
    }
}
