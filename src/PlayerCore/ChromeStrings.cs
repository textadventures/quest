using QuestViva.Engine;

namespace QuestViva.PlayerCore;

// Player-chrome UI strings (the Save/Load dialog) sourced from the game's own
// per-language Core library templates (src/Engine/Core/Languages/*.aslx), so a
// translation added there is baked into every game published from then on -
// see WorldModel.GetTemplateText. Defaults here match today's hardcoded
// English markup exactly, so Resolve(null) (no game loaded yet, or a legacy
// V4 .asl/.cas game with no WorldModel/template system at all) and an old
// already-published game missing these keys both fall back to unchanged
// behaviour.
public static class ChromeStrings
{
    public static readonly IReadOnlyDictionary<string, string> Defaults = new Dictionary<string, string>
    {
        ["SaveLoadTitle"] = "Save / Load",
        ["ContinueYourGamePrompt"] = "Continue your game?",
        ["StartFromBeginning"] = "Start from the beginning",
        ["SavedGamesHeading"] = "Saved games",
        ["NoSavedGamesYet"] = "No saved games yet.",
        ["DeleteSave"] = "Delete",
        ["SaveNamePlaceholder"] = "Save name",
        ["SaveNew"] = "Save new",
        ["SaveToFile"] = "Save to file",
        ["LoadFromFile"] = "Load from file",
        ["UnnamedSave"] = "this save",
        ["SaveWrongGame"] = "This save is for a different game — open that game first, then try again.",
        ["CloseDialog"] = "Close",
        ["SavingBusy"] = "Saving…",
        ["RestartingBusy"] = "Restarting…",
        ["DeleteSaveConfirm"] = "Delete \"[name]\"? This can't be undone.",
        ["SavedGameDefaultLabel"] = "Saved game — [timestamp]",
        ["CommandInputLabel"] = "Type your command",
        ["ShowPanesButton"] = "Show panels",
    };

    public static Dictionary<string, string> Resolve(WorldModel worldModel)
    {
        var result = new Dictionary<string, string>(Defaults);
        if (worldModel == null) return result;

        foreach (var key in Defaults.Keys)
        {
            var value = worldModel.GetTemplateText(key);
            if (value != null) result[key] = value;
        }

        return result;
    }
}
