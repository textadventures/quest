#nullable disable
using QuestViva.Engine.Expressions;
using QuestViva.Engine.Scripts;

namespace QuestViva.Engine.Functions
{
    public abstract class ExpressionBase
    {
        protected ScriptContext m_scriptContext;
        protected string m_originalExpression;
        protected string m_expression;
        private readonly WorldModel m_worldModel;

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
        private readonly IExpressionEvaluator<T> _expressionEvaluator;

        public Expression(string expression, ScriptContext scriptContext)
            : base(expression, scriptContext)
        {
            if (scriptContext.WorldModel.UseNcalc)
            {
                _expressionEvaluator = new NcalcExpressionEvaluator<T>(m_expression, m_scriptContext);
            }
            else
            {
                _expressionEvaluator = new FleeExpressionEvaluator<T>(m_expression, m_scriptContext);
            }
        }

        public IFunction<T> Clone()
        {
            return new Expression<T>(m_expression, m_scriptContext);
        }

        public T Execute(Context c)
        {
            return _expressionEvaluator.Evaluate(c);
        }

        public string Save()
        {
            return m_originalExpression;
        }
    }

    public class ExpressionDynamic : ExpressionBase, IFunctionDynamic
    {
        private readonly IDynamicExpressionEvaluator _dynamicExpressionEvaluator;

        public ExpressionDynamic(string expression, ScriptContext scriptContext)
            : base(expression, scriptContext)
        {
            if (scriptContext.WorldModel.UseNcalc)
            {
                _dynamicExpressionEvaluator = new NcalcExpressionEvaluator<object>(m_expression, m_scriptContext);
            }
            else
            {
                _dynamicExpressionEvaluator = new FleeDynamicExpressionEvaluator(m_expression, m_scriptContext);    
            }
        }

        public IFunctionDynamic Clone()
        {
            return new ExpressionDynamic(m_expression, m_scriptContext);
        }

        public object Execute(Context c)
        {
            return _dynamicExpressionEvaluator.Evaluate(c);
        }

        public string Save()
        {
            return m_originalExpression;
        }
    }
}