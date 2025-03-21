#nullable disable
using System;

namespace QuestViva.Engine;

public class ElementFieldUpdatedEventArgs : EventArgs
{
    internal ElementFieldUpdatedEventArgs(Element element, string attribute, object newValue, bool isUndo)
    {
        Element = element;
        Attribute = attribute;
        NewValue = newValue;
        IsUndo = isUndo;
    }

    public Element Element { get; private set; }
    public string Attribute { get; private set; }
    public object NewValue { get; private set; }
    public bool IsUndo { get; private set; }
    public bool Refresh { get; private set; }
}

public class ElementRefreshEventArgs : EventArgs
{
    internal ElementRefreshEventArgs(Element element)
    {
        Element = element;
    }

    public Element Element { get; private set; }
}

public class LoadStatusEventArgs(string status) : EventArgs
{
    public string Status { get; private set; } = status;
}