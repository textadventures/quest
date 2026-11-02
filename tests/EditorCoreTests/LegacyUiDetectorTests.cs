using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// A game that relies on the legacy player UI doesn't work on v2, so the editor offers it the
// classic interface. Its evidence is the published games' real hacks (see
// docs/research/player-ui-surface-survey.md).
[TestClass]
public class LegacyUiDetectorTests
{
    private static IReadOnlyList<LegacyUiHack> FindInScript(string script)
    {
        return LegacyUiDetector.Find([("start", script)], []);
    }

    private static IReadOnlyList<LegacyUiHack> FindInFile(string text)
    {
        return LegacyUiDetector.Find([], [("game.js", text)]);
    }

    [TestMethod]
    [DataRow("""JS.setCss ("#gamePanes", "display:none")""")]
    [DataRow("""JS.setCss ("body", "background:black")""")]
    [DataRow("""JS.addText ("<b>Hello</b>")""")]
    [DataRow("""JS.addScript ("<script>alert(1)</script>")""")]
    [DataRow("""JS.setCommands ("Look;Wait")""")]
    [DataRow("""JS.scrollToEnd ()""")]
    [DataRow("""JS.eval ("$('#txtCommand').attr('placeholder', 'Your answer');")""")]
    [DataRow("""JS.eval ("$('." + section + "').css('opacity', '0.5');")""")]
    [DataRow("""JS.eval ("$('.elementListWrapper').accordion();")""")]
    [DataRow("""JS.eval ("document.getElementById('divOutput').style.minHeight = null")""")]
    [DataRow("""JS.eval ("scrollToEnd = function () {}")""")]
    [DataRow("""JS.eval ("$(window).on('keydown',function(e){ if(_waitMode){endWait();} });")""")]
    [DataRow("""JS.eval ("gridApi.zoomIn = function(){};")""")]
    [DataRow("""JS.eval ("getCurrentDiv().css('opacity', '1.0');")""")]
    public void ScriptsThatRelyOnLegacyAreFound(string script)
    {
        Assert.AreEqual(1, FindInScript(script).Count);
    }

    [TestMethod]
    [DataRow("""JS.QuestUI.setAutoScroll (false)""")]
    [DataRow("""JS.QuestUI.addPane ("clues", "Clues")""")]
    [DataRow("""JS.myOwnFunction ("setCss")""")]
    [DataRow("""JS.eval ("document.title = 'A game';")""")]
    [DataRow("""JS.eval ("document.getElementById('clock').textContent = '12:00';")""")]
    [DataRow("""JS.eval ("if (name.endsWith('x')) { console.log(name); }")""")]
    [DataRow("""JS.eval ("sendCommand('look');")""")]
    [DataRow("""msg ("Hide #gamePanes with $('#gamePanes').hide()")""")]
    public void ScriptsThatWorkOnV2AreNotFound(string script)
    {
        Assert.AreEqual(0, FindInScript(script).Count);
    }

    [TestMethod]
    [DataRow("scrollToEnd = function () {};")]
    [DataRow("window.updateList = function (name, items) {};")]
    [DataRow("function sendCommand(text) { console.log(text); }")]
    [DataRow("if (_waitMode) { console.log('waiting'); }")]
    [DataRow("$(\"#gamePanes\").hide();")]
    [DataRow("document.getElementById('txtCommand').focus();")]
    public void JavascriptFilesThatRelyOnLegacyAreFound(string text)
    {
        var hack = FindInFile(text).Single();

        Assert.AreEqual("game.js", hack.Source);
        Assert.AreEqual(text, hack.Detail);
    }

    [TestMethod]
    [DataRow("this.updateList = function () {};")]
    [DataRow("table.scrollToEnd = function () {};")]
    [DataRow("$(a).show();")]
    [DataRow("function helper() { return hexToRgb('#fff'); }")]
    public void JavascriptFilesThatDoNotTouchLegacyAreNotFound(string text)
    {
        Assert.AreEqual(0, FindInFile(text).Count);
    }

    [TestMethod]
    public void AGameCanDefineAFunctionWithALegacyName()
    {
        var hacks = LegacyUiDetector.Find([("start", """JS.colourBlend ("red", "blue")""")],
            [("colours.js", "function colourBlend(a, b) { return a; }")]);

        Assert.AreEqual(0, hacks.Count);
    }

    [TestMethod]
    public void ACallIsFoundOnceHoweverOftenItAppears()
    {
        var hacks = LegacyUiDetector.Find([("start", "JS.scrollToEnd ()"), ("start", "JS.scrollToEnd ()")], []);

        Assert.AreEqual(1, hacks.Count);
    }

    [TestMethod]
    public void ALongCallIsShortened()
    {
        var detail = FindInScript($"""JS.setCss ("#gamePanes", "{new string('x', 200)}")""").Single().Detail;

        Assert.AreEqual(80, detail.Length);
        StringAssert.EndsWith(detail, "…");
    }

    private const string Hack = """JS.setCss ("#gamePanes", "display:none")""";

    private static async Task<EditorController> Load(string elements, string game = "")
    {
        var xml = $"""
                   <asl version="580">
                     <include ref="English.aslx"/>
                     <include ref="Core.aslx"/>
                     <include ref="hacks.aslx"/>
                     <game name="Test">
                       <gameid>23390000-0000-0000-0000-000000000001</gameid>
                       {game}
                     </game>
                     {elements}
                   </asl>
                   """;
        var library = $"""
                       <library>
                         <function name="LibraryHack"><![CDATA[{Hack}]]></function>
                         <javascript src="libraryhack.js"/>
                       </library>
                       """;
        var controller = new EditorController();
        var files = new Dictionary<string, byte[]> { ["hacks.aslx"] = Encoding.UTF8.GetBytes(library) };
        Assert.IsTrue(await controller.Initialise(new GameData(Encoding.UTF8.GetBytes(xml), "test.aslx", files)));
        return controller;
    }

    [TestMethod]
    public async Task StockCoreAndLibrariesAreNotFound()
    {
        using var controller = await Load("");

        Assert.AreEqual(0, controller.FindLegacyUiHacks(new Dictionary<string, string>()).Count);
        Assert.AreEqual(0, controller.OwnJavascriptFiles.Count());
    }

    [TestMethod]
    public async Task AGamesOwnScriptsAreFoundWhereverTheyAre()
    {
        using var controller = await Load(
            $"""
             <function name="MyFunction"><![CDATA[{Hack}]]></function>
             <object name="room">
               <enter type="script"><![CDATA[{Hack}]]></enter>
               <look type="scriptdictionary">
                 <item key="first"><![CDATA[{Hack}]]></item>
               </look>
             </object>
             """);

        var sources = controller.FindLegacyUiHacks(new Dictionary<string, string>()).Select(h => h.Source).ToList();

        CollectionAssert.AreEquivalent(new[] { "MyFunction", "room.enter", "room.look" }, sources);
    }

    [TestMethod]
    public async Task AnOverriddenLibraryFunctionIsTheGamesOwn()
    {
        using var controller = await Load($"""<function name="LibraryHack"><![CDATA[{Hack}]]></function>""");

        Assert.AreEqual("LibraryHack", controller.FindLegacyUiHacks(new Dictionary<string, string>()).Single().Source);
    }

    [TestMethod]
    public async Task AGamesOwnJavascriptFilesAreRead()
    {
        using var controller = await Load("""<javascript src="mine.js"/>""");

        CollectionAssert.AreEqual(new[] { "mine.js" }, controller.OwnJavascriptFiles.ToList());
        var hack = controller.FindLegacyUiHacks(new Dictionary<string, string> { ["mine.js"] = "_waitMode = true;" })
            .Single();
        Assert.AreEqual("mine.js", hack.Source);
    }

    [TestMethod]
    public async Task AGameOnTheClassicInterfaceHasNothingToReport()
    {
        using var controller = await Load($"""<function name="MyFunction"><![CDATA[{Hack}]]></function>""");

        controller.GetEditorData("game")!.SetAttribute("classicinterface", true);

        Assert.AreEqual(0, controller.FindLegacyUiHacks(new Dictionary<string, string>()).Count);
    }
}
