import { writable } from "svelte/store";
import { loadWasm } from "./wasm";
import type { WasmBridge } from "./wasm";
import type { TreeNode, ElementAttributes } from "./types";

let _bridge: WasmBridge | null = null;

export const isLoaded = writable(false);
export const gameFilename = writable<string | null>(null);
export const treeNodes = writable<TreeNode[]>([]);
export const selectedKey = writable<string | null>(null);
export const selectedAttributes = writable<ElementAttributes | null>(null);

export async function openGame(file: File): Promise<boolean> {
  const bytes = new Uint8Array(await file.arrayBuffer());
  _bridge = await loadWasm();
  const ok = await _bridge.Initialise(bytes, file.name);
  if (ok) {
    treeNodes.set(JSON.parse(_bridge.GetTreeNodes()));
    isLoaded.set(true);
    gameFilename.set(file.name);
  }
  return ok;
}

export function selectNode(key: string) {
  if (!_bridge) return;
  selectedKey.set(key);
  const json = _bridge.GetEditorData(key);
  selectedAttributes.set(json ? JSON.parse(json) : null);
}

export function saveGame(): string {
  return _bridge?.Save() ?? "";
}

export function undo() {
  _bridge?.Undo();
}

export function redo() {
  _bridge?.Redo();
}
