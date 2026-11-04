// @ts-check
import { defineConfig } from "astro/config";
import starlight from "@astrojs/starlight";
import starlightLlmsTxt from "starlight-llms-txt";
import { llmsExampleGames, llmsPageIndex, llmsPrimer, llmsTxtLinks } from "./src/llms-txt.mjs";
import { questGrammar } from "./src/quest-grammar.mjs";
import { redirects } from "./src/redirects.mjs";
import { sidebar } from "./src/sidebar.mjs";
import { betaSite, siteUrl } from "./src/channel.mjs";

const site = siteUrl;

// Options for starlight-llms-txt, which builds llms.txt, llms-full.txt and
// llms-small.txt for AI agents that aren't using `qv mcp`'s docs tool
// (docs/qv-mcp.md). llmsTxtLinks below reads them too, to fix up the links in
// the files the plugin writes.
const llmsTxtOptions = {
    description:
        "Quest Viva is an open-source system for creating and playing text adventure games and gamebooks, the successor to Quest 5. " +
        "Games are written in ASLX, an XML format, with scripts in Quest's own script language; the editor runs in the browser and as a desktop app.",
    details: [
        llmsPrimer(site),
        llmsExampleGames(site, [
            ["cloak_of_darkness.aslx", "the classic small text adventure, from the tutorial's worked example"],
            ["tutorial-game.aslx", "the game built step by step in the text adventure tutorial"],
            ["simple-combat.aslx", "a combat system: a yard with a spade and a pistol, and a street with two zombies (see the combat how-to)"],
            ["lighthouse.aslx", "the gamebook built in the gamebook tutorial"],
            ["PlayAudioExample.aslx", "sound: ambient sound in one room, silence in another, and sounds on moving and on a button"],
        ]),
        llmsPageIndex(sidebar, site, [
            "Language Reference",
            "Understanding Quest Viva",
            "Build Your Game",
            "Customise and Extend",
            "Publishing",
            "Start Here",
        ]),
    ].join("\n\n"),
    customSets: [
        {
            label: "Language reference",
            paths: ["reference/**"],
            description: "script commands, functions, attributes, elements and the .aslx file format",
        },
        {
            label: "Understanding Quest Viva",
            paths: ["understanding/**"],
            description: "how attributes, types and values work, when scripts run, and undo support",
        },
        {
            label: "How-to guides",
            paths: ["howto/**"],
            description: "task-focused guides to building a game: rooms, objects, characters, commands, scripting and more",
        },
    ],
    // The file format and its elements first, as the place to start writing a
    // game; then the rest of the reference.
    promote: ["reference/aslx", "reference/elements", "reference/**", "understanding/**", "howto/**"],
    demote: ["tutorial/**", "contribute/**"],
    exclude: ["tutorial/**", "contribute/**"],
    // The "Section titled ..." link Starlight puts beside every heading.
    customSelectors: { all: [".sl-anchor-link"] },
};

// https://astro.build/config
export default defineConfig({
    site,
    redirects,
    integrations: [
        starlight({
            title: "Quest Viva",
            expressiveCode: {
                shiki: {
                    langs: [questGrammar],
                },
            },
            logo: {
                src: "./src/assets/quest-viva.svg",
            },
            social: [
                { icon: "github", label: "GitHub", href: "https://github.com/textadventures/quest" },
                { icon: "discord", label: "Discord", href: "https://textadventures.co.uk/community/discord" },
            ],
            editLink: {
                baseUrl: "https://github.com/textadventures/quest/edit/main/site/",
            },
            customCss: ["./src/styles/custom.css"],
            // beta.questviva.com stays out of search results, so searches land on the docs for the
            // release people actually have.
            head: betaSite ? [{ tag: "meta", attrs: { name: "robots", content: "noindex" } }] : [],
            components: {
                Banner: "./src/components/Banner.astro",
                Header: "./src/components/Header.astro",
                PageFrame: "./src/components/PageFrame.astro",
            },
            sidebar,
            plugins: [
                starlightLlmsTxt(llmsTxtOptions),
            ],
        }),
        llmsTxtLinks(site, llmsTxtOptions),
    ],
});
