using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using QuestViva.Common;
using QuestViva.Engine.Scripts;

namespace QuestViva.Engine.Functions;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
internal class ExpressionOwner(WorldModel worldModel)
{
    private readonly Random _random = new();

    private static T GetParameter<T>(object? parameter, string caller, string expectedType) where T : class
    {
        if (parameter is not T result)
        {
            throw new Exception(
                $"{caller} function expected {expectedType} parameter but was passed '{parameter ?? "null"}'");
        }
        return result;
    }

    public string Template(string template)
    {
        return worldModel.Template.GetText(template);
    }

    public string DynamicTemplate(string template, params Element[] obj)
    {
        return worldModel.Template.GetDynamicText(template, obj);
    }

    public string DynamicTemplate(string template, string text)
    {
        return worldModel.Template.GetDynamicText(template, text);
    }

    public bool HasString(/* Element */ object obj, string property)
    {
        var element = GetParameter<Element>(obj, "HasString", "object");
        return element.Fields.HasString(property);
    }

    public string GetString(/* Element */ object obj, string property)
    {
        var element = GetParameter<Element>(obj, "GetString", "object");
        return element.Fields.GetString(property);
    }

    public bool HasBoolean(/* Element */ object obj, string property)
    {
        var element = GetParameter<Element>(obj, "HasBoolean", "object");
        return element.Fields.HasType<bool>(property);
    }

    public bool GetBoolean(/* Element */ object obj, string property)
    {
        var element = GetParameter<Element>(obj, "GetBoolean", "object");
        return element.Fields.GetAsType<bool>(property);
    }
        
    public bool HasInt(/* Element */ object obj, string property)
    {
        var element = GetParameter<Element>(obj, "HasInt", "object");
        return element.Fields.HasType<int>(property);
    }

    public int GetInt(/* Element */ object obj, string property)
    {
        var element = GetParameter<Element>(obj, "GetInt", "object");
        return element.Fields.GetAsType<int>(property);
    }

    public bool HasDouble(/* Element */ object obj, string property)
    {
        var element = GetParameter<Element>(obj, "HasDouble", "object");
        return element.Fields.HasType<double>(property);
    }

    public double GetDouble(/* Element */ object obj, string property)
    {
        var element = GetParameter<Element>(obj, "GetDouble", "object");
        return element.Fields.GetAsType<double>(property);
    }

    public bool HasScript(/* Element */ object obj, string property)
    {
        var element = GetParameter<Element>(obj, "HasScript", "object");
        return element.Fields.HasType<IScript>(property);
    }

    public bool HasObject(/* Element */ object obj, string property)
    {
        var element = GetParameter<Element>(obj, "HasObject", "object");
        return element.Fields.HasType<Element>(property);
    }

    public bool HasDelegateImplementation(/* Element */ object obj, string property)
    {
        var element = GetParameter<Element>(obj, "HasDelegateImplementation", "object");
        return element.Fields.HasType<DelegateImplementation>(property);
    }

    public object GetAttribute(/* Element */ object obj, string property)
    {
        var element = GetParameter<Element>(obj, "GetAttribute", "object");
        return element.Fields.Get(property);
    }

    public bool HasAttribute(/* Element */ object obj, string property)
    {
        var element = GetParameter<Element>(obj, "HasAttribute", "object");
        return element.Fields.Exists(property, true);
    }

    public QuestList<string> GetAttributeNames(/* Element */ object obj, bool includeInheritedAttributes)
    {
        var element = GetParameter<Element>(obj, "GetAttributeNames", "object");
        return new QuestList<string>(element.Fields.GetAttributeNames(includeInheritedAttributes));
    }

    public string? GetExitByLink(/* Element */ object from, /* Element */ object to)
    {
        var fromElement = GetParameter<Element>(from, "GetExitByLink", "object");
        var toElement = GetParameter<Element>(to, "GetExitByLink", "object");
        foreach (var e in worldModel.Objects)
        {
            if (e.Parent == fromElement && e.Fields[FieldDefinitions.To] == toElement) return e.Name;
        }

        return null;
    }

    public string? GetExitByName(/* Element */ object parent, string name)
    {
        var parentElement = GetParameter<Element>(parent, "GetExitByName", "object");
        foreach (var e in worldModel.Objects)
        {
            if (e.Parent == parentElement && e.Fields[FieldDefinitions.Alias] == name) return e.Name;
        }

        return null;
    }

    public bool Contains(/* Element */ object parent, /* Element */ object name)
    {
        var parentElement = GetParameter<Element>(parent, "Contains", "object");
        var nameElement = GetParameter<Element>(name, "Contains", "object");
        return WorldModel.ObjectContains(parentElement, nameElement);
    }

    public QuestList<Element> NewObjectList()
    {
        return new QuestList<Element>();
    }

    public QuestList<string> NewStringList()
    {
        return new QuestList<string>();
    }

    public QuestList<object> NewList()
    {
        return new QuestList<object>();
    }

    public QuestDictionary<string> NewStringDictionary()
    {
        return new QuestDictionary<string>();
    }

    public QuestDictionary<Element> NewObjectDictionary()
    {
        return new QuestDictionary<Element>();
    }

    public QuestDictionary<IScript> NewScriptDictionary()
    {
        return new QuestDictionary<IScript>();
    }

    public QuestDictionary<object> NewDictionary()
    {
        return new QuestDictionary<object>();
    }

    public bool ListContains(/* IQuestList */ object list, object item)
    {
        var questList = GetParameter<IQuestList>(list, "ListContains", "list");
        return questList.Contains(item);
    }

    public QuestList<Element> AllObjects()
    {
        var result = new QuestList<Element>();

        foreach (var item in worldModel.GetAllObjects().Where(o => o.Type == ObjectType.Object))
        {
            result.Add(item);
        }
        return result;
    }

    public QuestList<Element> AllExits()
    {
        var result = new QuestList<Element>();

        foreach (var item in worldModel.GetAllObjects().Where(o => o.Type == ObjectType.Exit))
        {
            result.Add(item);
        }
        return result;
    }

    public QuestList<Element> AllTurnScripts()
    {
        var result = new QuestList<Element>();

        foreach (var item in worldModel.GetAllObjects().Where(o => o.Type == ObjectType.TurnScript))
        {
            result.Add(item);
        }
        return result;
    }

    public QuestList<Element> AllCommands()
    {
        var result = new QuestList<Element>();
        foreach (var item in worldModel.GetAllObjects())
        {
            if (item.Type == ObjectType.Command)
                result.Add(item);
        }
        return result;
    }

    public int ListCount(/* ICollection */ object list)
    {
        var questList = GetParameter<ICollection>(list, "ListCount", "list");
        return questList.Count;
    }

    public object ListItem(/* IQuestList */ object list, int index)
    {
        var questList = GetParameter<IQuestList>(list, "ListItem", "list");

        try
        {
            return questList[index];
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new Exception(string.Format("ListItem: index {0} is out of range for this list ({1} items, last index is {2})", index, questList.Count, questList.Count - 1), ex);
        }
    }

    public string? StringListItem(/* IQuestList */ object list, int index)
    {
        var questList = GetParameter<IQuestList>(list, "StringListItem", "list");

        try
        {
            return questList[index] as string;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new Exception(
                $"StringListItem: index {index} is out of range for this list ({questList.Count} items, last index is {questList.Count - 1})", ex);
        }
    }

    public Element? ObjectListItem(/* IQuestList */ object list, int index)
    {
        var questList = GetParameter<IQuestList>(list, "ObjectListItem", "list");

        try
        {
            return questList[index] as Element;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new Exception(
                $"ObjectListItem: index {index} is out of range for this list ({questList.Count} items, last index is {questList.Count - 1})", ex);
        }
    }

    public Element GetObject(string name)
    {
        return TryGetElement(ElementType.Object, name);
    }

    public Element GetTimer(string name)
    {
        return TryGetElement(ElementType.Timer, name);
    }

    private Element TryGetElement(ElementType type, string name)
    {
        worldModel.Elements.TryGetValue(type, name, out var result);
        return result;
    }

    public string TypeOf(/* Element */ object obj, string attribute)
    {
        var element = GetParameter<Element>(obj, "TypeOf", "object");
        var value = element.Fields.Get(attribute);
        return TypeOf(value);
    }

    public string TypeOf(object? value)
    {
        return value switch
        {
            null => "null",
            DelegateImplementation implementation => implementation.TypeName,
            _ => WorldModel.ConvertTypeToTypeName(value.GetType())
        };
    }

    public object RunDelegateFunction(/* Element */ object obj, string del, params object[] parameters)
    {
        var element = GetParameter<Element>(obj, "RunDelegateFunction", "object");
        var impl = element.Fields.Get(del) as DelegateImplementation;

        if (impl == null)
        {
            throw new Exception(string.Format("Object '{0}' has no delegate implementation '{1}'", element.Name, del));
        }

        var paramValues = new Parameters();

        var cnt = 0;
        foreach (var p in parameters)
        {
            paramValues.Add((string)impl.Definition.Fields[FieldDefinitions.ParamNames][cnt], p);
            cnt++;
        }

        return worldModel.RunDelegateScript(impl.Implementation.Fields[FieldDefinitions.Script], paramValues, element);
    }

    // ReSharper disable once InconsistentNaming
    public string SafeXML(string input)
    {
        return Utility.SafeXML(input);
    }

    public bool IsRegexMatch(string regexPattern, string input)
    {
        return Utility.IsRegexMatch(regexPattern, input);
    }

    public bool IsRegexMatch(string regexPattern, string input, string cacheId)
    {
        return Utility.IsRegexMatch(regexPattern, input, worldModel.RegexCache, cacheId);
    }

    public int GetMatchStrength(string regexPattern, string input)
    {
        return Utility.GetMatchStrength(regexPattern, input);
    }

    public int GetMatchStrength(string regexPattern, string input, string cacheId)
    {
        return Utility.GetMatchStrength(regexPattern, input, worldModel.RegexCache, cacheId);
    }

    public QuestDictionary<string> Populate(string regexPattern, string input)
    {
        return Utility.Populate(regexPattern, input);
    }

    public QuestDictionary<string> Populate(string regexPattern, string input, string cacheId)
    {
        return Utility.Populate(regexPattern, input, worldModel.RegexCache, cacheId);
    }

    public object? DictionaryItem(/* IDictionary */ object obj, string key)
    {
        var dictionary = GetParameter<IDictionary>(obj, "DictionaryItem", "dictionary");
        return dictionary[key];
    }

    public string? StringDictionaryItem(/* IDictionary */ object obj, string key)
    {
        var dictionary = GetParameter<IDictionary>(obj, "StringDictionaryItem", "dictionary");
        return dictionary[key] as string;
    }

    public Element? ObjectDictionaryItem(/* IDictionary */ object obj, string key)
    {
        var dictionary = GetParameter<IDictionary>(obj, "ObjectDictionaryItem", "dictionary");
        return dictionary[key] as Element;
    }

    public IScript? ScriptDictionaryItem(/* IDictionary */ object obj, string key)
    {
        var dictionary = GetParameter<IDictionary>(obj, "ScriptDictionaryItem", "dictionary");
        return dictionary[key] as IScript;
    }

    public string ShowMenu(string caption, QuestDictionary<string> options, bool allowCancel)
    {
        if (worldModel.Version >= WorldModelVersion.v540)
        {
            throw new Exception("The 'ShowMenu' function is not supported for games written for Quest 5.4 or later. Use the 'show menu' script command instead.");
        }
        return worldModel.DisplayMenu(caption, options, allowCancel, false);
    }

    public string ShowMenu(string caption, QuestList<string> options, bool allowCancel)
    {
        if (worldModel.Version >= WorldModelVersion.v540)
        {
            throw new Exception("The 'ShowMenu' function is not supported for games written for Quest 5.4 or later. Use the 'show menu' script command instead.");
        }
        return worldModel.DisplayMenu(caption, options, allowCancel, false);
    }

    public bool DictionaryContains(/* IDictionary */ object obj, string key)
    {
        var dictionary = GetParameter<IDictionary>(obj, "DictionaryContains", "dictionary");
        return dictionary.Contains(key);
    }

    public int DictionaryCount(IDictionary obj)
    {
        var dictionary = GetParameter<IDictionary>(obj, "DictionaryCount", "dictionary");
        return dictionary.Count;
    }

    public int ToInt(string number)
    {
        return int.Parse(number, System.Globalization.CultureInfo.InvariantCulture);
    }

    public double ToDouble(string number)
    {
        return double.Parse(number, System.Globalization.CultureInfo.InvariantCulture);
    }

    public string ToString(int number)
    {
        return number.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    public string ToString(double number)
    {
        return number.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    public string? ToString(object obj)
    {
        return obj.ToString();
    }

    public bool IsInt(string number)
    {
        return int.TryParse(number, out _);
    }

    public bool IsDouble(string number)
    {
        return double.TryParse(number,
            System.Globalization.NumberStyles.AllowDecimalPoint |
            System.Globalization.NumberStyles.AllowLeadingSign,
            System.Globalization.CultureInfo.InvariantCulture, out _);
    }

    public string GetInput()
    {
        if (worldModel.Version >= WorldModelVersion.v540)
        {
            throw new Exception("The 'GetInput' function is not supported for games written for Quest 5.4 or later. Use the 'get input' script command instead.");
        }
        return worldModel.GetNextCommandInput(false);
    }

    // ReSharper disable once InconsistentNaming
    public string GetFileURL(string filename)
    {
        if (filename.Contains("..")) throw new Exception("Invalid filename");
        return worldModel.GetExternalUrl(filename);
    }

    public string? GetFileData(string filename)
    {
        if (filename.Contains("..")) throw new Exception("Invalid filename");
        return worldModel.GetResourceData(filename);
    }

    // This was added for Quest 5.8. Should be a responsibility of the front-end, not WorldModel.
    public string GetExternalFileData(string filename)
    {
        throw new Exception("GetExternalFileData is not supported");
    }

    // This was added for Quest 5.8. Should be a responsibility of the front-end, not WorldModel.
    public string SetExternalFileData(string filename, string content)
    {
        throw new Exception("SetExternalFileData is not supported");
    }

    public string GetUniqueElementName(string name)
    {
        return worldModel.GetUniqueElementName(name);
    }

    public bool Ask(string caption)
    {
        if (worldModel.Version >= WorldModelVersion.v540)
        {
            throw new Exception("The 'Ask' function is not supported for games written for Quest 5.4 or later. Use the 'ask' script command instead.");
        }
        return worldModel.ShowQuestion(caption);
    }

    public int GetRandomInt(int min, int max)
    {
        // The .net implementation of Random.Next defines the minValue as the
        // inclusive lower bound, but maxValue as the Exclusive upper bound.
        // It makes a bit more sense for maxValue to be inclusive, so we add 1.
        return _random.Next(min, max + 1);
    }

    public double GetRandomDouble()
    {
        return _random.NextDouble();
    }

    public object Eval(string expression)
    {
        return Eval(expression, null);
    }

    public object Eval(string expression, /* IDictionary */ object? obj)
    {
        var parameters = (obj == null) ? null : GetParameter<IDictionary>(obj, "Eval", "dictionary");
        var expr = new ExpressionDynamic(expression, new ScriptContext(worldModel));
        var context = new Context();
        if (parameters != null) context.Parameters = new Parameters(parameters);
        return expr.Execute(context);
    }

    public Element Clone(/* Element */ object obj)
    {
        var element = GetParameter<Element>(obj, "Clone", "object");
        var newElement = element.Clone();
        return newElement;
    }

    public Element ShallowClone(/* Element */ object obj)
    {
        var element = GetParameter<Element>(obj, "Clone", "object");
        var newElement = element.ShallowClone();
        return newElement;
    }

    public bool DoesInherit(/* Element */ object obj, string typeName)
    {
        var element = GetParameter<Element>(obj, "DoesInherit", "object");
        var type = worldModel.Elements.Get(ElementType.ObjectType, typeName);
        return element.Fields.InheritsTypeRecursive(type);
    }

    public QuestList<string> ListCombine(QuestList<string> list1, QuestList<string> list2)
    {
        return ListCombine<string>(list1, list2);
    }

    public QuestList<Element> ListCombine(QuestList<Element> list1, QuestList<Element> list2)
    {
        return ListCombine<Element>(list1, list2);
    }

    public QuestList<object> ListCombine(QuestList<object> list1, QuestList<object> list2)
    {
        return ListCombine<object>(list1, list2);
    }

    private QuestList<T> ListCombine<T>(QuestList<T>? list1, QuestList<T> list2)
    {
        return list1 == null ? new QuestList<T>(list2) : list1.MergeLists(list2);
    }

    public QuestList<string> ListExclude(QuestList<string> list, string str)
    {
        return list.Exclude(str);
    }

    public QuestList<Element> ListExclude(QuestList<Element> list, Element element)
    {
        return list.Exclude(element);
    }

    public QuestList<object> ListExclude(QuestList<object> list, Element element)
    {
        return list.Exclude(element);
    }

    public QuestList<string> ListExclude(QuestList<string> list, QuestList<string> excludeList)
    {
        return list.Exclude(excludeList);
    }

    public QuestList<Element> ListExclude(QuestList<Element> list, QuestList<Element> excludeList)
    {
        return list.Exclude(excludeList);
    }

    public QuestList<object> ListExclude(QuestList<object> list, QuestList<object> excludeList)
    {
        return list.Exclude(excludeList);
    }

    public QuestList<Element> GetAllChildObjects(/* Element */ object obj)
    {
        var element = GetParameter<Element>(obj, "GetAllChildObjects", "object");
        return GetAllChildren(element, ObjectType.Object);
    }

    private QuestList<Element> GetAllChildren(/* Element */ object obj, ObjectType type)
    {
        var element = GetParameter<Element>(obj, "GetAllChildren", "object");
        QuestList<Element> result = new QuestList<Element>();
        foreach (var child in worldModel.Elements.GetDirectChildren(element).Where(e => e.ElemType == ElementType.Object && e.Type == type))
        {
            result.Add(child);
            result.AddRange(GetAllChildren(child, type));
        }
        return result;
    }

    public QuestList<Element> GetDirectChildren(/* Element */ object obj)
    {
        var element = GetParameter<Element>(obj, "GetDirectChildren", "object");
        return new QuestList<Element>(
            worldModel.Elements.GetDirectChildren(element)
                .Where(e => e.ElemType == ElementType.Object && e.Type == ObjectType.Object));
    }

    public bool IsGameRunning()
    {
        return worldModel.State != GameState.Finished;
    }

    public QuestList<Element> ObjectListSort(/* QuestList<Element> */ object obj, params string[] attribute)
    {
        var list = GetParameter<QuestList<Element>>(obj, "ObjectListSort", "objectlist");
        var result = list.OrderBy(e => e.Fields.Get(attribute[0]));
        for (var i = 1; i < attribute.Length; i++)
        {
            var idx = i;    // need a local copy of i for the lambda
            result = result.ThenBy(e => e.Fields.Get(attribute[idx]));
        }
        return new QuestList<Element>(result);
    }

    public QuestList<Element> ObjectListSortDescending(/* QuestList<Element> */ object obj, params string[] attribute)
    {
        var list = GetParameter<QuestList<Element>>(obj, "ObjectListSortDescending", "objectlist");
        return new QuestList<Element>(ObjectListSort(list, attribute).Reverse());
    }

    public QuestList<string> StringListSort(/* QuestList<string> */ object obj)
    {
        var list = GetParameter<QuestList<string>>(obj, "StringListSort", "objectlist");
        return new QuestList<string>(list.OrderBy(item => item));
    }

    public QuestList<string> StringListSortDescending(/* QuestList<string> */ object obj)
    {
        var list = GetParameter<QuestList<string>>(obj, "StringListSortDescending", "objectlist");
        return new QuestList<string>(StringListSort(list).Reverse());
    }

    public string? GetUiOption(string optionName)
    {
        if (Enum.TryParse(optionName, out UIOption option))
        {
            return worldModel.PlayerUi.GetUIOption(option);
        }
        throw new Exception($"Unrecognised UI option name '{optionName}'");
    }
}