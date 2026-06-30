#nullable enable
using System.Collections;
using System.Reflection;
using QuestViva.Engine;
using QuestViva.Engine.GameLoader;
using QuestViva.Engine.Scripts;

namespace QuestViva.EngineTests;

/// <summary>
/// Verifies that every concrete implementation of each registry interface
/// appears in its corresponding explicit list. Uses reflection to discover
/// implementations, then compares against runtime state of the registry objects.
/// </summary>
[TestClass]
public class RegistrationTests
{
    private static readonly Assembly EngineAssembly = typeof(WorldModel).Assembly;
    private static readonly BindingFlags NonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;

    private static List<Type> FindImplementations(Type iface) =>
        EngineAssembly.GetTypes()
            .Where(t => iface.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface && !t.IsGenericTypeDefinition)
            .ToList();

    private static List<Type> FindImplementations(string fullTypeName) =>
        FindImplementations(EngineAssembly.GetType(fullTypeName)!);

    private static object? GetField(object obj, string name)
    {
        for (var t = obj.GetType(); t != null; t = t.BaseType)
        {
            var f = t.GetField(name, NonPublicInstance);
            if (f != null) return f.GetValue(obj);
        }
        return null;
    }

    private static object? GetProperty(object obj, string name) =>
        obj.GetType().GetProperty(name, NonPublicInstance)?.GetValue(obj);

    private static IEnumerable<TKey> DictKeys<TKey>(object? dict) =>
        ((IDictionary)dict!).Keys.Cast<TKey>();

    private static object? DictValue(object? dict, object key) =>
        ((IDictionary)dict!)[key];

    private static string? StringAppliesTo(object instance) =>
        (string?)instance.GetType().GetProperty("AppliesTo")?.GetValue(instance);

    private static object? EnumAppliesTo(object instance) =>
        instance.GetType().GetProperty("AppliesTo")?.GetValue(instance);

    private static GameLoader CreateGameLoader(GameLoader.LoadMode mode) =>
        new(new WorldModel(), mode);

    // Checks that every concrete string-keyed implementation appears in at least one mode's dict.
    private static List<string> FindUnregisteredStringKeyed(string ifaceFullName, GameLoader play, GameLoader edit,
        string memberName, bool isField)
    {
        var playDict  = isField ? GetField(play,  memberName) : GetProperty(play,  memberName);
        var editDict  = isField ? GetField(edit,  memberName) : GetProperty(edit,  memberName);
        var registered = new HashSet<string>(DictKeys<string>(playDict).Concat(DictKeys<string>(editDict)));

        return FindImplementations(ifaceFullName)
            .Select(t => (name: t.Name, appliesTo: StringAppliesTo(Activator.CreateInstance(t, nonPublic: true)!)))
            .Where(x => x.appliesTo != null && !registered.Contains(x.appliesTo))
            .Select(x => x.name)
            .ToList();
    }

    // ── ScriptFactory ────────────────────────────────────────────────────────

    [TestMethod]
    public void AllScriptConstructorsAreInExplicitList()
    {
        var factory = new ScriptFactory(new WorldModel());
        var registered = (Dictionary<string, IScriptConstructor>)
            typeof(ScriptFactory).GetField("m_scriptConstructors", NonPublicInstance)!.GetValue(factory)!;

        var missing = FindImplementations(typeof(IScriptConstructor))
            .Select(t => (IScriptConstructor)Activator.CreateInstance(t)!)
            .Where(c => c.Keyword != null && !registered.ContainsKey(c.Keyword))
            .Select(c => c.GetType().Name)
            .ToList();

        CollectionAssert.AreEqual(new List<string>(), missing,
            $"Not in ScriptFactory explicit list: {string.Join(", ", missing)}");
    }

    // ── WorldModel element factories ─────────────────────────────────────────

    [TestMethod]
    public void AllElementFactoriesAreInExplicitList()
    {
        var worldModel = new WorldModel();
        var factories = (Dictionary<ElementType, IElementFactory>)
            typeof(WorldModel).GetField("_elementFactories", NonPublicInstance)!.GetValue(worldModel)!;

        var missing = FindImplementations(typeof(IElementFactory))
            .Select(t => (IElementFactory)Activator.CreateInstance(t)!)
            .Where(f => !factories.ContainsKey(f.CreateElementType))
            .Select(f => f.GetType().Name)
            .ToList();

        CollectionAssert.AreEqual(new List<string>(), missing,
            $"Not in WorldModel.InitialiseElementFactories explicit list: {string.Join(", ", missing)}");
    }

    // ── GameLoader ───────────────────────────────────────────────────────────

    [TestMethod]
    public void AllAttributeLoadersAreInExplicitList()
    {
        var play = CreateGameLoader(GameLoader.LoadMode.Play);
        var edit = CreateGameLoader(GameLoader.LoadMode.Edit);
        var missing = FindUnregisteredStringKeyed(
            "QuestViva.Engine.GameLoader.GameLoader+IAttributeLoader",
            play, edit, "AttributeLoaders", isField: false);

        CollectionAssert.AreEqual(new List<string>(), missing,
            $"Not in AttributeLoaders explicit list: {string.Join(", ", missing)}");
    }

    [TestMethod]
    public void AllValueLoadersAreInExplicitList()
    {
        var play = CreateGameLoader(GameLoader.LoadMode.Play);
        var edit = CreateGameLoader(GameLoader.LoadMode.Edit);
        var missing = FindUnregisteredStringKeyed(
            "QuestViva.Engine.GameLoader.GameLoader+IValueLoader",
            play, edit, "_valueLoaders", isField: true);

        CollectionAssert.AreEqual(new List<string>(), missing,
            $"Not in IValueLoader explicit list: {string.Join(", ", missing)}");
    }

    [TestMethod]
    public void AllExtendedAttributeLoadersAreInExplicitList()
    {
        var play = CreateGameLoader(GameLoader.LoadMode.Play);
        var edit = CreateGameLoader(GameLoader.LoadMode.Edit);
        var missing = FindUnregisteredStringKeyed(
            "QuestViva.Engine.GameLoader.GameLoader+IExtendedAttributeLoader",
            play, edit, "ExtendedAttributeLoaders", isField: false);

        CollectionAssert.AreEqual(new List<string>(), missing,
            $"Not in ExtendedAttributeLoaders explicit list: {string.Join(", ", missing)}");
    }

    [TestMethod]
    public void AllXmlLoadersAreInExplicitList()
    {
        var play = CreateGameLoader(GameLoader.LoadMode.Play);
        var edit = CreateGameLoader(GameLoader.LoadMode.Edit);

        // DefaultXmlLoader has AppliesTo == null and is stored separately — skip it.
        var playDict  = GetField(play, "_xmlLoaders");
        var editDict  = GetField(edit, "_xmlLoaders");
        var registered = new HashSet<string>(DictKeys<string>(playDict).Concat(DictKeys<string>(editDict)));

        var missing = FindImplementations("QuestViva.Engine.GameLoader.GameLoader+IXmlLoader")
            .Select(t => (name: t.Name, instance: Activator.CreateInstance(t, nonPublic: true)!))
            .Select(x => (x.name, appliesTo: StringAppliesTo(x.instance)))
            .Where(x => x.appliesTo != null && !registered.Contains(x.appliesTo))
            .Select(x => x.name)
            .ToList();

        CollectionAssert.AreEqual(new List<string>(), missing,
            $"Not in AddXmlLoaders explicit list: {string.Join(", ", missing)}");
    }

    // ── GameSaver ────────────────────────────────────────────────────────────

    [TestMethod]
    public void AllElementSaversAreInExplicitList()
    {
        var gameSaver = new GameSaver(new WorldModel());
        var dictObj = typeof(GameSaver).GetField("_elementSavers", NonPublicInstance)!.GetValue(gameSaver)!;
        var registered = new HashSet<ElementType>(DictKeys<ElementType>(dictObj));

        var missing = FindImplementations("QuestViva.Engine.GameLoader.GameSaver+IElementSaver")
            .Select(t => (name: t.Name, instance: Activator.CreateInstance(t, nonPublic: true)!))
            .Select(x => (x.name, appliesTo: (ElementType?)EnumAppliesTo(x.instance)))
            .Where(x => x.appliesTo.HasValue && !registered.Contains(x.appliesTo.Value))
            .Select(x => x.name)
            .ToList();

        CollectionAssert.AreEqual(new List<string>(), missing,
            $"Not in GameSaver IElementSaver explicit list: {string.Join(", ", missing)}");
    }

    [TestMethod]
    public void AllElementsSaversAreInExplicitList()
    {
        var gameSaver = new GameSaver(new WorldModel());
        var dictObj = typeof(GameSaver).GetField("_elementsSavers", NonPublicInstance)!.GetValue(gameSaver)!;
        var registered = new HashSet<ElementType>(DictKeys<ElementType>(dictObj));

        var missing = FindImplementations("QuestViva.Engine.GameLoader.GameSaver+IElementsSaver")
            .Select(t => (name: t.Name, instance: Activator.CreateInstance(t, nonPublic: true)!))
            .Select(x => (x.name, appliesTo: (ElementType?)EnumAppliesTo(x.instance)))
            .Where(x => x.appliesTo.HasValue && !registered.Contains(x.appliesTo.Value))
            .Select(x => x.name)
            .ToList();

        CollectionAssert.AreEqual(new List<string>(), missing,
            $"Not in GameSaver IElementsSaver explicit list: {string.Join(", ", missing)}");
    }

    [TestMethod]
    public void AllObjectSaversAreInExplicitList()
    {
        var gameSaver = new GameSaver(new WorldModel());
        var elementSaversDict = typeof(GameSaver).GetField("_elementSavers", NonPublicInstance)!.GetValue(gameSaver)!;
        var objectSaver = DictValue(elementSaversDict, ElementType.Object)!;
        var objectSaversDict = objectSaver.GetType().GetField("_savers", NonPublicInstance)!.GetValue(objectSaver)!;
        var registered = new HashSet<ObjectType>(DictKeys<ObjectType>(objectSaversDict));

        var missing = FindImplementations("QuestViva.Engine.GameLoader.GameSaver+ObjectSaver+IObjectSaver")
            .Select(t => (name: t.Name, instance: Activator.CreateInstance(t, nonPublic: true)!))
            .Select(x => (x.name, appliesTo: (ObjectType?)EnumAppliesTo(x.instance)))
            .Where(x => x.appliesTo.HasValue && !registered.Contains(x.appliesTo.Value))
            .Select(x => x.name)
            .ToList();

        CollectionAssert.AreEqual(new List<string>(), missing,
            $"Not in ObjectSaver.Initialise explicit list: {string.Join(", ", missing)}");
    }

    [TestMethod]
    public void AllFieldSaversAreInExplicitList()
    {
        var gameSaver = new GameSaver(new WorldModel());
        var elementSaversDict = typeof(GameSaver).GetField("_elementSavers", NonPublicInstance)!.GetValue(gameSaver)!;
        // Any ElementSaverBase has a _fieldSaver; grab the first one.
        var anySaver = DictKeys<ElementType>(elementSaversDict)
            .Select(k => DictValue(elementSaversDict, k)!)
            .First();
        var fieldSaver = GetField(anySaver, "_fieldSaver")!;
        var fieldSaversDict = fieldSaver.GetType().GetField("_savers", NonPublicInstance)!.GetValue(fieldSaver)!;
        var registered = new HashSet<Type>(DictKeys<Type>(fieldSaversDict));

        var missing = FindImplementations("QuestViva.Engine.GameLoader.FieldSaver+IFieldSaver")
            .Select(t => (name: t.Name, instance: Activator.CreateInstance(t, nonPublic: true)!))
            .Select(x => (x.name, appliesTo: (Type?)EnumAppliesTo(x.instance)))
            .Where(x => x.appliesTo != null && !registered.Contains(x.appliesTo))
            .Select(x => x.name)
            .ToList();

        CollectionAssert.AreEqual(new List<string>(), missing,
            $"Not in FieldSaver explicit list: {string.Join(", ", missing)}");
    }
}
