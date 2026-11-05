namespace QuestViva.EditorCore;

// How a theme looks, for the editor's Theme chooser (EditorController.GetDropdownTypePreviews):
// its web font and base font, and its light and dark page, text and link colours. A null colour is
// the player's own, which follows their light or dark mode, and a null dark background means the
// theme has no dark colours of its own: with a light background too, it's always shown that way.
public record DropdownTypePreview(
    string Type,
    string? WebFont,
    string? Font,
    string? Background,
    string? Text,
    string? Link,
    string? DarkBackground,
    string? DarkText,
    string? DarkLink);
