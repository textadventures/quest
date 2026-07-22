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
// The pristine player chrome markup (playercore.htm), fetched once at boot
// and reused verbatim by restartGame() so a mid-session restart gets exactly
// the same clean slate as the initial boot, instead of the previous game's
// live DOM (see swapInPlayerUi()).
let originalPlayerHtml = null;

function computeGameId(filename) {
    return filename.split('/').pop();
}

// Nothing guarantees a 'resource-response' ever arrives: a raw picked File in
// the browser build's Play-tab flow has no responder for anything beyond the
// game file itself (see the source=local boot branch's comment), and even a
// real responder (the editor tab, Electron's ElectronFileAdapter) can vanish
// mid-request if that tab/window closes. Either way, without this timeout the
// promise sits pending forever, hanging whatever engine code awaited it — so
// fall back to the bare filename (same as the no-editorChannel case above),
// letting the browser attempt — and if unresolvable, cleanly fail — a plain
// relative fetch instead of hanging.
const RESOURCE_REQUEST_TIMEOUT_MS = 5000;

function getResourceUrl(name) {
    if (resourceRoot) return Promise.resolve(resourceRoot + name);
    if (!editorChannel) return Promise.resolve(name);
    return new Promise(resolve => {
        const id = crypto.randomUUID();
        const timeoutId = window.setTimeout(() => {
            pendingResources.delete(id);
            // The subsequent fetch of the bare filename will very likely also
            // fail (and the browser will log that on its own) — but that
            // failure alone won't explain itself, so name the actual cause
            // here: nobody answered the resource-request handoff in time.
            console.error(`[Quest] Timed out waiting for resource "${name}" — the tab/window that should have supplied it never answered. Falling back to a plain relative fetch, which will likely fail unless the game is self-contained.`);
            resolve(name);
        }, RESOURCE_REQUEST_TIMEOUT_MS);
        pendingResources.set(id, dataUrl => {
            window.clearTimeout(timeoutId);
            resolve(dataUrl);
        });
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

// Chrome (and others) block unmuted audio until the page has "user
// activation". A click through to this page (e.g. a target=_blank Play
// button) does transfer that activation across the navigation, but it
// doesn't reliably survive WasmPlayer's boot sequence (fetching the game,
// booting the AOT WASM runtime) for a slow-enough load — by the time the
// game's start logic tries to play a sound, the activation can already be
// gone, and the browser silently rejects it. Only relevant for games that
// might play a sound before the player has had any chance to interact with
// the page themselves (typing a command is itself a fresh activation).
function activationLikelyGranted() {
    // Electron's own default autoplay policy is already unrestricted (unlike
    // stock Chrome's), and ipc/player.ts's player windows set it explicitly —
    // true regardless of how the window was created (in-place nav, popup, or
    // a main-process-created window), so the gate below would only ever add
    // friction there, never actually prevent a blocked sound.
    if (navigator.userAgent.includes('Electron/')) return true;
    return !('userActivation' in navigator) || navigator.userActivation.hasBeenActive;
}

// Attached to the api/Game/{id} fetch below as analytics metadata (see
// ClientInfo on textadventures.co.uk's ApiController) — mirrors AppShell's
// home-catalog.ts clientInfoParams(), which does the same for the Catalog and
// GameDetails calls. This player window runs untrusted game content and
// deliberately has no preload bridge (see ipc/player.ts), so unlike
// home-catalog.ts it can't read window.electronApp.platform; Electron and its
// host OS are instead sniffed from the UA, the same signal
// activationLikelyGranted() above already relies on.
function clientInfoParams() {
    const params = new URLSearchParams();
    const isElectron = navigator.userAgent.includes('Electron/');
    params.set('source', isElectron ? 'electron' : 'web');
    if (window.QuestVivaVersion) params.set('version', window.QuestVivaVersion);
    if (isElectron) {
        const ua = navigator.userAgent;
        const platform = ua.includes('Macintosh') ? 'Mac'
            : ua.includes('Windows') ? 'Windows'
            : (ua.includes('Linux') || ua.includes('X11')) ? 'Linux'
            : null;
        if (platform) params.set('platform', platform);
    }
    return params;
}

// Shows a "click to begin" prompt in the still-visible start screen and
// waits for it, so the resulting click is a fresh, same-page activation —
// but only when it's actually needed (see activationLikelyGranted) and only
// for games that could plausibly play a sound before the player interacts
// (see Bridge.GameLikelyPlaysSound). Call before Bridge.Begin() and before
// the start screen overlay is removed.
async function maybeGateOnActivation(gameBytes) {
    if (activationLikelyGranted()) return;

    let usesSound = true;
    try { usesSound = Bridge.GameLikelyPlaysSound(gameBytes); } catch { /* don't block boot on detection failure */ }
    if (!usesSound) return;

    const pickers = document.getElementById('qv-pickers');
    const msg = document.getElementById('qv-loading-msg');
    const gate = document.getElementById('qv-clicktobegin');
    if (pickers) pickers.style.display = 'none';
    if (msg) msg.style.display = 'none';
    if (gate) gate.style.display = 'flex';

    await new Promise(resolve => {
        document.getElementById('qv-clicktobegin-btn')?.addEventListener('click', resolve, { once: true });
    });

    if (gate) gate.style.display = 'none';
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
        await Bridge.PrepareSaveGame(html);
        return Bridge.GetSaveGameBytes();
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

// Replaces document.body with a fresh copy of the player chrome, preserving
// #qv-start/#qv-saves/#questVivaDebugger (the static markup's overlays, which
// live outside the swapped-in playerHtml and would otherwise be destroyed by
// the innerHTML assignment). Used both for the initial boot and for a
// mid-session restart ("Start from the beginning" / Load) — a restart must
// get a genuinely new DOM, not just a cleared output pane, because games
// commonly use their external <javascript> resources to inject their own
// elements elsewhere in the chrome (e.g. a custom sidebar pane). Those run
// again on every restart (RegisterExternalScripts fires on every Initialise
// call), so anything short of a full DOM reset here leaves the previous run's
// injected elements in place for the new run to duplicate.
function swapInPlayerUi() {
    const startScreenEl = document.getElementById('qv-start');
    const savesDialogEl = document.getElementById('qv-saves');
    const debuggerDialogEl = document.getElementById('questVivaDebugger');
    startScreenEl?.remove();
    savesDialogEl?.remove();
    debuggerDialogEl?.remove();

    document.body.innerHTML = originalPlayerHtml;

    if (startScreenEl) document.body.appendChild(startScreenEl);
    if (savesDialogEl) document.body.appendChild(savesDialogEl);
    if (debuggerDialogEl) document.body.appendChild(debuggerDialogEl);
    return startScreenEl;
}

// Remembered across a mid-session restart (restartGame doesn't get isPreview
// as a parameter — it just re-initialises on top of the same boot session),
// so the Debug button's visibility stays correct after "Start from the
// beginning"/Load in an editor-preview session.
let currentIsPreview = false;

async function initWasmPlayer(gameBytes, filename, bc = null, saveBytes = null, isPreview = false) {
    currentIsPreview = isPreview;
    const [htmResponse, { dotnet }] = await Promise.all([
        fetch('playercore.htm'),
        import('./_framework/dotnet.js'),
    ]);
    originalPlayerHtml = await htmResponse.text();

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
    // behaviour is unchanged.
    const startScreenEl = swapInPlayerUi();

    const ok = saveBytes
        ? await Bridge.InitialiseWithSave(gameBytes, filename, saveBytes)
        : await Bridge.Initialise(gameBytes, filename);
    if (!ok) {
        console.error('[Quest] Failed to initialise game');
        throw new Error('Failed to initialise game');
    }

    await maybeGateOnActivation(gameBytes);

    // Now that startup has actually succeeded, remove the start screen overlay
    // to reveal the game UI underneath.
    startScreenEl?.remove();

    await setupPaperJs();

    WebPlayer.onSaveClick = () => openSavesDialog('manage');
    WebPlayer.initUI();
    wireDebuggerButton();
    // Editor-preview sessions (isPreview) don't offer saving at all — by
    // design: a save captures the whole game state, so reloading one while
    // iterating would show stale state that doesn't reflect the developer's
    // latest edits, and could be mistaken for saving the edits themselves.
    // Note this is distinct from bc: a source=local play session also uses a
    // BroadcastChannel (to hand off the picked file's bytes and answer
    // resource requests) but is a real play session and should offer saving.
    WebPlayer.setCanSave(!isPreview);
    // Only ever shown in editor-preview sessions (see the module doc comment
    // above the Debugger section) — never for a normal play session, even
    // though the loaded game itself might have DebugEnabled true.
    WebPlayer.setCanDebug(isPreview && Bridge.IsDebugEnabled());

    await Bridge.Begin();
}

// Reloads a save (or, with saveBytes null, the fresh original game) on top
// of the already-booted WASM runtime — used for in-game Load and "Start
// from the beginning". Rebuilds the player chrome from scratch via
// swapInPlayerUi() (same as the initial boot) rather than just clearing the
// output pane: RegisterExternalScripts fires again on every Initialise call,
// and games commonly use their external <javascript> resources to inject
// their own elements into the chrome (e.g. a custom sidebar pane) — without a
// full DOM reset here, that injected element from the previous run is still
// sitting in the (undisturbed) sidebar when the script re-runs and adds
// another one. A fresh DOM means initUI()/setupPaperJs() are being bound to
// brand-new elements each time, not the same live ones, so this isn't the
// double-binding repeat-init the old comment here used to warn about.
async function restartGame(saveBytes) {
    document.getElementById('qv-saves')?.close();
    swapInPlayerUi();
    resetGameOutput();

    const ok = saveBytes
        ? await Bridge.InitialiseWithSave(originalGameBytes, originalGameFilename, saveBytes)
        : await Bridge.Initialise(originalGameBytes, originalGameFilename);
    if (!ok) {
        showSavesError('Failed to load that save.');
        return;
    }

    await maybeGateOnActivation(originalGameBytes);

    await setupPaperJs();
    WebPlayer.initUI();
    wireDebuggerButton();
    WebPlayer.setCanSave(true);
    WebPlayer.setCanDebug(currentIsPreview && Bridge.IsDebugEnabled());
    await Bridge.Begin();
}

// Wraps initWasmPlayer with the boot-time "Continue / New Game" prompt: if
// saves already exist for this game, ask before committing to either the
// fresh game or a chosen save. Used by all boot entry points below.
async function startGame(bytes, filename, bc = null, gameIdOverride = null, isPreview = false) {
    originalGameBytes = bytes;
    originalGameFilename = filename;
    // gameIdOverride lets callers with a stronger identity than the filename
    // (e.g. the Text Adventures API's stable game id) use it instead.
    const gameId = gameIdOverride || computeGameId(filename);
    WebPlayer.gameId = gameId;

    let saveBytes = null;
    // Editor-preview sessions (isPreview) are excluded, same rationale as
    // setCanSave's call site below: saving/loading is intentionally not part
    // of the preview experience, so there's nothing to prompt about here
    // either. A source=local play session goes through this same code path
    // as a real play session and should get the boot prompt.
    if (!isPreview) {
        let saves = [];
        try { saves = await GameSaver.listSaves(gameId); } catch { saves = []; }
        if (saves.length > 0) {
            const choice = await openSavesDialog('boot', gameId);
            if (choice.type === 'load') {
                saveBytes = await GameSaver.load(choice.slotIndex, gameId);
            }
        }
    }

    await initWasmPlayer(bytes, filename, bc, saveBytes, isPreview);
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

// Waits for the browser to actually paint the current frame. WasmPlayer's
// WASM runtime is single-threaded and the engine's SaveAsync does its work
// synchronously (no genuine await inside it) — so a DOM update made right
// before calling into it sits queued but unpainted for the entire blocking
// call, since the one JS/UI thread has no chance to render until that call
// returns. Awaiting two animation frames forces a real yield back to the
// browser first, so a "Saving…" label actually reaches the screen before the
// blocking work starts, instead of only flashing briefly at the very end.
function nextPaint() {
    return new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
}

// Disables a button and (if given) swaps its label for the duration of an
// async op, so slower saves (a big transcript, IndexedDB contention, low-end
// devices) read as "working" rather than an unresponsive click — there was
// previously no feedback at all between clicking Save and the list
// refreshing. busyText is omitted for icon-only buttons (e.g. delete), whose
// content is an <svg>, not text — overwriting textContent would destroy it.
async function withBusyButton(button, busyText, fn) {
    const originalText = button.textContent;
    const wasDisabled = button.disabled;
    button.disabled = true;
    if (busyText) button.textContent = busyText;
    await nextPaint();
    try {
        return await fn();
    } finally {
        button.disabled = wasDisabled;
        if (busyText) button.textContent = originalText;
    }
}

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
            await withBusyButton(delBtn, '', async () => {
                await GameSaver.deleteSlot(Number(delBtn.dataset.deleteSlot), WebPlayer.gameId);
            });
            renderSavesList(await GameSaver.listSaves(WebPlayer.gameId), 'manage');
        }
    });

    document.getElementById('qv-saves-save-new').addEventListener('click', async (e) => {
        const input = document.getElementById('qv-saves-name-input');
        await withBusyButton(e.currentTarget, 'Saving…', async () => {
            await GameSaver.save(input.value.trim() || undefined);
        });
        input.value = '';
        renderSavesList(await GameSaver.listSaves(WebPlayer.gameId), 'manage');
    });

    document.getElementById('qv-saves-download').addEventListener('click', (e) =>
        withBusyButton(e.currentTarget, 'Saving…', downloadSaveToFile));

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

// ── Debugger (#questVivaDebugger) ─────────────────────────────────────────────
// Only ever opened in editor-preview sessions (see WebPlayer.setCanDebug's
// call site in initWasmPlayer/restartGame) — element browser, per-tab
// attribute inspector with a "hack your own game" override, and a walkthrough
// runner. Mirrors WebPlayer's Debugger/Attributes/Walkthrough.razor, which
// drive IGameDebug directly as Blazor components; here the same data comes
// over the bridge as JSON (Bridge.Get*Json — see WasmPlayerBridge.cs) for
// plain-DOM rendering instead.

let debuggerWired = false;
let debuggerActiveTab = 'Walkthrough';
let debuggerSelectedItem = null;
// Name of the walkthrough currently running, or null — the source of truth
// for the Run/Cancel/status UI, *not* the DOM elements themselves. Closing
// and reopening the dialog (or switching tabs/objects) rebuilds
// #qv-debugger-detail from scratch, which used to silently lose whatever
// Run/Cancel state only lived in the old (now-replaced) DOM nodes — a
// walkthrough kept running with no way to see its status or cancel it once
// the dialog was reopened. Every render of the Walkthrough detail panel
// checks this instead, so it comes back correctly regardless of how many
// times the panel gets rebuilt while a walkthrough is running.
let runningWalkthroughName = null;
// Which attribute row (if any) has its override panel open — a single
// fixed panel below the table, not one input per row (an earlier version
// tried cramming input+button+hint into a 4th table column and it didn't
// fit at any reasonable dialog width).
let debuggerSelectedAttr = null;
// Sort/filter state for the attribute table — reset whenever a different
// object/tab is selected (renderDebuggerList's click handler / selectDebuggerTab),
// but preserved across same-object re-renders (Apply, row selection).
let debuggerAttrSort = { column: 'attribute', direction: 'asc' };
let debuggerAttrSearch = '';
// The current object's raw {attrName: DebugDataItem} data, cached so
// re-sorting/re-filtering (every keystroke in the search box, every header
// click) doesn't need a fresh Bridge round-trip each time — only rebuilt
// when the object actually changes or an override is applied.
let debuggerAttrData = null;

// ── Move/resize/splitter/column-resize state ────────────────────────────────
// All in px, all session-only (in-memory, not persisted across a page
// reload) — a dev tool's window geometry isn't worth the complexity of
// persisting across reloads, but it should survive closing/reopening the
// dialog and game restarts within the same session, so these live at module
// scope rather than being recomputed/reset on every render.

const DEBUGGER_DEFAULT_WIDTH = 768;
const DEBUGGER_DEFAULT_HEIGHT = 600;
const DEBUGGER_MIN_WIDTH = 480;
const DEBUGGER_MIN_HEIGHT = 360;
const DEBUGGER_MIN_LIST_WIDTH = 100;
const DEBUGGER_MIN_DETAIL_WIDTH = 200;
const DEBUGGER_MIN_COLUMN_WIDTH = 60;

// null until the first time the dialog is ever opened — see applyDebuggerRect.
let debuggerRect = null;
let debuggerListWidth = 192; // matches the old w-48 (12rem) Tailwind default
let debuggerColWidths = { attribute: 160, value: 260 }; // 'source' just takes whatever's left

// Generic pointer-drag helper — used by the title bar, resize handle,
// splitter, and column-resize handles below. onMove receives the raw
// pointermove event; the caller computes whatever delta it needs from
// clientX/clientY captured at drag start. document.body.style.cursor is a
// plain inline style (works regardless of the .qv-chrome CSS scoping); the
// user-select:none class goes on the dialog itself, not <body> — see
// :scope.qv-debugger-drag-active's doc comment in chrome.css for why.
function startDebuggerDrag(startEvent, cursor, onMove, onEnd) {
    startEvent.preventDefault();
    const dlg = document.getElementById('questVivaDebugger');
    dlg.classList.add('qv-debugger-drag-active');
    document.body.style.cursor = cursor;

    const move = (e) => onMove(e);
    const end = (e) => {
        document.removeEventListener('pointermove', move);
        document.removeEventListener('pointerup', end);
        dlg.classList.remove('qv-debugger-drag-active');
        document.body.style.cursor = '';
        if (onEnd) onEnd(e);
    };

    document.addEventListener('pointermove', move);
    document.addEventListener('pointerup', end);
}

function clamp(value, min, max) {
    return Math.min(Math.max(value, min), Math.max(min, max));
}

// Applied right after every showModal() (see wireDebuggerButton/restartGame's
// call sites), before the browser gets a chance to paint — so the dialog
// never visibly flashes at its old browser-default centered position first.
// Computes a centered default the very first time it's called; every
// subsequent call (reopening the dialog, or after the user drags/resizes)
// just re-applies whatever debuggerRect currently holds.
function applyDebuggerRect() {
    const dlg = document.getElementById('questVivaDebugger');
    if (!dlg) return;

    if (!debuggerRect) {
        const width = Math.min(DEBUGGER_DEFAULT_WIDTH, window.innerWidth - 40);
        const height = Math.min(DEBUGGER_DEFAULT_HEIGHT, window.innerHeight - 40);
        debuggerRect = {
            left: Math.round((window.innerWidth - width) / 2),
            top: Math.round((window.innerHeight - height) / 2),
            width,
            height,
        };
    }

    dlg.style.left = `${debuggerRect.left}px`;
    dlg.style.top = `${debuggerRect.top}px`;
    dlg.style.width = `${debuggerRect.width}px`;
    dlg.style.height = `${debuggerRect.height}px`;
}

function wireDebuggerMoveResize() {
    const dlg = document.getElementById('questVivaDebugger');
    const titlebar = document.getElementById('qv-debugger-titlebar');
    const resizeHandle = document.getElementById('qv-debugger-resize-handle');

    titlebar.addEventListener('pointerdown', (e) => {
        if (e.target.closest('#qv-debugger-close')) return;
        const startX = e.clientX;
        const startY = e.clientY;
        const { left: startLeft, top: startTop, width, height } = debuggerRect;

        startDebuggerDrag(e, 'move', (moveEvent) => {
            const left = clamp(startLeft + (moveEvent.clientX - startX), 0, window.innerWidth - width);
            const top = clamp(startTop + (moveEvent.clientY - startY), 0, window.innerHeight - height);
            debuggerRect = { ...debuggerRect, left, top };
            dlg.style.left = `${left}px`;
            dlg.style.top = `${top}px`;
        });
    });

    resizeHandle.addEventListener('pointerdown', (e) => {
        const startX = e.clientX;
        const startY = e.clientY;
        const { left, top, width: startWidth, height: startHeight } = debuggerRect;

        startDebuggerDrag(e, 'nwse-resize', (moveEvent) => {
            const width = clamp(startWidth + (moveEvent.clientX - startX), DEBUGGER_MIN_WIDTH, window.innerWidth - left);
            const height = clamp(startHeight + (moveEvent.clientY - startY), DEBUGGER_MIN_HEIGHT, window.innerHeight - top);
            debuggerRect = { ...debuggerRect, width, height };
            dlg.style.width = `${width}px`;
            dlg.style.height = `${height}px`;
        });
    });
}

function wireDebuggerSplitter() {
    const splitter = document.getElementById('qv-debugger-splitter');
    const list = document.getElementById('qv-debugger-list');
    const panes = document.getElementById('qv-debugger-panes');

    splitter.addEventListener('pointerdown', (e) => {
        const startX = e.clientX;
        const startWidth = list.getBoundingClientRect().width;
        splitter.classList.add('qv-debugger-dragging');

        startDebuggerDrag(e, 'col-resize', (moveEvent) => {
            const containerWidth = panes.getBoundingClientRect().width;
            const maxWidth = containerWidth - DEBUGGER_MIN_DETAIL_WIDTH - splitter.getBoundingClientRect().width;
            debuggerListWidth = clamp(startWidth + (moveEvent.clientX - startX), DEBUGGER_MIN_LIST_WIDTH, maxWidth);
            list.style.width = `${debuggerListWidth}px`;
        }, () => splitter.classList.remove('qv-debugger-dragging'));
    });
}

// Attribute-table column resizing — only the first two columns (Attribute,
// Value) get a drag handle; Source just absorbs whatever width is left over
// (see the <colgroup> built in renderDebuggerAttributesDetail), matching how
// #qv-debugger-splitter only needs to move one boundary for a two-pane
// layout. Wired once in ensureDebuggerWired via delegation, since the
// handles themselves are rebuilt on every renderDebuggerAttributesDetail
// call (a fresh object/tab, or after Apply).
function wireDebuggerColumnResize(startEvent) {
    const handle = startEvent.target.closest('[data-resize-col]');
    if (!handle) return false;

    const key = handle.dataset.resizeCol;
    const table = handle.closest('table');
    const col = table.querySelector(`col[data-col="${key}"]`);
    if (!col) return true;

    const startX = startEvent.clientX;
    const startWidth = debuggerColWidths[key];
    handle.classList.add('qv-debugger-dragging');

    startDebuggerDrag(startEvent, 'col-resize', (moveEvent) => {
        const width = Math.max(DEBUGGER_MIN_COLUMN_WIDTH, startWidth + (moveEvent.clientX - startX));
        debuggerColWidths = { ...debuggerColWidths, [key]: width };
        col.style.width = `${width}px`;
    }, () => handle.classList.remove('qv-debugger-dragging'));

    return true;
}

// DebugDataItem.Value is a human-readable *display* string (Fields.cs's
// DefaultFormatter just calls ToString() — a string has no quotes, a bool
// reads "True"/"False", an object reference reads as "Object: kitchen").
// EditValue (same object, a sibling field) is the same underlying value
// pre-formatted as valid script syntax instead — quoted string, lowercase
// true/false, bare object name — ready to feed straight into Apply. Only
// shown at all when item.CanOverride is true (see Fields.cs's
// CanOverrideValue) — lists/dicts/scripts have no simple literal syntax to
// write back, so those get a short note instead of a dead-end textbox (see
// renderDebuggerOverridePanel).
const DEBUGGER_OVERRIDE_HINT = 'Edit and Apply to change this value for the rest of the session';

function renderDebuggerTabs(types) {
    const tabs = document.getElementById('qv-debugger-tabs');
    const allTabs = ['Walkthrough', ...types];
    tabs.innerHTML = allTabs.map(tab =>
        `<button type="button" class="btn btn-sm ${tab === debuggerActiveTab ? 'preset-filled-primary-500' : 'preset-tonal-surface'}" data-tab="${_esc(tab)}">${_esc(tab)}</button>`
    ).join('');
}

function renderDebuggerList() {
    const list = document.getElementById('qv-debugger-list');
    const items = debuggerActiveTab === 'Walkthrough'
        ? JSON.parse(Bridge.GetWalkthroughNamesJson())
        : JSON.parse(Bridge.GetDebugObjectsJson(debuggerActiveTab));

    if (!items.length) {
        list.innerHTML = '<li class="text-sm text-surface-500 px-2 py-1">Nothing here.</li>';
        return;
    }

    // A plain selectable list, not hyperlinks — .qv-debugger-list-item is a
    // block row with its own hover/selected background (shares
    // .qv-debugger-row-selected with the attribute table below).
    list.innerHTML = items.map(item => {
        const selected = item === debuggerSelectedItem;
        return `<li><button type="button" class="qv-debugger-list-item${selected ? ' qv-debugger-row-selected' : ''}" data-item="${_esc(item)}" aria-selected="${selected}">${_esc(item)}</button></li>`;
    }).join('');
}

function renderDebuggerWalkthroughDetail(name) {
    const detail = document.getElementById('qv-debugger-detail');
    const steps = JSON.parse(Bridge.GetWalkthroughStepsJson(name));
    const running = name === runningWalkthroughName;
    detail.innerHTML = '<div class="qv-debugger-scroll">'
        + '<ol class="list-decimal list-inside space-y-1">'
        + steps.map(step => `<li>${_esc(step)}</li>`).join('')
        + '</ol>'
        + '</div>'
        + '<div class="qv-debugger-footer flex gap-2 items-center">'
        + `<button type="button" class="btn preset-filled-primary-500" data-run-walkthrough="${_esc(name)}"${running ? ' disabled' : ''}>Run</button>`
        + `<button type="button" class="btn preset-tonal-surface${running ? '' : ' hidden'}" data-cancel-walkthrough>Cancel</button>`
        + `<span class="text-sm" data-walkthrough-status>${running ? 'Running…' : ''}</span>`
        + '</div>';
}

const DEBUGGER_ATTR_COLUMNS = [
    { key: 'attribute', label: 'Attribute' },
    { key: 'value', label: 'Value' },
    { key: 'source', label: 'Source' },
];

function debuggerAttrSortIndicator(column) {
    if (debuggerAttrSort.column !== column) return '';
    return debuggerAttrSort.direction === 'asc' ? ' ▲' : ' ▼';
}

// Applies debuggerAttrSearch (matches against the attribute name or its
// display value) and debuggerAttrSort to debuggerAttrData's keys.
function sortedFilteredDebuggerAttrs() {
    const query = debuggerAttrSearch.trim().toLowerCase();
    let attrs = Object.keys(debuggerAttrData);
    if (query) {
        attrs = attrs.filter(attr =>
            attr.toLowerCase().includes(query) || debuggerAttrData[attr].Value.toLowerCase().includes(query));
    }

    const { column, direction } = debuggerAttrSort;
    const sortKey = attr => column === 'attribute' ? attr
        : column === 'value' ? debuggerAttrData[attr].Value
            : (debuggerAttrData[attr].Source ?? '');
    attrs.sort((a, b) => sortKey(a).localeCompare(sortKey(b), undefined, { sensitivity: 'base' }));
    if (direction === 'desc') attrs.reverse();
    return attrs;
}

// Rebuilds just the <tbody> (and the header sort indicators) — deliberately
// not the whole panel, so re-sorting/typing in the search box doesn't touch
// .qv-debugger-scroll (would reset its scroll position) or the search
// <input> itself (would drop keyboard focus mid-keystroke).
function renderDebuggerAttrRows() {
    const tbody = document.getElementById('qv-debugger-attr-tbody');
    if (!tbody) return;

    // Only the label <span> gets its text replaced — a resize handle
    // (.qv-debugger-col-resize) is a sibling inside the same <th>, and
    // setting th.textContent directly would wipe that handle out on every
    // sort/search re-render.
    document.querySelectorAll('#qv-debugger-detail [data-sort-col]').forEach(th => {
        const col = th.dataset.sortCol;
        const label = th.querySelector('.qv-debugger-sort-label');
        if (label) label.textContent = DEBUGGER_ATTR_COLUMNS.find(c => c.key === col).label + debuggerAttrSortIndicator(col);
    });

    const attrs = sortedFilteredDebuggerAttrs();
    if (!attrs.length) {
        tbody.innerHTML = '<tr><td colspan="3" class="text-surface-500">No matching attributes.</td></tr>';
        return;
    }

    tbody.innerHTML = attrs.map(attr => {
        const item = debuggerAttrData[attr];
        const classes = [item.IsInherited ? 'qv-debugger-row-inherited' : '', attr === debuggerSelectedAttr ? 'qv-debugger-row-selected' : ''];
        return `<tr class="${classes.join(' ')}" data-attr-row="${_esc(attr)}">`
            + `<td>${_esc(attr)}</td>`
            + `<td>${_esc(item.Value)}</td>`
            + `<td>${_esc(item.Source ?? '')}</td>`
            + '</tr>';
    }).join('');
}

// The table (which can run to a couple dozen rows for an inherited-heavy
// object like the default player) scrolls in its own inner region
// (.qv-debugger-scroll); the override panel is a separate, non-scrolling
// footer so selecting a row near the top of a long table doesn't require
// scrolling past everything else to reach the Apply button. Sorting/search
// only ever rebuild the <tbody> (see renderDebuggerAttrRows) — this function
// is the one that talks to the bridge and rebuilds the whole panel shell
// (search box, sortable headers, override panel), for a genuine object
// switch or after Apply changes the underlying data.
function renderDebuggerAttributesDetail(tab, obj) {
    const detail = document.getElementById('qv-debugger-detail');
    debuggerAttrData = JSON.parse(Bridge.GetDebugDataJson(tab, obj)).Data || {};
    const attrs = Object.keys(debuggerAttrData);

    if (!attrs.length) {
        detail.innerHTML = '<p class="text-surface-500">No attributes.</p>';
        return;
    }

    if (!attrs.includes(debuggerSelectedAttr)) debuggerSelectedAttr = null;

    // Only Attribute/Value get an explicit <col> width + resize handle —
    // Source (the last column) just takes whatever width table-layout:fixed
    // leaves over, the same "only one boundary to drag" simplification
    // wireDebuggerSplitter uses for the two-pane list/detail split.
    detail.innerHTML = `<input type="text" class="qv-debugger-input mb-2" id="qv-debugger-attr-search" placeholder="Search attributes…" autocomplete="off" value="${_esc(debuggerAttrSearch)}">`
        + '<div class="qv-debugger-scroll">'
        + '<table class="table qv-debugger-table">'
        + `<colgroup><col data-col="attribute" style="width:${debuggerColWidths.attribute}px">`
        + `<col data-col="value" style="width:${debuggerColWidths.value}px"><col data-col="source"></colgroup>`
        + '<thead><tr>'
        + DEBUGGER_ATTR_COLUMNS.map(c => '<th data-sort-col="' + c.key + '">'
            + `<span class="qv-debugger-sort-label">${_esc(c.label)}${debuggerAttrSortIndicator(c.key)}</span>`
            + (c.key === 'source' ? '' : `<span class="qv-debugger-col-resize" data-resize-col="${c.key}"></span>`)
            + '</th>').join('')
        + '</tr></thead><tbody id="qv-debugger-attr-tbody"></tbody></table>'
        + '</div>'
        + '<div id="qv-debugger-override" class="qv-debugger-footer"></div>';

    renderDebuggerAttrRows();
    document.getElementById('qv-debugger-attr-search').addEventListener('input', (e) => {
        debuggerAttrSearch = e.target.value;
        renderDebuggerAttrRows();
    });

    if (debuggerSelectedAttr) renderDebuggerOverridePanel(debuggerSelectedAttr, debuggerAttrData[debuggerSelectedAttr]);
}

// renderDebuggerAttributesDetail rebuilds the whole #qv-debugger-detail
// subtree (innerHTML), which resets .qv-debugger-scroll's scroll position to
// 0 — fine for a genuine switch to a different object/tab (starting back at
// the top makes sense there), but jarring for a same-object re-render:
// selecting a row further down a long table, or re-rendering after Apply,
// used to visibly snap the table back to the top. Callers that are
// re-rendering the *same* object wrap the call in this so the scroll
// position survives the rebuild.
function withPreservedDebuggerScroll(renderFn) {
    const before = document.querySelector('#qv-debugger-detail .qv-debugger-scroll');
    const previousScrollTop = before ? before.scrollTop : 0;
    renderFn();
    const after = document.querySelector('#qv-debugger-detail .qv-debugger-scroll');
    if (after) after.scrollTop = previousScrollTop;
}

// The single "click a row above to edit it" panel — populated for whichever
// attribute is currently selected, instead of one input per row (see
// debuggerSelectedAttr's doc comment). item.CanOverride is false for value
// kinds with no simple literal syntax to write back (lists, dictionaries,
// scripts, ...) — an override textbox for those would just be a dead end, so
// a short note is shown instead of one (see Fields.cs's CanOverrideValue).
function renderDebuggerOverridePanel(attr, item) {
    const panel = document.getElementById('qv-debugger-override');
    if (!panel) return;

    if (!item.CanOverride) {
        panel.innerHTML = `<div class="text-sm font-semibold mb-1">${_esc(attr)}</div>`
            + '<div class="text-xs text-surface-500">This value isn\'t a simple type the debugger can override directly.</div>';
        return;
    }

    panel.innerHTML = `<div class="text-sm font-semibold mb-2">Override <code>${_esc(attr)}</code></div>`
        + '<div class="flex gap-2 items-center">'
        + `<input class="qv-debugger-input flex-1" type="text" value="${_esc(item.EditValue ?? item.Value)}" id="qv-debugger-override-input">`
        + '<button type="button" class="btn preset-tonal-primary" id="qv-debugger-override-apply">Apply</button>'
        + '</div>'
        + `<div class="text-xs text-surface-500 mt-1">${_esc(DEBUGGER_OVERRIDE_HINT)}</div>`
        + '<div class="text-xs preset-tonal-error hidden mt-1" id="qv-debugger-override-error"></div>';
}

function renderDebuggerDetail() {
    const detail = document.getElementById('qv-debugger-detail');
    if (!debuggerSelectedItem) {
        detail.innerHTML = '<p class="text-surface-500">Select an item to view details.</p>';
        return;
    }

    if (debuggerActiveTab === 'Walkthrough') {
        renderDebuggerWalkthroughDetail(debuggerSelectedItem);
    } else {
        renderDebuggerAttributesDetail(debuggerActiveTab, debuggerSelectedItem);
    }
}

function selectDebuggerTab(tab) {
    debuggerActiveTab = tab;
    // Same reasoning as wireDebuggerButton's reopen handling: switching back
    // to the Walkthrough tab while one is running should land on it, not a
    // blank "Select an item to view details."
    debuggerSelectedItem = tab === 'Walkthrough' ? runningWalkthroughName : null;
    debuggerSelectedAttr = null;
    debuggerAttrSort = { column: 'attribute', direction: 'asc' };
    debuggerAttrSearch = '';
    renderDebuggerTabs(JSON.parse(Bridge.GetDebuggerObjectTypesJson()));
    renderDebuggerList();
    renderDebuggerDetail();
}

async function applyDebugAttribute(attr, value) {
    const errorEl = document.getElementById('qv-debugger-override-error');
    const error = await Bridge.SetDebugAttribute(debuggerSelectedItem, attr, value);
    if (error) {
        if (errorEl) {
            errorEl.textContent = error;
            errorEl.classList.remove('hidden');
        }
        return;
    }
    // Re-render so Value/Source reflect the change (and any knock-on effects
    // the assignment had on other attributes of the same object) — the
    // override panel stays open on the same attribute (debuggerSelectedAttr
    // is unchanged) so applying again is a tight loop.
    withPreservedDebuggerScroll(() => renderDebuggerAttributesDetail(debuggerActiveTab, debuggerSelectedItem));
}

// Re-queries the DOM fresh rather than capturing element references once at
// the start of a run — the detail panel can be rebuilt from scratch one or
// more times while a walkthrough is running (closing/reopening the dialog,
// see wireDebuggerButton's doc comment), which would otherwise leave a
// stale/detached reference that no longer matches anything visible.
// [data-walkthrough-status]/[data-cancel-walkthrough] aren't name-scoped
// (only one detail panel exists at a time), so they're only touched when the
// panel currently on screen is actually still showing this walkthrough —
// otherwise the status of a *different* walkthrough the user has since
// selected would get clobbered.
function updateWalkthroughRunUi(name, { running, status }) {
    const runBtn = document.querySelector(`[data-run-walkthrough="${CSS.escape(name)}"]`);
    if (runBtn) runBtn.disabled = running;

    if (debuggerActiveTab !== 'Walkthrough' || debuggerSelectedItem !== name) return;
    const cancelBtn = document.querySelector('[data-cancel-walkthrough]');
    const statusEl = document.querySelector('[data-walkthrough-status]');
    if (cancelBtn) cancelBtn.classList.toggle('hidden', !running);
    if (statusEl) statusEl.textContent = status;
}

async function runSelectedWalkthrough(name) {
    runningWalkthroughName = name;
    updateWalkthroughRunUi(name, { running: true, status: 'Running…' });
    try {
        const error = await Bridge.RunWalkthrough(name);
        updateWalkthroughRunUi(name, { running: false, status: error ? `Error: ${error}` : 'Done' });
    } finally {
        runningWalkthroughName = null;
    }
}

function ensureDebuggerWired() {
    if (debuggerWired) return;
    debuggerWired = true;

    const dlg = document.getElementById('questVivaDebugger');
    if (!dlg) return;

    document.getElementById('qv-debugger-close').addEventListener('click', () => dlg.close());

    // Only needs setting once — #qv-debugger-list itself is never rebuilt
    // (only its innerHTML, by renderDebuggerList), so this inline width
    // survives every re-render and even a game restart (the dialog is on
    // swapInPlayerUi's preserve-list). Subsequent changes come from dragging
    // the splitter (wireDebuggerSplitter).
    document.getElementById('qv-debugger-list').style.width = `${debuggerListWidth}px`;

    wireDebuggerMoveResize();
    wireDebuggerSplitter();
    document.getElementById('qv-debugger-detail').addEventListener('pointerdown', wireDebuggerColumnResize);

    document.getElementById('qv-debugger-tabs').addEventListener('click', (e) => {
        const btn = e.target.closest('[data-tab]');
        if (btn) selectDebuggerTab(btn.dataset.tab);
    });

    document.getElementById('qv-debugger-list').addEventListener('click', (e) => {
        const btn = e.target.closest('[data-item]');
        if (!btn) return;
        debuggerSelectedItem = btn.dataset.item;
        debuggerSelectedAttr = null;
        // Fresh sort/search state for whichever object was just selected —
        // a leftover filter/sort from the previous object would otherwise
        // silently carry over and could hide attributes the user expects
        // to see (e.g. a search term that happens to match nothing here).
        debuggerAttrSort = { column: 'attribute', direction: 'asc' };
        debuggerAttrSearch = '';
        renderDebuggerList();
        renderDebuggerDetail();
    });

    document.getElementById('qv-debugger-detail').addEventListener('click', async (e) => {
        // A completed column-resize drag (pointerdown+move+pointerup on
        // [data-resize-col], handled separately above) still fires a
        // trailing click on whatever's underneath — swallow it here so it
        // doesn't also toggle that column's sort direction.
        if (e.target.closest('[data-resize-col]')) return;

        const sortCol = e.target.closest('[data-sort-col]');
        if (sortCol) {
            const col = sortCol.dataset.sortCol;
            debuggerAttrSort = debuggerAttrSort.column === col
                ? { column: col, direction: debuggerAttrSort.direction === 'asc' ? 'desc' : 'asc' }
                : { column: col, direction: 'asc' };
            renderDebuggerAttrRows();
            return;
        }

        const attrRow = e.target.closest('[data-attr-row]');
        if (attrRow) {
            debuggerSelectedAttr = attrRow.dataset.attrRow;
            renderDebuggerAttrRows();
            renderDebuggerOverridePanel(debuggerSelectedAttr, debuggerAttrData[debuggerSelectedAttr]);
            return;
        }

        if (e.target.closest('#qv-debugger-override-apply')) {
            const input = document.getElementById('qv-debugger-override-input');
            await applyDebugAttribute(debuggerSelectedAttr, input ? input.value : '');
            return;
        }

        const runBtn = e.target.closest('[data-run-walkthrough]');
        if (runBtn) {
            await runSelectedWalkthrough(runBtn.dataset.runWalkthrough);
            return;
        }

        if (e.target.closest('[data-cancel-walkthrough]')) {
            Bridge.CancelWalkthrough();
        }
    });
}

// Called after every WebPlayer.initUI() — #cmdDebug is part of playercore.htm,
// which is fully replaced on every boot/restart (see swapInPlayerUi), so its
// showModal() listener (wired in the shared playercore.js) is fresh each time
// and this "populate on open" listener needs re-attaching alongside it.
function wireDebuggerButton() {
    const cmdDebug = document.getElementById('cmdDebug');
    if (!cmdDebug) return;
    cmdDebug.addEventListener('click', () => {
        ensureDebuggerWired();
        applyDebuggerRect();
        debuggerActiveTab = 'Walkthrough';
        // If a walkthrough is running (started on a previous open of this
        // dialog, which fully rebuilds #qv-debugger-detail on every open —
        // see runningWalkthroughName's doc comment), jump straight to it so
        // its Running…/Cancel state is immediately visible again, rather
        // than resetting to a blank "Select an item to view details."
        debuggerSelectedItem = runningWalkthroughName;
        debuggerSelectedAttr = null;
        renderDebuggerTabs(JSON.parse(Bridge.GetDebuggerObjectTypesJson()));
        renderDebuggerList();
        renderDebuggerDetail();
    });
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

// The AppShell tab that was going to hand over game bytes (see the
// `source=local` boot branch below) turned out not to be there to answer —
// closed, navigated away, or superseded by a newer Start click in that same
// tab (see PlayCatalog.svelte's handleStart, which closes its previous
// channel before opening a new one). Without this, that's a silent infinite
// loading spinner with no way out; this at least gets the user back to a
// working picker in the same tab.
function showLocalHandoffTimeoutError() {
    document.documentElement.classList.remove('qv-booting');
    const pickers = document.getElementById('qv-pickers');
    const msg = document.getElementById('qv-loading-msg');
    const errorEl = document.getElementById('qv-error');
    if (msg) msg.style.display = 'none';
    if (pickers) pickers.style.display = 'block';
    if (!errorEl) return;
    errorEl.innerHTML = '<strong>Couldn\'t reconnect to Quest Viva.</strong> '
        + 'This tab was waiting for a game from the Play tab it was opened from, but that tab has since been closed, '
        + 'navigated away, or used to start a different game. Choose the file again below, or go back to Quest Viva '
        + 'and click Start again.';
    errorEl.style.display = 'block';
    wireStartScreen();
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
                await startGame(data.bytes, data.filename, bc, null, true);
            }
        };
        return;
    }

    // AppShell's Play tab (see PlayCatalog.svelte) — the user already picked
    // a file and clicked Start (browser build) or the window just got opened
    // straight from the file picker (Electron, see ipc/player.ts), so the
    // game bytes are sitting in that tab's memory. Same handoff as the editor
    // preview above, including resource-request support: in the browser
    // build a raw picked File has nothing to answer those with (so they just
    // go unanswered — fine for a self-contained .quest package, which is all
    // the plain file-input path in wireStartScreen() below ever supported
    // either), but Electron's PlayCatalog.svelte backs the picked file with a
    // real ElectronFileAdapter and answers them from disk, exactly like
    // editor-store.ts's previewInWasmPlayer does for the live editor. A
    // distinct channel name keeps this from cross-talking with a real
    // editor-preview tab open at the same time.
    if (params.get('source') === 'local') {
        const bc = new BroadcastChannel('quest-play-local');
        bc.postMessage({ type: 'ready' });
        // Covers the case where nothing ever answers — see
        // showLocalHandoffTimeoutError. 8s comfortably covers a normal
        // same-tick reply; DOMContentLoaded hasn't necessarily fired yet
        // when this timer is *set*, but it always has by the time it fires.
        const timeoutId = window.setTimeout(() => {
            bc.onmessage = null;
            bc.close();
            showLocalHandoffTimeoutError();
        }, 8000);
        bc.onmessage = async ({ data }) => {
            if (data.type === 'game') {
                window.clearTimeout(timeoutId);
                // Not closed (unlike the timeout branch above) — startGame
                // reassigns bc.onmessage itself to keep answering
                // 'resource-response' messages for the rest of the session.
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
            const apiResponse = await fetch(`${apiRoot}game/${id}?${clientInfoParams()}`);
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
