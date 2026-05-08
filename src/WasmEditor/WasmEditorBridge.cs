using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using QuestViva.Common;
using QuestViva.EditorCore;

namespace QuestViva.WasmEditor;

internal record TreeNodeData(string Key, string Text, string? Parent);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(List<TreeNodeData>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
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
        var data = _controller?.GetEditorData(key);
        if (data is not IEditorDataExtendedAttributeInfo extended) return null;

        var attrs = new Dictionary<string, string?>();
        foreach (var attr in extended.GetAttributeData())
        {
            attrs[attr.AttributeName] = data.GetAttribute(attr.AttributeName)?.ToString();
        }
        return JsonSerializer.Serialize(attrs, WasmEditorJsonContext.Default.DictionaryStringString);
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
}
