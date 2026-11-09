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

    // Regression test for issue #2665: Quest 5.9 writes an attribute literally called "attr" as a
    // bare <attr type="..."> with no name, which used to fail the load with "Invalid attribute".
    [TestMethod]
    public async Task TestNamelessAttrLoadsAsAttributeNamedAttr()
    {
        var (worldModel, ok) = await LoadGame("""
            <object name="thing">
              <attr type="string">hello</attr>
            </object>
            """);

        Assert.IsTrue(ok, "Errors: " + string.Join("; ", worldModel.Errors));
        Assert.AreEqual("hello", worldModel.Objects.First(o => o.Name == "thing").Fields.GetString("attr"));
    }

    // Regression tests for issue #2352: an unnamed element gets a name like exit1 when it loads. An
    // element the author named exit1 later in the file used to replace it, silently losing it.
    [TestMethod]
    public async Task TestUnnamedExitIsKeptWhenALaterObjectHasItsAutomaticName()
    {
        var (worldModel, ok) = await LoadGame("""
            <object name="hall">
              <exit alias="north" to="room2" />
            </object>
            <object name="exit1" />
            """);

        Assert.IsTrue(ok, "Errors: " + string.Join("; ", worldModel.Errors));
        Assert.IsNull(worldModel.Object("exit1").Parent, "exit1 should be the author's object, not the exit");
        var exit = worldModel.Objects.SingleOrDefault(o =>
            o.Type == ObjectType.Exit && o.Parent?.Name == "hall" && o.Fields[FieldDefinitions.Alias] == "north");
        Assert.IsNotNull(exit, "The unnamed exit should still be there");
        Assert.IsTrue(exit.Fields[FieldDefinitions.Anonymous]);
    }

    [TestMethod]
    public async Task TestUnnamedTurnScriptIsKeptWhenALaterFunctionHasItsAutomaticName()
    {
        var (worldModel, ok) = await LoadGame("""
            <turnscript>
              <script>msg ("tick")</script>
            </turnscript>
            <function name="k1" />
            """);

        Assert.IsTrue(ok, "Errors: " + string.Join("; ", worldModel.Errors));
        Assert.IsTrue(worldModel.Elements.ContainsKey(ElementType.Function, "k1"));
        Assert.AreEqual(1, worldModel.Objects.Count(o => o.Type == ObjectType.TurnScript && o.Name != "k1"));
    }

    // A game can still override a library's unnamed command by naming its own after the pattern.
    [TestMethod]
    public async Task TestGameCanStillOverrideALibrarysUnnamedCommand()
    {
        var library = """
            <library>
              <command pattern="frobnicate">
                <script>msg ("library")</script>
              </command>
            </library>
            """;
        var (worldModel, ok) = await LoadGame("""
            <command name="frobnicate" pattern="frobnicate">
              <script>msg ("game")</script>
            </command>
            """, includeXml: """<include ref="lib.aslx" />""",
            adjacentFiles: new Dictionary<string, byte[]> { ["lib.aslx"] = Encoding.UTF8.GetBytes(library) });

        Assert.IsTrue(ok, "Errors: " + string.Join("; ", worldModel.Errors));
        Assert.AreEqual(1, worldModel.Objects.Count(o => o.Fields.GetString("pattern") == "frobnicate"));
        Assert.IsFalse(worldModel.Object("frobnicate").MetaFields[MetaFieldDefinitions.Library]);
    }

    private static async Task<(WorldModel worldModel, bool ok)> LoadGame(string extraXml,
        string includeXml = "", IReadOnlyDictionary<string, byte[]>? adjacentFiles = null)
    {
        var xml = $"""
                   <asl version="580">
                     <include ref="English.aslx" />
                     <include ref="Core.aslx" />
                     {includeXml}
                     <game name="test"/>
                     <object name="room2">
                       <object name="player">
                         <inherit name="defaultplayer" />
                       </object>
                     </object>
                     {extraXml}
                   </asl>
                   """;

        var gameData = new GameData(Encoding.UTF8.GetBytes(xml), "test.aslx", adjacentFiles);
        var worldModel = Helpers.CreateWorldModel(gameData);

        var player = new Mock<IPlayer>();
        var ok = await worldModel.Initialise(player.Object);
        return (worldModel, ok);
    }
}
