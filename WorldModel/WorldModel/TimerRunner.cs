using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Scripts;

namespace TextAdventures.Quest
{
    internal class TimerRunner
    {
        private WorldModel m_worldModel;
        private Element m_gameElement;

        public TimerRunner(WorldModel worldModel, bool initialise)
        {
            m_worldModel = worldModel;
            if (initialise)
            {
                // When a game begins, set initial triggers. We don't need to do this when loading
                // a saved game.
                foreach (Element timer in EnabledTimers)
                {
                    timer.Fields[FieldDefinitions.Trigger] = timer.Fields[FieldDefinitions.Interval];
                }
            }
        }

        private IEnumerable<Element> EnabledTimers
        {
            get
            {
                return m_worldModel.Elements.GetElements(ElementType.Timer).Where(t => t.Fields[FieldDefinitions.Enabled]);
            }
        }

        private IEnumerable<Element> DisabledTimers
        {
            get
            {
                return m_worldModel.Elements.GetElements(ElementType.Timer).Where(t => !t.Fields[FieldDefinitions.Enabled]);
            }
        }

        private Element GameElement
        {
            get
            {
                if (m_gameElement == null)
                {
                    m_gameElement = m_worldModel.Elements.Get("game");
                }
                return m_gameElement;
            }
        }

        private int TimeElapsed
        {
            get { return GameElement.Fields[FieldDefinitions.TimeElapsed]; }
            set { GameElement.Fields[FieldDefinitions.TimeElapsed] = value; }
        }

        public void IncrementTime(int elapsedTime)
        {
            TimeElapsed += elapsedTime;

            foreach (var timer in DisabledTimers)
            {
                // Disabled timers get their triggers pushed into the future, so they will run if they become enabled.
                if (timer.Fields[FieldDefinitions.Trigger] < TimeElapsed)
                {
                    timer.Fields[FieldDefinitions.Trigger] = TimeElapsed + timer.Fields[FieldDefinitions.Interval];
                }
                else
                {
                    timer.Fields[FieldDefinitions.Trigger] += elapsedTime;
                }
            }
        }

        public IDictionary<Element, IScript> TickAndGetScripts(int elapsedTime)
        {
            System.Diagnostics.Debug.Print("Time: {0}", TimeElapsed);
            if (elapsedTime > 0) IncrementTime(elapsedTime);

            Dictionary<Element, IScript> scripts = new Dictionary<Element, IScript>();

            foreach (Element timer in EnabledTimers)
            {
                System.Diagnostics.Debug.Print("{0}: Next trigger at {1}", timer.Name, timer.Fields[FieldDefinitions.Trigger]);
                if (TimeElapsed >= timer.Fields[FieldDefinitions.Trigger])
                {
                    System.Diagnostics.Debug.Print("     - TRIGGER");
                    scripts.Add(timer, timer.Fields[FieldDefinitions.Script]);
                    timer.Fields[FieldDefinitions.Trigger] += timer.Fields[FieldDefinitions.Interval];
                }
            }

            return scripts;
        }

        public int GetTimeUntilNextTimerRuns()
        {
            int nextTrigger = TimeElapsed + 60;
            bool enabledTimerExists = false;

            foreach (Element timer in EnabledTimers)
            {
                enabledTimerExists = true;
                if (timer.Fields[FieldDefinitions.Trigger] < nextTrigger)
                {
                    nextTrigger = timer.Fields[FieldDefinitions.Trigger];
                }
            }

            if (!enabledTimerExists) return 0;

            return (nextTrigger - TimeElapsed);
        }
    }
}
