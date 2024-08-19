using Shouldly;
using TextAdventures.Quest;
using TextAdventures.Quest.Functions;
using TextAdventures.Quest.Scripts;

namespace WorldModelTests;

[TestClass]
public class ExpressionTests
{
    private const string ObjectName = "object";
    private const string ChildName = "child";
    private const string AttributeName = "attribute";
    private const string AttributeValue = "attributevalue";
    private const string ChildAttributeValue = "childattributevalue";

    private const string IntAttributeName = "intattribute";
    private const int IntAttributeValue = 42;

    private const string BoolAttributeName = "boolattribute";
    private const bool BoolAttributeValue = true;

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
        _object.Fields.Set(BoolAttributeName, BoolAttributeValue);

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
    [DataRow("\"hello world\"", "hello world")]
    [DataRow("\"hello\" + \" \" + \"world\"", "hello world")]
    [DataRow("\"hello \" + 3", "hello 3")]
    [DataRow($"{ObjectName}.{AttributeName}", AttributeValue)]
    [DataRow($"{ChildName}.{AttributeName}", ChildAttributeValue)]
    [DataRow($"{ChildName}.parent.{AttributeName}", AttributeValue)]
    [DataRow($"{ObjectName}.{AttributeName} + \"testconcat\"", AttributeValue + "testconcat")]
    public void TestStringExpressions(string expression, string expectedResult)
    {
        var result = RunExpression<string>(expression);
        result.ShouldBe(expectedResult);
    }

    [DataTestMethod]
    [DataRow("1", 1)]
    [DataRow("1 + 2", 3)]
    [DataRow($"{ObjectName}.{IntAttributeName}", IntAttributeValue)]
    [DataRow($"{ObjectName}.{IntAttributeName} + 3", IntAttributeValue + 3)]
    [DataRow("ListCount(AllObjects())", 2)]
    public void TestIntExpressions(string expression, int expectedResult)
    {
        var result = RunExpression<int>(expression);
        result.ShouldBe(expectedResult);
    }

    [DataTestMethod]
    [DataRow("true", true)]
    [DataRow("false", false)]
    [DataRow("not true", !true)]
    [DataRow("not false", !false)]
    [DataRow($"{ObjectName}.{BoolAttributeName}", BoolAttributeValue)]
    [DataRow($"not {ObjectName}.{BoolAttributeName}", !BoolAttributeValue)]
    [DataRow("1 = 1", true)]
    [DataRow("1 = 2", false)]
    [DataRow("1 + 2 = 3", true)]
    [DataRow("1 + 2 = 4", false)]
    [DataRow("1 <> 2", true)]
    [DataRow("1 <> 1", false)]
    [DataRow($"{ObjectName} <> null", true)]
    [DataRow($"{ObjectName} = null", false)]
    [DataRow($"{ObjectName}.parent = null", true)]
    [DataRow($"{ObjectName}.parent <> null", false)]
    [DataRow($"HasAttribute({ObjectName}, \"{AttributeName}\")", true)]
    [DataRow($"HasAttribute({ObjectName}, \"invalid\")", false)]
    [DataRow($"HasString({ObjectName}, \"{AttributeName}\")", true)]
    [DataRow($"HasBoolean({ObjectName}, \"{AttributeName}\")", false)]
    [DataRow($"HasBoolean({ObjectName}, \"{BoolAttributeName}\")", true)]
    [DataRow($"HasInt({ObjectName}, \"{IntAttributeName}\")", true)]
    [DataRow($"HasInt({ObjectName}, \"{AttributeName}\")", false)]
    [DataRow("ListContains(AllObjects(), object)", true)]
    public void TestBooleanExpressions(string expression, bool expectedResult)
    {
        var result = RunExpression<bool>(expression);
        result.ShouldBe(expectedResult);
    }

    [DataTestMethod]
    [DataRow("game", "game")]
    [DataRow("object", "object")]
    [DataRow("object.parent", "null")]
    [DataRow("child", "child")]
    [DataRow("child.parent", "object")]
    [DataRow("child.parent.parent", "null")]
    [DataRow("GetObject(\"object\")", "object")]
    [DataRow("GetObject(\"invalid\")", "null")]
    [DataRow("ObjectListItem(AllObjects(), 0)", "object")]
    public void TestObjectExpressions(string expression, string expectedElementName)
    {
        var result = RunExpressionGeneric(expression);
        var expectedResult = expectedElementName == "null" ? null : _worldModel.Elements.Get(expectedElementName);
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