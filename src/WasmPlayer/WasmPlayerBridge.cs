using System;
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
using QuestViva.Engine;
using QuestViva.PlayerCore;

namespace QuestViva.WasmPlayer;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class WasmPlayerBridge
{
    private static IGame? _game;
    private static PlayerHelper? _helper;
    private static WasmPlayerUi? _ui;

    // ── JS → C# exports ─────────────────────────────────────────────────────

    [JSExport]
    public static async Task<bool> Initialise(byte[] gameFileBytes, string filename)
    {
        _game?.Finish();

        var provider = new ByteArrayGameDataProvider(gameFileBytes, filename);
        var gameData = await provider.GetData();
        if (gameData == null) return false;

        var launcher = new GameLauncher(new WorldModelFactory());
        _game = launcher.GetGame(gameData, null);
        if (_game == null) return false;

        _ui = new WasmPlayerUi(gameData.GameId, _game);
        _helper = new PlayerHelper(_game, _ui);
        _ui.SetHelper(_helper);

        _game.LogError += ex =>
        {
            _ui.OutputText("[Sorry, an error occurred]");
            JsConsoleError(ex.Message);
        };
        _game.UpdateList += (listType, items) => _ui.HandleUpdateList(listType, items);
        _game.Finished += () => _ui.HandleFinished();
        _game.RequestNextTimerTick += seconds => _ui.SetPendingTimerTick(seconds);

        var (ok, errors) = await _helper.Initialise(_ui);
        if (!ok)
        {
            _ui.OutputText(string.Join("<br/>", errors) + "<br/>");
            _ui.FlushBuffer();
            return false;
        }

        var scripts = _game.GetExternalScripts();
        if (scripts != null)
        {
            foreach (var script in scripts)
                await JsAddExternalScript(await _ui.GetUrlAsync(script));
        }

        var stylesheets = _game.GetExternalStylesheets();
        if (stylesheets != null)
        {
            foreach (var stylesheet in stylesheets)
                JsAddExternalStylesheet(stylesheet);
        }

        return true;
    }

    [JSExport]
    public static string GetGameId() => _ui?.GameId ?? string.Empty;

    [JSExport]
    public static async Task Begin()
    {
        if (_game == null || _ui == null) return;
        await _game.Begin();
        _ui.FlushBuffer();
    }

    [JSExport]
    public static async Task SendCommand(string command, int tickCount, string? metadataJson)
    {
        if (_game == null || _ui == null || _ui.IsFinished) return;
        IDictionary<string, string> metadata = string.IsNullOrEmpty(metadataJson)
            ? new Dictionary<string, string>()
            : JsonSerializer.Deserialize(metadataJson, WasmJsonContext.Default.DictionaryStringString)
              ?? new Dictionary<string, string>();
        await _game.SendCommand(command, tickCount, metadata);
        _ui.FlushBuffer();
    }

    [JSExport]
    public static async Task SendEvent(string eventName, string param)
    {
        if (_game == null || _ui == null || _ui.IsFinished) return;
        await _game.SendEvent(eventName, param);
        _ui.FlushBuffer();
    }

    [JSExport]
    public static async Task FinishWait()
    {
        if (_game == null || _ui == null) return;
        await _game.FinishWait();
        _ui.FlushBuffer();
    }

    [JSExport]
    public static async Task FinishPause()
    {
        if (_game == null || _ui == null) return;
        await _game.FinishPause();
        _ui.FlushBuffer();
    }

    [JSExport]
    public static async Task SetMenuResponse(string? response)
    {
        if (_game == null || _ui == null) return;
        await _game.SetMenuResponse(response);
        _ui.FlushBuffer();
    }

    [JSExport]
    public static async Task SetQuestionResponse(bool response)
    {
        if (_game == null || _ui == null) return;
        await _game.SetQuestionResponse(response);
        _ui.FlushBuffer();
    }

    [JSExport]
    public static async Task Tick(int elapsedTime)
    {
        if (_game == null || _ui == null || _ui.IsFinished) return;
        await _game.Tick(elapsedTime);
        _ui.FlushBuffer();
    }

    [JSExport]
    public static async Task<string> SaveGame(string html)
    {
        if (_game == null) return string.Empty;
        var bytes = await _game.SaveAsync(html);
        return Convert.ToBase64String(bytes);
    }

    // ── C# → JS imports ─────────────────────────────────────────────────────

    [JSImport("addTextAndScroll", "wasm-player")]
    internal static partial void JsAddTextAndScroll(string html);

    [JSImport("createNewDiv", "wasm-player")]
    internal static partial void JsCreateNewDiv(string alignment);

    [JSImport("bindMenu", "wasm-player")]
    internal static partial void JsBindMenu(string linkId, string verbs, string text, string elementId);

    [JSImport("showMenu", "wasm-player")]
    internal static partial void JsShowMenu(string caption, string optionsJson, bool allowCancel);

    [JSImport("showQuestion", "wasm-player")]
    internal static partial void JsShowQuestion(string caption);

    [JSImport("beginWait", "wasm-player")]
    internal static partial void JsBeginWait();

    [JSImport("beginPause", "wasm-player")]
    internal static partial void JsBeginPause(int ms);

    [JSImport("updateLocation", "wasm-player")]
    internal static partial void JsUpdateLocation(string location);

    [JSImport("setGameName", "wasm-player")]
    internal static partial void JsSetGameName(string name);

    [JSImport("clearScreen", "wasm-player")]
    internal static partial void JsClearScreen();

    [JSImport("panesVisible", "wasm-player")]
    internal static partial void JsPanesVisible(bool visible);

    [JSImport("updateStatus", "wasm-player")]
    internal static partial void JsUpdateStatus(string text);

    [JSImport("setBackground", "wasm-player")]
    internal static partial void JsSetBackground(string colour);

    [JSImport("setForeground", "wasm-player")]
    internal static partial void JsSetForeground(string colour);

    [JSImport("updateList", "wasm-player")]
    internal static partial void JsUpdateList(string listName, string itemsJson);

    [JSImport("updateCompass", "wasm-player")]
    internal static partial void JsUpdateCompass(string data);

    [JSImport("gameFinished", "wasm-player")]
    internal static partial void JsGameFinished();

    [JSImport("requestNextTimerTick", "wasm-player")]
    internal static partial void JsRequestNextTimerTick(int seconds);

    [JSImport("uiShow", "wasm-player")]
    internal static partial void JsUiShow(string element);

    [JSImport("uiHide", "wasm-player")]
    internal static partial void JsUiHide(string element);

    [JSImport("addExternalScript", "wasm-player")]
    internal static partial Task JsAddExternalScript(string url);

    [JSImport("addExternalStylesheet", "wasm-player")]
    internal static partial void JsAddExternalStylesheet(string url);

    [JSImport("playSound", "wasm-player")]
    internal static partial void JsPlaySound(string url, bool synchronous, bool looped);

    [JSImport("stopSound", "wasm-player")]
    internal static partial void JsStopSound();

    [JSImport("runScript", "wasm-player")]
    internal static partial void JsRunScript(string call);

    [JSImport("jsYield", "wasm-player")]
    internal static partial Task JsYield();

    [JSImport("setCompassDirections", "wasm-player")]
    internal static partial void JsSetCompassDirections(string dirsJson);

    [JSImport("setInterfaceString", "wasm-player")]
    internal static partial void JsSetInterfaceString(string name, string text);

    [JSImport("setPanelContents", "wasm-player")]
    internal static partial void JsSetPanelContents(string html);

    [JSImport("consoleError", "wasm-player")]
    internal static partial void JsConsoleError(string message);

    [JSImport("consoleLog", "wasm-player")]
    internal static partial void JsConsoleLog(string message);

    [JSImport("getResourceUrl", "wasm-player")]
    internal static partial Task<string> JsGetResourceUrl(string filename);

    // ── Inner UI class ───────────────────────────────────────────────────────

    private static readonly Dictionary<string, string> ElementMap = new()
    {
        { "Panes", "#gamePanes" },
        { "Location", "#location" },
        { "Command", "#txtCommandDiv" }
    };

    private sealed class WasmPlayerUi : IPlayerHelperUI
    {
        private readonly Dictionary<string, string> _resourceUrls = new(StringComparer.OrdinalIgnoreCase);
        private readonly ListHandler _listHandler;
        private readonly IGame _game;
        private PlayerHelper? _helper;

        // Buffered JS calls — flushed at turn end or before any interactive call.
        private readonly List<Action> _uiBuffer = new();
        // Only the last requestNextTimerTick per turn matters; older ones are stale.
        private int? _pendingTimerTick = null;

        public string GameId { get; }
        public bool IsFinished { get; private set; }

        public WasmPlayerUi(string gameId, IGame game)
        {
            GameId = gameId;
            _game = game;
            _listHandler = new ListHandler((identifier, args) =>
            {
                if (identifier == "updateList"
                    && args?.Length == 2
                    && args[0] is string listName
                    && args[1] is Dictionary<string, string> items)
                {
                    var json = JsonSerializer.Serialize(items, WasmJsonContext.Default.DictionaryStringString);
                    _uiBuffer.Add(() => JsUpdateList(listName, json));
                }
                else if (identifier == "updateCompass"
                         && args?.Length == 1
                         && args[0] is string compassData)
                {
                    _uiBuffer.Add(() => JsUpdateCompass(compassData));
                }
            });
        }

        public void SetHelper(PlayerHelper helper) => _helper = helper;

        public async Task<string> GetUrlAsync(string filename)
        {
            if (_resourceUrls.TryGetValue(filename, out var cached))
                return cached;

            var stream = _game.GetResourceStream(filename);
            if (stream != null)
            {
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                var mimeType = PlayerHelper.GetContentType(filename);
                var dataUrl = $"data:{mimeType};base64,{Convert.ToBase64String(ms.ToArray())}";
                _resourceUrls[filename] = dataUrl;
                return dataUrl;
            }

            var url = await JsGetResourceUrl(filename);
            _resourceUrls[filename] = url;
            return url;
        }

        // Flush accumulated PlayerHelper text into the buffer (preserving ordering
        // relative to other buffered calls), then execute every buffered call and
        // emit the pending timer tick if one exists.
        public void FlushBuffer()
        {
            if (_helper != null)
                OutputText(_helper.ClearBuffer());

            var actions = _uiBuffer.ToArray();
            _uiBuffer.Clear();

            foreach (var action in actions)
                action();

            if (_pendingTimerTick.HasValue)
            {
                JsRequestNextTimerTick(_pendingTimerTick.Value);
                _pendingTimerTick = null;
            }
        }

        // Flush the buffer before an interactive call that suspends C# waiting for
        // a JS response — the JS side must see all preceding output first.
        private void FlushBeforeInteraction()
        {
            FlushBuffer();
        }

        public void SetPendingTimerTick(int seconds)
        {
            _pendingTimerTick = seconds;
        }

        public void FlushText()
        {
            if (_helper == null) return;
            OutputText(_helper.ClearBuffer());
        }

        public void HandleUpdateList(ListType listType, List<ListData> items) =>
            _listHandler.UpdateList(listType, items);

        public void HandleFinished()
        {
            _uiBuffer.Add(JsGameFinished);
            IsFinished = true;
        }

        // IPlayerHelperUI
        public void OutputText(string text)
        {
            if (text.Length == 0) return;
            text = text.Replace("\n", "").Replace("\r", "");
            _uiBuffer.Add(() => JsAddTextAndScroll(text));
        }

        public void SetAlignment(string alignment)
        {
            if (alignment.Length == 0) alignment = "left";
            FlushText();
            _uiBuffer.Add(() => JsCreateNewDiv(alignment));
        }

        public void BindMenu(string linkid, string verbs, string text, string elementId) =>
            _uiBuffer.Add(() => JsBindMenu(linkid, verbs, text, elementId));

        // IPlayer
        void IPlayer.ShowMenu(MenuData menuData)
        {
            FlushBeforeInteraction();
            JsShowMenu(menuData.Caption,
                JsonSerializer.Serialize(
                    menuData.Options as Dictionary<string, string> ?? new Dictionary<string, string>(menuData.Options),
                    WasmJsonContext.Default.DictionaryStringString),
                menuData.AllowCancel);
        }

        void IPlayer.DoWait()
        {
            FlushBeforeInteraction();
            JsBeginWait();
        }

        void IPlayer.DoPause(int ms)
        {
            FlushBeforeInteraction();
            JsBeginPause(ms);
        }

        void IPlayer.ShowQuestion(string caption)
        {
            FlushBeforeInteraction();
            JsShowQuestion(caption);
        }

        void IPlayer.SetWindowMenu(MenuData menuData) { }

        async Task IPlayer.PlaySoundAsync(string filename, bool synchronous, bool looped)
        {
            FlushBeforeInteraction();
            JsPlaySound(await GetUrlAsync(filename), synchronous, looped);
        }

        void IPlayer.StopSound() => JsStopSound();

        void IPlayer.WriteHTML(string html) => OutputText(html);

        Task<string> IPlayer.GetUrlAsync(string filename) => GetUrlAsync(filename);

        void IPlayer.LocationUpdated(string location) =>
            _uiBuffer.Add(() => JsUpdateLocation(location));

        void IPlayer.UpdateGameName(string name) =>
            _uiBuffer.Add(() => JsSetGameName(name));

        void IPlayer.ClearScreen()
        {
            FlushText();
            _uiBuffer.Add(JsClearScreen);
        }

        async Task IPlayer.ShowPictureAsync(string filename)
        {
            FlushText();
            var url = await GetUrlAsync(filename);
            _uiBuffer.Add(() => JsAddTextAndScroll($"<img src=\"{url}\" onload=\"scrollToEnd();\" /><br />"));
        }

        void IPlayer.SetPanesVisible(string data) =>
            _uiBuffer.Add(() => JsPanesVisible(data == "on"));

        void IPlayer.SetStatusText(string text) =>
            _uiBuffer.Add(() => JsUpdateStatus(text.Replace(System.Environment.NewLine, "<br />")));

        void IPlayer.SetBackground(string colour) =>
            _uiBuffer.Add(() => JsSetBackground(colour));

        void IPlayer.SetForeground(string colour)
        {
            _uiBuffer.Add(() => JsSetForeground(colour));
            _helper?.SetForeground(colour);
        }

        void IPlayer.SetLinkForeground(string colour) => _helper?.SetLinkForeground(colour);

        async Task IPlayer.RunScriptAsync(string function, object[]? parameters)
        {
            FlushBuffer();
            await JsYield();

            // Strip newlines from string parameters — some games depend on this (matching WebPlayer behaviour)
            var processedParams = parameters?.Select(p =>
                p is string s ? (object)s.Replace("\r", "").Replace("\n", "") : p).ToArray();

            // When function is "eval", pass the code directly rather than wrapping it in eval(...)
            // so it runs in global scope (matching WebPlayer behaviour; e.g. spondre defines global functions this way)
            if (function == "eval" && processedParams is [string evalCode])
            {
                JsRunScript(evalCode);
                return;
            }

            var serializedArgs = string.Join(',',
                processedParams?.Select(SerializeJsArg) ?? System.Array.Empty<string>());
            JsRunScript($"{function}({serializedArgs})");
        }

        private static string SerializeJsArg(object? arg) => arg switch
        {
            null => "null",
            string s => JsonSerializer.Serialize(s, WasmJsonContext.Default.String),
            bool b => b ? "true" : "false",
            int i => i.ToString(CultureInfo.InvariantCulture),
            long l => l.ToString(CultureInfo.InvariantCulture),
            double d => d.ToString(CultureInfo.InvariantCulture),
            float f => f.ToString(CultureInfo.InvariantCulture),
            IDictionary<string, string> dict => JsonSerializer.Serialize(
                dict as Dictionary<string, string> ?? new Dictionary<string, string>(dict),
                WasmJsonContext.Default.DictionaryStringString),
            IEnumerable<string> list => JsonSerializer.Serialize(list.ToArray(), WasmJsonContext.Default.StringArray),
            _ => JsonSerializer.Serialize(arg.ToString() ?? "", WasmJsonContext.Default.String),
        };

        void IPlayer.Quit() { }

        void IPlayer.SetFont(string fontName) => _helper?.SetFont(fontName);

        void IPlayer.SetFontSize(string fontSize) => _helper?.SetFontSize(fontSize);

        void IPlayer.Speak(string text) { }

        void IPlayer.RequestSave(string html) => JsRunScript("saveGame()");

        void IPlayer.Show(string element)
        {
            if (ElementMap.TryGetValue(element, out var jsElement))
                _uiBuffer.Add(() => JsUiShow(jsElement));
        }

        void IPlayer.Hide(string element)
        {
            if (ElementMap.TryGetValue(element, out var jsElement))
                _uiBuffer.Add(() => JsUiHide(jsElement));
        }

        void IPlayer.SetCompassDirections(IEnumerable<string> dirs) =>
            _uiBuffer.Add(() => JsSetCompassDirections(JsonSerializer.Serialize(dirs.ToArray(), WasmJsonContext.Default.StringArray)));

        void IPlayer.SetInterfaceString(string name, string text) =>
            _uiBuffer.Add(() => JsSetInterfaceString(name, text));

        void IPlayer.SetPanelContents(string html) =>
            _uiBuffer.Add(() => JsSetPanelContents(html));

        void IPlayer.Log(string text) => JsConsoleLog(text);

        string? IPlayer.GetUIOption(UIOption option) =>
            option is UIOption.UseGameColours or UIOption.UseGameFont ? "true" : null;
    }
}

[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(string))]
internal partial class WasmJsonContext : JsonSerializerContext { }
