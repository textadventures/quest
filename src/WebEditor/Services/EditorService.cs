using QuestViva.Common;
using QuestViva.EditorCore;
using QuestViva.WebEditor.Models;

namespace QuestViva.WebEditor.Services;

public class EditorService : IDisposable
{
    private readonly IConfig _config;
    private EditorController? _controller;
    private string? _tempFilePath;
    private bool _treeUpdateInProgress;

    public List<TreeNode> TreeNodes { get; } = new();
    public string? SelectedElementKey { get; set; }
    public bool IsLoaded { get; private set; }
    public string? GameName => _controller?.GameName;
    public bool CanUndo { get; private set; }
    public bool CanRedo { get; private set; }
    public string? LastMessage { get; private set; }

    public event Action? TreeChanged;
    public event Action? UndoStateChanged;
    public event Action? ElementDataChanged;
    public event Action<string>? MessageRaised;

    public EditorService(IConfig config)
    {
        _config = config;
    }

    public async Task<bool> CreateNewGame(string templateKey, string gameName)
    {
        var fileContent = EditorController.CreateNewGameFile(templateKey, gameName);
        var tempPath = Path.Combine(Path.GetTempPath(), $"questviva_{Guid.NewGuid()}.aslx");
        await File.WriteAllTextAsync(tempPath, fileContent);
        _tempFilePath = tempPath;
        return await LoadGameFromFile(tempPath);
    }

    public async Task<bool> LoadGameFromBytes(byte[] fileData, string fileName)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"questviva_{Guid.NewGuid()}_{fileName}");
        await File.WriteAllBytesAsync(tempPath, fileData);
        _tempFilePath = tempPath;
        return await LoadGameFromFile(tempPath);
    }

    private async Task<bool> LoadGameFromFile(string filePath)
    {
        _controller = new EditorController();
        _controller.EditorMode = EditorMode.Web;

        WireUpEvents();

        var result = await _controller.Initialise(_config, filePath);
        if (result)
        {
            IsLoaded = true;
            _controller.UpdateTree();
        }

        return result;
    }

    private void WireUpEvents()
    {
        if (_controller == null) return;

        _controller.ClearTree += OnClearTree;
        _controller.BeginTreeUpdate += OnBeginTreeUpdate;
        _controller.EndTreeUpdate += OnEndTreeUpdate;
        _controller.AddedNode += OnAddedNode;
        _controller.RemovedNode += OnRemovedNode;
        _controller.RenamedNode += OnRenamedNode;
        _controller.RetitledNode += OnRetitledNode;
        _controller.UndoListUpdated += OnUndoListUpdated;
        _controller.RedoListUpdated += OnRedoListUpdated;
        _controller.ElementUpdated += OnElementUpdated;
        _controller.ElementRefreshed += OnElementRefreshed;
        _controller.ShowMessage += OnShowMessage;
    }

    private void OnClearTree(object? sender, EventArgs e)
    {
        TreeNodes.Clear();
        if (!_treeUpdateInProgress)
            TreeChanged?.Invoke();
    }

    private void OnBeginTreeUpdate(object? sender, EventArgs e)
    {
        _treeUpdateInProgress = true;
    }

    private void OnEndTreeUpdate(object? sender, EventArgs e)
    {
        _treeUpdateInProgress = false;
        TreeChanged?.Invoke();
    }

    private void OnAddedNode(object? sender, EditorController.AddedNodeEventArgs e)
    {
        if (e.IsLibraryNode) return;

        var node = new TreeNode
        {
            Key = e.Key,
            Text = e.Text,
            IsLibraryNode = e.IsLibraryNode
        };

        if (string.IsNullOrEmpty(e.Parent))
        {
            if (e.Position.HasValue && e.Position.Value < TreeNodes.Count)
                TreeNodes.Insert(e.Position.Value, node);
            else
                TreeNodes.Add(node);
        }
        else
        {
            var parent = FindNode(TreeNodes, e.Parent);
            if (parent != null)
            {
                node.Parent = parent;
                if (e.Position.HasValue && e.Position.Value < parent.Children.Count)
                    parent.Children.Insert(e.Position.Value, node);
                else
                    parent.Children.Add(node);
            }
            else
            {
                TreeNodes.Add(node);
            }
        }

        if (!_treeUpdateInProgress)
            TreeChanged?.Invoke();
    }

    private void OnRemovedNode(object? sender, EditorController.RemovedNodeEventArgs e)
    {
        RemoveNode(TreeNodes, e.Key);
        if (!_treeUpdateInProgress)
            TreeChanged?.Invoke();
    }

    private void OnRenamedNode(object? sender, EditorController.RenamedNodeEventArgs e)
    {
        var node = FindNode(TreeNodes, e.OldName);
        if (node != null)
        {
            node.Key = e.NewName;
        }
        if (!_treeUpdateInProgress)
            TreeChanged?.Invoke();
    }

    private void OnRetitledNode(object? sender, EditorController.RetitledNodeEventArgs e)
    {
        var node = FindNode(TreeNodes, e.Key);
        if (node != null)
        {
            node.Text = e.NewTitle;
        }
        TreeChanged?.Invoke();
    }

    private void OnUndoListUpdated(object? sender, EditorController.UpdateUndoListEventArgs e)
    {
        CanUndo = e.UndoList.Any();
        UndoStateChanged?.Invoke();
    }

    private void OnRedoListUpdated(object? sender, EditorController.UpdateUndoListEventArgs e)
    {
        CanRedo = e.UndoList.Any();
        UndoStateChanged?.Invoke();
    }

    private void OnElementUpdated(object? sender, EditorController.ElementUpdatedEventArgs e)
    {
        ElementDataChanged?.Invoke();
    }

    private void OnElementRefreshed(object? sender, EditorController.ElementRefreshedEventArgs e)
    {
        ElementDataChanged?.Invoke();
    }

    private void OnShowMessage(object? sender, EditorController.ShowMessageEventArgs e)
    {
        LastMessage = e.Message;
        MessageRaised?.Invoke(e.Message);
    }

    public IEditorDefinition? GetEditorDefinition(string elementKey)
    {
        if (_controller == null) return null;
        var editorName = _controller.GetElementEditorName(elementKey);
        if (editorName == null) return null;
        return _controller.GetEditorDefinition(editorName);
    }

    public IEditorData? GetEditorData(string elementKey)
    {
        return _controller?.GetEditorData(elementKey);
    }

    public Dictionary<string, TemplateData> GetAvailableTemplates()
    {
        return EditorController.GetAvailableTemplates();
    }

    public void StartTransaction(string description)
    {
        _controller?.StartTransaction(description);
    }

    public void EndTransaction()
    {
        _controller?.EndTransaction();
    }

    public void Undo()
    {
        _controller?.Undo();
    }

    public void Redo()
    {
        _controller?.Redo();
    }

    public string? Save()
    {
        return _controller?.Save();
    }

    private TreeNode? FindNode(List<TreeNode> nodes, string key)
    {
        foreach (var node in nodes)
        {
            if (node.Key == key) return node;
            var found = FindNode(node.Children, key);
            if (found != null) return found;
        }
        return null;
    }

    private bool RemoveNode(List<TreeNode> nodes, string key)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].Key == key)
            {
                nodes.RemoveAt(i);
                return true;
            }
            if (RemoveNode(nodes[i].Children, key)) return true;
        }
        return false;
    }

    public void Dispose()
    {
        if (_tempFilePath != null && File.Exists(_tempFilePath))
        {
            try { File.Delete(_tempFilePath); } catch { }
        }
    }
}
