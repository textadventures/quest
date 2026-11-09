using QuestViva.EditorCore;

namespace QuestViva.EditorCoreTests;

// A gamebook page's options are shown to the player in stored order, so the editor has to show them
// in that order too, and let the author change it (issue #2394).
[TestClass]
public class EditableDictionaryOrderTests : EditorControllerTestBase
{
    private IEditableDictionary<string> _dict = null!;

    public override void DoExtraInitialisation()
    {
        _dict = Controller.CreateNewEditableStringDictionary("testobj", "dictattr", "one", "1", true);
        _dict.Add("two", "2");
        _dict.Add("three", "3");
    }

    private string Keys() => string.Join(";", _dict.Items.Values.Select(i => i.Key));

    private string SourceKeys()
    {
        var data = Controller.GetEditorData("testobj")!;
        var source = (IDictionary<string, string>) ((IDataWrapper) _dict).GetUnderlyingValue()!;
        Assert.AreSame(source, ((IDataWrapper) data.GetAttribute("dictattr")!).GetUnderlyingValue());
        return string.Join(";", source.Keys);
    }

    [TestMethod]
    public void ItemsAddedAfterRemovalsKeepInsertionOrder()
    {
        // A plain Dictionary would put "zebra" into "three"'s freed slot and "apple" into "two"'s,
        // listing them as apple, zebra.
        _dict.Remove("two");
        _dict.Remove("three");
        _dict.Add("zebra", "z");
        _dict.Add("apple", "a");

        Assert.AreEqual("one;zebra;apple", Keys());
        Assert.AreEqual(SourceKeys(), Keys());
    }

    [TestMethod]
    public void UpdateKeepsPosition()
    {
        _dict.Remove("one");
        _dict.Add("four", "4");
        _dict.Update("two", "changed");

        Assert.AreEqual("two;three;four", Keys());
        Assert.AreEqual("changed", _dict.Items["two"].Value);
    }

    [TestMethod]
    public void UndoRemoveRestoresPosition()
    {
        _dict.Remove("two");
        Controller.Undo();

        Assert.AreEqual("one;two;three", Keys());
    }

    [TestMethod]
    public void MoveReordersItemsAndUndoes()
    {
        _dict.Move("three", 0);
        Assert.AreEqual("three;one;two", Keys());
        Assert.AreEqual(SourceKeys(), Keys());

        _dict.Move("three", 2);
        Assert.AreEqual("one;two;three", Keys());

        _dict.Move("one", 1);
        Assert.AreEqual("two;one;three", Keys());
        Assert.AreEqual("1", _dict.Items["one"].Value);

        Controller.Undo();
        Assert.AreEqual("one;two;three", Keys());
        Assert.AreEqual(SourceKeys(), Keys());

        Controller.Redo();
        Assert.AreEqual("two;one;three", Keys());
    }

    [TestMethod]
    public void MoveOutOfRangeThrows()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _dict.Move("one", 3));
        Assert.ThrowsException<ArgumentException>(() => _dict.Move("missing", 0));
        Assert.AreEqual("one;two;three", Keys());
    }
}
