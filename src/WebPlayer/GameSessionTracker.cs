namespace QuestViva.WebPlayer;

public class GameSessionTracker
{
    private int _activeGames;

    public int ActiveGames => _activeGames;

    public void GameStarted() => Interlocked.Increment(ref _activeGames);
    public void GameEnded() => Interlocked.Decrement(ref _activeGames);
}
