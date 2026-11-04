using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace QuestViva.PlayerCore;

// What an HTML export says about the game: the package it plays, and what goes in the page's
// title, link-preview tags and IFID meta. Description is plain text (see PlainText).
public sealed record ExportedGame(
    byte[] Package,
    string Filename,
    string? Title,
    string? Description,
    string? Ifid,
    string? CoverUrl = null);

// Turns WasmPlayer's index.html into a page that plays one game, for the Editor's two HTML exports
// (through WasmEditorBridge) and `qv publish --format html|zip`. See
// docs/wasmplayer-single-file-export.md for the design.
//
// - Single file (cdnBase set): the game is embedded, and a <base href> makes the page load the
//   player from jsDelivr at a pinned version.
// - Zip (cdnBase null): the same page sits beside a copy of the player (PlayerFiles).
//
// The template is the player's own index.html, rewritten with regular expressions. inject-version.mjs
// stamps ?v=<version> onto its asset URLs, so the patterns accept an optional query (an exact
// src="wasm-player.js" once missed and made exports that never started, #2233). A pattern that
// doesn't match throws, since a quietly unchanged page would look right and not play.
public static class PlayerExport
{
    public static string CdnBase(string version) =>
        $"https://cdn.jsdelivr.net/npm/@textadventures/quest-viva-wasmplayer@{version}/";

    public static string BuildHtml(string template, ExportedGame game, string? cdnBase)
    {
        var html = template;
        // So the start screen's file and URL pickers never paint, even for a frame: this page only
        // ever plays its own game.
        html = ReplaceFirst(html, "<html lang=\"en\">", "<html lang=\"en\" class=\"qv-booting\">");

        // The game's own title and link-preview tags in place of the shell's, which describe Quest
        // Viva. No og:site_name, since wherever the author hosts it isn't Quest Viva's site.
        if (!string.IsNullOrEmpty(game.Title))
        {
            html = ReplaceOrFail(html, new Regex("<title>[^<]*</title>"),
                _ => $"<title>{EscapeAttribute(game.Title)}</title>", "the <title> tag");
        }

        html = Regex.Replace(html,
            """^[ \t]*<meta (?:name="description"|property="og:[^"]*"|name="twitter:[^"]*")[^>]*/>\r?\n""", "",
            RegexOptions.Multiline);
        var previewTags = new[]
        {
            string.IsNullOrEmpty(game.Description)
                ? null
                : $"    <meta name=\"description\" content=\"{EscapeAttribute(game.Description)}\" />",
            "    <meta property=\"og:type\" content=\"website\" />",
            string.IsNullOrEmpty(game.Title)
                ? null
                : $"    <meta property=\"og:title\" content=\"{EscapeAttribute(game.Title)}\" />",
            string.IsNullOrEmpty(game.Description)
                ? null
                : $"    <meta property=\"og:description\" content=\"{EscapeAttribute(game.Description)}\" />",
            string.IsNullOrEmpty(game.CoverUrl)
                ? null
                : $"    <meta property=\"og:image\" content=\"{EscapeAttribute(game.CoverUrl)}\" />",
            string.IsNullOrEmpty(game.CoverUrl) ? null : "    <meta name=\"twitter:card\" content=\"summary\" />"
        }.Where(t => t != null);
        html = ReplaceOrFail(html, new Regex(@"</title>\r?\n"),
            line => line + string.Join("\n", previewTags) + "\n", "the </title> line");

        // The Babel Treaty's IFID for an HTML story file: the game's id as an ifiction:ifid meta,
        // uppercase as the treaty's UUID:// form is, with the RDFa prefix that keeps it valid RDFa.
        var ifid = game.Ifid?.Trim().ToUpperInvariant();
        var headAttributes = string.IsNullOrEmpty(ifid)
            ? ""
            : " prefix=\"ifiction: http://babel.ifarchive.org/protocol/iFiction/\"";
        var headExtras = string.Join("\n", new[]
        {
            // Every relative URL in the page then resolves against the CDN.
            cdnBase == null ? null : $"    <base href=\"{cdnBase}\" />",
            string.IsNullOrEmpty(ifid) ? null : $"    <meta property=\"ifiction:ifid\" content=\"{ifid}\" />"
        }.Where(t => t != null));
        if (headAttributes.Length > 0 || headExtras.Length > 0)
        {
            html = ReplaceFirst(html, "<head>",
                headExtras.Length > 0 ? $"<head{headAttributes}>\n{headExtras}" : $"<head{headAttributes}>");
        }

        // quest-config.js configures a hosted player (API root, default game); nothing here uses it.
        html = ReplaceOrFail(html, ScriptLine("quest-config.js"), _ => "", "the quest-config.js tag");

        // A stylesheet from the CDN needs crossorigin for scripts to read its cssRules (#2192).
        // jsDelivr allows any origin. Not for a zip, whose player is on the page's own host, which
        // may not send the header. The player UI's own stylesheets aren't in the page: wasm-player.js
        // adds them, with crossorigin when they come from another origin.
        if (cdnBase != null)
        {
            html = ReplaceOrFail(html, StyleLine("chrome.css"),
                line => Regex.Replace(line, @" />(\r?\n)$", " crossorigin=\"anonymous\" />$1"),
                "the chrome.css stylesheet link");
        }

        // Notes about the player shell's own implementation are no use in a game's page.
        html = Regex.Replace(html, @"[ \t]*<!--[\s\S]*?-->\r?\n?", "");

        // The game, before wasm-player.js, which looks for it first thing. Its filename's extension
        // says how to load it (.quest, .aslx, or Quest 4's .asl/.cas). Embedded last, so none of the
        // rewriting above has to scan or copy the package, which can run to tens of megabytes: in
        // the Editor this runs in interpreted .NET on the browser's one thread.
        var embed = "    <script type=\"text/javascript\">\n" +
                    $"        window.QuestVivaEmbeddedGame = \"{Convert.ToBase64String(game.Package)}\";\n" +
                    $"        window.QuestVivaEmbeddedGameFilename = \"{JsonEncodedText.Encode(game.Filename)}\";\n" +
                    "    </script>\n";
        return ReplaceOrFail(html, ScriptLine("wasm-player.js"), line => embed + line, "the wasm-player.js tag");
    }

    // The player files a zip export needs beside its page: the shell files that
    // scripts/write-export-manifest.mjs lists in export-manifest.json, without quest-config.js,
    // which the page doesn't load, and the runtime files that _framework/dotnet.boot.js names.
    public static IReadOnlyList<string> PlayerFiles(string exportManifestJson, string bootJs)
    {
        List<string> shell;
        using (var manifest = JsonDocument.Parse(exportManifestJson))
        {
            shell = manifest.RootElement.EnumerateArray().Select(e => e.GetString()!).ToList();
        }

        const string startMarker = "/*json-start*/";
        const string endMarker = "/*json-end*/";
        var start = bootJs.IndexOf(startMarker, StringComparison.Ordinal);
        var end = bootJs.IndexOf(endMarker, StringComparison.Ordinal);
        if (start < 0 || end < 0)
        {
            throw new InvalidOperationException("Couldn't read the player's boot manifest (dotnet.boot.js).");
        }

        var framework = new List<string> { "dotnet.boot.js", "dotnet.js" };
        using (var boot = JsonDocument.Parse(bootJs[(start + startMarker.Length)..end]))
        {
            foreach (var group in boot.RootElement.GetProperty("resources").EnumerateObject())
            {
                if (group.Name == "hash" || group.Value.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                framework.AddRange(group.Value.EnumerateArray()
                    .Where(item => item.ValueKind == JsonValueKind.Object &&
                                   item.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String)
                    .Select(item => item.GetProperty("name").GetString()!));
            }
        }

        return shell.Where(p => p != "quest-config.js")
            .Concat(framework.Select(n => "_framework/" + n))
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    // A game's description, which is rich text, as plain text for link-preview tags, where markup or
    // entities would show up literally. Null when there's nothing left.
    public static string? PlainText(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return null;
        }

        var text = WebUtility.HtmlDecode(Regex.Replace(html, "<[^>]*>", ""));
        text = Regex.Replace(text, @"\s+", " ").Trim();
        return text.Length == 0 ? null : text;
    }

    private static string EscapeAttribute(string value) =>
        value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

    private static Regex ScriptLine(string file) => new(
        $"""^[ \t]*<script type="text/javascript" src="{Regex.Escape(file)}(?:\?[^"]*)?"></script>\r?\n""",
        RegexOptions.Multiline);

    private static Regex StyleLine(string file) => new(
        $"""^[ \t]*<link rel="stylesheet" type="text/css" href="{Regex.Escape(file)}(?:\?[^"]*)?" />\r?\n""",
        RegexOptions.Multiline);

    private static string ReplaceFirst(string text, string find, string replacement)
    {
        var index = text.IndexOf(find, StringComparison.Ordinal);
        return index < 0 ? text : text[..index] + replacement + text[(index + find.Length)..];
    }

    private static string ReplaceOrFail(string text, Regex pattern, Func<string, string> replacement, string what)
    {
        var match = pattern.Match(text);
        if (!match.Success)
        {
            throw new InvalidOperationException($"Couldn't find {what} in the player's index.html.");
        }

        return new StringBuilder(text, 0, match.Index, text.Length)
            .Append(replacement(match.Value))
            .Append(text, match.Index + match.Length, text.Length - match.Index - match.Length)
            .ToString();
    }
}
