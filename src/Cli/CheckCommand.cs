using System.Text.Json;
using QuestViva.Engine;
using QuestViva.PlayerCore;

namespace QuestViva.Cli;

// `qv check`: reports problems in a game without playing it. Exits 1 if it finds any, and 2 if it
// can't check the game at all, so authors can run it in CI.
internal static class CheckCommand
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static async Task<int> RunAsync(FileInfo game, bool json)
    {
        GameCheckResult result;
        try
        {
            result = await GameCheck.CheckAsync(GameFiles.Load(game));
        }
        catch (Exception ex) when (ex is FileNotFoundException or NotSupportedException)
        {
            await Console.Error.WriteLineAsync(ex.Message);
            return 2;
        }

        if (json)
        {
            Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions));
        }
        else
        {
            Console.Write(Format(result));
        }

        return result.Problems.Count == 0 ? 0 : 1;
    }

    public static string Format(GameCheckResult check)
    {
        var problems = check.Problems;
        if (problems.Count == 0)
        {
            return "No problems found.\n";
        }

        var result = new System.Text.StringBuilder();
        foreach (var problem in problems)
        {
            result.Append(Location(problem)).Append(problem.Message).Append('\n');
        }

        result.Append(problems.Count == 1 ? "1 problem found." : $"{problems.Count} problems found.");
        result.Append(check.Loaded ? "\n" : " The game didn't load, so its scripts weren't checked.\n");
        return result.ToString();
    }

    // "object 'room', attribute 'description': ", with the library first if it's in one.
    private static string Location(CheckProblem problem)
    {
        var parts = new List<string>();
        if (problem.Filename != null)
        {
            parts.Add(problem.Filename);
        }

        if (problem.Kind == "game")
        {
            parts.Add("game");
        }
        else if (problem.Element != null)
        {
            parts.Add($"{problem.Kind} '{problem.Element}'");
        }

        if (problem.Attribute != null)
        {
            parts.Add($"attribute '{problem.Attribute}'");
        }

        return parts.Count == 0 ? "" : string.Join(", ", parts) + ": ";
    }
}
