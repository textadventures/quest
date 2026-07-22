import { writable } from "svelte/store";

export interface ConfirmOptions {
    confirmLabel?: string;
    cancelLabel?: string;
    danger?: boolean;
}

interface ConfirmState extends Required<ConfirmOptions> {
    message: string;
    resolve: (result: boolean) => void;
}

export const confirmState = writable<ConfirmState | null>(null);

// Promise-based stand-in for window.confirm() — resolves true/false the same way, but renders
// through ConfirmDialog.svelte (mounted once in +layout.svelte) so it looks like the rest of the
// app instead of the browser's native dialog chrome.
export function confirmDialog(message: string, options?: ConfirmOptions): Promise<boolean> {
    return new Promise(resolve => {
        confirmState.set({
            message,
            confirmLabel: options?.confirmLabel ?? "OK",
            cancelLabel: options?.cancelLabel ?? "Cancel",
            danger: options?.danger ?? false,
            resolve,
        });
    });
}
