using QuestViva.PlayerCore;

namespace QuestViva.Cli;

// `qv publish`: makes a .quest package from an .aslx game, as the Editor's Publish does. Checks the
// game first and refuses if `qv check` finds problems, unless forced. Exits 0 when it publishes, 1
// when it refuses, and 2 when it can't publish the game at all.
internal static class PublishCommand
{
    public static async Task<int> RunAsync(FileInfo game, string format, FileInfo? output, bool force)
    {
        if (format != "quest")
        {
            await Console.Error.WriteLineAsync(
                $"Unknown format '{format}'. Only --format quest is supported so far.");
            return 2;
        }

        PublishResult result;
        try
        {
            result = await GamePublisher.PublishAsync(game, output, force);
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
