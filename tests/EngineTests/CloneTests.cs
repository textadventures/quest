using QuestViva.Engine;

namespace QuestViva.EngineTests;

[TestClass]
public class CloneTests
{
    private const string attributeName = "attribute";
    private const string attributeValue = "attributevalue";
    private const string listAttributeName = "listattribute";
    private readonly List<string> listAttributeValue = new() {"one", "two", "three"};
    private Element m_original;

    private WorldModel m_worldModel;

    [TestInitialize]
    public void Setup()
    {
        m_worldModel = Helpers.CreateWorldModel();

        m_original = m_worldModel.GetElementFactory(ElementType.Object).Create("original");
        m_original.Fields.Set(attributeName, attributeValue);
        m_original.Fields.Set(listAttributeName, new QuestList<string>(listAttributeValue));
        m_original.Fields.Resolve(null);
        Assert.AreEqual(attributeValue, m_original.Fields.GetString(attributeName));
        Assert.AreEqual(3, m_original.Fields.GetAsType<QuestList<string>>(listAttributeName).Count);
    }

    [TestMethod]
    public void TestClone()
    {
        var clone = m_original.Clone();

        // Original and clone must be different objects
        Assert.AreNotSame(m_original, clone);

        // Attribute values must be the same
        Assert.AreEqual(attributeValue, clone.Fields.GetString(attributeName));
        Assert.AreEqual(3, clone.Fields.GetAsType<QuestList<string>>(listAttributeName).Count);

        // Names must not match
        Assert.AreNotEqual(m_original.Name, clone.Name);

        // Both original and clone must be accessible by their names
        Assert.AreSame(m_original, m_worldModel.Elements.Get(m_original.Name));
        Assert.AreSame(clone, m_worldModel.Elements.Get(clone.Name));
    }

    [TestMethod]
    public async Task TestUndoCloning()
    {
        var originalObjectCount = m_worldModel.Elements.Count(ElementType.Object);

        m_worldModel.UndoLogger.StartTransaction("Create clone");
        var clone = m_original.Clone();
        m_worldModel.UndoLogger.EndTransaction();

        Assert.AreEqual(originalObjectCount + 1, m_worldModel.Elements.Count(ElementType.Object));

        await m_worldModel.UndoLogger.Undo();
        Assert.AreEqual(originalObjectCount, m_worldModel.Elements.Count(ElementType.Object));

        m_worldModel.UndoLogger.Redo();
        Assert.AreEqual(originalObjectCount + 1, m_worldModel.Elements.Count(ElementType.Object));

        await m_worldModel.UndoLogger.Undo();
        Assert.AreEqual(originalObjectCount, m_worldModel.Elements.Count(ElementType.Object));

        m_worldModel.UndoLogger.Redo();
        Assert.AreEqual(originalObjectCount + 1, m_worldModel.Elements.Count(ElementType.Object));
    }

    [TestMethod]
    public async Task TestChangingClonedStringAttribute()
    {
        const string newAttributeValue = "newattributevalue";

        var clone = m_original.Clone();

        m_worldModel.UndoLogger.StartTransaction("Change attribute");
        clone.Fields.Set(attributeName, newAttributeValue);
        m_worldModel.UndoLogger.EndTransaction();

        // Cloned's field value is changed
        Assert.AreEqual(newAttributeValue, clone.Fields.GetString(attributeName));

        // Original's field value is not changed
        Assert.AreEqual(attributeValue, m_original.Fields.GetString(attributeName));

        await m_worldModel.UndoLogger.Undo();

        // Cloned's field value is back to original value
        Assert.AreEqual(attributeValue, clone.Fields.GetString(attributeName));
    }

    [TestMethod]
    public async Task TestChangingClonedListAttribute()
    {
        var clone = m_original.Clone();

        m_worldModel.UndoLogger.StartTransaction("Change attribute");
        clone.Fields.GetAsType<QuestList<string>>(listAttributeName).Add("newvalue");
        m_worldModel.UndoLogger.EndTransaction();

        // Cloned's field value is changed
        Assert.AreEqual(4, clone.Fields.GetAsType<QuestList<string>>(listAttributeName).Count);

        // Original's field value is not changed
        Assert.AreEqual(3, m_original.Fields.GetAsType<QuestList<string>>(listAttributeName).Count);

        await m_worldModel.UndoLogger.Undo();

        // Cloned's field value is back to original value
        Assert.AreEqual(3, clone.Fields.GetAsType<QuestList<string>>(listAttributeName).Count);
    }

    [TestMethod]
    public void TestMultipleClones()
    {
        var clone = m_original.Clone();
        var clone2 = m_original.Clone();
        var clone3 = m_original.Clone();
        var clone4 = clone.Clone();
        var clone5 = clone.Clone();
        var clone6 = clone4.Clone();

        Assert.AreEqual(m_original.Name + "1", clone.Name);
        Assert.AreEqual(m_original.Name + "2", clone2.Name);
        Assert.AreEqual(m_original.Name + "3", clone3.Name);
        Assert.AreEqual(m_original.Name + "4", clone4.Name);
        Assert.AreEqual(m_original.Name + "5", clone5.Name);
        Assert.AreEqual(m_original.Name + "6", clone6.Name);
    }

    [TestMethod]
    public async Task TestCloneElementWithChildren()
    {
        // Create a child object of the original
        const string childAttrName = "childattribute";
        const string childAttrValue = "childvalue";
        var child = m_worldModel.GetElementFactory(ElementType.Object).Create("child");
        child.Fields.Set(childAttrName, childAttrValue);
        child.Parent = m_original;

        var originalElementCount = m_worldModel.Elements.Count(ElementType.Object);

        // Clone the original object. The cloned object should have a cloned child too.
        m_worldModel.UndoLogger.StartTransaction("Create clone");
        var clone = m_original.Clone();
        m_worldModel.UndoLogger.EndTransaction();

        // We should now have 2 more objects
        Assert.AreEqual(originalElementCount + 2, m_worldModel.Elements.Count(ElementType.Object));

        var cloneChildren = new List<Element>(m_worldModel.Elements.GetChildElements(clone));
        var originalChildren = new List<Element>(m_worldModel.Elements.GetChildElements(m_original));

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
        await m_worldModel.UndoLogger.Undo();
        Assert.AreEqual(originalElementCount, m_worldModel.Elements.Count(ElementType.Object));
    }
}