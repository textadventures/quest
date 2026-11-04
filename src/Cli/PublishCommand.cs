using QuestViva.PlayerCore;

namespace QuestViva.Cli;

// `qv publish`: makes a .quest package from an .aslx game, as the Editor's Publish does. Checks the
// game first and refuses if `qv check` finds problems, unless forced. Exits 0 when it publishes, 1
// when it refuses, and 2 when it can't publish the game at all.
internal static class PublishCommand
{
    public static async Task<int> RunAsync(FileInfo game, string format, FileInfo? output, bool force,
        DirectoryInfo? playerFolder, string? cdnVersion)
    {
        PublishResult result;
        try
        {
            result = await PublishAsync(game, format, output, force, playerFolder, cdnVersion);
        }
        catch (Exception ex) when (ex is FileNotFoundException or NotSupportedException or InvalidOperationException
                                       or IOException or UnauthorizedAccessException)
        {
            await Console.Error.WriteLineAsync(ex.Message);
            return 2;
        }

        Console.Write(Format(result));
        return result.Output == null ? 1 : 0;
    }

    // Publishes in the format named, finding the player the HTML formats need as qv preview does.
    public static Task<PublishResult> PublishAsync(FileInfo game, string format, FileInfo? output, bool force,
        DirectoryInfo? playerFolder, string? cdnVersion = null)
    {
        var publishFormat = format.ToLowerInvariant() switch
        {
            "quest" => PublishFormat.Quest,
            "html" => PublishFormat.Html,
            "zip" => PublishFormat.Zip,
            _ => throw new NotSupportedException($"Unknown format '{format}'. The formats are quest, html and zip.")
        };
        if (publishFormat == PublishFormat.Quest)
        {
            return GamePublisher.PublishAsync(game, output, force);
        }

        var player = PreviewCommand.FindPlayer(playerFolder);
        if (publishFormat == PublishFormat.Html && cdnVersion == null)
        {
            // The page loads the player from jsDelivr, so it needs a version that's been published
            // there, which is what a released qv is. One built from a checkout says the version it
            // was built after, whose player can be older than its engine.
            if (!player.BuiltIn)
            {
                throw new NotSupportedException(
                    "--format html loads the player from jsDelivr at qv's own version, so it needs a released " +
                    "qv. This one was built from a checkout. Use --format zip, which includes the player, or " +
                    "name a published player version with --cdn-version.");
            }

            cdnVersion = Version;
        }

        return GamePublisher.PublishAsync(game, output, force, publishFormat, player,
            publishFormat == PublishFormat.Html ? cdnVersion : null);
    }

    public static string Version =>
        typeof(PublishCommand).Assembly
            .GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
            .OfType<System.Reflection.AssemblyInformationalVersionAttribute>().FirstOrDefault()?.InformationalVersion
        ?? "";

    // force names the option, as the command line or the MCP tool spells it.
    public static string Format(PublishResult result, string force = "pass --force")
    {
        var text = new System.Text.StringBuilder();
        if (result.Output == null)
        {
            var check = result.Problems!;
            text.Append(CheckCommand.Format(check));
            text.Append(check.Loaded
                ? $"Not published. Fix these problems first, or {force} to publish anyway.\n"
                : "Not published.\n");
            return text.ToString();
        }

        text.Append("Published ").Append(result.Output).Append('\n');
        if (result.Files.Count == 0)
        {
            text.Append("No other files from the game's folder are in it.\n");
        }
        else
        {
            text.Append(result.Files.Count == 1 ? "With 1 file from the game's folder:\n" : $"With {result.Files.Count} files from the game's folder:\n");
            foreach (var file in result.Files)
            {
                text.Append("  ").Append(file.Name).Append(" (").Append(Size(file.Size)).Append(")\n");
            }
        }

        foreach (var warning in result.Warnings)
        {
            text.Append("Warning: ").Append(warning).Append('\n');
        }

        return text.ToString();
    }

    private static string Size(long bytes) => bytes switch
    {
        1 => "1 byte",
        < 1024 => $"{bytes} bytes",
        < 1024 * 1024 => $"{bytes / 1024.0:0.#} KB",
        _ => $"{bytes / (1024.0 * 1024):0.#} MB"
    };
}
