using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Scripts;

namespace TextAdventures.Quest
{
    public class Callback
    {
        public Callback(IScript script, Context context)
        {
            Script = script;
            Context = context;
        }

        public IScript Script { get; private set; }
        public Context Context { get; private set; }
    }

    internal class CallbackManager
    {
        public enum CallbackTypes
        {
            Menu,
            Wait,
            Question,
            GetInput
        }

        private Dictionary<CallbackTypes, Callback> m_callbacks = new Dictionary<CallbackTypes, Callback>();
        private List<Callback> m_onReadyCallbacks = new List<Callback>();

        public void Push(CallbackTypes type, Callback callback, string exception = "Callback already exists")
        {
            if (m_callbacks.ContainsKey(type))
            {
                throw new InvalidOperationException(exception);
            }

            m_callbacks[type] = callback;
        }

        public Callback Pop(CallbackTypes type)
        {
            if (!m_callbacks.ContainsKey(type)) return null;
            Callback result = m_callbacks[type];
            m_callbacks.Remove(type);
            return result;
        }

        public bool AnyOutstanding()
        {
            return m_callbacks.Count > 0;
        }

        public void AddOnReadyCallback(Callback callback)
        {
            m_onReadyCallbacks.Add(callback);
        }

        public IEnumerable<Callback> FlushOnReadyCallbacks()
        {
            List<Callback> currentList = m_onReadyCallbacks;
            m_onReadyCallbacks = new List<Callback>();
            return currentList;
        }
    }
}
