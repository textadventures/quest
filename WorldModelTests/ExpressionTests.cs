using Shouldly;
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

    private ScriptContext _scriptContext;

    [TestInitialize]
    public void Setup()
    {
        _worldModel = new WorldModel();
        _scriptContext = new ScriptContext(_worldModel);
        _object = _worldModel.GetElementFactory(ElementType.Object).Create("object");
        _object.Fields.Set(AttributeName, AttributeValue);
        _object.Fields.Set(IntAttributeName, IntAttributeValue);

        _child = _worldModel.GetElementFactory(ElementType.Object).Create("child");
        _child.Parent = _object;
        _child.Fields.Set(AttributeName, ChildAttributeValue);
    }

    private T RunExpression<T>(string expression)
    {
        var expr = new Expression<T>(expression, _scriptContext);
        var c = new Context();
        return expr.Execute(c);
    }
    
    private object RunExpressionGeneric(string expression)
    {
        var expr = new ExpressionGeneric(expression, _scriptContext);
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
        result.ShouldBe(expectedResult);
    }

    [DataTestMethod]
    [DataRow("object.intattribute", IntAttributeValue)]
    [DataRow("object.intattribute + 3", IntAttributeValue + 3)]
    public void TestIntFields(string expression, int expectedResult)
    {
        var result = RunExpression<int>(expression);
        result.ShouldBe(expectedResult);
    }

    [TestMethod]
    public void TestCallingNewStringListFunction()
    {
        var result = RunExpressionGeneric("NewStringList()");
        var resultList = result.ShouldBeAssignableTo<QuestList<string>>();
        resultList.Count.ShouldBe(0);
    }
    
    [TestMethod]
    public void TestCallingAllObjectsFunction()
    {
        var result = RunExpressionGeneric("AllObjects()");
        var resultList = result.ShouldBeAssignableTo<QuestList<Element>>();
        resultList.Count.ShouldBe(2);
    }

    [TestMethod]
    public void TestChangingTypes()
    {
        var expression = "a + b";
        var expr = new ExpressionGeneric(expression, _scriptContext);
        var c = new Context
        {
            Parameters = new Parameters
            {
                {"a", 1},
                {"b", 2}
            }
        };
        
        var result = expr.Execute(c);
        result.ShouldBe(3);

        c.Parameters["a"] = "A";
        c.Parameters["b"] = "B";
        
        result = expr.Execute(c);
        result.ShouldBe("AB");
    }
}