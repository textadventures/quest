// WasmPlayer JS bootstrap — replaces playerweb.js for the WASM-hosted player.
// Defines the WebPlayer object surface that playercore.js / player.js call into,
// then initialises the .NET WASM runtime and wires up [JSImport] callbacks.

// Printed as soon as this script runs (i.e. as the start screen appears) —
// window.QuestVivaVersion comes from the repo-root VERSION file, spliced in
// at build time by scripts/inject-version.mjs, so this doesn't need to wait
// for the WASM runtime to boot (which only happens once a game loads).
console.log(
    '%cQuest Viva %c' + (window.QuestVivaVersion || 'dev') + '\n%chttps://questviva.com',
    'font-weight:700;font-size:14px;color:#0ea5e9',
    'font-weight:400;color:#64748b',
    'color:#64748b'
);

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
        canSave = value;
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

    // Swap in the game UI *before* Initialise/Begin run, not after. Initialise
    // registers the game's external <javascript> resources (RegisterExternalScripts),
    // and games commonly use those to manipulate the game UI's own DOM (e.g.
    // inserting a custom pane next to the built-in Inventory/Status panes). If
    // the swap happens later, those scripts execute while the start screen is
    // still showing, find none of the elements they're looking for, and
    // silently no-op — a real game, "A Stranger Unregarded", does exactly this
    // with a custom Inventory2 pane that never appeared because of this
    // ordering.
    //
    // #qv-start is a fixed, full-viewport overlay (see chrome.css), so keeping
    // it in the DOM on top of the freshly-swapped-in game UI still covers the
    // screen exactly as before — the loading spinner/error UI's visible
    // behaviour is unchanged. #qv-saves is a sibling of #qv-start in the
    // static markup, so it'd also be destroyed by the innerHTML swap —
    // detach both first and re-append them right after.
    const startScreenEl = document.getElementById('qv-start');
    const savesDialogEl = document.getElementById('qv-saves');
    startScreenEl?.remove();
    savesDialogEl?.remove();

    document.body.innerHTML = playerHtml;

    if (startScreenEl) document.body.appendChild(startScreenEl);
    if (savesDialogEl) document.body.appendChild(savesDialogEl);

    const ok = saveBytes
        ? await Bridge.InitialiseWithSave(gameBytes, filename, saveBytes)
        : await Bridge.Initialise(gameBytes, filename);
    if (!ok) {
        console.error('[Quest] Failed to initialise game');
        throw new Error('Failed to initialise game');
    }

    // Now that startup has actually succeeded, remove the start screen overlay
    // to reveal the game UI underneath.
    startScreenEl?.remove();

    await setupPaperJs();

    WebPlayer.onSaveClick = () => openSavesDialog('manage');
    WebPlayer.initUI();
    // Editor-preview sessions (bc) don't offer saving at all — these are
    // transient test runs launched from the game editor, not real play
    // sessions worth persisting.
    WebPlayer.setCanSave(!bc);

    await Bridge.Begin();
}

// Reloads a save (or, with saveBytes null, the fresh original game) on top
// of the already-booted WASM runtime — used for in-game Load and "Start
// from the beginning", so it must NOT repeat WebPlayer.initUI()/setupPaperJs():
// those rebind #cmdSave/#cmdDebug click handlers, jQuery-UI widgets, etc.
// unconditionally, and doing that twice on the same live DOM would
// double-bind every one of them.
async function restartGame(saveBytes) {
    document.getElementById('qv-saves')?.close();
    resetGameOutput();

    const ok = saveBytes
        ? await Bridge.InitialiseWithSave(originalGameBytes, originalGameFilename, saveBytes)
        : await Bridge.Initialise(originalGameBytes, originalGameFilename);
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
    // gameIdOverride lets callers with a stronger identity than the filename
    // (e.g. the Text Adventures API's stable game id) use it instead.
    const gameId = gameIdOverride || computeGameId(filename);
    WebPlayer.gameId = gameId;

    let saveBytes = null;
    // Editor-preview sessions (bc) are excluded: a preview's filename is
    // transient/reused per edit, so prompting every reload would be noisy
    // and would fragment saves under a churny id.
    if (!bc) {
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
    clearUnsavedProgress();
}

// A .quest-save only carries state, not resources — it must be paired with
// the currently-loaded game's original bytes to load safely (see plan).
// Identity check: prefer the game's own embedded IFID (<gameid> in the save
// XML / Bridge.GetGameIfid() for the running game) over the loading
// filename — filename is often generic/shared (every textadventures.co.uk
// game's original attribute is literally "game.aslx" no matter which game
// it is), so it can tell neither "same game, different session" nor
// "different game entirely" apart reliably. IFID is stable regardless of
// how/where the game was loaded from, so it also makes a save downloaded
// from WebPlayer for a textadventures.co.uk game load correctly here. Only
// falls back to the filename if either side lacks an ifid (rare for modern
// games). Legacy .asl/.cas games skip the check altogether: V4Game.GameID is
// hardcoded null (no ifid concept) *and* their save data is raw restore-data
// text, not XML, so it carries no "original"/gameid to fall back to either —
// there's simply nothing reliable to cross-check. The engine's own
// save-format validation (V4Game's "QUEST300"/"QUEST200.1" header check) is
// the safety net there instead, same tradeoff WebPlayer's equivalent check
// already makes.
async function loadSaveFromFile(file) {
    const bytes = new Uint8Array(await file.arrayBuffer());

    if (/\.(asl|cas)$/i.test(originalGameFilename)) {
        hideSavesError();
        await restartGame(bytes);
        return;
    }

    let originalAttr = null;
    let uploadedIfid = null;
    try {
        const text = new TextDecoder('utf-8').decode(bytes);
        const xml = new DOMParser().parseFromString(text, 'application/xml');
        originalAttr = xml.documentElement?.getAttribute('original') ?? null;
        uploadedIfid = xml.querySelector('game > gameid')?.textContent?.trim() || null;
    } catch { /* falls through to the mismatch error below */ }

    const currentIfid = Bridge.GetGameIfid();
    const currentIdentity = currentIfid || computeGameId(originalGameFilename);
    const uploadedIdentity = uploadedIfid || (originalAttr && computeGameId(originalAttr));

    if (!uploadedIdentity || currentIdentity.toLowerCase() !== uploadedIdentity.toLowerCase()) {
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
        if (bootChoiceResolve) {
            const resolve = bootChoiceResolve;
            bootChoiceResolve = null;
            dlg.close();
            resolve({ type: 'new' });
            return;
        }
        // Manage mode: restarting mid-session, matches WebPlayer's Slots
        // "Start from the beginning" button (no confirm() there either).
        restartGame(null);
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

// Nothing here can ever work over file:// — the runtime fetches its own
// resources (playercore.htm, the dotnet WASM runtime) via relative URLs,
// and browsers refuse those fetches from a file: origin (each file: URL is
// treated as a unique, opaque origin with no ability to fetch even its own
// sibling files). Caught up front, before any load is attempted, so the
// user gets an accurate explanation instead of the generic "not a valid
// game file" message that fetch failures would otherwise produce.
function showFileProtocolError() {
    document.documentElement.classList.remove('qv-booting');
    const pickers = document.getElementById('qv-pickers');
    const msg = document.getElementById('qv-loading-msg');
    const errorEl = document.getElementById('qv-error');
    if (msg) msg.style.display = 'none';
    if (pickers) pickers.style.display = 'none';
    if (!errorEl) return;
    errorEl.innerHTML = '<strong>This can\'t run from a local file.</strong> '
        + 'You\'ve opened index.html directly from disk (a file:// address). Browsers block the web features this '
        + 'player needs &mdash; fetching its own files and running WebAssembly &mdash; when a page is loaded that way, '
        + 'so no game will load no matter which one you pick. '
        + 'Serve this folder over http(s) instead: for example, run <code>npx serve</code> in this folder and open '
        + 'the http://localhost address it prints, or upload the files to any web host.';
    errorEl.style.display = 'block';
}

function showLoading() {
    const pickers = document.getElementById('qv-pickers');
    const msg = document.getElementById('qv-loading-msg');
    if (pickers) pickers.style.display = 'none';
    if (msg) msg.style.display = 'flex';
}

// Maps an HTTP status code to a short, specific reason. Only used when the
// fetch actually reached a server and got a response — a blocked/failed
// fetch (CORS, DNS, offline) never gets this far and has no status at all.
function _describeHttpStatus(status) {
    if (status === 404) return 'the file wasn\'t found there (404 Not Found)';
    if (status === 401 || status === 403) return `access to it was denied (${status})`;
    if (status >= 500) return `the server hosting it reported an error (HTTP ${status})`;
    if (status >= 400) return `the request was rejected (HTTP ${status})`;
    return `the server responded with HTTP ${status}`;
}

function showError(downloadUrl, isLocalFile, error) {
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
    } else if (Number.isInteger(error?.status)) {
        // We got a real HTTP response, so we know exactly why it failed —
        // no need to guess at CORS. downloadUrl may still be unknown here
        // (e.g. the TextAdventures API lookup itself 404'd before we ever
        // learned the game's source URL), so the link is optional.
        html = '<strong>Couldn\'t load the game.</strong> '
            + `When we asked for it, ${_describeHttpStatus(error.status)}. `
            + (downloadUrl
                ? '<a class="anchor" href="' + _esc(downloadUrl) + '" target="_blank" rel="noopener">'
                    + 'Open the link</a> to double-check the URL, or load a saved copy with the file picker below.'
                : 'Double-check the address, or load a saved copy of the game file with the file picker below.');
    } else if (downloadUrl) {
        // fetch() itself failed before any response came back. The browser
        // gives no way to tell apart a CORS block, DNS failure, or being
        // offline in this case, so we can only name the likely suspects.
        html = '<strong>Couldn\'t load the game from that URL.</strong> '
            + 'This is usually because the site hosting it isn\'t configured to allow other sites to fetch the file directly '
            + '(a CORS restriction), but it could also be a network problem or a bad address '
            + '&mdash; this is a limitation of the hosting site or your connection, not something wrong with your download. '
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
        } catch (err) {
            showError(url, false, err);
        }
    }
    urlBtn.addEventListener('click', doLoadUrl);
    urlInput.addEventListener('keydown', e => { if (e.key === 'Enter') doLoadUrl(); });
}

async function fetchGameBytes(url) {
    const response = await fetch(url);
    if (!response.ok) {
        const err = new Error(`HTTP ${response.status}`);
        err.status = response.status;
        throw err;
    }
    const bytes = new Uint8Array(await response.arrayBuffer());
    let filename = (response.url || url).split('/').pop()?.split('?')[0] || 'game.aslx';
    // dev-server.mjs's CORS-avoidance proxy rewrites the Text Adventures API's
    // sourceGameUrl to /game-resource/<url-encoded-real-url>, so the "last
    // segment" above is that encoded string, not a real filename, in local
    // dev only. Detect and decode so the derived filename (used both for
    // GameLauncher's extension-based dispatch and the save-file "original"
    // cross-check) matches what production — no proxy — actually produces.
    try {
        const decoded = decodeURIComponent(filename);
        if (decoded !== filename && decoded.includes('/')) {
            filename = decoded.split('/').pop()?.split('?')[0] || filename;
        }
    } catch { /* not a valid encoded URL, keep filename as-is */ }
    return { bytes, filename };
}

// ── Boot ─────────────────────────────────────────────────────────────────────

(function () {
    if (window.location.protocol === 'file:') {
        document.addEventListener('DOMContentLoaded', showFileProtocolError);
        return;
    }

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
    // A URL-supplied game always wins over the configured default, so authors
    // testing/sharing a specific ?url= link aren't stuck on their own game.
    const gameUrl = params.get('url') || window.QuestVivaConfig?.defaultGameUrl;

    if (id) {
        // Start API fetch immediately; wire DOM once it's ready.
        let resolvedSourceUrl = null;
        const gamePromise = (async () => {
            const apiRoot = window.QuestVivaConfig?.textAdventuresApiRoot
                ?? 'https://textadventures.co.uk/api/';
            const apiResponse = await fetch(`${apiRoot}game/${id}`);
            if (!apiResponse.ok) {
                const err = new Error(`API: HTTP ${apiResponse.status}`);
                err.status = apiResponse.status;
                throw err;
            }
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
            } catch (err) {
                showError(resolvedSourceUrl, false, err);
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
            } catch (err) {
                showError(gameUrl, false, err);
            }
        });
        return;
    }

    // No game specified — show start screen and wire up controls.
    document.addEventListener('DOMContentLoaded', wireStartScreen);
})();
