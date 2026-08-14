// Safari doesn't support FileSystemFileHandle.createWritable() on the main thread —
// only the synchronous createSyncAccessHandle(), which is worker-only. This worker
// exists purely so opfs-writer.ts has somewhere to route writes on browsers like that.

interface WriteMsg {
    id: number;
    path: string[];
    filename: string;
    data: ArrayBuffer;
}

// createSyncAccessHandle is worker-only (Safari's only OPFS write API) and not
// yet in the TS DOM lib for this project's target.
interface SyncAccessHandle {
    truncate(size: number): void;
    write(buffer: Uint8Array, options?: { at?: number }): number;
    flush(): void;
    close(): void;
}
interface FileHandleWithSyncAccess extends FileSystemFileHandle {
    createSyncAccessHandle(): Promise<SyncAccessHandle>;
}

async function resolveDir(path: string[]): Promise<FileSystemDirectoryHandle> {
    let dir: FileSystemDirectoryHandle = await navigator.storage.getDirectory();
    for (const segment of path) {
        dir = await dir.getDirectoryHandle(segment, { create: true });
    }
    return dir;
}

self.onmessage = async (e: MessageEvent<WriteMsg>) => {
    const { id, path, filename, data } = e.data;
    try {
        const dir = await resolveDir(path);
        const fh = await dir.getFileHandle(filename, { create: true });
        const handle = await (fh as FileHandleWithSyncAccess).createSyncAccessHandle();
        try {
            handle.truncate(0);
            handle.write(new Uint8Array(data), { at: 0 });
            handle.flush();
        } finally {
            handle.close();
        }
        self.postMessage({ id, ok: true });
    } catch (err) {
        self.postMessage({ id, ok: false, error: String(err) });
    }
};
