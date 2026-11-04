using QuestViva.Common;
using QuestViva.Engine;

namespace QuestViva.PlayerCore;

// What checking a game found. When the game didn't load, Problems are the loader's errors, and its
// scripts weren't checked.
public sealed record GameCheckResult(IReadOnlyList<CheckProblem> Problems, bool Loaded);

// Loads a game without starting it and reports its problems: the loader's errors if it doesn't
// load, and otherwise what GameChecker finds in its scripts. Quest 5 and later only.
public static class GameCheck
{
    private static readonly string[] Quest4Extensions = [".asl", ".cas", ".qsg"];

    public static async Task<GameCheckResult> CheckAsync(GameData gameData)
    {
        if (Quest4Extensions.Contains(Path.GetExtension(gameData.Filename).ToLowerInvariant()))
        {
            throw new NotSupportedException("Checking only works for Quest 5 and later games (.aslx and .quest files).");
        }

        // Loaded the way the Editor loads it: a play-mode load stops at the first script it can't
        // create, and the checker wants all of them.
        var world = new WorldModel(gameData, null);
        bool loaded;
        try
        {
            loaded = await world.InitialiseEdit();
        }
        catch (Exception ex)
        {
            // A file that isn't a game at all, such as a corrupt .quest package.
            return new GameCheckResult([new CheckProblem($"The game couldn't be loaded: {ex.Message}")], false);
        }

        if (!loaded)
        {
            // The loader stops resolving the game at its first error, so its scripts can't be
            // checked until that's fixed.
            return new GameCheckResult(world.Errors
                .Select(e => new CheckProblem(e.StartsWith("Error: ") ? e[7..] : e))
                .ToList(), false);
        }

        return new GameCheckResult(GameChecker.Check(world), true);
    }
}
