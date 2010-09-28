using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;
using System.Collections;

namespace AxeSoftware.Quest.Scripts
{
    public class DoScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public override string Keyword
        {
            get { return "do"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            switch (parameters.Count)
            {
                case 2:
                    return new DoActionScript(WorldModel, new Expression<Element>(parameters[0]), new Expression<string>(parameters[1]));
                case 3:
                    return new DoActionScript(WorldModel, new Expression<Element>(parameters[0]), new Expression<string>(parameters[1]), new Expression<IDictionary>(parameters[2]));
            }
            return null;
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 2, 3 }; }
        }
        #endregion
    }

    public class RunDelegateScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public override string Keyword
        {
            get { return "rundelegate"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            if (parameters.Count < 3)
            {
                throw new Exception("Expected at least 3 parameters in rundelegate call");
            }

            List<IFunction<object>> paramExpressions = new List<IFunction<object>>();
            IFunction<Element> obj = null;
            int cnt = 0;
            IFunction<string> delegateName = null;

            foreach (string param in parameters)
            {
                cnt++;
                switch (cnt)
                {
                    case 1:
                        obj = new Expression<Element>(param);
                        break;
                    case 2:
                        delegateName = new Expression<string>(param);
                        break;
                    default:
                        paramExpressions.Add(new Expression<object>(param));
                        break;
                }
            }

            return new RunDelegateScript(WorldModel, obj, delegateName, paramExpressions);
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[]{}; }
        }
        #endregion
    }

    public class CallProcedureScriptConstructor : IScriptConstructor
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
                foreach (string s in parameters)
                {
                    paramExpressions.Add(new Expression<object>(s));
                }
            }

            if (WorldModel.Procedure(procName) == null)
            {
                throw new Exception(string.Format("Function not found: '{0}'", procName));
            }
            else
            {
                return new CallProcedureScript(WorldModel, procName, paramExpressions);
            }
        }

        public IScriptFactory ScriptFactory { get; set; }

        public WorldModel WorldModel { get; set; }

        public string Keyword
        {
            get { return null; }
        }
    }

    public class DoActionScript : ScriptBase
    {
        private WorldModel m_worldModel;
        private IFunction<Element> m_obj;
        private IFunction<string> m_action;
        private IFunction<IDictionary> m_parameters = null;

        public DoActionScript(WorldModel worldModel, IFunction<Element> obj, IFunction<string> action)
        {
            m_worldModel = worldModel;
            m_obj = obj;
            m_action = action;
        }

        public DoActionScript(WorldModel worldModel, IFunction<Element> obj, IFunction<string> action, IFunction<IDictionary> parameters)
            : this(worldModel, obj, action)
        {
            m_parameters = parameters;
        }

        #region IScript Members

        public override void Execute(Context c)
        {
            Element obj = m_obj.Execute(c);
            IScript action = obj.GetAction(m_action.Execute(c));
            if (m_parameters == null)
            {
                m_worldModel.RunScript(action, obj);
            }
            else
            {
                m_worldModel.RunScript(action, new Parameters(m_parameters.Execute(c)), obj);
            }
        }

        public override string Save()
        {
            if (m_parameters != null)
            {
                return SaveScript("do", m_obj.Save(), m_action.Save(), m_parameters.Save());
            }
            else
            {
                return SaveScript("do", m_obj.Save(), m_action.Save());
            }
        }

        #endregion
    }

    public class RunDelegateScript : ScriptBase
    {
        private WorldModel m_worldModel;
        private IFunction<string> m_delegate;
        private List<IFunction<object>> m_parameters = null;
        private IFunction<Element> m_appliesTo = null;

        public RunDelegateScript(WorldModel worldModel, IFunction<Element> obj, IFunction<string> del, List<IFunction<object>> parameters)
        {
            m_worldModel = worldModel;
            m_delegate = del;
            m_parameters = parameters;
            m_appliesTo = obj;
        }

        #region IScript Members

        public override void Execute(Context c)
        {
            if (m_parameters == null)
            {
                throw new NotImplementedException();
            }
            else
            {
                Element obj = m_appliesTo.Execute(c);
                string delName = m_delegate.Execute(c);
                DelegateImplementation impl = obj.Fields.Get(delName) as DelegateImplementation;

                if (impl == null)
                {
                    throw new Exception(string.Format("Object '{0}' has no delegate implementation '{1}'", obj.Name, m_delegate));
                }

                Parameters paramValues = new Parameters();

                int cnt = 0;
                foreach (IFunction<object> f in m_parameters)
                {
                    paramValues.Add((string)impl.Definition.Fields[FieldDefinitions.ParamNames][cnt], f.Execute(c));
                    cnt++;
                }

                m_worldModel.RunScript(impl.Implementation.Fields[FieldDefinitions.Script], paramValues, obj);
            }
        }

        public override string Save()
        {
            List<string> saveParameters = new List<string>();
            saveParameters.Add(m_appliesTo.Save());
            saveParameters.Add(m_delegate.Save());
            foreach (IFunction<object> p in m_parameters)
            {
                saveParameters.Add(p.Save());
            }

            return SaveScript("rundelegate", saveParameters.ToArray());
        }

        #endregion
    }

    public class CallProcedureScript : ScriptBase
    {
        private WorldModel m_worldModel;
        private string m_procedure;
        private List<IFunction<object>> m_parameters = null;

        public CallProcedureScript(WorldModel worldModel, string procedure)
        {
            m_worldModel = worldModel;
            m_procedure = procedure;
        }

        public CallProcedureScript(WorldModel worldModel, string procedure, List<IFunction<object>> parameters)
            : this(worldModel, procedure)
        {
            m_parameters = parameters;
        }

        #region IScript Members

        public override void Execute(Context c)
        {
            if (m_parameters == null)
            {
                m_worldModel.RunProcedure(m_procedure);
            }
            else
            {
                Parameters paramValues = new Parameters();
                Element proc = m_worldModel.Procedure(m_procedure);

                // TO DO: Check number of parameters matches, and that the function exists

                int cnt = 0;
                foreach (IFunction<object> f in m_parameters)
                {
                    paramValues.Add((string)proc.Fields[FieldDefinitions.ParamNames][cnt], f.Execute(c));
                    cnt++;
                }

                m_worldModel.RunProcedure(m_procedure, paramValues, false);
            }
        }

        public override string Save()
        {
            if (m_parameters == null)
            {
                return m_procedure;
            }

            List<string> saveParameters = new List<string>();
            foreach (IFunction<object> p in m_parameters)
            {
                saveParameters.Add(p.Save());
            }
            return SaveScript(m_procedure, saveParameters.ToArray());
        }

        #endregion
    }
}
