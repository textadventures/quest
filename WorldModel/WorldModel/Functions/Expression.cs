using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ciloci.Flee;
using AxeSoftware.Quest.Scripts;

namespace AxeSoftware.Quest.Functions
{
    public abstract class ExpressionBase
    {
        protected ExpressionContext m_expressionContext;
        protected Context m_context;
        protected string m_originalExpression;
        protected string m_expression;
        private Dictionary<string, Type> m_types = new Dictionary<string, Type>();
        protected WorldModel m_worldModel;

        public ExpressionBase(string expression, WorldModel worldModel)
        {
            m_worldModel = worldModel;
            m_expressionContext = new ExpressionContext(worldModel.ExpressionOwner);
            m_expressionContext.Imports.AddType(typeof(StringFunctions));

            m_expressionContext.Variables.ResolveVariableType += new EventHandler<ResolveVariableTypeEventArgs>(Variables_ResolveVariableType);
            m_expressionContext.Variables.ResolveVariableValue += new EventHandler<ResolveVariableValueEventArgs>(Variables_ResolveVariableValue);
            m_expressionContext.Variables.ResolveFunction += new EventHandler<ResolveFunctionEventArgs>(Variables_ResolveFunction);
            m_expressionContext.Variables.InvokeFunction += new EventHandler<InvokeFunctionEventArgs>(Variables_InvokeFunction);
            m_expressionContext.Options.ParseCulture = System.Globalization.CultureInfo.InvariantCulture;

            m_originalExpression = expression;
            if (!m_worldModel.EditMode)
            {
                expression = Utility.ConvertDottedPropertiesToVariable(expression);
            }

            // While it would be nice to compile expressions here when the game is loaded,
            // we don't necessarily have all the properties created that an expression may
            // refer to, as properties can be added or changed during the game.
            //m_expression = m_context.CompileGeneric<T>(expression);

            m_expression = expression;
        }

        void Variables_ResolveFunction(object sender, ResolveFunctionEventArgs e)
        {
            Element proc = m_worldModel.Procedure(e.FunctionName);
            if (proc != null)
            {
                e.ReturnType = WorldModel.ConvertTypeNameToType(proc.Fields[FieldDefinitions.ReturnType]);
            }
        }

        void Variables_InvokeFunction(object sender, InvokeFunctionEventArgs e)
        {
            Element proc = m_worldModel.Procedure(e.FunctionName);
            Parameters parameters = new Parameters();
            int cnt = 0;
            // TO DO: Check number of parameters matches
            foreach (object val in e.Arguments)
            {
                parameters.Add((string)proc.Fields[FieldDefinitions.ParamNames][cnt], val);
                cnt++;
            }

            e.Result = m_worldModel.RunProcedure(e.FunctionName, parameters, true);
        }

        void Variables_ResolveVariableValue(object sender, ResolveVariableValueEventArgs e)
        {
            e.VariableValue = ResolveVariable(e.VariableName);
        }

        void Variables_ResolveVariableType(object sender, ResolveVariableTypeEventArgs e)
        {
            Type type = GetVariableType(e.VariableName);
            m_types[e.VariableName] = type;
            e.VariableType = type;
        }

        private Type GetVariableType(string variable)
        {
            object value = ResolveVariable(variable);
            return (value == null) ? typeof(object) : value.GetType();
        }

        protected bool HaveVariableTypesChanged()
        {
            foreach (string variable in m_types.Keys)
            {
                if (GetVariableType(variable) != m_types[variable]) return true;
            }
            return false;
        }

        private object ResolveVariable(string name)
        {
            if (m_context.Parameters != null && m_context.Parameters.ContainsKey(name))
            {
                return m_context.Parameters[name];
            }
            else
            {
                Element result;
                if (m_worldModel.TryResolveExpressionElement(name, out result))
                {
                    return result;
                }
                else
                {
                    Fields fields;
                    string variable;
                    ResolveVariableName(name, out fields, out variable);

                    do
                    {
                        if (Utility.ContainsUnresolvedDotNotation(variable))
                        {
                            // We may have been passed in something like someobj.parent.someproperty
                            string nestedObj;
                            Utility.ResolveVariableName(variable, out nestedObj, out variable);
                            fields = fields.GetObject(nestedObj).Fields;
                        }
                    } while (Utility.ContainsUnresolvedDotNotation(variable));

                    if (!fields.Exists(variable))
                    {
                        return null;
                    }
                    return fields.Get(variable);
                }
            }
        }

        private void ResolveVariableName(string name, out Fields fields, out string variable)
        {
            string obj;
            Utility.ResolveVariableName(name, out obj, out variable);

            Element result;
            if (m_worldModel.TryResolveExpressionElement(name, out result))
            {
                fields = result.Fields;
            }
            else
            {
                if (obj == null) throw new Exception(string.Format("Unknown object or variable '{0}'", name));

                object value = ResolveVariable(obj);
                Element instance = value as Element;
                if (instance == null)
                {
                    throw new Exception(string.Format("Variable does not refer to an object: '{0}'", obj));
                }
                fields = instance.Fields;
            }
        }

        public override string ToString()
        {
            return "Expression: " + m_originalExpression;
        }

    }

    public class Expression<T> : ExpressionBase, IFunction<T>
    {
        private IGenericExpression<T> m_compiledExpression = null;

        public Expression(string expression, WorldModel worldModel)
            : base(expression, worldModel)
        {
        }

        public IFunction<T> Clone()
        {
            return new Expression<T>(m_expression, m_worldModel);
        }

        #region IFunction<T> Members

        public T Execute(Context c)
        {
            m_context = c;
            if (m_compiledExpression == null || HaveVariableTypesChanged())
            {
                // Lazy compilation since when the game is loaded, we don't know what types of
                // variables we have.
                try
                {
                    m_compiledExpression = m_expressionContext.CompileGeneric<T>(m_expression);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error compiling expression '{0}': {1}", Utility.ConvertVariableToDottedProperties(m_expression), ex.Message), ex);
                }
            }
            return m_compiledExpression.Evaluate();
        }

        public string Save()
        {
            return m_originalExpression;
        }

        #endregion
    }

    public class ExpressionGeneric : ExpressionBase, IFunctionGeneric
    {
        private IDynamicExpression m_compiledExpression = null;

        public ExpressionGeneric(string expression, WorldModel worldModel)
            : base(expression, worldModel)
        {
        }

        public IFunctionGeneric Clone()
        {
            return new ExpressionGeneric(m_expression, m_worldModel);
        }

        #region IFunctionGeneric Members

        public object Execute(Context c)
        {
            m_context = c;
            if (m_compiledExpression == null || HaveVariableTypesChanged())
            {
                // Lazy compilation since when the game is loaded, we don't know what types of
                // variables we have.
                m_compiledExpression = m_expressionContext.CompileDynamic(m_expression);
            }
            return m_compiledExpression.Evaluate();
        }

        public string Save()
        {
            return m_originalExpression;
        }

        #endregion
    }
}