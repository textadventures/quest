// Mirrors PlayerHelper.cs's MimeTypes - keep the two in step.
const MIME_TYPES: Record<string, string> = {
    ".jpg": "image/jpeg",
    ".jpeg": "image/jpeg",
    ".gif": "image/gif",
    ".bmp": "image/bmp",
    ".png": "image/png",
    ".wav": "audio/wav",
    ".mp3": "audio/mpeg",
    ".ogg": "audio/ogg",
    ".js": "application/javascript",
    ".ttf": "application/font-woff",
    ".svg": "image/svg+xml",
    ".css": "text/css",
    ".json": "application/json",
    ".txt": "text/plain",
    ".woff": "font/woff",
    ".woff2": "font/woff2",
};

export function guessMimeType(name: string): string {
    const dot = name.lastIndexOf(".");
    if (dot < 0) return "";
    return MIME_TYPES[name.slice(dot).toLowerCase()] ?? "";
}

// A game asset handed to the player as a data: URL needs a real type. An untyped Blob - from
// Electron's readFile, or a browser that doesn't know the extension - becomes
// data:application/octet-stream, which <audio> rejects with NotSupportedError and a
// <link rel="stylesheet"> refuses to apply.
export function withGuessedMimeType(blob: Blob, name: string): Blob {
    if (blob.type && blob.type !== "application/octet-stream") return blob;
    const type = guessMimeType(name);
    return type ? new Blob([blob], { type }) : blob;
}
