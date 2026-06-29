// WasmPlayer JS bootstrap — replaces playerweb.js for the WASM-hosted player.
// Defines the WebPlayer object surface that playercore.js / player.js call into,
// then initialises the .NET WASM runtime and wires up [JSImport] callbacks.

var platform = "wasmplayer";

var _audio = null;

function ui_init() { }

function sendEndWait() {
    window.setTimeout(async function () {
        await WebPlayer.uiEndWait();
    }, 100);
    waitEnded();
}

function afterSendCommand() { }

function playSound(url, synchronous, looped) {
    stopSound();
    _audio = new Audio(url);
    if (looped) _audio.loop = true;
    if (synchronous) {
        var showCmdDiv = isElementVisible("#txtCommandDiv");
        _waitingForSoundToFinish = true;
        $("#txtCommandDiv").hide();
        _audio.addEventListener('ended', function () { finishSync(showCmdDiv); });
    }
    _audio.play();
}

function stopSound() {
    if (_audio !== null) {
        _audio.pause();
        _audio.src = '';
        _audio = null;
    }
}

function finishSync(showCommandDiv) {
    _waitingForSoundToFinish = false;
    window.setTimeout(async function () {
        if (showCommandDiv) $("#txtCommandDiv").show();
        await WebPlayer.uiEndWait();
    }, 100);
}

// The WebPlayer object — same surface API as playerweb.js so that playercore.js
// and player.js can call into it without modification.
window.WebPlayer = {
    gameId: null,

    initUI() { initPlayerUI(); },

    setCanDebug(value) {
        const cmdDebug = document.getElementById("cmdDebug");
        if (cmdDebug) cmdDebug.style.display = value ? "initial" : "none";
    },

    setCanSave(value) {
        const cmdSave = document.getElementById("cmdSave");
        if (cmdSave) cmdSave.style.display = value ? "initial" : "none";
        if (!value) window.saveGame = () => addText("Disabled");
    },

    setAnimateScroll(value) { _animateScroll = value; },

    runJs(scripts) {
        const globalEval = window.eval;
        for (const script of scripts) {
            try { globalEval(script); } catch (e) { console.error(e); }
        }
    },

    async sendCommand(command, tickCount, metadata) {
        const metadataJson = metadata ? JSON.stringify(metadata) : null;
        await Bridge.SendCommand(command, tickCount, metadataJson);
        canSendCommand = true;
    },

    async uiChoice(choice) { await Bridge.SetMenuResponse(choice); },
    async uiChoiceCancel() { await Bridge.SetMenuResponse(null); },
    async uiTick(tickCount) { await Bridge.Tick(tickCount); },
    async uiEndWait() { await Bridge.FinishWait(); },
    async uiEndPause() { await Bridge.FinishPause(); },
    async uiSetQuestionResponse(response) { await Bridge.SetQuestionResponse(response); },

    async uiSendEvent(eventName, param) {
        await Bridge.SendEvent(eventName, param);
        canSendCommand = true;
    },

    async uiSaveGame(html) {
        const base64 = await Bridge.SaveGame(html);
        const binary = atob(base64);
        const bytes = new Uint8Array(binary.length);
        for (let i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i);
        return bytes;
    },

    listSaves: async () => GameSaver.listSaves(),
    loadSlot: async (slot) => GameSaver.load(slot),
};

// Exported bridge reference — set once WASM is loaded.
var Bridge;

function addPaperScript() {
    const canvas = document.getElementById('gridCanvas');
    const gridPanel = document.getElementById('gridPanel');
    if (canvas instanceof HTMLCanvasElement && gridPanel) {
        gridPanel.appendChild(canvas);
        canvas.style.display = '';
        if (window.paper && paper.view) {
            paper.view.viewSize = new paper.Size(canvas.clientWidth, canvas.clientHeight);
        }
    }
}

async function setupPaperJs() {
    if (!window.paper) return;
    const canvas = document.createElement('canvas');
    canvas.id = 'gridCanvas';
    canvas.width = 700;
    canvas.height = 300;
    canvas.style.display = 'none';
    document.body.appendChild(canvas);
    paper.setup(canvas);
    const code = await fetch('grid.js').then(r => r.text());
    paper.PaperScript.evaluate(code, paper);
}

async function initWasmPlayer(gameBytes, filename, bc = null) {
    const [htmResponse, { dotnet }] = await Promise.all([
        fetch('playercore.htm'),
        import('./_framework/dotnet.js'),
    ]);
    document.body.innerHTML = await htmResponse.text();

    const runtime = await dotnet.create();
    const { setModuleImports, getAssemblyExports, getConfig } = runtime;

    setModuleImports("wasm-player", {
        addTextAndScroll: (html) => addTextAndScroll(html),
        createNewDiv: (alignment) => createNewDiv(alignment),
        bindMenu: (linkId, verbs, text, elementId) => bindMenu(linkId, verbs, text, elementId),
        showMenu: (caption, optionsJson, allowCancel) => showMenu(caption, JSON.parse(optionsJson), allowCancel),
        showQuestion: (caption) => showQuestion(caption),
        beginWait: () => beginWait(),
        beginPause: (ms) => beginPause(ms),
        updateLocation: (loc) => updateLocation(loc),
        setGameName: (name) => setGameName(name),
        clearScreen: () => clearScreen(),
        panesVisible: (visible) => panesVisible(visible),
        updateStatus: (text) => updateStatus(text),
        setBackground: (colour) => setBackground(colour),
        setForeground: (colour) => setForeground(colour),
        updateList: (listName, itemsJson) => updateList(listName, JSON.parse(itemsJson)),
        updateCompass: (data) => updateCompass(data),
        gameFinished: () => gameFinished(),
        requestNextTimerTick: (seconds) => requestNextTimerTick(seconds),
        uiShow: (element) => uiShow(element),
        uiHide: (element) => uiHide(element),
        addExternalScript: (url) => addExternalScript(url),
        addExternalStylesheet: (url) => addExternalStylesheet(url),
        playSound: (url, synchronous, looped) => playSound(url, synchronous, looped),
        stopSound: () => stopSound(),
        runScript: (call) => {
            const globalEval = window.eval;
            try { globalEval(call); } catch (e) { console.error(e); }
        },
        setCompassDirections: (dirsJson) => setCompassDirections(JSON.parse(dirsJson)),
        setInterfaceString: (name, text) => setInterfaceString(name, text),
        setPanelContents: (html) => setPanelContents(html),
        consoleError: (msg) => console.error('[Quest]', msg),
        consoleLog: (msg) => console.log('[Quest]', msg),
    });

    await runtime.runMain();

    const config = getConfig();
    const exports = await getAssemblyExports(config.mainAssemblyName);
    Bridge = exports.QuestViva.WasmPlayer.WasmPlayerBridge;

    const ok = await Bridge.Initialise(gameBytes, filename);
    if (!ok) {
        console.error('[Quest] Failed to initialise game');
        return;
    }

    await setupPaperJs();

    WebPlayer.gameId = Bridge.GetGameId();
    WebPlayer.initUI();
    WebPlayer.setCanSave(true);

    Bridge.Begin();
}

(function () {
    const params = new URLSearchParams(window.location.search);

    if (params.get('source') === 'editor') {
        const bc = new BroadcastChannel('quest-preview');
        bc.postMessage({ type: 'ready' });
        bc.onmessage = async ({ data }) => {
            if (data.type === 'game') {
                bc.onmessage = null;   // hand off to resource handler (Phase 3)
                await initWasmPlayer(data.bytes, data.filename, bc);
            }
        };
        return;
    }

    const gameUrl = params.get('game');
    if (!gameUrl) {
        document.addEventListener('DOMContentLoaded', () => {
            document.body.innerHTML = '<p style="font-family:sans-serif;padding:2em">No game specified. Add <code>?game=path/to/game.aslx</code> to the URL.</p>';
        });
        return;
    }
    const filename = gameUrl.split('/').pop() || 'game.aslx';
    (async () => {
        const response = await fetch(gameUrl);
        if (!response.ok) {
            document.body.innerHTML = `<p>Failed to load game: ${response.statusText}</p>`;
            return;
        }
        const gameBytes = new Uint8Array(await response.arrayBuffer());
        await initWasmPlayer(gameBytes, filename);
    })().catch(e => console.error('[Quest] Init failed:', e));
})();
