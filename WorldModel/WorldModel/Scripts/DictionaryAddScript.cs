using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;
using System.Collections;

namespace AxeSoftware.Quest.Scripts
{
    public class DictionaryAddScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public DictionaryAddScriptConstructor()
        {
        }

        public override string Keyword
        {
            get { return "dictionary add"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new DictionaryAddScript(new ExpressionGeneric(parameters[0], WorldModel), new Expression<string>(parameters[1], WorldModel), new Expression<object>(parameters[2], WorldModel));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 3 }; }
        }
        #endregion
    }

    public class DictionaryAddScript : ScriptBase
    {
        private IFunctionGeneric m_dictionary;
        private IFunction<string> m_key;
        private IFunction<object> m_value;

        public DictionaryAddScript(IFunctionGeneric dictionary, IFunction<string> key, IFunction<object> value)
        {
            m_dictionary = dictionary;
            m_key = key;
            m_value = value;
        }

        #region IScript Members

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

        #endregion
    }

    public class DictionaryRemoveScriptConstructor : ScriptConstructorBase
    {
        #region ScriptConstructorBase Members

        public DictionaryRemoveScriptConstructor()
        {
        }

        public override string Keyword
        {
            get { return "dictionary remove"; }
        }

        protected override IScript CreateInt(List<string> parameters)
        {
            return new DictionaryRemoveScript(new ExpressionGeneric(parameters[0], WorldModel), new Expression<string>(parameters[1], WorldModel));
        }

        protected override int[] ExpectedParameters
        {
            get { return new int[] { 2 }; }
        }
        #endregion
    }

    public class DictionaryRemoveScript : ScriptBase
    {
        private IFunctionGeneric m_dictionary;
        private IFunction<string> m_key;

        public DictionaryRemoveScript(IFunctionGeneric dictionary, IFunction<string> key)
        {
            m_dictionary = dictionary;
            m_key = key;
        }

        #region IScript Members

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

        #endregion
    }
}
