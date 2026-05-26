using System.Reflection;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

public class Config : IConfig
{
    public string HomeFile { get; }
    public bool DevEnabled { get; }
    public bool UseNCalc => false;
}

[TestClass]
public abstract class EditorControllerTestBase
{
    private readonly EditorTreeData m_tree = new();

    protected EditorController Controller { get; private set; }

    protected List<string> UndoList { get; private set; } = new();

    protected List<string> RedoList { get; private set; } = new();

    [TestInitialize]
    public async Task Init()
    {
        Controller = new EditorController();
        Controller.ClearTree += m_controller_ClearTree;
        Controller.BeginTreeUpdate += m_controller_BeginTreeUpdate;
        Controller.EndTreeUpdate += m_controller_EndTreeUpdate;
        Controller.AddedNode += m_controller_AddedNode;
        Controller.UndoListUpdated += m_controller_UndoListUpdated;
        Controller.RedoListUpdated += m_controller_RedoListUpdated;
        var bytes = GetResourceBytes("QuestViva.EditorCoreTests.test.aslx");
        await Controller.Initialise(new Config(), new ByteArrayGameDataProvider(bytes, "test.aslx"));
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

    private void m_controller_ClearTree(object sender, EventArgs e)
    {
        m_tree.Clear();
    }

    private void m_controller_BeginTreeUpdate(object sender, EventArgs e)
    {
        m_tree.BeginUpdate();
    }

    private void m_controller_EndTreeUpdate(object sender, EventArgs e)
    {
        m_tree.EndUpdate();
    }

    private void m_controller_AddedNode(object sender, EditorController.AddedNodeEventArgs e)
    {
        m_tree.Add(e.Key, e.Text, e.Parent);
    }

    private void m_controller_UndoListUpdated(object sender, EditorController.UpdateUndoListEventArgs e)
    {
        UndoList = new List<string>(e.UndoList);
    }

    private void m_controller_RedoListUpdated(object sender, EditorController.UpdateUndoListEventArgs e)
    {
        RedoList = new List<string>(e.UndoList);
    }
}