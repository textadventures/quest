using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class SetScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "="; }
        }

        public IScript Create(string script, Element proc)
        {
            bool isScript = false;
            int offset = 0;
            int eqPos;

            // hide text within string expressions
            string obscuredScript = Utility.ObscureStrings(script);

            eqPos = obscuredScript.IndexOf("=>");
            if (eqPos != -1)
            {
                isScript = true;
                offset = 1;
            }
            else
            {
                eqPos = obscuredScript.IndexOf('=');
            }

            if (eqPos != -1)
            {
                string appliesTo = script.Substring(0, eqPos);
                string value = script.Substring(eqPos + 1 + offset).Trim();

                string variable;
                IFunction<Element> expr = GetAppliesTo(appliesTo, out variable);

                if (!isScript)
                {
                    return new SetExpressionScript(this, expr, variable, new Expression<object>(value, WorldModel));
                }
                else
                {
                    return new SetScriptScript(this, expr, variable, ScriptFactory.CreateScript(value));
                }
            }

            return null;
        }

        internal IFunction<Element> GetAppliesTo(string value, out string variable)
        {
            string var = Utility.ConvertDottedPropertiesToVariable(value).Trim();
            string obj;
            Utility.ResolveVariableName(var, out obj, out variable);
            return (obj == null) ? null : new Expression<Element>(obj, WorldModel);
        }

        public IScriptFactory ScriptFactory { get; set; }

        public WorldModel WorldModel { get; set; }
    }

    public abstract class SetScriptBase : ScriptBase
    {
        private WorldModel m_worldModel;
        private IFunction<Element> m_appliesTo;
        private string m_property;
        private SetScriptConstructor m_constructor;

        internal SetScriptBase(SetScriptConstructor constructor, IFunction<Element> appliesTo, string property)
        {
            m_constructor = constructor;
            m_worldModel = constructor.WorldModel;
            m_appliesTo = appliesTo;
            m_property = property;
        }

        public override void Execute(Context c)
        {
            object result = GetResult(c);

            if (m_appliesTo != null)
            {
                // we're setting an object property
                Element obj = m_appliesTo.Execute(c);
                obj.Fields.Set(m_property, result);
            }
            else
            {
                // we're setting a local variable
                c.Parameters[m_property] = result;
            }
        }

        public override string Save()
        {
            string result;

            if (m_appliesTo != null)
            {
                result = m_appliesTo.Save() + "." + m_property;
            }
            else
            {
                result = m_property;
            }

            result += GetEqualsString + GetSaveString();

            return result;
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_appliesTo == null ? m_property : m_appliesTo.Save() + "." + m_property;
                case 1:
                    return GetValue();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void SetParameterInternal(int index, object value)
        {
            switch (index)
            {
                case 0:
                    string variable;
                    m_appliesTo = m_constructor.GetAppliesTo((string)value, out variable);
                    m_property = variable;
                    break;
                case 1:
                    SetValue((string)value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected abstract object GetResult(Context c);
        protected abstract string GetEqualsString { get; }
        protected abstract string GetSaveString();
        protected abstract object GetValue();
        protected abstract void SetValue(string newValue);
        protected WorldModel WorldModel { get { return m_worldModel; } }
        protected SetScriptConstructor Constructor { get { return m_constructor; } }
        protected IFunction<Element> AppliesTo { get { return m_appliesTo; } }
        protected string Property { get { return m_property; } }
    }

    public class SetExpressionScript : SetScriptBase
    {
        private Expression<object> m_expr;

        public SetExpressionScript(SetScriptConstructor constructor, IFunction<Element> appliesTo, string property, Expression<object> expr)
            : base(constructor, appliesTo, property)
        {
            m_expr = expr;
        }

        protected override ScriptBase CloneScript()
        {
            return new SetExpressionScript(Constructor, AppliesTo == null ? null : AppliesTo.Clone(), Property, (Expression<object>)m_expr.Clone());
        }

        protected override object GetResult(Context c)
        {
            return m_expr.Execute(c);
        }

        protected override string GetSaveString()
        {
            return m_expr.Save();
        }

        protected override string GetEqualsString
        {
            get { return " = "; }
        }

        protected override void SetValue(string newValue)
        {
            m_expr = new Expression<object>(newValue, WorldModel);
        }

        public override string Keyword { get { return "="; } }

        protected override object GetValue()
        {
            return m_expr.Save();
        }
    }

    public class SetScriptScript : SetScriptBase
    {
        private IScript m_script;
        private IScriptFactory m_scriptFactory;

        public SetScriptScript(SetScriptConstructor constructor, IFunction<Element> appliesTo, string property, IScript script)
            : base(constructor, appliesTo, property)
        {
            m_script = script;
            m_scriptFactory = constructor.ScriptFactory;
        }

        protected override ScriptBase CloneScript()
        {
            return new SetScriptScript(Constructor, AppliesTo.Clone(), Property, (IScript)m_script.Clone());
        }

        protected override object GetResult(Context c)
        {
            return m_script;
        }

        protected override string GetSaveString()
        {
            return m_script.Save();
        }

        protected override string GetEqualsString
        {
            get { return " => "; }
        }

        protected override void SetValue(string newValue)
        {
            m_script = m_scriptFactory.CreateScript(newValue);
        }

        public override string Keyword { get { return "=>"; } }

        protected override object GetValue()
        {
            return m_script;
        }
    }
}
