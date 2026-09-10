using Moq;
using QuestViva.Engine;
using QuestViva.Engine.Scripts;

namespace QuestViva.EngineTests;

[TestClass]
public class MultiScriptTests
{
    private WorldModel _worldModel;

    [TestInitialize]
    public void Setup()
    {
        _worldModel = Helpers.CreateWorldModel();
    }

    private MultiScript CreateMultiScript(params string[] lines)
    {
        // Creates a new MultiScript with mock IScripts, with .Line returning each
        // line passed in as a parameter.

        var source = new List<IScript>();
        foreach (var line in lines)
        {
            var script = new Mock<IScript>();
            script.Setup(s => s.Line).Returns(line);
            source.Add(script.Object);
        }

        var result = new MultiScript(_worldModel, source.ToArray());
        result.UndoLog = _worldModel.UndoLogger;

        return result;
    }

    private string GetLinesString(MultiScript multiScript)
    {
        return string.Join(";", multiScript.Scripts.Select(s => s.Line));
    }

    [TestMethod]
    public void TestMultiScriptCreation()
    {
        var multiScript = CreateMultiScript("line 1", "line 2", "line 3", "line 4");
        Assert.AreEqual(4, multiScript.Scripts.Count());
        Assert.AreEqual("line 2", multiScript.Scripts.ElementAt(1).Line);
        Assert.AreEqual("line 1;line 2;line 3;line 4", GetLinesString(multiScript));
    }

    [TestMethod]
    public async Task TestMultiScriptSwap()
    {
        var multiScript = CreateMultiScript("line 1", "line 2", "line 3", "line 4");

        // Swap lines 2 and 3

        _worldModel.UndoLogger.StartTransaction("Swap lines 2 and 3");
        multiScript.Swap(1, 2);
        _worldModel.UndoLogger.EndTransaction();

        // Check they are swapped correctly

        Assert.AreEqual("line 1;line 3;line 2;line 4", GetLinesString(multiScript));

        // Undo - should be back to original

        await _worldModel.UndoLogger.Undo();
        Assert.AreEqual("line 1;line 2;line 3;line 4", GetLinesString(multiScript));

        // Redo - lines 2 and 3 swapped again

        _worldModel.UndoLogger.Redo();
        Assert.AreEqual("line 1;line 3;line 2;line 4", GetLinesString(multiScript));

        // Undo - should be back to original

        await _worldModel.UndoLogger.Undo();
        Assert.AreEqual("line 1;line 2;line 3;line 4", GetLinesString(multiScript));

        // Now swap two non-consecutive elements, lines 1 and 4

        _worldModel.UndoLogger.StartTransaction("Swap lines 1 and 4");
        multiScript.Swap(0, 3);
        _worldModel.UndoLogger.EndTransaction();

        // Check they are swapped correctly

        Assert.AreEqual("line 4;line 2;line 3;line 1", GetLinesString(multiScript));

        // Undo - should be back to original

        await _worldModel.UndoLogger.Undo();
        Assert.AreEqual("line 1;line 2;line 3;line 4", GetLinesString(multiScript));
    }
}