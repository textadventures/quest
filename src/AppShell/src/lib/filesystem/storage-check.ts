// Whether the browser lets the editor use the origin-private file system (OPFS), where local
// drafts are kept (local-adapter.ts). Strict privacy settings (e.g. Firefox blocking all cookies),
// private windows and privacy add-ons can block it: navigator.storage.getDirectory() then throws
// (often a SecurityError), or navigator.storage isn't there at all. Checked once by the Create tab,
// so it can say so plainly instead of every draft action failing with the browser's own message.
// Copied from Squiffy's app/src/lib/storage.ts (textadventures/squiffy#362).

export type StorageCheck = { ok: true } | { ok: false; detail: string };

type StorageLike = { getDirectory?: () => Promise<unknown> } | undefined;

export async function checkBrowserStorage(
    storage: StorageLike = typeof navigator !== "undefined" ? navigator.storage : undefined,
): Promise<StorageCheck> {
    if (typeof storage?.getDirectory !== "function") {
        return { ok: false, detail: "This browser doesn't provide the origin-private file system." };
    }
    try {
        await storage.getDirectory();
        return { ok: true };
    } catch (err) {
        const detail = err instanceof Error ? `${err.name}: ${err.message}` : String(err);
        return { ok: false, detail };
    }
}
