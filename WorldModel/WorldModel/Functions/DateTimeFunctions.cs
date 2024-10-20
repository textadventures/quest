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
            DateTime localDate = DateTime.Now;
            return localDate.ToShortTimeString();
        }


     
    }
}
