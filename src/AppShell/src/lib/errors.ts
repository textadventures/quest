// What the error page shows for an exception the app didn't catch, in place of SvelteKit's own
// "500 Internal Error", which says nothing about what went wrong (hooks.client.ts).
export function describeError(error: unknown): { message: string; detail?: string } {
    if (error instanceof Error) {
        const message = error.message || error.name;
        // A stack usually starts with "Name: message", which the page shows already
        const stack = error.stack?.trim();
        return { message, detail: stack && stack !== message ? stack : undefined };
    }
    if (typeof error === "string" && error) return { message: error };
    let text: string;
    try {
        text = JSON.stringify(error) ?? String(error);
    } catch {
        text = String(error);
    }
    return { message: text || "Unknown error" };
}
