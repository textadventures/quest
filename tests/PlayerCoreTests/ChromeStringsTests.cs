using System.Text;
using QuestViva.Common;
using QuestViva.Engine;
using QuestViva.PlayerCore;

namespace QuestViva.PlayerCoreTests;

[TestClass]
public class ChromeStringsTests
{
    private static async Task<WorldModel> LoadGame(string languageLibrary)
    {
        var aslx = $"""
            <asl version="610">
              <include ref="{languageLibrary}"/>
              <include ref="Core.aslx"/>
              <game name="Chrome strings test"/>
              <object name="room">
                <inherit name="editor_room"/>
                <object name="player">
                  <inherit name="defaultplayer"/>
                </object>
              </object>
            </asl>
            """;
        var worldModel = new WorldModel(new GameData(Encoding.UTF8.GetBytes(aslx), "game.aslx"), null);
        Assert.IsTrue(await worldModel.InitialiseEdit());
        return worldModel;
    }

    [TestMethod]
    public void TestNoGameIsEnglish()
    {
        CollectionAssert.AreEquivalent(ChromeStrings.Defaults.ToList(), ChromeStrings.Resolve(null).ToList());
    }

    [TestMethod]
    public async Task TestEnglishTemplatesMatchTheDefaults()
    {
        // English.aslx has a template for every key, with the same text as the default.
        var worldModel = await LoadGame("English.aslx");
        foreach (var (key, value) in ChromeStrings.Defaults)
        {
            Assert.AreEqual(value, worldModel.GetTemplateText(key), key);
        }
    }

    [TestMethod]
    public async Task TestTranslated()
    {
        var resolved = ChromeStrings.Resolve(await LoadGame("Deutsch.aslx"));
        Assert.AreEqual("Spiel gespeichert.", resolved["GameSaved"]);
        Assert.AreEqual("Kompass", resolved["CompassLabel"]);
        Assert.AreEqual("Gib deinen Befehl ein", resolved["CommandInputLabel"]);
    }

    [TestMethod]
    public async Task TestLanguageWithoutChromeTemplatesFallsBackToEnglish()
    {
        // Francais.aslx translates Core's own templates, such as CompassLabel, but not the chrome's.
        var resolved = ChromeStrings.Resolve(await LoadGame("Francais.aslx"));
        Assert.AreEqual("Compas", resolved["CompassLabel"]);
        Assert.AreEqual("Game saved.", resolved["GameSaved"]);
    }
}
