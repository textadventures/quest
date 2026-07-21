// Client-side "which installer does this visitor want" widget, used by
// DownloadButton.svelte on the Play tab's browser build (never shown inside
// Electron — that user already has the app). Sibling implementation:
// site/src/components/DownloadButton.astro (questviva.com's docs site) —
// same detect+fetch+match rules, duplicated rather than shared since these
// are separate build pipelines (SvelteKit vs. Astro) with no shared package
// between them.
//
// Deliberately independent of ApiController.Catalog's server-side matching
// (textadventures.co.uk's LatestVersionService.GetDownloadUrlAsync) even
// though the rules are the same — that endpoint only serves Electron
// clients (see home-catalog.ts's UpdateInfo), and a plain browser visitor
// here isn't sending ClientInfo at all. Hitting GitHub's public releases API
// directly keeps this page static/serverless, consistent with
// play.questviva.com having no backend of its own.

const RELEASES_API_URL = "https://api.github.com/repos/textadventures/quest/releases/latest";
const RELEASES_PAGE_URL = "https://github.com/textadventures/quest/releases/latest";

// Short-lived cache only to avoid a duplicate fetch if the widget re-mounts
// during one visit — not a substitute for GitHub's own CDN caching.
const CACHE_KEY = "questviva-latest-release";
const CACHE_TTL_MS = 10 * 60 * 1000;

export interface DownloadLink {
    label: string;
    url: string;
}

export interface DownloadLinks {
    version: string | null;
    primary: DownloadLink | null;
    others: DownloadLink[];
    releasePage: string;
}

interface ReleaseAsset {
    name: string;
    browser_download_url: string;
}

interface ReleaseResponse {
    tag_name: string;
    assets: ReleaseAsset[];
}

export type DetectedOs = "Windows" | "Mac" | "Linux" | null;

export function detectOs(): DetectedOs {
    if (typeof navigator === "undefined") return null;
    const ua = navigator.userAgent;
    if (/Windows/.test(ua)) return "Windows";
    if (/Mac OS X|Macintosh/.test(ua)) return "Mac";
    if (/Linux/.test(ua)) return "Linux";
    return null;
}

// Extension-based, not exact filename — electron-builder's default
// artifactName bakes the version into every asset name (no override in
// src/ElectronApp/package.json), so the extension is what survives a
// version bump. wantArm64 undefined means "don't care" — needed for Mac,
// whose only build is arm64 (so its .dmg filename always contains "arm64",
// unlike Windows/Linux where that substring means something). Linux itself
// defaults to the x64 .deb (best desktop integration, see
// docs/electron-desktop-app.md) since arch can't be reliably sniffed from a
// browser; arm64 users fall through to "other downloads".
function findAsset(assets: ReleaseAsset[], extension: string, wantArm64?: boolean): DownloadLink | null {
    const match = assets.find(
        (a) =>
            a.name.toLowerCase().endsWith(extension) &&
            (wantArm64 === undefined || a.name.toLowerCase().includes("arm64") === wantArm64),
    );
    return match ? { label: match.name, url: match.browser_download_url } : null;
}

function buildLinks(release: ReleaseResponse, detected: DetectedOs): DownloadLinks {
    const assets = release.assets;

    // Linux gets two candidates, not one — .deb and AppImage are both valid
    // choices (a release can also ship only one of the two;
    // electron-publish.yml's three OS legs run independently), so both are
    // listed for a Linux visitor to pick between rather than silently
    // dropping whichever one this widget didn't prefer. Order matters: .deb
    // listed first so it's the one picked as the primary/"Download for
    // Linux" button when both exist (best desktop integration, see
    // docs/electron-desktop-app.md).
    const candidates: { os: Exclude<DetectedOs, null>; label: string; link: DownloadLink | null }[] = [
        { os: "Windows", label: "Windows", link: findAsset(assets, ".exe") },
        { os: "Mac", label: "Mac (Apple Silicon)", link: findAsset(assets, ".dmg") },
        { os: "Linux", label: "Linux (.deb)", link: findAsset(assets, ".deb", false) },
        { os: "Linux", label: "Linux (.AppImage)", link: findAsset(assets, ".appimage", false) },
    ];

    const primaryEntry = detected ? candidates.find((c) => c.os === detected && c.link) : undefined;
    const primary = primaryEntry
        ? { label: `Download for ${primaryEntry.label}`, url: primaryEntry.link!.url }
        : null;

    const others = candidates
        .filter((c) => c !== primaryEntry && c.link)
        .map((c) => ({ label: c.label, url: c.link!.url }));

    return {
        version: release.tag_name,
        primary,
        others,
        releasePage: RELEASES_PAGE_URL,
    };
}

function fallbackLinks(): DownloadLinks {
    return { version: null, primary: null, others: [], releasePage: RELEASES_PAGE_URL };
}

export async function fetchDownloadLinks(): Promise<DownloadLinks> {
    const detected = detectOs();

    try {
        const cached = sessionStorage.getItem(CACHE_KEY);
        if (cached) {
            const { fetchedAt, release } = JSON.parse(cached) as { fetchedAt: number; release: ReleaseResponse };
            if (Date.now() - fetchedAt < CACHE_TTL_MS) {
                return buildLinks(release, detected);
            }
        }
    } catch {
        // Corrupt/unavailable sessionStorage — fall through to a fresh fetch.
    }

    try {
        const response = await fetch(RELEASES_API_URL);
        if (!response.ok) return fallbackLinks();
        const release = (await response.json()) as ReleaseResponse;

        try {
            sessionStorage.setItem(CACHE_KEY, JSON.stringify({ fetchedAt: Date.now(), release }));
        } catch {
            // Storage full/disabled — not fatal, just skip caching.
        }

        return buildLinks(release, detected);
    } catch {
        return fallbackLinks();
    }
}
