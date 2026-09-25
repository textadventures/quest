using QuestViva.Common;
using QuestViva.Engine;
using QuestViva.Engine.GameLoader;

namespace QuestViva.EngineTests;

// Regression test for https://github.com/textadventures/quest/issues/2382 - an object in the
// main game whose parent is an element from an included library. Objects are saved nested
// inside their parent's XML element, but a library element isn't written by an Editor save,
// so the child (and everything nested under it) used to be dropped from the saved file.
[TestClass]
public class LibraryParentSaveTests
{
    private static async Task<WorldModel> LoadForEdit(string filename)
    {
        var gameData = await new FileDirectoryGameDataProvider(Path.GetFullPath(filename)).GetData();
        var worldModel = Helpers.CreateWorldModel(gameData);
        worldModel.LogError += ex => throw ex;
        Assert.IsTrue(await worldModel.InitialiseEdit(), string.Join("\n", worldModel.Errors));
        return worldModel;
    }

    [TestMethod]
    public async Task ObjectWithLibraryParentKeepsParentAfterEditorSave()
    {
        var worldModel = await LoadForEdit("libraryparenttest.aslx");
        Assert.AreEqual("libraryroom", worldModel.Elements.Get("lamp").Parent?.Name);

        var saveData = worldModel.Save(SaveMode.Editor);
        StringAssert.DoesNotMatch(saveData, new System.Text.RegularExpressions.Regex("name=\"libraryroom\""),
            "the library element itself should not be written to the game file");

        var tempFilename = Path.Combine(Path.GetDirectoryName(Path.GetFullPath("libraryparenttest.aslx"))!,
            $"libraryparentsaved-{Guid.NewGuid():N}.aslx");
        try
        {
            await File.WriteAllTextAsync(tempFilename, saveData);
            var reloaded = await LoadForEdit(tempFilename);

            Assert.IsTrue(reloaded.Elements.ContainsKey(ElementType.Object, "lamp"), "lamp was dropped on save");
            Assert.AreEqual("libraryroom", reloaded.Elements.Get("lamp").Parent?.Name);
            Assert.AreEqual("lamp", reloaded.Elements.Get("wick").Parent?.Name);
        }
        finally
        {
            File.Delete(tempFilename);
        }
    }

    // The reverse case: a library object whose parent is a room in the main game must not be
    // written into the game file just because its parent is.
    [TestMethod]
    public async Task LibraryObjectInsideGameRoomIsNotWrittenByEditorSave()
    {
        var worldModel = await LoadForEdit("libraryparenttest.aslx");
        Assert.AreEqual("start", worldModel.Elements.Get("libraryitem").Parent?.Name);

        var saveData = worldModel.Save(SaveMode.Editor);
        StringAssert.DoesNotMatch(saveData, new System.Text.RegularExpressions.Regex("name=\"libraryitem\""));

        var tempFilename = Path.Combine(Path.GetDirectoryName(Path.GetFullPath("libraryparenttest.aslx"))!,
            $"libraryparentsaved-{Guid.NewGuid():N}.aslx");
        try
        {
            await File.WriteAllTextAsync(tempFilename, saveData);
            var reloaded = await LoadForEdit(tempFilename);
            Assert.AreEqual("start", reloaded.Elements.Get("libraryitem").Parent?.Name);
        }
        finally
        {
            File.Delete(tempFilename);
        }
    }
}
