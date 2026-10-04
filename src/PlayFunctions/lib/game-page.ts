// Rewrites the static HTML for a game link as a stream. The API call starts
// right away, but the page isn't held back for it: everything up to the first
// point that needs its result (including the player's modulepreload of
// dotnet.js and its <head> scripts) reaches the browser immediately, so those
// downloads overlap the API round trip. Every failure path (unknown id, API
// down or slow) leaves the page as it would be without these Functions.

import { escapeHtml, isHtml } from "./html";
import { fetchGameDetails, gameTitle, metaTags } from "./onebox";

export interface GamePageOptions {
    gameId: string | null;
}

// HTMLRewriter only emits its output once it has finished the input chunk it's
// working on, so a whole page in one chunk would hold everything up behind the
// first async handler. Small chunks let the markup ahead of it flush first.
const CHUNK_SIZE = 1024;

function inChunks(html: string): ReadableStream<Uint8Array> {
    const bytes = new TextEncoder().encode(html);
    let offset = 0;
    return new ReadableStream({
        pull(controller) {
            if (offset >= bytes.length) {
                controller.close();
                return;
            }
            controller.enqueue(bytes.subarray(offset, offset + CHUNK_SIZE));
            offset += CHUNK_SIZE;
        },
    });
}

export async function rewriteGamePage(
    request: Request,
    next: () => Promise<Response>,
    { gameId }: GamePageOptions,
): Promise<Response> {
    if (!gameId || request.method !== "GET") return next();

    const detailsPromise = fetchGameDetails(gameId);
    let page = await next();
    if (page.status !== 200 || !isHtml(page)) return page;

    const html = await page.text();
    page = new Response(inChunks(html), page);

    // <title> comes before the preloads, so it can't wait for the game's name.
    // It's moved to the end of <head> instead.
    let staticTitle = "";
    const rewritten = new HTMLRewriter()
        .on("head > title", {
            text(text) {
                staticTitle += text.text;
            },
            element(title) {
                title.remove();
            },
        })
        .on("head", {
            element(head) {
                head.onEndTag(async (end) => {
                    const details = await detailsPromise;
                    const title = details ? escapeHtml(gameTitle(details)) : staticTitle;
                    const meta = details ? metaTags(details, request.url) : "\n";
                    end.before(`    <title>${title}</title>${meta}`, { html: true });
                });
            },
        })
        .transform(page);

    // The static asset's ETag is shared by every game id, so it can't validate
    // this per-game body.
    const result = new Response(rewritten.body, rewritten);
    result.headers.delete("etag");
    result.headers.delete("content-length");
    return result;
}
