using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.WasmEditor;

internal record TreeNodeData(string Key, string Text, string? Parent);
internal record ControlOption(string Value, string Label);
internal record ControlInfo(string? Attribute, string ControlType, string? Caption, List<ControlOption>? Options);
internal record TabInfo(string? Caption, List<ControlInfo> Controls);
internal record EditorDataResponse(Dictionary<string, string?> Attributes, List<TabInfo> Tabs, List<ControlInfo> Controls);

internal record ScriptControlData(
    string ControlType,
    string? Caption,
    string? Attribute,
    string? Value,
    string? SimpleEditor,
    List<ControlOption>? Options,
    List<ScriptNodeData>? Scripts
);
internal record ElseIfClauseData(string Id, string Expression, List<ScriptNodeData> Scripts);
internal record ScriptNodeData(
    string Id,
    string Type,
    string? DisplayString,
    List<ScriptControlData>? Controls,
    string? Expression,
    List<ScriptNodeData>? ThenScripts,
    List<ElseIfClauseData>? ElseIfClauses,
    List<ScriptNodeData>? ElseScripts
);
internal record ScriptBlockData(List<ScriptNodeData> Scripts);
internal record ScriptCommandInfo(string Keyword, string Display, string Add, string CreateString);
internal record ScriptCategoryInfo(string Name, List<ScriptCommandInfo> Commands);
internal record ScriptCommandCategoriesData(List<ScriptCategoryInfo> Categories);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(List<TreeNodeData>))]
[JsonSerializable(typeof(EditorDataResponse))]
[JsonSerializable(typeof(ScriptBlockData))]
[JsonSerializable(typeof(ScriptCommandCategoriesData))]
internal partial class WasmEditorJsonContext : JsonSerializerContext { }

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class WasmEditorBridge
{
    private static EditorController? _controller;
    private static readonly List<TreeNodeData> TreeNodes = [];

    [JSExport]
    public static async Task<bool> Initialise(byte[] gameFileBytes, string filename)
    {
        _controller?.Dispose();
        TreeNodes.Clear();

        _controller = new EditorController();
        _controller.ClearTree += (_, _) => { };
        _controller.BeginTreeUpdate += (_, _) => { };
        _controller.EndTreeUpdate += (_, _) => { };
        _controller.AddedNode += OnAddedNode;
        // RetitledNode and RemovedNode are called without null guards in EditorController,
        // so they must be subscribed even if we don't act on them yet.
        _controller.RetitledNode += (_, _) => { };
        _controller.RemovedNode += (_, _) => { };
        _controller.RenamedNode += (_, _) => { };

        var provider = new ByteArrayGameDataProvider(gameFileBytes, filename);
        var ok = await _controller.Initialise(new WasmConfig(), provider);
        if (ok) _controller.UpdateTree();
        return ok;
    }

    [JSExport]
    public static string GetTreeNodes() =>
        JsonSerializer.Serialize(TreeNodes, WasmEditorJsonContext.Default.ListTreeNodeData);

    [JSExport]
    public static string? GetEditorData(string key)
    {
        if (_controller == null) return null;
        var data = _controller.GetEditorData(key);
        if (data is not IEditorDataExtendedAttributeInfo extended) return null;

        var attrs = new Dictionary<string, string?>();
        foreach (var attr in extended.GetAttributeData())
        {
            attrs[attr.AttributeName] = data.GetAttribute(attr.AttributeName)?.ToString();
        }

        var tabs = new List<TabInfo>();
        var topControls = new List<ControlInfo>();

        var editorName = _controller.GetElementEditorName(key);
        if (editorName != null)
        {
            var def = _controller.GetEditorDefinition(editorName);
            foreach (var tab in def.Tabs.Values)
            {
                if (!tab.IsTabVisible(data)) continue;
                var visibleControls = tab.Controls.Where(c => c.IsControlVisible(data)).ToList();
                AddDropdownTypeValues(attrs, visibleControls, key);
                tabs.Add(new TabInfo(tab.Caption, visibleControls.Select(ToControlInfo).ToList()));
            }
            var visibleTopControls = def.Controls.Where(c => c.IsControlVisible(data)).ToList();
            AddDropdownTypeValues(attrs, visibleTopControls, key);
            topControls.AddRange(visibleTopControls.Select(ToControlInfo));
        }

        return JsonSerializer.Serialize(
            new EditorDataResponse(attrs, tabs, topControls),
            WasmEditorJsonContext.Default.EditorDataResponse);
    }

    [JSExport]
    public static string SetAttribute(string elementKey, string attribute, string controlType, string value)
    {
        if (_controller == null) return "error";
        var data = _controller.GetEditorData(elementKey);
        if (data == null) return "error";

        object typedValue = controlType switch
        {
            "checkbox" => bool.Parse(value),
            "number" => int.TryParse(value, out var i) ? (object)i : value,
            "numberdouble" => double.TryParse(value, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var d) ? (object)d : value,
            _ => value
        };

        _controller.StartTransaction($"Set {attribute}");
        try
        {
            var result = data.SetAttribute(attribute, typedValue);
            return result.Valid ? "ok" : result.Message.ToString();
        }
        finally
        {
            _controller.EndTransaction();
        }
    }

    [JSExport]
    public static string Save() => _controller?.Save() ?? string.Empty;

    [JSExport]
    public static bool CanUndo() => _controller?.GetUndoItems().Any() ?? false;

    [JSExport]
    public static bool CanRedo() => _controller?.GetRedoItems().Any() ?? false;

    [JSExport]
    public static void Undo()
    {
        if (_controller == null || !_controller.GetUndoItems().Any()) return;
        _controller.Undo();
    }

    [JSExport]
    public static void Redo()
    {
        if (_controller == null || !_controller.GetRedoItems().Any()) return;
        _controller.Redo();
    }

    [JSExport]
    public static string SetDropdownType(string elementKey, string controlId, string selectedType)
    {
        if (_controller == null) return "error";
        var editorName = _controller.GetElementEditorName(elementKey);
        if (editorName == null) return "error";
        var def = _controller.GetEditorDefinition(editorName);

        var ctrl = def.Tabs.Values.SelectMany(t => t.Controls)
            .Concat(def.Controls)
            .FirstOrDefault(c => c.Id == controlId);
        if (ctrl == null) return "error";

        var types = ctrl.GetDictionary("types");
        if (types == null) return "error";

        _controller.StartTransaction($"Set type");
        try
        {
            foreach (var typeName in types.Keys.Where(k => k != "*"))
            {
                if (_controller.DoesElementInheritType(elementKey, typeName))
                    _controller.RemoveInheritedTypeFromElement(elementKey, typeName, false);
            }
            if (selectedType != "*")
            {
                var result = _controller.AddInheritedTypeToElement(elementKey, selectedType, false);
                if (!result.Valid) return result.Message.ToString();
            }
            return "ok";
        }
        finally
        {
            _controller.EndTransaction();
        }
    }

    private static void AddDropdownTypeValues(Dictionary<string, string?> attrs, List<IEditorControl> controls, string elementKey)
    {
        foreach (var ctrl in controls.Where(c => c.ControlType == "dropdowntypes"))
            attrs[ctrl.Id] = _controller!.GetSelectedDropDownType(ctrl, elementKey);
    }

    // ── Script editor API ──────────────────────────────────────────────────────

    [JSExport]
    public static string? GetScriptData(string elementKey, string attribute)
    {
        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null) return null;
        return JsonSerializer.Serialize(BuildScriptBlockData(scripts), WasmEditorJsonContext.Default.ScriptBlockData);
    }

    [JSExport]
    public static string SetScriptParameter(string elementKey, string attribute, string containerPath, int scriptIndex, string paramName, string value)
    {
        if (_controller == null) return "error";
        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null) return "error";
        var container = ResolveContainer(scripts, containerPath);
        if (container == null || scriptIndex < 0 || scriptIndex >= container.Count) return "error";
        var script = container[scriptIndex];

        _controller.StartTransaction($"Set script parameter");
        try
        {
            script.SetParameter(paramName, value);
            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
        finally
        {
            _controller.EndTransaction();
        }
    }

    [JSExport]
    public static string SetIfExpression(string elementKey, string attribute, string containerPath, int scriptIndex, string expression)
    {
        if (_controller == null) return "error";
        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null) return "error";
        var container = ResolveContainer(scripts, containerPath);
        if (container == null || scriptIndex < 0 || scriptIndex >= container.Count) return "error";
        var script = container[scriptIndex];
        if (script is not EditableIfScript ifScript) return "not an if script";

        _controller.StartTransaction("Set if expression");
        try
        {
            ifScript.SetAttribute("expression", expression);
            return "ok";
        }
        finally
        {
            _controller.EndTransaction();
        }
    }

    [JSExport]
    public static string SetElseIfExpression(string elementKey, string attribute, string containerPath, int scriptIndex, int elseIfIndex, string expression)
    {
        if (_controller == null) return "error";
        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null) return "error";
        var container = ResolveContainer(scripts, containerPath);
        if (container == null || scriptIndex < 0 || scriptIndex >= container.Count) return "error";
        var script = container[scriptIndex];
        if (script is not EditableIfScript ifScript) return "not an if script";
        var elseIf = ifScript.ElseIfScripts.ElementAtOrDefault(elseIfIndex);
        if (elseIf == null) return "error";

        _controller.StartTransaction("Set else-if expression");
        try
        {
            elseIf.SetAttribute("expression", expression);
            return "ok";
        }
        finally
        {
            _controller.EndTransaction();
        }
    }

    [JSExport]
    public static string AddScript(string elementKey, string attribute, string containerPath, string keyword)
    {
        if (_controller == null) return "error";
        var scripts = GetScripts(elementKey, attribute);

        // Attribute not yet set — create a new script container directly on the element
        if (scripts == null && string.IsNullOrEmpty(containerPath))
        {
            try
            {
                _controller.CreateNewEditableScripts(elementKey, attribute, keyword, true);
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        if (scripts == null) return "error";
        var container = ResolveContainer(scripts, containerPath);
        if (container == null) return "error";

        try
        {
            container.AddNew(keyword, elementKey);
            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    [JSExport]
    public static string DeleteScript(string elementKey, string attribute, string containerPath, int scriptIndex)
    {
        if (_controller == null) return "error";
        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null) return "error";
        var container = ResolveContainer(scripts, containerPath);
        if (container == null || scriptIndex < 0 || scriptIndex >= container.Count) return "error";

        container.Remove([scriptIndex]);
        return "ok";
    }

    [JSExport]
    public static string MoveScript(string elementKey, string attribute, string containerPath, int index1, int index2)
    {
        if (_controller == null) return "error";
        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null) return "error";
        var container = ResolveContainer(scripts, containerPath);
        if (container == null) return "error";
        if (index1 < 0 || index1 >= container.Count || index2 < 0 || index2 >= container.Count) return "error";

        _controller.StartTransaction("Move script");
        try
        {
            container.Swap(index1, index2);
            return "ok";
        }
        finally
        {
            _controller.EndTransaction();
        }
    }

    [JSExport]
    public static string AddElse(string elementKey, string attribute, string containerPath, int scriptIndex)
    {
        if (_controller == null) return "error";
        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null) return "error";
        var container = ResolveContainer(scripts, containerPath);
        if (container == null || scriptIndex < 0 || scriptIndex >= container.Count) return "error";
        if (container[scriptIndex] is not EditableIfScript ifScript) return "not an if script";

        try
        {
            ifScript.AddElse();
            return "ok";
        }
        catch (Exception ex) { return ex.Message; }
    }

    [JSExport]
    public static string AddElseIf(string elementKey, string attribute, string containerPath, int scriptIndex)
    {
        if (_controller == null) return "error";
        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null) return "error";
        var container = ResolveContainer(scripts, containerPath);
        if (container == null || scriptIndex < 0 || scriptIndex >= container.Count) return "error";
        if (container[scriptIndex] is not EditableIfScript ifScript) return "not an if script";

        try
        {
            ifScript.AddElseIf();
            return "ok";
        }
        catch (Exception ex) { return ex.Message; }
    }

    [JSExport]
    public static string RemoveElse(string elementKey, string attribute, string containerPath, int scriptIndex)
    {
        if (_controller == null) return "error";
        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null) return "error";
        var container = ResolveContainer(scripts, containerPath);
        if (container == null || scriptIndex < 0 || scriptIndex >= container.Count) return "error";
        if (container[scriptIndex] is not EditableIfScript ifScript) return "not an if script";

        try
        {
            ifScript.RemoveElse();
            return "ok";
        }
        catch (Exception ex) { return ex.Message; }
    }

    [JSExport]
    public static string RemoveElseIf(string elementKey, string attribute, string containerPath, int scriptIndex, int elseIfIndex)
    {
        if (_controller == null) return "error";
        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null) return "error";
        var container = ResolveContainer(scripts, containerPath);
        if (container == null || scriptIndex < 0 || scriptIndex >= container.Count) return "error";
        if (container[scriptIndex] is not EditableIfScript ifScript) return "not an if script";
        var elseIf = ifScript.ElseIfScripts.ElementAtOrDefault(elseIfIndex);
        if (elseIf == null) return "error";

        try
        {
            ifScript.RemoveElseIf(elseIf);
            return "ok";
        }
        catch (Exception ex) { return ex.Message; }
    }

    [JSExport]
    public static string GetScriptCommandCategories()
    {
        if (_controller == null) return "null";
        var scriptData = _controller.GetScriptEditorData();
        var grouped = scriptData
            .Where(kv => !string.IsNullOrEmpty(kv.Value.Category)
                      && !kv.Value.IsDesktopOnly
                      && kv.Value.IsVisible()
                      && !string.IsNullOrEmpty(kv.Value.CreateString))
            .GroupBy(kv => kv.Value.Category)
            .Select(g => new ScriptCategoryInfo(
                g.Key,
                g.Select(kv => new ScriptCommandInfo(
                    kv.Key,
                    kv.Value.DisplayString ?? kv.Key,
                    kv.Value.AdderDisplayString ?? kv.Key,
                    kv.Value.CreateString!
                )).OrderBy(c => c.Add).ToList()
            ))
            .ToList();
        var data = new ScriptCommandCategoriesData(grouped);
        return JsonSerializer.Serialize(data, WasmEditorJsonContext.Default.ScriptCommandCategoriesData);
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private static IEditableScripts? GetScripts(string elementKey, string attribute)
    {
        if (_controller == null) return null;
        var data = _controller.GetEditorData(elementKey);
        return data?.GetAttribute(attribute) as IEditableScripts;
    }

    private static IEditableScripts? ResolveContainer(IEditableScripts root, string containerPath)
    {
        if (string.IsNullOrEmpty(containerPath)) return root;

        var parts = containerPath.Split('/');
        var current = root;
        int i = 0;

        while (i < parts.Length)
        {
            if (!int.TryParse(parts[i], out int scriptIndex)) return null;
            i++;

            if (i >= parts.Length) return null; // path ends at a script index, not a container

            if (scriptIndex < 0 || scriptIndex >= current.Count) return null;
            var script = current[scriptIndex];
            var segment = parts[i];
            i++;

            if (script.Type == ScriptType.If)
            {
                var ifScript = (EditableIfScript)script;
                if (segment == "then")
                {
                    current = ifScript.ThenScript;
                }
                else if (segment == "else")
                {
                    if (ifScript.ElseScript == null) return null;
                    current = ifScript.ElseScript;
                }
                else if (segment == "elseif")
                {
                    if (i >= parts.Length || !int.TryParse(parts[i], out int elseIfIndex)) return null;
                    i++;
                    var elseIf = ifScript.ElseIfScripts.ElementAtOrDefault(elseIfIndex);
                    if (elseIf == null) return null;
                    current = elseIf.EditableScripts;
                }
                else return null;
            }
            else
            {
                if (segment == "param")
                {
                    if (i >= parts.Length) return null;
                    var paramAttr = parts[i++];
                    var scriptEditorData = _controller!.GetScriptEditorData(script);
                    if (scriptEditorData.GetAttribute(paramAttr) is not IEditableScripts nestedScripts) return null;
                    current = nestedScripts;
                }
                else return null;
            }
        }

        return current;
    }

    private static ScriptBlockData BuildScriptBlockData(IEditableScripts scripts) =>
        new(scripts.Scripts.Select(BuildScriptNodeData).ToList());

    private static ScriptNodeData BuildScriptNodeData(IEditableScript script)
    {
        if (script.Type == ScriptType.If)
        {
            var ifScript = (EditableIfScript)script;
            var elseIfClauses = ifScript.ElseIfScripts
                .Select(ei => new ElseIfClauseData(ei.Id, ei.Expression, BuildScriptBlockData(ei.EditableScripts).Scripts))
                .ToList();

            return new ScriptNodeData(
                Id: script.Id,
                Type: "if",
                DisplayString: null,
                Controls: null,
                Expression: ifScript.IfExpression,
                ThenScripts: BuildScriptBlockData(ifScript.ThenScript).Scripts,
                ElseIfClauses: elseIfClauses,
                ElseScripts: ifScript.ElseScript != null ? BuildScriptBlockData(ifScript.ElseScript).Scripts : null
            );
        }

        string? displayString = null;
        List<ScriptControlData>? controls = null;

        try
        {
            var def = _controller!.GetEditorDefinition(script);
            var editorData = _controller.GetScriptEditorData(script);
            displayString = script.DisplayString();
            controls = def.Controls.Select(c => BuildScriptControlData(c, editorData)).ToList();
        }
        catch
        {
            displayString = script.DisplayString();
            controls = [];
        }

        return new ScriptNodeData(
            Id: script.Id,
            Type: "normal",
            DisplayString: displayString,
            Controls: controls,
            Expression: null,
            ThenScripts: null,
            ElseIfClauses: null,
            ElseScripts: null
        );
    }

    private static ScriptControlData BuildScriptControlData(IEditorControl ctrl, IEditorData editorData)
    {
        string? value = null;
        string? simpleEditor = null;
        List<ControlOption>? options = null;
        List<ScriptNodeData>? nestedScripts = null;

        if (ctrl.Attribute != null)
        {
            if (ctrl.ControlType == "script")
            {
                var attrValue = editorData.GetAttribute(ctrl.Attribute);
                if (attrValue is IEditableScripts nested)
                    nestedScripts = BuildScriptBlockData(nested).Scripts;
            }
            else
            {
                value = editorData.GetAttribute(ctrl.Attribute)?.ToString();
            }
        }

        if (ctrl.ControlType == "expression")
        {
            simpleEditor = ctrl.GetString("simpleeditor");
        }
        else if (ctrl.ControlType == "dropdown")
        {
            var list = ctrl.GetListString("validvalues");
            if (list != null)
                options = list.Select(v => new ControlOption(v, v)).ToList();
            else
            {
                var dict = ctrl.GetDictionary("validvalues");
                if (dict != null)
                    options = dict.Select(kv => new ControlOption(kv.Key, kv.Value)).ToList();
            }
        }

        return new ScriptControlData(
            ControlType: ctrl.ControlType,
            Caption: ctrl.Caption,
            Attribute: ctrl.Attribute,
            Value: value,
            SimpleEditor: simpleEditor,
            Options: options,
            Scripts: nestedScripts
        );
    }

    private static void OnAddedNode(object? sender, EditorController.AddedNodeEventArgs e)
    {
        TreeNodes.Add(new TreeNodeData(e.Key, e.Text, e.Parent));
    }

    private static ControlInfo ToControlInfo(IEditorControl ctrl)
    {
        List<ControlOption>? options = null;
        string? attribute = ctrl.Attribute;

        if (ctrl.ControlType == "dropdown")
        {
            var list = ctrl.GetListString("validvalues");
            if (list != null)
            {
                options = list.Select(v => new ControlOption(v, v)).ToList();
            }
            else
            {
                var dict = ctrl.GetDictionary("validvalues");
                if (dict != null)
                {
                    options = dict.Select(kv => new ControlOption(kv.Key, kv.Value)).ToList();
                }
            }
        }
        else if (ctrl.ControlType == "dropdowntypes")
        {
            var dict = ctrl.GetDictionary("types");
            if (dict != null)
                options = dict.Select(kv => new ControlOption(kv.Key, kv.Value)).ToList();
            attribute = ctrl.Id;
        }

        return new ControlInfo(attribute, ctrl.ControlType, ctrl.Caption, options);
    }
}
