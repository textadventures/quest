import { writable, get } from "svelte/store";
import { zipSync } from "fflate";
import { PUBLIC_WASM_PLAYER_URL, PUBLIC_APPSHELL_VERSION } from "$env/static/public";
import { loadWasm } from "./wasm";
import type { WasmBridge } from "./wasm";
import type { AssetInfo, FileAdapter } from "./filesystem/types";
import { LocalDraftAdapter, shouldShowBackupBanner, markBackupBannerResolved } from "./filesystem/local-adapter";
import { ServerFileAdapter } from "./filesystem/server-adapter";
import { triggerDownload } from "./filesystem/download";
import { confirmDialog } from "./confirm";
import { showToast } from "./toast";
import type { TreeNode, EditorDataResponse, ScriptBlockData, ScriptCommandCategoriesData, ExpressionTemplateData, ExpressionTemplate, FullAttributeData, ExitsData, VerbInfo, ExpressionFunctionInfo } from "./types";

export type AddElementModalState = { type: "room" | "object" | "page" | "function" | "timer" | "walkthrough" | "template" | "dynamictemplate" | "type"; parent: string | null } | null;

let _bridge: WasmBridge | null = null;
let _adapter: FileAdapter | null = null;
let _loadedGameId: string | null = null;

export const isLoaded = writable(false);
export const loadingStatus = writable<string | null>(null);
export const addElementModal = writable<AddElementModalState>(null);
// Gamebook games are flat "pages" with no rooms/objects/verbs/exits — set once
// per load from EditorController.EditorStyle (see openGame()) and used to swap
// wording/affordances in the tree, toolbar and advanced-adders panel.
export const isGamebook = writable(false);

export function openAddModal(type: "room" | "object" | "page" | "function" | "timer" | "walkthrough" | "template" | "dynamictemplate" | "type", parent: string | null) {
    addElementModal.set({ type, parent });
}
// A Javascript element's src (the file it points to) can't be changed after creation — see
// PropertyEditor's "lockedAfterCreate" handling — so unlike the other adders above, creating one
// needs an up-front modal that gets a filename (existing/uploaded/new-blank) before the element
// ever exists, rather than creating an empty element and letting the properties panel fill it in.
export const addJavascriptModalOpen = writable(false);
export function openAddJavascriptModal() {
    addJavascriptModalOpen.set(true);
}
// An Included Library's filename can't be changed after creation either (same
// lockedaftercreate as Javascript's src — see CoreEditorIncludedLibrary.aslx) and, unlike
// Javascript, it can *only* ever point at an existing/uploaded file (a blank .aslx isn't a
// useful library), so its adder is the same up-front-modal shape but without JS's "type a
// new name to create a blank one" affordance.
export const addLibraryModalOpen = writable(false);
export function openAddLibraryModal() {
    addLibraryModalOpen.set(true);
}
// Holds the key of the element currently being moved, or null when the modal is closed.
export const moveElementModal = writable<string | null>(null);
export function openMoveModal(key: string) {
    moveElementModal.set(key);
}
export const gameFilename = writable<string | null>(null);
export const canSaveAs = writable(false);
export const canBackup = writable(false);
export const showBackupBanner = writable(false);
// Set when an Included Library is added or removed: like Quest 5's desktop editor, a
// library's own <editor>-defined panes are only picked up by EditorController.Initialise()
// at load time (neither CreateNewIncludedLibrary nor DeleteElement re-run it), so pane
// changes won't show up until reload.
export const showLibraryReloadBanner = writable(false);
export function dismissLibraryReloadBanner() {
    showLibraryReloadBanner.set(false);
}
export const canPublishToServer = writable(false);
export const treeNodes = writable<TreeNode[]>([]);
export const showLibraryElements = writable(false);
export const selectedKey = writable<string | null>(null);
export const selectedData = writable<EditorDataResponse | null>(null);
export const fullAttributeData = writable<FullAttributeData | null>(null);
export const canUndo = writable(false);
export const canRedo = writable(false);
// Selection history stack (Back/Forward), independent of Quest's own model undo/redo above —
// this just remembers which elements you've clicked through in the tree, like browser history.
let _navHistory: string[] = [];
let _navIndex = -1;
let _suppressHistoryPush = false;
export const canGoBack = writable(false);
export const canGoForward = writable(false);
function updateNavFlags() {
    canGoBack.set(_navIndex > 0);
    canGoForward.set(_navIndex >= 0 && _navIndex < _navHistory.length - 1);
}
// Called once a game (or a freshly reloaded model, see setGameXml) has already selected its
// root node via selectNode() — collapses whatever that call pushed down to a single starting
// entry, so Back/Forward don't reach across into the previous game/reload.
function resetNavHistory(currentKey: string | null) {
    _navHistory = currentKey ? [currentKey] : [];
    _navIndex = _navHistory.length - 1;
    updateNavFlags();
}
export async function navigateBack() {
    if (_navIndex <= 0) return;
    _navIndex--;
    _suppressHistoryPush = true;
    try { await selectNode(_navHistory[_navIndex]); } finally { _suppressHistoryPush = false; }
    updateNavFlags();
}
export async function navigateForward() {
    if (_navIndex >= _navHistory.length - 1) return;
    _navIndex++;
    _suppressHistoryPush = true;
    try { await selectNode(_navHistory[_navIndex]); } finally { _suppressHistoryPush = false; }
    updateNavFlags();
}
export const isDirty = writable(false);
// True while a field somewhere in the editor has an in-progress, not-yet-committed
// edit (set on input, cleared on focusout via a delegated listener in
// edit/+page.svelte) — the underlying attribute only commits to the WASM bridge
// on change/blur, so isDirty alone stays false while typing. Doesn't trigger
// autosave itself (nothing has actually changed in the bridge yet), but the
// toolbar pill and the close-tab/switch-tab guards both treat it the same as
// isDirty, since saveGame() blurs the focused field first — which flushes any
// pending edit into the bridge — before checking real isDirty and persisting.
export const isEditingField = writable(false);
// Not reactive state, just a plain reference — the toolbar's "Unsaved" chip
// uses this to restore focus to whatever the author was typing in after a
// click-to-save, since that click itself steals focus onto the chip.
let _lastEditedElement: HTMLElement | null = null;
export function getLastEditedElement(): HTMLElement | null {
    return _lastEditedElement;
}
export function markFieldEditing(el?: HTMLElement) {
    if (el) _lastEditedElement = el;
    isEditingField.set(true);
}
export function clearFieldEditing() {
    isEditingField.set(false);
}
export const scriptVersion = writable(0);
export const scriptClipboardHasContent = writable(false);
export const assets = writable<AssetInfo[]>([]);
export const assetManagerOpen = writable(false);
export const publishModalOpen = writable(false);
export const codeViewPanelOpen = writable(false);
// Bumped by Toolbar's toggle button when the panel is already open, asking CodeViewPanel to
// attempt closing — rather than the toolbar forcing codeViewPanelOpen to false directly, which
// would silently discard any not-yet-applied raw XML edits.
export const codeViewCloseRequested = writable(0);

function refreshUndoRedo() {
    canUndo.set(_bridge?.CanUndo() ?? false);
    canRedo.set(_bridge?.CanRedo() ?? false);
    const dirty = _bridge?.IsDirty() ?? false;
    isDirty.set(dirty);
    if (dirty) scheduleAutosave();
}

// Set by openGame() whenever it returns false, so callers can show the actual reason (a missing
// library, invalid XML, ...) instead of a generic "Failed to load game file." — see Initialise's
// "ok"/"error:{message}" convention in WasmEditorBridge.cs.
export const lastOpenGameError = writable<string | null>(null);

// An Included Library element only stores its filename (<include ref="lib.aslx"/>) — the
// library's actual content lives in the adapter's asset storage, not in the game bytes, so the
// engine can't resolve it on its own when (re)loading. AddLibraryModal only ever uploads
// .aslx files, so every asset with that extension is a candidate; stage them all into the WASM
// side (WasmEditorBridge.AddAdjacentFile) before Initialise/SetGameXml runs the load that needs
// them (see WorldModel.GetLibraryStream / ByteArrayGameDataProvider). Candidate discovery goes
// through listLibraryCandidates() rather than listAssets() — see its own comment on
// FileAdapter for why the two can't be the same list.
async function preloadAdjacentLibraryAssets(adapter: FileAdapter): Promise<void> {
    const candidates = adapter.listLibraryCandidates
        ? await adapter.listLibraryCandidates()
        : (await adapter.listAssets()).map(a => a.key).filter(key => key.toLowerCase().endsWith(".aslx"));
    for (const key of candidates) {
        const blob = await adapter.getAsset(key);
        if (!blob) continue;
        _bridge!.AddAdjacentFile(key, new Uint8Array(await blob.arrayBuffer()));
    }
}

export async function openGame(bytes: Uint8Array, filename: string, adapter: FileAdapter): Promise<boolean> {
    // A failed Initialise() below leaves WasmEditorBridge's own _controller null (see its
    // comments) — the WASM side has already discarded whatever was loaded before this call, so
    // if we *were* mid-session (reloadGame() re-opening the same game, or a switch to a
    // different game), there is no good "old" state left to keep showing. Remembered before the
    // reset below so the failure branch can tell a genuine first load (nothing to fall back to
    // anyway) apart from a failed re-open (stale tree/isLoaded=true would otherwise silently
    // point at a controller that no longer exists).
    const wasAlreadyLoaded = get(isLoaded);
    // Defensive reset: callers that switch games mid-session (Electron's menu
    // actions) already await saveGame() to flush the previous game first, so
    // this should always be a no-op in practice — but a leftover timer here
    // would otherwise fire against the wrong _bridge/_adapter below.
    cancelPendingAutosave();
    isSaving.set(false);
    saveError.set(null);
    lastOpenGameError.set(null);
    loadingStatus.set("Starting editor…");
    _bridge = await loadWasm();
    _adapter = adapter;
    canSaveAs.set(adapter.canSaveAs);
    canBackup.set(adapter instanceof LocalDraftAdapter);
    canPublishToServer.set(adapter instanceof ServerFileAdapter);
    showBackupBanner.set(adapter instanceof LocalDraftAdapter && await shouldShowBackupBanner(adapter.gameId));
    loadingStatus.set("Loading game…");
    await preloadAdjacentLibraryAssets(adapter);
    // Double rAF ensures the browser actually paints the status update before
    // Initialise blocks the JS thread (C# WASM calls are synchronous).
    await new Promise<void>(resolve => requestAnimationFrame(() => requestAnimationFrame(() => resolve())));
    const result = await _bridge.Initialise(bytes, filename);
    loadingStatus.set(null);
    clearAssetUrlCache();
    const ok = result === "ok";
    if (!ok) {
        lastOpenGameError.set(result.startsWith("error:") ? result.slice("error:".length) : result);
        // Stop rendering/interacting with the old game's now-disconnected tree — its bridge
        // calls would otherwise silently target nothing (see the _controller comment above).
        // A genuine first load (isLoaded was already false) has nothing to tear down here; its
        // own caller (e.g. +page.svelte's serverLoadError, /open's inline `error`) already
        // handles showing the failure without ever having rendered the editor in the first place.
        if (wasAlreadyLoaded) isLoaded.set(false);
    }
    if (ok) {
        _loadedGameId = _bridge.GetGameId() || null;
        isGamebook.set(_bridge.IsGamebook());
        const nodes: TreeNode[] = JSON.parse(_bridge.GetTreeNodes());
        treeNodes.set(nodes);
        showLibraryElements.set(false);
        showLibraryReloadBanner.set(false);
        isLoaded.set(true);
        gameFilename.set(filename);
        refreshUndoRedo();
        scriptClipboardHasContent.set(false);
        cutElementKeys.set(new Set());
        await refreshAssets();
        const gameNode = nodes.find(n => n.nodeType === "game");
        if (gameNode) await selectNode(gameNode.key);
        resetNavHistory(gameNode?.key ?? null);
    }
    return ok;
}

// Re-initializes the currently-open game in place (re-runs WASM Initialise(),
// picking up e.g. a newly-added library's <editor> panes — see
// LibraryReloadBanner.svelte) without a browser navigation, so the user isn't
// bounced back to the Create/Home picker the way a full page reload would be
// (there's no URL/session pointer back to a local-draft/FSA/Electron game —
// only this in-memory _adapter). Flushes any pending edit first so nothing is
// lost to the fresh Initialise() call.
// On failure this replaces the editor with a full-screen error (see +page.svelte's fallback
// branch, driven by isLoaded/lastOpenGameError) rather than leaving the stale tree up — the
// re-Initialise() has already discarded whatever was loaded before, so there's nothing safe
// left to keep displaying.
export async function reloadGame(): Promise<boolean> {
    if (!_adapter) return false;
    await saveGame();
    const bytes = await _adapter.reload();
    return openGame(bytes, _adapter.filename, _adapter);
}

// Read-only peek at the current game's XML for the raw Code View panel — unlike Save(), this
// does not clear the bridge's dirty flag (it's not a save action).
export function getGameXml(): string {
    return _bridge?.GetGameXml() ?? "";
}

// Applies raw-XML edits from the Code View panel. SetGameXml validates the new XML against a
// throwaway controller before touching the live one (see WasmEditorBridge.cs), so a malformed
// edit returns "error:..." without disturbing the currently-open game. On success this reloads
// the whole in-memory model, which mirrors openGame()'s post-load sequence — and necessarily
// wipes Quest's own undo/redo stack, which is why the caller (CodeViewPanel) confirms with the
// user before invoking this.
export async function setGameXml(xml: string): Promise<string> {
    if (!_bridge) return "error";
    cancelPendingAutosave();
    if (_adapter) await preloadAdjacentLibraryAssets(_adapter);
    // Double rAF ensures the browser actually paints the caller's "Applying…" state before
    // SetGameXml's Initialise() call blocks the JS thread (C# WASM calls are synchronous) — same
    // pattern openGame() uses below for the equivalent reason.
    await new Promise<void>(resolve => requestAnimationFrame(() => requestAnimationFrame(() => resolve())));
    const result = await _bridge.SetGameXml(xml);
    if (result === "ok") {
        clearAssetUrlCache();
        isGamebook.set(_bridge.IsGamebook());
        const nodes: TreeNode[] = JSON.parse(_bridge.GetTreeNodes());
        treeNodes.set(nodes);
        showLibraryElements.set(false);
        scriptClipboardHasContent.set(false);
        cutElementKeys.set(new Set());
        await refreshAssets();
        const gameNode = nodes.find(n => n.nodeType === "game");
        if (gameNode) await selectNode(gameNode.key);
        resetNavHistory(gameNode?.key ?? null);
        refreshUndoRedo();
        // SetGameXml() resets the bridge's dirty flag after the reload (matching Initialise()'s
        // own behavior), but the in-memory content now differs from the last persisted save, so
        // that must be corrected here rather than trusted from refreshUndoRedo() above.
        isDirty.set(true);
        scheduleAutosave();
    }
    return result;
}

export async function selectNode(key: string) {
    if (!_bridge) return;
    selectedKey.set(key);
    const json = await _bridge.GetEditorData(key);
    selectedData.set(json ? JSON.parse(json) : null);
    const attrJson = _bridge.GetFullAttributeData(key);
    fullAttributeData.set(attrJson ? JSON.parse(attrJson) : null);

    if (!_suppressHistoryPush && _navHistory[_navIndex] !== key) {
        _navHistory = _navHistory.slice(0, _navIndex + 1);
        _navHistory.push(key);
        _navIndex = _navHistory.length - 1;
        updateNavFlags();
    }
}

export function toggleShowLibraryElements(): void {
    if (!_bridge) return;
    const next = !get(showLibraryElements);
    _bridge.SetShowLibraryElements(next);
    showLibraryElements.set(next);
    refreshTree();
}

// "Copy into your game" — turns a read-only library element (from Core.aslx or another
// included library) into a normal, editable element of the user's own game.
export async function makeElementLocal(key: string): Promise<void> {
    if (!_bridge) return;
    _bridge.MakeElementLocal(key);
    refreshTree();
    await refreshSelectedDataAsync();
    refreshUndoRedo();
}

export function setAttribute(elementKey: string, attribute: string, controlType: string, value: string): string {
    if (!_bridge) return "error";
    const result = _bridge.SetAttribute(elementKey, attribute, controlType, value);
    if (result.startsWith("renamed:")) {
        const newKey = result.slice("renamed:".length);
        selectedKey.set(newKey);
        // Renaming changes the element's key, not the element — keep Back/Forward pointing at
        // it rather than leaving stale entries for a key that no longer resolves to anything.
        _navHistory = _navHistory.map(k => k === elementKey ? newKey : k);
        refreshTree();
        refreshSelectedData();
        refreshUndoRedo();
        return "ok";
    }
    if (result === "retitled") {
        // Attribute changed the element's display name without changing its key
        // (e.g. an Included Library's filename, a Javascript element's src) — the
        // tree label needs refreshing, but selection/history stay put.
        refreshTree();
        refreshSelectedData();
        refreshUndoRedo();
        return "ok";
    }
    refreshSelectedData();
    refreshUndoRedo();
    return result;
}

export function setMultiType(elementKey: string, attribute: string, newType: string): string {
    if (!_bridge) return "error";
    const result = _bridge.SetMultiType(elementKey, attribute, newType);
    refreshSelectedData();
    refreshUndoRedo();
    return result;
}

export function setObjectReference(elementKey: string, attribute: string, objectName: string): string {
    if (!_bridge) return "error";
    const result = _bridge.SetObjectReference(elementKey, attribute, objectName);
    refreshSelectedData();
    refreshUndoRedo();
    return result;
}

export function setSelectedFilter(elementKey: string, filterGroupName: string, filterValue: string): string {
    if (!_bridge) return "error";
    const result = _bridge.SetSelectedFilter(elementKey, filterGroupName, filterValue);
    refreshSelectedData();
    refreshUndoRedo();
    return result;
}

export function setDropdownType(elementKey: string, controlId: string, selectedType: string): string {
    if (!_bridge) return "error";
    const result = _bridge.SetDropdownType(elementKey, controlId, selectedType);
    refreshSelectedData();
    refreshUndoRedo();
    return result;
}

function blobToDataUrl(blob: Blob): Promise<string> {
    return new Promise((resolve) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result as string);
        reader.readAsDataURL(blob);
    });
}

let _previewChannel: BroadcastChannel | null = null;

export async function previewInWasmPlayer(wasmPlayerUrl: string, opts?: { recordWalkthrough?: string; runWalkthrough?: string }): Promise<void> {
    if (!_bridge || !_adapter) return;

    // Close stale channel so old bytes aren't sent to a new preview window.
    _previewChannel?.close();

    const filename = _adapter.filename;
    const adapter = _adapter;

    window.open(wasmPlayerUrl + "?source=editor", "_blank");
    const bc = new BroadcastChannel("quest-preview");
    _previewChannel = bc;

    bc.onmessage = async ({ data }) => {
        if (data.type === "ready") {
            // Serialize fresh on every 'ready' so WasmPlayer refreshes also pick up latest edits.
            if (!_bridge) return;
            const bytes = new TextEncoder().encode(_bridge.Save());
            bc.postMessage({
                type: "game",
                bytes,
                filename,
                recordWalkthrough: opts?.recordWalkthrough ?? null,
                runWalkthrough: opts?.runWalkthrough ?? null,
            });
        } else if (data.type === "resource-request") {
            const blob = await adapter.getAsset(data.name);
            if (blob) {
                const dataUrl = await blobToDataUrl(blob);
                bc.postMessage({ type: "resource-response", id: data.id, dataUrl });
            }
        } else if (data.type === "walkthrough-recorded") {
            if (!_bridge) return;
            _bridge.RecordWalkthroughSteps(data.name, JSON.stringify(data.steps));
            refreshSelectedData();
            refreshUndoRedo();
        }
    };
}

// Same default WasmPlayer URL Toolbar.svelte's Preview button uses, applied
// here too so the walkthrough Record button (ListEditor.svelte) doesn't need
// its own env plumbing.
export function recordWalkthrough(elementKey: string): Promise<void> {
    return previewInWasmPlayer(PUBLIC_WASM_PLAYER_URL || "/player/", { recordWalkthrough: elementKey });
}

export function playWalkthrough(elementKey: string): Promise<void> {
    return previewInWasmPlayer(PUBLIC_WASM_PLAYER_URL || "/player/", { runWalkthrough: elementKey });
}

// ── Autosave ─────────────────────────────────────────────────────────────────
// Edits already commit into the WASM model synchronously as they're made (see
// setAttribute etc. below, all wired to onchange/blur in the components) —
// that's what gives the v5-style "saved as you navigate" feel at the model
// layer. This section is only responsible for debouncing *persistence* of
// that already-current model out to the FileAdapter (disk/OPFS/server),
// instead of requiring an explicit Save click.

export const isSaving = writable(false);
export const saveError = writable<string | null>(null);

const AUTOSAVE_DEBOUNCE_MS = 800;
const RETRY_DELAYS_MS = [5000, 15000, 30000];

let _autosaveTimer: ReturnType<typeof setTimeout> | null = null;
let _retryTimer: ReturnType<typeof setTimeout> | null = null;
let _retryAttempt = 0;
let _saveInFlight: Promise<void> | null = null;
let _resaveQueued = false;

function cancelPendingAutosave(): void {
    if (_autosaveTimer !== null) { clearTimeout(_autosaveTimer); _autosaveTimer = null; }
    if (_retryTimer !== null) { clearTimeout(_retryTimer); _retryTimer = null; }
}

function scheduleAutosave(): void {
    if (_autosaveTimer !== null) clearTimeout(_autosaveTimer);
    _autosaveTimer = setTimeout(() => {
        _autosaveTimer = null;
        void persistNow();
    }, AUTOSAVE_DEBOUNCE_MS);
}

// Transient failures (e.g. ServerFileAdapter's PUT hitting a network blip)
// get a few automatic retries with backoff; scheduleRetry() is a no-op if one
// is already pending so a burst of failures doesn't stack up timers.
function scheduleRetry(): void {
    if (_retryTimer !== null) return;
    const delay = RETRY_DELAYS_MS[Math.min(_retryAttempt, RETRY_DELAYS_MS.length - 1)];
    _retryAttempt++;
    _retryTimer = setTimeout(() => {
        _retryTimer = null;
        void persistNow();
    }, delay);
}

// Serializes the bridge and writes it via the adapter. A caller that arrives
// while a save is already in flight (autosave firing right as a manual flush
// is in progress, say) folds into a single follow-up save afterwards rather
// than racing two concurrent writes to the same target.
export function persistNow(): Promise<void> {
    if (_saveInFlight) {
        _resaveQueued = true;
        return _saveInFlight;
    }
    _saveInFlight = doPersist().finally(() => {
        _saveInFlight = null;
        const queued = _resaveQueued;
        _resaveQueued = false;
        if (queued && get(isDirty)) void persistNow();
    });
    return _saveInFlight;
}

// A local draft's OPFS write is often fast enough (small game, no network)
// that isSaving flips true then false within ~10ms — well under a frame in
// practice, so the "Saving…" pill can go completely unseen (confirmed via
// rAF-precision polling: the class changes, but nothing ever gets painted in
// between). Same category of issue as wasm-player.js's nextPaint() calls
// elsewhere in this codebase — hold the visible state open for a minimum
// stretch so it's perceptible rather than a silent flicker.
const MIN_SAVING_VISIBLE_MS = 300;

async function doPersist(): Promise<void> {
    if (!_bridge || !_adapter) return;
    isSaving.set(true);
    const startedAt = performance.now();
    try {
        await rekeyIfGameIdChanged();
        await _adapter.saveFile(_bridge.Save()); // Save() clears _isDirty in the bridge
        isDirty.set(false);
        saveError.set(null);
        _retryAttempt = 0;
        if (_retryTimer !== null) { clearTimeout(_retryTimer); _retryTimer = null; }
        // Re-check rather than only at load: this is what lets the banner appear the
        // moment a fresh local draft crosses the activity threshold, instead of
        // waiting for the user to close and reopen it.
        if (_adapter instanceof LocalDraftAdapter) {
            showBackupBanner.set(await shouldShowBackupBanner(_adapter.gameId));
        }
    } catch (err) {
        saveError.set(err instanceof Error ? err.message : "Save failed");
        scheduleRetry();
    } finally {
        const elapsed = performance.now() - startedAt;
        if (elapsed < MIN_SAVING_VISIBLE_MS) {
            await new Promise(resolve => setTimeout(resolve, MIN_SAVING_VISIBLE_MS - elapsed));
        }
        isSaving.set(false);
    }
}

// If the user regenerated the gameid field (PropertyEditor.svelte's "Generate"
// button) since load, a GameId-keyed adapter (LocalDraftAdapter) needs to move
// its OPFS storage to the new key before the next write, or the old draft is
// orphaned and a new, empty one silently starts under the new id.
async function rekeyIfGameIdChanged(): Promise<void> {
    if (!_bridge || !_adapter?.rekey) return;
    const currentGameId = _bridge.GetGameId();
    if (!currentGameId || currentGameId === _loadedGameId) return;
    await _adapter.rekey(currentGameId);
    _loadedGameId = currentGameId;
}

// Forces any in-progress field edit to commit (blur fires its onchange) and
// flushes it to storage immediately, bypassing the debounce. Used for the
// Electron "Save" menu action, and awaited before navigating away to a
// different game or on tab hide/close, so nothing is left stranded in the
// debounce window.
export async function saveGame(): Promise<void> {
    (document.activeElement as HTMLElement | null)?.blur();
    cancelPendingAutosave();
    if (get(isDirty)) await persistNow();
}

// The status pill's "Retry" action after a failed autosave.
export function retrySave(): Promise<void> {
    cancelPendingAutosave();
    return persistNow();
}

export async function saveGameAs(): Promise<void> {
    if (!_bridge || !_adapter) return;
    (document.activeElement as HTMLElement | null)?.blur();
    cancelPendingAutosave();
    const newName = await _adapter.saveFileAs(_bridge.Save()); // Save() clears _isDirty in the bridge
    if (newName) gameFilename.set(newName);
    isDirty.set(false);
    saveError.set(null);
}

// Bundles the current game plus its assets into a downloadable .zip — the local
// draft's equivalent of "save a copy to a real file" since there's no disk file
// to save as. Uses a plain zip (not the .quest publish format): .quest packaging
// bundles included libraries into the .aslx itself, which is right for playback
// but would fork the game from its templates if re-imported for further editing.
export async function backupGame(): Promise<void> {
    if (!_bridge || !(_adapter instanceof LocalDraftAdapter)) return;
    cancelPendingAutosave();
    const gameXml = _bridge.Save(); // Save() clears _isDirty in the bridge
    isDirty.set(false);
    saveError.set(null);
    const zipEntries: Record<string, Uint8Array> = {
        [_adapter.filename]: new TextEncoder().encode(gameXml),
    };
    for (const asset of await _adapter.listAssets()) {
        const blob = await _adapter.getAsset(asset.key);
        if (blob) zipEntries[asset.key] = new Uint8Array(await blob.arrayBuffer());
    }
    const zipBytes = zipSync(zipEntries);
    const backupName = _adapter.filename.replace(/\.aslx$/i, "") + ".zip";
    triggerDownload(zipBytes, backupName);
    showBackupBanner.set(false);
    await markBackupBannerResolved(_adapter.gameId);
}

// The banner's own "Dismiss" action — distinct from backupGame() above, which
// also silences the banner as a side effect of an actual backup.
export async function dismissBackupBanner(): Promise<void> {
    showBackupBanner.set(false);
    if (_adapter instanceof LocalDraftAdapter) await markBackupBannerResolved(_adapter.gameId);
}

// Builds a .quest package (game.aslx with included libraries inlined, plus every
// known asset) — the same package format the v5 desktop editor called "Publish to
// file". Works identically for local and server-mode games while staging, since
// staging only touches the FileAdapter interface; the two modes diverge only in
// what happens to the finished bytes:
//   - server mode: POST to textadventures.co.uk's publish endpoint, then navigate
//     there to complete the submission (category/tags/visibility form).
//   - local mode: trigger a browser download; the user can use the site's classic
//     manual "submit a game" upload page with the downloaded file.
export async function publishGame(includeWalkthrough: boolean): Promise<void> {
    if (!_bridge || !_adapter) return;
    for (const asset of await _adapter.listAssets()) {
        const blob = await _adapter.getAsset(asset.key);
        if (blob) _bridge.AddPublishAsset(asset.key, new Uint8Array(await blob.arrayBuffer()));
    }
    const packageBytes = _bridge.CreatePublishPackage(includeWalkthrough);
    if (packageBytes.length === 0) throw new Error("Failed to build the .quest package.");

    if (_adapter instanceof ServerFileAdapter) {
        const resp = await fetch(`/api/editor/games/${_adapter.gameId}/publish`, {
            method: "POST",
            headers: { "Content-Type": "application/octet-stream" },
            body: new Blob([packageBytes.slice()]),
        });
        if (!resp.ok) throw new Error(`Publish failed: ${resp.status} ${resp.statusText}`);
        window.location.href = `/create/publish/${_adapter.gameId}`;
        return;
    }

    const publishName = _adapter.filename.replace(/\.aslx$/i, "") + ".quest";
    triggerDownload(packageBytes, publishName);
}

function bytesToBase64(bytes: Uint8Array): string {
    let binary = "";
    const chunkSize = 0x8000; // avoid a call-stack overflow from String.fromCharCode(...bytes) on a large package
    for (let i = 0; i < bytes.length; i += chunkSize) {
        binary += String.fromCharCode(...bytes.subarray(i, i + chunkSize));
    }
    return btoa(binary);
}

// Builds the same .quest package Publish… does (see publishGame above), then wraps it in a
// single self-contained HTML file that loads the WasmPlayer runtime from jsDelivr instead of
// needing any separate upload — see docs/wasmplayer-single-file-export.md and
// scripts/export-embedded.mjs (the standalone CLI equivalent this mirrors step-for-step). Reuses
// the exact WasmPlayer index.html already deployed alongside AppShell (PUBLIC_WASM_PLAYER_URL,
// the same file the Preview button loads) as its template, so the two flavors can't drift apart.
export async function exportSingleFile(): Promise<void> {
    if (!_bridge || !_adapter) return;

    // PUBLIC_APPSHELL_VERSION is the release tag (e.g. "v6.0.0-beta.49") in a real deployed
    // build (see deploy-play.yml) — only there is there a matching published npm version to pin
    // to, so this deliberately doesn't fall back to "latest" (see hosting.md's pinned-version
    // guidance) or to a git sha in local/preview builds.
    const version = PUBLIC_APPSHELL_VERSION?.replace(/^v/, "");
    if (!version) throw new Error("No release version available to pin the CDN runtime to — single-file export only works in a deployed build.");

    for (const asset of await _adapter.listAssets()) {
        const blob = await _adapter.getAsset(asset.key);
        if (blob) _bridge.AddPublishAsset(asset.key, new Uint8Array(await blob.arrayBuffer()));
    }
    const packageBytes = _bridge.CreatePublishPackage(false);
    if (packageBytes.length === 0) throw new Error("Failed to build the .quest package.");

    const templateUrl = `${PUBLIC_WASM_PLAYER_URL || "/player/"}index.html`;
    const templateResponse = await fetch(templateUrl);
    if (!templateResponse.ok) throw new Error(`Failed to fetch WasmPlayer template: HTTP ${templateResponse.status}`);
    const template = await templateResponse.text();

    const baseName = _adapter.filename.replace(/\.aslx$/i, "");
    const embeddedFilename = baseName + ".quest";
    const cdnBase = `https://cdn.jsdelivr.net/npm/@textadventures/quest-viva-wasmplayer@${version}/`;

    // Same three transforms as export-embedded.mjs — see that file's comments for why each is
    // needed (in particular why wasm-player.js itself, not this code, has to resolve its own
    // dynamic import of dotnet.js via an absolute URL rather than relying on <base href> alone).
    let html = template;
    html = html.replace('<html lang="en">', '<html lang="en" class="qv-booting">');
    html = html.replace("<head>", `<head>\n    <base href="${cdnBase}" />`);
    html = html.replace('    <script type="text/javascript" src="quest-config.js"></script>\n', "");
    const embedScript = "<script type=\"text/javascript\">\n"
        + `        window.QuestVivaEmbeddedGame = ${JSON.stringify(bytesToBase64(packageBytes))};\n`
        + `        window.QuestVivaEmbeddedGameFilename = ${JSON.stringify(embeddedFilename)};\n`
        + "    </script>\n"
        + "    <script type=\"text/javascript\" src=\"wasm-player.js\"></script>";
    html = html.replace('<script type="text/javascript" src="wasm-player.js"></script>', embedScript);

    triggerDownload(html, baseName + ".html");
}

// Keys of every Javascript/Included Library element currently in the tree — undo()/redo() diff
// this before/after to find ones that just reappeared, so the missing-asset check below only
// fires for elements the undo/redo itself brought back, not any pre-existing breakage elsewhere
// in the game (e.g. a hand-edited or legacy save).
function assetOwningKeys(): Set<string> {
    return new Set(get(treeNodes).filter(n => ASSET_OWNING_NODE_TYPES.has(n.nodeType)).map(n => n.key));
}

// deleteElement()'s asset cascade (see below) happens outside WorldModel's undo system entirely —
// undoing the element's own deletion is all Quest's Undo can do, so if that file is gone for good,
// undo/redo can resurrect an element whose filename now points at nothing. Can't fix that (the
// file really is gone), so at least surface it instead of leaving a silently broken element sitting
// in the tree.
async function warnAboutDetachedAssetOwners(beforeKeys: Set<string>): Promise<void> {
    if (!_adapter) return;
    const reappeared = get(treeNodes).filter(n => ASSET_OWNING_NODE_TYPES.has(n.nodeType) && !beforeKeys.has(n.key));
    for (const node of reappeared) {
        if (!node.text || node.text === NO_FILENAME_PLACEHOLDER) continue;
        const blob = await _adapter.getAsset(node.text);
        if (!blob) {
            showToast(`"${node.text}" was restored, but its file was permanently deleted and can't be recovered — delete this element or add a new file.`);
        }
    }
}

export async function undo() {
    const beforeKeys = assetOwningKeys();
    if (_bridge) await _bridge.Undo();
    refreshTree();
    refreshSelectedData();
    refreshUndoRedo();
    scriptVersion.update(n => n + 1);
    void warnAboutDetachedAssetOwners(beforeKeys);
}

export function redo() {
    const beforeKeys = assetOwningKeys();
    _bridge?.Redo();
    refreshTree();
    refreshSelectedData();
    refreshUndoRedo();
    scriptVersion.update(n => n + 1);
    void warnAboutDetachedAssetOwners(beforeKeys);
}

// ── Assets ───────────────────────────────────────────────────────────────────

const IMAGE_EXTENSIONS = new Set(["jpg", "jpeg", "png", "gif", "webp", "bmp", "svg"]);

// Unscoped classification (no <source> filter in play) — used by the standalone
// asset manager, which lists everything rather than filtering to one control's kind.
export function isImageAsset(filename: string): boolean {
    const dot = filename.lastIndexOf(".");
    return dot >= 0 && IMAGE_EXTENSIONS.has(filename.slice(dot + 1).toLowerCase());
}

export interface AssetFilter {
    // For the upload <input accept="...">. Empty string means no restriction.
    accept: string;
    kind: "image" | "other";
    matches(filename: string): boolean;
}

// Parses a Quest editor <source> filter like "*.jpg;*.jpeg;*.png;*.gif" or
// "*.wav;*.mp3" (already resolved from any [TemplateName] reference by the
// bridge) into something usable for filtering the asset list and restricting
// uploads. A missing/empty source (shouldn't happen for the known "file"
// controls, but defensive) falls back to permissive: show everything, no
// thumbnail.
export function parseAssetSource(source: string | null | undefined): AssetFilter {
    const exts = (source ?? "")
        .split(";")
        .map(s => s.trim().replace(/^\*/, "").replace(/^\./, "").toLowerCase())
        .filter(s => s.length > 0);
    if (exts.length === 0) {
        return { accept: "", kind: "other", matches: () => true };
    }
    const extSet = new Set(exts);
    return {
        accept: exts.map(e => `.${e}`).join(","),
        kind: exts.every(e => IMAGE_EXTENSIONS.has(e)) ? "image" : "other",
        matches: (filename: string) => {
            const dot = filename.lastIndexOf(".");
            return dot >= 0 && extSet.has(filename.slice(dot + 1).toLowerCase());
        },
    };
}

export async function refreshAssets(): Promise<void> {
    assets.set(_adapter ? await _adapter.listAssets() : []);
}

export async function uploadAsset(file: File): Promise<void> {
    if (!_adapter) return;
    await _adapter.putAsset(file.name, file);
    releaseAssetUrl(file.name); // in case this overwrote an asset with a cached object URL
    await refreshAssets();
}

// Picks a free asset filename starting from a template like "javascript.js" —
// used by the "+ New" affordance on file controls (e.g. CoreEditorJavascript's
// <newfile>) so creating a second new file doesn't clobber the first. Mirrors
// the base name if it's free, otherwise appends 1, 2, 3, … before the extension.
export function uniqueAssetName(template: string): string {
    const existing = new Set(get(assets).map(a => a.key));
    if (!existing.has(template)) return template;
    const dot = template.lastIndexOf(".");
    const stem = dot >= 0 ? template.slice(0, dot) : template;
    const ext = dot >= 0 ? template.slice(dot) : "";
    let i = 1;
    while (existing.has(`${stem}${i}${ext}`)) i++;
    return `${stem}${i}${ext}`;
}

// Reads an asset (e.g. a .js file referenced by a Javascript element's "src"
// attribute) as text, for controls like the JS source editor that edit file
// content rather than the attribute value itself.
export async function getAssetText(key: string): Promise<string | null> {
    if (!key || !_adapter) return null;
    const blob = await _adapter.getAsset(key);
    return blob ? await blob.text() : null;
}

export async function putAssetText(key: string, text: string): Promise<void> {
    if (!_adapter) return;
    await _adapter.putAsset(key, new Blob([text], { type: "text/javascript" }));
    releaseAssetUrl(key);
    await refreshAssets();
}

export async function deleteAssetByKey(key: string): Promise<void> {
    if (!_adapter) return;
    await _adapter.deleteAsset(key);
    releaseAssetUrl(key);
    await refreshAssets();
}

const assetUrlCache = new Map<string, string>();

function releaseAssetUrl(key: string) {
    const cached = assetUrlCache.get(key);
    if (cached) {
        URL.revokeObjectURL(cached);
        assetUrlCache.delete(key);
    }
}

function clearAssetUrlCache() {
    for (const url of assetUrlCache.values()) URL.revokeObjectURL(url);
    assetUrlCache.clear();
}

// Resolves an asset key to something usable as an <img src>. Server-mode
// assets already have a fetchable url from listAssets(); local-mode assets
// are read as a Blob and turned into a cached object URL (released when the
// asset changes or a new game is opened).
export async function resolveAssetUrl(key: string): Promise<string | null> {
    if (!key || !_adapter) return null;
    const info = get(assets).find(a => a.key === key);
    if (info?.url) return info.url;
    const cached = assetUrlCache.get(key);
    if (cached) return cached;
    const blob = await _adapter.getAsset(key);
    if (!blob) return null;
    const url = URL.createObjectURL(blob);
    assetUrlCache.set(key, url);
    return url;
}

// ── List editor functions ────────────────────────────────────────────────────

export function addListItem(elementKey: string, attribute: string, value: string): string {
    if (!_bridge) return "error";
    const result = _bridge.AddListItem(elementKey, attribute, value);
    refreshSelectedData();
    refreshUndoRedo();
    return result;
}

export function removeListItem(elementKey: string, attribute: string, key: string): string {
    if (!_bridge) return "error";
    const result = _bridge.RemoveListItem(elementKey, attribute, key);
    refreshSelectedData();
    refreshUndoRedo();
    return result;
}

export function updateListItem(elementKey: string, attribute: string, key: string, value: string): string {
    if (!_bridge) return "error";
    const result = _bridge.UpdateListItem(elementKey, attribute, key, value);
    refreshSelectedData();
    refreshUndoRedo();
    return result;
}

// ── Attributes editor functions ─────────────────────────────────────────────

export function removeAttribute(elementKey: string, attribute: string): string {
    if (!_bridge) return "error";
    const result = _bridge.RemoveAttribute(elementKey, attribute);
    if (result === "ok") refreshSelectedData();
    refreshUndoRedo();
    return result;
}

export function addInheritedType(elementKey: string, typeName: string): string {
    if (!_bridge) return "error";
    const result = _bridge.AddInheritedType(elementKey, typeName);
    if (result === "ok") refreshSelectedData();
    refreshUndoRedo();
    return result;
}

export function removeInheritedType(elementKey: string, typeName: string): string {
    if (!_bridge) return "error";
    const result = _bridge.RemoveInheritedType(elementKey, typeName);
    if (result === "ok") refreshSelectedData();
    refreshUndoRedo();
    return result;
}

export function getTypeNames(): string[] {
    if (!_bridge) return [];
    try { return JSON.parse(_bridge.GetTypeNames()); }
    catch { return []; }
}

export function addDictItem(elementKey: string, attribute: string, key: string, value: string): string {
    if (!_bridge) return "error";
    const result = _bridge.AddDictionaryItem(elementKey, attribute, key, value);
    if (result === "ok") refreshSelectedData();
    refreshUndoRedo();
    return result;
}

export function removeDictItem(elementKey: string, attribute: string, key: string): string {
    if (!_bridge) return "error";
    const result = _bridge.RemoveDictionaryItem(elementKey, attribute, key);
    if (result === "ok") refreshSelectedData();
    refreshUndoRedo();
    return result;
}

export function updateDictItem(elementKey: string, attribute: string, key: string, value: string): string {
    if (!_bridge) return "error";
    const result = _bridge.UpdateDictionaryItem(elementKey, attribute, key, value);
    if (result === "ok") refreshSelectedData();
    refreshUndoRedo();
    return result;
}

export function makeScriptEditable(elementKey: string, attribute: string): string {
    if (!_bridge) return "error";
    const result = _bridge.MakeScriptEditable(elementKey, attribute);
    if (result === "ok") { refreshSelectedData(); bumpScriptVersion(); }
    refreshUndoRedo();
    return result;
}

export function makeScriptDictEditable(elementKey: string, attribute: string): string {
    if (!_bridge) return "error";
    const result = _bridge.MakeScriptDictEditable(elementKey, attribute);
    if (result === "ok") { refreshSelectedData(); bumpScriptVersion(); }
    refreshUndoRedo();
    return result;
}

export function addScriptDictItem(elementKey: string, attribute: string, key: string): string {
    if (!_bridge) return "error";
    const result = _bridge.AddScriptDictionaryItem(elementKey, attribute, key);
    if (result === "ok") { refreshSelectedData(); bumpScriptVersion(); }
    refreshUndoRedo();
    return result;
}

export function removeScriptDictItem(elementKey: string, attribute: string, key: string): string {
    if (!_bridge) return "error";
    const result = _bridge.RemoveScriptDictionaryItem(elementKey, attribute, key);
    if (result === "ok") { refreshSelectedData(); bumpScriptVersion(); }
    refreshUndoRedo();
    return result;
}

export function changeAttributeType(elementKey: string, attribute: string, newType: string): string {
    if (!_bridge) return "error";
    const result = _bridge.ChangeAttributeType(elementKey, attribute, newType);
    if (result === "ok") refreshSelectedData();
    refreshUndoRedo();
    return result;
}

export function setPatternAttribute(elementKey: string, attribute: string, pattern: string): string {
    if (!_bridge) return "error";
    const result = _bridge.SetPatternAttribute(elementKey, attribute, pattern);
    if (result === "ok") refreshSelectedData();
    refreshUndoRedo();
    return result;
}

// ── Script editor functions ─────────────────────────────────────────────────

export function getScriptCode(elementKey: string, attribute: string): string {
    return _bridge?.GetScriptCode(elementKey, attribute) ?? "";
}

export function setScriptCode(elementKey: string, attribute: string, code: string): string {
    if (!_bridge) return "error";
    const result = _bridge.SetScriptCode(elementKey, attribute, code);
    if (result === "ok") bumpScriptVersion();
    refreshUndoRedo();
    return result;
}

export function copyScripts(elementKey: string, attribute: string, containerPath: string, indices: number[]): string {
    if (!_bridge) return "error";
    const result = _bridge.CopyScripts(elementKey, attribute, containerPath, JSON.stringify(indices));
    if (result === "ok") scriptClipboardHasContent.set(true);
    return result;
}

export function cutScripts(elementKey: string, attribute: string, containerPath: string, indices: number[]): string {
    if (!_bridge) return "error";
    const result = _bridge.CutScripts(elementKey, attribute, containerPath, JSON.stringify(indices));
    if (result === "ok") { bumpScriptVersion(); scriptClipboardHasContent.set(true); }
    refreshUndoRedo();
    return result;
}

export function deleteScripts(elementKey: string, attribute: string, containerPath: string, indices: number[]): string {
    if (!_bridge) return "error";
    const result = _bridge.DeleteScripts(elementKey, attribute, containerPath, JSON.stringify(indices));
    if (result === "ok") bumpScriptVersion();
    refreshUndoRedo();
    return result;
}

export function pasteScripts(elementKey: string, attribute: string, containerPath: string): string {
    if (!_bridge) return "error";
    const result = _bridge.PasteScripts(elementKey, attribute, containerPath);
    if (result === "ok") bumpScriptVersion();
    refreshUndoRedo();
    return result;
}

export function getScriptData(elementKey: string, attribute: string): ScriptBlockData | null {
    if (!_bridge) return null;
    const json = _bridge.GetScriptData(elementKey, attribute);
    return json ? JSON.parse(json) : null;
}

function bumpScriptVersion() {
    scriptVersion.update(n => n + 1);
}

export function setScriptParameter(elementKey: string, attribute: string, containerPath: string, scriptIndex: number, paramName: string, value: string): string {
    if (!_bridge) return "error";
    const result = _bridge.SetScriptParameter(elementKey, attribute, containerPath, scriptIndex, paramName, value);
    if (result === "ok") bumpScriptVersion();
    refreshUndoRedo();
    return result;
}

export function addScriptListItem(elementKey: string, attribute: string, containerPath: string, scriptIndex: number, paramAttribute: string, value: string): string {
    if (!_bridge) return "error";
    const result = _bridge.AddScriptListItem(elementKey, attribute, containerPath, scriptIndex, paramAttribute, value);
    if (result === "ok") bumpScriptVersion();
    refreshUndoRedo();
    return result;
}

export function removeScriptListItem(elementKey: string, attribute: string, containerPath: string, scriptIndex: number, paramAttribute: string, key: string): string {
    if (!_bridge) return "error";
    const result = _bridge.RemoveScriptListItem(elementKey, attribute, containerPath, scriptIndex, paramAttribute, key);
    if (result === "ok") bumpScriptVersion();
    refreshUndoRedo();
    return result;
}

export function updateScriptListItem(elementKey: string, attribute: string, containerPath: string, scriptIndex: number, paramAttribute: string, key: string, value: string): string {
    if (!_bridge) return "error";
    const result = _bridge.UpdateScriptListItem(elementKey, attribute, containerPath, scriptIndex, paramAttribute, key, value);
    if (result === "ok") bumpScriptVersion();
    refreshUndoRedo();
    return result;
}

export function setScriptListItemCount(elementKey: string, attribute: string, containerPath: string, scriptIndex: number, paramAttribute: string, count: number): string {
    if (!_bridge) return "error";
    const result = _bridge.SetScriptListItemCount(elementKey, attribute, containerPath, scriptIndex, paramAttribute, count);
    if (result === "ok") bumpScriptVersion();
    refreshUndoRedo();
    return result;
}

export function setIfExpression(elementKey: string, attribute: string, containerPath: string, scriptIndex: number, expression: string): string {
    if (!_bridge) return "error";
    const result = _bridge.SetIfExpression(elementKey, attribute, containerPath, scriptIndex, expression);
    if (result === "ok") bumpScriptVersion();
    refreshUndoRedo();
    return result;
}

export function setElseIfExpression(elementKey: string, attribute: string, containerPath: string, scriptIndex: number, elseIfIndex: number, expression: string): string {
    if (!_bridge) return "error";
    const result = _bridge.SetElseIfExpression(elementKey, attribute, containerPath, scriptIndex, elseIfIndex, expression);
    if (result === "ok") bumpScriptVersion();
    refreshUndoRedo();
    return result;
}

export function addScript(elementKey: string, attribute: string, containerPath: string, keyword: string): string {
    if (!_bridge) return "error";
    const result = _bridge.AddScript(elementKey, attribute, containerPath, keyword);
    if (result === "ok") bumpScriptVersion();
    refreshUndoRedo();
    return result;
}

export function deleteScript(elementKey: string, attribute: string, containerPath: string, scriptIndex: number): string {
    if (!_bridge) return "error";
    const result = _bridge.DeleteScript(elementKey, attribute, containerPath, scriptIndex);
    if (result === "ok") bumpScriptVersion();
    refreshUndoRedo();
    return result;
}

export function moveScript(elementKey: string, attribute: string, containerPath: string, index1: number, index2: number): string {
    if (!_bridge) return "error";
    const result = _bridge.MoveScript(elementKey, attribute, containerPath, index1, index2);
    if (result === "ok") bumpScriptVersion();
    refreshUndoRedo();
    return result;
}

export function addElse(elementKey: string, attribute: string, containerPath: string, scriptIndex: number): string {
    if (!_bridge) return "error";
    const result = _bridge.AddElse(elementKey, attribute, containerPath, scriptIndex);
    if (result === "ok") bumpScriptVersion();
    refreshUndoRedo();
    return result;
}

export function addElseIf(elementKey: string, attribute: string, containerPath: string, scriptIndex: number): string {
    if (!_bridge) return "error";
    const result = _bridge.AddElseIf(elementKey, attribute, containerPath, scriptIndex);
    if (result === "ok") bumpScriptVersion();
    refreshUndoRedo();
    return result;
}

export function removeElse(elementKey: string, attribute: string, containerPath: string, scriptIndex: number): string {
    if (!_bridge) return "error";
    const result = _bridge.RemoveElse(elementKey, attribute, containerPath, scriptIndex);
    if (result === "ok") bumpScriptVersion();
    refreshUndoRedo();
    return result;
}

export function removeElseIf(elementKey: string, attribute: string, containerPath: string, scriptIndex: number, elseIfIndex: number): string {
    if (!_bridge) return "error";
    const result = _bridge.RemoveElseIf(elementKey, attribute, containerPath, scriptIndex, elseIfIndex);
    if (result === "ok") bumpScriptVersion();
    refreshUndoRedo();
    return result;
}

export async function getScriptCommandCategories(): Promise<ScriptCommandCategoriesData | null> {
    if (!_bridge) return null;
    try { return JSON.parse(await _bridge.GetScriptCommandCategories()); }
    catch { return null; }
}

export function getObjectNames(): string[] | null {
    if (!_bridge) return null;
    try {
        return JSON.parse(_bridge.GetObjectNames());
    } catch { return null; }
}

export function getExitNames(): string[] | null {
    if (!_bridge) return null;
    try {
        return JSON.parse(_bridge.GetExitNames());
    } catch { return null; }
}

export function getExpressionTemplates(expressionType: string): ExpressionTemplate[] | null {
    if (!_bridge) return null;
    try {
        return JSON.parse(_bridge.GetExpressionTemplates(expressionType));
    } catch { return null; }
}

export function getExpressionTemplateData(expression: string, expressionType: string): ExpressionTemplateData | null {
    if (!_bridge) return null;
    try {
        const json = _bridge.GetExpressionTemplateData(expression, expressionType);
        return json ? JSON.parse(json) : null;
    } catch { return null; }
}

function refreshSelectedData() {
    void refreshSelectedDataAsync();
}

async function refreshSelectedDataAsync() {
    const key = get(selectedKey);
    if (!_bridge || !key) return;
    const json = await _bridge.GetEditorData(key);
    selectedData.set(json ? JSON.parse(json) : null);
    const attrJson = _bridge.GetFullAttributeData(key);
    fullAttributeData.set(attrJson ? JSON.parse(attrJson) : null);
}

function refreshTree() {
    if (!_bridge) return;
    treeNodes.set(JSON.parse(_bridge.GetTreeNodes()));
}

// ── Element creation / deletion ─────────────────────────────────────────────

export function validateName(name: string): string {
    return _bridge?.ValidateName(name) ?? "error";
}

export function getUniqueName(baseName: string): string {
    return _bridge?.GetUniqueName(baseName) ?? baseName;
}

function afterCreate(result: string, select = true): string {
    if (result.startsWith("error:")) return result;
    refreshTree();
    if (select) void selectNode(result);
    refreshUndoRedo();
    return result;
}

export function createRoom(name: string, parent: string | null): string {
    if (!_bridge) return "error:not loaded";
    return afterCreate(_bridge.CreateRoom(name, parent ?? ""));
}

export function createObject(name: string, parent: string | null): string {
    if (!_bridge) return "error:not loaded";
    return afterCreate(_bridge.CreateObject(name, parent ?? ""));
}

// Like createObject, but doesn't navigate the selection to the new element —
// used when creating an object as a side effect of another action (e.g.
// creating a gamebook page inline while linking to it) where the user should
// stay on the page they were editing.
export function createObjectSilent(name: string, parent: string | null): string {
    if (!_bridge) return "error:not loaded";
    return afterCreate(_bridge.CreateObject(name, parent ?? ""), false);
}

export function createFunction(name: string): string {
    if (!_bridge) return "error:not loaded";
    return afterCreate(_bridge.CreateFunction(name));
}

export function createTimer(name: string): string {
    if (!_bridge) return "error:not loaded";
    return afterCreate(_bridge.CreateTimer(name));
}

export function createExit(parent: string): string {
    if (!_bridge) return "error:not loaded";
    return afterCreate(_bridge.CreateExit(parent));
}

export function createTurnScript(parent: string): string {
    if (!_bridge) return "error:not loaded";
    return afterCreate(_bridge.CreateTurnScript(parent));
}

export function createCommand(parent: string | null): string {
    if (!_bridge) return "error:not loaded";
    return afterCreate(_bridge.CreateCommand(parent ?? ""));
}

export function createVerb(parent: string | null): string {
    if (!_bridge) return "error:not loaded";
    return afterCreate(_bridge.CreateVerb(parent ?? ""));
}

export function createWalkthrough(name: string, parent: string | null = null): string {
    if (!_bridge) return "error:not loaded";
    return afterCreate(_bridge.CreateWalkthrough(name, parent ?? ""));
}

export function createTemplate(name: string): string {
    if (!_bridge) return "error:not loaded";
    return afterCreate(_bridge.CreateTemplate(name));
}

export function createDynamicTemplate(name: string): string {
    if (!_bridge) return "error:not loaded";
    return afterCreate(_bridge.CreateDynamicTemplate(name));
}

export function createObjectType(name: string): string {
    if (!_bridge) return "error:not loaded";
    return afterCreate(_bridge.CreateObjectType(name));
}

export function createIncludedLibrary(filename: string): string {
    if (!_bridge) return "error:not loaded";
    const result = afterCreate(_bridge.CreateIncludedLibrary(filename));
    if (!result.startsWith("error:")) showLibraryReloadBanner.set(true);
    return result;
}

export function createJavascript(src: string): string {
    if (!_bridge) return "error:not loaded";
    return afterCreate(_bridge.CreateJavascript(src));
}

// Javascript's src and Included Library's filename are the only element attributes that own an
// asset outright (lockedaftercreate, one file per element — see AddJavascriptModal/AddLibraryModal),
// and the tree label mirrors that filename exactly (EditorController.GetDisplayName), so the asset
// key for one of these nodes is just its own text.
const ASSET_OWNING_NODE_TYPES = new Set(["javascript", "include"]);

// Finds another element of the same asset-owning type still pointing at the same file — deleting
// this element shouldn't take the asset out from under it.
function assetStillReferenced(key: string, nodeType: string, assetKey: string): boolean {
    return get(treeNodes).some(n => n.key !== key && n.nodeType === nodeType && n.text === assetKey);
}

const NO_FILENAME_PLACEHOLDER = "(filename not set)"; // EditorController.GetDisplayName's fallback

export async function deleteElement(key: string): Promise<void> {
    if (!_bridge) return;
    const node = get(treeNodes).find(n => n.key === key);
    // Model edits are covered by Quest's own Undo, but an asset file lives outside that — its
    // deletion is permanent, so confirm before an element delete would take one out with it. Both
    // owning types are lockedaftercreate and only ever created with a real uploaded file (see
    // AddJavascriptModal/AddLibraryModal), so a set filename is trusted here rather than checked
    // against the `assets` store — included-library .aslx files are deliberately excluded from
    // that store's listing on most adapters (LocalDraftAdapter/BrowserFileAdapter/ElectronAdapter
    // all filter `.aslx` out of listAssets() to avoid ever surfacing the game's own file as a
    // generic asset), so they'd never match there even though the file is real.
    if (node && ASSET_OWNING_NODE_TYPES.has(node.nodeType) && node.text && node.text !== NO_FILENAME_PLACEHOLDER
        && !assetStillReferenced(key, node.nodeType, node.text)) {
        const ok = await confirmDialog(
            `Delete "${node.text}"? This will also permanently delete that file from your assets — this can't be undone.`,
            { confirmLabel: "Delete", danger: true },
        );
        if (!ok) return;
        performDeleteElement(key);
        // Best-effort: some adapters (e.g. ServerFileAdapter) throw on a delete that 404s, which
        // shouldn't block the element delete above from having already gone through.
        try { await deleteAssetByKey(node.text); } catch { /* already gone, or adapter-side 404 */ }
        return;
    }
    performDeleteElement(key);
}

// Asset Manager's delete, for an asset owned by a Javascript/Included Library element — removes
// that element too rather than leaving it pointing at a now-missing file, keeping the two in sync
// regardless of which side (element or asset) the delete was triggered from. No confirm here: the
// caller (AssetManagerModal) already confirmed, naming the owning element in its own message.
export async function deleteAssetAndOwner(key: string): Promise<void> {
    const owner = get(treeNodes).find(n => ASSET_OWNING_NODE_TYPES.has(n.nodeType) && n.text === key);
    if (owner) performDeleteElement(owner.key);
    await deleteAssetByKey(key);
}

function performDeleteElement(key: string) {
    if (!_bridge) return;
    // Same staleness as adding one (see createIncludedLibrary above): a removed library's
    // <editor> panes stay registered in m_editorDefinitions until Initialise() re-runs, so a
    // deleted library can leave the editor showing panes for elements that no longer exist.
    if (get(treeNodes).find(n => n.key === key)?.nodeType === "include") showLibraryReloadBanner.set(true);
    _bridge.DeleteElement(key);
    selectedKey.set(null);
    selectedData.set(null);
    // Drop the deleted element from Back/Forward history too — it no longer resolves to
    // anything, so leaving it in would let navigateBack/Forward select a dead key.
    const keptBefore = _navHistory.slice(0, _navIndex + 1).filter(k => k !== key).length;
    _navHistory = _navHistory.filter(k => k !== key);
    _navIndex = keptBefore - 1;
    updateNavFlags();
    refreshTree();
    refreshUndoRedo();
}

// For deleting a child of the currently-selected node (e.g. a room's exit) where the parent's own
// tabs/attributes don't depend on that child's existence — leaves selectedKey/selectedData alone
// so the parent stays selected and PropertyEditor's active tab doesn't reset. deleteElement() above
// always clears the selection, which is right when the deleted item might itself be selected, but
// briefly nulling it out here would make PropertyEditor treat the reselect as a "different node"
// and jump back to its first tab.
export function deleteChildElement(key: string) {
    if (!_bridge) return;
    _bridge.DeleteElement(key);
    refreshTree();
    refreshUndoRedo();
}

// Same as deleteChildElement(), but wraps every deletion in one transaction — deleting them one
// at a time would each open/close their own transaction, so a single Undo would only restore the
// last of them.
export function deleteChildElements(keys: string[]) {
    if (!_bridge) return;
    _bridge.DeleteElements(JSON.stringify(keys));
    refreshTree();
    refreshUndoRedo();
}

export function swapElements(key1: string, key2: string): string {
    if (!_bridge) return "error";
    const result = _bridge.SwapElements(key1, key2);
    if (result === "ok") refreshTree();
    return result;
}

// ── Move / cut / copy / paste ───────────────────────────────────────────────

export function canMoveElement(key: string): boolean {
    return _bridge?.CanMoveElement(key) ?? false;
}

export function getMovePossibleParents(key: string): string[] {
    if (!_bridge) return [];
    return JSON.parse(_bridge.GetMovePossibleParents(key));
}

export function moveElement(key: string, newParent: string): string {
    if (!_bridge) return "error";
    const result = _bridge.MoveElement(key, newParent);
    if (result === "ok") {
        refreshTree();
        void selectNode(key);
        refreshUndoRedo();
    }
    return result;
}

// Bumped by copyElements/cutElements so TreePanel's per-node "⋯" menus (computed
// inline from nodeMenuOptions(), not driven by a store) know to recompute their
// "Paste" entry — the clipboard lives entirely in EditorController and mutating it
// doesn't touch treeNodes/selectedKey/anything else Svelte would already react to.
export const clipboardVersion = writable(0);

// Keys currently staged by Cut (not yet pasted) — TreePanel dims these rows so
// a cut element doesn't look untouched. Mirrors EditorController's own
// m_lastelementscutout flag: cleared by Copy (a fresh copy isn't a cut) and by
// a completed Paste (the element has landed at its new parent), set by Cut.
export const cutElementKeys = writable<Set<string>>(new Set());

export function copyElements(keys: string[]) {
    _bridge?.CopyElements(JSON.stringify(keys));
    cutElementKeys.set(new Set());
    clipboardVersion.update(n => n + 1);
}

export function cutElements(keys: string[]) {
    _bridge?.CutElements(JSON.stringify(keys));
    cutElementKeys.set(new Set(keys));
    clipboardVersion.update(n => n + 1);
}

export function canPasteElements(parentKey: string): boolean {
    return _bridge?.CanPasteElements(parentKey) ?? false;
}

export function pasteElements(parentKey: string): string {
    if (!_bridge) return "error";
    const result = _bridge.PasteElements(parentKey);
    if (result !== "error") {
        cutElementKeys.set(new Set());
        refreshTree();
        void selectNode(result);
        refreshUndoRedo();
    }
    return result;
}

// ── Exits editor ─────────────────────────────────────────────────────────────

export function getExitsData(roomKey: string): ExitsData | null {
    if (!_bridge) return null;
    const json = _bridge.GetExitsData(roomKey);
    return json ? JSON.parse(json) : null;
}

// Deliberately doesn't call selectNode() (unlike the create* helpers above via afterCreate) —
// quick-creating a directional exit should keep the room itself selected/in view.
export function createExitInDirection(roomKey: string, direction: string, to: string, createInverse: boolean): string {
    if (!_bridge) return "error:not loaded";
    const result = _bridge.CreateExitInDirection(roomKey, direction, to, createInverse);
    if (!result.startsWith("error:")) {
        refreshTree();
        refreshUndoRedo();
    }
    return result;
}

export function createLookExitInDirection(roomKey: string, direction: string): string {
    if (!_bridge) return "error:not loaded";
    const result = _bridge.CreateLookExitInDirection(roomKey, direction);
    if (!result.startsWith("error:")) {
        refreshTree();
        refreshUndoRedo();
    }
    return result;
}

// ── Verbs editor ─────────────────────────────────────────────────────────────

export function getVerbAttributesInfo(): VerbInfo[] {
    if (!_bridge) return [];
    try { return JSON.parse(_bridge.GetVerbAttributesInfo()); }
    catch { return []; }
}

// ── Expression helper ────────────────────────────────────────────────────────

export function getExpressionFunctions(): ExpressionFunctionInfo[] {
    if (!_bridge) return [];
    try { return JSON.parse(_bridge.GetExpressionFunctions()); }
    catch { return []; }
}

// A new custom verb also creates a game-wide command element (visible under the Commands tree
// node), so unlike createExitInDirection() this needs refreshTree() too, not just refreshSelectedData().
export function addVerb(elementKey: string, verbPattern: string): string {
    if (!_bridge) return "error:not loaded";
    const result = _bridge.AddVerb(elementKey, verbPattern);
    if (result.startsWith("ok:")) {
        refreshTree();
        refreshSelectedData();
        refreshUndoRedo();
    }
    return result;
}
