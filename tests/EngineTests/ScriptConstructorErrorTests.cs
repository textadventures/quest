using QuestViva.Engine;
using QuestViva.Engine.Scripts;
using Shouldly;

namespace QuestViva.EngineTests;

// Regression tests for issue #2249: keyword script constructors that take a parameter list used
// to fail with a NullReferenceException (rather than a clear error) when the line had no
// parameter list at all, e.g. a bare "for" with no "(...)".
[TestClass]
public class ScriptConstructorErrorTests
{
    private ScriptFactory _scriptFactory = null!;
    private ScriptContext _scriptContext = null!;
    private WorldModel _worldModel = null!;

    [TestInitialize]
    public void Setup()
    {
        _worldModel = Helpers.CreateWorldModel();
        _scriptFactory = new ScriptFactory(_worldModel);
        _scriptContext = new ScriptContext(_worldModel);
    }

    private T CreateConstructor<T>() where T : IScriptConstructor, new()
    {
        return new T { WorldModel = _worldModel, ScriptFactory = _scriptFactory };
    }

    [TestMethod]
    public void ForWithNoParameterListThrowsClearError()
    {
        var constructor = CreateConstructor<ForScriptConstructor>();
        Should.Throw<Exception>(() => constructor.Create("for", _scriptContext))
            .Message.ShouldContain("Expected a parameter list");
    }

    [TestMethod]
    public void ForEachWithNoParameterListThrowsClearError()
    {
        var constructor = CreateConstructor<ForEachScriptConstructor>();
        Should.Throw<Exception>(() => constructor.Create("foreach", _scriptContext))
            .Message.ShouldContain("Expected a parameter list");
    }

    [TestMethod]
    public void WhileWithNoParameterListThrowsClearError()
    {
        var constructor = CreateConstructor<WhileScriptConstructor>();
        Should.Throw<Exception>(() => constructor.Create("while", _scriptContext))
            .Message.ShouldContain("Expected a parameter list");
    }

    [TestMethod]
    public void IfWithNoParameterListThrowsClearError()
    {
        var constructor = CreateConstructor<IfScriptConstructor>();
        Should.Throw<Exception>(() => constructor.Create("if", _scriptContext))
            .Message.ShouldContain("Expected a parameter list");
    }

    [TestMethod]
    public void SwitchWithNoParameterListThrowsClearError()
    {
        var constructor = CreateConstructor<SwitchScriptConstructor>();
        Should.Throw<Exception>(() => constructor.Create("switch", _scriptContext))
            .Message.ShouldContain("Expected a parameter list");
    }

    [TestMethod]
    public void AskWithNoParameterListThrowsClearError()
    {
        var constructor = CreateConstructor<AskScriptConstructor>();
        Should.Throw<Exception>(() => constructor.Create("ask", _scriptContext))
            .Message.ShouldContain("Expected a parameter list");
    }

    [TestMethod]
    public void ShowMenuWithNoParameterListThrowsClearError()
    {
        var constructor = CreateConstructor<ShowMenuScriptConstructor>();
        Should.Throw<Exception>(() => constructor.Create("show menu", _scriptContext))
            .Message.ShouldContain("Expected a parameter list");
    }

    // Regression test for issue #2246: rundelegate is the only ScriptConstructorBase-derived
    // constructor with no ExpectedParameters, so a missing parameter list used to skip the
    // parameter-count check entirely and fail inside CreateInt with a NullReferenceException
    // instead of the intended "Expected at least 2 parameters" message.
    [TestMethod]
    public void RunDelegateWithNoParameterListThrowsClearError()
    {
        var constructor = new RunDelegateScriptConstructor { WorldModel = _worldModel, ScriptFactory = _scriptFactory };
        Should.Throw<Exception>(() => constructor.Create("rundelegate", _scriptContext))
            .Message.ShouldContain("Expected at least 2 parameters");
    }
}
