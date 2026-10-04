import { type DevProxyEnv, toGameResourceUrl, withDevApiRoot, withoutDevCompression } from "../../lib/dev-proxy";
import { rewriteGamePage } from "../../lib/game-page";

// WasmPlayer: /player/?id=<game id>
export const onRequest: PagesFunction<DevProxyEnv> = async ({ request, env, next }) => {
    const url = new URL(request.url);
    const isPlayerPage = url.pathname === "/player/" || url.pathname === "/player/index.html";
    const response = await rewriteGamePage(request, next, {
        gameId: isPlayerPage ? url.searchParams.get("id") : null,
        preloadGame: true,
        rewriteGameUrl: env.DEV_PROXY ? toGameResourceUrl : undefined,
    });
    return env.DEV_PROXY && isPlayerPage ? withoutDevCompression(withDevApiRoot(response)) : response;
};
