using QuestViva.Common;
using QuestViva.Engine;
using QuestViva.Legacy;

namespace QuestViva.PlayerCore;

public class GameLauncher(WorldModelFactory worldModelFactory)
{
    public IGame? GetGame(GameData gameData, Stream? saveData)
    {
        switch (Path.GetExtension(gameData.Filename).ToLower())
        {
            case ".aslx":
            case ".quest":
            case ".quest-save":
                return worldModelFactory.Create(gameData, saveData);
            case ".asl":
            case ".cas":
            case ".qsg":
                var game = new V4Game(gameData, saveData);
                return game;
            case ".zip":
                // Zipped games aren't supported
                throw new NotImplementedException();
            default:
                return null;
        }
    }
}