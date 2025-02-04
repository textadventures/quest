using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public class RunDelegateScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "rundelegate"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            if (parameters.Count < 2)
            {
                throw new Exception("Expected at least 2 parameters in rundelegate call");
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
                        obj = new Expression<Element>(param, scriptContext);
                        break;
                    case 2:
                        delegateName = new Expression<string>(param, scriptContext);
                        break;
                    default:
                        paramExpressions.Add(new Expression<object>(param, scriptContext));
                        break;
                }
            }

            return new RunDelegateScript(scriptContext, obj, delegateName, paramExpressions);
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { }; }
        }
    }

    public class RunDelegateScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private WorldModel m_worldModel;
        private IFunction<string> m_delegate;
        private FunctionCallParameters m_parameters;
        private IFunction<Element> m_appliesTo = null;

        public RunDelegateScript(ScriptContext scriptContext, IFunction<Element> obj, IFunction<string> del, IList<IFunction<object>> parameters)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;
            m_delegate = del;
            m_parameters = new FunctionCallParameters(m_worldModel, parameters);
            m_appliesTo = obj;
        }

        protected override ScriptBase CloneScript()
        {
            return new RunDelegateScript(m_scriptContext, m_appliesTo.Clone(), m_delegate.Clone(), m_parameters.Parameters);
        }

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
                foreach (IFunction<object> f in m_parameters.Parameters)
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
            foreach (string p in m_parameters.ParametersAsQuestList)
            {
                saveParameters.Add(p);
            }

            return SaveScript("rundelegate", saveParameters.ToArray());
        }

        public override string Keyword
        {
            get
            {
                return "rundelegate";
            }
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_appliesTo.Save();
                case 1:
                    return m_delegate.Save();
                case 2:
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
                    m_appliesTo = new Expression<Element>((string)value, m_scriptContext);
                    break;
                case 1:
                    m_delegate = new Expression<string>((string)value, m_scriptContext);
                    break;
                case 2:
                    // any updates to the parameters should change the list itself - nothing should cause SetParameter to be triggered.
                    throw new InvalidOperationException("Attempt to use SetParameter to change the parameters of a 'rundelegate'");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
