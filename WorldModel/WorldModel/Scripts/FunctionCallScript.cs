using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class FunctionCallScriptConstructor : IScriptConstructor
    {
        public IScript Create(string script, Element proc)
        {
            List<IFunction<object>> paramExpressions = null;
            string procName;

            string param = Utility.GetParameter(script);
            if (param == null)
            {
                procName = script;
            }
            else
            {
                List<string> parameters = Utility.SplitParameter(param);
                procName = script.Substring(0, script.IndexOf('(')).Trim();
                paramExpressions = new List<IFunction<object>>();
                if (param.Trim().Length > 0)
                {
                    foreach (string s in parameters)
                    {
                        paramExpressions.Add(new Expression<object>(s, WorldModel));
                    }
                }
            }

            if (WorldModel.Procedure(procName) == null)
            {
                throw new Exception(string.Format("Function not found: '{0}'", procName));
            }
            else
            {
                return new FunctionCallScript(WorldModel, procName, paramExpressions);
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

        public event EventHandler<ScriptUpdatedEventArgs> FunctionCallParametersUpdated;

        public FunctionCallScript(WorldModel worldModel, string procedure)
            : this(worldModel, procedure, null)
        {
        }

        public FunctionCallScript(WorldModel worldModel, string procedure, IList<IFunction<object>> parameters)
        {
            m_worldModel = worldModel;
            m_procedure = procedure;
            m_parameters = new FunctionCallParameters(worldModel, parameters);

            m_parameters.ParametersAsQuestList.Added += Parameters_Added;
        }

        void Parameters_Added(object sender, QuestListUpdatedEventArgs<string> e)
        {
            // the number of parameters in a function call cannot change. So, as QuestList doesn't
            // provide an Updated event (we simulate Updates with a Remove and an Add at the same
            // index), we assume that any Added event is really an update.

            FunctionCallParametersUpdated(this, new ScriptUpdatedEventArgs(e.Index, e.UpdatedItem));
        }

        protected override ScriptBase CloneScript()
        {
            return new FunctionCallScript(m_worldModel, m_procedure, m_parameters == null ? null : m_parameters.Parameters);
        }

        public override void Execute(Context c)
        {
            if (m_parameters.Parameters == null || m_parameters.Parameters.Count == 0)
            {
                m_worldModel.RunProcedure(m_procedure);
            }
            else
            {
                Parameters paramValues = new Parameters();
                Element proc = m_worldModel.Procedure(m_procedure);

                // TO DO: Check number of parameters matches, and that the function exists

                int cnt = 0;
                foreach (IFunction<object> f in m_parameters.Parameters)
                {
                    paramValues.Add((string)proc.Fields[FieldDefinitions.ParamNames][cnt], f.Execute(c));
                    cnt++;
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
                throw new Exception(string.Format("Unable to save call to function '{0}' - function does not exist", m_procedure));
            }

            if (m_parameters == null || m_parameters.ParametersAsQuestList.Count == 0)
            {
                return m_procedure;
            }

            List<string> saveParameters = new List<string>();
            foreach (string p in m_parameters.ParametersAsQuestList)
            {
                saveParameters.Add(p);
            }
            return SaveScript(m_procedure, saveParameters.ToArray());
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
            return m_parameters.ParametersAsQuestList[index];
        }

        public void SetFunctionCallParameter(int index, object value)
        {
            m_parameters.ParametersAsQuestList.Remove(m_parameters.ParametersAsQuestList[index], UpdateSource.User, index);
            m_parameters.ParametersAsQuestList.Add(value, UpdateSource.User, index);
        }
    }
}
