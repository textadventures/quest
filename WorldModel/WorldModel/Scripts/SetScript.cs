using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Functions;

namespace TextAdventures.Quest.Scripts
{
    public class SetScriptConstructor : IScriptConstructor
    {
        public string Keyword
        {
            get { return "="; }
        }

        public IScript Create(string script, ScriptContext scriptContext)
        {
            bool isScript = false;
            int offset = 0;
            int eqPos;

            // hide text within string expressions
            string obscuredScript = Utility.ObscureStrings(script);
            int bracePos = obscuredScript.IndexOf('{');
            if (bracePos != -1)
            {
                // only want to look for = and => before any other scripts which may
                // be defined on the same line, for example procedure calls of type
                //     MyProcedureCall (5) { some other script }

                obscuredScript = obscuredScript.Substring(0, bracePos);
            }

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
                IFunction<Element> expr = GetAppliesTo(scriptContext, appliesTo, out variable);

                if (!isScript)
                {
                    return new SetExpressionScript(this, scriptContext, expr, variable, new Expression<object>(value, scriptContext));
                }
                else
                {
                    return new SetScriptScript(this, scriptContext, expr, variable, ScriptFactory.CreateScript(value));
                }
            }

            return null;
        }

        internal IFunction<Element> GetAppliesTo(ScriptContext scriptContext, string value, out string variable)
        {
            string var = Utility.ConvertVariablesToFleeFormat(value).Trim();
            string obj;
            Utility.ResolveObjectDotAttribute(var, out obj, out variable);
            return (obj == null) ? null : new Expression<Element>(obj, scriptContext);
        }

        public IScriptFactory ScriptFactory { get; set; }

        public WorldModel WorldModel { get; set; }
    }

    public abstract class SetScriptBase : ScriptBase
    {
        protected ScriptContext m_scriptContext;
        private WorldModel m_worldModel;
        private IFunction<Element> m_appliesTo;
        private string m_property;
        private SetScriptConstructor m_constructor;

        internal SetScriptBase(SetScriptConstructor constructor, ScriptContext scriptContext, IFunction<Element> appliesTo, string property)
        {
            m_constructor = constructor;
            m_worldModel = constructor.WorldModel;
            m_scriptContext = scriptContext;
            AppliesTo = appliesTo;
            Property = property;
        }

        protected IFunction<Element> AppliesTo
        {
            get { return m_appliesTo; }
            private set
            {
                m_appliesTo = value;
                AddAttributeNameToWorldModel();
            }
        }

        protected string Property
        {
            get { return m_property; }
            private set
            {
                m_property = value;
                AddAttributeNameToWorldModel();
            }
        }

        private void AddAttributeNameToWorldModel()
        {
            if (AppliesTo != null && Property != null)
            {
                m_worldModel.AddAttributeName(Property);
            }
        }

        public override void Execute(Context c)
        {
            object result = GetResult(c);

            if (AppliesTo != null)
            {
                // we're setting an object property
                Element obj = AppliesTo.Execute(c);
                obj.Fields.Set(Property, result);
            }
            else
            {
                // we're setting a local variable
                c.Parameters[Property] = result;
            }
        }

        public override string Save()
        {
            string result;

            if (AppliesTo != null)
            {
                result = AppliesTo.Save() + "." + Property;
            }
            else
            {
                result = Property;
            }

            result += GetEqualsString + GetSaveString();

            return result;
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return AppliesTo == null ? Property : AppliesTo.Save() + "." + Property;
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
                    AppliesTo = m_constructor.GetAppliesTo(m_scriptContext, (string)value, out variable);
                    Property = variable;
                    break;
                case 1:
                    SetValue((string)value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override IEnumerable<string> GetDefinedVariables()
        {
            if (AppliesTo == null)
            {
                // If AppliesTo is null, then this is an expression setting a simple variable value
                return new List<string> { Property };
            }
            return base.GetDefinedVariables();
        }

        protected abstract object GetResult(Context c);
        protected abstract string GetEqualsString { get; }
        protected abstract string GetSaveString();
        protected abstract object GetValue();
        protected abstract void SetValue(string newValue);
        protected WorldModel WorldModel { get { return m_worldModel; } }
        protected SetScriptConstructor Constructor { get { return m_constructor; } }
    }

    public class SetExpressionScript : SetScriptBase
    {
        private Expression<object> m_expr;

        public SetExpressionScript(SetScriptConstructor constructor, ScriptContext scriptContext, IFunction<Element> appliesTo, string property, Expression<object> expr)
            : base(constructor, scriptContext, appliesTo, property)
        {
            m_expr = expr;
        }

        protected override ScriptBase CloneScript()
        {
            return new SetExpressionScript(Constructor, m_scriptContext, AppliesTo == null ? null : AppliesTo.Clone(), Property, (Expression<object>)m_expr.Clone());
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
            m_expr = new Expression<object>(newValue, m_scriptContext);
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

        public SetScriptScript(SetScriptConstructor constructor, ScriptContext scriptContext, IFunction<Element> appliesTo, string property, IScript script)
            : base(constructor, scriptContext, appliesTo, property)
        {
            m_script = script;
            m_scriptFactory = constructor.ScriptFactory;
        }

        protected override ScriptBase CloneScript()
        {
            return new SetScriptScript(Constructor, m_scriptContext, AppliesTo == null ? null : AppliesTo.Clone(), Property, (IScript)m_script.Clone());
        }

        protected override object GetResult(Context c)
        {
            return m_script;
        }

        protected override string GetSaveString()
        {
            return SaveScript("", m_script).Trim();
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
