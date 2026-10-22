namespace QuestViva.EditorCore;

// What an element still has set for a feature after that feature is turned off. A feature
// checkbox (e.g. "Container", or the game's "Ask/Tell") only shows or hides the feature's tab;
// the types and attributes the tab set stay on the element and keep taking effect in the game.
// Labels are the editor captions for each type and attribute, for describing them to the author.
public record FeatureLeftovers(
    string ElementKey,
    IReadOnlyList<string> Types,
    IReadOnlyList<string> Attributes,
    IReadOnlyList<string> Labels);
