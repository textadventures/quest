using QuestViva.Common;
using QuestViva.Engine;
using Moq;

namespace QuestViva.EngineTests;

[TestClass]
public class TemplateTests
{
    private WorldModel m_worldModel;

    [TestInitialize]
    public void Init()
    {
        m_worldModel = Helpers.CreateWorldModel();
        m_worldModel.Template.AddTemplate("Test1", "my text", false);
        m_worldModel.Template.AddTemplate("Test2", "other text", false);
    }

    [TestMethod]
    public void TestGetText()
    {
        Assert.AreEqual("my text", m_worldModel.Template.GetText("Test1"));
        Assert.AreEqual("other text", m_worldModel.Template.GetText("Test2"));
    }

    [TestMethod]
    public void TestReplaceTemplateText()
    {
        Assert.AreEqual("this is my text", m_worldModel.Template.ReplaceTemplateText("this is [Test1]"));
        Assert.AreEqual("this is other text", m_worldModel.Template.ReplaceTemplateText("this is [Test2]"));
        Assert.AreEqual("my text is my other text", m_worldModel.Template.ReplaceTemplateText("[Test1] is my [Test2]"));
    }

    [TestMethod]
    public void TestReplaceTemplateText_InvalidTemplateNames()
    {
        Assert.AreEqual("[unknown]", m_worldModel.Template.ReplaceTemplateText("[unknown]"));
        Assert.AreEqual("[unknown] and my text", m_worldModel.Template.ReplaceTemplateText("[unknown] and [Test1]"));
        Assert.AreEqual("other text and [unknown]", m_worldModel.Template.ReplaceTemplateText("[Test2] and [unknown]"));
        Assert.AreEqual("[unknown1], my text, [unknown2], other text",
            m_worldModel.Template.ReplaceTemplateText("[unknown1], [Test1], [unknown2], [Test2]"));
    }

    // Regression test for a bug affecting games loaded as a bare .aslx (not a compiled .quest
    // package - e.g. WasmPlayer's ?id= route, which fetches an unpacked .aslx via the
    // textadventures.co.uk API rather than a .quest zip). Loading a bare .aslx runs
    // GameLoader.ScanForTemplates as a preliminary pass over the base file, which locks in
    // the *first* value it sees for each template name as an unoverridable "base template" so
    // that a later library file can't clobber a game's own override. Old Quest 5.8 desktop
    // exports inline a full language pack directly in the base file instead of as a separate
    // library (e.g. an English block followed by a German block, both defining the same
    // template names) - and the base-template guard was blocking the second, later
    // definition from winning even though both come from the base file itself, silently
    // keeping the first (English) text. duplicatetemplatetest.aslx reproduces that shape:
    // the same template name defined twice with no <include>d libraries at all, so the only
    // way "German value" can win is if later-in-file redefinitions of a base file's own
    // templates are honoured, matching ordinary top-to-bottom XML semantics.
    [TestMethod]
    public async Task DuplicateTemplateInBaseAslxFile_LaterDefinitionWins()
    {
        var data = await new FileGameDataProvider("duplicatetemplatetest.aslx").GetData();
        var model = new WorldModel(data, null);

        var success = await model.Initialise(new Mock<IPlayer>().Object);

        Assert.IsTrue(success, string.Join("; ", model.Errors));
        Assert.AreEqual("German value", model.Template.GetText("DupeTemplate"));
    }
}