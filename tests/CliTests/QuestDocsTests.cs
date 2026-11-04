using QuestViva.Cli;

namespace QuestViva.CliTests;

[TestClass]
public class QuestDocsTests
{
    [TestMethod]
    public void TheSitesPagesAreBuiltIn()
    {
        var ids = QuestDocs.Pages.Select(p => p.Id).ToList();
        CollectionAssert.Contains(ids, "reference/script-commands");
        CollectionAssert.Contains(ids, "reference/functions/string");
        CollectionAssert.Contains(ids, "understanding/values-and-types");
        Assert.IsTrue(ids.Any(id => id.StartsWith("howto/")));
    }

    [TestMethod]
    public void ContentsListsPagesWithTheirTitles()
    {
        StringAssert.Contains(QuestDocs.Contents(), "reference/script-commands: Script commands\n");
    }

    [TestMethod]
    public void AnExactNameGivesJustItsEntry()
    {
        var result = QuestDocs.Search("GetBoolean");
        StringAssert.StartsWith(result, "[reference/functions/attributes#getboolean]\n## GetBoolean\n");
        Assert.AreEqual(1, result.Split("\n## ").Length - 1, "only the exact match is shown in full");
    }

    [TestMethod]
    public void WordsFindSectionsContainingAllOfThem()
    {
        StringAssert.Contains(QuestDocs.Search("unset attribute"), "## Unset attributes");
    }

    [TestMethod]
    public void NoMatchSaysSo()
    {
        StringAssert.StartsWith(QuestDocs.Search("zzqqxx"), "Nothing in the docs matches");
    }

    [TestMethod]
    [DataRow("reference/functions/string#template")]
    [DataRow("/reference/functions/string#template")]
    [DataRow("functions/string#Template")]
    [DataRow("string.md#template")]
    public void ASectionByLinkOrShortId(string id)
    {
        StringAssert.StartsWith(QuestDocs.Read(id), "[reference/functions/string#template]\n## Template\n");
    }

    [TestMethod]
    public void AWholePageStartsWithItsTitle()
    {
        StringAssert.StartsWith(QuestDocs.Read("reference/script-commands"), "# Script commands\n");
    }

    [TestMethod]
    public void AnUnknownPageListsThePages()
    {
        var result = QuestDocs.Read("nosuchpage");
        StringAssert.StartsWith(result, "There's no page 'nosuchpage'.");
        StringAssert.Contains(result, "reference/script-commands\n");
    }

    [TestMethod]
    public void ParseSplitsAtHeadingsButNotInCode()
    {
        var page = QuestDocs.Parse("test/page", """
            ---
            title: "A page"
            description: What it covers
            ---

            Intro.

            ## First
            <a href="/reference/functions/hardcoded" class="qv-badge">hard-coded</a> and <a href="/x">a link</a>

            ```quest
            ## not a heading
            ```

            ## Second
            Text.
            """);

        Assert.AreEqual("A page", page.Title);
        Assert.AreEqual("What it covers", page.Description);
        CollectionAssert.AreEqual(new[] { "A page", "First", "Second" }, page.Sections.Select(s => s.Heading).ToArray());
        StringAssert.Contains(page.Sections[1].Text, "[hard-coded] and a link");
        StringAssert.Contains(page.Sections[1].Text, "## not a heading");
    }
}
