using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using QuestViva.Common;
using QuestViva.EditorCore;
using QuestViva.Engine;
using QuestViva.Engine.Types;
using QuestViva.PlayerCore;

namespace QuestViva.WasmEditor;

internal record TreeNodeData(string Key, string Text, string? Parent, string NodeType, bool IsLibrary, bool CanDelete, string? Filename, string? Folder);

internal record ControlOption(string Value, string Label);

internal record TextProcessorCommand(string Command, string Info, string InsertBefore, string InsertAfter);

internal record ControlInfo(
    string? Attribute,
    string ControlType,
    string? Caption,
    List<ControlOption>? Options,
    List<ControlOption>? SubEditors = null,
    string? SubAttribute = null,
    List<TextProcessorCommand>? TextProcessorCommands = null,
    string? AddPrompt = null,
    string? ElementType = null,
    string? ObjectType = null,
    string? ListFilter = null,
    string? Source = null,
    bool Advanced = false,
    string? KeyPrompt = null,
    string? ValuePrompt = null,
    string? SourceExclude = null,
    string? SourceType = null,
    string? CheckboxCaption = null,
    bool IsWalkthrough = false,
    string? Href = null,
    string? NewFile = null,
    bool LockedAfterCreate = false,
    // <filefiltername> - human-readable label for what file types a "file" control accepts
    // (e.g. "Picture Files"), shown as the upload button's tooltip.
    string? FileFilterName = null,
    // <freetext/> - a "dropdown" control should let the user type a value not in its
    // <validvalues> list, instead of being restricted to picking one of the listed options.
    bool Freetext = false,
    // <minimum>/<maximum>/<increment> - bounds and step for a "number"/"numberdouble" control.
    double? Minimum = null,
    double? Maximum = null,
    double? Increment = null,
    // <nullable/> - emptying this control's value reverts the attribute to unset/inherited (as
    // opposed to just an empty string, which is still an explicit override) - the frontend calls
    // RemoveAttribute instead of SetAttribute("") when the new value is empty.
    bool Nullable = false,
    // <width> - fixed pixel width for this control (e.g. an exit's "to" object picker), instead
    // of the default flexible sizing.
    int? Width = null);

internal record TabInfo(string? Caption, List<ControlInfo> Controls);

// See WasmEditorBridge.ResolveLocalCover — DataUrl is null when Name is only a resource name
// the caller must resolve itself (a plain unpacked .aslx's sibling-file cover).
internal record LocalCoverResult(string Name, string? DataUrl);

internal record EditorDataResponse(
    Dictionary<string, string?> Attributes,
    List<TabInfo> Tabs,
    List<ControlInfo> Controls,
    bool IsLibraryElement,
    string? Filename);

internal record ScriptControlData(
    string ControlType,
    string? Caption,
    string? Attribute,
    string? Value,
    string? SimpleEditor,
    string? SimpleLabel,
    string? Source,
    List<ControlOption>? Options,
    List<ScriptNodeData>? Scripts,
    string? ObjectType = null,
    bool IsFunctionPicker = false,
    bool IsFunctionParams = false,
    // <breakbefore/> - forces this control onto a new line within the inline flex-wrap row of
    // script controls, e.g. so two adjacent checkboxes with distinct meanings (ShowPage's
    // "allow cancel" / "run turn scripts") don't run together just because they're both short.
    bool BreakBefore = false,
    // The expressionType (e.g. "set", "foreach") this control's <usetemplates> declares - see
    // CoreEditorScriptsVariables.aslx's "=" value control and CoreEditorScriptsScripts.aslx's
    // "foreach" list control. Null for the vast majority of "expression" controls, which have no
    // template picker (e.g. msg's text, MoveObject's destination). "if" conditions are a special
    // case (ScriptNodeData.Expression, not a Controls entry) but use the same "if" expressionType
    // and the same frontend picker, via IfExpressionControlDefinition.
    string? UseTemplates = null,
    // Pre-fetched nested script trees for a "scriptdictionary" control (e.g. switch's "cases"),
    // one per dictionary key - mirrors how the "script" controltype's own Scripts field lets a
    // nested ScriptEditor render from initialData without a further round trip.
    List<CaseScriptData>? Cases = null,
    // <multiline/> - the "textbox" simple editor should render as a resizable textarea that
    // keeps embedded newlines (e.g. msg's message text) instead of a single-line input.
    bool Multiline = false,
    // <expand/> - this control should grow to fill the remaining width of its row instead of
    // being capped to a fixed max-width.
    bool Expand = false,
    // <filefiltername> - human-readable label for what file types a "file" simple editor
    // accepts (e.g. "Picture Files"), shown as the upload button's tooltip.
    string? FileFilterName = null,
    // <freetext/> - a "dropdown" controltype or expression's "dropdown" simpleeditor should let
    // the user type a value not in its <validvalues> list, instead of being restricted to
    // picking one of the listed options.
    bool Freetext = false,
    // <minimum>/<maximum>/<increment> - bounds and step for a "number"/"numberdouble" simple
    // editor (e.g. Grid_DrawShape's opacity, 0.0-1.0 step 0.1).
    double? Minimum = null,
    double? Maximum = null,
    double? Increment = null
);

internal record ElseIfClauseData(string Id, string Expression, List<ScriptNodeData> Scripts);

internal record CaseScriptData(string Key, List<ScriptNodeData> Scripts);

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

internal record ScriptCommandInfo(string Keyword, string Display, string Add, string CreateString, bool Advanced, string? Common);

internal record ScriptCategoryInfo(string Name, List<ScriptCommandInfo> Commands, bool Advanced);

internal record ScriptCommandCategoriesData(List<ScriptCategoryInfo> Categories);

internal record ExpressionTemplateControlData(
    string Name,
    string ControlType,
    string? Caption,
    string? Value,
    string? SimpleEditor,
    string? SimpleLabel,
    List<ControlOption>? Options,
    // <minimum>/<maximum>/<increment> - bounds and step for a "number"/"numberdouble" control.
    double? Minimum = null,
    double? Maximum = null,
    double? Increment = null
);

internal record ExpressionTemplateData(
    string TemplateName,
    string OriginalPattern,
    List<ExpressionTemplateControlData> Controls
);

internal record ExpressionTemplate(string Name, string CreateExpression);

internal record ListItemData(string Key, string Value);

internal record AttributeDataItem(
    string Name,
    string? Value,
    bool IsInherited,
    string Source,
    bool IsDefaultType,
    string Type);

internal record FullAttributeData(List<AttributeDataItem> Attributes, List<AttributeDataItem> InheritedTypes);

internal record GameTemplateInfo(string Id, string Label, string Type);

internal record CompassDirectionInfo(
    string Direction,
    string TypeKey,
    string InverseDirection,
    string InverseTypeKey,
    string? ExitKey,
    string? To,
    bool LookOnly);

internal record ExitRowInfo(string Key, string? Alias, string? To, bool LookOnly);

internal record ExitsData(
    List<CompassDirectionInfo> Compass,
    List<ExitRowInfo> AllExits,
    List<ControlOption> Objects);

internal record VerbInfo(string Attribute, string DisplayPattern);

internal record ExpressionFunctionData(string Name, List<string> Parameters, bool IsUserDefined, bool IsLibrary);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(List<TreeNodeData>))]
[JsonSerializable(typeof(EditorDataResponse))]
[JsonSerializable(typeof(ScriptBlockData))]
[JsonSerializable(typeof(ScriptCommandCategoriesData))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(List<ExpressionTemplate>))]
[JsonSerializable(typeof(ExpressionTemplateData))]
[JsonSerializable(typeof(int[]))]
[JsonSerializable(typeof(List<ListItemData>))]
[JsonSerializable(typeof(FullAttributeData))]
[JsonSerializable(typeof(List<GameTemplateInfo>))]
[JsonSerializable(typeof(ExitsData))]
[JsonSerializable(typeof(List<VerbInfo>))]
[JsonSerializable(typeof(List<ExpressionFunctionData>))]
[JsonSerializable(typeof(LocalCoverResult))]
internal partial class WasmEditorJsonContext : JsonSerializerContext
{
}

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class WasmEditorBridge
{
    private static EditorController? _controller;
    private static readonly List<TreeNodeData> TreeNodes = [];
    private static bool _isRebuilding;
    private static bool _suppressTreeEvents;
    private static string? _pendingRenameNewKey;
    private static bool _pendingRetitle;
    private static bool _isDirty;

    // Wires the tree/dirty events a live controller needs to keep the JS-side TreeNodes cache and
    // _isDirty flag in sync. Shared by Initialise (wired before the initial parse, since nothing
    // valid exists yet to protect) and SetGameXml (wired only after a candidate controller has
    // already parsed successfully, so a failed raw-XML edit can never mutate the shared TreeNodes
    // list for the game that's still actually loaded).
    private static void AttachControllerEvents(EditorController controller)
    {
        controller.ClearTree += (_, _) =>
        {
            TreeNodes.Clear();
            _isRebuilding = true;
        };
        controller.BeginTreeUpdate += (_, _) => { };
        controller.EndTreeUpdate += (_, _) => { _isRebuilding = false; };
        controller.AddedNode += OnAddedNode;
        controller.RemovedNode += (_, e) =>
        {
            if (!_suppressTreeEvents)
            {
                TreeNodes.RemoveAll(n => n.Key == e.Key);
            }
        };
        controller.RenamedNode += (_, e) =>
        {
            _pendingRenameNewKey = e.NewName;
            for (var i = 0; i < TreeNodes.Count; i++)
            {
                if (TreeNodes[i].Key == e.OldName)
                {
                    TreeNodes[i] = TreeNodes[i] with { Key = e.NewName, Text = e.NewName };
                }
                else if (TreeNodes[i].Parent == e.OldName)
                {
                    TreeNodes[i] = TreeNodes[i] with {Parent = e.NewName};
                }
            }
        };
        controller.RetitledNode += (_, e) =>
        {
            var idx = TreeNodes.FindIndex(n => n.Key == e.Key);
            if (idx >= 0)
            {
                TreeNodes[idx] = TreeNodes[idx] with {Text = e.NewTitle};
                _pendingRetitle = true;
            }
        };

        controller.Dirty += (_, _) => { _isDirty = true; };
    }

    // Returns "ok" on success, or "error:{message}" — mirrors SetGameXml's convention. A raw
    // exception (e.g. a NullReferenceException from code that assumes Core.aslx's elements are
    // present, which isn't guaranteed if its <include> was deleted from the game) must never
    // cross the WASM boundary uncaught: AOT trimming strips exception message resources, so the
    // caller would otherwise see a bare "Arg_NullReferenceException" with no useful information.
    [JSExport]
    public static async Task<string> Initialise(byte[] gameFileBytes, string filename)
    {
        _controller?.Dispose();
        TreeNodes.Clear();

        var controller = new EditorController();
        string? errorMessage = null;
        controller.ShowMessage += (_, e) => errorMessage = e.Message;
        AttachControllerEvents(controller);
        _controller = controller;

        var provider = new ByteArrayGameDataProvider(gameFileBytes, filename, TakePendingAdjacentFiles());

        bool ok;
        try
        {
            ok = await controller.Initialise(provider);
        }
        catch (Exception ex)
        {
            controller.Dispose();
            _controller = null;
            return $"error:{ex.Message}";
        }

        if (!ok)
        {
            // WorldModel loaded with recorded errors (e.g. a missing library file, invalid XML) —
            // controller.Initialise already raised ShowMessage with the friendly, full list. Unlike
            // the UpdateTree failure below, there's no reason to keep this controller around: it
            // never reached GameState.Running (WorldModel.InitialiseInternal sets State to
            // Finished on failure), so Save()/GetGameXml() etc. would be operating on a model that
            // was never meant to be edited. Dispose and clear it — same as the exception path
            // above — so a caller that (bug notwithstanding) still tries to use it after a failed
            // Initialise() gets a clear NullReferenceException instead of silently touching a dead
            // model. The JS side (editor-store.ts's openGame) tears down its own stale UI state on
            // this same failure rather than leaving a disconnected controller reachable at all.
            controller.Dispose();
            _controller = null;
            return $"error:{errorMessage ?? "Failed to load game."}";
        }

        try
        {
            controller.UpdateTree();
        }
        catch (Exception ex)
        {
            // The game's element graph loaded without errors, but something in it is missing or
            // malformed enough to break tree construction — most likely a required library (e.g.
            // Core.aslx) was removed from the game's Included Libraries. Leave the controller in
            // place (rather than disposing it) since GetGameXml()/Save() still work off the
            // element graph alone and don't depend on the tree having built.
            TreeNodes.Clear();
            _isDirty = false;
            return $"error:This game could not be fully loaded — it may be missing a required library such as Core.aslx (technical details: {ex.Message}). Check the game's Included Libraries in Code View.";
        }

        _isDirty = false;
        return "ok";
    }

    [JSExport]
    public static string GetGameXml()
    {
        // Unlike Save(), this must not clear _isDirty — it's a read-only peek at the current
        // in-memory state for display purposes, not a save action.
        return _controller?.Save() ?? string.Empty;
    }

    [JSExport]
    public static async Task<string> SetGameXml(string xml)
    {
        if (_controller == null)
        {
            return "error:No game loaded.";
        }

        var filename = _controller.Filename;
        var bytes = System.Text.Encoding.UTF8.GetBytes(xml);
        var provider = new ByteArrayGameDataProvider(bytes, filename, TakePendingAdjacentFiles());

        // Parse into a throwaway controller first — the shared TreeNodes cache and _isDirty flag
        // must not be touched (see AttachControllerEvents) and the live _controller must not be
        // disposed until we know the new XML is actually valid, otherwise a malformed edit would
        // destroy the game that's currently open instead of just failing to apply.
        var candidate = new EditorController();
        string? errorMessage = null;
        candidate.ShowMessage += (_, e) => errorMessage = e.Message;

        bool ok;
        try
        {
            ok = await candidate.Initialise(provider);
        }
        catch (Exception ex)
        {
            candidate.Dispose();
            return $"error:{ex.Message}";
        }

        if (!ok)
        {
            candidate.Dispose();
            return $"error:{errorMessage ?? "Failed to parse ASLX."}";
        }

        AttachControllerEvents(candidate);
        _controller.Dispose();
        TreeNodes.Clear();
        _controller = candidate;

        try
        {
            _controller.UpdateTree();
        }
        catch (Exception ex)
        {
            // As in Initialise: the element graph parsed fine, but something in it is missing or
            // malformed enough to break tree construction (most likely a required library like
            // Core.aslx was removed from the XML just applied). The controller is already the
            // live one at this point, so GetGameXml()/Save() still reflect it — but TreeNodes is
            // left empty by UpdateTree()'s aborted run, so the tree panel stays blank until the
            // user fixes the XML and applies again.
            _isDirty = false;
            return $"error:This game could not be fully loaded — it may be missing a required library such as Core.aslx (technical details: {ex.Message}). Check the game's Included Libraries in Code View.";
        }

        _isDirty = false;
        return "ok";
    }

    // True when a library filename refers to one shipped inside the Engine assembly (Core.aslx,
    // GamebookCore.aslx, per-language files such as English.aslx, ...) rather than a file the
    // game author uploaded alongside the game — see WorldModel.IsBuiltInLibrary. The Code View
    // panel uses this to show built-in libraries read-only: they don't belong to the game, so
    // editing them directly isn't possible (and IsBuiltInLibrary is also what stops them being
    // deleted, see EditorController.CanDelete).
    [JSExport]
    public static bool IsBuiltInLibrary(string filename)
    {
        return WorldModel.IsBuiltInLibrary(filename);
    }

    // Returns the raw text of a built-in library (e.g. Core.aslx) for the Code View panel's
    // read-only browsing. Custom libraries' content lives in the host's asset storage instead
    // (see AddAdjacentFile), so the JS side reads those via its own FileAdapter — this is only
    // for the engine-shipped ones embedded in the Engine assembly (Core.aslx, per-language files
    // such as English.aslx, ...), resolved with the same resource names GetLibraryStream uses.
    // Null when the library can't be found (defensive; built-in filenames always resolve).
    [JSExport]
    public static string? GetLibraryXml(string filename)
    {
        if (string.IsNullOrEmpty(filename))
        {
            return null;
        }

        try
        {
            using var stream = WorldModel.GetEmbeddedResourceStream("QuestViva.Engine.Core." + filename)
                               ?? WorldModel.GetEmbeddedResourceStream("QuestViva.Engine.Core.Languages." + filename);
            if (stream == null)
            {
                return null;
            }

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        catch
        {
            return null;
        }
    }

    // Validates an edited Included Library's content without touching the live game, mirroring
    // SetGameXml's throwaway-controller approach: the current game is serialized, this library's
    // staged adjacent file is swapped for the new text, and a fresh EditorController tries to
    // load the result. On "ok" the JS side commits the file to the adapter and re-Initialises
    // the live game (libraries are consumed at load time, so the whole model has to reload to
    // pick the change up). A malformed edit returns "error:{message}" here instead — the current
    // game and its live controller are left completely undisturbed.
    [JSExport]
    public static async Task<string> SetLibraryXml(string filename, string xml)
    {
        if (_controller == null)
        {
            return "error:No game loaded.";
        }

        if (WorldModel.IsBuiltInLibrary(filename))
        {
            return "error:This is one of the built-in libraries that ships with the engine. It doesn't belong to your game, so it can't be edited here — copy its contents into a library of your own instead.";
        }

        var gameBytes = System.Text.Encoding.UTF8.GetBytes(_controller.Save());
        var adjacentFiles = TakePendingAdjacentFiles();
        adjacentFiles[filename] = System.Text.Encoding.UTF8.GetBytes(xml);
        var provider = new ByteArrayGameDataProvider(gameBytes, _controller.Filename, adjacentFiles);

        var candidate = new EditorController();
        string? errorMessage = null;
        candidate.ShowMessage += (_, e) => errorMessage = e.Message;

        bool ok;
        try
        {
            ok = await candidate.Initialise(provider);
        }
        catch (Exception ex)
        {
            candidate.Dispose();
            return $"error:{ex.Message}";
        }

        if (!ok)
        {
            candidate.Dispose();
            return $"error:{errorMessage ?? "Failed to parse ASLX."}";
        }

        // Never becomes the live controller (no AttachControllerEvents), so the shared TreeNodes
        // cache and _isDirty flag are untouched — the game that's currently open stays exactly
        // as it is.
        candidate.Dispose();
        return "ok";
    }

    [JSExport]
    public static string GetTreeNodes()
    {
        return JsonSerializer.Serialize(TreeNodes, WasmEditorJsonContext.Default.ListTreeNodeData);
    }

    [JSExport]
    public static void SetShowLibraryElements(bool show)
    {
        if (_controller == null) return;

        var options = new FilterOptions();
        options.Set("libraries", show);
        _controller.UpdateFilterOptions(options);
        _controller.UpdateTree();
    }

    [JSExport]
    public static string MakeElementLocal(string elementKey)
    {
        if (_controller == null) return "error:No game loaded.";

        _controller.MakeElementLocal(elementKey);
        return "ok";
    }

    [JSExport]
    public static async Task<string?> GetEditorData(string key)
    {
        if (_controller == null)
        {
            return null;
        }

        var data = _controller.GetEditorData(key);

        // data is null for synthetic header nodes (e.g. _objects, _functions) which don't
        // exist as world model elements but do have editor definitions in the ASLX.
        var extended = data as IEditorDataExtendedAttributeInfo;

        var attrs = new Dictionary<string, string?>();
        if (extended != null)
        {
            foreach (var attr in extended.GetAttributeData())
            {
                var val = data!.GetAttribute(attr.AttributeName);
                attrs[attr.AttributeName] = SerializeAttributeValue(val);
            }
        }

        var tabs = new List<TabInfo>();
        var topControls = new List<ControlInfo>();

        var editorName = _controller.GetElementEditorName(key);
        if (editorName != null)
        {
            var def = _controller.GetEditorDefinition(editorName);
            foreach (var tab in def.Tabs.Values)
            {
                if (data != null && !await tab.IsTabVisible(data))
                {
                    continue;
                }

                var visibleControls = data != null
                    ? await FilterVisibleAsync(tab.Controls, data)
                    : tab.Controls.ToList();
                if (data != null)
                {
                    AddDropdownTypeValues(attrs, visibleControls, key, data);
                }

                tabs.Add(new TabInfo(tab.Caption, visibleControls.Select(c => ToControlInfo(c, key)).ToList()));
            }

            var visibleTopControls = data != null
                ? await FilterVisibleAsync(def.Controls, data)
                : def.Controls.ToList();
            if (data != null)
            {
                AddDropdownTypeValues(attrs, visibleTopControls, key, data);
            }

            topControls.AddRange(visibleTopControls.Select(c => ToControlInfo(c, key)));
        }
        else if (data == null)
        {
            return null;
        }

        return JsonSerializer.Serialize(
            new EditorDataResponse(attrs, tabs, topControls, extended?.IsLibraryElement ?? false, extended?.Filename),
            WasmEditorJsonContext.Default.EditorDataResponse);
    }

    [JSExport]
    public static string SetAttribute(string elementKey, string attribute, string controlType, string value)
    {
        if (_controller == null)
        {
            return "error";
        }

        var data = _controller.GetEditorData(elementKey);
        if (data == null)
        {
            return "error";
        }

        var typedValue = controlType switch
        {
            "checkbox" => bool.Parse(value),
            "number" => int.TryParse(value, out var i) ? i : value,
            "numberdouble" => double.TryParse(value, NumberStyles.Any,
                CultureInfo.InvariantCulture, out var d)
                ? (object) d
                : value,
            _ => value
        };

        _pendingRenameNewKey = null;
        _pendingRetitle = false;
        _controller.StartTransaction($"Set {attribute}");
        try
        {
            var result = data.SetAttribute(attribute, typedValue);
            if (!result.Valid)
            {
                return $"error:{EditorController.GetValidationError(result, typedValue)}";
            }

            if (_pendingRenameNewKey != null)
            {
                return $"renamed:{_pendingRenameNewKey}";
            }

            return _pendingRetitle ? "retitled" : "ok";
        }
        finally
        {
            _controller.EndTransaction();
        }
    }

    [JSExport]
    public static string SetMultiType(string elementKey, string attribute, string newType)
    {
        if (_controller == null)
        {
            return "error";
        }

        var data = _controller.GetEditorData(elementKey);
        if (data == null)
        {
            return "error";
        }

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
                case "boolean":
                    data.SetAttribute(attribute, false);
                    break;
                case "script":
                    _controller.CreateNewEditableScripts(elementKey, attribute, null!, false);
                    break;
                case "scriptdictionary":
                    _controller.CreateNewEditableScriptDictionary(elementKey, attribute, null!, null!, false);
                    break;
                case "simplepattern":
                    data.SetAttribute(attribute, new EditorCommandPattern(""));
                    break;
                default:
                    return $"Unknown type: {newType}";
            }

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
    public static string SetObjectReference(string elementKey, string attribute, string objectName)
    {
        if (_controller == null)
        {
            return "error";
        }

        var data = _controller.GetEditorData(elementKey);
        if (data == null)
        {
            return "error";
        }

        _controller.StartTransaction($"Set {attribute}");
        try
        {
            var objRef = data.GetAttribute(attribute) as IEditableObjectReference
                         ?? _controller.CreateNewEditableObjectReference(elementKey, attribute, false);
            objRef.Reference = objectName;
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
    public static string Save()
    {
        var xml = _controller?.Save() ?? string.Empty;
        _isDirty = false;
        return xml;
    }

    [JSExport]
    public static bool IsDirty() => _isDirty;

    [JSExport]
    public static string GetGameId() => _controller?.GameId ?? string.Empty;

    [JSExport]
    public static bool IsGamebook() => _controller?.EditorStyle == EditorStyle.GameBook;

    // Play tab "Recently played" cover art for local files — entirely independent of
    // _controller (doesn't touch the currently-open editor session), a fresh GameQuery per
    // call instead. Several of these can be in flight at once (one per recent-local-play
    // card, resolved concurrently on the Play tab), so unlike PrepareSaveGame/GetSaveGameBytes
    // above this can't stash its result in a shared static field for a synchronous follow-up
    // call to collect — two overlapping calls would race over that field. Instead the whole
    // result comes back in one shot, as a JSON LocalCoverResult: DataUrl is set when the game
    // format embeds its own resources (.quest package, legacy .asl/.cas) and the cover was
    // found inside; otherwise (a plain unpacked .aslx, where the cover lives as a sibling file
    // GetResourceStream can't see — ByteArrayGameDataProvider has no filesystem access) DataUrl
    // is null and the caller resolves Name itself via its own FileAdapter.
    [JSExport]
    public static async Task<string?> ResolveLocalCover(byte[] gameFileBytes, string filename)
    {
        try
        {
            var query = new GameQuery(filename, gameFileBytes);
            if (!await query.Initialise()) return null;

            var coverName = query.CoverResourceName;
            if (string.IsNullOrEmpty(coverName)) return null;

            string? dataUrl = null;
            using (var stream = query.GetResource(coverName))
            {
                if (stream != null)
                {
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    dataUrl = $"data:{PlayerHelper.GetContentType(coverName)};base64,{Convert.ToBase64String(ms.ToArray())}";
                }
            }

            return JsonSerializer.Serialize(new LocalCoverResult(coverName, dataUrl), WasmEditorJsonContext.Default.LocalCoverResult);
        }
        catch
        {
            // Best-effort: a malformed/unsupported local file must never break the
            // Recently Played list, it should just keep the placeholder icon.
            return null;
        }
    }

    // Included Library elements only store a filename (<include ref="lib.aslx"/> — see
    // IncludeSaver); the library's actual content lives in the host's asset storage (IndexedDB,
    // OPFS, disk, ...), which this WASM module has no access to itself. So before every
    // (re)load, the JS side fetches each candidate asset's bytes and stages them here one at a
    // time ([JSExport] can't marshal an array of blobs in one call), the same way
    // AddPublishAsset/PendingPublishAssets stage assets for CreatePublishPackage below.
    // Initialise/SetGameXml consume and clear this before constructing their
    // ByteArrayGameDataProvider, regardless of whether the load succeeds.
    private static readonly Dictionary<string, byte[]> PendingAdjacentFiles = new();

    [JSExport]
    public static void AddAdjacentFile(string filename, byte[] data)
    {
        PendingAdjacentFiles[filename] = data;
    }

    private static Dictionary<string, byte[]> TakePendingAdjacentFiles()
    {
        var files = new Dictionary<string, byte[]>(PendingAdjacentFiles);
        PendingAdjacentFiles.Clear();
        return files;
    }

    // [JSExport] can't marshal an array of blobs in one call, so assets are staged one
    // at a time (same shape as the dirty-flag polling above) then consumed by
    // CreatePublishPackage, which also clears the staged list whether or not it succeeds.
    private static readonly List<EditorController.PackageIncludeFile> PendingPublishAssets = [];

    [JSExport]
    public static void AddPublishAsset(string filename, byte[] data)
    {
        PendingPublishAssets.Add(new EditorController.PackageIncludeFile
        {
            Filename = filename,
            Content = new MemoryStream(data)
        });
    }

    // Returns the .quest package bytes, or an empty array on failure — a real package
    // is never empty since it always contains at least the game.aslx entry.
    [JSExport]
    public static byte[] CreatePublishPackage(bool includeWalkthrough)
    {
        var assets = PendingPublishAssets.ToArray();
        PendingPublishAssets.Clear();
        if (_controller == null)
        {
            return [];
        }

        using var stream = new MemoryStream();
        var result = _controller.Publish(null, includeWalkthrough, assets, stream);
        return result.Valid ? stream.ToArray() : [];
    }

    [JSExport]
    public static bool CanUndo()
    {
        return _controller?.GetUndoItems().Any() ?? false;
    }

    [JSExport]
    public static bool CanRedo()
    {
        return _controller?.GetRedoItems().Any() ?? false;
    }

    [JSExport]
    public static async Task Undo()
    {
        if (_controller == null || !_controller.GetUndoItems().Any())
        {
            return;
        }

        await _controller.Undo();
    }

    [JSExport]
    public static void Redo()
    {
        if (_controller == null || !_controller.GetRedoItems().Any())
        {
            return;
        }

        _controller.Redo();
    }

    [JSExport]
    public static string SetDropdownType(string elementKey, string controlId, string selectedType)
    {
        if (_controller == null)
        {
            return "error";
        }

        var editorName = _controller.GetElementEditorName(elementKey);
        if (editorName == null)
        {
            return "error";
        }

        var def = _controller.GetEditorDefinition(editorName);

        var ctrl = def.Tabs.Values.SelectMany(t => t.Controls)
            .Concat(def.Controls)
            .FirstOrDefault(c => c.Id == controlId);
        if (ctrl == null)
        {
            return "error";
        }

        var types = ctrl.GetDictionary("types");
        if (types == null)
        {
            return "error";
        }

        _controller.StartTransaction("Set type");
        try
        {
            foreach (var typeName in types.Keys.Where(k => k != "*"))
            {
                if (_controller.DoesElementInheritType(elementKey, typeName))
                {
                    _controller.RemoveInheritedTypeFromElement(elementKey, typeName, false);
                }
            }

            if (selectedType != "*")
            {
                var result = _controller.AddInheritedTypeToElement(elementKey, selectedType, false);
                if (!result.Valid)
                {
                    return result.Message.ToString();
                }
            }

            return "ok";
        }
        finally
        {
            _controller.EndTransaction();
        }
    }

    [JSExport]
    public static string SetSelectedFilter(string elementKey, string filterGroupName, string filterValue)
    {
        if (_controller == null)
        {
            return "error";
        }

        var data = _controller.GetEditorData(elementKey);
        if (data == null)
        {
            return "error";
        }

        // GetEditorData() returns a fresh IEditorData on every call, so the in-memory selection
        // SetSelectedFilter() would normally record doesn't survive past this one call — the very
        // next GetEditorData() (e.g. the refetch after this returns) starts over and falls back to
        // GetDefaultFilterName()'s inference from which of the group's attributes is populated. So
        // to make the choice stick, drive that same inference: populate the chosen filter's
        // attribute (if not already) and clear the others in the group.
        var editorName = _controller.GetElementEditorName(elementKey);
        if (editorName == null)
        {
            return "error";
        }

        var def = _controller.GetEditorDefinition(editorName);
        var groupControls = def.Tabs.Values.SelectMany(t => t.Controls)
            .Concat(def.Controls)
            .Where(c => c.Attribute != null && c.GetString("filtergroup") == filterGroupName)
            .ToList();

        _controller.StartTransaction($"Set {filterGroupName}");
        try
        {
            foreach (var ctrl in groupControls)
            {
                var isSelected = ctrl.GetString("filter") == filterValue;
                var hasValue = data.GetAttribute(ctrl.Attribute!) != null;
                if (isSelected && !hasValue)
                {
                    data.SetAttribute(ctrl.Attribute!, "");
                }
                else if (!isSelected && hasValue)
                {
                    data.SetAttribute(ctrl.Attribute!, null!);
                }
            }

            data.SetSelectedFilter(filterGroupName, filterValue);
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
        if (_controller == null)
        {
            return "error";
        }

        var data = _controller.GetEditorData(elementKey);
        if (data == null)
        {
            return "error";
        }

        try
        {
            var existing = data.GetAttribute(attribute) as IEditableList<string>;
            if (existing == null)
            {
                _controller.CreateNewEditableList(elementKey, attribute, value, true);
            }
            else
            {
                existing = EnsureLocalList(data, attribute, existing);
                var validation = existing.CanAdd(value);
                if (!validation.Valid)
                {
                    return validation.Message.ToString();
                }

                existing.Add(value);
            }

            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    [JSExport]
    public static string RemoveListItem(string elementKey, string attribute, string key)
    {
        if (_controller == null)
        {
            return "error";
        }

        var data = _controller.GetEditorData(elementKey);
        if (data == null)
        {
            return "error";
        }

        var list = data.GetAttribute(attribute) as IEditableList<string>;
        if (list == null)
        {
            return "error";
        }

        try
        {
            list = EnsureLocalList(data, attribute, list);
            list.Remove(key);
            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    [JSExport]
    public static string UpdateListItem(string elementKey, string attribute, string key, string value)
    {
        if (_controller == null)
        {
            return "error";
        }

        var data = _controller.GetEditorData(elementKey);
        if (data == null)
        {
            return "error";
        }

        var list = data.GetAttribute(attribute) as IEditableList<string>;
        if (list == null)
        {
            return "error";
        }

        try
        {
            list = EnsureLocalList(data, attribute, list);
            list.Update(key, value);
            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    // Script-parameter-list variants of Add/Remove/UpdateListItem above. A script "list" control
    // (e.g. Call function's parameters, FunctionCallScript attribute "1") lives inside a script
    // tree at an arbitrary containerPath/scriptIndex rather than directly on an element, so it
    // can't be reached via GetEditorData(elementKey) — and unlike element attributes there's no
    // inherited/default-type variant to localize, so EnsureLocalList doesn't apply. Mutating the
    // IEditableList<string> directly (Add/Remove/Update) is required here rather than going
    // through script.SetParameter(paramAttribute, ...): FunctionCallScript.SetParameterInternal
    // deliberately throws for its parameter-list index, since parameter changes must go through
    // the list itself.
    private static IEditableList<string>? GetScriptParamList(string elementKey, string attribute,
        string containerPath, int scriptIndex, string paramAttribute)
    {
        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null)
        {
            return null;
        }

        var container = ResolveContainer(scripts, containerPath);
        if (container == null || scriptIndex < 0 || scriptIndex >= container.Count)
        {
            return null;
        }

        var editorData = _controller!.GetScriptEditorData(container[scriptIndex]);
        return editorData.GetAttribute(paramAttribute) as IEditableList<string>;
    }

    [JSExport]
    public static string AddScriptListItem(string elementKey, string attribute, string containerPath,
        int scriptIndex, string paramAttribute, string value)
    {
        if (_controller == null)
        {
            return "error";
        }

        var list = GetScriptParamList(elementKey, attribute, containerPath, scriptIndex, paramAttribute);
        if (list == null)
        {
            return "error";
        }

        // No StartTransaction/EndTransaction here — EditableList<T>.Add already opens its own
        // UndoLogger transaction, and nesting one throws ("previous transaction not finished").
        try
        {
            list.Add(value);
            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    [JSExport]
    public static string RemoveScriptListItem(string elementKey, string attribute, string containerPath,
        int scriptIndex, string paramAttribute, string key)
    {
        if (_controller == null)
        {
            return "error";
        }

        var list = GetScriptParamList(elementKey, attribute, containerPath, scriptIndex, paramAttribute);
        if (list == null)
        {
            return "error";
        }

        try
        {
            list.Remove(key);
            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    [JSExport]
    public static string UpdateScriptListItem(string elementKey, string attribute, string containerPath,
        int scriptIndex, string paramAttribute, string key, string value)
    {
        if (_controller == null)
        {
            return "error";
        }

        var list = GetScriptParamList(elementKey, attribute, containerPath, scriptIndex, paramAttribute);
        if (list == null)
        {
            return "error";
        }

        try
        {
            list.Update(key, value);
            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    // Resizes a script parameter list to an exact count, trimming or padding at the end while
    // leaving surviving indices' values untouched. Used when the Call function picker's chosen
    // function has a known, fixed arity (see EditorController.GetExpressionFunctions) so the
    // parameter boxes can be relabelled with real parameter names and kept in sync with the
    // function's declared signature, instead of requiring the generic +/- item-list UI.
    [JSExport]
    public static string SetScriptListItemCount(string elementKey, string attribute, string containerPath,
        int scriptIndex, string paramAttribute, int count)
    {
        if (_controller == null)
        {
            return "error";
        }

        var list = GetScriptParamList(elementKey, attribute, containerPath, scriptIndex, paramAttribute);
        if (list == null)
        {
            return "error";
        }

        try
        {
            var keys = list.ItemsList.Select(i => i.Key).ToList();
            while (keys.Count > count)
            {
                var key = keys[^1];
                list.Remove(key);
                keys.RemoveAt(keys.Count - 1);
            }

            while (keys.Count < count)
            {
                list.Add("");
                keys = list.ItemsList.Select(i => i.Key).ToList();
            }

            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    // Script-parameter-scriptdictionary variant of the above, for a "scriptdictionary" control
    // living inside a script tree (e.g. switch's "cases", attribute "1") rather than directly on
    // an element - same rationale as GetScriptParamList. Unlike switch's expression/default
    // parameters, SwitchScript.CasesAsQuestDictionary always exists (created empty in the
    // constructor), so — unlike AddScriptDictionaryItem's element-attribute equivalent — there's
    // no "create the dictionary if it doesn't exist yet" branch to worry about here.
    private static IEditableDictionary<IEditableScripts>? GetScriptParamDict(string elementKey, string attribute,
        string containerPath, int scriptIndex, string paramAttribute)
    {
        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null)
        {
            return null;
        }

        var container = ResolveContainer(scripts, containerPath);
        if (container == null || scriptIndex < 0 || scriptIndex >= container.Count)
        {
            return null;
        }

        var editorData = _controller!.GetScriptEditorData(container[scriptIndex]);
        return editorData.GetAttribute(paramAttribute) as IEditableDictionary<IEditableScripts>;
    }

    [JSExport]
    public static string AddScriptDictCase(string elementKey, string attribute, string containerPath,
        int scriptIndex, string paramAttribute, string key)
    {
        if (_controller == null)
        {
            return "error";
        }

        var dict = GetScriptParamDict(elementKey, attribute, containerPath, scriptIndex, paramAttribute);
        if (dict == null)
        {
            return "error";
        }

        try
        {
            var validation = dict.CanAdd(key);
            if (!validation.Valid)
            {
                return validation.Message.ToString();
            }

            var emptyScript = _controller.CreateNewEditableScripts(null!, null!, null!, false);
            dict.Add(key, emptyScript);
            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    [JSExport]
    public static string RemoveScriptDictCase(string elementKey, string attribute, string containerPath,
        int scriptIndex, string paramAttribute, string key)
    {
        if (_controller == null)
        {
            return "error";
        }

        var dict = GetScriptParamDict(elementKey, attribute, containerPath, scriptIndex, paramAttribute);
        if (dict == null)
        {
            return "error";
        }

        try
        {
            dict.Remove(key);
            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    [JSExport]
    public static string RenameScriptDictCase(string elementKey, string attribute, string containerPath,
        int scriptIndex, string paramAttribute, string oldKey, string newKey)
    {
        if (_controller == null)
        {
            return "error";
        }

        var dict = GetScriptParamDict(elementKey, attribute, containerPath, scriptIndex, paramAttribute);
        if (dict == null)
        {
            return "error";
        }

        if (oldKey == newKey)
        {
            return "ok";
        }

        try
        {
            var validation = dict.CanAdd(newKey);
            if (!validation.Valid)
            {
                return validation.Message.ToString();
            }

            dict.ChangeKey(oldKey, newKey);
            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    private static async Task<List<IEditorControl>> FilterVisibleAsync(IEnumerable<IEditorControl> controls, IEditorData data)
    {
        var result = new List<IEditorControl>();
        foreach (var c in controls)
        {
            if (await c.IsControlVisible(data))
            {
                result.Add(c);
            }
        }
        return result;
    }

    private static IEditableList<string> EnsureLocalList(IEditorData data, string attribute, IEditableList<string> list)
    {
        if (data is not IEditorDataExtendedAttributeInfo extended)
        {
            return list;
        }

        var attrInfo = extended.GetAttributeData().FirstOrDefault(a => a.AttributeName == attribute);
        if (attrInfo is not {IsInherited: true} and not {IsDefaultType: true})
        {
            return list;
        }

        var copy = new QuestList<string>();
        foreach (var item in list.ItemsList)
        {
            copy.Add(item.Value);
        }

        data.SetAttribute(attribute, copy);
        return (data.GetAttribute(attribute) as IEditableList<string>)!;
    }

    private static IEditableDictionary<string> EnsureLocalDict(IEditorData data, string attribute,
        IEditableDictionary<string> dict)
    {
        if (data is not IEditorDataExtendedAttributeInfo extended)
        {
            return dict;
        }

        var attrInfo = extended.GetAttributeData().FirstOrDefault(a => a.AttributeName == attribute);
        if (attrInfo is not {IsInherited: true} and not {IsDefaultType: true})
        {
            return dict;
        }

        var copy = new QuestDictionary<string>();
        foreach (var kvp in dict.Items)
        {
            copy.Add(kvp.Key, kvp.Value.Value);
        }

        data.SetAttribute(attribute, copy);
        return (data.GetAttribute(attribute) as IEditableDictionary<string>)!;
    }

    private static void AddDropdownTypeValues(Dictionary<string, string?> attrs, List<IEditorControl> controls,
        string elementKey, IEditorData data)
    {
        foreach (var ctrl in controls.Where(c => c.ControlType == "dropdowntypes"))
        {
            attrs[ctrl.Id] = _controller!.GetSelectedDropDownType(ctrl, elementKey);
        }

        foreach (var ctrl in controls.Where(c => c.ControlType == "multi" && c.Attribute != null))
        {
            var val = data.GetAttribute(ctrl.Attribute!);
            attrs[ctrl.Id] = val switch
            {
                null => "null",
                string => "string",
                bool => "boolean",
                int => "int",
                double => "double",
                IEditableScripts => "script",
                IEditableObjectReference => "object",
                IEditableCommandPattern => "simplepattern",
                IEditableDictionary<IEditableScripts> => "scriptdictionary",
                _ => "null"
            };
        }

        foreach (var ctrl in controls.Where(c => c.ControlType == "filter"))
        {
            var filterGroupName = ctrl.GetString("filtergroupname");
            if (filterGroupName == null)
            {
                continue;
            }

            attrs[ctrl.Id] = data.GetSelectedFilter(filterGroupName)
                ?? ctrl.Parent.GetDefaultFilterName(filterGroupName, data);
        }
    }

    // ── Script editor API ──────────────────────────────────────────────────────

    [JSExport]
    public static string GetScriptCode(string elementKey, string attribute)
    {
        var scripts = GetScripts(elementKey, attribute);
        if (scripts is not EditableScripts editableScripts)
        {
            return string.Empty;
        }

        return editableScripts.Code;
    }

    [JSExport]
    public static string SetScriptCode(string elementKey, string attribute, string code)
    {
        if (_controller == null)
        {
            return "error";
        }

        var scripts = GetScripts(elementKey, attribute);

        if (scripts is not EditableScripts editableScripts)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return "ok";
            }

            // Attribute not yet set — create an empty container then fall through to set Code
            _controller.CreateNewEditableScripts(elementKey, attribute, null!, false);
            scripts = GetScripts(elementKey, attribute);
            if (scripts is not EditableScripts newScripts)
            {
                return "error";
            }

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
        if (_controller == null)
        {
            return "error";
        }

        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null)
        {
            return "error";
        }

        var container = ResolveContainer(scripts, containerPath);
        if (container == null)
        {
            return "error";
        }

        var indices = JsonSerializer.Deserialize(indicesJson, WasmEditorJsonContext.Default.Int32Array);
        if (indices == null || indices.Length == 0)
        {
            return "error";
        }

        container.Copy(indices);
        return "ok";
    }

    [JSExport]
    public static string CutScripts(string elementKey, string attribute, string containerPath, string indicesJson)
    {
        if (_controller == null)
        {
            return "error";
        }

        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null)
        {
            return "error";
        }

        var container = ResolveContainer(scripts, containerPath);
        if (container == null)
        {
            return "error";
        }

        var indices = JsonSerializer.Deserialize(indicesJson, WasmEditorJsonContext.Default.Int32Array);
        if (indices == null || indices.Length == 0)
        {
            return "error";
        }

        try
        {
            container.Cut(indices);
            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    [JSExport]
    public static string DeleteScripts(string elementKey, string attribute, string containerPath, string indicesJson)
    {
        if (_controller == null)
        {
            return "error";
        }

        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null)
        {
            return "error";
        }

        var container = ResolveContainer(scripts, containerPath);
        if (container == null)
        {
            return "error";
        }

        var indices = JsonSerializer.Deserialize(indicesJson, WasmEditorJsonContext.Default.Int32Array);
        if (indices == null || indices.Length == 0)
        {
            return "ok";
        }

        container.Remove(indices);
        return "ok";
    }

    [JSExport]
    public static string PasteScripts(string elementKey, string attribute, string containerPath)
    {
        if (_controller == null)
        {
            return "error";
        }

        if (!_controller.CanPasteScript())
        {
            return "error";
        }

        var scripts = GetScripts(elementKey, attribute);

        // No scripts container yet (attribute never set) — create an empty one then paste
        if (scripts == null && string.IsNullOrEmpty(containerPath))
        {
            _controller.StartTransaction("Paste script");
            try
            {
                _controller.CreateNewEditableScripts(elementKey, attribute, null!, false);
                scripts = GetScripts(elementKey, attribute);
                if (scripts == null)
                {
                    return "error";
                }

                scripts.Paste(0, false);
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

        if (scripts == null)
        {
            return "error";
        }

        var container = ResolveContainer(scripts, containerPath);
        if (container == null)
        {
            return "error";
        }

        container.Paste(container.Count, true);
        return "ok";
    }

    [JSExport]
    public static bool CanPasteScript()
    {
        return _controller?.CanPasteScript() ?? false;
    }

    [JSExport]
    public static string? GetScriptData(string elementKey, string attribute)
    {
        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null)
        {
            return null;
        }

        return JsonSerializer.Serialize(BuildScriptBlockData(scripts), WasmEditorJsonContext.Default.ScriptBlockData);
    }

    [JSExport]
    public static string SetScriptParameter(string elementKey, string attribute, string containerPath, int scriptIndex,
        string paramName, string value)
    {
        if (_controller == null)
        {
            return "error";
        }

        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null)
        {
            return "error";
        }

        var container = ResolveContainer(scripts, containerPath);
        if (container == null || scriptIndex < 0 || scriptIndex >= container.Count)
        {
            return "error";
        }

        var script = container[scriptIndex];

        _controller.StartTransaction("Set script parameter");
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
    public static string SetIfExpression(string elementKey, string attribute, string containerPath, int scriptIndex,
        string expression)
    {
        if (_controller == null)
        {
            return "error";
        }

        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null)
        {
            return "error";
        }

        var container = ResolveContainer(scripts, containerPath);
        if (container == null || scriptIndex < 0 || scriptIndex >= container.Count)
        {
            return "error";
        }

        var script = container[scriptIndex];
        if (script is not EditableIfScript ifScript)
        {
            return "not an if script";
        }

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
    public static string SetElseIfExpression(string elementKey, string attribute, string containerPath, int scriptIndex,
        int elseIfIndex, string expression)
    {
        if (_controller == null)
        {
            return "error";
        }

        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null)
        {
            return "error";
        }

        var container = ResolveContainer(scripts, containerPath);
        if (container == null || scriptIndex < 0 || scriptIndex >= container.Count)
        {
            return "error";
        }

        var script = container[scriptIndex];
        if (script is not EditableIfScript ifScript)
        {
            return "not an if script";
        }

        var elseIf = ifScript.ElseIfScripts.ElementAtOrDefault(elseIfIndex);
        if (elseIf == null)
        {
            return "error";
        }

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
        if (_controller == null)
        {
            return "error";
        }

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

        if (scripts == null)
        {
            return "error";
        }

        var container = ResolveContainer(scripts, containerPath);
        if (container == null)
        {
            return "error";
        }

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
        if (_controller == null)
        {
            return "error";
        }

        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null)
        {
            return "error";
        }

        var container = ResolveContainer(scripts, containerPath);
        if (container == null || scriptIndex < 0 || scriptIndex >= container.Count)
        {
            return "error";
        }

        container.Remove([scriptIndex]);
        return "ok";
    }

    [JSExport]
    public static string MoveScript(string elementKey, string attribute, string containerPath, int index1, int index2)
    {
        if (_controller == null)
        {
            return "error";
        }

        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null)
        {
            return "error";
        }

        var container = ResolveContainer(scripts, containerPath);
        if (container == null)
        {
            return "error";
        }

        if (index1 < 0 || index1 >= container.Count || index2 < 0 || index2 >= container.Count)
        {
            return "error";
        }

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
        if (_controller == null)
        {
            return "error";
        }

        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null)
        {
            return "error";
        }

        var container = ResolveContainer(scripts, containerPath);
        if (container == null || scriptIndex < 0 || scriptIndex >= container.Count)
        {
            return "error";
        }

        if (container[scriptIndex] is not EditableIfScript ifScript)
        {
            return "not an if script";
        }

        try
        {
            ifScript.AddElse();
            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    [JSExport]
    public static string AddElseIf(string elementKey, string attribute, string containerPath, int scriptIndex)
    {
        if (_controller == null)
        {
            return "error";
        }

        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null)
        {
            return "error";
        }

        var container = ResolveContainer(scripts, containerPath);
        if (container == null || scriptIndex < 0 || scriptIndex >= container.Count)
        {
            return "error";
        }

        if (container[scriptIndex] is not EditableIfScript ifScript)
        {
            return "not an if script";
        }

        try
        {
            ifScript.AddElseIf();
            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    [JSExport]
    public static string RemoveElse(string elementKey, string attribute, string containerPath, int scriptIndex)
    {
        if (_controller == null)
        {
            return "error";
        }

        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null)
        {
            return "error";
        }

        var container = ResolveContainer(scripts, containerPath);
        if (container == null || scriptIndex < 0 || scriptIndex >= container.Count)
        {
            return "error";
        }

        if (container[scriptIndex] is not EditableIfScript ifScript)
        {
            return "not an if script";
        }

        try
        {
            ifScript.RemoveElse();
            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    [JSExport]
    public static string RemoveElseIf(string elementKey, string attribute, string containerPath, int scriptIndex,
        int elseIfIndex)
    {
        if (_controller == null)
        {
            return "error";
        }

        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null)
        {
            return "error";
        }

        var container = ResolveContainer(scripts, containerPath);
        if (container == null || scriptIndex < 0 || scriptIndex >= container.Count)
        {
            return "error";
        }

        if (container[scriptIndex] is not EditableIfScript ifScript)
        {
            return "not an if script";
        }

        var elseIf = ifScript.ElseIfScripts.ElementAtOrDefault(elseIfIndex);
        if (elseIf == null)
        {
            return "error";
        }

        try
        {
            ifScript.RemoveElseIf(elseIf);
            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    [JSExport]
    public static async Task<string> GetScriptCommandCategories()
    {
        if (_controller == null)
        {
            return "null";
        }

        var scriptData = _controller.GetScriptEditorData();
        var visibleEntries = new List<KeyValuePair<string, EditableScriptData>>();
        foreach (var kv in scriptData)
        {
            // "Call function" (appliesto "()") deliberately has an empty <create> string —
            // the function name and parameters aren't known until the user fills them in,
            // so EditableScripts.AddNewInternal special-cases an empty/null keyword to build
            // a blank FunctionCallScript instead of inserting literal text. Every other entry
            // has a real create string, so this is the one explicit exception to the filter.
            if (!string.IsNullOrEmpty(kv.Value.Category)
                && await kv.Value.IsVisible()
                && (!string.IsNullOrEmpty(kv.Value.CreateString) || kv.Key == "()"))
            {
                visibleEntries.Add(kv);
            }
        }
        var grouped = visibleEntries
            .GroupBy(kv => kv.Value.Category)
            .Select(g => new ScriptCategoryInfo(
                g.Key,
                // Non-advanced commands first (in declared order), then advanced ones
                // (also in declared order) — keeps a mixed category's everyday commands
                // together at the top instead of interleaved with advanced ones.
                g.OrderBy(kv => !kv.Value.IsVisibleInSimpleMode).ThenBy(kv => kv.Value.Order)
                    .Select(kv => new ScriptCommandInfo(
                        kv.Key,
                        kv.Value.DisplayString ?? kv.Key,
                        kv.Value.AdderDisplayString ?? kv.Key,
                        kv.Value.CreateString!,
                        !kv.Value.IsVisibleInSimpleMode,
                        kv.Value.CommonButton
                    )).ToList(),
                g.All(kv => !kv.Value.IsVisibleInSimpleMode)
            ))
            .ToList();
        var data = new ScriptCommandCategoriesData(grouped);
        return JsonSerializer.Serialize(data, WasmEditorJsonContext.Default.ScriptCommandCategoriesData);
    }

    [JSExport]
    public static string GetObjectNames()
    {
        if (_controller == null)
        {
            return "[]";
        }

        var names = _controller.GetObjectNames("object", false).ToList();
        return JsonSerializer.Serialize(names, WasmEditorJsonContext.Default.ListString);
    }

    [JSExport]
    public static string GetExitNames()
    {
        if (_controller == null)
        {
            return "[]";
        }

        var names = _controller.GetObjectNames("exit", false).ToList();
        return JsonSerializer.Serialize(names, WasmEditorJsonContext.Default.ListString);
    }

    [JSExport]
    public static string GetExpressionTemplates(string expressionType)
    {
        if (_controller == null)
        {
            return "[]";
        }

        var templates = _controller.GetExpressionEditorNames(expressionType)
            .Select(name => new ExpressionTemplate(name, _controller.GetNewExpression(name)))
            .ToList();
        return JsonSerializer.Serialize(templates, WasmEditorJsonContext.Default.ListExpressionTemplate);
    }

    [JSExport]
    public static string? GetExpressionTemplateData(string expression, string expressionType)
    {
        if (_controller == null || string.IsNullOrEmpty(expression))
        {
            return null;
        }

        var definition = _controller.GetExpressionEditorDefinition(expression, expressionType);
        if (definition == null)
        {
            return null;
        }

        IEditorData templateData;
        try
        {
            templateData = _controller.GetExpressionEditorData(expression, expressionType, null!);
        }
        catch
        {
            return null;
        }

        var controls = definition.Controls
            // A "label" control (e.g. RandomChance's "% of the time") has no <attribute> - it's
            // static caption text, not a value - but it still needs to reach the UI in sequence
            // with the value controls around it, so it can't be dropped by the attribute filter.
            .Where(ctrl => ctrl.Attribute != null || ctrl.ControlType == "label")
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

                string? paramValue = null;
                if (ctrl.Attribute != null)
                {
                    try
                    {
                        paramValue = (string?) templateData.GetAttribute(ctrl.Attribute);
                    }
                    catch
                    {
                        /* parameter not present in expression */
                    }
                }

                var isNumeric = simpleEditor is "number" or "numberdouble";
                var minimum = isNumeric ? GetNumericHint(ctrl, "minimum") : null;
                var maximum = isNumeric ? GetNumericHint(ctrl, "maximum") : null;
                var increment = isNumeric ? GetNumericHint(ctrl, "increment") : null;

                return new ExpressionTemplateControlData(
                    ctrl.Attribute ?? ctrl.Id,
                    ctrl.ControlType,
                    ctrl.Caption,
                    paramValue,
                    simpleEditor,
                    simpleLabel,
                    options,
                    minimum,
                    maximum,
                    increment
                );
            })
            .ToList();

        return JsonSerializer.Serialize(
            new ExpressionTemplateData(
                definition.Description,
                definition.OriginalPattern,
                controls
            ),
            WasmEditorJsonContext.Default.ExpressionTemplateData
        );
    }

    // ── Element creation / deletion ───────────────────────────────────────────

    [JSExport]
    public static string ValidateName(string name)
    {
        if (_controller == null)
        {
            return "Not initialised";
        }

        var result = _controller.CanAdd(name);
        return result.Valid ? "ok" : EditorController.GetValidationError(result, name);
    }

    [JSExport]
    public static string GetUniqueName(string baseName)
    {
        if (_controller == null)
        {
            return baseName;
        }

        return _controller.GetUniqueElementName(baseName);
    }

    [JSExport]
    public static string CreateRoom(string name, string? parent)
    {
        if (_controller == null)
        {
            return "error:Not initialised";
        }

        var validation = _controller.CanAdd(name);
        if (!validation.Valid)
        {
            return $"error:{EditorController.GetValidationError(validation, name)}";
        }

        try
        {
            _controller.CreateNewRoom(name, string.IsNullOrEmpty(parent) ? null : parent, null);
            return name;
        }
        catch (Exception ex)
        {
            return $"error:{ex.Message}";
        }
    }

    [JSExport]
    public static string CreateObject(string name, string? parent)
    {
        if (_controller == null)
        {
            return "error:Not initialised";
        }

        var validation = _controller.CanAdd(name);
        if (!validation.Valid)
        {
            return $"error:{EditorController.GetValidationError(validation, name)}";
        }

        try
        {
            _controller.CreateNewObject(name, string.IsNullOrEmpty(parent) ? null : parent, null);
            return name;
        }
        catch (Exception ex)
        {
            return $"error:{ex.Message}";
        }
    }

    [JSExport]
    public static string CreatePage(string name, string? parent)
    {
        if (_controller == null)
        {
            return "error:Not initialised";
        }

        var validation = _controller.CanAdd(name);
        if (!validation.Valid)
        {
            return $"error:{EditorController.GetValidationError(validation, name)}";
        }

        try
        {
            _controller.CreateNewPage(name, string.IsNullOrEmpty(parent) ? null : parent, null);
            // The AddedNode event fires while the object is still a plain object - the
            // dialoguepage type that gives it its "page" tree presentation is only added
            // afterwards (within the same transaction), so rebuild for the correct icon.
            _controller.UpdateTree();
            return name;
        }
        catch (Exception ex)
        {
            return $"error:{ex.Message}";
        }
    }

    // Object names inheriting the dialoguepage type, for controls with
    // <sourcetype>dialoguepage</sourcetype> (the TA page tab's options control).
    [JSExport]
    public static string GetPageNames()
    {
        if (_controller == null)
        {
            return "[]";
        }

        var names = _controller.GetObjectNames("object", false)
            .Where(n => _controller.IsDialoguePage(n))
            .ToList();
        return JsonSerializer.Serialize(names, WasmEditorJsonContext.Default.ListString);
    }

    [JSExport]
    public static string CreateFunction(string name)
    {
        if (_controller == null)
        {
            return "error:Not initialised";
        }

        var validation = _controller.CanAdd(name);
        if (!validation.Valid)
        {
            return $"error:{EditorController.GetValidationError(validation, name)}";
        }

        try
        {
            _controller.CreateNewFunction(name);
            return name;
        }
        catch (Exception ex)
        {
            return $"error:{ex.Message}";
        }
    }

    [JSExport]
    public static string CreateTimer(string name)
    {
        if (_controller == null)
        {
            return "error:Not initialised";
        }

        var validation = _controller.CanAdd(name);
        if (!validation.Valid)
        {
            return $"error:{EditorController.GetValidationError(validation, name)}";
        }

        try
        {
            _controller.CreateNewTimer(name);
            return name;
        }
        catch (Exception ex)
        {
            return $"error:{ex.Message}";
        }
    }

    [JSExport]
    public static string CreateExit(string parent)
    {
        if (_controller == null)
        {
            return "error:Not initialised";
        }

        try
        {
            return _controller.CreateNewExit(parent);
        }
        catch (Exception ex)
        {
            return $"error:{ex.Message}";
        }
    }

    // Index-based opposite-direction lookup into the 12-entry compass list
    // (NW,N,NE,W,E,SW,S,SE,Up,Down,In,Out) — ported from v5's ExitsControl.xaml.cs so it stays
    // correct regardless of the localized caption text.
    private static readonly int[] OppositeDirs = [7, 6, 5, 4, 3, 2, 1, 0, 9, 8, 11, 10];

    private static IEditorControl? FindExitsControl(string roomKey)
    {
        if (_controller == null)
        {
            return null;
        }

        var editorName = _controller.GetElementEditorName(roomKey);
        if (editorName == null)
        {
            return null;
        }

        var def = _controller.GetEditorDefinition(editorName);
        return def.Tabs.Values.SelectMany(t => t.Controls)
            .FirstOrDefault(c => c.ControlType == "exits");
    }

    [JSExport]
    public static string GetExitsData(string roomKey)
    {
        var empty = new ExitsData([], [], []);
        if (_controller == null)
        {
            return JsonSerializer.Serialize(empty, WasmEditorJsonContext.Default.ExitsData);
        }

        var exitsControl = FindExitsControl(roomKey);
        if (exitsControl == null)
        {
            return JsonSerializer.Serialize(empty, WasmEditorJsonContext.Default.ExitsData);
        }

        var directions = exitsControl.GetListString("compass").ToList();
        var types = exitsControl.GetDictionary("compasstypes");

        var compass = directions.Select((d, i) => new CompassDirectionInfo(
            d, types[d], directions[OppositeDirs[i]], types[directions[OppositeDirs[i]]],
            null, null, false)).ToList();

        var allExits = new List<ExitRowInfo>();
        foreach (var exitKey in _controller.GetObjectNames("exit", roomKey, true))
        {
            var data = _controller.GetEditorData(exitKey);
            var to = data.GetAttribute("to") as IEditableObjectReference;
            var alias = data.GetAttribute("alias") as string;
            var lookOnly = data.GetAttribute("lookonly") as bool? == true;
            allExits.Add(new ExitRowInfo(exitKey, alias, to?.Reference, lookOnly));

            var dirIndex = alias != null ? directions.IndexOf(alias) : -1;
            if (dirIndex >= 0)
            {
                compass[dirIndex] = compass[dirIndex] with
                {
                    ExitKey = exitKey, To = to?.Reference, LookOnly = lookOnly
                };
            }
        }

        var objects = _controller.GetObjectNames("object")
            .Where(n => n != roomKey && _controller.IsRoom(n))
            .OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase)
            .Select(n => new ControlOption(n, n)).ToList();

        var result = new ExitsData(compass, allExits, objects);
        return JsonSerializer.Serialize(result, WasmEditorJsonContext.Default.ExitsData);
    }

    [JSExport]
    public static string CreateExitInDirection(string roomKey, string direction, string to, bool createInverse)
    {
        if (_controller == null)
        {
            return "error:Not initialised";
        }

        var exitsControl = FindExitsControl(roomKey);
        if (exitsControl == null)
        {
            return "error:No exits control found";
        }

        var directions = exitsControl.GetListString("compass").ToList();
        var types = exitsControl.GetDictionary("compasstypes");
        var dirIndex = directions.IndexOf(direction);
        if (dirIndex < 0)
        {
            return "error:Unknown direction";
        }

        var type = types[direction];

        try
        {
            if (!createInverse)
            {
                return _controller.CreateNewExit(roomKey, to, direction, type, false);
            }

            var inverseDirection = directions[OppositeDirs[dirIndex]];
            var inverseType = types[inverseDirection];

            var inverseAlreadyExists = _controller.GetObjectNames("exit", to, true).Any(exitKey =>
            {
                var data = _controller.GetEditorData(exitKey);
                var lookOnly = data.GetAttribute("lookonly") as bool? == true;
                return !lookOnly && data.GetAttribute("alias") as string == inverseDirection;
            });

            if (inverseAlreadyExists)
            {
                var newExit = _controller.CreateNewExit(roomKey, to, direction, type, false);
                return $"warning:An exit already exists in that direction from the destination room|{newExit}";
            }

            return _controller.CreateNewExit(roomKey, to, direction, inverseDirection, type, inverseType);
        }
        catch (Exception ex)
        {
            return $"error:{ex.Message}";
        }
    }

    [JSExport]
    public static string CreateLookExitInDirection(string roomKey, string direction)
    {
        if (_controller == null)
        {
            return "error:Not initialised";
        }

        var exitsControl = FindExitsControl(roomKey);
        if (exitsControl == null)
        {
            return "error:No exits control found";
        }

        var types = exitsControl.GetDictionary("compasstypes");
        if (!types.TryGetValue(direction, out var type))
        {
            return "error:Unknown direction";
        }

        try
        {
            return _controller.CreateNewExit(roomKey, null, direction, type, true);
        }
        catch (Exception ex)
        {
            return $"error:{ex.Message}";
        }
    }

    [JSExport]
    public static string CreateTurnScript(string parent)
    {
        if (_controller == null)
        {
            return "error:Not initialised";
        }

        try
        {
            return _controller.CreateNewTurnScript(parent);
        }
        catch (Exception ex)
        {
            return $"error:{ex.Message}";
        }
    }

    [JSExport]
    public static string CreateCommand(string? parent)
    {
        if (_controller == null)
        {
            return "error:Not initialised";
        }

        try
        {
            return _controller.CreateNewCommand(string.IsNullOrEmpty(parent) ? null : parent);
        }
        catch (Exception ex)
        {
            return $"error:{ex.Message}";
        }
    }

    [JSExport]
    public static string CreateVerb(string? parent)
    {
        if (_controller == null)
        {
            return "error:Not initialised";
        }

        try
        {
            return _controller.CreateNewVerb(string.IsNullOrEmpty(parent) ? null : parent, true);
        }
        catch (Exception ex)
        {
            return $"error:{ex.Message}";
        }
    }

    [JSExport]
    public static string CreateWalkthrough(string name, string? parent)
    {
        if (_controller == null)
        {
            return "error:Not initialised";
        }

        var validation = _controller.CanAdd(name);
        if (!validation.Valid)
        {
            return $"error:{EditorController.GetValidationError(validation, name)}";
        }

        try
        {
            _controller.CreateNewWalkthrough(name, string.IsNullOrEmpty(parent) ? null : parent);
            return name;
        }
        catch (Exception ex)
        {
            return $"error:{ex.Message}";
        }
    }

    // Appends steps captured by WasmPlayer's walkthrough recorder (see wasm-player.js's
    // recordWalkthroughName/recordedSteps) onto an existing walkthrough element — the same
    // append-not-replace behaviour as EditorController.RecordWalkthrough, which this just exposes
    // across the JS boundary.
    [JSExport]
    public static string RecordWalkthroughSteps(string name, string stepsJson)
    {
        if (_controller == null)
        {
            return "error:Not initialised";
        }

        try
        {
            var steps = JsonSerializer.Deserialize(stepsJson, WasmEditorJsonContext.Default.ListString);
            _controller.RecordWalkthrough(name, steps ?? []);
            return "ok";
        }
        catch (Exception ex)
        {
            return $"error:{ex.Message}";
        }
    }

    [JSExport]
    public static string CreateTemplate(string name)
    {
        if (_controller == null)
        {
            return "error:Not initialised";
        }

        try
        {
            return _controller.CreateNewTemplate(name);
        }
        catch (Exception ex)
        {
            return $"error:{ex.Message}";
        }
    }

    [JSExport]
    public static string CreateDynamicTemplate(string name)
    {
        if (_controller == null)
        {
            return "error:Not initialised";
        }

        var validation = _controller.CanAdd(name);
        if (!validation.Valid)
        {
            return $"error:{EditorController.GetValidationError(validation, name)}";
        }

        try
        {
            _controller.CreateNewDynamicTemplate(name);
            return name;
        }
        catch (Exception ex)
        {
            return $"error:{ex.Message}";
        }
    }

    [JSExport]
    public static string CreateObjectType(string name)
    {
        if (_controller == null)
        {
            return "error:Not initialised";
        }

        var validation = _controller.CanAdd(name);
        if (!validation.Valid)
        {
            return $"error:{EditorController.GetValidationError(validation, name)}";
        }

        try
        {
            _controller.CreateNewType(name);
            return name;
        }
        catch (Exception ex)
        {
            return $"error:{ex.Message}";
        }
    }

    [JSExport]
    public static string CreateIncludedLibrary(string filename)
    {
        if (_controller == null)
        {
            return "error:Not initialised";
        }

        try
        {
            return _controller.CreateNewIncludedLibrary(filename);
        }
        catch (Exception ex)
        {
            return $"error:{ex.Message}";
        }
    }

    [JSExport]
    public static string CreateJavascript(string src)
    {
        if (_controller == null)
        {
            return "error:Not initialised";
        }

        try
        {
            return _controller.CreateNewJavascript(src);
        }
        catch (Exception ex)
        {
            return $"error:{ex.Message}";
        }
    }

    [JSExport]
    public static void DeleteElement(string key)
    {
        if (_controller == null || !_controller.CanDelete(key))
        {
            return;
        }

        _controller.DeleteElement(key, true);
    }

    // Wraps every deletion in a single transaction (mirrors v5's ExitsControl.xaml.cs
    // toolbar_DeleteClicked) so one Undo reverts all of them together — calling DeleteElement()
    // once per key instead would open/close its own transaction each time, so Undo would only
    // restore the last one.
    [JSExport]
    public static void DeleteElements(string keysJson)
    {
        if (_controller == null)
        {
            return;
        }

        var keys = JsonSerializer.Deserialize(keysJson, WasmEditorJsonContext.Default.ListString);
        if (keys == null || keys.Count == 0)
        {
            return;
        }

        _controller.StartTransaction($"Delete {keys.Count} elements");
        foreach (var key in keys)
        {
            if (_controller.CanDelete(key))
            {
                _controller.DeleteElement(key, false);
            }
        }
        _controller.EndTransaction();
    }

    [JSExport]
    public static string SwapElements(string key1, string key2)
    {
        if (_controller == null)
        {
            return "error";
        }

        // Capture positions before SwapElements fires tree events (Remove+AddToEnd) that would scramble the list.
        var i1 = TreeNodes.FindIndex(n => n.Key == key1);
        var i2 = TreeNodes.FindIndex(n => n.Key == key2);
        _suppressTreeEvents = true;
        try
        {
            _controller.SwapElements(key1, key2);
        }
        finally
        {
            _suppressTreeEvents = false;
        }

        if (i1 >= 0 && i2 >= 0)
        {
            (TreeNodes[i1], TreeNodes[i2]) = (TreeNodes[i2], TreeNodes[i1]);
        }

        return "ok";
    }

    // ── Move / cut / copy / paste ───────────────────────────────────────────

    [JSExport]
    public static bool CanMoveElement(string elementKey)
    {
        return _controller?.CanMoveElement(elementKey) ?? false;
    }

    [JSExport]
    public static string GetMovePossibleParents(string elementKey)
    {
        var parents = _controller?.GetMovePossibleParents(elementKey)?.ToList() ?? [];
        return JsonSerializer.Serialize(parents, WasmEditorJsonContext.Default.ListString);
    }

    [JSExport]
    public static string MoveElement(string elementKey, string newParentKey)
    {
        if (_controller == null || !_controller.CanMoveElement(elementKey, newParentKey))
        {
            return "error";
        }

        _controller.MoveElement(elementKey, newParentKey);
        return "ok";
    }

    [JSExport]
    public static string GetFunctionFolders()
    {
        var folders = _controller?.GetFunctionFolders().ToList() ?? [];
        return JsonSerializer.Serialize(folders, WasmEditorJsonContext.Default.ListString);
    }

    [JSExport]
    public static string SetFunctionFolder(string elementKey, string? folder)
    {
        if (_controller == null)
        {
            return "error";
        }

        _controller.SetFunctionFolder(elementKey, folder);
        // A full rebuild, not just an incremental refresh: SetFunctionFolder can reposition
        // several functions at once (see its own comment), and the AddedNode/RemovedNode pair
        // EditorController fires per repositioned element only ever *appends* to this class's
        // TreeNodes cache when the key isn't already present (see OnAddedNode) - correct only
        // when every repositioned element ends up trailing all its untouched siblings, which
        // isn't guaranteed in general (e.g. inserting into the middle of an existing folder that
        // has more functions after it). SwapElements avoids this the other way, by suppressing
        // the natural events and patching its own two known slots directly; a full rebuild here
        // is simpler to get right for an operation that can touch an arbitrary-sized slice, and
        // is still cheap since it's one pass over the (now name-only) element list rather than
        // the many redundant per-element writes the old implementation made.
        _controller.UpdateTree();
        return "ok";
    }

    [JSExport]
    public static bool CanMoveFunctionUp(string elementKey)
    {
        return _controller?.CanMoveFunctionUp(elementKey) ?? false;
    }

    [JSExport]
    public static bool CanMoveFunctionDown(string elementKey)
    {
        return _controller?.CanMoveFunctionDown(elementKey) ?? false;
    }

    [JSExport]
    public static bool CanMoveFunctionFolderUp(string folder)
    {
        return _controller?.CanMoveFunctionFolderUp(folder) ?? false;
    }

    [JSExport]
    public static bool CanMoveFunctionFolderDown(string folder)
    {
        return _controller?.CanMoveFunctionFolderDown(folder) ?? false;
    }

    [JSExport]
    public static string MoveFunctionFolderUp(string folder)
    {
        if (_controller == null)
        {
            return "error";
        }

        _controller.MoveFunctionFolderUp(folder);
        // Full rebuild, not an incremental patch - same reasoning as SetFunctionFolder's wrapper
        // above: this can reposition an arbitrary-sized slice of functions at once.
        _controller.UpdateTree();
        return "ok";
    }

    [JSExport]
    public static string MoveFunctionFolderDown(string folder)
    {
        if (_controller == null)
        {
            return "error";
        }

        _controller.MoveFunctionFolderDown(folder);
        _controller.UpdateTree();
        return "ok";
    }

    [JSExport]
    public static string GetPossibleNewObjectParentsForCurrentSelection(string elementKey)
    {
        var parents = _controller?.GetPossibleNewObjectParentsForCurrentSelection(elementKey)?.ToList() ?? [];
        return JsonSerializer.Serialize(parents, WasmEditorJsonContext.Default.ListString);
    }

    [JSExport]
    public static void CopyElements(string keysJson)
    {
        if (_controller == null)
        {
            return;
        }

        var keys = JsonSerializer.Deserialize(keysJson, WasmEditorJsonContext.Default.ListString);
        if (keys == null || keys.Count == 0)
        {
            return;
        }

        _controller.CopyElements(keys);
    }

    [JSExport]
    public static void CutElements(string keysJson)
    {
        if (_controller == null)
        {
            return;
        }

        var keys = JsonSerializer.Deserialize(keysJson, WasmEditorJsonContext.Default.ListString);
        if (keys == null || keys.Count == 0)
        {
            return;
        }

        _controller.CutElements(keys);
    }

    [JSExport]
    public static bool CanPasteElements(string parentKey)
    {
        return _controller?.CanPaste(parentKey) ?? false;
    }

    [JSExport]
    public static string PasteElements(string parentKey)
    {
        if (_controller == null || !_controller.CanPaste(parentKey))
        {
            return "error";
        }

        return _controller.PasteElements(parentKey) ?? "error";
    }

    // ── Verbs editor API ─────────────────────────────────────────────────────

    private static IEditorControl? FindVerbsControl(string elementKey)
    {
        if (_controller == null)
        {
            return null;
        }

        var editorName = _controller.GetElementEditorName(elementKey);
        if (editorName == null)
        {
            return null;
        }

        var def = _controller.GetEditorDefinition(editorName);
        return def.Tabs.Values.SelectMany(t => t.Controls)
            .FirstOrDefault(c => c.ControlType == "verbs");
    }

    // Property name -> friendly display pattern (e.g. "lookin" -> "look in") for every verb
    // defined anywhere in the game (library verbs plus any custom ones already added to any
    // object) — used both to identify which of an object's attributes are verbs, and to build
    // the autocomplete list when adding a new one.
    [JSExport]
    public static string GetVerbAttributesInfo()
    {
        if (_controller == null)
        {
            return "[]";
        }

        var verbs = _controller.GetVerbProperties()
            .Select(kv => new VerbInfo(kv.Key, kv.Value))
            .ToList();

        return JsonSerializer.Serialize(verbs, WasmEditorJsonContext.Default.ListVerbInfo);
    }

    // Feeds the expression-insert helper's Functions list — built-in engine functions plus the
    // game's own (non-library) Function elements, both with real parameter names.
    [JSExport]
    public static string GetExpressionFunctions()
    {
        if (_controller == null)
        {
            return "[]";
        }

        var functions = _controller.GetExpressionFunctions()
            .Select(f => new ExpressionFunctionData(f.Name, f.Parameters.ToList(), f.IsUserDefined, f.IsLibrary))
            .ToList();

        return JsonSerializer.Serialize(functions, WasmEditorJsonContext.Default.ListExpressionFunctionData);
    }

    // Feeds the code view's attribute-name autocomplete: every attribute name ever set on any
    // object in the game (the values GetAttribute/HasAttribute/set/etc. take as their attribute
    // argument), so authors get suggestions for attributes defined on other objects, not just
    // ones already typed in the file open in the editor.
    [JSExport]
    public static string GetAttributeNames()
    {
        if (_controller == null)
        {
            return "[]";
        }

        var names = _controller.GetPropertyNames().ToList();
        return JsonSerializer.Serialize(names, WasmEditorJsonContext.Default.ListString);
    }

    [JSExport]
    public static string AddVerb(string elementKey, string verbPattern)
    {
        if (_controller == null)
        {
            return "error:Not initialised";
        }

        verbPattern = verbPattern.Trim().ToLowerInvariant();
        if (verbPattern.Length == 0)
        {
            return "error:Enter a verb";
        }

        var data = _controller.GetEditorData(elementKey);
        if (data == null)
        {
            return "error:No data";
        }

        var verbsControl = FindVerbsControl(elementKey);
        var verbAttribute = _controller.GetVerbAttributeForPattern(verbPattern);
        var isExistingVerb = _controller.IsVerbAttribute(verbAttribute);

        if (!isExistingVerb)
        {
            var canAdd = _controller.CanAddVerb(verbPattern);
            if (!canAdd.CanAdd)
            {
                var message = $"Verb would clash with the '{canAdd.ClashingCommandDisplay}' command";
                var clashMessages = verbsControl?.GetDictionary("clashmessages");
                if (clashMessages != null && canAdd.ClashingCommand != null &&
                    clashMessages.TryGetValue(canAdd.ClashingCommand, out var extra))
                {
                    message += ". " + extra;
                }

                return $"error:{message}";
            }
        }

        if (data.GetAttribute(verbAttribute) != null)
        {
            return "error:This verb has already been added";
        }

        _controller.StartTransaction($"Add '{verbPattern}' verb");
        try
        {
            if (!isExistingVerb)
            {
                // A custom verb doesn't exist as a game-wide command yet, so create it (this is
                // what makes the pattern actually get parsed by the player at runtime) before
                // recording that this object handles it.
                var newVerbId = _controller.CreateNewVerb(null, false);
                var verbData = _controller.GetEditorData(newVerbId);
                verbData.SetAttribute("property", verbAttribute);
                if (verbData.GetAttribute("pattern") is IEditableCommandPattern pattern)
                {
                    pattern.Pattern = verbPattern;
                }

                var defaultExpressionTemplate = verbsControl?.GetString("defaultexpression");
                if (!string.IsNullOrEmpty(defaultExpressionTemplate))
                {
                    verbData.SetAttribute("defaultexpression",
                        defaultExpressionTemplate.Replace("#verb#", verbPattern));
                }
            }

            var result = data.SetAttribute(verbAttribute, string.Empty);
            if (!result.Valid)
            {
                return $"error:{result.Message}";
            }

            return $"ok:{verbAttribute}";
        }
        finally
        {
            _controller.EndTransaction();
        }
    }

    // ── Attributes editor API ──────────────────────────────────────────────────

    [JSExport]
    public static string? GetFullAttributeData(string elementKey)
    {
        if (_controller == null)
        {
            return null;
        }

        var data = _controller.GetEditorData(elementKey);
        if (data is not IEditorDataExtendedAttributeInfo extended)
        {
            return null;
        }

        var attributes = extended.GetAttributeData().Select(attr =>
        {
            var val = data.GetAttribute(attr.AttributeName);
            return new AttributeDataItem(attr.AttributeName, SerializeAttributeValue(val), attr.IsInherited,
                attr.Source, attr.IsDefaultType, DetermineAttributeType(val));
        }).ToList();

        var inheritedTypes = extended.GetInheritedTypes()
            .Select(t =>
                new AttributeDataItem(t.AttributeName, t.AttributeName, t.IsInherited, t.Source, t.IsDefaultType,
                    "type"))
            .ToList();

        return JsonSerializer.Serialize(
            new FullAttributeData(attributes, inheritedTypes),
            WasmEditorJsonContext.Default.FullAttributeData);
    }

    [JSExport]
    public static string RemoveAttribute(string elementKey, string attribute)
    {
        if (_controller == null)
        {
            return "error";
        }

        var data = _controller.GetEditorData(elementKey);
        if (data is not IEditorDataExtendedAttributeInfo extended)
        {
            return "error";
        }

        _controller.StartTransaction($"Remove {attribute}");
        try
        {
            extended.RemoveAttribute(attribute);
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
    public static string AddInheritedType(string elementKey, string typeName)
    {
        if (_controller == null)
        {
            return "error";
        }

        _controller.StartTransaction($"Add type {typeName}");
        try
        {
            var result = _controller.AddInheritedTypeToElement(elementKey, typeName, false);
            return result.Valid ? "ok" : result.Message.ToString();
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
    public static string RemoveInheritedType(string elementKey, string typeName)
    {
        if (_controller == null)
        {
            return "error";
        }

        _controller.StartTransaction($"Remove type {typeName}");
        try
        {
            _controller.RemoveInheritedTypeFromElement(elementKey, typeName, false);
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
    public static string GetTypeNames()
    {
        if (_controller == null)
        {
            return "[]";
        }

        var names = _controller.GetElementNames("type").ToList();
        return JsonSerializer.Serialize(names, WasmEditorJsonContext.Default.ListString);
    }

    [JSExport]
    public static string ChangeAttributeType(string elementKey, string attribute, string newType)
    {
        if (_controller == null)
        {
            return "error";
        }

        var data = _controller.GetEditorData(elementKey);
        if (data == null)
        {
            return "error";
        }

        _controller.StartTransaction($"Change type of {attribute}");
        try
        {
            if (newType == "script")
            {
                _controller.CreateNewEditableScripts(elementKey, attribute, null!, false);
            }
            else if (newType == "scriptdictionary")
            {
                _controller.CreateNewEditableScriptDictionary(elementKey, attribute, null!, null!, false);
            }
            else if (newType == "object")
            {
                _controller.CreateNewEditableObjectReference(elementKey, attribute, false);
            }
            else
            {
                var newValue = newType switch
                {
                    "null" => null,
                    "string" => "",
                    "boolean" => false,
                    "int" => 0,
                    "double" => 0.0,
                    "simplepattern" => new EditorCommandPattern(""),
                    "stringlist" => new QuestList<string>(),
                    "stringdictionary" => (object) new QuestDictionary<string>(),
                    _ => null
                };
                if (newType != "null" && newValue == null)
                {
                    return $"Unsupported type: {newType}";
                }

                var result = data.SetAttribute(attribute, newValue!);
                if (!result.Valid)
                {
                    return result.Message.ToString();
                }
            }

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
    public static string SetPatternAttribute(string elementKey, string attribute, string pattern)
    {
        if (_controller == null)
        {
            return "error";
        }

        var data = _controller.GetEditorData(elementKey);
        if (data == null)
        {
            return "error";
        }

        _controller.StartTransaction($"Set {attribute}");
        try
        {
            var cmd = data.GetAttribute(attribute) as IEditableCommandPattern;
            if (cmd != null)
            {
                cmd.Pattern = pattern;
            }
            else
            {
                data.SetAttribute(attribute, new EditorCommandPattern(pattern));
            }

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
    public static string AddDictionaryItem(string elementKey, string attribute, string key, string value)
    {
        if (_controller == null)
        {
            return "error";
        }

        var data = _controller.GetEditorData(elementKey);
        if (data == null)
        {
            return "error";
        }

        try
        {
            var existing = data.GetAttribute(attribute) as IEditableDictionary<string>;
            if (existing == null)
            {
                _controller.CreateNewEditableStringDictionary(elementKey, attribute, key, value, true);
            }
            else
            {
                existing = EnsureLocalDict(data, attribute, existing);
                var validation = existing.CanAdd(key);
                if (!validation.Valid)
                {
                    return validation.Message.ToString();
                }

                existing.Add(key, value);
            }

            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    [JSExport]
    public static string RemoveDictionaryItem(string elementKey, string attribute, string key)
    {
        if (_controller == null)
        {
            return "error";
        }

        var data = _controller.GetEditorData(elementKey);
        if (data == null)
        {
            return "error";
        }

        var dict = data.GetAttribute(attribute) as IEditableDictionary<string>;
        if (dict == null)
        {
            return "error";
        }

        try
        {
            dict = EnsureLocalDict(data, attribute, dict);
            dict.Remove(key);
            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    [JSExport]
    public static string UpdateDictionaryItem(string elementKey, string attribute, string key, string value)
    {
        if (_controller == null)
        {
            return "error";
        }

        var data = _controller.GetEditorData(elementKey);
        if (data == null)
        {
            return "error";
        }

        var dict = data.GetAttribute(attribute) as IEditableDictionary<string>;
        if (dict == null)
        {
            return "error";
        }

        try
        {
            dict = EnsureLocalDict(data, attribute, dict);
            dict.Update(key, value);
            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    [JSExport]
    public static string MakeScriptEditable(string elementKey, string attribute)
    {
        if (_controller == null)
        {
            return "error";
        }

        var scripts = GetScripts(elementKey, attribute);
        if (scripts == null)
        {
            return "error";
        }

        _controller.StartTransaction($"Copy {attribute} script to {elementKey}");
        try
        {
            scripts.Clone(elementKey, attribute);
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
    public static string MakeScriptDictEditable(string elementKey, string attribute)
    {
        if (_controller == null)
        {
            return "error";
        }

        var data = _controller.GetEditorData(elementKey);
        if (data == null)
        {
            return "error";
        }

        if (data.GetAttribute(attribute) is not IEditableDictionary<IEditableScripts>)
        {
            return "error";
        }

        try
        {
            _controller.MakeScriptDictionaryEditable(elementKey, attribute);
            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    [JSExport]
    public static string AddScriptDictionaryItem(string elementKey, string attribute, string key)
    {
        if (_controller == null)
        {
            return "error";
        }

        var data = _controller.GetEditorData(elementKey);
        if (data == null)
        {
            return "error";
        }

        try
        {
            var existing = data.GetAttribute(attribute) as IEditableDictionary<IEditableScripts>;
            if (existing == null)
            {
                var emptyScript = _controller.CreateNewEditableScripts(null!, null!, null!, false);
                _controller.CreateNewEditableScriptDictionary(elementKey, attribute, key, emptyScript, true);
            }
            else
            {
                var validation = existing.CanAdd(key);
                if (!validation.Valid)
                {
                    return validation.Message.ToString();
                }

                var emptyScript = _controller.CreateNewEditableScripts(null!, null!, null!, false);
                existing.Add(key, emptyScript);
            }

            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    [JSExport]
    public static string RemoveScriptDictionaryItem(string elementKey, string attribute, string key)
    {
        if (_controller == null)
        {
            return "error";
        }

        var data = _controller.GetEditorData(elementKey);
        if (data == null)
        {
            return "error";
        }

        var dict = data.GetAttribute(attribute) as IEditableDictionary<IEditableScripts>;
        if (dict == null)
        {
            return "error";
        }

        try
        {
            dict.Remove(key);
            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    [JSExport]
    public static string RenameScriptDictionaryItem(string elementKey, string attribute, string oldKey, string newKey)
    {
        if (_controller == null)
        {
            return "error";
        }

        var data = _controller.GetEditorData(elementKey);
        if (data == null)
        {
            return "error";
        }

        var dict = data.GetAttribute(attribute) as IEditableDictionary<IEditableScripts>;
        if (dict == null)
        {
            return "error";
        }

        if (oldKey == newKey)
        {
            return "ok";
        }

        try
        {
            var validation = dict.CanAdd(newKey);
            if (!validation.Valid)
            {
                return validation.Message.ToString();
            }

            dict.ChangeKey(oldKey, newKey);
            return "ok";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private static string? SerializeAttributeValue(object? val)
    {
        return val switch
        {
            null => null,
            bool b => b.ToString(),
            int i => i.ToString(),
            double d => d.ToString(CultureInfo.InvariantCulture),
            IEditableObjectReference objRef => objRef.Reference,
            IEditableList<string> list => JsonSerializer.Serialize(
                list.ItemsList.Select(i => new ListItemData(i.Key, i.Value)).ToList(),
                WasmEditorJsonContext.Default.ListListItemData),
            IEditableDictionary<string> dict => JsonSerializer.Serialize(
                dict.Items.Values.Select(i => new ListItemData(i.Key, i.Value)).ToList(),
                WasmEditorJsonContext.Default.ListListItemData),
            IEditableScripts scripts => scripts.DisplayString(-1, string.Empty),
            IEditableCommandPattern cmd => cmd.Pattern,
            IEditableDictionary<IEditableScripts> scriptDict => JsonSerializer.Serialize(
                scriptDict.Items.Values.Select(i => new ListItemData(i.Key, i.Value.DisplayString(-1, string.Empty)))
                    .ToList(),
                WasmEditorJsonContext.Default.ListListItemData),
            _ => val.ToString()
        };
    }

    private static string DetermineAttributeType(object? val)
    {
        return val switch
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
    }

    private static IEditableScripts? GetScripts(string elementKey, string attribute)
    {
        if (_controller == null)
        {
            return null;
        }

        var data = _controller.GetEditorData(elementKey);
        if (data == null)
        {
            return null;
        }

        var bracketIdx = attribute.IndexOf('[');
        if (bracketIdx >= 0 && attribute.EndsWith(']'))
        {
            var attrName = attribute[..bracketIdx];
            var key = attribute[(bracketIdx + 1)..^1];
            var dict = data.GetAttribute(attrName) as IEditableDictionary<IEditableScripts>;
            if (dict == null || !dict.Items.TryGetValue(key, out var item))
            {
                return null;
            }

            return item.Value;
        }

        return data.GetAttribute(attribute) as IEditableScripts;
    }

    private static IEditableScripts? ResolveContainer(IEditableScripts root, string containerPath)
    {
        if (string.IsNullOrEmpty(containerPath))
        {
            return root;
        }

        var parts = containerPath.Split('/');
        var current = root;
        var i = 0;

        while (i < parts.Length)
        {
            if (!int.TryParse(parts[i], out var scriptIndex))
            {
                return null;
            }

            i++;

            if (i >= parts.Length)
            {
                return null; // path ends at a script index, not a container
            }

            if (scriptIndex < 0 || scriptIndex >= current.Count)
            {
                return null;
            }

            var script = current[scriptIndex];
            var segment = parts[i];
            i++;

            if (script.Type == ScriptType.If)
            {
                var ifScript = (EditableIfScript) script;
                if (segment == "then")
                {
                    current = ifScript.ThenScript;
                }
                else if (segment == "else")
                {
                    if (ifScript.ElseScript == null)
                    {
                        return null;
                    }

                    current = ifScript.ElseScript;
                }
                else if (segment == "elseif")
                {
                    if (i >= parts.Length || !int.TryParse(parts[i], out var elseIfIndex))
                    {
                        return null;
                    }

                    i++;
                    var elseIf = ifScript.ElseIfScripts.ElementAtOrDefault(elseIfIndex);
                    if (elseIf == null)
                    {
                        return null;
                    }

                    current = elseIf.EditableScripts;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (segment == "param")
                {
                    if (i >= parts.Length)
                    {
                        return null;
                    }

                    var paramAttr = parts[i++];
                    var scriptEditorData = _controller!.GetScriptEditorData(script);
                    if (scriptEditorData.GetAttribute(paramAttr) is not IEditableScripts nestedScripts)
                    {
                        return null;
                    }

                    current = nestedScripts;
                }
                else if (segment == "case")
                {
                    // "case" descends into one entry of a "scriptdictionary" parameter (e.g.
                    // switch's "cases") - unlike "param" this needs a second path part beyond
                    // the parameter name, since the parameter itself is keyed. The key is
                    // percent-encoded by the caller so arbitrary case-match expressions (which
                    // may contain "/" or other path-meaningful characters) can't corrupt the
                    // containerPath's own "/"-delimited segmentation.
                    if (i + 1 >= parts.Length)
                    {
                        return null;
                    }

                    var paramAttr = parts[i++];
                    var key = Uri.UnescapeDataString(parts[i++]);
                    var scriptEditorData = _controller!.GetScriptEditorData(script);
                    if (scriptEditorData.GetAttribute(paramAttr) is not IEditableDictionary<IEditableScripts> dict
                        || !dict.Items.TryGetValue(key, out var item))
                    {
                        return null;
                    }

                    current = item.Value;
                }
                else
                {
                    return null;
                }
            }
        }

        return current;
    }

    private static ScriptBlockData BuildScriptBlockData(IEditableScripts scripts)
    {
        return new ScriptBlockData(scripts.Scripts.Select(BuildScriptNodeData).ToList());
    }

    private static ScriptNodeData BuildScriptNodeData(IEditableScript script)
    {
        if (script.Type == ScriptType.If)
        {
            var ifScript = (EditableIfScript) script;
            var elseIfClauses = ifScript.ElseIfScripts
                .Select(ei =>
                    new ElseIfClauseData(ei.Id, ei.Expression, BuildScriptBlockData(ei.EditableScripts).Scripts))
                .ToList();

            return new ScriptNodeData(
                script.Id,
                "if",
                null,
                null,
                ifScript.IfExpression,
                BuildScriptBlockData(ifScript.ThenScript).Scripts,
                elseIfClauses,
                ifScript.ElseScript != null ? BuildScriptBlockData(ifScript.ElseScript).Scripts : null
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
            script.Id,
            "normal",
            displayString,
            controls,
            null,
            null,
            null,
            null
        );
    }

    private static ScriptControlData BuildScriptControlData(IEditorControl ctrl, IEditorData editorData)
    {
        string? value = null;
        string? simpleEditor = null;
        List<ControlOption>? options = null;
        List<ScriptNodeData>? nestedScripts = null;
        List<CaseScriptData>? cases = null;

        if (ctrl.Attribute != null)
        {
            if (ctrl.ControlType == "script")
            {
                var attrValue = editorData.GetAttribute(ctrl.Attribute);
                if (attrValue is IEditableScripts nested)
                {
                    nestedScripts = BuildScriptBlockData(nested).Scripts;
                }
            }
            else if (ctrl.ControlType == "scriptdictionary")
            {
                var attrValue = editorData.GetAttribute(ctrl.Attribute);
                if (attrValue is IEditableDictionary<IEditableScripts> dict)
                {
                    value = SerializeAttributeValue(dict);
                    cases = dict.Items
                        .Select(kv => new CaseScriptData(kv.Key, BuildScriptBlockData(kv.Value.Value).Scripts))
                        .ToList();
                }
            }
            else
            {
                // SerializeAttributeValue (not a plain .ToString()) so a "list" control's
                // IEditableList<string> — e.g. Call function's parameter list — comes through
                // as the same [{key,value}] JSON shape ListEditor.svelte-style controls expect,
                // instead of the useless default object.ToString().
                value = SerializeAttributeValue(editorData.GetAttribute(ctrl.Attribute));
            }
        }

        string? simpleLabel = null;
        string? source = null;
        string? objectType = null;
        // Read for every control type, not just "expression" — e.g. Call function's plain
        // "textbox" attribute-0 control uses <functionpicker/> to ask the frontend for a
        // dropdown of the game's user-defined functions instead of a free-text box. A
        // dedicated tag rather than reusing <objecttype>, which elsewhere always means one
        // of Quest's real object types (dispatched through WorldModel.GetObjectTypeForTypeString)
        // — functions are ElementType.Function, not ElementType.Object, so that machinery
        // doesn't apply to them and overloading the tag would be misleading.
        var isFunctionPicker = ctrl.GetBool("functionpicker");
        // Marks Call function's "list" parameter control (as opposed to e.g. rundelegate's,
        // whose delegate name is a runtime expression so its arity can't be known ahead of
        // time) so the frontend can relabel each box with the selected function's real
        // parameter name and auto-size the box count to match its arity, instead of the
        // generic +/- item list.
        var isFunctionParams = ctrl.GetBool("functionparams");
        var breakBefore = ctrl.GetBool("breakbefore");
        var multiline = ctrl.GetBool("multiline");
        var expand = ctrl.Expand;
        var freetext = ctrl.GetBool("freetext");

        string? useTemplates = null;

        if (ctrl.ControlType == "expression")
        {
            simpleLabel = ctrl.GetString("simple");
            var simpleEditorTag = ctrl.GetString("simpleeditor");
            // If <simpleeditor> is absent but <simple> is set, default simple editor is a textbox
            simpleEditor = simpleEditorTag ?? (simpleLabel != null ? "textbox" : null);
            source = ctrl.GetString("source");
            objectType = ctrl.GetString("objecttype");
            useTemplates = ctrl.GetString("usetemplates");

            if (simpleEditor == "dropdown")
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
        }
        else if (ctrl.ControlType == "dropdown")
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

        var fileFilterName = simpleEditor == "file" ? ctrl.GetString("filefiltername") : null;
        var isNumericSimpleEditor = simpleEditor is "number" or "numberdouble";
        var minimum = isNumericSimpleEditor ? GetNumericHint(ctrl, "minimum") : null;
        var maximum = isNumericSimpleEditor ? GetNumericHint(ctrl, "maximum") : null;
        var increment = isNumericSimpleEditor ? GetNumericHint(ctrl, "increment") : null;

        return new ScriptControlData(
            ctrl.ControlType,
            ctrl.Caption,
            ctrl.Attribute,
            value,
            simpleEditor,
            simpleLabel,
            source,
            options,
            nestedScripts,
            objectType,
            isFunctionPicker,
            isFunctionParams,
            breakBefore,
            useTemplates,
            cases,
            multiline,
            expand,
            fileFilterName,
            freetext,
            minimum,
            maximum,
            increment
        );
    }

    private static void OnAddedNode(object? sender, EditorController.AddedNodeEventArgs e)
    {
        if (_suppressTreeEvents)
        {
            return;
        }

        // Authoritative — mirrors exactly what DeleteElement/DeleteElements will actually allow
        // (see EditorController.CanDelete), rather than the UI guessing from node type/text.
        var node = new TreeNodeData(e.Key, e.Text, e.Parent, e.NodeType,
            e.IsLibraryNode, _controller!.CanDelete(e.Key), e.Filename, e.Folder);
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
            {
                TreeNodes[idx] = node;
            }
            else
            {
                TreeNodes.Add(node);
            }
        }
    }

    private static List<TextProcessorCommand>? GetTextProcessorCommands()
    {
        try
        {
            var data =
                _controller!.GetElementDataAttribute("_RichTextControl_TextProcessorCommands", "data") as IEnumerable;
            if (data == null)
            {
                return null;
            }

            var commands = new List<TextProcessorCommand>();
            foreach (IDictionary<string, string?> commandData in data)
            {
                commandData.TryGetValue("command", out var command);
                commandData.TryGetValue("info", out var info);
                commandData.TryGetValue("insertbefore", out var insertBefore);
                commandData.TryGetValue("insertafter", out var insertAfter);
                if (command != null)
                {
                    commands.Add(new TextProcessorCommand(command, info ?? "", insertBefore ?? "", insertAfter ?? ""));
                }
            }

            return commands.Count > 0 ? commands : null;
        }
        catch
        {
            return null;
        }
    }

    private static ControlInfo ToControlInfo(IEditorControl ctrl, string key)
    {
        List<ControlOption>? options = null;
        var attribute = ctrl.Attribute;
        var freetext = false;

        if (ctrl.ControlType == "dropdown")
        {
            freetext = ctrl.GetBool("freetext");
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
                else
                {
                    var fonts = ctrl.GetString("source") switch
                    {
                        "basefonts" => _controller!.AvailableBaseFonts(),
                        "webfonts" => _controller!.AvailableWebFonts(),
                        _ => null
                    };
                    if (fonts != null)
                    {
                        options = fonts.Select(f => new ControlOption(f, f)).ToList();
                    }
                }
            }
        }
        else if (ctrl.ControlType == "dropdowntypes")
        {
            var dict = ctrl.GetDictionary("types");
            if (dict != null)
            {
                options = dict.Select(kv => new ControlOption(kv.Key, kv.Value)).ToList();
            }

            attribute = ctrl.Id;
        }
        else if (ctrl.ControlType == "filter")
        {
            var dict = ctrl.GetDictionary("filters");
            options = dict?.Select(kv => new ControlOption(kv.Key, kv.Value)).ToList();

            return new ControlInfo(ctrl.Id, "filter", ctrl.Caption, options, null,
                ctrl.GetString("filtergroupname"), Advanced: !ctrl.IsControlVisibleInSimpleMode, Width: ctrl.Width);
        }
        else if (ctrl.ControlType == "objects")
        {
            // Dialogue Pages opt out of world scoping (see CorePages.aslx) - they're data, not
            // physical objects, so they never belong in an attribute-level object picker. An
            // exit's "to" destination is further narrowed to rooms only.
            var names = _controller!.GetObjectNames("object", false).Where(n => !_controller.IsDialoguePage(n));
            if (ctrl.Attribute == "to" && _controller.GetObjectType(key) == "exit")
            {
                names = names.Where(n => _controller.IsRoom(n));
            }

            options = names.Select(n => new ControlOption(n, n)).ToList();
        }
        else if (ctrl.ControlType == "multi")
        {
            var typesDict = ctrl.GetDictionary("types");
            if (typesDict != null)
            {
                options = typesDict.Select(kv => new ControlOption(kv.Key, kv.Value)).ToList();
            }

            var editorsDict = ctrl.GetDictionary("editors");
            var subEditors = editorsDict?.Select(kv => new ControlOption(kv.Key, kv.Value)).ToList();

            var hasRichtextSubEditor = editorsDict?.Values.Any(v => v == "richtext") ?? false;
            var multiTpCommands = hasRichtextSubEditor && !ctrl.GetBool("notextprocessor")
                ? GetTextProcessorCommands()
                : null;

            var caption = ctrl.Caption ?? ctrl.GetString("selfcaption");
            return new ControlInfo(ctrl.Id, ctrl.ControlType, caption, options, subEditors, ctrl.Attribute,
                multiTpCommands, Source: ctrl.GetString("source"), Advanced: !ctrl.IsControlVisibleInSimpleMode, CheckboxCaption: ctrl.GetString("checkbox"));
        }
        else if (ctrl.ControlType == "elementslist")
        {
            return new ControlInfo(null, "elementslist", ctrl.Caption, null, null, null, null, null,
                ctrl.GetString("elementtype"),
                ctrl.GetString("objecttype"),
                ctrl.GetString("listfilter"),
                Advanced: !ctrl.IsControlVisibleInSimpleMode);
        }

        List<TextProcessorCommand>? textProcessorCommands = null;
        if (ctrl.ControlType == "richtext" && !ctrl.GetBool("notextprocessor"))
        {
            textProcessorCommands = GetTextProcessorCommands();
        }

        var addPrompt = ctrl.ControlType == "list" ? ctrl.GetString("editprompt") : null;
        var isWalkthrough = ctrl.ControlType == "list" && ctrl.GetBool("iswalkthrough");
        var isDictionary = ctrl.ControlType is "stringdictionary" or "gamebookoptions";
        var source = ctrl.ControlType == "file" || isDictionary ? ctrl.GetString("source") : null;
        var newFile = ctrl.ControlType == "file" ? ctrl.GetString("newfile") : null;
        var lockedAfterCreate = ctrl.ControlType == "file" && ctrl.GetBool("lockedaftercreate");
        var fileFilterName = ctrl.ControlType == "file" ? ctrl.GetString("filefiltername") : null;
        var keyPrompt = isDictionary ? ctrl.GetString("keyprompt") : null;
        var valuePrompt = isDictionary ? ctrl.GetString("valueprompt") : null;
        var sourceExclude = isDictionary ? ctrl.GetString("sourceexclude") : null;
        // Restricts an object-sourced dictionary control's key dropdown to objects
        // inheriting the named type (e.g. the TA page tab's options control lists
        // dialogue pages only - filtered by type, not location).
        var sourceType = isDictionary ? ctrl.GetString("sourcetype") : null;

        var href = ctrl.ControlType == "label" ? ctrl.GetString("href") : null;

        var isNumeric = ctrl.ControlType is "number" or "numberdouble";
        var minimum = isNumeric ? GetNumericHint(ctrl, "minimum") : null;
        var maximum = isNumeric ? GetNumericHint(ctrl, "maximum") : null;
        var increment = isNumeric ? GetNumericHint(ctrl, "increment") : null;

        // Only meaningful for a control bound to a single attribute value (dropdown, textbox,
        // number, ...) - the dictionary/list/multi controltypes above return early and never
        // reach here, so this can't misfire on something with no single value to clear.
        var nullable = ctrl.GetBool("nullable");

        return new ControlInfo(attribute, ctrl.ControlType, ctrl.Caption ?? ctrl.GetString("selfcaption"), options,
            null, null, textProcessorCommands, addPrompt, Source: source,
            Advanced: !ctrl.IsControlVisibleInSimpleMode,
            KeyPrompt: keyPrompt, ValuePrompt: valuePrompt, SourceExclude: sourceExclude, SourceType: sourceType,
            IsWalkthrough: isWalkthrough, Href: href, NewFile: newFile, LockedAfterCreate: lockedAfterCreate,
            FileFilterName: fileFilterName,
            Freetext: freetext,
            Minimum: minimum, Maximum: maximum, Increment: increment,
            Nullable: nullable,
            Width: ctrl.Width);
    }

    // <minimum>/<maximum>/<increment> can be authored as either an int or a double literal in
    // the .aslx (e.g. a whole-number bound vs. "0.1") - GetInt/GetDouble each only match their
    // own literal type, so try both instead of assuming which one a given hint was written as.
    private static double? GetNumericHint(IEditorControl ctrl, string tag)
    {
        return ctrl.GetDouble(tag) ?? (double?)ctrl.GetInt(tag);
    }

    [JSExport]
    public static string GetGameTemplates()
    {
        var templates = EditorController.GetAvailableTemplates()
            .Values
            .OrderBy(t => t.Type)
            .ThenBy(t => t.TemplateName)
            .Select(t => new GameTemplateInfo(
                t.ResourceName,
                t.TemplateName,
                t.Type == EditorStyle.GameBook ? "gamebook" : "textadventure"))
            .ToList();
        return JsonSerializer.Serialize(templates, WasmEditorJsonContext.Default.ListGameTemplateInfo);
    }

    [JSExport]
    public static string CreateGameFromTemplate(string templateId, string gameName)
    {
        return EditorController.CreateNewGameFile(templateId, gameName);
    }
}