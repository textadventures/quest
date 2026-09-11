using QuestViva.Engine;

namespace QuestViva.EngineTests;

[TestClass]
public class FieldsTests
{
    private const string InheritedAttributeName = "inheritedattribute";
    private const string InheritedAttributeValue = "inheritedattributevalue";

    private const string InheritedAttribute2Name = "otherattribute";
    private const string InheritedAttribute2Value = "othervalue";

    private const string AttributeDefinedByDefaultName = "somedefaultattribute";
    private const string AttributeDefinedByDefaultValue = "somedefaultvalue";

    private const string AttributeDefinedByDefault2Name = "otherdefaultattribute";
    private const string AttributeDefinedByDefault2Value = "otherdefaultvalue";
    private const string AttributeDefinedByDefault2OverriddenValue = "overriddendefaultvalue";
    private Element _defaultType = null!;
    private Element _object = null!;
    private Element _objectType = null!;
    private Element _subType = null!;

    private WorldModel _worldModel = null!;

    [TestInitialize]
    public void Setup()
    {
        const string inheritedTypeName = "inherited";
        const string subInheritedTypeName = "subtype";
        const string defaultObject = "defaultobject";

        _worldModel = Helpers.CreateWorldModel();

        _defaultType = _worldModel.GetElementFactory(ElementType.ObjectType).Create(defaultObject);
        _defaultType.Fields.Set(AttributeDefinedByDefaultName, AttributeDefinedByDefaultValue);
        _defaultType.Fields.Set(AttributeDefinedByDefault2Name, AttributeDefinedByDefault2Value);

        _subType = _worldModel.GetElementFactory(ElementType.ObjectType).Create(subInheritedTypeName);
        _subType.Fields.Set(InheritedAttribute2Name, InheritedAttribute2Value);
        _subType.Fields.Set(AttributeDefinedByDefault2Name, AttributeDefinedByDefault2OverriddenValue);

        _objectType = _worldModel.GetElementFactory(ElementType.ObjectType).Create(inheritedTypeName);
        _objectType.Fields.Set(InheritedAttributeName, InheritedAttributeValue);
        _objectType.Fields.AddType(_subType);

        _object = _worldModel.GetElementFactory(ElementType.Object).Create("object");
        _object.Fields.Resolve(null!);
        _object.Fields.AddType(_objectType);
    }

    [TestMethod]
    public void TestStringFields()
    {
        const string property = "property";
        const string value1 = "first value";
        const string value2 = "second value";
        _object.Fields.Set(property, value1);
        Assert.AreEqual(value1, _object.Fields.Get(property));

        _worldModel.UndoLogger.StartTransaction("Set value2");
        _object.Fields.Set(property, value2);
        _worldModel.UndoLogger.EndTransaction();

        Assert.AreEqual(value2, _object.Fields.Get(property));
        _worldModel.UndoLogger.Undo();
        Assert.AreEqual(value1, _object.Fields.Get(property));
    }

    [TestMethod]
    public void TestInheritedFields()
    {
        const string newValue = "newvalue";
        Assert.AreEqual(InheritedAttributeValue, _object.Fields.Get(InheritedAttributeName));

        _worldModel.UndoLogger.StartTransaction("Override inherited field value");
        _object.Fields.Set(InheritedAttributeName, newValue);
        _worldModel.UndoLogger.EndTransaction();

        Assert.AreEqual(newValue, _object.Fields.Get(InheritedAttributeName));
        _worldModel.UndoLogger.Undo();
        Assert.AreEqual(InheritedAttributeValue, _object.Fields.Get(InheritedAttributeName));
    }

    [TestMethod]
    public void TestSubinheritedFields()
    {
        // Test fields from:
        //    _subType -> _objectType -> _object
        Assert.AreEqual(InheritedAttribute2Value, _object.Fields.Get(InheritedAttribute2Name));

        // Test that defaultobject values are picked up
        Assert.AreEqual(AttributeDefinedByDefaultValue, _object.Fields.Get(AttributeDefinedByDefaultName));

        // Test a defaultobject value overridden by a subtype
        Assert.AreEqual(AttributeDefinedByDefault2OverriddenValue, _object.Fields.Get(AttributeDefinedByDefault2Name));
    }

    [TestMethod]
    public void TestObjectCreationUndo()
    {
        _worldModel.UndoLogger.StartTransaction("Create new object");
        _worldModel.GetElementFactory(ElementType.Object).Create("newobj");
        _worldModel.UndoLogger.EndTransaction();

        // There should be 3 elements now - game, object, newobj
        Assert.AreEqual(3, _worldModel.Elements.GetElements(ElementType.Object).Count());

        _worldModel.UndoLogger.Undo();

        // Now we should have 2 elements again
        Assert.AreEqual(2, _worldModel.Elements.GetElements(ElementType.Object).Count());
    }

    [TestMethod]
    public void TestObjectDeletionUndo()
    {
        var obj = _worldModel.GetElementFactory(ElementType.Object).Create("newobj");
        var type = _worldModel.GetElementFactory(ElementType.ObjectType).Create("objtype");
        type.Fields.Set("attrfromtype", "attrvalue");
        type.Fields.Set("overridenattr", "valuefromtype1");
        var type2 = _worldModel.GetElementFactory(ElementType.ObjectType).Create("objtype2");
        type2.Fields.Set("overridenattr", "valuefromtype2");
        obj.Fields.AddType(type);
        obj.Fields.AddType(type2);
        obj.Fields.Set("attrfromobj", "fromobjvalue");
        obj.Fields.Resolve(null!);

        // Test initial values are correct first
        Assert.AreEqual("fromobjvalue", obj.Fields.GetString("attrfromobj"));
        Assert.AreEqual("attrvalue", obj.Fields.GetString("attrfromtype"));
        Assert.AreEqual("valuefromtype2", obj.Fields.GetString("overridenattr"));
        Assert.AreEqual(AttributeDefinedByDefaultValue, obj.Fields.GetString(AttributeDefinedByDefaultName));
        Assert.AreEqual(AttributeDefinedByDefault2Value, obj.Fields.GetString(AttributeDefinedByDefault2Name));

        // There should be 3 elements now - game, object, newobj
        Assert.AreEqual(3, _worldModel.Elements.GetElements(ElementType.Object).Count());

        // Delete the object
        _worldModel.UndoLogger.StartTransaction("Destroy object");
        _worldModel.GetElementFactory(ElementType.Object).DestroyElement("newobj");
        _worldModel.UndoLogger.EndTransaction();

        // Now we should have 2 elements again
        Assert.AreEqual(2, _worldModel.Elements.GetElements(ElementType.Object).Count());

        // Undo the deletion
        _worldModel.UndoLogger.Undo();

        // There should be 3 elements now - game, object, newobj
        Assert.AreEqual(3, _worldModel.Elements.GetElements(ElementType.Object).Count());

        // Ensure our object reference is pointing to the one in the worldmodel
        obj = _worldModel.Elements.Get(ElementType.Object, "newobj");

        // Test the initial values again
        Assert.AreEqual("fromobjvalue", obj.Fields.GetString("attrfromobj"));
        Assert.AreEqual("attrvalue", obj.Fields.GetString("attrfromtype"));
        Assert.AreEqual("valuefromtype2", obj.Fields.GetString("overridenattr"));
        Assert.AreEqual(AttributeDefinedByDefaultValue, obj.Fields.GetString(AttributeDefinedByDefaultName));
    }

    [TestMethod]
    public void TestMetaFieldUndoRedo()
    {
        var obj = _worldModel.GetElementFactory(ElementType.Object).Create("newobj");
        const string initialValue = "initialValue";
        const string alteredValue = "alteredValue";

        obj.MetaFields[MetaFieldDefinitions.Filename] = initialValue;
        Assert.AreEqual(initialValue, obj.MetaFields[MetaFieldDefinitions.Filename]);

        _worldModel.UndoLogger.StartTransaction("Set metafield");
        obj.MetaFields[MetaFieldDefinitions.Filename] = alteredValue;
        _worldModel.UndoLogger.EndTransaction();

        Assert.AreEqual(alteredValue, obj.MetaFields[MetaFieldDefinitions.Filename]);

        _worldModel.UndoLogger.Undo();

        Assert.AreEqual(initialValue, obj.MetaFields[MetaFieldDefinitions.Filename]);
    }
}