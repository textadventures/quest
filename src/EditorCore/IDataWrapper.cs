using System;

namespace QuestViva.EditorCore
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
