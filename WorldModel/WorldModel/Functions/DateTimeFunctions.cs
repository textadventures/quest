using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Functions
{
    public static class DateTimeFunctions
    {
        public static string CurrentDateUTC()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        }

    }
}
