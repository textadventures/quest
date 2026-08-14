using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
using QuestViva.Engine;
using QuestViva.Engine.Scripts;

namespace QuestViva.EditorCore;

public class EditableScript : EditableScriptBase, IEditableScript
{
    private static readonly Regex s_regex = new("#(?<attribute>\\d+)");
    private readonly List<EditableScripts> m_watchedNestedScripts = new();
    private string m_editorName;

    internal EditableScript(EditorController controller, IScript script, UndoLogger undoLogger)
        : base(controller, script, undoLogger)
    {
        m_editorName = Script.Keyword;
    }

    internal string DisplayTemplate { get; set; }

    public override string DisplayString(int index, object newValue)
    {
        // This version of DisplayString is used while we are editing an attribute value
        // as we don't want to save updates for every keypress

        if (string.IsNullOrEmpty(DisplayTemplate))
        {
            return Script == null ? null : Script.Save();
        }

        var result = DisplayTemplate;
        var startAt = 1;

        while (startAt < result.Length && s_regex.IsMatch(result, startAt))
        {
            var m = s_regex.Match(result);
            var attributeNum = int.Parse(m.Groups["attribute"].Value);
            string attributeValue;

            object value;
            if (attributeNum == index)
            {
                value = newValue;
            }
            else
            {
                value = GetParameter(attributeNum.ToString());
            }

            if (value is IDataWrapper)
            {
                value = ((IDataWrapper) value).GetUnderlyingValue();
            }

            var scriptValue = value as IScript;
            var stringValue = value as string;
            var collectionValue = value as ICollection;

            if (stringValue != null)
            {
                attributeValue = stringValue.Length == 0 ? "?" : stringValue;
            }
            else if (scriptValue != null)
            {
                var editableScripts = EditableScripts.GetInstance(Controller, scriptValue);
                RegisterNestedScriptForUpdates(editableScripts);
                attributeValue = editableScripts.Count == 0 ? "(nothing)" : editableScripts.DisplayString();
            }
            else if (collectionValue != null)
            {
                attributeValue = collectionValue.Count.ToString();
            }
            else if (value == null)
            {
                attributeValue = "(nothing)";
            }
            else
            {
                Debug.Assert(false, "Unknown attribute type for DisplayString");
                attributeValue = "<unknown>";
            }

            result = s_regex.Replace(result, attributeValue, 1, startAt);
            startAt = m.Groups["attribute"].Index + attributeValue.Length;
        }

        return result;
    }

    public override string EditorName
    {
        get => m_editorName;
        set => m_editorName = value;
    }

    public override object GetParameter(string index)
    {
        if (EditorName.StartsWith("(function)"))
        {
            if (index == "script")
            {
                return FunctionCallScript.GetFunctionCallParameterScript();
            }

            return FunctionCallScript.GetFunctionCallParameter(int.Parse(index));
        }

        return Script.GetParameter(int.Parse(index));
    }

    public override void SetParameter(string index, object value)
    {
        var valueToSet = value;
        var wrappedValue = value as IDataWrapper;
        if (wrappedValue != null)
        {
            valueToSet = wrappedValue.GetUnderlyingValue();
        }

        if (EditorName.StartsWith("(function)"))
        {
            if (index == "script")
            {
                FunctionCallScript.SetFunctionCallParameterScript(valueToSet as IScript);
                RaiseUpdated(new EditableScriptUpdatedEventArgs {IsNestedScriptUpdate = true});
                return;
            }

            FunctionCallScript.SetFunctionCallParameter(int.Parse(index), valueToSet);
            return;
        }

        Script.SetParameter(int.Parse(index), valueToSet);
    }

    public override ScriptType Type => ScriptType.Normal;

    private void RegisterNestedScriptForUpdates(EditableScripts script)
    {
        if (!m_watchedNestedScripts.Contains(script))
        {
            script.Updated += nestedScript_Updated;
            m_watchedNestedScripts.Add(script);
        }
    }

    private void nestedScript_Updated(object sender, EditableScriptsUpdatedEventArgs e)
    {
        RaiseUpdateForNestedScriptChange(e);
    }
}