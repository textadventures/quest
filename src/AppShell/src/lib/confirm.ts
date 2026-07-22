import { writable } from "svelte/store";

export interface DialogChoice<T> {
    label: string;
    value: T;
    danger?: boolean;
}

interface DialogState {
    message: string;
    choices: DialogChoice<unknown>[];
    resolve: (result: unknown) => void;
}

export const dialogState = writable<DialogState | null>(null);

// General N-choice prompt rendered through ConfirmDialog.svelte (mounted once in +layout.svelte).
// Resolves with the chosen value, or null if dismissed via Escape/backdrop click without choosing.
// The store itself is untyped (a single instance is shared across every call site, each with its
// own T) — type safety lives at the call site via this function's generic parameter.
export function chooseDialog<T>(message: string, choices: DialogChoice<T>[]): Promise<T | null> {
    return new Promise(resolve => {
        dialogState.set({
            message,
            choices: choices as DialogChoice<unknown>[],
            resolve: resolve as (result: unknown) => void,
        });
    });
}

export interface ConfirmOptions {
    confirmLabel?: string;
    cancelLabel?: string;
    danger?: boolean;
}

// Two-choice convenience wrapper — promise-based stand-in for window.confirm(), resolving
// true/false the same way (Escape/backdrop dismissal counts as false, i.e. "cancel").
export async function confirmDialog(message: string, options?: ConfirmOptions): Promise<boolean> {
    const result = await chooseDialog(message, [
        { label: options?.cancelLabel ?? "Cancel", value: false },
        { label: options?.confirmLabel ?? "OK", value: true, danger: options?.danger },
    ]);
    return result ?? false;
}
