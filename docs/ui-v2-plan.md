# Player UI v2

A plan for a modern, responsive player interface with no jQuery, the default for games saved as ASL 610 or later. It has defined, supported ways to customise it. Older games keep today's interface, frozen.

This folds in #2337 (a supported game stylesheet, CSS custom properties, status attributes on phones). It's grounded in a survey of all 21,704 published games ([research/player-ui-surface-survey.md](research/player-ui-surface-survey.md)).

## Why now, and why this shape

Today's player UI (`src/WasmPlayer/wwwroot/legacy/`, formerly `player-ui/`) is Quest 5's `PlayerController` UI, carried forward:
- jQuery 2.1, jQuery UI and a multi-open-accordion plugin
- paper.js for the map
- `playercore.js`, a 3,000-line file of globals

Every element ID in it is effectively public API, for two reasons:
- **Published games carry their own copy of Core** (see CLAUDE.md's "Core Library Semantics"), and stock Core addresses the UI by ID. For example, `turnoffcompass` does `JS.setCss("#compassAccordion", "display:none")`.
- **Authors patch the DOM directly.** 2.3% of games reach into it, and that share has been rising: 0.7% of games saved by Quest 5.4, 3.4% by 5.8, 8.5% by 5.9.

So the current UI can only be changed carefully and additively (the mobile exits strip, #2581, is the model), and it can't be modernised wholesale.

The survey settles the two main design questions:

1. **Keep the legacy UI frozen; don't make it a compatibility layer over the new one.** Stock Core's surface is fully known (about 40 JS functions, about 15 element IDs, about 20 `request()` types), but 0.5% of games replace player functions (`scrollToEnd = function(){}`, a shared snippet rewriting `updateList`/`updateStatus`) and 0.2% read internals like `_waitMode`. Those need the original code verbatim, so a compatibility layer would just be a second legacy UI.
2. **Give authors supported ways to do what they currently hack,** or they'll hack v2 the same way. The recurring hacks are:
   - hiding, reordering or adding panes
   - styling the command bar
   - turning autoscroll off
   - click-to-continue on waits
   - fading or disabling earlier output
   - turning map interaction off
   - custom status and command panes

## Goals

- **Responsive from the start.** Phone, tablet and desktop are first-class layouts, not a desktop layout collapsed behind a hamburger menu.
- **Accessible by default:** live-region output, real buttons, focus management, 44px touch targets, reduced-motion support, colour contrast that survives theming.
- **No jQuery, jQuery UI or paper.js.** A v2 game shouldn't download about 200 KB (brotli) of legacy UI stack. That's roughly 6% of the published player, which matters most on phones.
- **Customisation in defined tiers**, each documented and versioned (see below), so authors never need `JS.eval` against internals for common changes.
- **Same engine, same hosts:** play.questviva.com, textadventures.co.uk (through play.questviva.com), the Electron app, the editor's Play/preview, and the single-file HTML export (`docs/wasmplayer-single-file-export.md`).

Non-goals for the first release:
- **Letting a game replace the whole UI.** There are layout presets, custom panes and stylesheets instead. A full replacement would make the game's UI code frozen with the game, which is the problem this plan avoids.
- **Changing Quest 4 (`.asl`/`.cas`) or pre-610 games.** They stay on legacy. ("Later" below notes a possible future route for old games that only use stock Core.)

## How a game gets its UI

The choice is made at boot, after the game file is loaded and its version is known:

| Game | UI |
|---|---|
| ASL 610 or later | v2 |
| ASL 600 or earlier, or Quest 4 `.asl`/`.cas` | legacy |
| any game with `game.classicinterface = true` | legacy (an author escape hatch, shown in the editor) |
| `?ui=v2` / `?ui=legacy` URL parameter | forced, for development and testing only |

Today, `index.html` loads jQuery, jQuery UI, paper.js, `playercore.js` and `player.js` up front, and `swapInPlayerUi()` injects `playercore.htm`. Instead, each UI becomes a lazily loaded bundle: `legacy/` (today's files, moved and frozen) and `v2/`. The host (`wasm-player.js`) keeps the parts that are the same for both: the start screen, save-slot manager, debugger and transcript viewer, which are already in the scoped Tailwind/Skeleton "chrome" stack. It then picks a bundle. #2493 has already moved the player UI into `wwwroot/legacy/` as static web assets.

**Timing caveat for 610:** `v610` became the editor's save version on 2026-10-22 (#2576/#2577) and hasn't shipped in any release yet. No 6.1 release, beta included, ships until 610 Core's UI calls have all been rewritten onto the contract, so no published 610 game ever carries today's ID- and jQuery-based `InitInterface` ([ui-v2-contract.md](ui-v2-contract.md), finding 1). Any 610 game saved by a 6.1 beta before v2 is the default will switch to v2 when that happens. That's acceptable for a beta audience, provided that:
- v2 only becomes the default for 610 once it reaches parity (Phase 5), so until then 610 games play on legacy as now
- the editor's upgrade warning (#2577) also flags games that use legacy-UI hacks (`JS.eval`/`JS.setCss` against legacy IDs), with `classicinterface` as the one-click way to keep them on legacy

Gating v2 on a later version (620), or on the attribute alone, was considered and rejected (decision 1).

## The contract between engine and UI

The engine already talks to the UI through a mostly typed interface: `IPlayer` (`src/Common/IGame.cs`), about 30 members. WasmPlayer maps these one to one onto JS imports (`setModuleImports("wasm-player", …)` in `wasm-player.js`):
- `ShowMenu`, `ShowQuestion`, `UpdateList`, `SetCompassDirections`, `SetStatusText`, `LocationUpdated`, `SetTurnPending`, `Show`/`Hide`, `SetInterfaceString`, …

The untyped parts are what make the legacy UI hard to change:

- **`RunScriptAsync`**, which is how every `JS.*` call reaches the UI: arbitrary function names evaluated as global JS.
- **Output as HTML strings** with inline handlers (`<a class="cmdlink" onclick="sendCommand('…')">` from `PlayerHelper`, and object-link markup from Core's text processor).

For v2, three things change:

1. **A named JS API, `QuestUI`,** is the only thing 610 Core calls, and it's what authors are told to use. Together with the CSS custom properties and `data-qv-part` names (see below), it forms **the 610 UI contract**. The `QuestUI` part is the set 610 Core needs: panes, status, compass, location, command bar, menus, picture frame, map, media, background, fonts, output sections and text effects, plus the author hooks below. 610 Core is inlined into every 610 game, so a published 610 game must **never** break on any future player: the contract only ever gains additions. The contract is versioned by ASL version, so there's no separate API number. If a later Core ever needs a breaking change, that's a new ASL version with its own contract, and 610 games keep theirs. The interface behind the contract (layout, accessibility, phone behaviour) keeps improving for every 610 game, the same way the exits strip (#2581) reached every legacy game.
2. **610 Core goes through `QuestUI` only.** Where 610 Core today calls `JS.setCss("#compassAccordion", …)`, it calls `JS.QuestUI.setVisible("compass", false)` instead. No new typed `IPlayer` members are added (see [ui-v2-contract.md](ui-v2-contract.md), Decisions). That rewrite of `InitInterface` and friends only applies under the 610 version gate. Pre-610 Core is unchanged by definition.
3. **Output markup for 610 uses data attributes, not inline `onclick`:**
   - `<a class="cmdlink commandlink" data-command="look">`
   - `<a class="cmdlink elementmenu" data-elementid="…">`

   These are the names current Core already uses for object, exit and command links. 610 removes the inline handlers that remain (ShowMenu's fallback links and `{popup:}`). v2 handles clicks through delegated listeners, which makes it CSP-friendly and lets v2 own menus, disabling and accessibility. `PlayerHelper`'s inline-handler links only reach pre-540 games, whose output doesn't go through Core's text processor, so they stay as they are. Author HTML in `msg` passes through as today.

`JS.eval` and `JS.*` still work in v2, since a game's own JavaScript is legitimate. But only `QuestUI`, the CSS tokens and the documented parts are stable. Anything else in v2's DOM is internal and may change.

This also moves #1780 (embedding without HTML) forward: once 610 Core goes through typed calls, a non-HTML `IPlayer` sees far less raw JS and HTML.

## Customisation tiers

From least to most effort for authors. Every tier is part of the 610 UI contract, so it's documented and stable for good.

1. **Options:** the existing Display/Interface settings in the editor (fonts, colours, panes on/off, command bar, location bar, border, width/padding, picture frame, map). They map onto tier 3's tokens rather than onto DOM operations.
2. **Themes:** named sets of options and tokens. The existing `theme_novella`/`theme_retro`/`theme_typewriter`/`theme_hotdogstand` types are re-expressed as token sets, answering #2337's open question. Light/dark variants follow `prefers-color-scheme` unless the game pins one.
3. **CSS custom properties:** a documented set such as:
   - `--qv-font-body`, `--qv-font-ui`, `--qv-text`, `--qv-link`, `--qv-bg`, `--qv-surface`
   - `--qv-pane-bg`, `--qv-pane-fg`, `--qv-status-bg`, `--qv-status-fg`
   - `--qv-command-bg`, `--qv-radius`, `--qv-space`, `--qv-output-width`
   - …

   Authors override them in a game stylesheet. This is #2337 item 2.
4. **Game stylesheet plus documented parts:**
   - A stylesheet element in the game (#2337 item 1), injected on start *and* on reload/restore.
   - v2's stylesheets sit in cascade layers (`@layer qv.base, qv.theme, qv.game`), so author CSS always wins without `!important`.
   - Elements authors may target carry stable `data-qv-part` names (`output`, `command-bar`, `status-bar`, `pane`, `pane[data-qv-pane=inventory]`, `exits`, `picture-frame`, `map`, `menu`, …), and a short supported-selectors list documents them.
   - IDs and classes stay internal.
5. **Layout:** named regions (`header`, `output`, `below-output`, `side`, `drawer`) and presets:
   - **side panes:** today's desktop layout
   - **no panes:** the gamebook style
   - **bottom bar:** a mobile-first layout

   Options control which pane goes where and in what order, separately per breakpoint. For example, compass in the side column on desktop and as the exits strip below the output on phones (#2581's strip becomes a first-class region). Status attributes can be pinned to a status bar that stays visible on phones, with bar formats (`value/max`). That's #2337 item 3.
6. **Script hooks (`QuestUI` plus ASL wrappers):**
   - The supported versions of the survey's common hacks: `addPane`/`setPaneContent`, `setAutoScroll(false)`, `setContinueOnClick(true)`, `fadePreviousOutput`, `setCommandPlaceholder`, `setMapInteractive(false)`, and events (`turnStarted`, `turnEnded`, `outputAdded`, `locationChanged`).
   - Each also has a 610 Core function (`SetPaneVisible`, `AddPane`, `SetAutoScroll`, …), so most authors never write JS.
   - Custom panes can be plain HTML or a custom element the game's own JavaScript defines.

## Building it

**Stack (recommended):**
- TypeScript, compiled with esbuild to one ES module plus one CSS file per UI.
- Light-DOM custom elements for components (`<qv-output>`, `<qv-pane>`, `<qv-command-bar>`, …). Light DOM rather than shadow DOM, so game stylesheets and `data-qv-part` selectors reach them.
- No framework runtime. That keeps the bundle small, keeps the single-file export simple, and avoids tying a UI that must stay stable for years to a framework's major versions.
- Styling is hand-written CSS on the token set, not Tailwind utilities, because authors read and override this CSS. Colour values can come from the same Skeleton palette as the chrome.

Svelte 5, for consistency with AppShell, was considered and rejected (decision 2).

**Native platform features in place of the old plugins:**
- `<dialog>` for ShowMenu/Ask, which resolves #2006 for v2
- the Popover API for object verb menus, replacing jjmenu
- `<details>`, or an ARIA disclosure pattern, for collapsible panes, replacing the multi-open accordion
- SVG for the map (#2437)
- CSS container queries for pane layouts

**Where things live:** `src/WasmPlayer/ui-v2/` (source), built into the AppBundle beside the `legacy/` folder holding today's UI unchanged. `wasm-player.js` gains the bundle selection and a small adapter that routes the existing JS imports to whichever UI is loaded.

## Phases

**Phase 0: contract.** Write the 610 UI contract (the `QuestUI` surface, token list and part names) as a reference page, and open tracking issues. The draft is [ui-v2-contract.md](ui-v2-contract.md). The contract and the 610 Core rewrite onto it (with a legacy `QuestUI` adapter) must land before the first 6.1 beta ships. v2 itself needn't: 610 games play on legacy until Phase 5.

**Phase 1: plumbing and a minimal v2.**
- Lazy UI bundle selection in the host, plus the `?ui=` override.
- Move legacy into `legacy/` (with or after #2493).
- A `QuestUI` adapter in legacy, so 610 Core can call the contract while 610 games still play on legacy.
- The 610 Core rewrite onto `QuestUI`, which must be complete before the first 6.1 release.
- A v2 that plays a text game end to end: output, command bar, links and verb menus, wait/pause/continue, `<dialog>` menus and questions, clear screen, save/restore of the transcript.
- 610 games stay on legacy by default during this phase.

**Phase 2: layout and theming.**
- Panes (inventory, places and objects with verbs, status attributes, compass, custom status pane, command pane), the location bar and the picture frame.
- Responsive layouts and presets, the exits strip, and the pinned status bar with bar formats.
- Tokens, themes as token sets, and the game stylesheet element with editor support (#2337).

**Phase 3: the rest of the old feature set.** Map and drawing on SVG (#2437, built for v2 first, then dropped into legacy if wanted), media (sound, YouTube/Vimeo), background images, opacity and colour blend, web fonts, text effects (`TextFX_*`), output sections, the gamebook layout, and interface strings/i18n.

**Phase 4: the author surface.**
- The `QuestUI` hooks and their 610 Core wrappers.
- Editor UI for layout and pane placement, and a live theme/token preview in the editor's Play pane.
- Documentation under `site/src/content/docs/customise/` (rewrite `css.md`, `look-and-feel.md`, `panes.md`, `player-interface.md` and `javascript.md` for v2, keep the legacy pages, and add a migration guide from common `JS.eval` snippets to their v2 equivalents).
- The upgrade warning that flags legacy-UI hacks.

**Phase 5: parity and switch-over.**
- Port the WasmPlayer e2e scripts that cover behaviour (not legacy DOM details) to run against v2 via `?ui=v2`, and add v2-specific ones.
- Run the stock-only part of the corpus (about 97% of published games, identifiable with the survey's classifier) under `?ui=v2` as a dev-only parity check.
- When it's clean, make v2 the default for 610.

After switch-over, legacy is frozen: bug fixes and security fixes only, no new features.

## Testing

- The existing 37 `verify-wasmplayer-*` e2e scripts keep testing legacy unchanged. They're its regression suite once it's frozen.
- v2 gets its own `verify-wasmplayer-v2-*.mjs` scripts, wired into the `wasmplayer_chromium` job, plus v2 runs of the behavioural scripts.
- The quest-e2e-tests corpus (all pre-610) stays on legacy. Add a handful of 610 fixture games covering each pane, layout and customisation tier.
- Accessibility checks with axe-core in e2e. Visual checks at phone, tablet and desktop widths in light and dark.

## Decisions

Settled on 2026-10-22:

1. **What selects v2:** ASL 610 or later, with `game.classicinterface` as an opt-out. v2 only becomes the 610 default once it reaches parity (Phase 5); until then 610 games play on legacy. Gating on ASL 620, or on an attribute alone, was rejected.
2. **Stack:** TypeScript, light-DOM custom elements and esbuild, with no framework runtime. Svelte 5 was rejected, to keep the bundle small and the player free of a framework's upgrade cycle.
3. **Game stylesheet** (#2337's open question): a game-level stylesheet element stored in the `.aslx`, editable in the editor. It may also reference asset `.css` files.
4. **Stability:** the 610 UI contract is permanent. Published 610 games must never break on a future player, so the contract only gains additions. A breaking change would need a new ASL version with its own contract.
5. **Older games:** legacy stays legacy for now. No player-facing opt-in, since prompting players about interfaces isn't wanted.
6. **Whole-UI replacement:** out of scope. Authors get layout presets, custom panes, stylesheets and script hooks instead.

## Later

**v2 for known-compatible older games.** The survey's classifier can identify pre-610 games that only use stock Core's UI calls (about 97% of published games). Stock Core's UI surface is fully enumerated, so an adapter covering just that set is bounded. A future step could combine the classifier with an automated check (each game played under `?ui=v2`) to build a list of known-working game IDs, which the player then shows with v2 automatically, without asking the player anything. Not planned until v2 is the 610 default.

## Related issues

#2337 (stylesheet, tokens, phone status: folded in), #2359 (drawer styling: fixed by design in v2, small fix in legacy), #2006 (jQuery UI dialog), #2437 (map without paper.js), #2199 (picture backgrounds on the map), #2493/#2494/#2495 (static web assets), #1780 (embedding without HTML), #2581 (exits strip), #2577 (upgrade warning).
