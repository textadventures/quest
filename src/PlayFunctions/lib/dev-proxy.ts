// Local `wrangler pages dev` only (enabled by `--binding DEV_PROXY=1`, which
// the deployed Pages projects never set). Game files on playtextadventures.com
// only send CORS headers for allowlisted origins like play.questviva.com, so on
// localhost the player's API call and game fetch are routed through these
// same-origin proxies instead — the same approach as WasmPlayer's dev-server.mjs.

import { isHtml } from "./html";

export interface DevProxyEnv {
    DEV_PROXY?: string;
}

const API_ORIGIN = "https://textadventures.co.uk";
const GAME_RESOURCE_PREFIX = "/game-resource/";

function toGameResourceUrl(url: string): string {
    return GAME_RESOURCE_PREFIX + encodeURIComponent(url);
}

export async function proxyApi(request: Request): Promise<Response> {
    const url = new URL(request.url);
    const upstream = await fetch(`${API_ORIGIN}${url.pathname}${url.search}`);
    const contentType = upstream.headers.get("content-type") ?? "application/json";
    if (!upstream.ok || !contentType.includes("application/json")) {
        return new Response(upstream.body, { status: upstream.status, headers: { "content-type": contentType } });
    }
    const json = await upstream.json() as Record<string, unknown>;
    for (const key of ["sourceGameUrl", "resourceRoot"]) {
        if (typeof json[key] === "string") json[key] = toGameResourceUrl(json[key]);
    }
    return Response.json(json);
}

// The player appends resource filenames to the encoded resourceRoot, so the
// whole remaining path is decoded as one URL.
export async function proxyGameResource(request: Request): Promise<Response> {
    const { pathname } = new URL(request.url);
    const target = decodeURIComponent(pathname.slice(GAME_RESOURCE_PREFIX.length));
    if (!/^https:\/\//.test(target)) return new Response("Bad game resource URL", { status: 400 });
    const upstream = await fetch(target);
    return new Response(upstream.body, {
        status: upstream.status,
        headers: { "content-type": upstream.headers.get("content-type") ?? "application/octet-stream" },
    });
}

// Local wrangler compresses responses with non-streaming gzip/Brotli, which
// buffers the whole body and hides game-page.ts's streaming (preloads reaching
// the browser before the API calls finish). Deployed Workers are compressed by
// Cloudflare's front line with streaming-capable compression instead.
// https://github.com/cloudflare/workers-sdk/issues/8004
// https://github.com/cloudflare/workers-sdk/issues/6577
// Fix (unmerged): https://github.com/cloudflare/workers-sdk/pull/9625
export function withoutDevCompression(response: Response): Response {
    if (!isHtml(response)) return response;
    const result = new Response(response.body, response);
    result.headers.set("content-encoding", "identity");
    return result;
}

// Runs after quest-config.js, pointing the player's API root at proxyApi.
export function withDevApiRoot(response: Response): Response {
    if (!isHtml(response)) return response;
    return new HTMLRewriter()
        .on('script[src^="quest-config.js"]', {
            element(script) {
                script.after("<script>window.QuestVivaConfig.textAdventuresApiRoot = '/api/';</script>", { html: true });
            },
        })
        .transform(response);
}
