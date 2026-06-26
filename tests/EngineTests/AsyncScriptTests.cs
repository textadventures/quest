using QuestViva.Engine;
using QuestViva.Engine.Scripts;
using Shouldly;

namespace QuestViva.EngineTests;

[TestClass]
public class AsyncScriptTests
{
    private WorldModel _worldModel;
    private ScriptContext _scriptContext;
    private ScriptFactory _scriptFactory;
    private Element _obj;

    [TestInitialize]
    public void Setup()
    {
        _worldModel = Helpers.CreateWorldModel();
        _scriptContext = new ScriptContext(_worldModel);
        _scriptFactory = new ScriptFactory(_worldModel);
        _obj = _worldModel.GetElementFactory(ElementType.Object).Create("obj");
    }

    private void AddFunction(string name, string script, params string[] paramNames)
    {
        var function = _worldModel.GetElementFactory(ElementType.Function).Create(name);
        function.Fields[FieldDefinitions.ParamNames] = new QuestList<string>(paramNames);
        function.Fields[FieldDefinitions.Script] = _scriptFactory.CreateScript(script, _scriptContext);
    }

    private async Task<object> CallAsync(string name, Parameters parameters = null)
    {
        return await _worldModel.RunProcedureAsync(name, parameters, true);
    }

    [TestMethod]
    public async Task TestSimpleReturnAsync()
    {
        AddFunction("SimpleReturn", "return (42)");
        (await CallAsync("SimpleReturn")).ShouldBe(42);
    }

    [TestMethod]
    public async Task TestMultiScriptAsync()
    {
        AddFunction("MultiStep",
            """
            x = 1
            x = x + 1
            x = x + 1
            return (x)
            """);
        (await CallAsync("MultiStep")).ShouldBe(3);
    }

    [TestMethod]
    public async Task TestIfTrueBranchAsync()
    {
        AddFunction("IfTrue",
            """
            if (true) {
                return ("yes")
            } else {
                return ("no")
            }
            """);
        (await CallAsync("IfTrue")).ShouldBe("yes");
    }

    [TestMethod]
    public async Task TestIfFalseBranchAsync()
    {
        AddFunction("IfFalse",
            """
            if (false) {
                return ("yes")
            } else {
                return ("no")
            }
            """);
        (await CallAsync("IfFalse")).ShouldBe("no");
    }

    [TestMethod]
    public async Task TestForScriptAsync()
    {
        AddFunction("ForLoop",
            """
            total = 0
            for (i, 1, 5) {
                total = total + i
            }
            return (total)
            """);
        (await CallAsync("ForLoop")).ShouldBe(15);
    }

    [TestMethod]
    public async Task TestWhileScriptAsync()
    {
        AddFunction("WhileLoop",
            """
            x = 0
            while (x < 5) {
                x = x + 1
            }
            return (x)
            """);
        (await CallAsync("WhileLoop")).ShouldBe(5);
    }

    [TestMethod]
    public async Task TestForEachScriptAsync()
    {
        AddFunction("ForEachCount",
            """
            items = NewStringList()
            list add (items, "a")
            list add (items, "b")
            list add (items, "c")
            count = 0
            foreach (item, items) {
                count = count + 1
            }
            return (count)
            """);
        (await CallAsync("ForEachCount")).ShouldBe(3);
    }

    [TestMethod]
    public async Task TestSwitchMatchesCaseAsync()
    {
        AddFunction("SwitchMatch",
            """
            switch ("b") {
                case ("a") {
                    return ("got a")
                }
                case ("b") {
                    return ("got b")
                }
                default {
                    return ("got default")
                }
            }
            """);
        (await CallAsync("SwitchMatch")).ShouldBe("got b");
    }

    [TestMethod]
    public async Task TestSwitchFallsToDefaultAsync()
    {
        AddFunction("SwitchDefault",
            """
            switch ("z") {
                case ("a") {
                    return ("got a")
                }
                default {
                    return ("got default")
                }
            }
            """);
        (await CallAsync("SwitchDefault")).ShouldBe("got default");
    }

    [TestMethod]
    public async Task TestFirstTimeScriptAsync()
    {
        AddFunction("FirstTimeFunc",
            """
            firsttime {
                return ("first")
            } otherwise {
                return ("otherwise")
            }
            """);
        (await CallAsync("FirstTimeFunc")).ShouldBe("first");
        (await CallAsync("FirstTimeFunc")).ShouldBe("otherwise");
    }

    [TestMethod]
    public async Task TestNestedFunctionCallAsync()
    {
        AddFunction("Double", "return (n * 2)", "n");
        AddFunction("CallDouble",
            """
            result = Double(5)
            return (result)
            """);
        (await CallAsync("CallDouble")).ShouldBe(10);
    }

    [TestMethod]
    public async Task TestFunctionCallWithParametersAsync()
    {
        AddFunction("Add", "return (a + b)", "a", "b");
        var result = await _worldModel.RunProcedureAsync("Add",
            new Parameters { { "a", 3 }, { "b", 7 } }, true);
        result.ShouldBe(10);
    }

    [TestMethod]
    public async Task TestDoScriptAsync()
    {
        _obj.Fields.Set("flag", "unset");
        _obj.Fields.Set("action",
            _scriptFactory.CreateScript("obj.flag = \"done\"", _scriptContext));

        AddFunction("RunAction", "do (obj, \"action\")");
        await _worldModel.RunProcedureAsync("RunAction", null, false);
        _obj.Fields.GetString("flag").ShouldBe("done");
    }
}
