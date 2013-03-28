using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public class FunctionCallScriptConstructor : IScriptConstructor
    {
        public IScript Create(string script, ScriptContext scriptContext)
        {
            List<IFunction<object>> paramExpressions = null;
            string procName, afterParameter;

            string param = Utility.GetParameter(script, out afterParameter);
            IScript paramScript = null;

            // Handle functions of the form
            //    SomeFunction (parameter) { script }
            if (afterParameter != null)
            {
                afterParameter = afterParameter.Trim();
                if (afterParameter.Length > 0)
                {
                    string paramScriptString = Utility.GetScript(afterParameter);
                    paramScript = ScriptFactory.CreateScript(paramScriptString);
                }
            }

            if (param == null && paramScript == null)
            {
                procName = script;
            }
            else
            {
                if (param != null)
                {
                    List<string> parameters = Utility.SplitParameter(param);
                    procName = script.Substring(0, script.IndexOf('(')).Trim();
                    paramExpressions = new List<IFunction<object>>();
                    if (param.Trim().Length > 0)
                    {
                        foreach (string s in parameters)
                        {
                            paramExpressions.Add(new Expression<object>(s, scriptContext));
                        }
                    }
                }
                else
                {
                    procName = script.Substring(0, script.IndexOfAny(new char[] { '{', ' ' }));
                }
            }

            if (!WorldModel.EditMode && WorldModel.Procedure(procName) == null)
            {
                throw new Exception(string.Format("Function not found: '{0}'", procName));
            }
            else
            {
                return new FunctionCallScript(WorldModel, procName, paramExpressions, paramScript);
            }
        }

        public IScriptFactory ScriptFactory { get; set; }

        public WorldModel WorldModel { get; set; }

        public string Keyword
        {
            get { return null; }
        }
    }

    public class FunctionCallScript : ScriptBase, IFunctionCallScript
    {
        private WorldModel m_worldModel;
        private string m_procedure;
        private FunctionCallParameters m_parameters;
        private IScript m_paramFunction;

        public event EventHandler<ScriptUpdatedEventArgs> FunctionCallParametersUpdated;

        public FunctionCallScript(WorldModel worldModel, string procedure)
            : this(worldModel, procedure, null, null)
        {
        }

        public FunctionCallScript(WorldModel worldModel, string procedure, IList<IFunction<object>> parameters, IScript paramFunction)
        {
            m_worldModel = worldModel;
            m_procedure = procedure;
            m_parameters = new FunctionCallParameters(worldModel, parameters);
            m_paramFunction = paramFunction;

            m_parameters.ParametersAsQuestList.Added += Parameters_Added;
            m_parameters.ParametersAsQuestList.Removed += Parameters_Removed;
        }

        void Parameters_Added(object sender, QuestListUpdatedEventArgs<string> e)
        {
            // the number of parameters in a function call cannot change. So, as QuestList doesn't
            // provide an Updated event (we simulate Updates with a Remove and an Add at the same
            // index), we assume that any Added event is really an update.

            FunctionCallParametersUpdated(this, new ScriptUpdatedEventArgs(e.Index, e.UpdatedItem));
        }

        void Parameters_Removed(object sender, QuestListUpdatedEventArgs<string> e)
        {
            // the only time we care about a parameter being removed is if it's the first parameter being
            // deleted. Everything else should simply be a Remove followed by an Add, and we handle the
            // Add above.
            if (e.Index == 0)
            {
                FunctionCallParametersUpdated(this, new ScriptUpdatedEventArgs(e.Index, string.Empty));
            }
        }

        protected override ScriptBase CloneScript()
        {
            return new FunctionCallScript(m_worldModel, m_procedure, m_parameters == null ? null : m_parameters.Parameters, m_paramFunction);
        }

        public override void Execute(Context c)
        {
            if ((m_parameters.Parameters == null || m_parameters.Parameters.Count == 0) && m_paramFunction == null)
            {
                m_worldModel.RunProcedure(m_procedure);
            }
            else
            {
                Parameters paramValues = new Parameters();
                Element proc = m_worldModel.Procedure(m_procedure);

                QuestList<string> paramNames = proc.Fields[FieldDefinitions.ParamNames];

                int paramCount = m_parameters.Parameters.Count;
                if (m_paramFunction != null) paramCount++;

                if (paramCount > paramNames.Count)
                {
                    throw new Exception(string.Format("Too many parameters passed to {0} function - {1} passed, but only {2} expected",
                        m_procedure,
                        paramCount,
                        paramNames.Count));
                }

                if (m_worldModel.Version >= WorldModelVersion.v520)
                {
                    // Only check for too few parameters for games for Quest 5.2 or later, as previous Quest versions
                    // would ignore this (but would usually still fail when the function was run, as the required
                    // variable wouldn't exist)

                    if (paramCount < paramNames.Count)
                    {
                        throw new Exception(string.Format("Too few parameters passed to {0} function - only {1} passed, but {2} expected",
                            m_procedure,
                            paramCount,
                            paramNames.Count));
                    }
                }

                int cnt = 0;
                foreach (IFunction<object> f in m_parameters.Parameters)
                {
                    paramValues.Add((string)paramNames[cnt], f.Execute(c));
                    cnt++;
                }

                if (m_paramFunction != null)
                {
                    paramValues.Add((string)paramNames[cnt], m_paramFunction);
                }

                m_worldModel.RunProcedure(m_procedure, paramValues, false);
            }
        }

        public override string Keyword
        {
            get
            {
                return "(function)" + m_procedure;
            }
        }

        public override string Save()
        {
            if (m_worldModel.Procedure(m_procedure) == null)
            {
                // TO DO: this is the wrong place to be throwing an exception, because Save may be called while editing a script,
                // and maybe the user simply hasn't created their function yet. Maybe instead we should append to a list of warnings
                // when doing an actual File Save, then we can display any warnings after saving.
                //throw new Exception(string.Format("Unable to save call to function '{0}' - function does not exist", m_procedure));
            }

            if ((m_parameters == null || m_parameters.ParametersAsQuestList.Count == 0) && m_paramFunction == null)
            {
                return m_procedure;
            }

            List<string> saveParameters = new List<string>();
            foreach (string p in m_parameters.ParametersAsQuestList)
            {
                saveParameters.Add(p);
            }

            if (m_paramFunction == null)
            {
                return SaveScript(m_procedure, saveParameters.ToArray());
            }
            else
            {
                if (saveParameters.Count > 0)
                {
                    return SaveScript(m_procedure, m_paramFunction, saveParameters.ToArray());
                }
                else
                {
                    return SaveScript(m_procedure + "()", m_paramFunction);
                }
            }
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_procedure;
                case 1:
                    return m_parameters.ParametersAsQuestList;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void SetParameterInternal(int index, object value)
        {
            switch (index)
            {
                case 0:
                    m_procedure = (string)value;
                    break;
                case 1:
                    // any updates to the parameters should change the list itself - nothing should cause SetParameter to be triggered.
                    throw new InvalidOperationException("Attempt to use SetParameter to change the parameters of a function call");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object GetFunctionCallParameter(int index)
        {
            if (index >= m_parameters.ParametersAsQuestList.Count)
            {
                // In the editor, when a blank function call is created, it will have no parameters, but
                // if the editor requests a first parameter then we want to return a blank default instead
                // of throwing an error.
                return "";
            }
            return m_parameters.ParametersAsQuestList[index];
        }

        public void SetFunctionCallParameter(int index, object value)
        {
            if (index < m_parameters.ParametersAsQuestList.Count)
            {
                // In the editor, when a blank function call is created, it will have no parameters
                m_parameters.ParametersAsQuestList.Remove(m_parameters.ParametersAsQuestList[index], UpdateSource.User, index);
            }
            m_parameters.ParametersAsQuestList.Add(value, UpdateSource.User, index);
        }

        public IScript GetFunctionCallParameterScript()
        {
            return m_paramFunction;
        }
    }
}
