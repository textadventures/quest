using QuestViva.Engine;

namespace QuestViva.EngineTests;

[TestClass]
public class ElementsTests
{
    private WorldModel _worldModel;

    [TestInitialize]
    public void Setup()
    {
        _worldModel = Helpers.CreateWorldModel();

        // a
        // - b
        //   - c
        //   - d
        // - e
        // f

        var a = _worldModel.GetElementFactory(ElementType.Object).Create("a");
        var b = _worldModel.GetElementFactory(ElementType.Object).Create("b");
        b.Parent = a;
        var c = _worldModel.GetElementFactory(ElementType.Object).Create("c");
        c.Parent = b;
        var d = _worldModel.GetElementFactory(ElementType.Object).Create("d");
        d.Parent = b;
        var e = _worldModel.GetElementFactory(ElementType.Object).Create("e");
        e.Parent = a;
        var f = _worldModel.GetElementFactory(ElementType.Object).Create("f");
    }

    [TestMethod]
    public void TestGetChildrenOfA()
    {
        var childList = new List<string>(
            _worldModel.Elements.GetChildElements(_worldModel.Elements.Get("a")).Select(e => e.Name));

        // all children of a should be b,c,d,e

        Assert.AreEqual(4, childList.Count);
        Assert.AreEqual("b", childList[0]);
        Assert.AreEqual("c", childList[1]);
        Assert.AreEqual("d", childList[2]);
        Assert.AreEqual("e", childList[3]);
    }

    [TestMethod]
    public void TestGetChildrenOfF()
    {
        var childList = new List<string>(
            _worldModel.Elements.GetChildElements(_worldModel.Elements.Get("f")).Select(e => e.Name));

        // no children of f

        Assert.AreEqual(0, childList.Count);
    }

    [TestMethod]
    public void TestSortIndexes()
    {
        // d is after c
        Assert.IsTrue(_worldModel.Elements.Get("d").MetaFields[MetaFieldDefinitions.SortIndex]
                      > _worldModel.Elements.Get("c").MetaFields[MetaFieldDefinitions.SortIndex],
            "d should be after c in the sort order");

        // e is after b
        Assert.IsTrue(_worldModel.Elements.Get("e").MetaFields[MetaFieldDefinitions.SortIndex]
                      > _worldModel.Elements.Get("b").MetaFields[MetaFieldDefinitions.SortIndex],
            "e should be after b in the sort order");

        // f is after a
        Assert.IsTrue(_worldModel.Elements.Get("f").MetaFields[MetaFieldDefinitions.SortIndex]
                      > _worldModel.Elements.Get("a").MetaFields[MetaFieldDefinitions.SortIndex],
            "f should be after a in the sort order");
    }

    [TestMethod]
    public void TestSortIndexesOnMove()
    {
        // move d as child of a, so new tree looks like this:
        // a
        // - b
        //   - c
        // - e
        // - d
        // f

        var d = _worldModel.Elements.Get("d");
        d.Parent = _worldModel.Elements.Get("a");

        // e is after b
        Assert.IsTrue(_worldModel.Elements.Get("e").MetaFields[MetaFieldDefinitions.SortIndex]
                      > _worldModel.Elements.Get("b").MetaFields[MetaFieldDefinitions.SortIndex],
            "e should be after b in the sort order");

        // d is after e
        Assert.IsTrue(_worldModel.Elements.Get("d").MetaFields[MetaFieldDefinitions.SortIndex]
                      > _worldModel.Elements.Get("e").MetaFields[MetaFieldDefinitions.SortIndex],
            "d should be after e in the sort order");
    }

    [TestMethod]
    public void UpdateParentByFieldName()
    {
        var element = _worldModel.GetElementFactory(ElementType.Object).Create("element");
        var parent1 = _worldModel.GetElementFactory(ElementType.Object).Create("parent");
        var parent2 = _worldModel.GetElementFactory(ElementType.Object).Create("parent2");

        element.Parent = parent1;
        Assert.AreEqual(parent1, element.Parent);

        element.Fields.Set("parent", parent2);
        Assert.AreEqual(parent2, element.Parent);
    }
}