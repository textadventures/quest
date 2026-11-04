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

// Publishes an .aslx game as a .quest package, as the Editor's Publish does: the game with its
// included libraries inlined (Packager), plus the files in its folder.
//
// Which files: like the Editor, every file at the top of the game's folder except .aslx and .xml
// files (the game and its libraries, which are inlined) and macOS junk. Unlike the Editor, it
// also leaves out hidden files and earlier publish output - any .quest file, and the game's own
// .html, .zip (the Editor's backup) and -html.zip - since a folder an agent works in tends to
// collect them. The result lists the files, so the author can see what went in.
internal static class GamePublisher
{
    public static async Task<PublishResult> PublishAsync(FileInfo game, FileInfo? output, bool force)
    {
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
        var outputFile = output ?? new FileInfo(Path.Combine(game.DirectoryName!, baseName + ".quest"));
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
        try
        {
            // Written to a temporary file first, so a failure leaves any earlier package as it was.
            var temp = outputFile.FullName + ".tmp";
            if (!world.CreatePackage(temp, out var error, streams, null))
            {
                File.Delete(temp);
                throw new InvalidOperationException("The package couldn't be created: " + error);
            }

            File.Move(temp, outputFile.FullName, overwrite: true);
        }
        finally
        {
            foreach (var stream in streams)
            {
                stream.Content.Dispose();
            }
        }

        return new PublishResult(outputFile.FullName, files.Select(f => new PublishedFile(f.Name, f.Length)).ToList(),
            warnings, check.Problems.Count > 0 ? check : null);
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
