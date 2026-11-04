using System.Text.RegularExpressions;
using QuestViva.PlayerCore;

namespace QuestViva.PlayerCoreTests;

[TestClass]
public class PlayerExportTests
{
    private const string Cdn = "https://cdn.jsdelivr.net/npm/@textadventures/quest-viva-wasmplayer@6.1.0-beta.5/";

    private static readonly ExportedGame Game = new(
        [1, 2, 3], "my game.quest", "Golden & \"Test\"", "A tiny test & nothing more.",
        "448e73df-2d2f-47e7-a494-a46b40d4cfb3");

    private static string Template => File.ReadAllText("player-index.html");

    // As inject-version.mjs leaves the built page: every asset URL stamped with ?v=.
    private static string StampedTemplate =>
        Regex.Replace(Template, @"(src|href)=""((?:[\w./-]+)\.(?:js|css|svg))""", @"$1=""$2?v=6.1.0""");

    [TestMethod]
    public void SingleFileLoadsThePlayerFromTheCdn()
    {
        var html = PlayerExport.BuildHtml(Template, Game, Cdn);
        StringAssert.Contains(html, "<html lang=\"en\" class=\"qv-booting\">");
        StringAssert.Contains(html,
            $"<head prefix=\"ifiction: http://babel.ifarchive.org/protocol/iFiction/\">\n    <base href=\"{Cdn}\" />\n" +
            "    <meta property=\"ifiction:ifid\" content=\"448E73DF-2D2F-47E7-A494-A46B40D4CFB3\" />");
        StringAssert.Contains(html, "<link rel=\"stylesheet\" type=\"text/css\" href=\"chrome.css\" crossorigin=\"anonymous\" />");
    }

    [TestMethod]
    public void EmbedsTheGameBeforeThePlayerScript()
    {
        var html = PlayerExport.BuildHtml(Template, Game, null);
        StringAssert.Contains(html,
            "    <script type=\"text/javascript\">\n" +
            "        window.QuestVivaEmbeddedGame = \"AQID\";\n" +
            "        window.QuestVivaEmbeddedGameFilename = \"my game.quest\";\n" +
            "    </script>\n" +
            "    <script type=\"text/javascript\" src=\"wasm-player.js\"></script>");
        Assert.DoesNotContain("quest-config.js", html);
    }

    [TestMethod]
    public void ZipLeavesRelativeUrlsAlone()
    {
        var html = PlayerExport.BuildHtml(Template, Game, null);
        Assert.DoesNotContain("<base", html);
        Assert.DoesNotContain("crossorigin", html);
        StringAssert.Contains(html, "<meta property=\"ifiction:ifid\"");
    }

    [TestMethod]
    public void UsesTheGamesTitleAndDescriptionForLinkPreviews()
    {
        var html = PlayerExport.BuildHtml(Template, Game with { CoverUrl = "cover.png" }, null);
        StringAssert.Contains(html,
            "<title>Golden &amp; &quot;Test&quot;</title>\n" +
            "    <meta name=\"description\" content=\"A tiny test &amp; nothing more.\" />\n" +
            "    <meta property=\"og:type\" content=\"website\" />\n" +
            "    <meta property=\"og:title\" content=\"Golden &amp; &quot;Test&quot;\" />\n" +
            "    <meta property=\"og:description\" content=\"A tiny test &amp; nothing more.\" />\n" +
            "    <meta property=\"og:image\" content=\"cover.png\" />\n" +
            "    <meta name=\"twitter:card\" content=\"summary\" />\n");
        Assert.DoesNotContain("og:site_name", html);
        Assert.DoesNotContain("Play and create text adventure games", html);
    }

    [TestMethod]
    public void WithoutAnIfidOrDescriptionThoseTagsAreLeftOut()
    {
        var html = PlayerExport.BuildHtml(Template, Game with { Ifid = null, Description = null }, null);
        StringAssert.Contains(html, "<head>\n");
        Assert.DoesNotContain("ifiction", html);
        Assert.DoesNotContain("name=\"description\"", html);
    }

    [TestMethod]
    public void StripsTheShellsComments()
    {
        Assert.DoesNotContain("<!--", PlayerExport.BuildHtml(Template, Game, Cdn));
    }

    // #2233: the built page's URLs carry ?v=, and an exact match on src="wasm-player.js" made
    // exports that never started.
    [TestMethod]
    public void WorksOnTheVersionStampedPage()
    {
        var html = PlayerExport.BuildHtml(StampedTemplate, Game, Cdn);
        StringAssert.Contains(html, "window.QuestVivaEmbeddedGame = \"AQID\";\n" +
                                    "        window.QuestVivaEmbeddedGameFilename = \"my game.quest\";\n" +
                                    "    </script>\n" +
                                    "    <script type=\"text/javascript\" src=\"wasm-player.js?v=6.1.0\"></script>");
        StringAssert.Contains(html, "href=\"chrome.css?v=6.1.0\" crossorigin=\"anonymous\" />");
        Assert.DoesNotContain("quest-config.js", html);
    }

    [TestMethod]
    public void APageItCantRewriteIsAnError()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            PlayerExport.BuildHtml("<html lang=\"en\"><head><title>x</title>\n</head></html>", Game, null));
        StringAssert.Contains(ex.Message, "quest-config.js");
    }

    [TestMethod]
    public void AFilenameCantCloseTheScript()
    {
        var html = PlayerExport.BuildHtml(Template, Game with { Filename = "</script><b>.quest" }, null);
        Assert.DoesNotContain("</script><b>", html);
    }

    [TestMethod]
    public void ListsThePlayerFilesForAZip()
    {
        const string manifest = """["index.html", "quest-config.js", "wasm-player.js", "legacy/player.js"]""";
        const string boot = """
            export const config = /*json-start*/{"resources":{"hash":"sha256-x",
              "jsModuleNative":[{"name":"dotnet.native.js"}],
              "assembly":[{"name":"QuestViva.Engine.wasm","hash":"y"},{"name":"System.Private.CoreLib.wasm"}],
              "icu":[{"name":"icudt.dat"}]}}/*json-end*/;
            """;
        CollectionAssert.AreEqual(new[]
        {
            "index.html", "wasm-player.js", "legacy/player.js",
            "_framework/dotnet.boot.js", "_framework/dotnet.js", "_framework/dotnet.native.js",
            "_framework/QuestViva.Engine.wasm", "_framework/System.Private.CoreLib.wasm", "_framework/icudt.dat"
        }, PlayerExport.PlayerFiles(manifest, boot).ToList());
    }

    [TestMethod]
    public void DescriptionsBecomePlainText()
    {
        Assert.AreEqual("A tiny test & nothing more.",
            PlayerExport.PlainText("A <i>tiny</i> test &amp; nothing\n   more."));
        Assert.IsNull(PlayerExport.PlainText("<br/> "));
        Assert.IsNull(PlayerExport.PlainText(null));
    }
}
