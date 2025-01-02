using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Functions
{
    public static class DateTimeFunctions
    {
        public static int CurrentDateUTC()
        {
            return (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

    }
}
