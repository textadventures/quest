import { defineRouteMiddleware } from "@astrojs/starlight/route-data";
import { betaSite, playSiteUrl, stablePlaySiteUrl } from "./channel.mjs";

// On beta.questviva.com, the homepage's "Start creating" and "Play games" buttons go to
// play-beta.questviva.com, so the editor people land in is the one these docs describe. Hero actions
// are frontmatter, so they're rewritten here rather than in the page. Other links to
// play.questviva.com in the docs (shareable player links, the CORS origin) are about the stable
// site and stay as they are.
export const onRequest = defineRouteMiddleware((context) => {
    if (!betaSite) return;
    const hero = context.locals.starlightRoute.entry.data.hero;
    for (const action of hero?.actions ?? []) {
        if (action.link.startsWith(stablePlaySiteUrl)) {
            action.link = playSiteUrl + action.link.slice(stablePlaySiteUrl.length);
        }
    }
});
