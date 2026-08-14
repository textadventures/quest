namespace QuestViva.EditorCore;

public class AttributeSubEditorControlData : IEditorControl
{
    private static readonly Dictionary<string, string> s_allTypes = new()
    {
        {"string", "String"},
        {"boolean", "Boolean"},
        {"int", "Integer"},
        {"double", "Double"},
        {"script", "Script"},
        {"stringlist", "String List"},
        {"object", "Object"},
        {"simplepattern", "Command pattern"},
        {"stringdictionary", "String dictionary"},
        {"scriptdictionary", "Script dictionary"},
        {"null", "Null"}
    };

    public AttributeSubEditorControlData(string attribute)
    {
        Attribute = attribute;
    }

    protected virtual Dictionary<string, string> AllowedTypes => s_allTypes;

    public string Attribute { get; }

    public string Caption => null;

    public string ControlType => null;

    public bool Expand => false;

    public bool GetBool(string tag)
    {
        return false;
    }

    public IDictionary<string, string> GetDictionary(string tag)
    {
        if (tag == "types")
        {
            return AllowedTypes;
        }

        if (tag == "editors")
        {
            return null;
        }

        throw new NotImplementedException();
    }

    public int? GetInt(string tag)
    {
        return null;
    }

    public double? GetDouble(string tag)
    {
        return null;
    }

    public IEnumerable<string> GetListString(string tag)
    {
        throw new NotImplementedException();
    }

    public virtual string GetString(string tag)
    {
        switch (tag)
        {
            case "checkbox":
                return "True";
            case "editprompt":
            case "valueprompt":
                return "Please enter a value";
            //return L.T("EditorPleaseEnterValue");
            case "keyprompt":
                return "Please enter a key";
            //return L.T("EditorPleaseEnterKey");
            default:
                return null;
        }
    }

    public int? Height => null;

    public Task<bool> IsControlVisible(IEditorData data)
    {
        return Task.FromResult(true);
    }

    public int? Width => null;

    public IEditorDefinition Parent => null;

    public bool IsControlVisibleInSimpleMode => true;

    public string Id => throw new NotImplementedException();
}