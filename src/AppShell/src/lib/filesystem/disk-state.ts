// What a FileAdapter found when it compared its game file with what it last read or wrote
// (FileAdapter.checkDisk). Something outside the editor, such as an AI agent or a text editor,
// can change or delete the file while it's open, and the editor must not save over that.
export type DiskState = "unchanged" | "changed" | "missing";

// The bytes a save writes. FileAdapter.saveFile() takes either form, and both adapters that
// track the disk write a string as UTF-8, which is what this gives.
export function toBytes(data: Uint8Array | string): Uint8Array {
    return typeof data === "string" ? new TextEncoder().encode(data) : data;
}

export function sameBytes(a: Uint8Array, b: Uint8Array): boolean {
    if (a.length !== b.length) return false;
    for (let i = 0; i < a.length; i++) {
        if (a[i] !== b[i]) return false;
    }
    return true;
}
