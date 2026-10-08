using System.Text;
using System.Text.RegularExpressions;
using Moq;
using QuestViva.Common;
using QuestViva.Engine;
using QuestViva.Engine.GameLoader;
using Shouldly;

namespace QuestViva.EngineTests;

// Regression coverage for issue #2345: a game whose elements were written on a single line
// could silently lose content at load time - an object with a boolean attribute right before its
// closing tag made every later object in the file disappear. The same game written over several
// lines loaded fine, so this loads it both ways and checks they match.
[TestClass]
public partial class SingleLineLoadTests
{
    [GeneratedRegex(@">\s+<")]
    private static partial Regex WhitespaceBetweenTags();

    private static async Task<WorldModel> LoadAsync(string xml)
    {
        var worldModel = Helpers.CreateWorldModel(await new ByteArrayGameDataProvider(Encoding.UTF8.GetBytes(xml), "singlelinetest.aslx").GetData());
        var success = await worldModel.Initialise(new Mock<IPlayer>().Object);
        success.ShouldBeTrue(string.Join("; ", worldModel.Errors));
        return worldModel;
    }

    private static string Minify(string xml) => WhitespaceBetweenTags().Replace(xml, "><");

    [TestMethod]
    public async Task GameWrittenOnOneLine_LoadsTheSameAsMultiLine()
    {
        var pretty = await File.ReadAllTextAsync("singlelinetest.aslx");
        var minified = Minify(pretty);

        var expected = await LoadAsync(pretty);
        var actual = await LoadAsync(minified);

        actual.Save(SaveMode.Editor, html: null).ShouldBe(expected.Save(SaveMode.Editor, html: null));
    }

    [TestMethod]
    public async Task GameWrittenOnOneLine_KeepsEveryElement()
    {
        var worldModel = await LoadAsync(Minify(await File.ReadAllTextAsync("singlelinetest.aslx")));

        foreach (var expression in new[]
                 {
                     "GetBoolean(key, \"take\")",
                     "lamp.scenery",
                     "lamp.weight = 1.5",
                     "ListCount(lamp.colours) = 2",
                     "ListCount(lamp.mixed) = 2",
                     "DictionaryItem(lamp.labels, \"b\") = \"two\"",
                     "DictionaryItem(lamp.names, \"x\") = \"ex\"",
                     "DictionaryContains(lamp.reactions, \"hello\")",
                     "HasScript(lamp, \"look\")",
                     "coin.parent = box",
                     "coin.alias = \"gold coin\"",
                     "northexit.locked",
                     "northexit.lockmessage = \"The door is locked.\"",
                     "northexit.to = hall",
                     "game.counter = 3",
                     "Twice(2) = 4",
                     "Template(\"CustomGreeting\") = \"Hello there.\"",
                     "DynamicTemplate(\"CustomDynamic\", lamp) = \"Dynamic lamp\"",
                 })
        {
            (await worldModel.AssertAsync(expression)).ShouldBeTrue(expression);
        }

        worldModel.Elements.ContainsKey(ElementType.Object, "hall").ShouldBeTrue();
        worldModel.Elements.ContainsKey(ElementType.Object, "gc").ShouldBeTrue();
        worldModel.Elements.ContainsKey(ElementType.ObjectType, "shiny").ShouldBeTrue();
    }
}
