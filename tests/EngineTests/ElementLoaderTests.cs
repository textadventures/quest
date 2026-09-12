using System.Text;
using Moq;
using QuestViva.Common;
using QuestViva.Engine;

namespace QuestViva.EngineTests;

[TestClass]
public class ElementLoaderTests
{
    // Regression test for issue #2241: an <inherit> with no name attribute used to fail at
    // resolve time with a confusing null-key ArgumentNullException rather than a load error
    // pointing at the malformed element.
    [TestMethod]
    public async Task TestInheritWithNoNameIsAGracefulLoadError()
    {
        var (worldModel, ok) = await LoadGame("""
            <object name="room">
              <inherit />
            </object>
            """);

        Assert.IsFalse(ok, "Game with a nameless <inherit> should fail to load");
        Assert.IsTrue(
            worldModel.Errors.Any(e => e.Contains("Expected 'name' attribute in inherit")),
            "Expected a friendly missing-name error; got: " + string.Join("; ", worldModel.Errors));
    }

    // Regression test for issue #2251: an <include> with no ref attribute at all (as opposed to
    // ref="") used to fail with a NullReferenceException.
    [TestMethod]
    public async Task TestIncludeWithNoRefDoesNotCrash()
    {
        var (_, ok) = await LoadGame("""
            <include />
            """);

        Assert.IsTrue(ok, "A malformed <include> with no ref should be skipped, not crash the load");
    }

    // Regression test for issue #2251: a <javascript> with no src attribute at all used to fail
    // with a NullReferenceException.
    [TestMethod]
    public async Task TestJavascriptWithNoSrcDoesNotCrash()
    {
        var (_, ok) = await LoadGame("""
            <javascript />
            """);

        Assert.IsTrue(ok, "A malformed <javascript> with no src should be skipped, not crash the load");
    }

    private static async Task<(WorldModel worldModel, bool ok)> LoadGame(string extraXml)
    {
        var xml = $"""
                   <asl version="580">
                     <include ref="English.aslx" />
                     <include ref="Core.aslx" />
                     <game name="test"/>
                     <object name="room2">
                       <object name="player">
                         <inherit name="defaultplayer" />
                       </object>
                     </object>
                     {extraXml}
                   </asl>
                   """;

        var provider = new ByteArrayGameDataProvider(Encoding.UTF8.GetBytes(xml), "test.aslx");
        var gameData = await provider.GetData();
        var worldModel = Helpers.CreateWorldModel(gameData);

        var player = new Mock<IPlayer>();
        var ok = await worldModel.Initialise(player.Object);
        return (worldModel, ok);
    }
}
