using System.Text;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// Regression test for issue #2255: adding a template whose name clashes with one already
// defined in the game file (a "base" template) used to fail with a bare null-reference error
// from EditorController.CreateNewTemplate, instead of the validation message CanAddTemplate
// already exists to provide.
[TestClass]
public class TemplateCreationTests
{
    private EditorController _controller = null!;

    [TestInitialize]
    public async Task Setup()
    {
        const string xml = """
            <asl version="580">
              <game name="Test Game"/>
              <template name="UnknownObject">existing</template>
            </asl>
            """;

        _controller = new EditorController();
        var bytes = Encoding.UTF8.GetBytes(xml);
        await _controller.Initialise(new ByteArrayGameDataProvider(bytes, "test.aslx"));
    }

    [TestCleanup]
    public void Cleanup()
    {
        _controller.Dispose();
    }

    [TestMethod]
    public void CanAddTemplateRejectsNameClashingWithBaseTemplate()
    {
        var result = _controller.CanAddTemplate("UnknownObject");
        Assert.IsFalse(result.Valid);
        Assert.AreEqual(ValidationMessage.ElementAlreadyExists, result.Message);
    }

    [TestMethod]
    public void CanAddTemplateAllowsNewName()
    {
        var result = _controller.CanAddTemplate("SomeOtherTemplate");
        Assert.IsTrue(result.Valid);
    }
}
