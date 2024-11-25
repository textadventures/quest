using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Functions
{
    public static class DateTimeFunctions
    {
        public static string CurrentDate()
        {
            DateTime localDate = DateTime.Now; 
            return localDate.ToShortDateString();
        }

        public static string CurrentTime()
        {
            DateTime localTime = DateTime.Now;
            return localTime.ToShortTimeString();
        }
        
        public static string CurrentUtcDate()
        {
            DateTime utcDate = DateTime.UtcNow; 
            return utcDate.ToShortDateString();
        }

        public static string CurrentUtcTime()
        {
            DateTime utcTime = DateTime.UtcNow;
            return utcTime.ToShortTimeString();
        }
     
    }
}
