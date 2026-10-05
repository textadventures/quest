using System.IO.Compression;
using System.Text;
using QuestViva.Engine;
using QuestViva.PlayerCore;

namespace QuestViva.Cli;

// A file put in the package beside the game, with its size in bytes.
internal sealed record PublishedFile(string Name, long Size);

// What publishing did. Output is null when it refused, because the game has problems: then
// Problems are what `qv check` found. Warnings don't stop a publish.
internal sealed record PublishResult(
    string? Output,
    IReadOnlyList<PublishedFile> Files,
    IReadOnlyList<string> Warnings,
    GameCheckResult? Problems);

// What qv publish makes, as the Editor's Publish dialog offers: a .quest package; one HTML file
// with the package embedded, which loads the player from jsDelivr; or a zip of that page beside a
// copy of the player, for any web host.
internal enum PublishFormat
{
    Quest,
    Html,
    Zip
}

// Publishes an .aslx game as the Editor's Publish does: a .quest package of the game with its
// included libraries inlined (Packager), plus the files in its folder, and for the HTML formats
// that package embedded in WasmPlayer's page (PlayerExport).
//
// Which files: like the Editor, every file at the top of the game's folder except .aslx and .xml
// files (the game and its libraries, which are inlined) and macOS junk. Unlike the Editor, it
// also leaves out hidden files and earlier publish output - any .quest file, and the game's own
// .html, .zip (the Editor's backup) and -html.zip - since a folder an agent works in tends to
// collect them. The result lists the files, so the author can see what went in.
internal static class GamePublisher
{
    // player is the WasmPlayer bundle the HTML formats take the page from, and the zip the player.
    // The html format pins the CDN's player to cdnVersion, which must be a published version.
    public static async Task<PublishResult> PublishAsync(FileInfo game, FileInfo? output, bool force,
        PublishFormat format = PublishFormat.Quest, PlayerBundle? player = null, string? cdnVersion = null)
    {
        if (format != PublishFormat.Quest && player == null)
        {
            throw new ArgumentNullException(nameof(player), "The HTML formats need the player.");
        }

        if (format == PublishFormat.Html && cdnVersion == null)
        {
            throw new ArgumentNullException(nameof(cdnVersion), "The html format needs a version to pin the player to.");
        }

        if (!game.Exists)
        {
            throw new FileNotFoundException($"File not found: {game.FullName}");
        }

        if (!game.Extension.Equals(".aslx", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException(game.Extension.Equals(".quest", StringComparison.OrdinalIgnoreCase)
                ? "A .quest file is already published. Publish the .aslx file it was made from."
                : "Only an .aslx game can be published.");
        }

        var check = await GameCheck.CheckAsync(GameFiles.Load(game));
        if (!check.Loaded || (check.Problems.Count > 0 && !force))
        {
            return new PublishResult(null, [], [], check);
        }

        var world = new WorldModel(GameFiles.Load(game), null);
        if (!await world.InitialiseEdit())
        {
            // GameCheck loaded it, so this would only happen if the file changed in between.
            throw new InvalidOperationException("The game couldn't be loaded: " + string.Join(" ", world.Errors));
        }

        var baseName = Path.GetFileNameWithoutExtension(game.Name);
        var outputFile = output ?? new FileInfo(Path.Combine(game.DirectoryName!, baseName + format switch
        {
            PublishFormat.Html => ".html",
            PublishFormat.Zip => "-html.zip",
            _ => ".quest"
        }));
        var folder = game.Directory!;
        var files = folder.EnumerateFiles()
            .Where(f => Included(f, baseName, outputFile))
            .OrderBy(f => f.Name, StringComparer.Ordinal)
            .ToList();

        var warnings = new List<string>();
        if (check.Problems.Count > 0)
        {
            warnings.Add(check.Problems.Count == 1
                ? "Published anyway, with 1 problem that check found."
                : $"Published anyway, with {check.Problems.Count} problems that check found.");
        }

        var included = files.Select(f => f.Name).ToHashSet(StringComparer.Ordinal);
        foreach (var name in GameFileReferences.Find(world).Where(n => !included.Contains(n)).Order(StringComparer.Ordinal))
        {
            warnings.Add(File.Exists(Path.Combine(folder.FullName, name))
                ? $"The game refers to '{name}', which is left out of the package."
                : $"The game refers to '{name}', which isn't in the game's folder.");
        }

        if (string.IsNullOrWhiteSpace(world.Game.Fields.GetString("gameid")))
        {
            // The packager makes one up, but it isn't saved back to the .aslx, so the next publish
            // would give the game a different IFID. Set it here so it can be reported.
            var gameId = Guid.NewGuid().ToString();
            world.Game.Fields.Set("gameid", gameId);
            warnings.Add($"The game has no gameid, so this package was given a new one, {gameId}. " +
                         $"Add <gameid>{gameId}</gameid> to the game element so the next publish keeps it: " +
                         "it's the game's IFID, which identifies it on sites such as IFDB.");
        }

        var streams = files.Select(f => new WorldModel.PackageIncludeFile
        {
            Filename = f.Name,
            Content = f.OpenRead()
        }).ToList();
        byte[] package;
        try
        {
            using var packageStream = new MemoryStream();
            if (!world.CreatePackage(null, out var error, streams, packageStream))
            {
                throw new InvalidOperationException("The package couldn't be created: " + error);
            }

            package = packageStream.ToArray();
        }
        finally
        {
            foreach (var stream in streams)
            {
                stream.Content.Dispose();
            }
        }

        var bytes = format switch
        {
            PublishFormat.Html => Encoding.UTF8.GetBytes(PlayerExport.BuildHtml(Template(player!),
                ExportedGame(world, package, baseName, null), PlayerExport.CdnBase(cdnVersion!))),
            PublishFormat.Zip => PlayerZip(world, package, baseName, player!, files),
            _ => package
        };

        // Written to a temporary file first, so a failure leaves any earlier output as it was.
        var temp = outputFile.FullName + ".tmp";
        await File.WriteAllBytesAsync(temp, bytes);
        File.Move(temp, outputFile.FullName, overwrite: true);

        return new PublishResult(outputFile.FullName, files.Select(f => new PublishedFile(f.Name, f.Length)).ToList(),
            warnings, check.Problems.Count > 0 ? check : null);
    }

    private static ExportedGame ExportedGame(WorldModel world, byte[] package, string baseName, string? coverUrl) =>
        new(package, baseName + ".quest",
            string.IsNullOrWhiteSpace(world.GameName) ? baseName : world.GameName.Trim(),
            PlayerExport.PlainText(world.Description),
            world.GameID,
            coverUrl);

    private static string Template(PlayerBundle player) =>
        Encoding.UTF8.GetString(player.Read("index.html")
                                ?? throw new InvalidOperationException("The player has no index.html."));

    // The page and the player files it needs, plus the game's cover art as cover.<ext>, under a
    // name that can't clash with a player file, for link previews to show. The game's own copy of
    // the cover stays in the package, which is what the player uses.
    private static byte[] PlayerZip(WorldModel world, byte[] package, string baseName, PlayerBundle player,
        List<FileInfo> folderFiles)
    {
        var manifest = player.Read("export-manifest.json")
                       ?? throw new InvalidOperationException("The player has no export-manifest.json.");
        var boot = player.Read("_framework/dotnet.js")
                   ?? throw new InvalidOperationException("The player has no _framework/dotnet.js.");

        var cover = world.Cover?.Trim() is { Length: > 0 } coverName
            ? folderFiles.FirstOrDefault(f => f.Name == coverName)
            : null;
        var coverPath = cover == null ? null : "cover" + cover.Extension.ToLowerInvariant();

        using var zipStream = new MemoryStream();
        var added = new HashSet<string>(StringComparer.Ordinal);
        using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var path in PlayerExport.PlayerFiles(Encoding.UTF8.GetString(manifest), Encoding.UTF8.GetString(boot)))
            {
                if (path == "index.html")
                {
                    continue;
                }

                // Every shell file must be there. A runtime file the boot manifest names that the
                // player doesn't have is left out, as the Editor's zip export does.
                var file = player.Read(path);
                if (file == null)
                {
                    if (path.StartsWith("_framework/", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    throw new InvalidOperationException($"The player has no {path}.");
                }

                Add(path, file);
            }

            Add("index.html", Encoding.UTF8.GetBytes(PlayerExport.BuildHtml(Template(player),
                ExportedGame(world, package, baseName, coverPath), null)));

            if (cover != null)
            {
                Add(coverPath!, File.ReadAllBytes(cover.FullName));
            }

            void Add(string path, byte[] content)
            {
                if (!added.Add(path))
                {
                    throw new InvalidOperationException($"{path} is in the zip twice.");
                }

                using var entry = zip.CreateEntry(path, CompressionLevel.Optimal).Open();
                entry.Write(content);
            }
        }

        return zipStream.ToArray();
    }

    private static bool Included(FileInfo file, string baseName, FileInfo output)
    {
        var name = file.Name;
        var lower = name.ToLowerInvariant();
        var baseLower = baseName.ToLowerInvariant();
        return !lower.EndsWith(".aslx") && !lower.EndsWith(".xml") &&
               !name.StartsWith('.') &&
               !lower.EndsWith(".quest") && !lower.EndsWith(".tmp") &&
               lower != baseLower + ".html" && lower != baseLower + ".zip" && lower != baseLower + "-html.zip" &&
               !string.Equals(file.FullName, output.FullName, StringComparison.Ordinal);
    }
}
