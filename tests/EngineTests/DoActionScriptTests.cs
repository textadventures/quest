using QuestViva.Engine;
using QuestViva.Engine.Scripts;
using Shouldly;

namespace QuestViva.EngineTests;

// Regression test for issue #2245: "do (obj, action)" with an action the object doesn't have
// used to fail with a bare NullReferenceException instead of a clear error naming the object
// and action.
[TestClass]
public class DoActionScriptTests
{
    [TestMethod]
    public async Task DoWithMissingActionThrowsClearError()
    {
        var worldModel = Helpers.CreateWorldModel();
        var scriptContext = new ScriptContext(worldModel);
        var scriptFactory = new ScriptFactory(worldModel);
        worldModel.GetElementFactory(ElementType.Object).Create("obj");

        var script = scriptFactory.CreateScript("do (obj, \"missingaction\")", scriptContext);

        var ex = await Should.ThrowAsync<Exception>(() => script.ExecuteAsync(new Context()));
        ex.Message.ShouldContain("obj");
        ex.Message.ShouldContain("missingaction");
    }
}
