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

    [DataTestMethod]
    [DataRow("object.attribute", AttributeValue)]
    [DataRow("child.attribute", ChildAttributeValue)]
    [DataRow("child.parent.attribute", AttributeValue)]
    [DataRow("object.attribute + \"testconcat\"", AttributeValue + "testconcat")]
    public void TestStringFields(string expression, string expectedResult)
    {
        var result = RunExpression<string>(expression);
        Assert.AreEqual(expectedResult, result);
    }

    [DataTestMethod]
    [DataRow("object.intattribute", IntAttributeValue)]
    [DataRow("object.intattribute + 3", IntAttributeValue + 3)]
    public void TestIntFields(string expression, int expectedResult)
    {
        var result = RunExpression<int>(expression);
        Assert.AreEqual(expectedResult, result);
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