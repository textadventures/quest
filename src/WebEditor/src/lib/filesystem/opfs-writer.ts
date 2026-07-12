// OPFS read operations (getFileHandle, directory enumeration, removeEntry) work on
// the main thread in every browser that ships OPFS. Only the writable-stream API
// (FileSystemFileHandle.createWritable) is missing in Safari, so writes alone are
// routed through a worker there — see opfs-writer.worker.ts.

export async function resolveOpfsDir(path: string[], create: boolean): Promise<FileSystemDirectoryHandle | null> {
    let dir: FileSystemDirectoryHandle = await navigator.storage.getDirectory();
    try {
        for (const segment of path) {
            dir = await dir.getDirectoryHandle(segment, { create });
        }
        return dir;
    } catch {
        return null;
    }
}

export async function removeOpfsFile(path: string[], filename: string): Promise<void> {
    const dir = await resolveOpfsDir(path, false);
    if (!dir) return;
    await dir.removeEntry(filename).catch(() => {});
}

export async function removeOpfsDir(path: string[]): Promise<void> {
    if (path.length === 0) return;
    const parent = await resolveOpfsDir(path.slice(0, -1), false);
    if (!parent) return;
    await parent.removeEntry(path[path.length - 1], { recursive: true }).catch(() => {});
}

export async function listOpfsDirs(path: string[]): Promise<string[]> {
    const dir = await resolveOpfsDir(path, false);
    if (!dir) return [];
    const names: string[] = [];
    for await (const [name, handle] of dir as unknown as AsyncIterable<[string, FileSystemHandle]>) {
        if (handle.kind === "directory") names.push(name);
    }
    return names;
}

function supportsMainThreadWrite(): boolean {
    return typeof FileSystemFileHandle !== "undefined" && "createWritable" in FileSystemFileHandle.prototype;
}

async function writeMainThread(path: string[], filename: string, data: Uint8Array): Promise<void> {
    const dir = await resolveOpfsDir(path, true);
    if (!dir) throw new Error(`Could not open OPFS directory: ${path.join("/")}`);
    const fh = await dir.getFileHandle(filename, { create: true });
    const writable = await fh.createWritable();
    await writable.write(data.slice());
    await writable.close();
}

let _worker: Worker | null = null;
let _nextId = 1;
const _pending = new Map<number, { resolve: () => void; reject: (err: Error) => void }>();

function getWorker(): Worker {
    if (_worker) return _worker;
    const worker = new Worker(new URL("./opfs-writer.worker.ts", import.meta.url), { type: "module" });
    worker.onmessage = (e: MessageEvent<{ id: number; ok: boolean; error?: string }>) => {
        const { id, ok, error } = e.data;
        const p = _pending.get(id);
        if (!p) return;
        _pending.delete(id);
        if (ok) p.resolve();
        else p.reject(new Error(error));
    };
    _worker = worker;
    return worker;
}

function writeViaWorker(path: string[], filename: string, data: Uint8Array): Promise<void> {
    return new Promise((resolve, reject) => {
        const id = _nextId++;
        _pending.set(id, { resolve, reject });
        // Copy so the transferred buffer doesn't detach memory the caller still holds.
        const copy = data.slice();
        getWorker().postMessage({ id, path, filename, data: copy.buffer }, [copy.buffer]);
    });
}

// Writes bytes to <opfs-root>/<path...>/<filename>, creating intermediate
// directories as needed. Picks the write backend once per session based on
// what the browser actually supports.
export function writeOpfsFile(path: string[], filename: string, data: Uint8Array): Promise<void> {
    return supportsMainThreadWrite()
        ? writeMainThread(path, filename, data)
        : writeViaWorker(path, filename, data);
}
