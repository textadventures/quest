using System.Text.RegularExpressions;
using QuestViva.Engine.Scripts;

namespace QuestViva.Engine;

// The files a game's own elements name, for `qv publish` to warn about one that isn't beside the
// game: attributes whose whole value is a filename, such as a picture or a <javascript src>, and
// quoted filenames in scripts, such as picture ("map.png"). Library elements are skipped: the
// libraries built into Quest ship with it, and an included library's files are the author's to
// check. It goes by the extension, so it finds the usual images, sounds, scripts, stylesheets and
// fonts, not every file a game could open.
public static class GameFileReferences
{
    private static readonly Regex Filename = new(
        @"^[^/\\:*?""<>|\r\n]+\.(png|jpe?g|gif|svg|webp|bmp|ico|mp3|wav|ogg|oga|m4a|aac|flac|mp4|webm|ogv|js|css|ttf|otf|woff2?)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex QuotedString = new(@"""([^""\r\n]*)""", RegexOptions.CultureInvariant);

    public static IReadOnlyCollection<string> Find(WorldModel world)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var element in world.Elements.GetElements())
        {
            if (element.MetaFields[MetaFieldDefinitions.Library])
            {
                continue;
            }

            foreach (var attribute in element.Fields.FieldNames.ToList())
            {
                switch (element.Fields.Get(attribute))
                {
                    case string value when Filename.IsMatch(value.Trim()):
                        names.Add(value.Trim());
                        break;
                    case IScript script:
                        AddFromScript(script);
                        break;
                }
            }
        }

        return names;

        void AddFromScript(IScript script)
        {
            string text;
            try
            {
                text = script is LazyLoadScript { UnparsedScript: { } unparsed } ? unparsed : script.Save();
            }
            catch
            {
                // A script that can't be read is one for qv check to report.
                return;
            }

            foreach (Match match in QuotedString.Matches(text))
            {
                var value = match.Groups[1].Value.Trim();
                if (Filename.IsMatch(value))
                {
                    names.Add(value);
                }
            }
        }
    }
}
