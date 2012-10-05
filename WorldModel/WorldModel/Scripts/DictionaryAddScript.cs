using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;
using System.Collections;

namespace TextAdventures.Quest.Scripts
{
    public class DictionaryAddScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "dictionary add"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            return new DictionaryAddScript(scriptContext,
                new ExpressionGeneric(parameters[0], scriptContext),
                new Expression<string>(parameters[1], scriptContext),
                new Expression<object>(parameters[2], scriptContext));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 3 }; }
        }
    }

    public class DictionaryAddScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private IFunctionGeneric m_dictionary;
        private IFunction<string> m_key;
        private IFunction<object> m_value;
        private WorldModel m_worldModel;

        public DictionaryAddScript(ScriptContext scriptContext, IFunctionGeneric dictionary, IFunction<string> key, IFunction<object> value)
        {
            m_scriptContext = scriptContext;
            m_dictionary = dictionary;
            m_key = key;
            m_value = value;
            m_worldModel = scriptContext.WorldModel;
        }

        protected override ScriptBase CloneScript()
        {
            return new DictionaryAddScript(m_scriptContext, m_dictionary.Clone(), m_key.Clone(), m_value.Clone());
        }

        public override void Execute(Context c)
        {
            IDictionary result = m_dictionary.Execute(c) as IDictionary;

            if (result != null)
            {
                result.Add(m_key.Execute(c), m_value.Execute(c));
            }
            else
            {
                throw new Exception("Unrecognised dictionary type");
            }
        }

        public override string Save()
        {
            return SaveScript("dictionary add", m_dictionary.Save(), m_key.Save(), m_value.Save());
        }

        public override string Keyword
        {
            get
            {
                return "dictionary add";
            }
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_dictionary.Save();
                case 1:
                    return m_key.Save();
                case 2:
                    return m_value.Save();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void SetParameterInternal(int index, object value)
        {
            switch (index)
            {
                case 0:
                    m_dictionary = new ExpressionGeneric((string)value, m_scriptContext);
                    break;
                case 1:
                    m_key = new Expression<string>((string)value, m_scriptContext);
                    break;
                case 2:
                    m_value = new Expression<object>((string)value, m_scriptContext);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class DictionaryRemoveScriptConstructor : ScriptConstructorBase
    {
        public override string Keyword
        {
            get { return "dictionary remove"; }
        }

        protected override IScript CreateInt(List<string> parameters, ScriptContext scriptContext)
        {
            return new DictionaryRemoveScript(scriptContext,
                new ExpressionGeneric(parameters[0], scriptContext),
                new Expression<string>(parameters[1], scriptContext));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 2 }; }
        }
    }

    public class DictionaryRemoveScript : ScriptBase
    {
        private ScriptContext m_scriptContext;
        private IFunctionGeneric m_dictionary;
        private IFunction<string> m_key;
        private WorldModel m_worldModel;

        public DictionaryRemoveScript(ScriptContext scriptContext, IFunctionGeneric dictionary, IFunction<string> key)
        {
            m_scriptContext = scriptContext;
            m_dictionary = dictionary;
            m_key = key;
            m_worldModel = scriptContext.WorldModel;
        }

        protected override ScriptBase CloneScript()
        {
            return new DictionaryRemoveScript(m_scriptContext, m_dictionary.Clone(), m_key.Clone());
        }

        public override void Execute(Context c)
        {
            IDictionary result = m_dictionary.Execute(c) as IDictionary;

            if (result != null)
            {
                result.Remove(m_key.Execute(c));
            }
            else
            {
                throw new Exception("Unrecognised dictionary type");
            }
        }

        public override string Save()
        {
            return SaveScript("dictionary remove", m_dictionary.Save(), m_key.Save());
        }

        public override string Keyword
        {
            get
            {
                return "dictionary remove";
            }
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_dictionary.Save();
                case 1:
                    return m_key.Save();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void SetParameterInternal(int index, object value)
        {
            switch (index)
            {
                case 0:
                    m_dictionary = new ExpressionGeneric((string)value, m_scriptContext);
                    break;
                case 1:
                    m_key = new Expression<string>((string)value, m_scriptContext);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
