// Builds the page index that llms.txt carries, so an AI agent reading it can go
// straight to the page it needs instead of downloading llms-full.txt.
//
// The starlight-llms-txt plugin generates llms.txt itself, but only links to
// whole documentation sets, so this list goes in as its `details`. The llms.txt
// format (https://llmstxt.org/) allows no headings there, so each sidebar group
// is a bold line followed by its pages. Groups come in `priority` order: the
// reference and guides first, the tutorials and contributor docs last.
//
// Labels and order come from the sidebar, and Starlight fails the build if a
// sidebar slug names a page that doesn't exist, so every link here is live.
// A page's `description` front matter is added after its link when it has one.

import { existsSync, readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath, URL } from "node:url";
import { join } from "node:path";

const docsDir = fileURLToPath(new URL("./content/docs/", import.meta.url));

function readDescription(slug) {
    const candidates = [".md", ".mdx", "/index.md", "/index.mdx"].map((ext) => join(docsDir, slug + ext));
    const file = candidates.find((path) => existsSync(path));
    if (!file) return undefined;
    const frontMatter = readFileSync(file, "utf8").match(/^---\n([\s\S]*?)\n---/);
    const line = frontMatter?.[1].match(/^description:\s*(.+)$/m);
    return line?.[1].trim().replace(/^(["'])(.*)\1$/, "$2");
}

function renderItems(items, site, depth) {
    const indent = "  ".repeat(depth);
    return items.flatMap((item) => {
        if (item.items) {
            return [`${indent}- ${item.label}`, ...renderItems(item.items, site, depth + 1)];
        }
        const url = new URL(`/${item.slug}/`, site).href;
        const description = readDescription(item.slug);
        return [`${indent}- [${item.label}](${url})${description ? `: ${description}` : ""}`];
    });
}

/**
 * @param {Array<any>} sidebar Starlight's sidebar config
 * @param {string} site the site's absolute URL
 * @param {string[]} priority top-level group labels to list first, in this order
 */
export function llmsPageIndex(sidebar, site, priority) {
    const rank = (group) => {
        const index = priority.indexOf(group.label);
        return index === -1 ? priority.length : index;
    };
    const groups = [...sidebar].sort((a, b) => rank(a) - rank(b));
    const sections = groups.map((group) => [`**${group.label}**`, "", ...renderItems(group.items, site, 0)].join("\n"));
    return ["Every documentation page, grouped as in the site's sidebar:", ...sections].join("\n\n");
}

// llms-small.txt is the abridged docs, for agents short on context. Its
// cross-references (every mention of a script command, function, attribute or
// type links to its reference entry) are relative links, which go nowhere in a
// standalone text file, and add up to about a tenth of the file. So this
// replaces each relative link with its text, leaving external links, images
// and code blocks alone. llms-full.txt keeps every link.
export function unwrapRelativeLinks(markdown) {
    let inFence = false;
    return markdown
        .split("\n")
        .map((line) => {
            if (/^\s*(```|~~~)/.test(line)) inFence = !inFence;
            if (inFence) return line;
            return line.replace(/(?<!!)\[([^\]]+)\]\((?:\/|#|\.\.?\/)[^)\s]*\)/g, "$1");
        })
        .join("\n");
}

/** Astro integration that applies unwrapRelativeLinks to the built llms-small.txt. */
export function llmsSmallWithoutLinks() {
    return {
        name: "llms-small-without-links",
        hooks: {
            "astro:build:done": ({ dir, logger }) => {
                const file = fileURLToPath(new URL("llms-small.txt", dir));
                if (!existsSync(file)) return;
                const before = readFileSync(file, "utf8");
                const after = unwrapRelativeLinks(before);
                writeFileSync(file, after);
                logger.info(`llms-small.txt: ${before.length} -> ${after.length} bytes`);
            },
        },
    };
}
