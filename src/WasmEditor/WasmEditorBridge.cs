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

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(List<TreeNodeData>))]
[JsonSerializable(typeof(EditorDataResponse))]
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
                tabs.Add(new TabInfo(tab.Caption, tab.Controls.Select(ToControlInfo).ToList()));
            }
            topControls.AddRange(def.Controls.Select(ToControlInfo));
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
        var result = data.SetAttribute(attribute, typedValue);
        _controller.EndTransaction();

        return result.Valid ? "ok" : result.Message.ToString();
    }

    [JSExport]
    public static string Save() => _controller?.Save() ?? string.Empty;

    [JSExport]
    public static void Undo() => _controller?.Undo();

    [JSExport]
    public static void Redo() => _controller?.Redo();

    private static void OnAddedNode(object? sender, EditorController.AddedNodeEventArgs e)
    {
        TreeNodes.Add(new TreeNodeData(e.Key, e.Text, e.Parent));
    }

    private static ControlInfo ToControlInfo(IEditorControl ctrl)
    {
        List<ControlOption>? options = null;
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
        return new ControlInfo(ctrl.Attribute, ctrl.ControlType, ctrl.Caption, options);
    }
}
