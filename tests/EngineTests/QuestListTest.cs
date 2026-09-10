using QuestViva.Engine;

namespace QuestViva.EngineTests;

[TestClass]
public class QuestListTest
{
    private Element _a, _b, _c;
    private WorldModel _worldModel;

    [TestInitialize]
    public void Setup()
    {
        _worldModel = Helpers.CreateWorldModel();

        _a = _worldModel.GetElementFactory(ElementType.Object).Create("a");
        _b = _worldModel.GetElementFactory(ElementType.Object).Create("b");
        _c = _worldModel.GetElementFactory(ElementType.Object).Create("c");
    }

    [TestMethod]
    public void ExcludeTest()
    {
        //string lists
        var stringList = new QuestList<string> {"a", "a", "b", "c"};

        var expected = new QuestList<string> {"b", "c"};
        var actual = stringList.Exclude("a");
        Assert.IsTrue(actual.SequenceEqual(expected));

        //element lists
        var elList = new QuestList<Element> {_a, _a, _b, _c};

        var expectedEList = new QuestList<Element> {_b, _c};
        var actualEList = elList.Exclude(_a);
        Assert.IsTrue(actualEList.SequenceEqual(expectedEList));
    }

    [TestMethod]
    public void ExcludeWithListTest()
    {
        //string lists
        var stringList = new QuestList<string> {"a", "a", "b", "c"};
        var excludeList = new QuestList<string> {"a", "b"};

        var expected = new QuestList<string> {"c"};
        var actual = stringList.Exclude(excludeList);
        Assert.IsTrue(actual.SequenceEqual(expected));

        //element lists
        var elList = new QuestList<Element> {_a, _a, _b, _c};
        var excludeEList = new QuestList<Element> {_a, _b};

        var expectedEList = new QuestList<Element> {_c};
        var actualEList = elList.Exclude(excludeEList);
        Assert.IsTrue(actualEList.SequenceEqual(expectedEList));
    }
}