namespace QuestViva.Engine;

public enum GameState
{
    NotStarted,
    Loading,
    Running,
    Finished
}

public enum ThreadState
{
    Ready,
    Working,
    Waiting
}

public enum UpdateSource
{
    System,
    User
}

public enum WorldModelVersion
{
    v500,
    v510,
    v520,
    v530,
    v540,
    v550,
    v580
}