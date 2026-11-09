using QuestViva.Common;
using QuestViva.Engine;
using QuestViva.PlayerCore;

namespace QuestViva.PlayerCoreTests;

[TestClass]
public class GameCheckTests
{
    private static Task<GameCheckResult> CheckAsync(string filename) =>
        GameCheck.CheckAsync(new GameData(File.ReadAllBytes(filename), filename,
            new Dictionary<string, byte[]> { ["checklib.aslx"] = File.ReadAllBytes("checklib.aslx") }));

    private static async Task<IReadOnlyList<CheckProblem>> ProblemsAsync()
    {
        var result = await CheckAsync("check.aslx");
        Assert.IsTrue(result.Loaded);
        return result.Problems;
    }

    private static CheckProblem Single(IReadOnlyList<CheckProblem> problems, string messagePart) =>
        problems.Single(p => p.Message.Contains(messagePart));

    [TestMethod]
    public async Task AWorkingGameHasNoProblems()
    {
        var result = await CheckAsync("headless.aslx");
        Assert.IsTrue(result.Loaded);
        Assert.IsEmpty(result.Problems);
    }

    [TestMethod]
    public async Task UnknownCommandsAreFoundInEveryBlock()
    {
        var problems = await ProblemsAsync();
        var prnt = Single(problems, "'Prnt'");
        Assert.AreEqual("game", prnt.Kind);
        Assert.AreEqual("start", prnt.Attribute);
        Assert.IsNull(prnt.Filename);
        Single(problems, "'msgg'");
    }

    [TestMethod]
    public async Task UnknownFunctionInAnExpression()
    {
        var problem = Single(await ProblemsAsync(), "Unknown function 'NoSuchFunction'");
        Assert.AreEqual("object", problem.Kind);
        Assert.AreEqual("room", problem.Element);
        Assert.AreEqual("description", problem.Attribute);
    }

    [TestMethod]
    public async Task TooManyArguments()
    {
        var problems = (await ProblemsAsync()).Where(p => p.Message.Contains("'TwoParams' takes 2 parameters")).ToList();
        // Once as a statement in start, once in an expression in description; too few in an
        // expression is fine.
        Assert.HasCount(2, problems);
        Assert.IsTrue(problems.All(p => p.Message.Contains("given 3")));
    }

    [TestMethod]
    public async Task UnparseableAndEmptyExpressions()
    {
        var problems = await ProblemsAsync();
        Single(problems, "Can't parse the expression '(1 +'");
        Single(problems, "An expression is empty");
    }

    [TestMethod]
    public async Task SeveralCaseValuesAndUnknownObjectsAreNotReported()
    {
        var problems = await ProblemsAsync();
        Assert.IsFalse(problems.Any(p => p.Message.Contains("7, 8, 9")));
        Assert.IsFalse(problems.Any(p => p.Message.Contains("nosuchobject")));
    }

    [TestMethod]
    public async Task DuplicateElementNames()
    {
        var problem = Single(await ProblemsAsync(), "more than one element called 'lamp'");
        Assert.AreEqual("lamp", problem.Element);
    }

    [TestMethod]
    public async Task AnIncludedLibraryIsCheckedAndNamed()
    {
        var problem = Single(await ProblemsAsync(), "'AlsoNoSuchFunction'");
        Assert.AreEqual("checklib.aslx", problem.Filename);
        Assert.AreEqual("function", problem.Kind);
        Assert.AreEqual("LibraryFunction", problem.Element);
    }

    [TestMethod]
    public async Task AVerbRepeatingABuiltInOneIsFound()
    {
        var problem = Single(await ProblemsAsync(), "runs the attribute 'buy'");
        Assert.AreEqual("command", problem.Kind);
        Assert.AreEqual("property", problem.Attribute);
        StringAssert.Contains(problem.Message, "(buy)");
        StringAssert.Contains(problem.Message, "built-in verb 'buy' in CoreCommands.aslx");
    }

    [TestMethod]
    public async Task TwoGameVerbsWithOneAttributeAreReportedOnce()
    {
        var problem = Single(await ProblemsAsync(), "runs the attribute 'shine'");
        Assert.AreEqual("buff", problem.Element);
        StringAssert.Contains(problem.Message, "the verb 'polish'");
    }

    [TestMethod]
    public async Task AFunctionReturningAValueNamedAfterAnEngineFunction()
    {
        // The engine's names match whatever the case, as the expression evaluator's do.
        var problem = Single(await ProblemsAsync(), "'capfirst' is also the name of a function built into the engine");
        Assert.AreEqual("function", problem.Kind);
        Assert.AreEqual("capfirst", problem.Element);
        Assert.IsNull(problem.Filename);
    }

    [TestMethod]
    public async Task AFunctionWithNoReturnTypeNamedAfterAnEngineFunctionIsFine()
    {
        // A statement calls the game's function, so one that returns nothing still runs.
        Assert.IsFalse((await ProblemsAsync()).Any(p => p.Element == "ToString"));
    }

    [TestMethod]
    public async Task NoCoreFunctionIsHiddenByAnEngineFunction()
    {
        // A published game has Core inlined, where it's checked like the game's own functions, so a
        // Core function that returned a value under an engine function's name would be reported in
        // every published game.
        var world = new WorldModel(new GameData(File.ReadAllBytes("headless.aslx"), "headless.aslx"), null);
        Assert.IsTrue(await world.InitialiseEdit());
        var hidden = world.Elements.GetElements(ElementType.Function)
            .Where(GameChecker.IsHiddenByEngineFunction)
            .Select(f => f.Name);
        Assert.IsEmpty(hidden);
    }

    [TestMethod]
    public async Task BuiltInLibrariesAreNotChecked()
    {
        Assert.IsTrue((await ProblemsAsync()).All(p => p.Filename is null or "checklib.aslx"));
    }

    [TestMethod]
    public async Task ExactlyTheExpectedProblems()
    {
        Assert.HasCount(12, await ProblemsAsync());
    }

    [TestMethod]
    public async Task LoaderErrorsMeanScriptsArentChecked()
    {
        var result = await CheckAsync("checkbadtype.aslx");
        Assert.IsFalse(result.Loaded);
        StringAssert.Contains(result.Problems.Single().Message, "nosuchtype");
    }

    [TestMethod]
    public async Task Quest4GamesAreNotSupported()
    {
        await Assert.ThrowsAsync<NotSupportedException>(() => CheckAsync("quest4.asl"));
    }
}
