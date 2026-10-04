import { type DevProxyEnv, withoutDevCompression } from "../../lib/dev-proxy";
import { rewriteGamePage } from "../../lib/game-page";

// AppShell game-detail page: /play/<game id> (the sibling /play/category/<slug>
// and /play/search routes are not game ids).
const NON_GAME_SEGMENTS = new Set(["category", "search"]);

export const onRequest: PagesFunction<DevProxyEnv> = async ({ request, env, next }) => {
    const match = new URL(request.url).pathname.match(/^\/play\/([^/]+)\/?$/);
    const gameId = match && !NON_GAME_SEGMENTS.has(match[1]) ? decodeURIComponent(match[1]) : null;
    const response = await rewriteGamePage(request, next, { gameId });
    return env.DEV_PROXY && gameId ? withoutDevCompression(response) : response;
};
