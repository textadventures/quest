#nullable disable
namespace QuestViva.Engine.Scripts;

internal class DelegateImplementation
{
    public DelegateImplementation(string typeName, Element def, Element impl)
    {
        Definition = def;
        Implementation = impl;
        TypeName = typeName;
    }

    public Element Definition { get; }

    public Element Implementation { get; }

    public string TypeName { get; }

    public override string ToString()
    {
        var script = Implementation.Fields[FieldDefinitions.Script];
        return script == null ? string.Empty : script.ToString();
    }
}