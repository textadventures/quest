using Shouldly;

namespace QuestViva.EngineTests;

// Regression test for a production incident: a "changed<field>" script that sets the field
// it's watching recurses through WorldModel.RunScriptAsync with no base case, eventually
// throwing a StackOverflowException. That exception can't be caught, so it killed the whole
// WebPlayer process - and every other player connected to it - instead of just failing the
// one script. WorldModel.RunScriptAsync now caps recursion depth and throws a normal,
// catchable exception once it's exceeded.
[TestClass]
public class RecursionGuardTests
{
    [TestMethod]
    public async Task ChangedFieldScript_SetsSameField_FailsGracefullyInsteadOfCrashing()
    {
        var driver = await GameDriver.LoadAsync("recursiontest.aslx");

        var ex = await Should.ThrowAsync<Exception>(() => driver.SendCommandAsync("triggerrecursion"));

        ex.InnerException.ShouldNotBeNull();
        ex.InnerException.Message.ShouldContain("Script execution depth exceeded");
    }
}
