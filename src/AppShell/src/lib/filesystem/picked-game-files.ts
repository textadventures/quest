// An .aslx game can <include ref="..."/> a library from its own folder, and a picked or dropped
// File brings no folder with it, so the browser build lets the player choose the game together
// with its libraries. Of the files chosen, the game is the .quest/.asl/.cas, or else the .aslx
// whose root element is <asl> (a library's is <library>); every other .aslx goes to the player
// as an adjacent file for WorldModel.GetLibraryStream to find. WasmPlayer's own start-screen
// picker does the same (wasm-player.js's splitPickedGameFiles).

export interface PickedGame {
    file: File;
    bytes: Uint8Array;
    adjacentFiles: Record<string, Uint8Array>;
}

const PACKAGE_EXTENSION = /\.(quest|asl|cas)$/i;
const ASLX_EXTENSION = /\.aslx$/i;

// The name of an XML file's root element, skipping any XML declaration and comments (TextDecoder drops a BOM).
export function rootElementName(bytes: Uint8Array): string | null {
    const text = new TextDecoder().decode(bytes.subarray(0, 4096));
    return /^\s*(?:<\?[\s\S]*?\?>\s*|<!--[\s\S]*?-->\s*)*<([A-Za-z_][\w.-]*)/.exec(text)?.[1] ?? null;
}

// The game among the picked files, or null when there isn't one. A single file is always taken
// as the game, so the caller can still say what's wrong with it.
export async function splitPickedGameFiles(files: File[]): Promise<PickedGame | null> {
    const read = await Promise.all(files.map(async (file) => ({ file, bytes: new Uint8Array(await file.arrayBuffer()) })));
    const isAslx = (f: { file: File }) => ASLX_EXTENSION.test(f.file.name);
    const game = read.find((f) => PACKAGE_EXTENSION.test(f.file.name))
        ?? read.find((f) => isAslx(f) && rootElementName(f.bytes) === "asl")
        ?? (read.length === 1 ? read[0] : null);
    if (!game) return null;
    const adjacentFiles: Record<string, Uint8Array> = {};
    for (const f of read) {
        if (f !== game && isAslx(f)) adjacentFiles[f.file.name] = f.bytes;
    }
    return { file: game.file, bytes: game.bytes, adjacentFiles };
}
