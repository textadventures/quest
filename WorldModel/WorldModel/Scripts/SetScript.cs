using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class SetScriptConstructor : IScriptConstructor
    {
        #region IScriptConstructor Members

        public string Keyword
        {
            get { return null; }
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
                string var = Utility.ConvertDottedPropertiesToVariable(script.Substring(0, eqPos)).Trim();
                string value = script.Substring(eqPos + 1 + offset).Trim();
                string obj;
                string variable;
                Utility.ResolveVariableName(var, out obj, out variable);
                IFunction<Element> expr = (obj == null) ? null : new Expression<Element>(obj, WorldModel);

                if (!isScript)
                {
                    return new SetScript(WorldModel, expr, variable, new Expression<object>(value, WorldModel));
                }
                else
                {
                    return new SetScript(WorldModel, expr, variable, ScriptFactory.CreateScript(value));
                }
            }

            return null;
        }

        public IScriptFactory ScriptFactory { get; set; }

        public WorldModel WorldModel { get; set; }

        #endregion
    }

    public class SetScript : ScriptBase
    {
        private WorldModel m_worldModel;
        private IFunction<Element> m_appliesTo;
        private string m_property;
        private Expression<object> m_expr = null;
        private IScript m_script = null;

        private SetScript(WorldModel worldModel, IFunction<Element> appliesTo, string property)
        {
            m_worldModel = worldModel;
            m_appliesTo = appliesTo;
            m_property = property;
        }

        public SetScript(WorldModel worldModel, IFunction<Element> appliesTo, string property, Expression<object> expr)
            : this(worldModel, appliesTo, property)
        {
            m_expr = expr;
        }

        public SetScript(WorldModel worldModel, IFunction<Element> appliesTo, string property, IScript script)
            : this(worldModel, appliesTo, property)
        {
            m_script = script;
        }

        #region IScript Members

        public override void Execute(Context c)
        {
            object result;

            if (m_expr != null)
            {
                result = m_expr.Execute(c);
            }
            else
            {
                result = m_script;
            }

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

            if (m_expr != null)
            {
                result += " = " + m_expr.Save();
            }
            else {
                result += " => " + m_script.Save();
            }

            return result;
        }

        #endregion
    }
}
