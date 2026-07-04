// WasmPlayer JS bootstrap — replaces playerweb.js for the WASM-hosted player.
// Defines the WebPlayer object surface that playercore.js / player.js call into,
// then initialises the .NET WASM runtime and wires up [JSImport] callbacks.

var platform = "wasmplayer";

var _audio = null;

const pendingResources = new Map();
let editorChannel = null;
let resourceRoot = null;

// The *original* game bytes/filename for the current session — always the
// primary game file, never a save. Saves are loaded as an overlay on top of
// these via Bridge.InitialiseWithSave, so resource lookups (images/sounds
// bundled in a .quest package) keep working. Set once per boot in startGame()
// and never reassigned by a slot/file load.
let originalGameBytes = null;
let originalGameFilename = null;
let isLegacyGame = false;

function computeGameId(filename) {
    return filename.split('/').pop();
}

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

async function initWasmPlayer(gameBytes, filename, bc = null, saveBytes = null) {
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
        updateObjectLinks: (verbsJson) => updateObjectLinks(JSON.parse(verbsJson)),
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

    const ok = saveBytes
        ? await Bridge.InitialiseWithSave(gameBytes, filename, saveBytes)
        : await Bridge.Initialise(gameBytes, filename);
    if (!ok) {
        console.error('[Quest] Failed to initialise game');
        throw new Error('Failed to initialise game');
    }

    // #qv-saves is a sibling of #qv-start in the static markup, so it would
    // otherwise be destroyed by the innerHTML swap below along with the rest
    // of the start screen — detach it first and re-append it right after, so
    // a failure in between (nothing throws here today, but keep the window
    // tight) can't strand it permanently out of the DOM.
    const savesDialogEl = document.getElementById('qv-saves');
    savesDialogEl?.remove();

    // Only swap in the game UI once everything has succeeded — keeps the start
    // screen's loading spinner on screen for the whole WASM boot + game-parse
    // sequence instead of cutting to a blank page while that runs.
    document.body.innerHTML = playerHtml;

    if (savesDialogEl) document.body.appendChild(savesDialogEl);

    await setupPaperJs();

    WebPlayer.onSaveClick = isLegacyGame
        ? () => GameSaver.save()
        : () => openSavesDialog('manage');
    WebPlayer.initUI();
    WebPlayer.setCanSave(true);

    await Bridge.Begin();
}

// Reloads a save on top of the already-booted WASM runtime — used for
// in-game Load, so it must NOT repeat WebPlayer.initUI()/setupPaperJs():
// those rebind #cmdSave/#cmdDebug click handlers, jQuery-UI widgets, etc.
// unconditionally, and doing that twice on the same live DOM would
// double-bind every one of them.
async function restartGame(saveBytes) {
    document.getElementById('qv-saves')?.close();
    resetGameOutput();

    const ok = await Bridge.InitialiseWithSave(originalGameBytes, originalGameFilename, saveBytes);
    if (!ok) {
        showSavesError('Failed to load that save.');
        return;
    }
    WebPlayer.setCanSave(true);
    await Bridge.Begin();
}

// Wraps initWasmPlayer with the boot-time "Continue / New Game" prompt: if
// saves already exist for this game, ask before committing to either the
// fresh game or a chosen save. Used by all boot entry points below.
async function startGame(bytes, filename, bc = null, gameIdOverride = null) {
    originalGameBytes = bytes;
    originalGameFilename = filename;
    isLegacyGame = /\.(asl|cas)$/i.test(filename);
    // gameIdOverride lets callers with a stronger identity than the filename
    // (e.g. the Text Adventures API's stable game id) use it instead.
    const gameId = gameIdOverride || computeGameId(filename);
    WebPlayer.gameId = gameId;

    let saveBytes = null;
    // Editor-preview sessions (bc) and legacy games are excluded: a preview's
    // filename is transient/reused per edit (prompting every reload would be
    // noisy and would fragment saves under a churny id), and legacy .asl/.cas
    // saves aren't self-contained enough for the full slot manager (see plan).
    if (!bc && !isLegacyGame) {
        let saves = [];
        try { saves = await GameSaver.listSaves(gameId); } catch { saves = []; }
        if (saves.length > 0) {
            const choice = await openSavesDialog('boot', gameId);
            if (choice.type === 'load') {
                saveBytes = await GameSaver.load(choice.slotIndex, gameId);
            }
        }
    }

    await initWasmPlayer(bytes, filename, bc, saveBytes);
}

// ── Start screen helpers ──────────────────────────────────────────────────────

function _esc(str) {
    return String(str)
        .replace(/&/g, '&amp;').replace(/</g, '&lt;')
        .replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

// ── Save/load manager (#qv-saves) ─────────────────────────────────────────────
// One dialog, two modes: "boot" (the lightweight Continue/New-Game prompt,
// shown before a game starts if saves already exist) and "manage" (the
// persistent in-game Save/Load manager, opened via the Save button).

let savesDialogWired = false;
let bootChoiceResolve = null;

function showSavesError(message) {
    const el = document.getElementById('qv-saves-error');
    if (!el) return;
    el.textContent = message;
    el.style.display = 'block';
}

function hideSavesError() {
    const el = document.getElementById('qv-saves-error');
    if (el) el.style.display = 'none';
}

function renderSavesList(saves, mode) {
    const list = document.getElementById('qv-saves-list');
    if (!list) return;
    if (!saves.length) {
        list.innerHTML = '<li class="text-sm text-surface-500">No saved games yet.</li>';
        return;
    }
    // s.name already carries a human-readable date/time when the user didn't
    // type a custom one (see GameSaver.save's default label) — don't also
    // render s.timestamp separately, or the same moment shows up twice in
    // two different formats.
    list.innerHTML = saves.map(s => {
        const deleteBtn = mode === 'manage'
            ? `<button type="button" class="btn-icon preset-tonal-error" data-delete-slot="${s.slotIndex}" aria-label="Delete">`
                + `<svg class="qv-icon" aria-hidden="true"><use href="#trash-2"></use></svg></button>`
            : '';
        return `<li class="flex items-center justify-between gap-2">`
            + `<button type="button" class="anchor text-left flex-1" data-slot="${s.slotIndex}">${_esc(s.name)}</button>`
            + deleteBtn
            + `</li>`;
    }).join('');
}

async function downloadSaveToFile() {
    const bytes = await WebPlayer.uiSaveGame($("#divOutput").html());
    const blob = new Blob([bytes], { type: 'application/xml' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = WebPlayer.gameId.replace(/\.[^.]+$/, '') + '.quest-save';
    a.click();
    URL.revokeObjectURL(url);
}

// A .quest-save only carries state, not resources — it must be paired with
// the currently-loaded game's original bytes to load safely (see plan). The
// save's root <asl original="..."> attribute records which game it's for;
// refuse the load if it doesn't match what's currently running.
async function loadSaveFromFile(file) {
    const bytes = new Uint8Array(await file.arrayBuffer());
    let originalAttr = null;
    try {
        const text = new TextDecoder('utf-8').decode(bytes);
        const xml = new DOMParser().parseFromString(text, 'application/xml');
        originalAttr = xml.documentElement?.getAttribute('original') ?? null;
    } catch { /* falls through to the mismatch error below */ }

    if (!originalAttr || computeGameId(originalAttr) !== WebPlayer.gameId) {
        showSavesError('This save is for a different game — open that game first, then try again.');
        return;
    }
    hideSavesError();
    await restartGame(bytes);
}

function ensureSavesDialogWired() {
    if (savesDialogWired) return;
    savesDialogWired = true;

    const dlg = document.getElementById('qv-saves');
    if (!dlg) return;

    // Non-cancellable while a boot-time choice is pending (matches WebPlayer's
    // Slots dialog); freely closable otherwise.
    dlg.addEventListener('cancel', (e) => {
        if (bootChoiceResolve) e.preventDefault();
    });

    document.getElementById('qv-saves-start-new').addEventListener('click', () => {
        if (!bootChoiceResolve) return;
        const resolve = bootChoiceResolve;
        bootChoiceResolve = null;
        dlg.close();
        resolve({ type: 'new' });
    });

    document.getElementById('qv-saves-list').addEventListener('click', async (e) => {
        const loadBtn = e.target.closest('[data-slot]');
        if (loadBtn) {
            const slotIndex = Number(loadBtn.dataset.slot);
            if (bootChoiceResolve) {
                const resolve = bootChoiceResolve;
                bootChoiceResolve = null;
                dlg.close();
                resolve({ type: 'load', slotIndex });
            } else {
                const bytes = await GameSaver.load(slotIndex, WebPlayer.gameId);
                await restartGame(bytes);
            }
            return;
        }
        const delBtn = e.target.closest('[data-delete-slot]');
        if (delBtn) {
            const name = delBtn.closest('li')?.querySelector('[data-slot]')?.textContent ?? 'this save';
            if (!confirm(`Delete "${name}"? This can't be undone.`)) return;
            await GameSaver.deleteSlot(Number(delBtn.dataset.deleteSlot), WebPlayer.gameId);
            renderSavesList(await GameSaver.listSaves(WebPlayer.gameId), 'manage');
        }
    });

    document.getElementById('qv-saves-save-new').addEventListener('click', async () => {
        const input = document.getElementById('qv-saves-name-input');
        await GameSaver.save(input.value.trim() || undefined);
        input.value = '';
        renderSavesList(await GameSaver.listSaves(WebPlayer.gameId), 'manage');
    });

    document.getElementById('qv-saves-download').addEventListener('click', downloadSaveToFile);

    const uploadBtn = document.getElementById('qv-saves-upload-btn');
    const fileInput = document.getElementById('qv-saves-file-input');
    uploadBtn.addEventListener('click', () => fileInput.click());
    fileInput.addEventListener('change', async () => {
        const file = fileInput.files?.[0];
        fileInput.value = '';
        if (file) await loadSaveFromFile(file);
    });

    document.getElementById('qv-saves-close').addEventListener('click', () => dlg.close());
}

// mode: 'boot' resolves {type:'new'} or {type:'load', slotIndex} once the
// user picks; 'manage' just opens the persistent manager (callers act via
// the wired-up click handlers above, nothing to await).
async function openSavesDialog(mode, gameId = WebPlayer.gameId) {
    ensureSavesDialogWired();
    const dlg = document.getElementById('qv-saves');
    hideSavesError();
    dlg.dataset.mode = mode;
    renderSavesList(await GameSaver.listSaves(gameId), mode);

    if (mode === 'boot') {
        return new Promise(resolve => {
            bootChoiceResolve = resolve;
            dlg.showModal();
        });
    }

    dlg.showModal();
}

function showLoading() {
    const pickers = document.getElementById('qv-pickers');
    const msg = document.getElementById('qv-loading-msg');
    if (pickers) pickers.style.display = 'none';
    if (msg) msg.style.display = 'flex';
}

function showError(downloadUrl, isLocalFile) {
    const pickers = document.getElementById('qv-pickers');
    const msg = document.getElementById('qv-loading-msg');
    const errorEl = document.getElementById('qv-error');
    if (msg) msg.style.display = 'none';
    // Explicit 'block' (not '') so this wins over the html.qv-booting CSS that
    // hides #qv-pickers by default when a game was specified on the URL — an
    // inline style always beats an external stylesheet rule, cleared or not.
    if (pickers) pickers.style.display = 'block';
    if (!errorEl) return;
    let html;
    if (isLocalFile) {
        html = '<strong>Couldn\'t open that file.</strong> '
            + 'It doesn\'t look like a Quest game file (.quest, .aslx, .asl or .cas). Choose a different file below.';
    } else if (downloadUrl) {
        html = '<strong>Couldn\'t load the game from that URL.</strong> '
            + 'The site hosting it likely isn\'t configured to allow other sites to fetch the file directly '
            + '(a CORS restriction) &mdash; this is a limitation of the hosting site, not something wrong with your download. '
            + '<a class="anchor" href="' + _esc(downloadUrl) + '" target="_blank" rel="noopener">'
            + 'Open the file in a new tab</a>, save it to your computer, then load it with the file picker below.';
    } else {
        html = '<strong>Couldn\'t load the game.</strong> '
            + 'Try opening a saved copy of the game file instead, using the file picker below.';
    }
    errorEl.innerHTML = html;
    // 'block', not '' — errorEl carries the .hidden class, so clearing the
    // inline style would fall back to that (display:none) instead of showing.
    errorEl.style.display = 'block';
    wireStartScreen();
}

// Keeps the address bar in sync with the game that's actually loaded (or being
// attempted), so a refresh reproduces it. `name`/`value` set the active source
// param; both `id` and `url` are cleared first since only one can be current.
function setLocationParam(name, value) {
    const params = new URLSearchParams(window.location.search);
    params.delete('id');
    params.delete('url');
    if (name) params.set(name, value);
    const query = params.toString();
    const newUrl = window.location.pathname + (query ? `?${query}` : '') + window.location.hash;
    history.replaceState(null, '', newUrl);
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
        setLocationParam(null);
        showLoading();
        const reader = new FileReader();
        reader.onload = () => startGame(new Uint8Array(reader.result), file.name)
            .catch(() => showError(null, true));
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
            await startGame(bytes, filename);
            setLocationParam('url', url);
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
                await startGame(data.bytes, data.filename, bc);
            }
        };
        return;
    }

    const id = params.get('id');
    const gameUrl = params.get('url');

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
                await startGame(bytes, filename, null, id);
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
                await startGame(bytes, filename);
            } catch {
                showError(gameUrl);
            }
        });
        return;
    }

    // No game specified — show start screen and wire up controls.
    document.addEventListener('DOMContentLoaded', wireStartScreen);
})();
