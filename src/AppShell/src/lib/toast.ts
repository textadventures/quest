import { writable } from "svelte/store";

export interface ToastMessage {
    id: number;
    text: string;
    kind: "error" | "info";
}

export const toasts = writable<ToastMessage[]>([]);

let nextId = 0;

// Fire-and-forget notification rendered through Toast.svelte (mounted once in +layout.svelte,
// so it survives whatever tab/element/route is currently active — unlike an inline error tied
// to a specific control, which disappears the moment the user navigates away from it).
export function showToast(text: string, kind: ToastMessage["kind"] = "error", durationMs = 6000): void {
    const id = ++nextId;
    toasts.update(list => [...list, { id, text, kind }]);
    setTimeout(() => {
        toasts.update(list => list.filter(t => t.id !== id));
    }, durationMs);
}

export function dismissToast(id: number): void {
    toasts.update(list => list.filter(t => t.id !== id));
}
