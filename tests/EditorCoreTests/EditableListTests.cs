using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

[TestClass]
public class EditableListTests : EditorControllerTestBase
{
    private IEditableList<string> _list = null!;
    private object _listattr = null!;

    public override void DoExtraInitialisation()
    {
        _listattr = Controller.GetEditorData("testobj")!.GetAttribute("listattr")!;
        _list = (IEditableList<string>) _listattr;
    }

    private string GetItemListString(IEditableList<string> list)
    {
        return string.Join(";", list.Items.Select(i => i.Value.Value));
    }

    private string[] GetItemListKeys(IEditableList<string> list)
    {
        return list.Items.Select(i => i.Key).ToArray();
    }

    [TestMethod]
    public void TestListSetup()
    {
        Assert.IsInstanceOfType(_listattr, typeof(IEditableList<string>));
        Assert.AreEqual("one;two;three;four;five", GetItemListString(_list));
    }

    [TestMethod]
    public void TestListAdd()
    {
        _list.Add("new");
        Assert.AreEqual("one;two;three;four;five;new", GetItemListString(_list));

        Controller.Undo();
        Assert.AreEqual("one;two;three;four;five", GetItemListString(_list));
    }

    [TestMethod]
    public void TestListRemove()
    {
        // Remove individual item
        _list.Remove(GetItemListKeys(_list)[2]);
        Assert.AreEqual("one;two;four;five", GetItemListString(_list));

        Controller.Undo();
        Assert.AreEqual("one;two;three;four;five", GetItemListString(_list));

        // Remove multiple items
        _list.Remove(GetItemListKeys(_list)[0], GetItemListKeys(_list)[3]);
        Assert.AreEqual("two;three;five", GetItemListString(_list));

        Controller.Undo();
        Assert.AreEqual("one;two;three;four;five", GetItemListString(_list));
    }
}