using Moq;
using QuestViva.Common;
using QuestViva.Engine.GameLoader;

namespace QuestViva.EngineTests;

// Regression test for https://github.com/textadventures/quest/issues/2309 - list and
// dictionary attributes whose names contain a space used to throw on save (Invalid name
// character) because the list/dictionary savers wrote the attribute name directly as an XML
// element name instead of falling back to <attr name="...">, unlike every other attribute type.
[TestClass]
public class SpaceAttributeSaveTests
{
    [TestMethod]
    public async Task ListAndDictionaryAttributesWithSpacesInNamesSurviveSaveAndReload()
    {
        var gameDataProvider = new FileGameDataProvider("spaceattributesavetest.aslx");
        var gameData = await gameDataProvider.GetData();
        var worldModel = Helpers.CreateWorldModel(gameData);

        worldModel.LogError += ex => throw ex;

        var player = new Mock<IPlayer>();
        var success = await worldModel.Initialise(player.Object);
        Assert.IsTrue(success, "Initialisation failed");

        await worldModel.Begin();
        await worldModel.SendCommand("addruntimeattributes");

        var tempFilename = Path.GetTempFileName();
        try
        {
            var saveData = worldModel.Save(SaveMode.SavedGame, html: null);
            await File.WriteAllTextAsync(tempFilename, saveData);

            var reloadedGameDataProvider = new FileGameDataProvider(tempFilename);
            var reloadedGameData = await reloadedGameDataProvider.GetData();
            var reloadedWorldModel = Helpers.CreateWorldModel(reloadedGameData);
            reloadedWorldModel.LogError += ex => throw ex;

            success = await reloadedWorldModel.Initialise(player.Object);
            Assert.IsTrue(success, "Initialisation failed after reload");

            await reloadedWorldModel.Begin();

            Assert.IsTrue(await reloadedWorldModel.AssertAsync(
                "ListCount(GetAttribute(player, \"my list\")) = 2"));
            Assert.IsTrue(await reloadedWorldModel.AssertAsync(
                "StringListItem(GetAttribute(player, \"my list\"), 0) = \"a\""));
            Assert.IsTrue(await reloadedWorldModel.AssertAsync(
                "StringListItem(GetAttribute(player, \"my list\"), 1) = \"b\""));

            Assert.IsTrue(await reloadedWorldModel.AssertAsync(
                "DictionaryCount(GetAttribute(player, \"my string dictionary\")) = 2"));
            Assert.IsTrue(await reloadedWorldModel.AssertAsync(
                "StringDictionaryItem(GetAttribute(player, \"my string dictionary\"), \"key1\") = \"value1\""));

            Assert.IsTrue(await reloadedWorldModel.AssertAsync(
                "ListCount(GetAttribute(player, \"my runtime list\")) = 3"));
            Assert.IsTrue(await reloadedWorldModel.AssertAsync(
                "StringListItem(GetAttribute(player, \"my runtime list\"), 2) = \"z\""));

            Assert.IsTrue(await reloadedWorldModel.AssertAsync(
                "DictionaryCount(GetAttribute(player, \"my runtime dictionary\")) = 1"));
            Assert.IsTrue(await reloadedWorldModel.AssertAsync(
                "StringDictionaryItem(GetAttribute(player, \"my runtime dictionary\"), \"rkey1\") = \"rvalue1\""));
        }
        finally
        {
            File.Delete(tempFilename);
        }
    }
}
