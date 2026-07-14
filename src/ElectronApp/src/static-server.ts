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

    return new Promise((resolve, reject) => {
        server.once("error", reject);
        server.listen(0, "127.0.0.1", () => {
            const address = server.address();
            if (!address || typeof address === "string") {
                reject(new Error("Failed to determine static server port"));
                return;
            }
            resolve({
                port: address.port,
                close: () => new Promise((res) => server.close(() => res())),
            });
        });
    });
}
