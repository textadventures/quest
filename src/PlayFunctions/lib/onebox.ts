// Open Graph / Twitter Card meta tags for a shared game link, so link unfurlers
// (Discourse oneboxes, Discord, Slack, etc.) that don't run JavaScript can show
// the game's name, description and cover art.

import { escapeAttribute } from "./html";

const API_ROOT = "https://textadventures.co.uk/api";
const SITE_NAME = "Quest Viva";
const MAX_DESCRIPTION_LENGTH = 300;
const API_TIMEOUT_MS = 1500;

export interface GameDetails {
    id: string;
    name: string;
    description: string | null;
    author: string | null;
    cover: string | null;
    thumbnail: string | null;
}

export async function fetchGameDetails(id: string): Promise<GameDetails | null> {
    try {
        const response = await fetch(`${API_ROOT}/GameDetails/${encodeURIComponent(id)}?source=onebox`, {
            signal: AbortSignal.timeout(API_TIMEOUT_MS),
            cf: { cacheTtl: 3600, cacheEverything: true },
        });
        if (!response.ok) return null;
        const details = await response.json() as GameDetails;
        return details?.name ? details : null;
    } catch {
        return null;
    }
}

function summarize(description: string): string {
    const collapsed = description.replace(/\s+/g, " ").trim();
    if (collapsed.length <= MAX_DESCRIPTION_LENGTH) return collapsed;
    return collapsed.slice(0, MAX_DESCRIPTION_LENGTH - 1).trimEnd() + "…";
}

// Cover art URLs from the API contain literal spaces.
function absoluteImageUrl(url: string): string {
    return encodeURI(decodeURI(url));
}

export function gameTitle(details: GameDetails): string {
    return `${details.name} | ${SITE_NAME}`;
}

export function metaTags(details: GameDetails, pageUrl: string): string {
    const title = details.author ? `${details.name} by ${details.author}` : details.name;
    const description = details.description ? summarize(details.description) : null;
    const image = details.cover ?? details.thumbnail;

    const tags: [string, string, string][] = [
        ["property", "og:type", "website"],
        ["property", "og:site_name", SITE_NAME],
        ["property", "og:title", title],
        ["property", "og:url", pageUrl],
        ["name", "twitter:card", image ? "summary_large_image" : "summary"],
        ["name", "twitter:title", title],
    ];
    if (description) {
        tags.push(
            ["name", "description", description],
            ["property", "og:description", description],
            ["name", "twitter:description", description],
        );
    }
    if (image) {
        const imageUrl = absoluteImageUrl(image);
        tags.push(
            ["property", "og:image", imageUrl],
            ["property", "og:image:alt", `Cover art for ${details.name}`],
            ["name", "twitter:image", imageUrl],
        );
    }

    const meta = tags
        .map(([attr, key, value]) => `<meta ${attr}="${key}" content="${escapeAttribute(value)}" />`)
        .join("\n    ");
    return `\n    ${meta}\n    <link rel="canonical" href="${escapeAttribute(pageUrl)}" />\n`;
}
