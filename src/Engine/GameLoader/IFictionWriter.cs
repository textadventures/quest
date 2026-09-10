using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using QuestViva.Common;

namespace QuestViva.Engine.GameLoader;

/// <summary>
///     Builds a Treaty of Babel iFiction record from a WorldModel's bibliographic fields,
///     for embedding as <c>metadata.iFiction</c> inside a published <c>.quest</c> package.
/// </summary>
internal static class IFictionWriter
{
    private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex HtmlTag = new(@"<[^>]+>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex BrTag = new(@"<br\s*/?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex ClosePTag = new(@"</p\s*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex OpenPTag = new(@"<p\b[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex FirstPublishedYear = new(@"^\d{4}$", RegexOptions.Compiled);
    private static readonly Regex FirstPublishedDate = new(@"^\d{4}-\d{2}-\d{2}$", RegexOptions.Compiled);

    public static string Write(WorldModel worldModel,
        IEnumerable<WorldModel.PackageIncludeFile>? includeFiles)
    {
        var game = worldModel.Game.Fields;
        var gameId = game.GetString("gameid");
        if (string.IsNullOrWhiteSpace(gameId))
        {
            throw new InvalidOperationException(
                "Cannot publish without a gameid (IFID). Generate one in the game's Setup tab.");
        }

        var ifid = gameId.Trim().ToUpperInvariant();
        var title = NormalizeText(game.GetString("gamename")) ?? "An Interactive Fiction";
        var author = NormalizeText(game.GetString("author")) ?? "Anonymous";
        var headline = NormalizeText(game.GetString("subtitle"));
        var genre = NormalizeText(game.GetString("category"));
        var firstPublished = NormalizeFirstPublished(game.GetString("firstpublished"));
        var description = ToIFictionDescription(game.GetString("description"));
        var language = NormalizeLanguage(worldModel.LanguageId);
        var cover = TryGetCoverInfo(game.GetString("cover"), includeFiles);

        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            Indent = true,
            IndentChars = "  ",
            OmitXmlDeclaration = false,
            NewLineChars = "\n",
            NewLineHandling = NewLineHandling.Replace
        };

        using var stringWriter = new StringWriter(CultureInfo.InvariantCulture) { NewLine = "\n" };
        using (var writer = XmlWriter.Create(stringWriter, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("ifindex", "http://babel.ifarchive.org/protocol/iFiction/");
            writer.WriteAttributeString("version", "1.0");

            writer.WriteStartElement("story");

            writer.WriteStartElement("identification");
            writer.WriteElementString("ifid", ifid);
            writer.WriteElementString("format", "quest");
            writer.WriteEndElement(); // identification

            writer.WriteStartElement("bibliographic");
            writer.WriteElementString("title", title);
            writer.WriteElementString("author", author);
            if (headline != null)
            {
                writer.WriteElementString("headline", headline);
            }

            if (genre != null)
            {
                writer.WriteElementString("genre", genre);
            }

            if (firstPublished != null)
            {
                writer.WriteElementString("firstpublished", firstPublished);
            }

            if (description != null)
            {
                WriteDescription(writer, description);
            }

            if (language != null)
            {
                writer.WriteElementString("language", language);
            }

            writer.WriteEndElement(); // bibliographic

            if (cover != null)
            {
                writer.WriteStartElement("cover");
                writer.WriteElementString("format", cover.Format);
                writer.WriteElementString("height", cover.Height.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("width", cover.Width.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement(); // cover
            }

            // Format-specific section
            var questVersion = NormalizeText(game.GetString("version"));
            var coverLeafname = cover != null
                ? Path.GetFileName(NormalizeText(game.GetString("cover")))
                : null;
            var questStyle = worldModel.IsGamebook ? "Gamebook" : "Text Adventure";
            writer.WriteStartElement("quest");
            writer.WriteElementString("style", questStyle);
            if (questVersion != null)
            {
                writer.WriteElementString("version", questVersion);
            }

            if (coverLeafname != null)
            {
                writer.WriteElementString("coverleafname", coverLeafname);
            }

            writer.WriteEndElement(); // quest

            var releaseDate = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var releaseVersion = FormatReleaseVersion(game);
            writer.WriteStartElement("releases");
            writer.WriteStartElement("attached");
            writer.WriteStartElement("release");
            if (releaseVersion != null)
            {
                writer.WriteElementString("version", releaseVersion);
            }

            writer.WriteElementString("releasedate", releaseDate);
            writer.WriteElementString("compiler", "Quest Viva");
            writer.WriteElementString("compilerversion", VersionInfo.Version);
            writer.WriteEndElement(); // release
            writer.WriteEndElement(); // attached
            writer.WriteEndElement(); // releases

            writer.WriteStartElement("colophon");
            writer.WriteElementString("generator", "Quest Viva");
            writer.WriteElementString("generatorversion", VersionInfo.Version);
            writer.WriteElementString("originated", releaseDate);
            writer.WriteEndElement(); // colophon

            writer.WriteEndElement(); // story
            writer.WriteEndElement(); // ifindex
            writer.WriteEndDocument();
        }

        // XmlWriter emits a UTF-8 declaration claiming encoding="utf-16" when writing to
        // StringWriter; rewrite the declaration to match the UTF-8 bytes we actually store.
        var xml = stringWriter.ToString();
        if (xml.StartsWith("<?xml", StringComparison.Ordinal))
        {
            var end = xml.IndexOf("?>", StringComparison.Ordinal);
            if (end >= 0)
            {
                xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + xml[(end + 2)..];
            }
        }

        return xml.EndsWith('\n') ? xml : xml + "\n";
    }

    private static string? NormalizeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Whitespace.Replace(value.Trim(), " ");
    }

    private static string? NormalizeFirstPublished(string? value)
    {
        var normalized = NormalizeText(value);
        if (normalized == null)
        {
            return null;
        }

        return FirstPublishedYear.IsMatch(normalized) || FirstPublishedDate.IsMatch(normalized)
            ? normalized
            : null;
    }

    /// <summary>
    ///     Babel requires <c>&lt;release&gt;/&lt;version&gt;</c> to be a non-negative integer.
    ///     Quest stores that as <c>versioncode</c>; the freeform display <c>version</c> string
    ///     is not used here.
    /// </summary>
    private static string? FormatReleaseVersion(Fields game)
    {
        if (!game.HasType<int>("versioncode"))
        {
            return null;
        }

        var code = game.GetAsType<int>("versioncode");
        return code < 0 ? null : code.ToString(CultureInfo.InvariantCulture);
    }

    private static string? NormalizeLanguage(string? languageId)
    {
        return NormalizeText(languageId);
    }

    /// <summary>
    ///     Convert Quest richtext HTML into Babel plain text with literal <c>&lt;br/&gt;</c>
    ///     paragraph breaks. Returns null when nothing usable remains.
    /// </summary>
    private static string? ToIFictionDescription(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return null;
        }

        var text = ClosePTag.Replace(html, "\n");
        text = OpenPTag.Replace(text, "");
        text = BrTag.Replace(text, "\n");
        text = HtmlTag.Replace(text, "");
        text = WebUtility.HtmlDecode(text);

        var parts = text.Split('\n')
            .Select(part => Whitespace.Replace(part, " ").Trim())
            .Where(part => part.Length > 0)
            .ToArray();

        if (parts.Length == 0)
        {
            return null;
        }

        return string.Join("<br/>", parts);
    }

    private static void WriteDescription(XmlWriter writer, string description)
    {
        writer.WriteStartElement("description");
        var parts = description.Split("<br/>");
        for (var i = 0; i < parts.Length; i++)
        {
            if (i > 0)
            {
                writer.WriteRaw("<br/>");
            }

            writer.WriteString(parts[i]);
        }

        writer.WriteEndElement();
    }

    private static CoverInfo? TryGetCoverInfo(string? coverName,
        IEnumerable<WorldModel.PackageIncludeFile>? includeFiles)
    {
        if (string.IsNullOrWhiteSpace(coverName) || includeFiles == null)
        {
            return null;
        }

        var file = includeFiles.FirstOrDefault(f =>
            string.Equals(f.Filename, coverName, StringComparison.OrdinalIgnoreCase));
        if (file?.Content == null || !file.Content.CanSeek)
        {
            // Non-seekable streams can't be rewound after we peek at the header;
            // omit cover metadata rather than truncate the image in WriteZip.
            return null;
        }

        var format = CoverFormatFromFilename(coverName);
        if (format == null)
        {
            return null;
        }

        var position = file.Content.Position;
        try
        {
            file.Content.Position = 0;

            if (!TryReadImageSize(file.Content, format, out var width, out var height))
            {
                return null;
            }

            return new CoverInfo(format, width, height);
        }
        finally
        {
            file.Content.Position = position;
        }
    }

    private static string? CoverFormatFromFilename(string filename)
    {
        var ext = Path.GetExtension(filename).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "jpg",
            ".png" => "png",
            _ => null
        };
    }

    private static bool TryReadImageSize(Stream stream, string format, out int width, out int height)
    {
        width = 0;
        height = 0;
        return format switch
        {
            "png" => TryReadPngSize(stream, out width, out height),
            "jpg" => TryReadJpegSize(stream, out width, out height),
            _ => false
        };
    }

    private static bool TryReadPngSize(Stream stream, out int width, out int height)
    {
        width = 0;
        height = 0;
        Span<byte> header = stackalloc byte[24];
        if (stream.ReadAtLeast(header, 24, throwOnEndOfStream: false) < 24)
        {
            return false;
        }

        // PNG signature + IHDR length/type
        ReadOnlySpan<byte> signature = [137, 80, 78, 71, 13, 10, 26, 10];
        if (!header[..8].SequenceEqual(signature) ||
            header[12] != (byte)'I' || header[13] != (byte)'H' ||
            header[14] != (byte)'D' || header[15] != (byte)'R')
        {
            return false;
        }

        width = (header[16] << 24) | (header[17] << 16) | (header[18] << 8) | header[19];
        height = (header[20] << 24) | (header[21] << 16) | (header[22] << 8) | header[23];
        return width > 0 && height > 0;
    }

    private static bool TryReadJpegSize(Stream stream, out int width, out int height)
    {
        width = 0;
        height = 0;
        if (stream.ReadByte() != 0xFF || stream.ReadByte() != 0xD8)
        {
            return false;
        }

        while (true)
        {
            int b;
            do
            {
                b = stream.ReadByte();
                if (b < 0)
                {
                    return false;
                }
            } while (b != 0xFF);

            do
            {
                b = stream.ReadByte();
                if (b < 0)
                {
                    return false;
                }
            } while (b == 0xFF);

            // SOF0..SOF3, SOF5..SOF7, SOF9..SOF11, SOF13..SOF15 (skip DHT/JPG/DAC)
            if ((b & 0xF0) == 0xC0 && b != 0xC4 && b != 0xC8 && b != 0xCC)
            {
                if (stream.ReadByte() < 0 || stream.ReadByte() < 0 || stream.ReadByte() < 0)
                {
                    return false;
                }

                var h1 = stream.ReadByte();
                var h2 = stream.ReadByte();
                var w1 = stream.ReadByte();
                var w2 = stream.ReadByte();
                if (h1 < 0 || h2 < 0 || w1 < 0 || w2 < 0)
                {
                    return false;
                }

                height = (h1 << 8) | h2;
                width = (w1 << 8) | w2;
                return width > 0 && height > 0;
            }

            if (b is 0xD8 or 0xD9)
            {
                return false;
            }

            var l1 = stream.ReadByte();
            var l2 = stream.ReadByte();
            if (l1 < 0 || l2 < 0)
            {
                return false;
            }

            var length = ((l1 << 8) | l2) - 2;
            if (length < 0)
            {
                return false;
            }

            stream.Position += length;
        }
    }

    private sealed record CoverInfo(string Format, int Width, int Height);
}
