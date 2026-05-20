import { writable, get } from "svelte/store";
import { loadWasm } from "./wasm";
import type { WasmBridge } from "./wasm";
import type { TreeNode, EditorDataResponse, ScriptBlockData, ScriptCommandCategoriesData } from "./types";

let _bridge: WasmBridge | null = null;

export const isLoaded = writable(false);
export const gameFilename = writable<string | null>(null);
export const treeNodes = writable<TreeNode[]>([]);
export const selectedKey = writable<string | null>(null);
export const selectedData = writable<EditorDataResponse | null>(null);
export const canUndo = writable(false);
export const canRedo = writable(false);
export const scriptVersion = writable(0);

function refreshUndoRedo() {
    canUndo.set(_bridge?.CanUndo() ?? false);
    canRedo.set(_bridge?.CanRedo() ?? false);
}

export async function openGame(file: File): Promise<boolean> {
    const bytes = new Uint8Array(await file.arrayBuffer());
    _bridge = await loadWasm();
    const ok = await _bridge.Initialise(bytes, file.name);
    if (ok) {
        treeNodes.set(JSON.parse(_bridge.GetTreeNodes()));
        isLoaded.set(true);
        gameFilename.set(file.name);
        refreshUndoRedo();
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
    refreshSelectedData();
    refreshUndoRedo();
    scriptVersion.update(n => n + 1);
}

export function redo() {
    _bridge?.Redo();
    refreshSelectedData();
    refreshUndoRedo();
    scriptVersion.update(n => n + 1);
}

// ── Script editor functions ─────────────────────────────────────────────────

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

function refreshSelectedData() {
    const key = get(selectedKey);
    if (!_bridge || !key) return;
    const json = _bridge.GetEditorData(key);
    selectedData.set(json ? JSON.parse(json) : null);
}
