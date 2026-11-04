// Which docs site this build is for. questviva.com documents the current stable release and is built
// from its release branch (release/6.0); beta.questviva.com documents the next release and is built
// from main. Cloudflare Pages sets PUBLIC_BETA_SITE=true for the beta build; see "Docs site" in
// docs/release-channels.md.

import process from "node:process";

export const stableSiteUrl = "https://questviva.com";
export const betaSiteUrl = "https://beta.questviva.com";

export const betaSite = process.env.PUBLIC_BETA_SITE === "true";

/** The site this build is served from, for canonical links, the sitemap and llms.txt. */
export const siteUrl = betaSite ? betaSiteUrl : stableSiteUrl;

export const stablePlaySiteUrl = "https://play.questviva.com";
export const betaPlaySiteUrl = "https://play-beta.questviva.com";

/** The web app that matches these docs, for "Start creating" and "Play games" links. */
export const playSiteUrl = betaSite ? betaPlaySiteUrl : stablePlaySiteUrl;
