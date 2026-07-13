import { writable, get } from "svelte/store";
import { zipSync } from "fflate";
import { loadWasm } from "./wasm";
import type { WasmBridge } from "./wasm";
import type { AssetInfo, FileAdapter } from "./filesystem/types";
import { LocalDraftAdapter } from "./filesystem/local-adapter";
import { ServerFileAdapter } from "./filesystem/server-adapter";
import { triggerDownload } from "./filesystem/download";
import type { TreeNode, EditorDataResponse, ScriptBlockData, ScriptCommandCategoriesData, IfExpressionTemplateData, IfExpressionTemplate, FullAttributeData } from "./types";

export type AddElementModalState = { type: "room" | "object" | "function" | "timer" | "walkthrough" | "template" | "dynamictemplate" | "type"; parent: string | null } | null;

let _bridge: WasmBridge | null = null;
let _adapter: FileAdapter | null = null;
let _loadedGameId: string | null = null;

export const isLoaded = writable(false);
export const loadingStatus = writable<string | null>(null);
export const addElementModal = writable<AddElementModalState>(null);

export function openAddModal(type: "room" | "object" | "function" | "timer" | "walkthrough" | "template" | "dynamictemplate" | "type", parent: string | null) {
    addElementModal.set({ type, parent });
}
export const gameFilename = writable<string | null>(null);
export const canSaveAs = writable(false);
export const canBackup = writable(false);
export const canPublishToServer = writable(false);
export const treeNodes = writable<TreeNode[]>([]);
export const selectedKey = writable<string | null>(null);
export const selectedData = writable<EditorDataResponse | null>(null);
export const fullAttributeData = writable<FullAttributeData | null>(null);
export const canUndo = writable(false);
export const canRedo = writable(false);
export const isDirty = writable(false);
export const scriptVersion = writable(0);
export const scriptClipboardHasContent = writable(false);
export const assets = writable<AssetInfo[]>([]);
export const assetManagerOpen = writable(false);
export const publishModalOpen = writable(false);

function refreshUndoRedo() {
    canUndo.set(_bridge?.CanUndo() ?? false);
    canRedo.set(_bridge?.CanRedo() ?? false);
    isDirty.set(_bridge?.IsDirty() ?? false);
}

export async function openGame(bytes: Uint8Array, filename: string, adapter: FileAdapter): Promise<boolean> {
    loadingStatus.set("Starting editor…");
    _bridge = await loadWasm();
    _adapter = adapter;
    canSaveAs.set(adapter.canSaveAs);
    canBackup.set(adapter instanceof LocalDraftAdapter);
    canPublishToServer.set(adapter instanceof ServerFileAdapter);
    loadingStatus.set("Loading game…");
    // Double rAF ensures the browser actually paints the status update before
    // Initialise blocks the JS thread (C# WASM calls are synchronous).
    await new Promise<void>(resolve => requestAnimationFrame(() => requestAnimationFrame(() => resolve())));
    const ok = await _bridge.Initialise(bytes, filename);
    loadingStatus.set(null);
    clearAssetUrlCache();
    if (ok) {
        _loadedGameId = _bridge.GetGameId() || null;
        const nodes: TreeNode[] = JSON.parse(_bridge.GetTreeNodes());
        treeNodes.set(nodes);
        isLoaded.set(true);
        gameFilename.set(filename);
        refreshUndoRedo();
        scriptClipboardHasContent.set(false);
        await refreshAssets();
        const gameNode = nodes.find(n => n.nodeType === "game");
        if (gameNode) await selectNode(gameNode.key);
    }
    return ok;
}

export async function selectNode(key: string) {
    if (!_bridge) return;
    selectedKey.set(key);
    const json = await _bridge.GetEditorData(key);
    selectedData.set(json ? JSON.parse(json) : null);
    const attrJson = _bridge.GetFullAttributeData(key);
    fullAttributeData.set(attrJson ? JSON.parse(attrJson) : null);
}

export function setAttribute(elementKey: string, attribute: string, controlType: string, value: string): string {
    if (!_bridge) return "error";
    const result = _bridge.SetAttribute(elementKey, attribute, controlType, value);
    if (result.startsWith("renamed:")) {
        const newKey = result.slice("renamed:".length);
        selectedKey.set(newKey);
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

export async function previewInWasmPlayer(wasmPlayerUrl: string): Promise<void> {
    if (!_bridge || !_adapter) return;

    // Close stale channel so old bytes aren't sent to a new preview window.
    _previewChannel?.close();

    const filename = _adapter.filename;
    const adapter = _adapter;

    window.open(wasmPlayerUrl + '?source=editor', '_blank');
    const bc = new BroadcastChannel('quest-preview');
    _previewChannel = bc;

    bc.onmessage = async ({ data }) => {
        if (data.type === 'ready') {
            // Serialize fresh on every 'ready' so WasmPlayer refreshes also pick up latest edits.
            if (!_bridge) return;
            const bytes = new TextEncoder().encode(_bridge.Save());
            bc.postMessage({ type: 'game', bytes, filename });
        } else if (data.type === 'resource-request') {
            const blob = await adapter.getAsset(data.name);
            if (blob) {
                const dataUrl = await blobToDataUrl(blob);
                bc.postMessage({ type: 'resource-response', id: data.id, dataUrl });
            }
        }
    };
}

export async function saveGame(): Promise<void> {
    if (!_bridge || !_adapter) return;
    await rekeyIfGameIdChanged();
    await _adapter.saveFile(_bridge.Save()); // Save() clears _isDirty in the bridge
    isDirty.set(false);
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

export async function saveGameAs(): Promise<void> {
    if (!_bridge || !_adapter) return;
    const newName = await _adapter.saveFileAs(_bridge.Save()); // Save() clears _isDirty in the bridge
    if (newName) gameFilename.set(newName);
    isDirty.set(false);
}

// Bundles the current game plus its assets into a downloadable .zip — the local
// draft's equivalent of "save a copy to a real file" since there's no disk file
// to save as. Uses a plain zip (not the .quest publish format): .quest packaging
// bundles included libraries into the .aslx itself, which is right for playback
// but would fork the game from its templates if re-imported for further editing.
export async function backupGame(): Promise<void> {
    if (!_bridge || !(_adapter instanceof LocalDraftAdapter)) return;
    const gameXml = _bridge.Save(); // Save() clears _isDirty in the bridge
    isDirty.set(false);
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

export async function undo() {
    if (_bridge) await _bridge.Undo();
    refreshTree();
    refreshSelectedData();
    refreshUndoRedo();
    scriptVersion.update(n => n + 1);
}

export function redo() {
    _bridge?.Redo();
    refreshTree();
    refreshSelectedData();
    refreshUndoRedo();
    scriptVersion.update(n => n + 1);
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

export function getIfExpressionTemplates(): IfExpressionTemplate[] | null {
    if (!_bridge) return null;
    try {
        return JSON.parse(_bridge.GetIfExpressionTemplates());
    } catch { return null; }
}

export function getIfExpressionTemplateData(expression: string): IfExpressionTemplateData | null {
    if (!_bridge) return null;
    try {
        const json = _bridge.GetIfExpressionTemplateData(expression);
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

function afterCreate(result: string): string {
    if (result.startsWith("error:")) return result;
    refreshTree();
    void selectNode(result);
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

export function createWalkthrough(name: string): string {
    if (!_bridge) return "error:not loaded";
    return afterCreate(_bridge.CreateWalkthrough(name, ""));
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

export function createIncludedLibrary(): string {
    if (!_bridge) return "error:not loaded";
    return afterCreate(_bridge.CreateIncludedLibrary());
}

export function createJavascript(): string {
    if (!_bridge) return "error:not loaded";
    return afterCreate(_bridge.CreateJavascript());
}

export function deleteElement(key: string) {
    if (!_bridge) return;
    _bridge.DeleteElement(key);
    selectedKey.set(null);
    selectedData.set(null);
    refreshTree();
    refreshUndoRedo();
}

export function swapElements(key1: string, key2: string): string {
    if (!_bridge) return "error";
    const result = _bridge.SwapElements(key1, key2);
    if (result === "ok") refreshTree();
    return result;
}
