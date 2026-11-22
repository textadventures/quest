// The Quest Viva skill for AI agents lives in the Claude Code plugin,
// plugins/questviva/skills/questviva/ at the root of the repo: SKILL.md and
// the files in its references/ folder. The site uses it twice:
//
// - llms.txt carries references/asl-primer.md (llmsPrimer in llms-txt.mjs).
// - agents.md, at the root of the built site, is the whole skill as one
//   Markdown file, for agents other than Claude Code: SKILL.md without its
//   front matter, then each reference file, with links made absolute and the
//   links between the files turned into links within agents.md.
//
// Links in the skill to the docs are site-relative ("/reference/elements/"),
// so they also work as page ids in `qv mcp`'s docs tool. The build fails if
// one names a page that doesn't exist, or if a link to another file of the
// skill names a file or heading that isn't there.

import { existsSync, readFileSync, writeFileSync } from "node:fs";
import { fileURLToPath, URL } from "node:url";
import { join } from "node:path";
import { pageFile } from "./llms-txt.mjs";

const skillDir = fileURLToPath(new URL("../../plugins/questviva/skills/questviva/", import.meta.url));

// The skill's files, in the order agents.md has them, and the heading each
// reference file gets there. asl-primer.md has no heading of its own, since
// llms.txt allows none.
const references = [
    ["asl-primer.md", "The ASL primer"],
    ["elements.md", null],
    ["gamebooks.md", null],
];

const fence = /^\s*(```|~~~)/;
const link = /(?<!!)\[([^\]]+)\]\(([^)\s]+)\)/g;

/** @param {string} name a file in the skill's folder, such as "references/elements.md" */
export function readSkillFile(name) {
    const path = join(skillDir, name);
    if (!existsSync(path)) throw new Error(`agent-guide.mjs: the skill has no ${name}`);
    return readFileSync(path, "utf8");
}

// GitHub's heading anchors, which is how agents and Markdown viewers resolve
// "file.md#anchor": lower case, punctuation dropped, spaces as hyphens.
const slug = (heading) =>
    heading
        .toLowerCase()
        .replace(/`/g, "")
        .replace(/[^\p{L}\p{N}\s-]/gu, "")
        .trim()
        .replace(/\s/g, "-");

/** Calls `fn` on each line outside a code fence, and returns the lines it gives back. */
function mapProse(markdown, fn) {
    let inFence = false;
    return markdown
        .split("\n")
        .map((line) => {
            if (fence.test(line)) {
                inFence = !inFence;
                return line;
            }
            return inFence ? line : fn(line);
        })
        .join("\n");
}

const headings = (markdown) => {
    const found = [];
    mapProse(markdown, (line) => {
        const heading = line.match(/^#+\s+(.*)$/);
        if (heading) found.push(slug(heading[1]));
        return line;
    });
    return found;
};

const stripFrontMatter = (markdown) => markdown.replace(/^---\n[\s\S]*?\n---\n+/, "");

// Each skill file by its path from the skill's folder, with the anchor it
// starts at in agents.md.
function skillFiles() {
    const files = [{ name: "SKILL.md", text: stripFrontMatter(readSkillFile("SKILL.md")), anchor: null }];
    for (const [file, heading] of references) {
        let text = readSkillFile(`references/${file}`).trim() + "\n";
        if (heading) text = `# ${heading}\n\n${text}`;
        files.push({ name: `references/${file}`, text, anchor: headings(text)[0] });
    }
    return files;
}

/**
 * Checks every link in the skill, and returns `fn` applied to each link's
 * target, resolved against the file it's in.
 */
function rewriteSkillLinks(file, files, fn) {
    const dir = file.name.includes("/") ? file.name.replace(/[^/]*$/, "") : "";
    return mapProse(file.text, (line) =>
        line.replace(link, (whole, text, target) => {
            if (/^[a-z]+:/.test(target)) return whole;
            if (target.startsWith("/")) {
                if (!pageFile(target.replace(/#.*$/, "").replace(/^\/|\/$/g, ""))) {
                    throw new Error(`agent-guide.mjs: ${file.name} links to ${target}, which isn't a page`);
                }
                return fn({ text, page: target });
            }
            const [path, anchor] = target.split("#");
            const resolved = path ? join(dir, path) : file.name;
            const other = files.find((f) => f.name === resolved);
            if (!other) throw new Error(`agent-guide.mjs: ${file.name} links to ${target}, which isn't a file of the skill`);
            if (anchor && !headings(other.text).includes(anchor)) {
                throw new Error(`agent-guide.mjs: ${file.name} links to ${target}, but ${resolved} has no such heading`);
            }
            return fn({ text, anchor: anchor ?? other.anchor });
        }),
    );
}

/** Fails if a link in the skill is broken. Run when the site's config loads. */
export function checkSkillLinks() {
    const files = skillFiles();
    for (const file of files) rewriteSkillLinks(file, files, ({ text }) => text);
}

/**
 * The skill as one Markdown file, with its links absolute.
 * @param {string} site the site's absolute URL
 */
export function agentGuide(site) {
    const files = skillFiles();
    const parts = files.map((file, index) => {
        const text = rewriteSkillLinks(file, files, ({ text, page, anchor }) =>
            page ? `[${text}](${new URL(page, site).href})` : `[${text}](#${anchor})`,
        );
        // SKILL.md's own heading is the document's; every other file's headings
        // go down a level beneath it.
        return index === 0 ? text.trim() : mapProse(text.trim(), (line) => line.replace(/^#/, "##"));
    });
    const anchors = parts.flatMap(headings);
    const duplicate = anchors.find((anchor, index) => anchors.indexOf(anchor) !== index);
    if (duplicate) throw new Error(`agent-guide.mjs: two headings in agents.md would share the anchor #${duplicate}`);
    const intro =
        `> The Quest Viva skill for AI agents, as one file. Point an agent at ${new URL("/agents.md", site).href}, ` +
        "or save it as AGENTS.md in your game's folder. It's built from the Quest Viva plugin for Claude Code: " +
        `${new URL("/howto/testing/ai-agents/", site).href}`;
    return [parts[0].replace(/^(# .*\n)/, `$1\n${intro}\n`), ...parts.slice(1)].join("\n\n") + "\n";
}

/**
 * Astro integration that writes agents.md into the built site.
 * @param {string} site the site's absolute URL
 */
export function agentGuideFile(site) {
    checkSkillLinks();
    return {
        name: "quest-agent-guide",
        hooks: {
            "astro:build:done": ({ dir }) => {
                writeFileSync(new URL("agents.md", dir), agentGuide(site));
            },
        },
    };
}
