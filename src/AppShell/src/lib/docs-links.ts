// Single source of truth for links out to the documentation site
// (questviva.com — the Astro/Starlight project in site/).
//
// questviva.com's root is a marketing splash (site/src/content/docs/index.mdx,
// template: splash), so it's the wrong landing spot for someone who clicked
// "Documentation" from inside the app — that page's own Documentation action
// points at /intro, and so do we.
//
// Sibling hard-coded link: src/ElectronApp/src/main.ts's Help menu. It's a
// separate build (Electron main process) with no import path into AppShell, so
// it stays duplicated rather than shared.

const DOCS_BASE_URL = "https://questviva.com";

// Trailing slashes are deliberate: the site builds to directory-style routes,
// so questviva.com/intro answers 308 -> /intro/. Linking to the canonical form
// saves every click a redirect hop.
function docsUrl(path: string): string {
    return `${DOCS_BASE_URL}${path}/`;
}

/** Documentation home — the reference/guide entry point, not the splash page. */
export const DOCS_URL = docsUrl("/intro");

/** Step-by-step "build your first game" walkthrough, for first-time authors. */
export const DOCS_TUTORIAL_URL = docsUrl("/tutorial/tutorial-introduction");
