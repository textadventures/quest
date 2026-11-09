using System.Text;
using QuestViva.Common;
using Shouldly;

namespace QuestViva.EngineTests;

// LOOK IN / LOOK INSIDE <object> is a Core verb (property "lookin"). Containers get a default
// script from container_base that lists their contents; anything else gets DefaultLookIn.
[TestClass]
public class LookInTests
{
    private static async Task<string> Run(GameDriver driver, string command)
    {
        return string.Join("\n", await driver.SendCommandAsync(command));
    }

    [TestMethod]
    public async Task OpenContainerListsContentsAndRevealsHiddenChildren()
    {
        var driver = await GameDriver.LoadAsync("lookintest.aslx");
        driver.Model.Elements.Get("box").Fields.GetAsType<bool>("hidechildren").ShouldBeTrue();

        var output = await Run(driver, "look in box");

        output.ShouldContain("It contains an apple and a pear.");
        driver.Model.Elements.Get("box").Fields.GetAsType<bool>("hidechildren").ShouldBeFalse();
    }

    [TestMethod]
    public async Task LookInsideIsASynonym()
    {
        var driver = await GameDriver.LoadAsync("lookintest.aslx");
        (await Run(driver, "look inside box")).ShouldContain("It contains an apple and a pear.");
    }

    [TestMethod]
    public async Task ClosedOpaqueContainer()
    {
        var driver = await GameDriver.LoadAsync("lookintest.aslx");
        var output = await Run(driver, "look in chest");
        output.ShouldContain("It is closed.");
        output.ShouldNotContain("gold");
    }

    [TestMethod]
    public async Task ClosedTransparentContainer()
    {
        var driver = await GameDriver.LoadAsync("lookintest.aslx");
        (await Run(driver, "look in jar")).ShouldContain("It contains a marble.");
    }

    [TestMethod]
    public async Task EmptyContainer()
    {
        var driver = await GameDriver.LoadAsync("lookintest.aslx");
        (await Run(driver, "look in bucket")).ShouldContain("It is empty.");
    }

    [TestMethod]
    public async Task NonContainer()
    {
        var driver = await GameDriver.LoadAsync("lookintest.aslx");
        (await Run(driver, "look in rock")).ShouldContain("You can't see inside it.");
    }

    [TestMethod]
    public async Task PerObjectStringOverride()
    {
        var driver = await GameDriver.LoadAsync("lookintest.aslx");
        (await Run(driver, "look in mirror")).ShouldContain("You see your own reflection.");
    }

    [TestMethod]
    public async Task PerObjectScriptOverridesContainerDefault()
    {
        var driver = await GameDriver.LoadAsync("lookintest.aslx");
        var output = await Run(driver, "look in cupboard");
        output.ShouldContain("A moth flies out.");
        output.ShouldNotContain("contains");
    }

    [TestMethod]
    public async Task DarkRoom()
    {
        var driver = await GameDriver.LoadAsync("lookintest.aslx");
        await Run(driver, "in");

        // A weak light source can be seen, and looked inside, without lighting the room, but only
        // contents that give off light themselves are listed. Nothing visible isn't "empty".
        (await Run(driver, "look in lantern")).ShouldContain("It contains a candle.");
        (await Run(driver, "look in tin")).ShouldContain("It is too dark to make anything out.");
        (await Run(driver, "look in crate")).ShouldNotContain("bottle");
        var output = await Run(driver, "look in pouch");
        output.ShouldContain("It is too dark to make anything out.");
        output.ShouldNotContain("coin");
    }

    [TestMethod]
    public async Task BareLookInStillReachesTheInLookExit()
    {
        var driver = await GameDriver.LoadAsync("lookintest.aslx");
        (await Run(driver, "look in")).ShouldContain("Steps lead down.");
    }

    [TestMethod]
    public async Task GameCommandBeatsCoreVerb()
    {
        var driver = await GameDriver.LoadAsync("lookinoverridetest.aslx");
        (await Run(driver, "look in box")).ShouldContain("Game command looks in box.");
    }

    [TestMethod]
    public async Task GameVerbBeatsCoreVerb()
    {
        var driver = await GameDriver.LoadAsync("lookinoverridetest.aslx");
        (await Run(driver, "look inside rock")).ShouldContain("Game verb: nothing inside rock.");
    }

    // German and Spanish are held at full coverage, so they have their own wordings and messages.
    [TestMethod]
    [DataRow("Deutsch.aslx", "schau in kiste", "Sie ist geschlossen.", "sieh in eimer", "Er ist leer.", "schaue in stein", "Da kannst du nicht hineinsehen.")]
    [DataRow("Espanol.aslx", "mira en cofre", "El cofre está cerrado.", "mirar dentro de cubo", "El cubo está vacío.", "mira en roca", "No puedes ver nada dentro.")]
    public async Task OtherLanguages(string library, string closedCommand, string closed, string emptyCommand, string empty,
        string nonContainerCommand, string nonContainer)
    {
        var driver = await GameDriver.LoadAsync(LocalisedGame(library));
        (await Run(driver, closedCommand)).ShouldContain(closed);
        (await Run(driver, emptyCommand)).ShouldContain(empty);
        (await Run(driver, nonContainerCommand)).ShouldContain(nonContainer);
    }

    private static GameData LocalisedGame(string library)
    {
        var (closed, closedGender, closedArticle, empty, emptyGender, emptyArticle, rock) = library == "Deutsch.aslx"
            ? ("kiste", "sie", "die", "eimer", "er", "der", "stein")
            : ("cofre", "él", "el", "cubo", "él", "el", "roca");
        var xml = $"""
            <asl version="600">
              <include ref="{library}" />
              <include ref="Core.aslx" />
              <game name="lookinlanguagetest"/>
              <object name="room">
                <object name="player">
                  <inherit name="defaultplayer" />
                </object>
                <object name="{closed}">
                  <inherit name="container_closed" />
                  <gender>{closedGender}</gender>
                  <article>{closedArticle}</article>
                </object>
                <object name="{empty}">
                  <inherit name="container_open" />
                  <gender>{emptyGender}</gender>
                  <article>{emptyArticle}</article>
                </object>
                <object name="{rock}" />
              </object>
            </asl>
            """;
        return new GameData(Encoding.UTF8.GetBytes(xml), "lookinlanguagetest.aslx");
    }
}
