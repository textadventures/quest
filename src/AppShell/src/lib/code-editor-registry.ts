import { writable } from "svelte/store";
import type { EditorView } from "@codemirror/view";
import { undo as cmUndoCommand, redo as cmRedoCommand } from "@codemirror/commands";

// Only one CodeEditor (per-script "Code view" or the whole-file raw XML panel) is ever mounted at
// a time in this app, so a single module-level slot is enough to let the toolbar's Undo/Redo
// buttons reach whichever CodeMirror instance is currently active. Without this, those buttons
// would keep operating on Quest's own undo/redo stack while the author is mid-edit in a
// not-yet-applied text buffer — silently mutating the underlying model in the background instead
// of undoing the text they're actually looking at.
let activeView: EditorView | null = null;

export const hasActiveCmView = writable(false);

export function registerActiveCmView(view: EditorView | null): void {
    activeView = view;
    hasActiveCmView.set(view !== null);
}

export function cmUndo(): boolean {
    return activeView ? cmUndoCommand(activeView) : false;
}

export function cmRedo(): boolean {
    return activeView ? cmRedoCommand(activeView) : false;
}
