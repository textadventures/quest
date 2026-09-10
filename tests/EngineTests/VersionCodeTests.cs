using Shouldly;

namespace QuestViva.EngineTests;

[TestClass]
public class VersionCodeTests
{
    [TestMethod]
    public async Task VersionCommand_IncludesVersionCodeWhenPresent()
    {
        var driver = await GameDriver.LoadAsync("savetest.aslx");
        driver.Model.Game.Fields.Set("version", "1.0");
        driver.Model.Game.Fields.Set("versioncode", 42);

        var output = string.Join("\n", await driver.SendCommandAsync("version"));
        output.ShouldContain("VERSION:");
        output.ShouldContain("1.0");
        output.ShouldContain("(42)");
    }

    [TestMethod]
    public async Task VersionCommand_UsesDefaultVersionCodeFromType()
    {
        var driver = await GameDriver.LoadAsync("savetest.aslx");
        driver.Model.Game.Fields.Set("version", "1.0");

        var output = string.Join("\n", await driver.SendCommandAsync("version"));
        output.ShouldContain("VERSION:");
        output.ShouldContain("1.0 (1)");
    }
}
