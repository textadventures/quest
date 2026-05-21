import { writable, get } from "svelte/store";
import { loadWasm } from "./wasm";
import type { WasmBridge } from "./wasm";
import type { TreeNode, EditorDataResponse, ScriptBlockData, ScriptCommandCategoriesData, IfExpressionTemplateData, IfExpressionTemplate } from "./types";

export type AddElementModalState = { type: "room" | "object" | "function" | "timer"; parent: string | null } | null;

let _bridge: WasmBridge | null = null;

export const isLoaded = writable(false);
export const loadingStatus = writable<string | null>(null);
export const addElementModal = writable<AddElementModalState>(null);

export function openAddModal(type: "room" | "object" | "function" | "timer", parent: string | null) {
    addElementModal.set({ type, parent });
}
export const gameFilename = writable<string | null>(null);
export const treeNodes = writable<TreeNode[]>([]);
export const selectedKey = writable<string | null>(null);
export const selectedData = writable<EditorDataResponse | null>(null);
export const canUndo = writable(false);
export const canRedo = writable(false);
export const scriptVersion = writable(0);
export const scriptClipboardHasContent = writable(false);

function refreshUndoRedo() {
    canUndo.set(_bridge?.CanUndo() ?? false);
    canRedo.set(_bridge?.CanRedo() ?? false);
}

export async function openGame(file: File): Promise<boolean> {
    loadingStatus.set("Starting editor…");
    const bytes = new Uint8Array(await file.arrayBuffer());
    _bridge = await loadWasm();
    loadingStatus.set("Loading game…");
    // Double rAF ensures the browser actually paints the status update before
    // Initialise blocks the JS thread (C# WASM calls are synchronous).
    await new Promise<void>(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
    const ok = await _bridge.Initialise(bytes, file.name);
    loadingStatus.set(null);
    if (ok) {
        const nodes: TreeNode[] = JSON.parse(_bridge.GetTreeNodes());
        treeNodes.set(nodes);
        isLoaded.set(true);
        gameFilename.set(file.name);
        refreshUndoRedo();
        scriptClipboardHasContent.set(false);
        const gameNode = nodes.find(n => n.nodeType === "game");
        if (gameNode) selectNode(gameNode.key);
    }
    return ok;
}

export function selectNode(key: string) {
    if (!_bridge) return;
    selectedKey.set(key);
    const json = _bridge.GetEditorData(key);
    selectedData.set(json ? JSON.parse(json) : null);
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

export function setDropdownType(elementKey: string, controlId: string, selectedType: string): string {
    if (!_bridge) return "error";
    const result = _bridge.SetDropdownType(elementKey, controlId, selectedType);
    refreshSelectedData();
    refreshUndoRedo();
    return result;
}

export function saveGame(): string {
    return _bridge?.Save() ?? "";
}

export function undo() {
    _bridge?.Undo();
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

export function getScriptCommandCategories(): ScriptCommandCategoriesData | null {
    if (!_bridge) return null;
    const json = _bridge.GetScriptCommandCategories();
    return json ? JSON.parse(json) : null;
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
    const key = get(selectedKey);
    if (!_bridge || !key) return;
    const json = _bridge.GetEditorData(key);
    selectedData.set(json ? JSON.parse(json) : null);
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
    selectNode(result);
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

export function deleteElement(key: string) {
    if (!_bridge) return;
    _bridge.DeleteElement(key);
    selectedKey.set(null);
    selectedData.set(null);
    refreshTree();
    refreshUndoRedo();
}
