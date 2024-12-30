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
            return CurrentDate(0);
        }
        public static string CurrentDate(int offset)
        {
            DateTime localDate = DateTime.Now;
            double offsetD= Convert.ToDouble(offset);
            localDate = localDate.AddHours(offsetD);
            return localDate.ToString("yyyy-MM-dd");
        }
        public static string CurrentTime()
        {
            return CurrentTime(0);
        }

        public static string CurrentTime(int offset)
        {
            DateTime localTime = DateTime.Now;
            double offsetD = Convert.ToDouble(offset);
            localTime = localTime.AddHours(offsetD);
            return localTime.ToString("HH:mm:ss");
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
