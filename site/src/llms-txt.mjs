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
import { join, relative } from "node:path";
import { globSync } from "node:fs";

const docsDir = fileURLToPath(new URL("./content/docs/", import.meta.url));

function readFrontMatterField(file, field) {
    const frontMatter = readFileSync(file, "utf8").match(/^---\n([\s\S]*?)\n---/);
    const line = frontMatter?.[1].match(new RegExp(`^${field}:\\s*(.+)$`, "m"));
    return line?.[1].trim().replace(/^(["'])(.*)\1$/, "$2");
}

function readDescription(slug) {
    const candidates = [".md", ".mdx", "/index.md", "/index.mdx"].map((ext) => join(docsDir, slug + ext));
    const file = candidates.find((path) => existsSync(path));
    return file && readFrontMatterField(file, "description");
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


// The rest of this file post-processes the files starlight-llms-txt writes,
// since the plugin has no option for either change:
//
// - llms-small.txt is the abridged docs, for agents short on context. Every
//   mention of a script command, function, attribute or type links to its
//   reference entry, and those links add up to about a tenth of the file, so
//   each relative link is replaced by its text.
// - llms-full.txt and the per-section files keep their links, made absolute
//   so they work outside the site: "/reference/js" becomes
//   "https://questviva.com/reference/js", and "#msg" becomes the full URL of
//   the page it's on.
//
// The second needs to know which page each line belongs to. The plugin writes
// each page as a "# Title" line, and two pages share a title, so instead of
// looking titles up this works out the plugin's page order (its filters, and
// its promote/demote sort in generator.ts) and checks every title against it.
// If the plugin's output ever stops matching, the build fails rather than
// writing wrong links.

const relativeLink = /(?<!!)\[([^\]]+)\]\(((?:\/|#)[^)\s]*)\)/g;
const fence = /^\s*(```|~~~)/;

// The plugin matches with micromatch. These options only use "dir" and
// "dir/**" (which, as in micromatch, matches "dir" itself too), so match just
// those rather than depend on micromatch directly.
function matchesGlob(id, pattern) {
    if (pattern.endsWith("/**")) return id === pattern.slice(0, -3) || id.startsWith(pattern.slice(0, -2));
    if (/[*?[{]/.test(pattern)) throw new Error(`llms-txt.mjs: unsupported glob "${pattern}"`);
    return id === pattern;
}
const matchesAny = (id, patterns) => patterns.some((pattern) => matchesGlob(id, pattern));

function readDocs() {
    return globSync("**/*.{md,mdx}", { cwd: docsDir }).map((path) => {
        const id = path.replace(/\.mdx?$/, "").replace(/(^|\/)index$/, "") || "index";
        const file = join(docsDir, path);
        return { id, title: readFrontMatterField(file, "title"), draft: readFrontMatterField(file, "draft") === "true" };
    });
}

/** The pages the plugin writes into one file, in the order it writes them. */
function pagesInPluginOrder(docs, { include, exclude }, { promote = [], demote = [] }) {
    const rank = (id) => {
        const demoted = demote.findIndex((pattern) => matchesGlob(id, pattern));
        const promoted = demoted > -1 ? -1 : promote.findIndex((pattern) => matchesGlob(id, pattern));
        return "_".repeat((promoted > -1 ? promote.length - promoted : 0) + demote.length - demoted - 1) + id;
    };
    const collator = new Intl.Collator("en");
    return docs
        .filter((doc) => !doc.draft && (!include || matchesAny(doc.id, include)) && !(exclude && matchesAny(doc.id, exclude)))
        .sort((a, b) => collator.compare(rank(a.id), rank(b.id)));
}

function rewriteLinks(markdown, pages, site, name) {
    let inFence = false;
    let next = 0;
    let page;
    const lines = markdown.split("\n").map((line) => {
        if (fence.test(line)) inFence = !inFence;
        if (inFence) return line;
        if (next < pages.length && line === `# ${pages[next].title}`) page = pages[next++];
        return line.replace(relativeLink, (_, text, target) => {
            if (!page) throw new Error(`llms-txt.mjs: ${name} has a link before its first page`);
            const path = target.startsWith("#") ? `/${page.id === "index" ? "" : `${page.id}/`}${target}` : target;
            return `[${text}](${new URL(path, site).href})`;
        });
    });
    if (next < pages.length) {
        throw new Error(
            `llms-txt.mjs: ${name} doesn't list its pages in the order expected (no "# ${pages[next].title}" for ${pages[next].id}); ` +
                "check pagesInPluginOrder against starlight-llms-txt's generator.ts",
        );
    }
    return lines.join("\n");
}

function unwrapRelativeLinks(markdown) {
    let inFence = false;
    return markdown
        .split("\n")
        .map((line) => {
            if (fence.test(line)) inFence = !inFence;
            return inFence ? line : line.replace(relativeLink, "$1");
        })
        .join("\n");
}

/**
 * Astro integration that rewrites the links in the built llms-*.txt files.
 * @param {string} site the site's absolute URL
 * @param {any} options the options passed to starlight-llms-txt
 */
export function llmsTxtLinks(site, options) {
    return {
        name: "llms-txt-links",
        hooks: {
            "astro:build:done": ({ dir, logger }) => {
                const outDir = fileURLToPath(dir);
                const docs = readDocs();
                const rewrite = (path, transform) => {
                    const file = join(outDir, path);
                    if (!existsSync(file)) throw new Error(`llms-txt.mjs: ${path} wasn't built`);
                    const before = readFileSync(file, "utf8");
                    const after = transform(before);
                    writeFileSync(file, after);
                    logger.info(`${relative(outDir, file)}: ${before.length} -> ${after.length} bytes`);
                };
                rewrite("llms-small.txt", unwrapRelativeLinks);
                rewrite("llms-full.txt", (text) => rewriteLinks(text, pagesInPluginOrder(docs, {}, options), site, "llms-full.txt"));
                for (const set of options.customSets ?? []) {
                    const path = `_llms-txt/${set.label.toLowerCase().replace(/[^a-z0-9]+/g, "-")}.txt`;
                    rewrite(path, (text) => rewriteLinks(text, pagesInPluginOrder(docs, { include: set.paths }, options), site, path));
                }
            },
        },
    };
}
