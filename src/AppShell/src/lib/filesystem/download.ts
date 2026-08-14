export function toBlob(data: Uint8Array | string): Blob {
    return typeof data === "string"
        ? new Blob([data], { type: "application/xml" })
        : new Blob([data.slice()]);
}

export function triggerDownload(data: Uint8Array | string, filename: string): void {
    const url = URL.createObjectURL(toBlob(data));
    const a = document.createElement("a");
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
}
