import http from "node:http";
import path from "node:path";
import { readFile, stat } from "node:fs/promises";

// Mirrors src/WasmPlayer/dev-server.mjs's mime map — same static-asset serving
// need, just for the three directories deploy-play.yml already produces
// (editor/, AppBundle/, player/) instead of nginx.
const MIME_TYPES: Record<string, string> = {
    ".html": "text/html",
    ".js": "application/javascript",
    ".mjs": "application/javascript",
    ".wasm": "application/wasm",
    ".json": "application/json",
    ".css": "text/css",
    ".png": "image/png",
    ".svg": "image/svg+xml",
    ".dat": "application/octet-stream",
    ".blat": "application/octet-stream",
    ".dll": "application/octet-stream",
    ".pdb": "application/octet-stream",
    ".aslx": "application/xml",
};

export interface StaticServerRoots {
    editor: string;
    appBundle: string;
    player: string;
}

export interface StaticServerHandle {
    port: number;
    close(): Promise<void>;
}

// spaFallback serves index.html for any path that doesn't match a real file —
// needed for the editor root (SvelteKit adapter-static's client-side routing),
// not for AppBundle/player which are plain asset trees.
async function resolveFile(root: string, urlPath: string, spaFallback: boolean): Promise<string | null> {
    const normalizedRoot = path.normalize(root);
    const relative = urlPath === "/" || urlPath === "" ? "/index.html" : urlPath;
    const filePath = path.normalize(path.join(normalizedRoot, decodeURIComponent(relative)));
    if (filePath !== normalizedRoot && !filePath.startsWith(normalizedRoot + path.sep)) return null;

    try {
        const s = await stat(filePath);
        if (s.isFile()) return filePath;
    } catch {
        // fall through to SPA fallback below
    }

    if (spaFallback) {
        const indexPath = path.join(normalizedRoot, "index.html");
        try {
            await stat(indexPath);
            return indexPath;
        } catch {
            return null;
        }
    }
    return null;
}

// Tried first, before falling back to an OS-assigned ephemeral port. Chosen
// arbitrarily (high, unregistered, not used elsewhere in this repo) — it
// only needs to be *a* stable port, not any particular one.
//
// Why this matters: this server's origin (http://127.0.0.1:{port}) is what
// localStorage/IndexedDB partition by. A random port every launch means a
// fresh, empty storage partition every launch too — WasmPlayer's IndexedDB
// game saves and the "script on" transcript feature (localStorage) would
// both silently lose all data the moment the app restarts, since the
// previous launch's data is still on disk but under an origin nothing will
// ever request again. Keeping the port stable across launches keeps the
// origin (and thus that data) stable too. Falling back to an ephemeral port
// on conflict trades that stability away only in the rare case something
// else on the machine already holds this exact port — same behavior as
// before this fallback existed.
const PREFERRED_PORT = 47893;

export function startStaticServer(roots: StaticServerRoots): Promise<StaticServerHandle> {
    const server = http.createServer((req, res) => {
        void (async () => {
            const urlPath = (req.url ?? "/").split("?")[0] ?? "/";
            try {
                let filePath: string | null;
                if (urlPath === "/AppBundle" || urlPath.startsWith("/AppBundle/")) {
                    filePath = await resolveFile(roots.appBundle, urlPath.slice("/AppBundle".length), false);
                } else if (urlPath === "/player" || urlPath.startsWith("/player/")) {
                    filePath = await resolveFile(roots.player, urlPath.slice("/player".length), false);
                } else {
                    filePath = await resolveFile(roots.editor, urlPath, true);
                }

                if (!filePath) {
                    res.writeHead(404);
                    res.end("Not found");
                    return;
                }

                const data = await readFile(filePath);
                res.writeHead(200, { "Content-Type": MIME_TYPES[path.extname(filePath)] ?? "application/octet-stream" });
                res.end(data);
            } catch (err) {
                res.writeHead(500);
                res.end(String(err));
            }
        })();
    });

    function listen(port: number): Promise<StaticServerHandle> {
        return new Promise((resolve, reject) => {
            const onError = (err: NodeJS.ErrnoException) => {
                server.removeListener("listening", onListening);
                reject(err);
            };
            const onListening = () => {
                server.removeListener("error", onError);
                const address = server.address();
                if (!address || typeof address === "string") {
                    reject(new Error("Failed to determine static server port"));
                    return;
                }
                resolve({
                    port: address.port,
                    close: () => new Promise((res) => server.close(() => res())),
                });
            };
            server.once("error", onError);
            server.once("listening", onListening);
            server.listen(port, "127.0.0.1");
        });
    }

    return listen(PREFERRED_PORT).catch((err: NodeJS.ErrnoException) => {
        if (err.code !== "EADDRINUSE") throw err;
        return listen(0);
    });
}
