using Moq;
using QuestViva.Common;
using Shouldly;

namespace QuestViva.EngineTests;

// 610 CoreGrid passes a room's picture (grid_image, #2199) to QuestUI.map.drawBox as a tenth
// argument: its URL through GetFileURL, or "" for a room without one.
[TestClass]
public class MapRoomPictureTests
{
    private static List<object[]> DrawBoxCalls(GameDriver driver) =>
        driver.PlayerMock.Invocations
            .Where(i => i.Method.Name == nameof(IPlayer.RunScriptAsync)
                        && (string)i.Arguments[0] == "QuestUI.map.drawBox")
            .Select(i => (object[])i.Arguments[1])
            .ToList();

    private static Task<GameDriver> LoadAsync() =>
        GameDriver.LoadAsync("mapimagetest.aslx", setUpPlayer: player => player
            .Setup(p => p.GetUrlAsync(It.IsAny<string>()))
            .ReturnsAsync((string file) => $"https://resources.example.test/{file}"));

    [TestMethod]
    public async Task ARoomWithAPicture_PassesItsUrlToDrawBox()
    {
        var driver = await LoadAsync();

        var call = DrawBoxCalls(driver).ShouldHaveSingleItem();
        call.Length.ShouldBe(10);
        call[9].ShouldBe("https://resources.example.test/hall.png");
    }

    [TestMethod]
    public async Task ARoomWithoutAPicture_PassesAnEmptyString()
    {
        var driver = await LoadAsync();
        await driver.SendCommandAsync("down");

        var call = DrawBoxCalls(driver).Last();
        call.Length.ShouldBe(10);
        call[9].ShouldBe("");
    }
}
