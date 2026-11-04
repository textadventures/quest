using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace QuestViva.Cli;

// The reference, "understanding" and how-to pages of the documentation site (site/src/content/docs),
// built into qv so an agent reads the docs for the version of Quest it's running, offline. Each page is
// split into sections at its "## " headings; reference pages have one per function or command.
internal static partial class QuestDocs
{
    private const int MaxSectionLength = 6000;
    private const int FullResults = 3;
    private const int MoreResults = 10;

    internal sealed record Page(string Id, string Title, string? Description, IReadOnlyList<Section> Sections);

    internal sealed record Section(Page Page, string Heading, string Text)
    {
        public string Anchor => Slug(Heading);
    }

    private static readonly Lazy<IReadOnlyList<Page>> LazyPages = new(Load);

    internal static IReadOnlyList<Page> Pages => LazyPages.Value;

    // Every page, with its title and what it covers.
    public static string Contents()
    {
        var result = new StringBuilder(
            "Quest Viva documentation built into qv. Pass a page id as page to read it, or search with query.\n\n");
        foreach (var page in Pages.OrderBy(p => p.Id, StringComparer.Ordinal))
        {
            result.Append(page.Id).Append(": ").Append(page.Title);
            if (page.Description != null)
            {
                result.Append(" - ").Append(page.Description);
            }

            result.Append('\n');
        }

        return result.ToString();
    }

    // A page, or one section of it with "page#section". The id may leave out its folders, as in
    // "string" for "reference/functions/string", when that's unambiguous, and links in the docs
    // ("/reference/functions/string#template") work as they are.
    public static string Read(string id)
    {
        var parts = id.Trim().TrimStart('/').Split('#', 2);
        var path = parts[0].TrimEnd('/');
        if (path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            path = path[..^3];
        }

        var matches = Pages.Where(p => p.Id.Equals(path, StringComparison.OrdinalIgnoreCase)).ToList();
        if (matches.Count == 0)
        {
            matches = Pages.Where(p => p.Id.EndsWith("/" + path, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (matches.Count != 1)
        {
            return (matches.Count == 0 ? $"There's no page '{path}'." : $"'{path}' could be any of these pages.") +
                   " Pages:\n" + string.Join("\n", (matches.Count == 0 ? Pages : matches).Select(p => p.Id).Order()) + "\n";
        }

        var page = matches[0];
        if (parts.Length == 1 || parts[1].Length == 0)
        {
            return string.Join("\n", page.Sections.Select(s => s.Text)).TrimEnd() + "\n";
        }

        var anchor = parts[1];
        var section = page.Sections.FirstOrDefault(s =>
            s.Anchor == Slug(anchor) || s.Heading.Equals(anchor, StringComparison.OrdinalIgnoreCase));
        return section != null
            ? Format(section)
            : $"There's no section '{anchor}' on {page.Id}. Its sections: " +
              string.Join(", ", page.Sections.Skip(1).Select(s => s.Heading)) + "\n";
    }

    // The sections that best match the query: an exact heading match first (a function or command
    // name), then the sections containing every word, those with the words in their heading
    // ranked highest.
    public static string Search(string query)
    {
        var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (words.Length == 0)
        {
            return Contents();
        }

        var ranked = Pages.SelectMany(p => p.Sections)
            .Select(s => (Section: s, Score: Score(s, query.Trim(), words)))
            .Where(r => r.Score > 0)
            .OrderByDescending(r => r.Score)
            .Select(r => r.Section)
            .ToList();

        if (ranked.Count == 0)
        {
            return $"Nothing in the docs matches '{query}'. Try fewer or other words, or call docs with no " +
                   "arguments to list the pages.\n";
        }

        // A query that names a command or function gets just its entries in full.
        var exact = ranked.TakeWhile(s => s.Heading.Equals(query.Trim(), StringComparison.OrdinalIgnoreCase)).Count();
        var full = exact > 0 ? exact : FullResults;

        var result = new StringBuilder();
        foreach (var section in ranked.Take(full))
        {
            result.Append(Format(section)).Append('\n');
        }

        var more = ranked.Skip(full).Take(MoreResults).ToList();
        if (more.Count > 0)
        {
            result.Append("Also matching, to read with page:\n");
            foreach (var section in more)
            {
                result.Append(section.Page.Id).Append('#').Append(section.Anchor).Append('\n');
            }
        }

        return result.ToString();
    }

    private static int Score(Section section, string query, string[] words)
    {
        if (section.Heading.Equals(query, StringComparison.OrdinalIgnoreCase))
        {
            return 1000;
        }

        var score = 0;
        foreach (var word in words)
        {
            var inHeading = section.Heading.Contains(word, StringComparison.OrdinalIgnoreCase);
            var count = Count(section.Text, word);
            if (count == 0)
            {
                return 0;
            }

            score += (inHeading ? 50 : 0) + Math.Min(count, 5);
        }

        return score;
    }

    private static int Count(string text, string word)
    {
        var count = 0;
        for (var i = text.IndexOf(word, StringComparison.OrdinalIgnoreCase);
             i >= 0;
             i = text.IndexOf(word, i + word.Length, StringComparison.OrdinalIgnoreCase))
        {
            count++;
        }

        return count;
    }

    private static string Format(Section section)
    {
        var text = section.Text.TrimEnd();
        if (text.Length > MaxSectionLength)
        {
            text = text[..MaxSectionLength] + $"\n[... read the rest with page {section.Page.Id}#{section.Anchor}]";
        }

        return $"[{section.Page.Id}#{section.Anchor}]\n{text}\n";
    }

    // As the docs site makes heading anchors: lower case, spaces to hyphens, punctuation dropped.
    private static string Slug(string heading) =>
        NonSlugCharacters().Replace(heading.Trim().ToLowerInvariant().Replace(' ', '-'), "");

    private static List<Page> Load()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var pages = new List<Page>();
        foreach (var name in assembly.GetManifestResourceNames().Where(n => n.StartsWith("docs/", StringComparison.Ordinal)))
        {
            using var stream = assembly.GetManifestResourceStream(name)!;
            using var reader = new StreamReader(stream);
            var id = name["docs/".Length..^".md".Length].Replace('\\', '/');
            pages.Add(Parse(id, reader.ReadToEnd()));
        }

        return pages;
    }

    internal static Page Parse(string id, string markdown)
    {
        var lines = markdown.Replace("\r\n", "\n").Split('\n').ToList();
        string? title = null;
        string? description = null;

        // Front matter: the title and description the site shows.
        if (lines.Count > 0 && lines[0] == "---")
        {
            var end = lines.IndexOf("---", 1);
            if (end > 0)
            {
                foreach (var line in lines.Take(end).Skip(1))
                {
                    if (line.StartsWith("title:", StringComparison.Ordinal))
                    {
                        title = Unquote(line[6..]);
                    }
                    else if (line.StartsWith("description:", StringComparison.Ordinal))
                    {
                        description = Unquote(line[12..]);
                    }
                }

                lines.RemoveRange(0, end + 1);
            }
        }

        title ??= id;
        var sections = new List<(string Heading, StringBuilder Text)> { (title, new StringBuilder($"# {title}\n")) };
        var inCode = false;
        foreach (var line in lines)
        {
            if (line.StartsWith("```", StringComparison.Ordinal))
            {
                inCode = !inCode;
            }

            if (!inCode && line.StartsWith("## ", StringComparison.Ordinal))
            {
                sections.Add((line[3..].Trim(), new StringBuilder()));
            }

            sections[^1].Text.Append(inCode ? line : Clean(line)).Append('\n');
        }

        var page = new Page(id, title, description, []);
        var built = sections
            .Where((s, i) => i > 0 || s.Text.ToString().Trim().Length > title.Length + 2)
            .Select(s => new Section(page, s.Heading, s.Text.ToString().Trim() + "\n"))
            .ToList();
        return page with { Sections = built };
    }

    private static string Unquote(string value)
    {
        value = value.Trim();
        return value.Length >= 2 && value[0] == '"' && value[^1] == '"' ? value[1..^1] : value;
    }

    // The site's badges ("hard-coded", "Quest Viva 6.0") as [badge], and other HTML links as their text.
    private static string Clean(string line) =>
        HtmlLink().Replace(line, m => m.Groups["attrs"].Value.Contains("qv-badge")
            ? $"[{m.Groups["text"].Value}]"
            : m.Groups["text"].Value);

    [GeneratedRegex("<a (?<attrs>[^>]*)>(?<text>[^<]*)</a>")]
    private static partial Regex HtmlLink();

    [GeneratedRegex("[^a-z0-9_-]")]
    private static partial Regex NonSlugCharacters();
}
