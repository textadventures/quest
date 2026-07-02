// WasmPlayer JS bootstrap — replaces playerweb.js for the WASM-hosted player.
// Defines the WebPlayer object surface that playercore.js / player.js call into,
// then initialises the .NET WASM runtime and wires up [JSImport] callbacks.

var platform = "wasmplayer";

var _audio = null;

const pendingResources = new Map();
let editorChannel = null;
let resourceRoot = null;

function getResourceUrl(name) {
    if (resourceRoot) return Promise.resolve(resourceRoot + name);
    if (!editorChannel) return Promise.resolve(name);
    return new Promise(resolve => {
        const id = crypto.randomUUID();
        pendingResources.set(id, resolve);
        editorChannel.postMessage({ type: 'resource-request', name, id });
    });
}

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
    const playerHtml = await htmResponse.text();

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
        jsYield: () => new Promise(resolve => setTimeout(resolve, 0)),
        setCompassDirections: (dirsJson) => setCompassDirections(JSON.parse(dirsJson)),
        setInterfaceString: (name, text) => setInterfaceString(name, text),
        setPanelContents: (html) => setPanelContents(html),
        consoleError: (msg) => console.error('[Quest]', msg),
        consoleLog: (msg) => console.log('[Quest]', msg),
        getResourceUrl: (name) => getResourceUrl(name),
    });

    await runtime.runMain();

    const config = getConfig();
    const exports = await getAssemblyExports(config.mainAssemblyName);
    Bridge = exports.QuestViva.WasmPlayer.WasmPlayerBridge;

    if (bc) {
        editorChannel = bc;
        bc.onmessage = ({ data }) => {
            if (data.type === 'resource-response') {
                pendingResources.get(data.id)?.(data.dataUrl);
                pendingResources.delete(data.id);
            }
        };
    }

    const ok = await Bridge.Initialise(gameBytes, filename);
    if (!ok) {
        console.error('[Quest] Failed to initialise game');
        throw new Error('Failed to initialise game');
    }

    // Only swap in the game UI once everything has succeeded — keeps the start
    // screen's loading spinner on screen for the whole WASM boot + game-parse
    // sequence instead of cutting to a blank page while that runs.
    document.body.innerHTML = playerHtml;

    await setupPaperJs();

    WebPlayer.gameId = Bridge.GetGameId();
    WebPlayer.initUI();
    WebPlayer.setCanSave(true);

    await Bridge.Begin();
}

// ── Start screen helpers ──────────────────────────────────────────────────────

function _esc(str) {
    return String(str)
        .replace(/&/g, '&amp;').replace(/</g, '&lt;')
        .replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

function showLoading() {
    const pickers = document.getElementById('qv-pickers');
    const msg = document.getElementById('qv-loading-msg');
    if (pickers) pickers.style.display = 'none';
    if (msg) msg.style.display = 'flex';
}

function showError(downloadUrl) {
    const pickers = document.getElementById('qv-pickers');
    const msg = document.getElementById('qv-loading-msg');
    const errorEl = document.getElementById('qv-error');
    if (msg) msg.style.display = 'none';
    // Explicit 'block' (not '') so this wins over the html.qv-booting CSS that
    // hides #qv-pickers by default when a game was specified on the URL — an
    // inline style always beats an external stylesheet rule, cleared or not.
    if (pickers) pickers.style.display = 'block';
    if (!errorEl) return;
    let html = '<strong>Couldn\'t load the game.</strong> '
        + 'The server may not allow this player to load the file directly (CORS restriction).';
    if (downloadUrl) {
        html += ' <a href="' + _esc(downloadUrl) + '" target="_blank" rel="noopener">'
            + 'Open the file in a new tab</a> to save it, then open it with the file picker below.';
    } else {
        html += ' You can try opening a saved copy of the game file instead.';
    }
    errorEl.innerHTML = html;
    // 'block', not '' — errorEl carries the .hidden class, so clearing the
    // inline style would fall back to that (display:none) instead of showing.
    errorEl.style.display = 'block';
    wireStartScreen();
}

let _startScreenWired = false;
function wireStartScreen() {
    if (_startScreenWired) return;
    _startScreenWired = true;

    const fileBtn = document.getElementById('qv-file-btn');
    const fileInput = document.getElementById('qv-file-input');
    const urlInput = document.getElementById('qv-url-input');
    const urlBtn = document.getElementById('qv-url-btn');
    if (!fileBtn) return;

    fileBtn.addEventListener('click', () => fileInput.click());
    fileInput.addEventListener('change', () => {
        const file = fileInput.files?.[0];
        if (!file) return;
        showLoading();
        const reader = new FileReader();
        reader.onload = () => initWasmPlayer(new Uint8Array(reader.result), file.name)
            .catch(() => showError(null));
        reader.readAsArrayBuffer(file);
    });

    async function doLoadUrl() {
        const url = urlInput.value.trim();
        if (!url) return;
        const errorEl = document.getElementById('qv-error');
        if (errorEl) errorEl.style.display = 'none';
        showLoading();
        try {
            const { bytes, filename } = await fetchGameBytes(url);
            await initWasmPlayer(bytes, filename);
        } catch {
            showError(url);
        }
    }
    urlBtn.addEventListener('click', doLoadUrl);
    urlInput.addEventListener('keydown', e => { if (e.key === 'Enter') doLoadUrl(); });
}

async function fetchGameBytes(url) {
    const response = await fetch(url);
    if (!response.ok) throw new Error(`HTTP ${response.status}`);
    const bytes = new Uint8Array(await response.arrayBuffer());
    const filename = (response.url || url).split('/').pop()?.split('?')[0] || 'game.aslx';
    return { bytes, filename };
}

// ── Boot ─────────────────────────────────────────────────────────────────────

(function () {
    const params = new URLSearchParams(window.location.search);

    if (params.get('source') === 'editor') {
        const bc = new BroadcastChannel('quest-preview');
        bc.postMessage({ type: 'ready' });
        bc.onmessage = async ({ data }) => {
            if (data.type === 'game') {
                bc.onmessage = null;
                await initWasmPlayer(data.bytes, data.filename, bc);
            }
        };
        return;
    }

    const id = params.get('id');
    const gameUrl = params.get('game');

    if (id) {
        // Start API fetch immediately; wire DOM once it's ready.
        let resolvedSourceUrl = null;
        const gamePromise = (async () => {
            const apiRoot = window.QuestVivaConfig?.textAdventuresApiRoot
                ?? 'https://textadventures.co.uk/api/';
            const apiResponse = await fetch(`${apiRoot}game/${id}`);
            if (!apiResponse.ok) throw new Error(`API: HTTP ${apiResponse.status}`);
            const { sourceGameUrl, resourceRoot: resRoot } = await apiResponse.json();
            resolvedSourceUrl = sourceGameUrl;
            resourceRoot = resRoot || null;
            const { bytes, filename } = await fetchGameBytes(sourceGameUrl);
            return { bytes, filename };
        })();

        document.addEventListener('DOMContentLoaded', async () => {
            showLoading();
            try {
                const { bytes, filename } = await gamePromise;
                await initWasmPlayer(bytes, filename);
            } catch {
                showError(resolvedSourceUrl);
            }
        });
        return;
    }

    if (gameUrl) {
        // Start fetch immediately; wire DOM once it's ready.
        const gamePromise = fetchGameBytes(gameUrl);

        document.addEventListener('DOMContentLoaded', async () => {
            showLoading();
            try {
                const { bytes, filename } = await gamePromise;
                await initWasmPlayer(bytes, filename);
            } catch {
                showError(gameUrl);
            }
        });
        return;
    }

    // No game specified — show start screen and wire up controls.
    document.addEventListener('DOMContentLoaded', wireStartScreen);
})();
