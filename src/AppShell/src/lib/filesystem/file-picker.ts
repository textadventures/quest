// Generic <input type=file> wrapper — not FSA-specific, just reads whatever
// bytes the user picks. Callers decide what to do with the resulting File.
export function pickFile(accept: string): Promise<File | null> {
    return new Promise((resolve) => {
        const input = document.createElement("input");
        input.type = "file";
        input.accept = accept;
        input.addEventListener("change", () => {
            resolve(input.files?.[0] ?? null);
        }, { once: true });
        input.addEventListener("cancel", () => resolve(null), { once: true });
        input.click();
    });
}
