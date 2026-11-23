// Publishing a game to textadventures.co.uk from the editor. The editor has no login there, so it
// uploads the .quest package with no credentials and is given a page on textadventures.co.uk for
// the author to open (the claim URL), where their own login finishes publishing it. The editor
// then asks what became of the upload, to say when it's published.
// The site's side is textadventures.co.uk's PublishApiController, and its docs/one-click-publish.md
// has the design. Squiffy's editor has the same module.
import { PUBLIC_APPSHELL_VERSION, PUBLIC_PUBLISH_API_URL } from "$app/env/public";
import { isElectron } from "./runtime";

// PUBLIC_PUBLISH_API_URL points a development build at a local copy of textadventures.co.uk
export const PUBLISH_API_URL: string = PUBLIC_PUBLISH_API_URL || "https://textadventures.co.uk/api/publish/uploads";

// The site's limit, checked here first so a game that's too big isn't uploaded to be refused
export const MAX_UPLOAD_BYTES = 50 * 1024 * 1024;

// Which editor is publishing, as for the home page's catalog (see home-catalog.ts)
function uploadUrl(filename: string): string {
    const url = new URL(PUBLISH_API_URL);
    url.searchParams.set("filename", filename);
    url.searchParams.set("source", isElectron() ? "electron" : "web");
    if (PUBLIC_APPSHELL_VERSION) url.searchParams.set("version", PUBLIC_APPSHELL_VERSION);
    const platform = window.electronApp?.platform;
    if (platform) url.searchParams.set("platform", platform);
    return url.toString();
}

export interface Upload {
    id: string;
    // The page on textadventures.co.uk where the author finishes publishing
    claimUrl: string;
}

const isHttpUrl = (value: unknown): value is string => {
    if (typeof value !== "string") return false;
    try {
        return ["https:", "http:"].includes(new URL(value).protocol);
    } catch {
        return false;
    }
};

// The site's answer to an upload, or null if it isn't as expected. The claim URL is opened, so it
// has to be a web page.
function parseUpload(data: unknown): Upload | null {
    const upload = data as { id?: unknown; claimUrl?: unknown } | null;
    if (!upload || typeof upload.id !== "string" || !upload.id || !isHttpUrl(upload.claimUrl)) return null;
    return { id: upload.id, claimUrl: upload.claimUrl };
}

// Why an upload failed: the site's HTTP status, or 0 if it couldn't be reached. The dialog has
// the words for each (publishModal.uploadError*).
export class UploadError extends Error {
    constructor(readonly status: number) {
        super(`Upload failed (${status})`);
    }
}

// Uploads the package. XMLHttpRequest, as fetch can't say how far an upload has got.
export function uploadGame(
    bytes: Uint8Array,
    filename: string,
    options: { onprogress?: (fraction: number) => void; signal?: AbortSignal } = {},
): Promise<Upload> {
    return new Promise((resolve, reject) => {
        if (bytes.byteLength > MAX_UPLOAD_BYTES) {
            reject(new UploadError(413));
            return;
        }
        const request = new XMLHttpRequest();
        request.open("POST", uploadUrl(filename));
        request.setRequestHeader("Content-Type", "application/octet-stream");
        request.responseType = "json";
        request.upload.onprogress = e => {
            if (e.lengthComputable) options.onprogress?.(e.loaded / e.total);
        };
        request.onload = () => {
            const upload = request.status === 201 ? parseUpload(request.response) : null;
            if (upload) resolve(upload);
            else reject(new UploadError(request.status));
        };
        request.onerror = () => reject(new UploadError(0));
        request.onabort = () => reject(new DOMException("The upload was cancelled", "AbortError"));
        options.signal?.addEventListener("abort", () => request.abort());
        request.send(bytes as XMLHttpRequestBodyInit);
    });
}

export type PublishStatus =
    // Waiting for the author to finish on textadventures.co.uk
    | { status: "pending" }
    | { status: "published"; gameUrl?: string }
    // The upload has expired (after an hour), so it would need publishing again
    | { status: "gone" };

// What became of an upload, or null if the site can't say just now
export async function checkStatus(id: string): Promise<PublishStatus | null> {
    try {
        const response = await fetch(`${PUBLISH_API_URL.replace(/\/$/, "")}/${encodeURIComponent(id)}`, { cache: "no-store" });
        if (response.status === 404) return { status: "gone" };
        if (response.status !== 200) return null;
        const answer = await response.json() as { status?: unknown; gameUrl?: unknown } | null;
        if (answer?.status === "pending") return { status: "pending" };
        if (answer?.status === "published") return { status: "published", gameUrl: isHttpUrl(answer.gameUrl) ? answer.gameUrl : undefined };
        return null;
    } catch {
        return null;
    }
}
