namespace QuestViva.EditorCore;

// The fonts the editor offers in its font dropdowns (a control's <source>basefonts</source>):
// the fonts built into the player, then fonts most computers have. The Appearance tab's own font
// settings use AppShell's font picker instead, which also offers the game's own fonts.
internal class FontsManager
{
    // Each the same font list as AppShell's lib/fonts.ts BUILT_IN_FONTS. The player has these
    // fonts built in (WasmPlayer's wwwroot/fonts), so they look the same on every computer.
    private readonly List<string> _basefonts =
    [
        "Literata, Georgia, serif",
        "Nunito, 'Trebuchet MS', sans-serif",
        "'Courier Prime', 'Courier New', Courier, monospace",
        "'Special Elite', 'Courier New', Courier, monospace",
        "VT323, 'Lucida Console', Monaco, monospace",
        "Georgia, serif",
        "'Palatino Linotype', 'Book Antiqua', Palatino, serif",
        "'Times New Roman', Times, serif",
        "Arial, Helvetica, sans-serif",
        "'Arial Black', Gadget, sans-serif",
        "'Comic Sans MS', cursive, sans-serif",
        "Impact, Charcoal, sans-serif",
        "'Lucida Sans Unicode', 'Lucida Grande', sans-serif",
        "Tahoma, Geneva, sans-serif",
        "'Trebuchet MS', Helvetica, sans-serif",
        "Verdana, Geneva, sans-serif",
        "'Courier New', Courier, monospace",
        "'Lucida Console', Monaco, monospace"
    ];

    public List<string> GetBaseFonts() => _basefonts;

    // Google Fonts' names, for a SetWebFontName script a game already has. The list isn't fetched
    // any more, so nothing asks Google for anything: the font's name is typed in.
    public List<string> GetWebFonts() => [string.Empty];
}
