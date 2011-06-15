using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Scripts;

namespace AxeSoftware.Quest
{
    internal class TimerRunner
    {
        private int m_timeElapsed;

        public IEnumerable<IScript> TickAndGetScripts(int elapsedTime)
        {
            m_timeElapsed += elapsedTime;

            List<IScript> scripts = new List<IScript>();
            return scripts;

            // now go through all timers and see if m_timeElapsed >= nextTimeTrigger
        }

        public int GetTimeUntilNextTimerRuns()
        {
            return 0;
            // after a Tick, raise a RequestNextTimerTick
            // 0=never, otherwise a number of seconds
            // (find the earliest next timer, then send how many seconds until it's triggered)
        }
    }
}
