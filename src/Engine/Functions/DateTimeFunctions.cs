using System;

namespace QuestViva.Engine.Functions;

public static class DateTimeFunctions
{
    public static int CurrentDateUTC()
    {
        return (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }

}