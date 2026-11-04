// @ts-check
import { defineConfig } from "astro/config";
import starlight from "@astrojs/starlight";
import starlightLlmsTxt from "starlight-llms-txt";
import { llmsPageIndex } from "./src/llms-txt.mjs";
import { questGrammar } from "./src/quest-grammar.mjs";
import { redirects } from "./src/redirects.mjs";
import { sidebar } from "./src/sidebar.mjs";

const site = "https://questviva.com";

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
            components: {
                Header: "./src/components/Header.astro",
                PageFrame: "./src/components/PageFrame.astro",
            },
            sidebar,
            plugins: [
                // llms.txt, llms-full.txt and llms-small.txt, for AI agents
                // that aren't using `qv mcp`'s docs tool (docs/qv-mcp.md).
                starlightLlmsTxt({
                    description:
                        "Quest Viva is an open-source system for creating and playing text adventure games and gamebooks, the successor to Quest 5. " +
                        "Games are written in ASLX, an XML format, with scripts in Quest's own script language; the editor runs in the browser and as a desktop app.",
                    details: llmsPageIndex(sidebar, site, [
                        "Language Reference",
                        "Understanding Quest Viva",
                        "Build Your Game",
                        "Customise and Extend",
                        "Publishing",
                        "Start Here",
                    ]),
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
                            paths: ["howto", "howto/**"],
                            description: "task-focused guides to building a game: rooms, objects, characters, commands, scripting and more",
                        },
                    ],
                    promote: ["reference/**", "understanding/**", "howto", "howto/**"],
                    demote: ["tutorial/**", "contribute/**"],
                    exclude: ["tutorial/**", "contribute/**"],
                }),
            ],
        }),
    ],
});
