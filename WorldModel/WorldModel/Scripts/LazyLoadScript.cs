using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class LazyLoadScript : IScript, IIfScript, IFirstTimeScript
    {
        private IScriptConstructor m_constructor;
        private string m_scriptString;
        private ScriptContext m_scriptContext;
        private IScript m_script;
        private IScriptParent m_parent;

        private static int count = 0;
        private static int initialised = 0;

        public LazyLoadScript(IScriptConstructor constructor, string scriptString, ScriptContext scriptContext)
        {
            count++;
            System.Diagnostics.Debug.WriteLine("New LazyLoadScript {0}", count);
            m_constructor = constructor;
            m_scriptString = scriptString;
            m_scriptContext = scriptContext;
        }

        private void Initialise()
        {
            if (m_script != null) return;
            initialised++;
            System.Diagnostics.Debug.WriteLine("Initialise LazyLoadScript {0}", initialised);
            m_script = m_constructor.Create(m_scriptString, m_scriptContext);
            m_script.Line = m_scriptString;
            m_script.Parent = m_parent;
        }

        public IMutableField Clone()
        {
            if (m_script != null) return m_script.Clone();
            LazyLoadScript result = new LazyLoadScript(m_constructor, m_scriptString, m_scriptContext);
            result.Line = m_scriptString;
            result.Parent = m_parent;
            return result;
        }

        public event EventHandler<ScriptUpdatedEventArgs> ScriptUpdated
        {
            add
            {
                Initialise();
                m_script.ScriptUpdated += value;
            }
            remove
            {
                Initialise();
                m_script.ScriptUpdated -= value;
            }
        }

        public void Execute(Context c)
        {
            Initialise();
            m_script.Execute(c);
        }

        public string Line
        {
            get
            {
                if (m_script == null) return m_scriptString;
                return m_script.Line;
            }
            set
            {
                if (m_script == null)
                {
                    m_scriptString = value;
                    return;
                }
                m_script.Line = value;
            }
        }

        public string Save()
        {
            Initialise();
            return m_script.Save();
        }

        public void SetParameter(int index, object value)
        {
            Initialise();
            m_script.SetParameter(index, value);
        }

        public void SetParameterSilent(int index, object value)
        {
            Initialise();
            m_script.SetParameterSilent(index, value);
        }

        public object GetParameter(int index)
        {
            Initialise();
            return m_script.GetParameter(index);
        }

        public string Keyword
        {
            get
            {
                Initialise();
                return m_script.Keyword;
            }
        }

        public IScriptParent Parent
        {
            get
            {
                if (m_script == null) return m_parent;
                return m_script.Parent;
            }
            set
            {
                if (m_script == null)
                {
                    m_parent = value;
                    return;
                }
                m_script.Parent = value;
            }
        }

        public IEnumerable<string> GetDefinedVariables()
        {
            Initialise();
            return m_script.GetDefinedVariables();
        }

        public UndoLogger UndoLog
        {
            get
            {
                Initialise();
                return m_script.UndoLog;
            }
            set
            {
                Initialise();
                m_script.UndoLog = value;
            }
        }

        public Element Owner
        {
            get
            {
                Initialise();
                return m_script.Owner;
            }
            set
            {
                Initialise();
                m_script.Owner = value;
            }
        }

        public bool Locked
        {
            get
            {
                Initialise();
                return m_script.Locked;
            }
            set
            {
                Initialise();
                m_script.Locked = value;
            }
        }

        public bool RequiresCloning
        {
            get
            {
                Initialise();
                return m_script.RequiresCloning;
            }
        }

        public void SetElse(IScript elseScript)
        {
            Initialise();
            ((IfScript)m_script).SetElse(elseScript);
        }

        public IfScript.ElseIfScript AddElseIf(Functions.IFunction<bool> expression, IScript script)
        {
            Initialise();
            return ((IfScript)m_script).AddElseIf(expression, script);
        }

        public Functions.IFunction<bool> Expression
        {
            get
            {
                Initialise();
                return ((IfScript)m_script).Expression;
            }
        }

        public IScript ThenScript
        {
            get
            {
                Initialise();
                return ((IfScript)m_script).ThenScript;
            }
            set
            {
                Initialise();
                ((IfScript)m_script).ThenScript = value;
            }
        }

        public void SetOtherwiseScript(IScript script)
        {
            Initialise();
            ((FirstTimeScript)m_script).SetOtherwiseScript(script);
        }
    }
}
