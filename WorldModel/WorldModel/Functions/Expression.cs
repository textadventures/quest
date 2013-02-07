using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ciloci.Flee;
using TextAdventures.Quest.Scripts;

namespace TextAdventures.Quest.Functions
{
    public abstract class ExpressionBase
    {
        protected ScriptContext m_scriptContext;
        protected string m_originalExpression;
        protected string m_expression;
        protected WorldModel m_worldModel;
        protected Dictionary<string, Type> m_types = new Dictionary<string, Type>();

        public ExpressionBase(string expression, ScriptContext scriptContext)
        {
            m_scriptContext = scriptContext;
            m_worldModel = scriptContext.WorldModel;

            m_originalExpression = expression;
            if (!m_worldModel.EditMode)
            {
                expression = Utility.ConvertVariablesToFleeFormat(expression);
            }

            m_expression = expression;
        }

        public override string ToString()
        {
            return "Expression: " + m_originalExpression;
        }
    }

    public class Expression<T> : ExpressionBase, IFunction<T>
    {
        private IGenericExpression<T> m_compiledExpression = null;

        public Expression(string expression, ScriptContext scriptContext)
            : base(expression, scriptContext)
        {
        }

        public IFunction<T> Clone()
        {
            return new Expression<T>(m_expression, m_scriptContext);
        }

        public T Execute(Context c)
        {
            m_scriptContext.ExecutionContext = c;
            if (m_compiledExpression == null || m_scriptContext.HaveVariableTypesChanged(m_compiledExpression.Info.GetReferencedVariables(), m_types))
            {
                // Lazy compilation since when the game is loaded, we don't know what types of
                // variables we have.
                try
                {
                    m_compiledExpression = m_scriptContext.ExpressionContext.CompileGeneric<T>(m_expression);
                    m_scriptContext.PopulateVariableTypesCache(m_compiledExpression.Info.GetReferencedVariables(), m_types);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error compiling expression '{0}': {1}", Utility.ConvertFleeFormatToVariables(m_expression), ex.Message), ex);
                }
            }
            try
            {
                return m_compiledExpression.Evaluate();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error evaluating expression '{0}': {1}", Utility.ConvertFleeFormatToVariables(m_expression), ex.Message), ex);
            }
        }

        public string Save()
        {
            return m_originalExpression;
        }
    }

    public class ExpressionGeneric : ExpressionBase, IFunctionGeneric
    {
        private IDynamicExpression m_compiledExpression = null;

        public ExpressionGeneric(string expression, ScriptContext scriptContext)
            : base(expression, scriptContext)
        {
        }

        public IFunctionGeneric Clone()
        {
            return new ExpressionGeneric(m_expression, m_scriptContext);
        }

        public object Execute(Context c)
        {
            m_scriptContext.ExecutionContext = c;
            if (m_compiledExpression == null || m_scriptContext.HaveVariableTypesChanged(m_compiledExpression.Info.GetReferencedVariables(), m_types))
            {
                // Lazy compilation since when the game is loaded, we don't know what types of
                // variables we have.
                try
                {
                    m_compiledExpression = m_scriptContext.ExpressionContext.CompileDynamic(m_expression);
                    m_scriptContext.PopulateVariableTypesCache(m_compiledExpression.Info.GetReferencedVariables(), m_types);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error compiling expression '{0}': {1}", Utility.ConvertFleeFormatToVariables(m_expression), ex.Message), ex);
                }
            }
            try
            {
                return m_compiledExpression.Evaluate();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error evaluating expression '{0}': {1}", Utility.ConvertFleeFormatToVariables(m_expression), ex.Message), ex);
            }
        }

        public string Save()
        {
            return m_originalExpression;
        }
    }
}