# Quest Viva plugin for Claude Code

A [Claude Code](https://claude.com/claude-code) plugin that helps Claude write, check, play and publish [Quest Viva](https://questviva.com) games.

It adds:

- **A Quest Viva skill**, which Claude loads when it works on `.aslx` game files. It covers the game file format, the ASL script language and the mistakes agents most often make with it, text adventures and gamebooks, and the loop of editing a game, checking it with `qv check`, testing it with walkthroughs, playing it and previewing it.
- **The `qv` MCP server**, `qv mcp --debug-tools`, whose tools play, check, test, preview and publish games and look things up in the documentation.

## Installing

```text
/plugin marketplace add textadventures/quest
/plugin install questviva@questviva
```

The MCP server needs `qv` on your `PATH`: `dotnet tool install -g QuestViva.Cli`, or a standalone download from the [latest release](https://github.com/textadventures/quest/releases/latest). See [Writing with an AI agent](https://questviva.com/howto/testing/ai-agents/) for more.

## Developing

The skill is `skills/questviva/SKILL.md`, with detail in `skills/questviva/references/`. The documentation site uses it too: `llms.txt` carries `references/asl-primer.md`, and the site's `agents.md` is the whole skill as one file, for other agents (`site/src/agent-guide.mjs`). The site's build fails if a link in the skill names a docs page, a file or a heading that doesn't exist. Links to the docs are site-relative (`/reference/elements/`), so they work as page ids in `qv mcp`'s `docs` tool too.

The primer has no headings, since the `llms.txt` format doesn't allow them where it goes.

To try changes, from the root of this repository:

```text
/plugin marketplace add ./
/plugin install questviva@questviva
```

and check the plugin with `claude plugin validate ./`. When Quest's language, Core library or `qv` changes in a way an author would notice, update the skill along with the docs in `site/`.
