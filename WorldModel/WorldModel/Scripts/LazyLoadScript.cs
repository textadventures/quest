using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextAdventures.Quest.Scripts
{
    public class LazyLoadScript : IScript, IIfScript, IFirstTimeScript, IMultiScript
    {
        private ScriptFactory m_scriptFactory;
        private IScriptConstructor m_scriptConstructor;
        private string m_scriptString;
        private ScriptContext m_scriptContext;
        private IScript m_script;
        private IScriptParent m_parent;
        private WorldModel m_worldModel;

        public LazyLoadScript(ScriptFactory scriptFactory, string scriptString, ScriptContext scriptContext)
        {
            m_scriptFactory = scriptFactory;
            m_scriptString = scriptString;
            m_scriptContext = scriptContext;
            m_worldModel = scriptFactory.WorldModel;
        }

        public LazyLoadScript(ScriptFactory scriptFactory, IScriptConstructor scriptConstructor, string scriptString, ScriptContext scriptContext)
        {
            m_scriptFactory = scriptFactory;
            m_scriptConstructor = scriptConstructor;
            m_scriptString = scriptString;
            m_scriptContext = scriptContext;
            m_worldModel = scriptConstructor.WorldModel;
        }

        private void Initialise()
        {
            if (m_script != null) return;

            try
            {
                if (m_scriptConstructor == null)
                {
                    m_script = m_scriptFactory.CreateScript(m_scriptString, m_scriptContext, false, false);
                }
                else
                {
                    m_script = m_scriptConstructor.Create(m_scriptString, m_scriptContext);
                    m_script.Line = m_scriptString;
                }
            }
            catch
            {
                if (!m_worldModel.EditMode) throw;

                m_script = new FailedScript(m_scriptString);
                if (m_scriptConstructor == null)
                {
                    m_script = new MultiScript(m_scriptFactory.WorldModel, m_script);
                }
            }
            m_script.Parent = m_parent;
            m_scriptString = null;
        }

        public IMutableField Clone()
        {
            if (m_script != null) return m_script.Clone();
            LazyLoadScript result = new LazyLoadScript(m_scriptFactory, m_scriptString, m_scriptContext);
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

        public event EventHandler<IfScriptUpdatedEventArgs> IfScriptUpdated
        {
            add
            {
                Initialise();
                ((IfScript)m_script).IfScriptUpdated += value;
            }
            remove
            {
                Initialise();
                ((IfScript)m_script).IfScriptUpdated -= value;
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

        public IElseIfScript AddElseIf(string expression, IScript script)
        {
            Initialise();
            return ((IfScript)m_script).AddElseIf(expression, script);
        }

        public IElseIfScript AddElseIf(Functions.IFunction<bool> expression, IScript script)
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

        public IEnumerable<IScript> Scripts
        {
            get
            {
                Initialise();
                return ((MultiScript)m_script).Scripts;
            }
        }

        public void Add(params IScript[] scripts)
        {
            Initialise();
            ((MultiScript)m_script).Add(scripts);
        }

        public void Remove(int index)
        {
            Initialise();
            ((MultiScript)m_script).Remove(index);
        }

        public void Swap(int index1, int index2)
        {
            Initialise();
            ((MultiScript)m_script).Swap(index1, index2);
        }

        public void Insert(int index, IScript script)
        {
            Initialise();
            ((MultiScript)m_script).Insert(index, script);
        }

        public void LoadCode(string code)
        {
            Initialise();
            ((MultiScript)m_script).LoadCode(code);
        }

        public IScript ElseScript
        {
            get
            {
                Initialise();
                return ((IfScript)m_script).ElseScript;
            }
        }

        public IList<IElseIfScript> ElseIfScripts
        {
            get
            {
                Initialise();
                return ((IfScript)m_script).ElseIfScripts;
            }
        }

        public string ExpressionString
        {
            get
            {
                Initialise();
                return ((IfScript)m_script).ExpressionString;
            }
            set
            {
                Initialise();
                ((IfScript)m_script).ExpressionString = value;
            }
        }

        public void RemoveElseIf(IElseIfScript elseIfScript)
        {
            Initialise();
            ((IfScript)m_script).RemoveElseIf(elseIfScript);
        }

        public override string ToString()
        {
            if (m_script != null) return m_script.ToString();
            return m_scriptString;
        }
    }
}
