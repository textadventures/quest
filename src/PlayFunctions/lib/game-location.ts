// Inlines the textadventures.co.uk `api/game/<id>` response into /player/?id=,
// so wasm-player.js can start fetching the game file as soon as it runs instead
// of waiting on its own API round trip first (textadventures/quest#2194), and
// preloads the game file itself so that download starts while the page parses.

import { escapeAttribute } from "./html";

const API_ROOT = "https://textadventures.co.uk/api";
const API_TIMEOUT_MS = 1500;

export interface GameLocation {
    sourceGameUrl: string;
    resourceRoot: string | null;
}

// Deliberately not edge-cached, and sent with the same analytics params as
// wasm-player.js's own clientInfoParams(), in case the endpoint counts plays.
export async function fetchGameLocation(id: string, version: string | null): Promise<GameLocation | null> {
    const params = new URLSearchParams({ source: "web" });
    if (version) params.set("version", version);
    try {
        const response = await fetch(`${API_ROOT}/game/${encodeURIComponent(id)}?${params}`, {
            signal: AbortSignal.timeout(API_TIMEOUT_MS),
        });
        if (!response.ok) return null;
        const json = await response.json() as Partial<GameLocation>;
        if (typeof json.sourceGameUrl !== "string") return null;
        return { sourceGameUrl: json.sourceGameUrl, resourceRoot: json.resourceRoot ?? null };
    } catch {
        return null;
    }
}

// Spliced into the player's index.html by WasmPlayer's inject-version.mjs.
export function playerVersion(html: string): string | null {
    const match = html.match(/window\.QuestVivaVersion = ("[^"\n]*")/);
    return match ? JSON.parse(match[1]) as string : null;
}

// Only set when using Cloudflare Bot Management (an Enterprise add-on), and
// null otherwise, including in local wrangler. Declared here because
// workers-types claims botManagement is always present.
interface BotManagementCf {
    botManagement?: { score?: number; verifiedBot?: boolean } | null;
}

// Cloudflare's "automated" (1) and "likely automated" (2–29) bot score groups.
// https://developers.cloudflare.com/bots/concepts/bot-score/#bot-groupings
const MAX_AUTOMATED_BOT_SCORE = 29;

// Link unfurlers never run the game, so they shouldn't trigger the API call.
// The user-agent check still applies with Bot Management, since many unfurlers
// (e.g. self-hosted Discourse forums) aren't verified bots.
export function isLinkPreviewBot(request: Request): boolean {
    const botManagement = (request.cf as BotManagementCf | undefined)?.botManagement;
    if (botManagement?.verifiedBot) return true;
    if (botManagement?.score !== undefined && botManagement.score <= MAX_AUTOMATED_BOT_SCORE) return true;
    return /bot|crawl|spider|preview|onebox|facebookexternalhit|embedly|slack|whatsapp/i
        .test(request.headers.get("user-agent") ?? "");
}

// Goes right before wasm-player.js. The preload still helps there: the scripts
// ahead of wasm-player.js have to download and run first, while the preload
// scanner starts the game download as soon as these bytes arrive. It must
// match fetchGameBytes()'s plain fetch() (CORS mode, no credentials
// cross-origin) for the browser to reuse it.
export function preloadedGameHtml(id: string, location: GameLocation): string {
    const data = JSON.stringify({ id, ...location }).replace(/</g, "\\u003c");
    return `<link rel="preload" href="${escapeAttribute(location.sourceGameUrl)}" as="fetch" crossorigin="anonymous" />\n    `
        + `<script>window.QuestVivaPreloadedGame = ${data};</script>\n    `;
}
