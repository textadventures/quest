using QuestViva.Engine;

namespace QuestViva.PlayerCore;

// Player-chrome UI strings (the Save/Load dialog, and labels in the player UI itself) sourced from the game's own
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
        ["SaveNameOptional"] = "Name (optional)",
        ["SaveButton"] = "Save",
        ["SaveToFile"] = "Save to file",
        ["LoadFromFile"] = "Load from file",
        ["CopyBugReport"] = "Copy for a bug report",
        ["BugReportCopied"] = "Copied. Paste it into your bug report.",
        ["BugReportCopyFailed"] = "Copy this into your bug report:",
        ["UnnamedSave"] = "this save",
        ["SaveWrongGame"] = "This save is for a different game — open that game first, then try again.",
        ["CloseDialog"] = "Close",
        ["SavingBusy"] = "Saving…",
        ["RestartingBusy"] = "Restarting…",
        ["DeleteSaveConfirm"] = "Delete \"[name]\"? This can't be undone.",
        ["SavedGameDefaultLabel"] = "Saved game — [timestamp]",
        ["ContinueGame"] = "Continue",
        ["OtherSaves"] = "Other saves",
        ["LoadSave"] = "Load",
        ["SaveOptions"] = "Options for [name]",
        ["OverwriteSave"] = "Overwrite",
        ["RenameSave"] = "Rename",
        ["RenameSaveLabel"] = "New name",
        ["StartAgain"] = "Start again",
        ["SavedToday"] = "Today, [time]",
        ["SavedYesterday"] = "Yesterday, [time]",
        ["SavedGameTitle"] = "Saved game",
        ["LoadSaveConfirm"] = "Load \"[name]\"? You'll lose anything you've done since you last saved.",
        ["StartAgainConfirm"] = "Start again from the beginning? You'll lose anything you've done since you last saved.",
        ["OverwriteSaveConfirm"] = "Overwrite \"[name]\"?",
        ["CancelButton"] = "Cancel",
        ["CommandInputLabel"] = "Type your command",
        ["ShowPanesButton"] = "Show panels",
        ["MapLabel"] = "Map",
        ["MapZoomIn"] = "Zoom in",
        ["MapZoomOut"] = "Zoom out",
        ["ColourSchemeButton"] = "Light or dark",
        ["ColourSchemeSystem"] = "Match my device",
        ["ColourSchemeLight"] = "Light",
        ["ColourSchemeDark"] = "Dark",
        ["YouTubeVideoTitle"] = "YouTube video",
        ["VimeoVideoTitle"] = "Vimeo video",
        ["GameSaved"] = "Game saved.",
        // Core's own template, which games already have in every language, standing in for
        // QuestUI.setInterfaceString("CompassLabel") until the game sets it.
        ["CompassLabel"] = "Compass",
    };

    // Keys that replaced one whose English wording no longer fits the dialog: the new key, the old
    // one, and the old key's English text. A game published before the change carries the old key
    // in its inlined Core templates and not the new one. Its old text is used only when it's a
    // translation (or the game's own wording), so an English game gets the new wording, and a game
    // in another language doesn't get English in the middle of its own.
    private static readonly (string Key, string OldKey, string OldEnglish)[] Superseded =
    [
        ("SaveNameOptional", "SaveNamePlaceholder", "Save name"),
        ("SaveButton", "SaveNew", "Save new"),
    ];

    public static Dictionary<string, string> Resolve(WorldModel? worldModel) =>
        worldModel == null ? new Dictionary<string, string>(Defaults) : ResolveTemplates(worldModel.GetTemplateText);

    // getTemplate gives the game's template text, or null when it has none.
    public static Dictionary<string, string> ResolveTemplates(Func<string, string?> getTemplate)
    {
        var result = new Dictionary<string, string>(Defaults);

        foreach (var key in Defaults.Keys)
        {
            var value = getTemplate(key);
            if (value != null) result[key] = value;
        }

        foreach (var (key, oldKey, oldEnglish) in Superseded)
        {
            if (getTemplate(key) != null) continue;
            var old = getTemplate(oldKey);
            if (old != null && old != oldEnglish) result[key] = old;
        }

        return result;
    }
}
