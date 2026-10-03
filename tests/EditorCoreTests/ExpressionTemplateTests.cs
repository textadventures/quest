using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// Regression coverage for issue #2343: expression templates turned each #param# into a greedy
// "(?<param>.*)", so a template like ShowMenu(#caption#,#options#,#allowcancel#) split its
// parameters on any comma, including ones nested inside an inner call such as Split("a;b", ";").
// Template parameters now only match bracket-balanced text, with string literals kept intact.
[TestClass]
public class ExpressionTemplateTests
{
    private static EditorController _controller = null!;

    [ClassInitialize]
    public static async Task ClassInit(TestContext _)
    {
        var templates = EditorController.GetAvailableTemplates();
        var template = templates.Values.Single(t => t.TemplateName == "English");
        var initialFileText = EditorController.CreateNewGameFile(template.ResourceName, "Test");
        var bytes = Encoding.UTF8.GetBytes(initialFileText);

        _controller = new EditorController();
        _controller.ClearTree += (_, _) => { };
        _controller.BeginTreeUpdate += (_, _) => { };
        _controller.EndTreeUpdate += (_, _) => { };
        _controller.AddedNode += (_, _) => { };

        var ok = await _controller.Initialise(new ByteArrayGameDataProvider(bytes, "test.aslx"));
        Assert.IsTrue(ok, "Initialisation failed");
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _controller.Dispose();
    }

    private static IEditorData GetParameters(string expression, string expressionType, string expectedPattern)
    {
        var definition = _controller.GetExpressionEditorDefinition(expression, expressionType);
        Assert.IsNotNull(definition, $"No template matched '{expression}'");
        Assert.AreEqual(expectedPattern, definition.OriginalPattern);
        return _controller.GetExpressionEditorData(expression, expressionType, null!);
    }

    [TestMethod]
    public void ShowMenuWithNestedCommasSplitsOnTopLevelCommasOnly()
    {
        var data = GetParameters(
            """ShowMenu("Your character class?", Split("Warrior;Wizard;Priest;Thief", ";"), false)""",
            "set", "ShowMenu(#caption#,#options#,#allowcancel#)");

        Assert.AreEqual("\"Your character class?\"", data.GetAttribute("caption"));
        Assert.AreEqual(""" Split("Warrior;Wizard;Priest;Thief", ";")""", data.GetAttribute("options"));
        Assert.AreEqual(" false", data.GetAttribute("allowcancel"));
    }

    [TestMethod]
    public void CommasAndBracketsInsideStringsAreIgnored()
    {
        var data = GetParameters(
            """ShowMenu("Pick one, (or not)", Split("a,b", ","), true)""",
            "set", "ShowMenu(#caption#,#options#,#allowcancel#)");

        Assert.AreEqual("\"Pick one, (or not)\"", data.GetAttribute("caption"));
        Assert.AreEqual(""" Split("a,b", ",")""", data.GetAttribute("options"));
        Assert.AreEqual(" true", data.GetAttribute("allowcancel"));
    }

    [TestMethod]
    public void SimpleParametersStillMatch()
    {
        var data = GetParameters("GetRandomInt(1,6)", "set", "GetRandomInt(#min#,#max#)");

        Assert.AreEqual("1", data.GetAttribute("min"));
        Assert.AreEqual("6", data.GetAttribute("max"));
    }

    [TestMethod]
    public void TemplateDoesNotMatchAcrossUnbalancedBrackets()
    {
        // Previously "Got(#object#)" matched this with object = "a) and Got(b"
        Assert.IsNull(_controller.GetExpressionEditorDefinition("Got(a) and Got(b)", "if"));
    }
}
