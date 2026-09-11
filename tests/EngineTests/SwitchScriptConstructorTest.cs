using QuestViva.Engine;
using QuestViva.Engine.Scripts;

namespace QuestViva.EngineTests;

[TestClass]
public class SwitchScriptConstructorTest
{
    private SwitchScriptConstructor _constructor = null!;
    private ScriptFactory _scriptFactory = null!;
    private WorldModel _worldModel = null!;

    private ScriptContext _scriptContext = null!;

    [TestInitialize]
    public void Setup()
    {
        _worldModel = Helpers.CreateWorldModel();
        _scriptFactory = new ScriptFactory(_worldModel);

        _constructor = new SwitchScriptConstructor();
        _constructor.WorldModel = _worldModel;
        _constructor.ScriptFactory = _scriptFactory;

        _scriptContext = new ScriptContext(_worldModel);
    }

    [TestMethod]
    public void CreateTest()
    {
        var text = @"switch (""1"") {
            case (1) {
                msg (""!"")
            }
            }";
        var script = _constructor.Create(text, _scriptContext);
        var actualCases = (QuestDictionary<IScript>) script.GetParameter(1)!;

        Assert.AreEqual(1, actualCases.Count);
        Assert.IsTrue(actualCases.Contains("1"));

        text = @"switch (""1"") {
            case (StringListItem(myStringList, 0), 1) {
                msg (""!"")
            }
            }";
        script = _constructor.Create(text, _scriptContext);
        actualCases = (QuestDictionary<IScript>) script.GetParameter(1)!;

        Assert.AreEqual(2, actualCases.Count);
        Assert.AreSame(actualCases["StringListItem(myStringList, 0)"],
            actualCases["1"]);
    }
}