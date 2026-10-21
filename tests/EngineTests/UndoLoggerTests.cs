using QuestViva.Engine;

namespace QuestViva.EngineTests;

// Drives the UndoLogger the way Core does: each ordinary command calls "start transaction" (RollTransaction),
// and the undo command calls "undo" (RollbackTransaction) without starting a transaction of its own.
[TestClass]
public class UndoLoggerTests
{
    private WorldModel _worldModel = null!;
    private Element _object = null!;

    [TestInitialize]
    public void Setup()
    {
        _worldModel = Helpers.CreateWorldModel();
        _object = _worldModel.GetElementFactory(ElementType.Object).Create("object");
        _object.Fields.Set("count", 0);
    }

    private void Command(string name, int count)
    {
        _worldModel.UndoLogger.RollTransaction(name);
        _object.Fields.Set("count", count);
    }

    private Task UndoCommand()
    {
        return _worldModel.UndoLogger.RollbackTransaction();
    }

    [TestMethod]
    public async Task CommandAfterUndoDoesNotLogUndoneTurnAgain()
    {
        Command("first", 1);
        Command("second", 2);

        await UndoCommand();
        Assert.AreEqual(1, _object.Fields.Get("count"));

        Command("third", 3);
        CollectionAssert.AreEqual(new[] { "first" }, _worldModel.UndoLogger.UndoList().ToArray());

        await UndoCommand();
        Assert.AreEqual(1, _object.Fields.Get("count"));

        await UndoCommand();
        Assert.AreEqual(0, _object.Fields.Get("count"));

        // Everything has been undone, so there must be nothing left
        await Assert.ThrowsExceptionAsync<Exception>(UndoCommand);
    }
}
