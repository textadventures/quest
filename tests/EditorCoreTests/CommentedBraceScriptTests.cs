using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// A whole-line comment holding a brace, such as a commented-out "if (...) {", inside a block. The
// Editor keeps whole-line comments when it parses a script, and its brace matching used to count
// that brace, so the block never closed: the script loaded as unparsed text, and every edit to it
// failed to parse and was dropped (#2695).
[TestClass]
public class CommentedBraceScriptTests
{
    private const string Game = """
        <asl version="580">
          <include ref="English.aslx" />
          <include ref="Core.aslx" />
          <game name="test" />
          <object name="room">
            <isroom />
            <object name="player">
              <inherit name="editor_player" />
            </object>
          </object>
          <function name="myfn"><![CDATA[
            if (game.pov = player) {
              interrupted = false
              //if (HasScript(game.pov, "script")) {
              if (HasScript(game.pov, "script") and not interrupted) {
                msg ("a")
              }
              msg ("b")
            }
          ]]></function>
        </asl>
        """;

    private static async Task<EditorController> Load(string xml)
    {
        var controller = new EditorController();
        Assert.IsTrue(await controller.Initialise(new GameData(System.Text.Encoding.UTF8.GetBytes(xml), "test.aslx")));
        return controller;
    }

    private static EditableScripts Script(EditorController controller) =>
        (EditableScripts) controller.GetEditorData("myfn")!.GetAttribute("script")!;

    [TestMethod]
    public async Task TheBlockParsesIntoScripts()
    {
        using var controller = await Load(Game);

        var outerIf = Script(controller).Scripts.Single();
        Assert.AreEqual("if", outerIf.EditorName);
    }

    [TestMethod]
    public async Task AnEditInCodeViewIsKeptAndSaved()
    {
        using var controller = await Load(Game);
        var script = Script(controller);

        script.Code = script.Code.Replace("msg (\"b\")", "msg (\"b\")\n  msg (\"new\")");

        StringAssert.Contains(script.Code, "msg (\"new\")");
        using var reloaded = await Load(controller.Save());
        StringAssert.Contains(Script(reloaded).Code, "msg (\"new\")");
    }

    [TestMethod]
    public async Task TheLinesAfterTheCommentAreNotIndentedAnExtraLevel()
    {
        using var controller = await Load(Game);

        var lines = Script(controller).Code.Split('\n');

        CollectionAssert.Contains(lines, "  // if (HasScript(game.pov, \"script\")) {");
        CollectionAssert.Contains(lines, "  if (HasScript(game.pov, \"script\") and not interrupted) {");
        CollectionAssert.Contains(lines, "  msg (\"b\")");
    }
}
