using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using QuestViva.Common;
using QuestViva.EditorCore;
using QuestViva.Engine;
using QuestViva.Engine.Types;

namespace QuestViva.WasmEditor;

internal record TreeNodeData(string Key, string Text, string? Parent, string? NodeIcon, string NodeType);
internal record ControlOption(string Value, string Label);
internal record TextProcessorCommand(string Command, string Info, string InsertBefore, string InsertAfter);
internal record ControlInfo(string? Attribute, string ControlType, string? Caption, List<ControlOption>? Options, List<ControlOption>? SubEditors = null, string? SubAttribute = null, List<TextProcessorCommand>? TextProcessorCommands = null, string? AddPrompt = null);
internal record TabInfo(string? Caption, List<ControlInfo> Controls);
internal record EditorDataResponse(Dictionary<string, string?> Attributes, List<TabInfo> Tabs, List<ControlInfo> Controls);

internal record ScriptControlData(
    string ControlType,
    string? Caption,
    string? Attribute,
    string? Value,
    string? SimpleEditor,
    string? SimpleLabel,
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

internal record ExpressionTemplateControlData(
    string Name,
    string? Value,
    string? SimpleEditor,
    string? SimpleLabel,
    List<ControlOption>? Options
);
internal record IfExpressionTemplateData(
    string TemplateName,
    string OriginalPattern,
    List<ExpressionTemplateControlData> Controls
);
internal record IfExpressionTemplate(string Name, string CreateExpression);
internal record ListItemData(string Key, string Value);
internal record AttributeDataItem(string Name, string? Value, bool IsInherited, string Source, bool IsDefaultType, string Type);
internal record FullAttributeData(List<AttributeDataItem> Attributes, List<AttributeDataItem> InheritedTypes);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(List<TreeNodeData>))]
[JsonSerializable(typeof(EditorDataResponse))]
[JsonSerializable(typeof(ScriptBlockData))]
[JsonSerializable(typeof(ScriptCommandCategoriesData))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(List<IfExpressionTemplate>))]
[JsonSerializable(typeof(IfExpressionTemplateData))]
[JsonSerializable(typeof(int[]))]
[JsonSerializable(typeof(List<ListItemData>))]
[JsonSerializable(typeof(FullAttributeData))]
internal partial class WasmEditorJsonContext : JsonSerializerContext { }

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class WasmEditorBridge
{
    private static EditorController? _controller;
    private static readonly List<TreeNodeData> TreeNodes = [];
    private static bool _isRebuilding;
    private static string? _pendingRenameNewKey;

    [JSExport]
    public static async Task<bool> Initialise(byte[] gameFileBytes, string filename)
    {
        _controller?.Dispose();
        TreeNodes.Clear();

        _controller = new EditorController();
        _controller.ClearTree += (_, _) => { TreeNodes.Clear(); _isRebuilding = true; };
        _controller.BeginTreeUpdate += (_, _) => { };
        _controller.EndTreeUpdate += (_, _) => { _isRebuilding = false; };
        _controller.AddedNode += OnAddedNode;
        _controller.RemovedNode += (_, e) => TreeNodes.RemoveAll(n => n.Key == e.Key);
        _controller.RenamedNode += (_, e) =>
        {
            _pendingRenameNewKey = e.NewName;
            for (int i = 0; i < TreeNodes.Count; i++)
            {
                if (TreeNodes[i].Key == e.OldName)
                    TreeNodes[i] = TreeNodes[i] with { Key = e.NewName, Text = e.NewName, NodeType = GetNodeType(e.NewName, TreeNodes[i].NodeIcon) };
                else if (TreeNodes[i].Parent == e.OldName)
                    TreeNodes[i] = TreeNodes[i] with { Parent = e.NewName };
            }
        };
        _controller.RetitledNode += (_, e) =>
        {
            var idx = TreeNodes.FindIndex(n => n.Key == e.Key);
            if (idx >= 0) TreeNodes[idx] = TreeNodes[idx] with { Text = e.NewTitle };
        };

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
            var val = data.GetAttribute(attr.AttributeName);
            attrs[attr.AttributeName] = SerializeAttributeValue(val);
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
                AddDropdownTypeValues(attrs, visibleControls, key, data);
                tabs.Add(new TabInfo(tab.Caption, visibleControls.Select(ToControlInfo).ToList()));
            }
            var visibleTopControls = def.Controls.Where(c => c.IsControlVisible(data)).ToList();
            AddDropdownTypeValues(attrs, visibleTopControls, key, data);
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

        _pendingRenameNewKey = null;
        _controller.StartTransaction($"Set {attribute}");
        try
        {
            var result = data.SetAttribute(attribute, typedValue);
            if (!result.Valid) return result.Message.ToString();
            return _pendingRenameNewKey != null ? $"renamed:{_pendingRenameNewKey}" : "ok";
        }
        finally
        {
            _controller.EndTransaction();
        }
    }

    [JSExport]
    public static string SetMultiType(string elementKey, string attribute, string newType)
    {
        if (_controller == null) return "error";
        var data = _controller.GetEditorData(elementKey);
        if (data == null) return "error";

        _controller.StartTransaction($"Set type of {attribute}");
        try
        {
            switch (newType)
            {
                case "null":
                    data.SetAttribute(attribute, null!);
                    break;
                case "string":
                    data.SetAttribute(attribute, "");
                    break;
                case "script":
                    _controller.CreateNewEditableScripts(elementKey, attribute, null!, false);
                    break;
                default:
                    return $"Unknown type: {newType}";
            }
            return "ok";
        }
        catch (Exception ex) { return ex.Message; }
        finally { _controller.EndTransaction(); }
    }

    [JSExport]
    public static string SetObjectReference(string elementKey, string attribute, string objectName)
    {
        if (_controller == null) return "error";
        var data = _controller.GetEditorData(elementKey);
        if (data == null) return "error";

        _controller.StartTransaction($"Set {attribute}");
        try
        {
            var objRef = data.GetAttribute(attribute) as IEditableObjectReference
                ?? _controller.CreateNewEditableObjectReference(elementKey, attribute, false);
            objRef.Reference = objectName;
            return "ok";
        }
        catch (Exception ex) { return ex.Message; }
        finally { _controller.EndTransaction(); }
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

    [JSExport]
    public static string AddListItem(string elementKey, string attribute, string value)
    {
        if (_controller == null) return "error";
        var data = _controller.GetEditorData(elementKey);
        if (data == null) return "error";

        try
        {
            var existing = data.GetAttribute(attribute) as IEditableList<string>;
            if (existing == null)
            {
                _controller.CreateNewEditableList(elementKey, attribute, value, true);
            }
            else
            {
                var validation = existing.CanAdd(value);
                if (!validation.Valid) return validation.Message.ToString();
                existing.Add(value);
            }
            return "ok";
        }
        catch (Exception ex) { return ex.Message; }
    }

    [JSExport]
    public static string RemoveListItem(string elementKey, string attribute, string key)
    {
        if (_controller == null) return "error";
        var data = _controller.GetEditorData(elementKey);
        if (data == null) return "error";

        var list = data.GetAttribute(attribute) as IEditableList<string>;
        if (list == null) return "error";

        try
        {
            list.Remove(key);
            return "ok";
        }
        catch (Exception ex) { return ex.Message; }
    }

    [JSExport]
    public static string UpdateListItem(string elementKey, string attribute, string key, string value)
    {
        if (_controller == null) return "error";
        var data = _controller.GetEditorData(elementKey);
        if (data == null) return "error";

        var list = data.GetAttribute(attribute) as IEditableList<string>;
        if (list == null) return "error";

        try
        {
            list.Update(key, value);
            return "ok";
        }
        catch (Exception ex) { return ex.Message; }
    }

    private static void AddDropdownTypeValues(Dictionary<string, string?> attrs, List<IEditorControl> controls, string elementKey, IEditorData data)
    {
        foreach (var ctrl in controls.Where(c => c.ControlType == "dropdowntypes"))
            attrs[ctrl.Id] = _controller!.GetSelectedDropDownType(ctrl, elementKey);

        foreach (var ctrl in controls.Where(c => c.ControlType == "multi" && c.Attribute != null))
        {
            var val = data.GetAttribute(ctrl.Attribute!);
            attrs[ctrl.Id] = val switch
            {
                null => "null",
                string => "string",
                IEditableScripts => "script",
                IEditableObjectReference => "object",
                _ => "null"
            };
        }
    }

    // ── Script editor API ──────────────────────────────────────────────────────

    [JSExport]
    public static string GetScriptCode(string elementKey, string attribute)
    {
        var scripts = GetScripts(elementKey, attribute);
        if (scripts is not EditableScripts editableScripts) return string.Empty;
        return editableScripts.Code;
    }

    [JSExport]
    public static string SetScriptCode(string elementKey, string attribute, string code)
    {
        if (_controller == null) return "error";
        var scripts = GetScripts(elementKey, attribute);

        if (scripts is not EditableScripts editableScripts)
        {
            if (string.IsNullOrWhiteSpace(code)) return "ok";
            // Attribute not yet set — create an empty container then fall through to set Code
            _controller.CreateNewEditableScripts(elementKey, attribute, null!, false);
            scripts = GetScripts(elementKey, attribute);
            if (scripts is not EditableScripts newScripts) return "error";
            editableScripts = newScripts;
        }

        try
        {
            editableScripts.Code = code;
            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    [JSExport]
    public static string CopyScripts(string elementKey, string attribute, string containerPath, string indicesJson)
    {
        if (_controller == null) return "error";
        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null) return "error";
        var container = ResolveContainer(scripts, containerPath);
        if (container == null) return "error";
        var indices = JsonSerializer.Deserialize(indicesJson, WasmEditorJsonContext.Default.Int32Array);
        if (indices == null || indices.Length == 0) return "error";
        container.Copy(indices);
        return "ok";
    }

    [JSExport]
    public static string CutScripts(string elementKey, string attribute, string containerPath, string indicesJson)
    {
        if (_controller == null) return "error";
        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null) return "error";
        var container = ResolveContainer(scripts, containerPath);
        if (container == null) return "error";
        var indices = JsonSerializer.Deserialize(indicesJson, WasmEditorJsonContext.Default.Int32Array);
        if (indices == null || indices.Length == 0) return "error";
        try { container.Cut(indices); return "ok"; }
        catch (Exception ex) { return ex.Message; }
    }

    [JSExport]
    public static string DeleteScripts(string elementKey, string attribute, string containerPath, string indicesJson)
    {
        if (_controller == null) return "error";
        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null) return "error";
        var container = ResolveContainer(scripts, containerPath);
        if (container == null) return "error";
        var indices = JsonSerializer.Deserialize(indicesJson, WasmEditorJsonContext.Default.Int32Array);
        if (indices == null || indices.Length == 0) return "ok";
        container.Remove(indices);
        return "ok";
    }

    [JSExport]
    public static string PasteScripts(string elementKey, string attribute, string containerPath)
    {
        if (_controller == null) return "error";
        if (!_controller.CanPasteScript()) return "error";
        var scripts = GetScripts(elementKey, attribute);

        // No scripts container yet (attribute never set) — create an empty one then paste
        if (scripts == null && string.IsNullOrEmpty(containerPath))
        {
            _controller.StartTransaction("Paste script");
            try
            {
                _controller.CreateNewEditableScripts(elementKey, attribute, null!, false);
                scripts = GetScripts(elementKey, attribute);
                if (scripts == null) return "error";
                scripts.Paste(0, false);
                return "ok";
            }
            catch (Exception ex) { return ex.Message; }
            finally { _controller.EndTransaction(); }
        }

        if (scripts == null) return "error";
        var container = ResolveContainer(scripts, containerPath);
        if (container == null) return "error";
        container.Paste(container.Count, true);
        return "ok";
    }

    [JSExport]
    public static bool CanPasteScript() => _controller?.CanPasteScript() ?? false;

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
                g.OrderBy(kv => kv.Value.Order).Select(kv => new ScriptCommandInfo(
                    kv.Key,
                    kv.Value.DisplayString ?? kv.Key,
                    kv.Value.AdderDisplayString ?? kv.Key,
                    kv.Value.CreateString!
                )).ToList()
            ))
            .ToList();
        var data = new ScriptCommandCategoriesData(grouped);
        return JsonSerializer.Serialize(data, WasmEditorJsonContext.Default.ScriptCommandCategoriesData);
    }

    [JSExport]
    public static string GetObjectNames()
    {
        if (_controller == null) return "[]";
        var names = _controller.GetObjectNames("object", includeLibraryObjects: false).ToList();
        return JsonSerializer.Serialize(names, WasmEditorJsonContext.Default.ListString);
    }

    [JSExport]
    public static string GetIfExpressionTemplates()
    {
        if (_controller == null) return "[]";
        var templates = _controller.GetExpressionEditorNames("if")
            .Select(name => new IfExpressionTemplate(Name: name, CreateExpression: _controller.GetNewExpression(name)))
            .ToList();
        return JsonSerializer.Serialize(templates, WasmEditorJsonContext.Default.ListIfExpressionTemplate);
    }

    [JSExport]
    public static string? GetIfExpressionTemplateData(string expression)
    {
        if (_controller == null || string.IsNullOrEmpty(expression)) return null;
        var definition = _controller.GetExpressionEditorDefinition(expression, "if");
        if (definition == null) return null;

        IEditorData templateData;
        try { templateData = _controller.GetExpressionEditorData(expression, "if", null!); }
        catch { return null; }

        var controls = definition.Controls
            .Where(ctrl => ctrl.Attribute != null)
            .Select(ctrl =>
            {
                var simpleLabel = ctrl.GetString("simple");
                var simpleEditorTag = ctrl.GetString("simpleeditor");
                var simpleEditor = simpleEditorTag ?? (simpleLabel != null ? "textbox" : null);

                List<ControlOption>? options = null;
                if (simpleEditor == "dropdown")
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

                string? paramValue = null;
                try { paramValue = (string?)templateData.GetAttribute(ctrl.Attribute!); }
                catch { /* parameter not present in expression */ }

                return new ExpressionTemplateControlData(
                    Name: ctrl.Attribute!,
                    Value: paramValue,
                    SimpleEditor: simpleEditor,
                    SimpleLabel: simpleLabel,
                    Options: options
                );
            })
            .ToList();

        return JsonSerializer.Serialize(
            new IfExpressionTemplateData(
                TemplateName: definition.Description,
                OriginalPattern: definition.OriginalPattern,
                Controls: controls
            ),
            WasmEditorJsonContext.Default.IfExpressionTemplateData
        );
    }

    // ── Element creation / deletion ───────────────────────────────────────────

    [JSExport]
    public static string ValidateName(string name)
    {
        if (_controller == null) return "Not initialised";
        var result = _controller.CanAdd(name);
        return result.Valid ? "ok" : EditorController.GetValidationError(result, name);
    }

    [JSExport]
    public static string GetUniqueName(string baseName)
    {
        if (_controller == null) return baseName;
        return _controller.GetUniqueElementName(baseName);
    }

    [JSExport]
    public static string CreateRoom(string name, string? parent)
    {
        if (_controller == null) return "error:Not initialised";
        var validation = _controller.CanAdd(name);
        if (!validation.Valid) return $"error:{EditorController.GetValidationError(validation, name)}";
        try { _controller.CreateNewRoom(name, string.IsNullOrEmpty(parent) ? null : parent, null); return name; }
        catch (Exception ex) { return $"error:{ex.Message}"; }
    }

    [JSExport]
    public static string CreateObject(string name, string? parent)
    {
        if (_controller == null) return "error:Not initialised";
        var validation = _controller.CanAdd(name);
        if (!validation.Valid) return $"error:{EditorController.GetValidationError(validation, name)}";
        try { _controller.CreateNewObject(name, string.IsNullOrEmpty(parent) ? null : parent, null); return name; }
        catch (Exception ex) { return $"error:{ex.Message}"; }
    }

    [JSExport]
    public static string CreateFunction(string name)
    {
        if (_controller == null) return "error:Not initialised";
        var validation = _controller.CanAdd(name);
        if (!validation.Valid) return $"error:{EditorController.GetValidationError(validation, name)}";
        try { _controller.CreateNewFunction(name); return name; }
        catch (Exception ex) { return $"error:{ex.Message}"; }
    }

    [JSExport]
    public static string CreateTimer(string name)
    {
        if (_controller == null) return "error:Not initialised";
        var validation = _controller.CanAdd(name);
        if (!validation.Valid) return $"error:{EditorController.GetValidationError(validation, name)}";
        try { _controller.CreateNewTimer(name); return name; }
        catch (Exception ex) { return $"error:{ex.Message}"; }
    }

    [JSExport]
    public static string CreateExit(string parent)
    {
        if (_controller == null) return "error:Not initialised";
        try { return _controller.CreateNewExit(parent); }
        catch (Exception ex) { return $"error:{ex.Message}"; }
    }

    [JSExport]
    public static string CreateTurnScript(string parent)
    {
        if (_controller == null) return "error:Not initialised";
        try { return _controller.CreateNewTurnScript(parent); }
        catch (Exception ex) { return $"error:{ex.Message}"; }
    }

    [JSExport]
    public static string CreateCommand(string? parent)
    {
        if (_controller == null) return "error:Not initialised";
        try { return _controller.CreateNewCommand(string.IsNullOrEmpty(parent) ? null : parent); }
        catch (Exception ex) { return $"error:{ex.Message}"; }
    }

    [JSExport]
    public static string CreateVerb(string? parent)
    {
        if (_controller == null) return "error:Not initialised";
        try { return _controller.CreateNewVerb(string.IsNullOrEmpty(parent) ? null : parent, true); }
        catch (Exception ex) { return $"error:{ex.Message}"; }
    }

    [JSExport]
    public static void DeleteElement(string key)
    {
        _controller?.DeleteElement(key, true);
    }

    // ── Attributes editor API ──────────────────────────────────────────────────

    [JSExport]
    public static string? GetFullAttributeData(string elementKey)
    {
        if (_controller == null) return null;
        var data = _controller.GetEditorData(elementKey);
        if (data is not IEditorDataExtendedAttributeInfo extended) return null;

        var attributes = extended.GetAttributeData().Select(attr =>
        {
            var val = data.GetAttribute(attr.AttributeName);
            return new AttributeDataItem(attr.AttributeName, SerializeAttributeValue(val), attr.IsInherited, attr.Source, attr.IsDefaultType, DetermineAttributeType(val));
        }).ToList();

        var inheritedTypes = extended.GetInheritedTypes()
            .Select(t => new AttributeDataItem(t.AttributeName, t.AttributeName, t.IsInherited, t.Source, t.IsDefaultType, "type"))
            .ToList();

        return JsonSerializer.Serialize(
            new FullAttributeData(attributes, inheritedTypes),
            WasmEditorJsonContext.Default.FullAttributeData);
    }

    [JSExport]
    public static string RemoveAttribute(string elementKey, string attribute)
    {
        if (_controller == null) return "error";
        var data = _controller.GetEditorData(elementKey);
        if (data is not IEditorDataExtendedAttributeInfo extended) return "error";

        _controller.StartTransaction($"Remove {attribute}");
        try
        {
            extended.RemoveAttribute(attribute);
            return "ok";
        }
        catch (Exception ex) { return ex.Message; }
        finally { _controller.EndTransaction(); }
    }

    [JSExport]
    public static string AddInheritedType(string elementKey, string typeName)
    {
        if (_controller == null) return "error";
        _controller.StartTransaction($"Add type {typeName}");
        try
        {
            var result = _controller.AddInheritedTypeToElement(elementKey, typeName, false);
            return result.Valid ? "ok" : result.Message.ToString();
        }
        catch (Exception ex) { return ex.Message; }
        finally { _controller.EndTransaction(); }
    }

    [JSExport]
    public static string RemoveInheritedType(string elementKey, string typeName)
    {
        if (_controller == null) return "error";
        _controller.StartTransaction($"Remove type {typeName}");
        try
        {
            _controller.RemoveInheritedTypeFromElement(elementKey, typeName, false);
            return "ok";
        }
        catch (Exception ex) { return ex.Message; }
        finally { _controller.EndTransaction(); }
    }

    [JSExport]
    public static string GetTypeNames()
    {
        if (_controller == null) return "[]";
        var names = _controller.GetElementNames("type").ToList();
        return JsonSerializer.Serialize(names, WasmEditorJsonContext.Default.ListString);
    }

    [JSExport]
    public static string ChangeAttributeType(string elementKey, string attribute, string newType)
    {
        if (_controller == null) return "error";
        var data = _controller.GetEditorData(elementKey);
        if (data == null) return "error";

        _controller.StartTransaction($"Change type of {attribute}");
        try
        {
            if (newType == "script")
            {
                _controller.CreateNewEditableScripts(elementKey, attribute, null!, false);
            }
            else
            {
                object? newValue = newType switch
                {
                    "null" => null,
                    "string" => (object)"",
                    "boolean" => (object)false,
                    "int" => (object)0,
                    "double" => (object)0.0,
                    "simplepattern" => (object)new EditorCommandPattern(""),
                    "stringlist" => (object)new QuestList<string>(),
                    "stringdictionary" => (object)new QuestDictionary<string>(),
                    _ => null
                };
                if (newType != "null" && newValue == null) return $"Unsupported type: {newType}";
                var result = data.SetAttribute(attribute, newValue!);
                if (!result.Valid) return result.Message.ToString();
            }
            return "ok";
        }
        catch (Exception ex) { return ex.Message; }
        finally { _controller.EndTransaction(); }
    }

    [JSExport]
    public static string SetPatternAttribute(string elementKey, string attribute, string pattern)
    {
        if (_controller == null) return "error";
        var data = _controller.GetEditorData(elementKey);
        if (data == null) return "error";

        _controller.StartTransaction($"Set {attribute}");
        try
        {
            var cmd = data.GetAttribute(attribute) as IEditableCommandPattern;
            if (cmd != null)
                cmd.Pattern = pattern;
            else
                data.SetAttribute(attribute, new EditorCommandPattern(pattern));
            return "ok";
        }
        catch (Exception ex) { return ex.Message; }
        finally { _controller.EndTransaction(); }
    }

    [JSExport]
    public static string AddDictionaryItem(string elementKey, string attribute, string key, string value)
    {
        if (_controller == null) return "error";
        var data = _controller.GetEditorData(elementKey);
        if (data == null) return "error";

        try
        {
            var existing = data.GetAttribute(attribute) as IEditableDictionary<string>;
            if (existing == null)
            {
                _controller.CreateNewEditableStringDictionary(elementKey, attribute, key, value, true);
            }
            else
            {
                var validation = existing.CanAdd(key);
                if (!validation.Valid) return validation.Message.ToString();
                existing.Add(key, value);
            }
            return "ok";
        }
        catch (Exception ex) { return ex.Message; }
    }

    [JSExport]
    public static string RemoveDictionaryItem(string elementKey, string attribute, string key)
    {
        if (_controller == null) return "error";
        var data = _controller.GetEditorData(elementKey);
        if (data == null) return "error";

        var dict = data.GetAttribute(attribute) as IEditableDictionary<string>;
        if (dict == null) return "error";

        try { dict.Remove(key); return "ok"; }
        catch (Exception ex) { return ex.Message; }
    }

    [JSExport]
    public static string UpdateDictionaryItem(string elementKey, string attribute, string key, string value)
    {
        if (_controller == null) return "error";
        var data = _controller.GetEditorData(elementKey);
        if (data == null) return "error";

        var dict = data.GetAttribute(attribute) as IEditableDictionary<string>;
        if (dict == null) return "error";

        try { dict.Update(key, value); return "ok"; }
        catch (Exception ex) { return ex.Message; }
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private static string? SerializeAttributeValue(object? val) => val switch
    {
        null => null,
        bool b => b.ToString(),
        int i => i.ToString(),
        double d => d.ToString(System.Globalization.CultureInfo.InvariantCulture),
        IEditableObjectReference objRef => objRef.Reference,
        IEditableList<string> list => JsonSerializer.Serialize(
            list.ItemsList.Select(i => new ListItemData(i.Key, i.Value)).ToList(),
            WasmEditorJsonContext.Default.ListListItemData),
        IEditableDictionary<string> dict => JsonSerializer.Serialize(
            dict.Items.Values.Select(i => new ListItemData(i.Key, i.Value)).ToList(),
            WasmEditorJsonContext.Default.ListListItemData),
        IEditableScripts => "(script)",
        IEditableCommandPattern cmd => cmd.Pattern,
        IEditableDictionary<IEditableScripts> => "(script dictionary)",
        _ => val.ToString()
    };

    private static string DetermineAttributeType(object? val) => val switch
    {
        null => "null",
        bool => "boolean",
        int => "int",
        double => "double",
        IEditableObjectReference => "object",
        IEditableList<string> => "stringlist",
        IEditableDictionary<string> => "stringdictionary",
        IEditableScripts => "script",
        IEditableCommandPattern => "simplepattern",
        IEditableDictionary<IEditableScripts> => "scriptdictionary",
        _ => "string"
    };

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

        string? simpleLabel = null;

        if (ctrl.ControlType == "expression")
        {
            simpleLabel = ctrl.GetString("simple");
            var simpleEditorTag = ctrl.GetString("simpleeditor");
            // If <simpleeditor> is absent but <simple> is set, default simple editor is a textbox
            simpleEditor = simpleEditorTag ?? (simpleLabel != null ? "textbox" : null);

            if (simpleEditor == "dropdown")
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
            SimpleLabel: simpleLabel,
            Options: options,
            Scripts: nestedScripts
        );
    }

    private static string GetNodeType(string key, string? nodeIcon) => key.StartsWith("_")
        ? "header"
        : nodeIcon switch
        {
            "s_room" => "room",
            "s_object" => "object",
            "s_exit" => "exit",
            "s_verb" => "verb",
            "s_command" => "command",
            "s_turn" => "turnscript",
            "s_function" => "function",
            "s_timer" => "timer",
            "s_game" => "game",
            _ => "other"
        };

    private static void OnAddedNode(object? sender, EditorController.AddedNodeEventArgs e)
    {
        var node = new TreeNodeData(e.Key, e.Text, e.Parent, e.NodeIcon, GetNodeType(e.Key, e.NodeIcon));
        if (_isRebuilding)
        {
            // ClearTree already emptied the list; all keys are unique in a fresh rebuild.
            TreeNodes.Add(node);
        }
        else
        {
            // Outside a full rebuild, AddedNode can fire for an existing key
            // (e.g. field-update paths re-fire for nodes that move between parents).
            var idx = TreeNodes.FindIndex(n => n.Key == e.Key);
            if (idx >= 0)
                TreeNodes[idx] = node;
            else
                TreeNodes.Add(node);
        }
    }

    private static List<TextProcessorCommand>? GetTextProcessorCommands()
    {
        try
        {
            var data = _controller!.GetElementDataAttribute("_RichTextControl_TextProcessorCommands", "data") as IEnumerable;
            if (data == null) return null;

            var commands = new List<TextProcessorCommand>();
            foreach (IDictionary<string, string?> commandData in data)
            {
                commandData.TryGetValue("command", out var command);
                commandData.TryGetValue("info", out var info);
                commandData.TryGetValue("insertbefore", out var insertBefore);
                commandData.TryGetValue("insertafter", out var insertAfter);
                if (command != null)
                    commands.Add(new TextProcessorCommand(command, info ?? "", insertBefore ?? "", insertAfter ?? ""));
            }
            return commands.Count > 0 ? commands : null;
        }
        catch { return null; }
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
        else if (ctrl.ControlType == "objects")
        {
            var names = _controller!.GetObjectNames("object", includeLibraryObjects: false);
            options = names.Select(n => new ControlOption(n, n)).ToList();
        }
        else if (ctrl.ControlType == "multi")
        {
            var typesDict = ctrl.GetDictionary("types");
            if (typesDict != null)
                options = typesDict.Select(kv => new ControlOption(kv.Key, kv.Value)).ToList();

            var editorsDict = ctrl.GetDictionary("editors");
            List<ControlOption>? subEditors = editorsDict?.Select(kv => new ControlOption(kv.Key, kv.Value)).ToList();

            bool hasRichtextSubEditor = editorsDict?.Values.Any(v => v == "richtext") ?? false;
            List<TextProcessorCommand>? multiTpCommands = (hasRichtextSubEditor && !ctrl.GetBool("notextprocessor"))
                ? GetTextProcessorCommands()
                : null;

            var caption = ctrl.Caption ?? ctrl.GetString("selfcaption");
            return new ControlInfo(ctrl.Id, ctrl.ControlType, caption, options, subEditors, ctrl.Attribute, multiTpCommands);
        }

        List<TextProcessorCommand>? textProcessorCommands = null;
        if (ctrl.ControlType == "richtext" && !ctrl.GetBool("notextprocessor"))
            textProcessorCommands = GetTextProcessorCommands();

        string? addPrompt = ctrl.ControlType == "list" ? ctrl.GetString("editprompt") : null;

        return new ControlInfo(attribute, ctrl.ControlType, ctrl.Caption ?? ctrl.GetString("selfcaption"), options, null, null, textProcessorCommands, addPrompt);
    }
}
