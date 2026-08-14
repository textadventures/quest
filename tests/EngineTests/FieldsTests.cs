using QuestViva.Engine;

namespace QuestViva.EngineTests;

[TestClass]
public class FieldsTests
{
    private const string inheritedAttributeName = "inheritedattribute";
    private const string inheritedAttributeValue = "inheritedattributevalue";

    private const string inheritedAttribute2Name = "otherattribute";
    private const string inheritedAttribute2Value = "othervalue";

    private const string attributeDefinedByDefaultName = "somedefaultattribute";
    private const string attributeDefinedByDefaultValue = "somedefaultvalue";

    private const string attributeDefinedByDefault2Name = "otherdefaultattribute";
    private const string attributeDefinedByDefault2Value = "otherdefaultvalue";
    private const string attributeDefinedByDefault2OverriddenValue = "overriddendefaultvalue";
    private Element m_defaultType;
    private Element m_object;
    private Element m_objectType;
    private Element m_subType;

    private WorldModel m_worldModel;

    [TestInitialize]
    public void Setup()
    {
        const string inheritedTypeName = "inherited";
        const string subInheritedTypeName = "subtype";
        const string defaultObject = "defaultobject";

        m_worldModel = Helpers.CreateWorldModel();

        m_defaultType = m_worldModel.GetElementFactory(ElementType.ObjectType).Create(defaultObject);
        m_defaultType.Fields.Set(attributeDefinedByDefaultName, attributeDefinedByDefaultValue);
        m_defaultType.Fields.Set(attributeDefinedByDefault2Name, attributeDefinedByDefault2Value);

        m_subType = m_worldModel.GetElementFactory(ElementType.ObjectType).Create(subInheritedTypeName);
        m_subType.Fields.Set(inheritedAttribute2Name, inheritedAttribute2Value);
        m_subType.Fields.Set(attributeDefinedByDefault2Name, attributeDefinedByDefault2OverriddenValue);

        m_objectType = m_worldModel.GetElementFactory(ElementType.ObjectType).Create(inheritedTypeName);
        m_objectType.Fields.Set(inheritedAttributeName, inheritedAttributeValue);
        m_objectType.Fields.AddType(m_subType);

        m_object = m_worldModel.GetElementFactory(ElementType.Object).Create("object");
        m_object.Fields.Resolve(null);
        m_object.Fields.AddType(m_objectType);
    }

    [TestMethod]
    public void TestStringFields()
    {
        const string property = "property";
        const string value1 = "first value";
        const string value2 = "second value";
        m_object.Fields.Set(property, value1);
        Assert.AreEqual(value1, m_object.Fields.Get(property));

        m_worldModel.UndoLogger.StartTransaction("Set value2");
        m_object.Fields.Set(property, value2);
        m_worldModel.UndoLogger.EndTransaction();

        Assert.AreEqual(value2, m_object.Fields.Get(property));
        m_worldModel.UndoLogger.Undo();
        Assert.AreEqual(value1, m_object.Fields.Get(property));
    }

    [TestMethod]
    public void TestInheritedFields()
    {
        const string newValue = "newvalue";
        Assert.AreEqual(inheritedAttributeValue, m_object.Fields.Get(inheritedAttributeName));

        m_worldModel.UndoLogger.StartTransaction("Override inherited field value");
        m_object.Fields.Set(inheritedAttributeName, newValue);
        m_worldModel.UndoLogger.EndTransaction();

        Assert.AreEqual(newValue, m_object.Fields.Get(inheritedAttributeName));
        m_worldModel.UndoLogger.Undo();
        Assert.AreEqual(inheritedAttributeValue, m_object.Fields.Get(inheritedAttributeName));
    }

    [TestMethod]
    public void TestSubinheritedFields()
    {
        // Test fields from:
        //    m_subType -> m_objectType -> m_object
        Assert.AreEqual(inheritedAttribute2Value, m_object.Fields.Get(inheritedAttribute2Name));

        // Test that defaultobject values are picked up
        Assert.AreEqual(attributeDefinedByDefaultValue, m_object.Fields.Get(attributeDefinedByDefaultName));

        // Test a defaultobject value overridden by a subtype
        Assert.AreEqual(attributeDefinedByDefault2OverriddenValue, m_object.Fields.Get(attributeDefinedByDefault2Name));
    }

    [TestMethod]
    public void TestObjectCreationUndo()
    {
        m_worldModel.UndoLogger.StartTransaction("Create new object");
        m_worldModel.GetElementFactory(ElementType.Object).Create("newobj");
        m_worldModel.UndoLogger.EndTransaction();

        // There should be 3 elements now - game, object, newobj
        Assert.AreEqual(3, m_worldModel.Elements.GetElements(ElementType.Object).Count());

        m_worldModel.UndoLogger.Undo();

        // Now we should have 2 elements again
        Assert.AreEqual(2, m_worldModel.Elements.GetElements(ElementType.Object).Count());
    }

    [TestMethod]
    public void TestObjectDeletionUndo()
    {
        var obj = m_worldModel.GetElementFactory(ElementType.Object).Create("newobj");
        var type = m_worldModel.GetElementFactory(ElementType.ObjectType).Create("objtype");
        type.Fields.Set("attrfromtype", "attrvalue");
        type.Fields.Set("overridenattr", "valuefromtype1");
        var type2 = m_worldModel.GetElementFactory(ElementType.ObjectType).Create("objtype2");
        type2.Fields.Set("overridenattr", "valuefromtype2");
        obj.Fields.AddType(type);
        obj.Fields.AddType(type2);
        obj.Fields.Set("attrfromobj", "fromobjvalue");
        obj.Fields.Resolve(null);

        // Test initial values are correct first
        Assert.AreEqual("fromobjvalue", obj.Fields.GetString("attrfromobj"));
        Assert.AreEqual("attrvalue", obj.Fields.GetString("attrfromtype"));
        Assert.AreEqual("valuefromtype2", obj.Fields.GetString("overridenattr"));
        Assert.AreEqual(attributeDefinedByDefaultValue, obj.Fields.GetString(attributeDefinedByDefaultName));
        Assert.AreEqual(attributeDefinedByDefault2Value, obj.Fields.GetString(attributeDefinedByDefault2Name));

        // There should be 3 elements now - game, object, newobj
        Assert.AreEqual(3, m_worldModel.Elements.GetElements(ElementType.Object).Count());

        // Delete the object
        m_worldModel.UndoLogger.StartTransaction("Destroy object");
        m_worldModel.GetElementFactory(ElementType.Object).DestroyElement("newobj");
        m_worldModel.UndoLogger.EndTransaction();

        // Now we should have 2 elements again
        Assert.AreEqual(2, m_worldModel.Elements.GetElements(ElementType.Object).Count());

        // Undo the deletion
        m_worldModel.UndoLogger.Undo();

        // There should be 3 elements now - game, object, newobj
        Assert.AreEqual(3, m_worldModel.Elements.GetElements(ElementType.Object).Count());

        // Ensure our object reference is pointing to the one in the worldmodel
        obj = m_worldModel.Elements.Get(ElementType.Object, "newobj");

        // Test the initial values again
        Assert.AreEqual("fromobjvalue", obj.Fields.GetString("attrfromobj"));
        Assert.AreEqual("attrvalue", obj.Fields.GetString("attrfromtype"));
        Assert.AreEqual("valuefromtype2", obj.Fields.GetString("overridenattr"));
        Assert.AreEqual(attributeDefinedByDefaultValue, obj.Fields.GetString(attributeDefinedByDefaultName));
    }

    [TestMethod]
    public void TestMetaFieldUndoRedo()
    {
        var obj = m_worldModel.GetElementFactory(ElementType.Object).Create("newobj");
        const string initialValue = "initialValue";
        const string alteredValue = "alteredValue";

        obj.MetaFields[MetaFieldDefinitions.Filename] = initialValue;
        Assert.AreEqual(initialValue, obj.MetaFields[MetaFieldDefinitions.Filename]);

        m_worldModel.UndoLogger.StartTransaction("Set metafield");
        obj.MetaFields[MetaFieldDefinitions.Filename] = alteredValue;
        m_worldModel.UndoLogger.EndTransaction();

        Assert.AreEqual(alteredValue, obj.MetaFields[MetaFieldDefinitions.Filename]);

        m_worldModel.UndoLogger.Undo();

        Assert.AreEqual(initialValue, obj.MetaFields[MetaFieldDefinitions.Filename]);
    }
}