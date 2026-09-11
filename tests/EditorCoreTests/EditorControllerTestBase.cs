using System.Reflection;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

[TestClass]
public abstract class EditorControllerTestBase
{
    private readonly EditorTreeData _tree = new();

    protected EditorController Controller { get; private set; } = null!;

    protected List<string> UndoList { get; private set; } = new();

    protected List<string> RedoList { get; private set; } = new();

    [TestInitialize]
    public async Task Init()
    {
        Controller = new EditorController();
        Controller.ClearTree += OnControllerClearTree;
        Controller.BeginTreeUpdate += OnControllerBeginTreeUpdate;
        Controller.EndTreeUpdate += OnControllerEndTreeUpdate;
        Controller.AddedNode += OnControllerAddedNode;
        Controller.UndoListUpdated += OnControllerUndoListUpdated;
        Controller.RedoListUpdated += OnControllerRedoListUpdated;
        var bytes = GetResourceBytes("QuestViva.EditorCoreTests.test.aslx");
        await Controller.Initialise(new ByteArrayGameDataProvider(bytes, "test.aslx"));
        DoExtraInitialisation();
    }

    private static byte[] GetResourceBytes(string resource)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource)!;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    public virtual void DoExtraInitialisation()
    {
    }

    [TestCleanup]
    public void Cleanup()
    {
        Controller.Dispose();
    }

    private void OnControllerClearTree(object? sender, EventArgs e)
    {
        _tree.Clear();
    }

    private void OnControllerBeginTreeUpdate(object? sender, EventArgs e)
    {
        _tree.BeginUpdate();
    }

    private void OnControllerEndTreeUpdate(object? sender, EventArgs e)
    {
        _tree.EndUpdate();
    }

    private void OnControllerAddedNode(object? sender, EditorController.AddedNodeEventArgs e)
    {
        _tree.Add(e.Key, e.Text, e.Parent);
    }

    private void OnControllerUndoListUpdated(object? sender, EditorController.UpdateUndoListEventArgs e)
    {
        UndoList = new List<string>(e.UndoList);
    }

    private void OnControllerRedoListUpdated(object? sender, EditorController.UpdateUndoListEventArgs e)
    {
        RedoList = new List<string>(e.UndoList);
    }
}