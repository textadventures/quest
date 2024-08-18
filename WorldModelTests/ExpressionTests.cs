using TextAdventures.Quest;
using TextAdventures.Quest.Functions;
using TextAdventures.Quest.Scripts;

namespace WorldModelTests;

[TestClass]
public class ExpressionTests
{
    private const string AttributeName = "attribute";
    private const string AttributeValue = "attributevalue";
    private const string ChildAttributeValue = "childattributevalue";

    private const string IntAttributeName = "intattribute";
    private const int IntAttributeValue = 42;

    private WorldModel _worldModel;
    private Element _object;
    private Element _child;

    [TestInitialize]
    public void Setup()
    {
        _worldModel = new WorldModel();
        _object = _worldModel.GetElementFactory(ElementType.Object).Create("object");
        _object.Fields.Set(AttributeName, AttributeValue);
        _object.Fields.Set(IntAttributeName, IntAttributeValue);

        _child = _worldModel.GetElementFactory(ElementType.Object).Create("child");
        _child.Parent = _object;
        _child.Fields.Set(AttributeName, ChildAttributeValue);
    }

    private T RunExpression<T>(string expression)
    {
        var expr = new Expression<T>(expression, new ScriptContext(_worldModel));
        var c = new Context();
        return expr.Execute(c);
    }

    [TestMethod]
    public void TestReadStringFields()
    {
        var result = RunExpression<string>("object.attribute");
        Assert.AreEqual(AttributeValue, result);

        result = RunExpression<string>("child.attribute");
        Assert.AreEqual(ChildAttributeValue, result);
    }

    [TestMethod]
    public void TestReadChildParentField()
    {
        var result = RunExpression<string>("child.parent.attribute");
        Assert.AreEqual(AttributeValue, result);
    }

    [TestMethod]
    public void TestStringConcatenate()
    {
        const string extraString = "testconcat";
        var result = RunExpression<string>("object.attribute + \"" + extraString + "\"");
        Assert.AreEqual(AttributeValue + extraString, result);
    }

    [TestMethod]
    public void TestReadIntField()
    {
        var result = RunExpression<int>("object.intattribute");
        Assert.AreEqual(IntAttributeValue, result);
    }

    [TestMethod]
    public void TestAddition()
    {
        var result = RunExpression<int>("object.intattribute + 3");
        Assert.AreEqual(IntAttributeValue + 3, result);
    }

    [TestMethod]
    public void TestChangingTypes()
    {
        var scriptContext = new ScriptContext(_worldModel);
        var expression = "a + b";
        var expr = new ExpressionGeneric(expression, scriptContext);
        var c = new Context
        {
            Parameters = new Parameters
            {
                {"a", 1},
                {"b", 2}
            }
        };

        Assert.AreEqual(3, (int)expr.Execute(c));

        c.Parameters["a"] = "A";
        c.Parameters["b"] = "B";

        Assert.AreEqual("AB", (string)expr.Execute(c));
    }
}