using System;
using System.Collections.Generic;
using System.Linq;
using Ciloci.Flee;
using QuestViva.Engine.Functions;
using QuestViva.Engine.Scripts;

namespace QuestViva.Engine.Expressions;

public class FleeExpressionContext
{
    private readonly WorldModel _worldModel;

    public FleeExpressionContext(WorldModel worldModel)
    {
        _worldModel = worldModel;
        
        ExpressionContext = new ExpressionContext(_worldModel.ExpressionOwner);
        ExpressionContext.Imports.AddType(typeof(StringFunctions));
        ExpressionContext.Imports.AddType(typeof(Math));
        ExpressionContext.Imports.AddType(typeof(DateTimeFunctions));

        ExpressionContext.Variables.ResolveVariableType += Variables_ResolveVariableType;
        ExpressionContext.Variables.ResolveVariableValue += Variables_ResolveVariableValue;
        ExpressionContext.Variables.ResolveFunction += Variables_ResolveFunction;
        ExpressionContext.Variables.InvokeFunction += Variables_InvokeFunction;
        ExpressionContext.Options.ParseCulture = System.Globalization.CultureInfo.InvariantCulture;
    }

    private void Variables_ResolveFunction(object sender, ResolveFunctionEventArgs e)
    {
        if (e.FunctionName == "IsDefined")
        {
            e.ReturnType = typeof(bool);
            return;
        }
        var proc = _worldModel.Procedure(e.FunctionName);
        if (proc != null)
        {
            e.ReturnType = WorldModel.ConvertTypeNameToType(proc.Fields[FieldDefinitions.ReturnType]);
        }
    }

    private void Variables_InvokeFunction(object sender, InvokeFunctionEventArgs e)
    {
        if (e.FunctionName == "IsDefined")
        {
            e.Result = ExecutionContext.Parameters.ContainsKey((string)e.Arguments[0]);
            return;
        }
        var proc = _worldModel.Procedure(e.FunctionName);
        var parameters = new Parameters();
        var cnt = 0;

        foreach (var val in e.Arguments)
        {
            parameters.Add((string)proc.Fields[FieldDefinitions.ParamNames][cnt], val);
            cnt++;
        }

        e.Result = _worldModel.RunProcedure(e.FunctionName, parameters, true);
    }

    private void Variables_ResolveVariableValue(object sender, ResolveVariableValueEventArgs e)
    {
        e.VariableValue = ResolveVariable(e.VariableName);
    }

    private void Variables_ResolveVariableType(object sender, ResolveVariableTypeEventArgs e)
    {
        var type = GetVariableType(e.VariableName);
        e.VariableType = type;
    }

    private Type GetVariableType(string variable)
    {
        var value = ResolveVariable(variable);
        return (value == null) ? typeof(object) : value.GetType();
    }

    public bool HaveVariableTypesChanged(string[] variables, Dictionary<string, Type> typesCache)
    {
        return variables.Any(variable => GetVariableType(variable) != typesCache[variable]);
    }

    public void PopulateVariableTypesCache(string[] variables, Dictionary<string, Type> typesCache)
    {
        foreach (var variable in variables)
        {
            typesCache[variable] = GetVariableType(variable);
        }
    }

    private object ResolveVariable(string name)
    {
        if (ExecutionContext.Parameters?.ContainsKey(name) == true)
        {
            return ExecutionContext.Parameters[name];
        }

        if (_worldModel.TryResolveExpressionElement(Utility.ResolveElementName(name), out var result))
        {
            return result;
        }

        ResolveVariableName(name, out var fields, out var variable);

        do
        {
            if (!Utility.ContainsUnresolvedDotNotation(variable))
            {
                continue;
            }

            // We may have been passed in something like someobj.parent.someproperty
            Utility.ResolveVariableName(ref variable, out var nestedObj, out variable);
            fields = fields.GetObject(nestedObj).Fields;
        } while (Utility.ContainsUnresolvedDotNotation(variable));

        return !fields.Exists(variable, true) ? null : fields.Get(variable);
    }

    private void ResolveVariableName(string name, out Fields fields, out string variable)
    {
        Utility.ResolveVariableName(ref name, out var obj, out variable);

        if (_worldModel.TryResolveExpressionElement(name, out var result))
        {
            fields = result.Fields;
        }
        else
        {
            if (obj == null) throw new Exception($"Unknown object or variable '{name}'");

            var value = ResolveVariable(obj);
            if (value is not Element instance)
            {
                throw new Exception($"Variable does not refer to an object: '{obj}'");
            }
            fields = instance.Fields;
        }
    }
    
    public Context ExecutionContext { get; set; }
    public ExpressionContext ExpressionContext { get; }
}