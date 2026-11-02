using System.Text.RegularExpressions;

namespace QuestViva.EditorCore;

// A place where a game's own code relies on the legacy player UI, which doesn't work on v2.
// Source is the element, or the .js file, it's in. Detail is the code itself, shortened to fit
// in the editor's banner.
public record LegacyUiHack(string Source, string Detail);

// Finds a game's own use of the legacy player UI, so the editor can offer the classic interface.
// It's ported from the published-games survey's classifier (docs/research/player-ui-surface-survey),
// narrowed to what v2 doesn't provide. v2 has no jQuery and none of legacy's element IDs, and of
// legacy's global functions it keeps only sendCommand and ASLEvent, so a JS.* call to any other
// one fails. It works on text alone: EditorController chooses which code to pass in, and leaves
// out library elements, so stock Core can't trigger it.
public static partial class LegacyUiDetector
{
    private const int MaxCallLength = 2000;
    private const int MaxDetailLength = 80;

    // The global functions of legacy's scripts that a game could mean to call.
    private static readonly HashSet<string> LegacyFunctions =
    [
        "AddVimeo", "AddYouTube", "EndOutputSection", "Grid_AddNewShapePoint", "Grid_ClearAllLayers",
        "Grid_ClearCustomLayer", "Grid_DrawArrow", "Grid_DrawBox", "Grid_DrawGridLines", "Grid_DrawImage",
        "Grid_DrawLabel", "Grid_DrawLine", "Grid_DrawPlayer", "Grid_DrawShape", "Grid_DrawSquare", "Grid_DrawSvg",
        "Grid_LoadSvg", "Grid_SetCentre", "Grid_SetScale", "Grid_ShowCustomLayer", "HideOutputSection",
        "SaveTranscript", "SetAnimateScroll", "SetBackgroundImage", "SetBackgroundOpacity", "SetMenuBackground",
        "SetMenuFontName", "SetMenuFontSize", "SetMenuForeground", "SetMenuHoverBackground",
        "SetMenuHoverForeground", "ShowGrid", "StartOutputSection", "TurnOffHyperlinksUnderline", "WriteToLog",
        "WriteToTranscript", "addCSSRule", "addLogEntry", "addScript", "addText", "addTextAndScroll", "beginPause",
        "beginWait", "bindMenu", "clearScreen", "colourBlend", "commandKey", "compassClick", "createNewDiv",
        "disableAllCommandLinks", "disableInterface", "disableMainScrollbar", "disableMenuOutputSection",
        "disableTranscript", "enableTranscript", "endPause", "endWait", "focusCommandInput", "gameFinished",
        "getCSSRule", "getCurrentDiv", "getDivCount", "goUrl", "hideBorder", "initPlayerUI", "isElementVisible",
        "isMobilePlayer", "isNarrowLayout", "keyPressCode", "killCSSRule", "killTranscript", "loadHtml",
        "markScrollPosition", "msgboxSubmit", "openMsgbox", "paneButtonClick", "printTranscriptDiv",
        "replaceTranscriptString", "requestNextTimerTick", "resetGameOutput", "respondToGridClicks",
        "reviveTranscript", "runCommand", "saveGame", "scrollToEnd", "scrollToTurnStart", "setBackground",
        "setCommandBarStyle", "setCommands", "setCompass", "setCompassDirections", "setCss", "setCurrentDiv",
        "setCustomStatus", "setDivCount", "setElement", "setForeground", "setGameName", "setGamePadding",
        "setInterfaceString", "setOutputBufferId", "setPanelContents", "setPanes", "setTranscriptStatus",
        "showCustomLayer", "showMenu", "showPopup", "showPopupCustomSize", "showPopupFullscreen", "showQuestion",
        "showScrollback", "showStatusVisible", "showTranscript", "startTimer", "stopTimer", "timerTick", "uiHide",
        "uiShow", "updateCommandLinks", "updateCompass", "updateDir", "updateExitLinks", "updateExitStrip",
        "updateList", "updateLocation", "updateObjectLinks", "updateStatus", "updateStatusVisibility",
        "updateVerbButtons", "whereAmI", "writeToTranscript"
    ];

    [GeneratedRegex(@"\bJS\.([A-Za-z_]\w*)\s*\(")]
    private static partial Regex JsCall();

    [GeneratedRegex(@"""((?:[^""\\]|\\.)*)""")]
    private static partial Regex AslString();

    // A call to a global, not a method: "text".endsWith(…) isn't legacy's endsWith.
    [GeneratedRegex(@"(?<![\w.$])([A-Za-z_]\w*)\s*\(")]
    private static partial Regex GlobalCall();

    [GeneratedRegex(@"(?<![\w.$])(?:\$|jQuery)\s*\(")]
    private static partial Regex JQueryCall();

    // A function the game defines for itself, which can share a name with one of legacy's.
    [GeneratedRegex(@"\bfunction\s+([A-Za-z_]\w*)\s*\(|(?<![\w.$])(?:window\.)?([A-Za-z_]\w*)\s*=\s*function\b")]
    private static partial Regex OwnFunction();

    // The globals the survey found games replacing, which a game can't have meant to define
    // for itself.
    [GeneratedRegex(@"(?:\bfunction\s+|(?<![\w.$])(?:window\.)?)(?:addText|addTextAndScroll|beginWait|endWait|scrollToEnd|sendCommand|setCustomStatus|showStatusVisible|updateList|updateStatus|updateStatusVisibility|updateVerbButtons)\s*(?:\(\s*[\w\s,]*\)\s*\{|=\s*(?:function\b|\())")]
    private static partial Regex GlobalOverride();

    [GeneratedRegex(@"(?<![\w.$])(?:_animateScroll|_waitMode|_pauseMode|gridApi)\b")]
    private static partial Regex Internal();

    // A lookup of one of legacy's own elements (including their Quest 5 names) or classes.
    [GeneratedRegex(@"(?:\$|jQuery|querySelector(?:All)?|getElementById)\s*\(\s*\\?['""]#?(?:cmdCompass\w*|cmdInventory\d|cmdPlacesObjects\d|commandPane|commandPaneHeading|compassAccordion|compassLabel|compassTable|controlButtons|customStatusPane|divOutput|endWaitDiv|endWaitLink|gameBorder|gameContent|gamePanel|gamePanelSpacer|gamePanes|gamePanesFinished|gamePanesRunning|gameTitle|gridCanvas|gridPanel|inventoryAccordion|inventoryLabel|inventoryWrapper|location|lstInventory|lstPlacesObjects|outputData|placesObjectsAccordion|placesObjectsButtons|placesObjectsLabel|placesObjectsWrapper|status|statusVars|statusVarsAccordion|statusVarsLabel|txtCommand|txtCommandContainer|txtCommandDiv|txtCommandPrompt|txtCommandSpacer|\.elementListWrapper|\.ui-[\w-]+)\b")]
    private static partial Regex LegacyElement();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    // scripts: each script of the game's own, as ASL. javascriptFiles: the text of each .js file
    // the game itself includes. A hack can show up in several forms, so each call counts once.
    public static IReadOnlyList<LegacyUiHack> Find(
        IEnumerable<(string Source, string Script)> scripts,
        IEnumerable<(string Filename, string Text)> javascriptFiles)
    {
        var scriptList = scripts.ToList();
        var fileList = javascriptFiles.ToList();

        var calls = scriptList
            .SelectMany(script => JsCall().Matches(script.Script).Select(call => (
                script.Source,
                Name: call.Groups[1].Value,
                Text: CallText(script.Script, call.Index, call.Length)))
            ).ToList();
        var evaluated = calls
            .Where(call => call.Name == "eval")
            .Select(call => (call.Source, Call: call.Text, JavaScript: StringsIn(call.Text)))
            .ToList();

        var ownFunctions = fileList.Select(file => file.Text)
            .Concat(evaluated.Select(e => e.JavaScript))
            .SelectMany(OwnFunctions)
            .ToHashSet();

        var hacks = new List<LegacyUiHack>();
        foreach (var (source, name, text) in calls)
        {
            if (LegacyFunctions.Contains(name) && !ownFunctions.Contains(name))
            {
                hacks.Add(new LegacyUiHack(source, Shorten(text)));
            }
        }

        foreach (var (source, call, javaScript) in evaluated)
        {
            if (FirstHack(javaScript) != null || JQueryCall().IsMatch(javaScript) || CallsLegacyFunction(javaScript, ownFunctions))
            {
                hacks.Add(new LegacyUiHack(source, Shorten(call)));
            }
        }

        foreach (var (filename, text) in fileList)
        {
            var match = FirstHack(text);
            if (match != null)
            {
                hacks.Add(new LegacyUiHack(filename, Shorten(LineAt(text, match.Index))));
            }
        }

        return hacks.Distinct().ToList();
    }

    private static Match? FirstHack(string javaScript)
    {
        return new[] { GlobalOverride(), Internal(), LegacyElement() }
            .Select(regex => regex.Match(javaScript))
            .Where(match => match.Success)
            .MinBy(match => match.Index);
    }

    private static bool CallsLegacyFunction(string javaScript, HashSet<string> ownFunctions)
    {
        return GlobalCall().Matches(javaScript)
            .Select(call => call.Groups[1].Value)
            .Any(name => LegacyFunctions.Contains(name) && !ownFunctions.Contains(name));
    }

    private static IEnumerable<string> OwnFunctions(string javaScript)
    {
        return OwnFunction().Matches(javaScript)
            .Select(match => match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value);
    }

    // The JavaScript inside a JS.* call's string arguments, with its ASL escapes removed. The
    // pieces of a concatenation are joined, so "$('#" + id + "')" still reads as a jQuery call.
    private static string StringsIn(string call)
    {
        return string.Concat(AslString().Matches(call).Select(s => s.Groups[1].Value.Replace("\\\"", "\"")));
    }

    // A call from its first character to its closing bracket, or as much of it as there is.
    private static string CallText(string text, int start, int headerLength)
    {
        var depth = 1;
        var inString = false;
        var end = Math.Min(text.Length, start + MaxCallLength);
        for (var i = start + headerLength; i < end; i++)
        {
            var c = text[i];
            if (inString)
            {
                if (c == '\\')
                {
                    i++;
                }
                else if (c == '"')
                {
                    inString = false;
                }
            }
            else if (c == '"')
            {
                inString = true;
            }
            else if (c == '(')
            {
                depth++;
            }
            else if (c == ')' && --depth == 0)
            {
                return text[start..(i + 1)];
            }
        }

        return text[start..end];
    }

    private static string LineAt(string text, int index)
    {
        var start = text.LastIndexOf('\n', index) + 1;
        var end = text.IndexOf('\n', index);
        return text[start..(end < 0 ? text.Length : end)];
    }

    private static string Shorten(string code)
    {
        var singleLine = Whitespace().Replace(code, " ").Trim();
        return singleLine.Length <= MaxDetailLength ? singleLine : singleLine[..(MaxDetailLength - 1)] + "…";
    }
}
