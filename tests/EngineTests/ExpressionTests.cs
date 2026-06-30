using QuestViva.Engine;
using QuestViva.Engine.Functions;
using QuestViva.Engine.Scripts;
using Shouldly;

namespace QuestViva.EngineTests;

[TestClass]
public class ExpressionTests
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

    [TestInitialize]
    public void Setup()
    {
        _worldModel = Helpers.CreateWorldModel();
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

    private Task<T> RunExpression<T>(string expression)
    {
        var expr = new Expression<T>(expression, _scriptContext);
        var c = new Context();
        return expr.ExecuteAsync(c);
    }

    private Task<object> RunExpressionGeneric(string expression)
    {
        var expr = new ExpressionDynamic(expression, _scriptContext);
        var c = new Context();
        return expr.ExecuteAsync(c);
    }

    [DataTestMethod]
    [DataRow("\"hello world\"", "hello world")]
    [DataRow($"{ObjectName}.{AttributeName}", AttributeValue)]
    [DataRow($"{ChildName}.{AttributeName}", ChildAttributeValue)]
    [DataRow($"GetString({ObjectName}, \"{AttributeName}\")", AttributeValue)]
    [DataRow($"\"{AttributeValue}\"", AttributeValue)]
    [DataRow("\"hello\" + \" \" + \"world\"", "hello world")]
    [DataRow($"{ObjectName}.{AttributeName} + \" suffix\"", AttributeValue + " suffix")]
    [DataRow("\"prefix \" + object.attribute", "prefix " + AttributeValue)]
    [DataRow("CustomStringFunction(\"b\", \"c\")", "abc")]
    [DataRow("TypeOf(\"hello\")", "string")]
    [DataRow("TypeOf(123)", "int")]
    [DataRow("TypeOf(null)", "null")]
    public async Task TestStringExpressions(string expression, string expectedResult)
    {
        var result = await RunExpression<string>(expression);
        result.ShouldBe(expectedResult);
    }

    [TestMethod]
    public async Task TestPropertyAccess_AttributeNameWithSpace()
    {
        _object.Fields.Set("my attr", "spacedvalue");
        var result = await RunExpression<string>("object.my attr");
        result.ShouldBe("spacedvalue");
    }

    [DataTestMethod]
    [DataRow("1", 1)]
    [DataRow("1 + 2", 3)]
    [DataRow($"{ObjectName}.{IntAttributeName}", IntAttributeValue)]
    [DataRow($"{ObjectName}.{IntAttributeName} + 3", IntAttributeValue + 3)]
    [DataRow("ListCount(AllObjects())", 3)]
    [DataRow("CustomIntFunction(1, 2)", 3)]
    public async Task TestIntExpressions(string expression, int expectedResult)
    {
        var result = await RunExpression<int>(expression);
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
    public async Task TestDoubleExpressions(string expression, double expectedResult)
    {
        var result = await RunExpression<double>(expression);
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
    public async Task TestBooleanExpressions(string expression, bool expectedResult)
    {
        var result = await RunExpression<bool>(expression);
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
    [DataRow("GetObject(\"child\").parent", "object")]
    public async Task TestObjectExpressions(string expression, string expectedElementName)
    {
        var result = await RunExpressionGeneric(expression);
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
    public async Task TestComparisonOperators(string expression, bool expectedResult)
    {
        var result = await RunExpression<bool>(expression);
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
    public async Task TestStringFunctions(string expression, object expectedResult)
    {
        var result = await RunExpressionGeneric(expression);
        result.ShouldBe(expectedResult);
    }

    [DataTestMethod]
    [DataRow($"GetBoolean({ObjectName}, \"{BoolAttributeName}\")", true)]
    [DataRow($"GetString({ObjectName}, \"{AttributeName}\")", AttributeValue)]
    [DataRow($"GetInt({ObjectName}, \"{IntAttributeName}\")", IntAttributeValue)]
    public async Task TestGetAttributeFunctions(string expression, object expectedResult)
    {
        var result = await RunExpressionGeneric(expression);
        result.ShouldBe(expectedResult);
    }

    [TestMethod]
    public async Task TestGetDouble()
    {
        var result = await RunExpressionGeneric($"GetDouble({ObjectName}, \"{DoubleAttributeName}\")");
        ((double)result).ShouldBe(DoubleAttributeValue, 0.000001);
    }

    [DataTestMethod]
    [DataRow($"DictionaryContains({ObjectName}.{DictAttributeName}, \"key1\")", true)]
    [DataRow($"DictionaryContains({ObjectName}.{DictAttributeName}, \"missing\")", false)]
    [DataRow($"DictionaryCount({ObjectName}.{DictAttributeName})", 2)]
    [DataRow($"StringDictionaryItem({ObjectName}.{DictAttributeName}, \"key1\")", "val1")]
    [DataRow($"StringDictionaryItem({ObjectName}.{DictAttributeName}, \"key2\")", "val2")]
    public async Task TestDictionaryFunctions(string expression, object expectedResult)
    {
        var result = await RunExpressionGeneric(expression);
        result.ShouldBe(expectedResult);
    }

    [DataTestMethod]
    [DataRow("ToInt(\"42\")", 42)]
    [DataRow("ToDouble(\"3.14\")", 3.14)]
    [DataRow("ToString(42)", "42")]
    [DataRow("ToString(3.14)", "3.14")]
    public async Task TestTypeConversionFunctions(string expression, object expectedResult)
    {
        var result = await RunExpressionGeneric(expression);
        result.ShouldBe(expectedResult);
    }

    [TestMethod]
    public async Task TestCallingNewStringListFunction()
    {
        var result = await RunExpressionGeneric("NewStringList()");
        var resultList = result.ShouldBeAssignableTo<QuestList<string>>();
        resultList.Count.ShouldBe(0);
    }

    [TestMethod]
    public async Task TestCustomStringListFunction()
    {
        var result = await RunExpressionGeneric("CustomStringListFunction(\"a\", \"b\")");
        var resultList = result.ShouldBeAssignableTo<QuestList<string>>();
        resultList.Count.ShouldBe(2);
        resultList[0].ShouldBe("a");
        resultList[1].ShouldBe("b");
    }

    [TestMethod]
    public async Task TestListIndexingSyntax()
    {
        var list = new QuestList<string>(["alpha", "beta", "gamma"]);
        var expr = new Expression<string>("mylist[0]", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "mylist", list } } };
        (await expr.ExecuteAsync(c)).ShouldBe("alpha");

        expr = new Expression<string>("mylist[2]", _scriptContext);
        (await expr.ExecuteAsync(c)).ShouldBe("gamma");
    }

    [TestMethod]
    public async Task TestListIndexingWithVariableIndex()
    {
        var list = new QuestList<string>(["alpha", "beta", "gamma"]);
        var expr = new Expression<string>("mylist[idx]", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "mylist", list }, { "idx", 1 } } };
        (await expr.ExecuteAsync(c)).ShouldBe("beta");
    }

    [TestMethod]
    public async Task TestDictionaryIndexingSyntax()
    {
        var dict = new QuestDictionary<string> { { "foo", "bar" }, { "baz", "qux" } };
        var expr = new Expression<string>("mydict[\"foo\"]", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "mydict", dict } } };
        (await expr.ExecuteAsync(c)).ShouldBe("bar");
    }

    [TestMethod]
    public async Task TestDictionaryIndexingWithVariableKey()
    {
        var dict = new QuestDictionary<string> { { "foo", "bar" }, { "baz", "qux" } };
        var expr = new Expression<string>("mydict[k]", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "mydict", dict }, { "k", "baz" } } };
        (await expr.ExecuteAsync(c)).ShouldBe("qux");
    }

    [TestMethod]
    public async Task TestMethodCallSyntax()
    {
        (await RunExpressionGeneric("\"hello world\".StartsWith(\"hello\")")).ShouldBe(true);
        (await RunExpressionGeneric("\"hello world\".EndsWith(\"world\")")).ShouldBe(true);
        (await RunExpressionGeneric("\"hello world\".Contains(\"lo wo\")")).ShouldBe(true);
        (await RunExpressionGeneric("\"hello world\".ToUpper()")).ShouldBe("HELLO WORLD");
    }

    [TestMethod]
    public async Task TestMethodCallWithNullArg()
    {
        // When a method arg is null, NCalc's GetMethod(name, [typeof(object)]) won't find
        // List<Element>.Contains(Element), so the null-arg fallback must kick in.
        // AllObjects() as receiver avoids dot-notation parsing since the receiver is a function call.
        var expr = new Expression<bool>("AllObjects().Contains(nullobj)", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "nullobj", null } } };
        (await expr.ExecuteAsync(c)).ShouldBeFalse();
    }

    [TestMethod]
    public async Task TestSplitFunction()
    {
        var result = await RunExpressionGeneric("Split(\"a,b,c\", \",\")");
        var resultList = result.ShouldBeAssignableTo<QuestList<string>>();
        resultList.Count.ShouldBe(3);
        resultList[0].ShouldBe("a");
        resultList[1].ShouldBe("b");
        resultList[2].ShouldBe("c");
    }

    [TestMethod]
    public async Task TestJoinFunction()
    {
        var list = new QuestList<string>(["x", "y", "z"]);
        var expr = new Expression<string>("Join(mylist, \",\")", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "mylist", list } } };
        (await expr.ExecuteAsync(c)).ShouldBe("x,y,z");
    }

    [DataTestMethod]
    [DataRow("cast(3.7, int)", 3)]
    [DataRow("cast(3, double)", 3.0)]
    [DataRow("cast(3, single)", 3.0f)]
    [DataRow("1 + cast(2.9, int)", 3)]
    public async Task TestCastFunction(string expression, object expectedResult)
    {
        (await RunExpressionGeneric(expression)).ShouldBe(expectedResult);
    }

    [DataTestMethod]
    [DataRow("if(true, \"yes\", \"no\")", "yes")]
    [DataRow("if(false, \"yes\", \"no\")", "no")]
    [DataRow("if(1 = 1, \"yes\", \"no\")", "yes")]
    [DataRow("if(1 = 2, \"yes\", \"no\")", "no")]
    public async Task TestIfFunction(string expression, string expectedResult)
    {
        (await RunExpression<string>(expression)).ShouldBe(expectedResult);
    }

    [TestMethod]
    public async Task TestIsDefinedFunction()
    {
        var expr = new Expression<bool>("IsDefined(\"myvar\")", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "myvar", 42 } } };
        (await expr.ExecuteAsync(c)).ShouldBeTrue();

        var c2 = new Context { Parameters = new Parameters() };
        (await expr.ExecuteAsync(c2)).ShouldBeFalse();
    }

    [TestMethod]
    public async Task TestCallingAllObjectsFunction()
    {
        var result = await RunExpressionGeneric("AllObjects()");
        var resultList = result.ShouldBeAssignableTo<QuestList<Element>>();
        resultList.Count.ShouldBe(3);
    }

    [TestMethod]
    public async Task TestChangingTypes()
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

        var result = await expr.ExecuteAsync(c);
        result.ShouldBe(3);

        c.Parameters["a"] = "A";
        c.Parameters["b"] = "B";

        result = await expr.ExecuteAsync(c);
        result.ShouldBe("AB");
    }

    [TestMethod]
    public async Task TestCurrentDateUTC()
    {
        const string expression = "CurrentDateUTC()";
        var result = await RunExpression<int>(expression);
        var now = DateTimeOffset.UtcNow;
        var expected = (int) now.ToUnixTimeSeconds();
        Math.Abs(result - expected).ShouldBeLessThan(2);
    }

    [TestMethod]
    public async Task TestUnicodeIdentifiers()
    {
        var expr = new Expression<int>("sérgio + fósforo", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "sérgio", 10 }, { "fósforo", 5 } } };
        (await expr.ExecuteAsync(c)).ShouldBe(15);
    }

    [DataTestMethod]
    [DataRow("0xFF", 255)]
    [DataRow("0x10", 16)]
    [DataRow("0xABCDEF", 11259375)]
    [DataRow("0xFF + 1", 256)]
    public async Task TestFleeCompatHexLiterals(string expression, int expectedResult)
    {
        (await RunExpression<int>(expression)).ShouldBe(expectedResult);
    }

    [DataTestMethod]
    [DataRow("0x10u", 16)]
    [DataRow("42u", 42)]
    [DataRow("100L", 100)]
    public async Task TestFleeCompatIntegerSuffixes(string expression, int expectedResult)
    {
        (await RunExpression<int>(expression)).ShouldBe(expectedResult);
    }

    [DataTestMethod]
    [DataRow("1.5f", 1.5)]
    [DataRow("3.14d", 3.14)]
    public async Task TestFleeCompatRealSuffixes(string expression, double expectedResult)
    {
        var result = await RunExpression<double>(expression);
        Math.Abs(result - expectedResult).ShouldBeLessThan(0.000001);
    }

    [TestMethod]
    public async Task TestFleeCompatDecimalSuffix()
    {
        var result = await RunExpression<double>("2.0m");
        Math.Abs(result - 2.0).ShouldBeLessThan(0.000001);
    }

    [TestMethod]
    public async Task TestListMinusElement()
    {
        // QuestList<T> defines operator-, which FLEE resolves via IL compilation.
        // NCalc needs the EvaluateBinary hook to dispatch to the C# operator overload.
        var list = new QuestList<Element>([_object, _child]);
        var expr = new ExpressionDynamic("mylist - myobj", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "mylist", list }, { "myobj", _object } } };
        var result = (await expr.ExecuteAsync(c)).ShouldBeAssignableTo<QuestList<Element>>();
        result.Count.ShouldBe(1);
        result[0].ShouldBe(_child);
    }

    [TestMethod]
    public async Task TestListPlusElement()
    {
        var list = new QuestList<Element>([_object]);
        var expr = new ExpressionDynamic("mylist + myobj", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "mylist", list }, { "myobj", _child } } };
        var result = (await expr.ExecuteAsync(c)).ShouldBeAssignableTo<QuestList<Element>>();
        result.Count.ShouldBe(2);
    }

    [TestMethod]
    public async Task TestListPlusList()
    {
        var list1 = new QuestList<string>(["a", "b"]);
        var list2 = new QuestList<string>(["c", "d"]);
        var expr = new ExpressionDynamic("list1 + list2", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "list1", list1 }, { "list2", list2 } } };
        var result = (await expr.ExecuteAsync(c)).ShouldBeAssignableTo<QuestList<string>>();
        result.ShouldBe(["a", "b", "c", "d"]);
    }

    [TestMethod]
    public async Task TestElementPlusList()
    {
        var list = new QuestList<Element>([_child]);
        var expr = new ExpressionDynamic("myobj + mylist", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "mylist", list }, { "myobj", _object } } };
        var result = (await expr.ExecuteAsync(c)).ShouldBeAssignableTo<QuestList<Element>>();
        result.Count.ShouldBe(2);
        result[0].ShouldBe(_object);
        result[1].ShouldBe(_child);
    }

    [TestMethod]
    public async Task TestListTimesListUnionDedup()
    {
        var list1 = new QuestList<string>(["a", "b", "c"]);
        var list2 = new QuestList<string>(["b", "c", "d"]);
        var expr = new ExpressionDynamic("list1 * list2", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "list1", list1 }, { "list2", list2 } } };
        var result = (await expr.ExecuteAsync(c)).ShouldBeAssignableTo<QuestList<string>>();
        result.ShouldBe(["a", "b", "c", "d"]);
    }

    [DataTestMethod]
    [DataRow("5 / 2", 2)]
    [DataRow("6 / 2", 3)]
    [DataRow("7 / 2", 3)]
    [DataRow("100 / 10", 10)]
    [DataRow("-7 / 2", -3)]
    public async Task TestIntegerDivision(string expression, int expectedResult)
    {
        (await RunExpression<int>(expression)).ShouldBe(expectedResult);
    }

    [TestMethod]
    public async Task TestIntegerDivisionWithVariables()
    {
        // Exercises the NumberToWords pattern: int attribute / int literal
        var expr = new ExpressionDynamic("number / 100", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "number", 342 } } };
        var result = await expr.ExecuteAsync(c);
        result.ShouldBe(3);
    }

    [TestMethod]
    public async Task TestIntegerDivisionThenModulo()
    {
        // Mirrors the tens = (number / 10) % 10 pattern in NumberToWords
        var expr = new ExpressionDynamic("(number / 10) % 10", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "number", 42 } } };
        var result = await expr.ExecuteAsync(c);
        result.ShouldBe(4);
    }

    [TestMethod]
    public async Task TestObjectEqualToStringReturnsFalse()
    {
        // Cross-type equality (Element vs string) must return false, not throw IConvertible.
        var expr = new Expression<bool>("myobj = \"somestring\"", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "myobj", _object } } };
        (await expr.ExecuteAsync(c)).ShouldBe(false);
    }

    [TestMethod]
    public async Task TestStringEqualToObjectReturnsFalse()
    {
        // Cross-type equality (string vs Element) must return false, not throw IConvertible.
        var expr = new Expression<bool>("mystr = myobj", _scriptContext);
        var c = new Context { Parameters = new Parameters { { "mystr", "somestring" }, { "myobj", _object } } };
        (await expr.ExecuteAsync(c)).ShouldBe(false);
    }
}
