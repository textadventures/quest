// Rewrites the static HTML for a game link as a stream. The API calls start
// right away, but the page isn't held back for them: everything up to the
// first point that needs their results (including the player's modulepreload
// of dotnet.js and its <head> scripts) reaches the browser immediately, so
// those downloads overlap the API round trips. Every failure path (unknown id,
// API down or slow) leaves the page as it would be without these Functions.

import { fetchGameLocation, type GameLocation, isLinkPreviewBot, playerVersion, preloadedGameHtml } from "./game-location";
import { escapeHtml, isHtml } from "./html";
import { fetchGameDetails, gameTitle, metaTags } from "./onebox";

export interface GamePageOptions {
    gameId: string | null;
    preloadGame?: boolean;
    rewriteGameUrl?: (url: string) => string;
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
    { gameId, preloadGame = false, rewriteGameUrl }: GamePageOptions,
): Promise<Response> {
    if (!gameId || request.method !== "GET") return next();

    const detailsPromise = fetchGameDetails(gameId);
    let page = await next();
    if (page.status !== 200 || !isHtml(page)) return page;

    const html = await page.text();
    page = new Response(inChunks(html), page);

    let locationPromise: Promise<GameLocation | null> = Promise.resolve(null);
    if (preloadGame && !isLinkPreviewBot(request)) {
        locationPromise = fetchGameLocation(gameId, playerVersion(html)).then(location =>
            location && rewriteGameUrl
                ? {
                    sourceGameUrl: rewriteGameUrl(location.sourceGameUrl),
                    resourceRoot: location.resourceRoot && rewriteGameUrl(location.resourceRoot),
                }
                : location);
    }

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
        .on('script[src^="wasm-player.js"]', {
            async element(script) {
                const location = await locationPromise;
                if (location) script.before(preloadedGameHtml(gameId, location), { html: true });
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
