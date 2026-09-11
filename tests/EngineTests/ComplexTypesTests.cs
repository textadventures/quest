using QuestViva.Engine;

namespace QuestViva.EngineTests;

[TestClass]
public class ComplexTypesTests
{
    private Element _object = null!;
    private WorldModel _worldModel = null!;

    [TestInitialize]
    public void Setup()
    {
        _worldModel = Helpers.CreateWorldModel();

        _object = _worldModel.GetElementFactory(ElementType.Object).Create("object");
        var list = new QuestList<object> {"string1"};
        var dictionary = new QuestDictionary<object> {{"key1", "nested string"}};
        list.Add(dictionary);
        _object.Fields.Set("list", list);
        _object.Fields.Resolve(null!);
    }

    [TestMethod]
    public void TestSetup()
    {
        var obj = _worldModel.Elements.Get("object");
        Assert.IsNotNull(obj);
        var list = obj.Fields.GetAsType<QuestList<object>>("list");
        Assert.IsNotNull(list);
        Assert.AreEqual("string1", list[0]);
        var dictionary = list[1] as QuestDictionary<object>;
        Assert.IsNotNull(dictionary);
        Assert.IsTrue(dictionary.ContainsKey("key1"));
        Assert.AreEqual("nested string", dictionary["key1"]);
    }

    [TestMethod]
    public void TestAddListItemUndoRedo()
    {
        var obj = _worldModel.Elements.Get("object");
        var list = obj.Fields.GetAsType<QuestList<object>>("list")!;
        Assert.AreEqual(2, list.Count);

        _worldModel.UndoLogger.StartTransaction("Add list item");
        list.Add("new item");
        _worldModel.UndoLogger.EndTransaction();

        Assert.AreEqual(3, list.Count);

        _worldModel.UndoLogger.Undo();
        Assert.AreEqual(2, list.Count);
    }

    [TestMethod]
    public void TestNestedDictionaryAddUndoRedo()
    {
        var obj = _worldModel.Elements.Get("object");
        var list = obj.Fields.GetAsType<QuestList<object>>("list")!;
        var dictionary = (QuestDictionary<object>) list[1]!;
        Assert.AreEqual(1, dictionary.Count);

        _worldModel.UndoLogger.StartTransaction("Add dictionary item");
        dictionary.Add("key2", "new string");
        _worldModel.UndoLogger.EndTransaction();

        Assert.AreEqual(2, dictionary.Count);

        _worldModel.UndoLogger.Undo();
        Assert.AreEqual(1, dictionary.Count);
    }
}