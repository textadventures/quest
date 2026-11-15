using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// A multi-line script's closing tag has to line up with its opening tag, which the XmlWriter
// doesn't do by itself, and lining it up mustn't change the script from one save to the next.
[TestClass]
public class MultilineScriptSaveTests
{
    private const string Game = """
        <asl version="580">
          <include ref="English.aslx" />
          <include ref="Core.aslx" />
          <game name="Test">
            <start type="script">
              msg ("one")
              msg ("two")
            </start>
            <inituserinterface type="script"><![CDATA[
              if (1 < 2) {
                msg ("three")
              }
            ]]></inituserinterface>
          </game>
          <object name="room" />
          <function name="Shout" parameters="text">
            msg (text)
            msg (text)
          </function>
          <turnscript name="ts">
            <script>
              msg ("four")
              msg ("five")
            </script>
          </turnscript>
        </asl>
        """;

    private static async Task<string> Save(string xml)
    {
        using var controller = new EditorController();
        Assert.IsTrue(await controller.Initialise(new ByteArrayGameDataProvider(System.Text.Encoding.UTF8.GetBytes(xml), "test.aslx")));
        return controller.Save();
    }

    [TestMethod]
    [DataRow("    </start>")]
    [DataRow("    ]]></inituserinterface>")]
    [DataRow("  </function>")]
    [DataRow("    </script>")]
    public async Task AMultilineScriptsClosingTagIsIndented(string closingLine)
    {
        var lines = (await Save(Game)).Split('\n').Select(l => l.TrimEnd('\r'));

        CollectionAssert.Contains(lines.ToList(), closingLine);
    }

    [TestMethod]
    public async Task AMultilineScriptSurvivesSavingTwice()
    {
        var once = await Save(Game);
        var twice = await Save(once);

        Assert.AreEqual(once, twice);
    }
}
