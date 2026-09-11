using QuestViva.Engine;

namespace QuestViva.EngineTests;

[TestClass]
public class CloneTests
{
    private const string AttributeName = "attribute";
    private const string AttributeValue = "attributevalue";
    private const string ListAttributeName = "listattribute";
    private readonly List<string> _listAttributeValue = ["one", "two", "three"];
    private Element _original = null!;

    private WorldModel _worldModel = null!;

    [TestInitialize]
    public void Setup()
    {
        _worldModel = Helpers.CreateWorldModel();

        _original = _worldModel.GetElementFactory(ElementType.Object).Create("original");
        _original.Fields.Set(AttributeName, AttributeValue);
        _original.Fields.Set(ListAttributeName, new QuestList<string>(_listAttributeValue));
        _original.Fields.Resolve(null!);
        Assert.AreEqual(AttributeValue, _original.Fields.GetString(AttributeName));
        Assert.AreEqual(3, _original.Fields.GetAsType<QuestList<string>>(ListAttributeName)!.Count);
    }

    [TestMethod]
    public void TestClone()
    {
        var clone = _original.Clone();

        // Original and clone must be different objects
        Assert.AreNotSame(_original, clone);

        // Attribute values must be the same
        Assert.AreEqual(AttributeValue, clone.Fields.GetString(AttributeName));
        Assert.AreEqual(3, clone.Fields.GetAsType<QuestList<string>>(ListAttributeName)!.Count);

        // Names must not match
        Assert.AreNotEqual(_original.Name, clone.Name);

        // Both original and clone must be accessible by their names
        Assert.AreSame(_original, _worldModel.Elements.Get(_original.Name));
        Assert.AreSame(clone, _worldModel.Elements.Get(clone.Name));
    }

    [TestMethod]
    public async Task TestUndoCloning()
    {
        var originalObjectCount = _worldModel.Elements.Count(ElementType.Object);

        _worldModel.UndoLogger.StartTransaction("Create clone");
        var clone = _original.Clone();
        _worldModel.UndoLogger.EndTransaction();

        Assert.AreEqual(originalObjectCount + 1, _worldModel.Elements.Count(ElementType.Object));

        await _worldModel.UndoLogger.Undo();
        Assert.AreEqual(originalObjectCount, _worldModel.Elements.Count(ElementType.Object));

        _worldModel.UndoLogger.Redo();
        Assert.AreEqual(originalObjectCount + 1, _worldModel.Elements.Count(ElementType.Object));

        await _worldModel.UndoLogger.Undo();
        Assert.AreEqual(originalObjectCount, _worldModel.Elements.Count(ElementType.Object));

        _worldModel.UndoLogger.Redo();
        Assert.AreEqual(originalObjectCount + 1, _worldModel.Elements.Count(ElementType.Object));
    }

    [TestMethod]
    public async Task TestChangingClonedStringAttribute()
    {
        const string newAttributeValue = "newattributevalue";

        var clone = _original.Clone();

        _worldModel.UndoLogger.StartTransaction("Change attribute");
        clone.Fields.Set(AttributeName, newAttributeValue);
        _worldModel.UndoLogger.EndTransaction();

        // Cloned's field value is changed
        Assert.AreEqual(newAttributeValue, clone.Fields.GetString(AttributeName));

        // Original's field value is not changed
        Assert.AreEqual(AttributeValue, _original.Fields.GetString(AttributeName));

        await _worldModel.UndoLogger.Undo();

        // Cloned's field value is back to original value
        Assert.AreEqual(AttributeValue, clone.Fields.GetString(AttributeName));
    }

    [TestMethod]
    public async Task TestChangingClonedListAttribute()
    {
        var clone = _original.Clone();

        _worldModel.UndoLogger.StartTransaction("Change attribute");
        clone.Fields.GetAsType<QuestList<string>>(ListAttributeName)!.Add("newvalue");
        _worldModel.UndoLogger.EndTransaction();

        // Cloned's field value is changed
        Assert.AreEqual(4, clone.Fields.GetAsType<QuestList<string>>(ListAttributeName)!.Count);

        // Original's field value is not changed
        Assert.AreEqual(3, _original.Fields.GetAsType<QuestList<string>>(ListAttributeName)!.Count);

        await _worldModel.UndoLogger.Undo();

        // Cloned's field value is back to original value
        Assert.AreEqual(3, clone.Fields.GetAsType<QuestList<string>>(ListAttributeName)!.Count);
    }

    [TestMethod]
    public void TestMultipleClones()
    {
        var clone = _original.Clone();
        var clone2 = _original.Clone();
        var clone3 = _original.Clone();
        var clone4 = clone.Clone();
        var clone5 = clone.Clone();
        var clone6 = clone4.Clone();

        Assert.AreEqual(_original.Name + "1", clone.Name);
        Assert.AreEqual(_original.Name + "2", clone2.Name);
        Assert.AreEqual(_original.Name + "3", clone3.Name);
        Assert.AreEqual(_original.Name + "4", clone4.Name);
        Assert.AreEqual(_original.Name + "5", clone5.Name);
        Assert.AreEqual(_original.Name + "6", clone6.Name);
    }

    [TestMethod]
    public async Task TestCloneElementWithChildren()
    {
        // Create a child object of the original
        const string childAttrName = "childattribute";
        const string childAttrValue = "childvalue";
        var child = _worldModel.GetElementFactory(ElementType.Object).Create("child");
        child.Fields.Set(childAttrName, childAttrValue);
        child.Parent = _original;

        var originalElementCount = _worldModel.Elements.Count(ElementType.Object);

        // Clone the original object. The cloned object should have a cloned child too.
        _worldModel.UndoLogger.StartTransaction("Create clone");
        var clone = _original.Clone();
        _worldModel.UndoLogger.EndTransaction();

        // We should now have 2 more objects
        Assert.AreEqual(originalElementCount + 2, _worldModel.Elements.Count(ElementType.Object));

        var cloneChildren = new List<Element>(_worldModel.Elements.GetChildElements(clone));
        var originalChildren = new List<Element>(_worldModel.Elements.GetChildElements(_original));

        // Check the original and the clone now each have one child
        Assert.AreEqual(1, cloneChildren.Count);
        Assert.AreEqual(1, originalChildren.Count);

        // Check the children are not the same, but that the cloned child has the correct attributes
        Assert.AreNotSame(originalChildren[0], cloneChildren[0]);
        Assert.AreNotEqual(originalChildren[0].Name, cloneChildren[0].Name);
        Assert.AreSame(child, originalChildren[0]);
        Assert.AreEqual("child", originalChildren[0].Name);
        Assert.AreEqual("child1", cloneChildren[0].Name);
        Assert.AreEqual(childAttrValue, cloneChildren[0].Fields.GetString(childAttrName));

        // Now undo, and verify we have the original number of objects again
        await _worldModel.UndoLogger.Undo();
        Assert.AreEqual(originalElementCount, _worldModel.Elements.Count(ElementType.Object));
    }
}