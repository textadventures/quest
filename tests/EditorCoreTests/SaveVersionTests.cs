namespace QuestViva.EditorCoreTests;

// The editor saves every game as WorldModel.EditorSaveVersion, which older Quest versions can't
// open, so it warns about a game that a save would upgrade (#2339). test.aslx is version 580.
[TestClass]
public class SaveVersionTests : EditorControllerTestBase
{
    [TestMethod]
    public void OlderGameReportsItsVersionAndThatSavingWillUpgradeIt()
    {
        Assert.AreEqual(580, Controller.AslVersion);
        Assert.IsTrue(Controller.SaveWillUpgrade);
    }

    [TestMethod]
    public void SavingUpgradesTheGame()
    {
        var xml = Controller.Save();

        StringAssert.Contains(xml, "<asl version=\"610\">");
        Assert.AreEqual(610, Controller.AslVersion);
        Assert.IsFalse(Controller.SaveWillUpgrade);
    }
}
