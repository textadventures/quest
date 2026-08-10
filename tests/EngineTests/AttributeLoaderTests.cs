using System.Text;
using Moq;
using QuestViva.Common;
using QuestViva.Engine;

namespace QuestViva.EngineTests;

[TestClass]
public class AttributeLoaderTests
{
    // Regression test for a bug found in a third-party library (RunCommand.aslx) whose
    // simplestringdictionary attribute repeated the same key twice. QuestDictionary.Add /
    // Dictionary.Add throw ArgumentException ("Argument_AddingDuplicateWithKey") on a duplicate
    // key, and nothing between SimpleStringDictionaryLoader.Load and the caller
    // (WasmEditorBridge.Initialise) catches that specific exception type - so instead of a
    // friendly "Failed to load game due to the following errors" message, the user saw a raw,
    // untranslated exception (AOT trimming strips the resource string for
    // Argument_AddingDuplicateWithKey). The fix makes the loader record a normal load error
    // instead of throwing, matching how a missing '=' is already handled a few lines above it.
    [TestMethod]
    public async Task TestDuplicateKeyInSimpleStringDictionaryIsAGracefulLoadError()
    {
        var (worldModel, ok) = await LoadGameWithDictionaryAttribute(
            "stringdictionaryattr",
            "type=\"simplestringdictionary\"",
            "\"on\"=on;\"to\"=to;\"to\"=to");

        Assert.IsFalse(ok, "Game with a duplicate dictionary key should fail to load");
        // The key includes literal quote characters, matching Quest's quoted-key dictionary syntax
        // ("to"=to) - this is what the user's reported raw exception ("Argument_AddingDuplicateWithKey,
        // \"to\"") was actually showing before this fix.
        Assert.IsTrue(
            worldModel.Errors.Any(e => e.Contains("Duplicate key '\"to\"'") && e.Contains("stringdictionaryattr")),
            "Expected a friendly duplicate-key error; got: " + string.Join("; ", worldModel.Errors));
    }

    [TestMethod]
    public async Task TestDuplicateKeyInSimpleObjectDictionaryIsAGracefulLoadError()
    {
        var (worldModel, ok) = await LoadGameWithDictionaryAttribute(
            "objectdictionaryattr",
            "type=\"simpleobjectdictionary\"",
            "key1=room;key1=room");

        Assert.IsFalse(ok, "Game with a duplicate dictionary key should fail to load");
        Assert.IsTrue(
            worldModel.Errors.Any(e => e.Contains("Duplicate key 'key1'") && e.Contains("objectdictionaryattr")),
            "Expected a friendly duplicate-key error; got: " + string.Join("; ", worldModel.Errors));
    }

    private static async Task<(WorldModel worldModel, bool ok)> LoadGameWithDictionaryAttribute(
        string attributeName, string typeAttr, string attributeValue)
    {
        var xml = $"""
                   <asl version="580">
                     <include ref="English.aslx" />
                     <include ref="Core.aslx" />
                     <game name="test"/>
                     <object name="room">
                       <object name="player">
                         <inherit name="defaultplayer" />
                       </object>
                       <{attributeName} {typeAttr}>{attributeValue}</{attributeName}>
                     </object>
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
