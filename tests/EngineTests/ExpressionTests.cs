using QuestViva.Engine;
using QuestViva.Engine.Functions;
using QuestViva.Engine.Scripts;
using Shouldly;

namespace QuestViva.EngineTests;

public abstract class ExpressionTestsBase
{
    private const string ObjectName = "object";

    private const string ChildName = "child";

    // object names can contain spaces, so this must be handled in expressions
    private const string OtherObjectName = "other object";

    // note - attribute names cannot contain spaces
    private const string AttributeName = "attribute";
    private const string AttributeValue = "attributevalue";
    private const string ChildAttributeValue = "childattributevalue";
    private const string OtherObjectAttributeValue = "some other value";

    private const string IntAttributeName = "intattribute";
    private const int IntAttributeValue = 42;

    private const string DoubleAttributeName = "doubleattribute";
    private const double DoubleAttributeValue = 23.45;

    private const string BoolAttributeName = "boolattribute";
    private const bool BoolAttributeValue = true;

    private const string DictAttributeName = "dictattribute";
    private Element _child;
    private Element _object;
    private ScriptContext _scriptContext;
    private ScriptFactory _scriptFactory;

    private WorldModel _worldModel;
    protected abstract bool UseNCalc { get; }

    [TestInitialize]
    public void Setup()
    {
        _worldModel = Helpers.CreateWorldModel(UseNCalc);
        _scriptContext = new ScriptContext(_worldModel);
        _scriptFactory = new ScriptFactory(_worldModel);

        _object = _worldModel.GetElementFactory(ElementType.Object).Create("object");
        _object.Fields.Set(AttributeName, AttributeValue);
        _object.Fields.Set(IntAttributeName, IntAttributeValue);
        _object.Fields.Set(BoolAttributeName, BoolAttributeValue);
        _object.Fields.Set(DoubleAttributeName, DoubleAttributeValue);
        _object.Fields.Set(DictAttributeName, new QuestDictionary<string> { { "key1", "val1" }, { "key2", "val2" } });

        _child = _worldModel.GetElementFactory(ElementType.Object).Create("child");
        _child.Parent = _object;
        _child.Fields.Set(AttributeName, ChildAttributeValue);

        var otherObject = _worldModel.GetElementFactory(ElementType.Object).Create(OtherObjectName);
        otherObject.Fields.Set(AttributeName, OtherObjectAttributeValue);

        AddFunction("CustomIntFunction", "int", ["param1", "param2"], "return (param1 + param2)");
        AddFunction("CustomStringFunction", "string", ["param1", "param2"], "return (\"a\" + param1 + param2)");
        AddFunction("CustomDoubleFunction", "double", ["param1", "param2"], "return (1.1 + param1 + param2)");
        AddFunction("CustomBooleanFunction", "boolean", ["param1", "param2"], "return (param1 or param2)");
        AddFunction("CustomStringListFunction", "stringlist", ["param1", "param2"],
            """
            result = NewStringList()
            list add (result, param1)
            list add (result, param2)
            return (result)
            """);
    }

    private void AddFunction(string name, string returnType, IEnumerable<string> parameters, string script)
    {
        var function = _worldModel.GetElementFactory(ElementType.Function).Create(name);
        function.Fields[FieldDefinitions.ReturnType] = returnType;
        function.Fields[FieldDefinitions.ParamNames] = new QuestList<string>(parameters);
        function.Fields[FieldDefinitions.Script] = _scriptFactory.CreateScript(script, _scriptContext);
    }

    private T RunExpression<T>(string expression)
    {
        var expr = new Expression<T>(expression, _scriptContext);
        var c = new Context();
        return expr.Execute(c);
    }

    private object RunExpressionGeneric(string expression)
    {
        var expr = new ExpressionDynamic(expression, _scriptContext);
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
    [DataRow($"{OtherObjectName}.{AttributeName}", OtherObjectAttributeValue)]
    [DataRow("CustomStringFunction(\"b\", \"c\")", "abc")]
    [DataRow("Left(\"abcdef\", 3)", "abc")]
    [DataRow("UCase(\"abc\")", "ABC")]
    [DataRow("TypeOf(\"some string\")", "string")]
    [DataRow("TypeOf(123)", "int")]
    [DataRow("TypeOf(null)", "null")]
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
    [DataRow("ListCount(AllObjects())", 3)]
    [DataRow("CustomIntFunction(1, 2)", 3)]
    public void TestIntExpressions(string expression, int expectedResult)
    {
        var result = RunExpression<int>(expression);
        result.ShouldBe(expectedResult);
    }

    [DataTestMethod]
    [DataRow("1.0", 1.0)]
    [DataRow("1.1 + 2", 3.1)]
    [DataRow("1.1 + 2.2", 3.3)]
    [DataRow($"{ObjectName}.{DoubleAttributeName}", DoubleAttributeValue)]
    [DataRow("CustomDoubleFunction(2.2, 3.3)", 6.6)]
    [DataRow("PI", Math.PI)]
    [DataRow("pi", Math.PI)]
    [DataRow("Sqrt(4)", 2.0)]
    [DataRow("sqrt(4)", 2.0)]
    [DataRow("Abs(-5)", 5.0)]
    [DataRow("2 ^ 3", 8.0)]
    [DataRow("2 ^ -1", 0.5)]
    [DataRow("E ^ -1", 1.0 / Math.E)]
    public void TestDoubleExpressions(string expression, double expectedResult)
    {
        var result = RunExpression<double>(expression);
        Math.Abs(result - expectedResult).ShouldBeLessThan(0.000001);
    }

    [DataTestMethod]
    [DataRow("true", true)]
    [DataRow("false", false)]
    [DataRow("True", true)]
    [DataRow("False", false)]
    [DataRow("TRUE", true)]
    [DataRow("FALSE", false)]
    [DataRow("not true", !true)]
    [DataRow("not false", !false)]
    [DataRow("true and true", true)]
    [DataRow("true and false", false)]
    [DataRow("true or true", true)]
    [DataRow("false or true", true)]
    [DataRow("false or false", false)]
    [DataRow("true xor true", false)]
    [DataRow("true xor false", true)]
    [DataRow("false xor true", true)]
    [DataRow("false xor false", false)]
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
    [DataRow($"not ({ObjectName} = null)", true)]
    [DataRow($"not {ObjectName} = null", true)]
    [DataRow($"{ObjectName} = {ObjectName}", true)]
    [DataRow($"{ObjectName} = {ChildName}", false)]
    [DataRow($"{ObjectName} <> {ChildName}", true)]
    [DataRow($"{ObjectName}.parent = null", true)]
    [DataRow($"{ObjectName}.parent <> null", false)]
    [DataRow($"{ChildName}.parent = {ObjectName}", true)]
    [DataRow($"{ChildName}.parent = {ObjectName} and true", true)]
    [DataRow($"true and {ChildName}.parent = {ObjectName} and true", true)]
    [DataRow($"HasAttribute({ObjectName}, \"{AttributeName}\")", true)]
    [DataRow($"not HasAttribute({ObjectName}, \"{AttributeName}\")", false)]
    [DataRow($"HasAttribute({ObjectName}, \"invalid\")", false)]
    [DataRow($"not HasAttribute({ObjectName}, \"invalid\")", true)]
    [DataRow($"not HasAttribute({ObjectName}, \"invalid\") and true", true)]
    [DataRow($"true and not HasAttribute({ObjectName}, \"invalid\")", true)]
    [DataRow($"HasString({ObjectName}, \"{AttributeName}\")", true)]
    [DataRow($"HasBoolean({ObjectName}, \"{AttributeName}\")", false)]
    [DataRow($"HasBoolean({ObjectName}, \"{BoolAttributeName}\")", true)]
    [DataRow($"HasInt({ObjectName}, \"{IntAttributeName}\")", true)]
    [DataRow($"HasInt({ObjectName}, \"{AttributeName}\")", false)]
    [DataRow("ListContains(AllObjects(), object)", true)]
    [DataRow("listcontains(allobjects(), object)", true)]
    [DataRow("CustomBooleanFunction(true, false)", true)]
    [DataRow($"{OtherObjectName} = {OtherObjectName} and (true xor false)", true)]
    [DataRow($"{OtherObjectName} = {OtherObjectName} and (false xor false)", false)]
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

    [DataTestMethod]
    [DataRow("1 < 2", true)]
    [DataRow("2 < 1", false)]
    [DataRow("1 < 1", false)]
    [DataRow("2 > 1", true)]
    [DataRow("1 > 2", false)]
    [DataRow("1 > 1", false)]
    [DataRow("1 <= 1", true)]
    [DataRow("1 <= 2", true)]
    [DataRow("2 <= 1", false)]
    [DataRow("1 >= 1", true)]
    [DataRow("2 >= 1", true)]
    [DataRow("1 >= 2", false)]
    [DataRow($"{ObjectName}.{IntAttributeName} > 0", true)]
    [DataRow($"{ObjectName}.{IntAttributeName} < 100", true)]
    [DataRow($"{ObjectName}.{IntAttributeName} >= 42", true)]
    [DataRow($"{ObjectName}.{IntAttributeName} <= 42", true)]
    public void TestComparisonOperators(string expression, bool expectedResult)
    {
        var result = RunExpression<bool>(expression);
        result.ShouldBe(expectedResult);
    }

    [DataTestMethod]
    [DataRow("LCase(\"ABC\")", "abc")]
    [DataRow("LCase(\"Hello World\")", "hello world")]
    [DataRow("LengthOf(\"hello\")", 5)]
    [DataRow("LengthOf(\"\")", 0)]
    [DataRow("Mid(\"hello\", 2)", "ello")]
    [DataRow("Mid(\"hello\", 2, 3)", "ell")]
    [DataRow("Right(\"abcdef\", 3)", "def")]
    [DataRow("Instr(\"hello world\", \"world\")", 7)]
    [DataRow("Instr(\"hello world\", \"xyz\")", 0)]
    [DataRow("EndsWith(\"hello\", \"lo\")", true)]
    [DataRow("EndsWith(\"hello\", \"he\")", false)]
    [DataRow("StartsWith(\"hello\", \"he\")", true)]
    [DataRow("StartsWith(\"hello\", \"lo\")", false)]
    [DataRow("Replace(\"hello world\", \"world\", \"there\")", "hello there")]
    [DataRow("Trim(\"  hello  \")", "hello")]
    [DataRow("IsNumeric(\"42\")", true)]
    [DataRow("IsNumeric(\"abc\")", false)]
    public void TestStringFunctions(string expression, object expectedResult)
    {
        var result = RunExpressionGeneric(expression);
        result.ShouldBe(expectedResult);
    }

    [DataTestMethod]
    [DataRow($"GetBoolean({ObjectName}, \"{BoolAttributeName}\")", true)]
    [DataRow($"GetString({ObjectName}, \"{AttributeName}\")", AttributeValue)]
    [DataRow($"GetInt({ObjectName}, \"{IntAttributeName}\")", IntAttributeValue)]
    public void TestGetAttributeFunctions(string expression, object expectedResult)
    {
        var result = RunExpressionGeneric(expression);
        result.ShouldBe(expectedResult);
    }

    [TestMethod]
    public void TestGetDouble()
    {
        var result = RunExpressionGeneric($"GetDouble({ObjectName}, \"{DoubleAttributeName}\")");
        ((double)result).ShouldBe(DoubleAttributeValue, 0.000001);
    }

    [DataTestMethod]
    [DataRow($"DictionaryContains({ObjectName}.{DictAttributeName}, \"key1\")", true)]
    [DataRow($"DictionaryContains({ObjectName}.{DictAttributeName}, \"missing\")", false)]
    [DataRow($"DictionaryCount({ObjectName}.{DictAttributeName})", 2)]
    [DataRow($"StringDictionaryItem({ObjectName}.{DictAttributeName}, \"key1\")", "val1")]
    [DataRow($"StringDictionaryItem({ObjectName}.{DictAttributeName}, \"key2\")", "val2")]
    public void TestDictionaryFunctions(string expression, object expectedResult)
    {
        var result = RunExpressionGeneric(expression);
        result.ShouldBe(expectedResult);
    }

    [DataTestMethod]
    [DataRow("ToInt(\"42\")", 42)]
    [DataRow("ToDouble(\"3.14\")", 3.14)]
    [DataRow("ToString(42)", "42")]
    [DataRow("ToString(3.14)", "3.14")]
    public void TestTypeConversionFunctions(string expression, object expectedResult)
    {
        var result = RunExpressionGeneric(expression);
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
    public void TestCustomStringListFunction()
    {
        var result = RunExpressionGeneric("CustomStringListFunction(\"a\", \"b\")");
        var resultList = result.ShouldBeAssignableTo<QuestList<string>>();
        resultList.Count.ShouldBe(2);
        resultList[0].ShouldBe("a");
        resultList[1].ShouldBe("b");
    }

    [TestMethod]
    public void TestListIndexingSyntax()
    {
        if (!UseNCalc) return; // FLEE handles [] natively; this verifies the NCalc parser extension

        var list = new QuestList<string>(["alpha", "beta", "gamma"]);
        var expr = new Expression<string>("mylist[0]", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "mylist", list } } };
        expr.Execute(c).ShouldBe("alpha");

        expr = new Expression<string>("mylist[2]", _scriptContext);
        expr.Execute(c).ShouldBe("gamma");
    }

    [TestMethod]
    public void TestListIndexingWithVariableIndex()
    {
        if (!UseNCalc) return; // FLEE handles [] natively; this verifies the NCalc parser extension

        var list = new QuestList<string>(["alpha", "beta", "gamma"]);
        var expr = new Expression<string>("mylist[idx]", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "mylist", list }, { "idx", 1 } } };
        expr.Execute(c).ShouldBe("beta");
    }

    [TestMethod]
    public void TestDictionaryIndexingSyntax()
    {
        if (!UseNCalc) return; // FLEE handles [] natively; this verifies the NCalc parser extension

        var dict = new QuestDictionary<string> { { "foo", "bar" }, { "baz", "qux" } };
        var expr = new Expression<string>("mydict[\"foo\"]", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "mydict", dict } } };
        expr.Execute(c).ShouldBe("bar");
    }

    [TestMethod]
    public void TestDictionaryIndexingWithVariableKey()
    {
        if (!UseNCalc) return; // FLEE handles [] natively; this verifies the NCalc parser extension

        var dict = new QuestDictionary<string> { { "foo", "bar" }, { "baz", "qux" } };
        var expr = new Expression<string>("mydict[k]", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "mydict", dict }, { "k", "baz" } } };
        expr.Execute(c).ShouldBe("qux");
    }

    [TestMethod]
    public void TestMethodCallSyntax()
    {
        if (!UseNCalc) return; // FLEE handles instance method calls natively; this verifies the NCalc parser extension

        RunExpressionGeneric("\"hello world\".StartsWith(\"hello\")").ShouldBe(true);
        RunExpressionGeneric("\"hello world\".EndsWith(\"world\")").ShouldBe(true);
        RunExpressionGeneric("\"hello world\".Contains(\"lo wo\")").ShouldBe(true);
        RunExpressionGeneric("\"hello world\".ToUpper()").ShouldBe("HELLO WORLD");
    }

    [TestMethod]
    public void TestSplitFunction()
    {
        var result = RunExpressionGeneric("Split(\"a,b,c\", \",\")");
        var resultList = result.ShouldBeAssignableTo<QuestList<string>>();
        resultList.Count.ShouldBe(3);
        resultList[0].ShouldBe("a");
        resultList[1].ShouldBe("b");
        resultList[2].ShouldBe("c");
    }

    [TestMethod]
    public void TestJoinFunction()
    {
        var list = new QuestList<string>(["x", "y", "z"]);
        var expr = new Expression<string>("Join(mylist, \",\")", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "mylist", list } } };
        expr.Execute(c).ShouldBe("x,y,z");
    }

    [DataTestMethod]
    [DataRow("cast(3.7, int)", 3)]
    [DataRow("cast(3, double)", 3.0)]
    [DataRow("cast(3, single)", 3.0f)]
    [DataRow("1 + cast(2.9, int)", 3)]
    public void TestCastFunction(string expression, object expectedResult)
    {
        RunExpressionGeneric(expression).ShouldBe(expectedResult);
    }

    [DataTestMethod]
    [DataRow("if(true, \"yes\", \"no\")", "yes")]
    [DataRow("if(false, \"yes\", \"no\")", "no")]
    [DataRow("if(1 = 1, \"yes\", \"no\")", "yes")]
    [DataRow("if(1 = 2, \"yes\", \"no\")", "no")]
    public void TestIfFunction(string expression, string expectedResult)
    {
        RunExpression<string>(expression).ShouldBe(expectedResult);
    }

    [TestMethod]
    public void TestIsDefinedFunction()
    {
        var expr = new Expression<bool>("IsDefined(\"myvar\")", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "myvar", 42 } } };
        expr.Execute(c).ShouldBeTrue();

        var c2 = new Context { Parameters = new Parameters() };
        expr.Execute(c2).ShouldBeFalse();
    }

    [TestMethod]
    public void TestCallingAllObjectsFunction()
    {
        var result = RunExpressionGeneric("AllObjects()");
        var resultList = result.ShouldBeAssignableTo<QuestList<Element>>();
        resultList.Count.ShouldBe(3);
    }

    [TestMethod]
    public void TestChangingTypes()
    {
        var expression = "a + b";
        var expr = new ExpressionDynamic(expression, _scriptContext);
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

    [TestMethod]
    public void TestCurrentDateUTC()
    {
        const string expression = "CurrentDateUTC()";
        var result = RunExpression<int>(expression);
        var now = DateTimeOffset.UtcNow;
        var expected = (int) now.ToUnixTimeSeconds();
        Math.Abs(result - expected).ShouldBeLessThan(2);
    }
}

[TestClass]
public class NCalcExpressionTests : ExpressionTestsBase
{
    protected override bool UseNCalc => true;
}

[TestClass]
public class FleeExpressionTests : ExpressionTestsBase
{
    protected override bool UseNCalc => false;
}