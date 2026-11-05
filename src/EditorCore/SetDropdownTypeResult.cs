namespace QuestViva.EditorCore;

// What EditorController.SetDropdownType did: whether the type was set (or why not), and which of
// the element's own attributes it removed for a control with <resetoverrides/>.
public record SetDropdownTypeResult(bool Valid, string? Message, List<string> ResetAttributes);
