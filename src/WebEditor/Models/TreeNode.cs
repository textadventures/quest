namespace QuestViva.WebEditor.Models;

public class TreeNode
{
    public string Key { get; set; } = "";
    public string Text { get; set; } = "";
    public bool IsLibraryNode { get; set; }
    public List<TreeNode> Children { get; set; } = new();
    public TreeNode? Parent { get; set; }
}
