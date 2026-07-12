using QuestViva.Engine;
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

    // Regression test for a follow-up production incident: once the depth guard above trips,
    // the *same* recursing script keeps getting invoked on every subsequent command (e.g. because
    // the corrupted state persists for the rest of the session), so it fails identically forever.
    // Nothing stopped that from repeating indefinitely, and each failed attempt appends to
    // output/log buffers that are only ever cleared after a successful turn - so the session
    // just grew memory without bound until the shared WebPlayer process OOMed, taking every
    // other connected player down with it. WorldModel now trips a circuit breaker once a session
    // accumulates too many script failures and ends the game instead of retrying forever.
    [TestMethod]
    public async Task ChangedFieldScript_RepeatedlyTriggered_TripsCircuitBreakerInsteadOfRetryingForever()
    {
        var driver = await GameDriver.LoadAsync("recursiontest.aslx");

        var failureCount = 0;
        for (var i = 0; i < 30; i++)
        {
            try
            {
                await driver.SendCommandAsync("triggerrecursion");
            }
            catch (Exception ex)
            {
                failureCount++;
                ex.InnerException.ShouldNotBeNull();
                ex.InnerException.Message.ShouldContain("Script execution depth exceeded");
            }
        }

        // A bounded number of attempts are reported as failures before the breaker trips ...
        failureCount.ShouldBeInRange(1, 29);
        // ... after which the game ends instead of continuing to retry the same broken script.
        driver.State.ShouldBe(GameState.Finished);

        // Further commands after the game has ended must be safe no-ops, not more failures.
        await Should.NotThrowAsync(() => driver.SendCommandAsync("triggerrecursion"));
    }
}
