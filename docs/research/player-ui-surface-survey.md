# Player-UI surface survey: what published games rely on

October 2026. This survey measures how much of the in-game player UI (`src/WasmPlayer/player-ui/`, inherited from Quest 5's `PlayerController/`) published games actually depend on. It's input to the question of replacing the jQuery/jQuery UI interface with a new one tied to a new ASL version. That only makes sense if a frozen legacy UI for existing games is small and bounded.

## Headline numbers

The corpus is every game published to textadventures.co.uk: **21,704 `.aslx` files** (21,589 distinct `<gameid>`s), all of which parsed. Most were saved by Quest 5.6 (6,661) or 5.8 (8,120).

| What the game does to the player UI | Games | Share |
|---|---:|---:|
| Touches it at all, through stock Core | 21,704 | 100% |
| Author-written `JS.*` call of any kind | 677 | 3.1% |
| Author-written `JS.eval` | 490 | 2.3% |
| Author code that reaches into the player DOM, its jQuery or its JS globals | 489 | 2.3% |
| Author `request()` calls (`Hide`/`Show` Command or Panes, etc.) | 315 | 1.5% |
| Ships its own `.js` file (`<javascript src>`) | 292 | 1.3% |
| Author code that **replaces player functions** (`scrollToEnd = function(){}`, `sendCommand = ...`) | 102 | 0.5% |
| Author code that **reads or writes player internals** (`_waitMode`, `_animateScroll`, ...) | 39 | 0.2% |
| Inline `<script>` in game output | 63 | 0.3% |

In short, the surface splits in two:

- **Stock Core's surface is large but finite and fully known.** Every game uses it, but only through the calls Core's own library makes. Those are enumerable from Core's history (see below), and the current player supports all but two of them: `#status` (already shimmed) and `RestartGame` (not shimmed; see below).
- **The author-written surface is a long tail across about 2–3% of games.** It's concentrated in about 20 element IDs and a handful of recurring intents. Only about 0.5% of games do things that need the *original code* rather than the original *IDs*.

## Method

[`extract.py`](player-ui-surface-survey/extract.py) parses each game and records every UI-facing occurrence, together with the top-level element (function, type, command, object, game) it appears in:

- `JS.*` calls
- `request(...)` statements
- `#id` and `.class` selectors and `getElementById` arguments inside `JS.*` string arguments
- jQuery and jQuery UI method calls
- player JS globals called from eval strings
- inline `<script>`/`<style>`
- `<javascript src>` references
- UI option flags on `<game>`

[`aggregate.py`](player-ui-surface-survey/aggregate.py) then classifies each occurrence:

- **core:** the whitespace-normalised call text appears in the current `src/Engine/Core` or Quest 5's final `WorldModel/WorldModel/Core`. Also, a call inside a Core-named element that at least 5 games share, which catches older Core versions. Translated interface strings inside `InitInterface` also count, since old Core versions baked each language's labels into it as literals.
- **library:** not in a Core-named element, but the same call in the same element appears in 20 or more games (shared third-party libraries).
- **override:** inside a Core-named element but not a stock call: the author edited a library function, or filled in an author hook like `InitUserInterface`.
- **author:** everything else.

The tables below fold "override" into "author", since both are author-written.

Each referenced ID, class and function is also checked against the current player UI files and Quest 5's (`PlayerController/*`, `Player/desktopplayer.js`). "Q5 only" means it existed in Quest 5 but is gone now. Selectors that match neither are the author's own elements (e.g. a `<div id="weapon-span">` they printed themselves), so they aren't player-UI dependencies and are left out.

## Stock Core's surface

This is what every game relies on just by having been saved with Core. It's the guaranteed minimum any legacy UI must support.

- **About 40 player JS functions called directly**, all present in the current player. For example:
  - `addText`, `SetMenu*`, `setCommandBarStyle`, `setGameWidth`, `setGamePadding`, `hideBorder`
  - `StartOutputSection`/`EndOutputSection`/`HideOutputSection`, `AddYouTube`/`AddVimeo`, `createNewDiv`
  - `SetBackgroundImage`/`SetBackgroundOpacity`, `addExternalStylesheet`
  - `ShowGrid`/`Grid_Draw*`, `uiShow`/`uiHide`, `setPanes`, `updateLocation`, `addScript`, `setInterfaceString`
- **About 15 element IDs, all addressed from `InitInterface`'s option handling:**
  - `txtCommand`, `txtCommandDiv`, `txtCommandPrompt`, `gameBorder`, `location`, `controlButtons`
  - the side-pane `inventory*`, `placesObjects*`, `statusVars*` and `compass*` label/accordion pairs
  - `commandPane`, `customStatusPane`
  - and `status`, the Quest 5 name for what is now `#qv-status`. 6,260 games carry it, and it's the one stock reference the current UI already has to shim (#2169).
- **jQuery used by stock Core:** `insertBefore` (the `statusandcompassattop` reordering, 6,225 games), `.html`, `.append`, `.css`, and one `.ui-widget-header` background-image rule (401 games).
- **`RestartGame`, which the current player doesn't define.** Quest 5's stock `restart` command, inlined into 4,941 games, runs `if(webPlayer){reload} else if (typeof(RestartGame) != 'undefined'){RestartGame()} else {addTextAndScroll('Try pressing CTRL+R.')}`. WasmPlayer sets `webPlayer = false` (`player.js`) and has no `RestartGame`. So in those games, `restart` followed by "yes" prints "Try pressing CTRL+R." instead of restarting, which is no help on a phone or in the desktop app. Current Core's `restart` reloads the page directly, so only already-published games are affected. Defining a `RestartGame()` in the player that calls WasmPlayer's own restart would fix all of them at once.
- **About 20 `request()` types**, mostly `Show`/`Hide` of Panes, Command and Location, plus `SetPanelContents`, `RequestSave`, `Pause`, `Background`/`Foreground`/`LinkForeground`, `RunScript` and `SetInterfaceString`.

This set can be enumerated mechanically, and it's already known: it's everything Core has ever emitted.

## The author-written surface

### Player element IDs authors touch directly

Only about 50 player IDs are referenced by author code at all, and the tail drops off fast.

| ID | Games (author) | Notes |
|---|---:|---|
| `txtCommand` | 136 | placeholder text, focus, border styling |
| `txtCommandDiv` | 95 | hide/show, positioning, centring |
| `gamePanes` | 93 | hide, resize |
| `gameBorder` | 82 | background, borders |
| `divOutput` | 81 | min-height, fading old output |
| `inventoryLabel` / `inventoryAccordion` | 72 / 43 | hiding the inventory pane |
| `compassAccordion` / `compassLabel` | 68 / 64 | hiding the compass |
| `status` (Q5 only) | 59 | location-bar colours |
| `placesObjectsLabel` / `placesObjectsAccordion` | 35 / 26 | |
| `gridPanel`, `gamePanesRunning`, `endWaitLink`, `gameContent` | 31–39 each | |
| `inventoryWrapper`, `lstInventory`, `placesObjectsWrapper`, `location`, `statusVars*` | 15–27 each | custom pane behaviour |
| `gamePanelSpacer` (Q5 only), `gridCanvas`, `gamePanel`, `customStatusPane`, `commandPane` | 11–21 each | |
| `cmdCompass*` (each direction) | 2–8 each | custom compasses |
| everything else | 1–5 each | |

Player CSS classes are rarer still: `elementListWrapper` (33 games), `cmdlink`/`commandlink` (about 19 and 15), the jQuery UI accordion and button classes (under 20 each), then single digits.

### What authors are trying to do

The 2,912 distinct author `JS.eval` strings cluster into a few recurring intents. The commonest, by games:

- **Hide or reshape the panes:** hide the compass or inventory, resize `#gamePanes`, insert an extra accordion section (a "map" pane, 12 games).
- **Restyle the command bar:** placeholder text, borders via `getCSSRule`, fixed positioning, centring.
- **Change how output behaves:** fade or disable earlier output, disable old command links, stop autoscroll (`scrollToEnd = function(){}`, 39 games), force scrolling.
- **Click or keypress to continue a wait:** a window handler that checks `_waitMode` and calls `endWait()` (about 30 games).
- **Media:** play, pause or remove `<audio>`/`<video>` by ID. These are mostly the author's own elements, not player DOM. One shared audio helper accounts for 18 games of `JS.*` calls to functions defined in a bundled `.js` file.
- **Custom status/command panes:** `setCustomStatus` (44 games) and `setCommands` (68) called directly. Both are current player functions with no Core wrapper, which authors evidently found and used.
- **Stop map interaction:** `gridApi.zoomIn = function(){}` and turning off paper.js drag (20 games each).

### The parts only the original code can satisfy

- **102 games (0.5%) replace player functions.**
  - Mostly the status/list pipeline: `updateList`, `updateVerbButtons`, `updateStatus`, `setCustomStatus`, `showStatusVisible` and `updateStatusVisibility`, each replaced in about 21–24 games, apparently one shared snippet.
  - Then `scrollToEnd` (39), `sendCommand` (18) and `gridApi.zoomIn` (20).
- **39 games (0.2%) read or write internal state:** `_waitMode` in 33 of them, `_animateScroll` in 5.

Keeping element IDs alive over new components doesn't help these games. They need the old functions with the old names and old calling conventions.

### Bundled JavaScript

292 games (1.3%) reference `<javascript src>` files. The corpus has only the `.aslx`, not the files, so their contents aren't surveyed here. 62 of them are one shared library (`DevMode/debugtable.js` + `tabulator.min.js`). The rest are mostly one-off names like `clearScreen.js`, `hidepanes.js`, `hideCompass.js` and `InvPane2.js`, which suggest the same intents as above.

### By engine version

The share of games with author code in the player DOM rises with each Quest 5 release:

| Saved by | Games | Author DOM/jQuery | Any author `JS.*` |
|---|---:|---:|---:|
| 5.0–5.3 | 569 | 0.0% | 0.0% |
| 5.4 | 1,451 | 0.7% | 1.6% |
| 5.5 | 2,092 | 1.8% | 2.8% |
| 5.6 | 6,661 | 0.9% | 1.2% |
| 5.7 | 2,171 | 2.6% | 3.3% |
| 5.8 | 8,120 | 3.4% | 4.7% |
| 5.9 | 585 | **8.5%** | **10.4%** |
| 5.10 | 55 | 1.8% | 1.8% |

The 5.9 jump comes from a much smaller sample, but the direction is consistent: as authors get more ambitious they reach for the DOM, because nothing else lets them change the layout.

### UI options authors set on `<game>`

These are the sanctioned route, handled by Core. The most used options:

- `gridmap` (2,211 games)
- `showborder` (1,619)
- `showpanes` (1,121)
- `feature_pictureframe` (980)
- `showcommandbar` (871)
- `setcustomwidth` (738)
- `setcustompadding` (632)
- `turnoffcompass` (629)
- `showlocation` (617)
- `turnoffplacesandobjects` (506)
- `statusandcompassattop` (302)
- `turnoffinventory` (269)

Every one is an *intent* rather than a DOM operation. Several of the author hacks above, such as hiding the compass or inventory, duplicate options that already exist.

## What this means for the revamp

1. **A frozen legacy UI is a bounded thing.** Stock Core's surface is fully known, and the author tail is about 50 IDs and a few dozen functions. Keeping today's `player-ui` frozen for games saved before a new ASL version costs maintenance on a fixed target, not an open-ended one.
2. **A compatibility layer over new components wouldn't let the old UI go.** Keeping the stock IDs and functions alive over a new UI would cover every stock call and most author CSS tweaks, but not the 0.5–1% of games that replace player functions or read `_waitMode`. Those need the original code verbatim. So the frozen legacy UI stays either way, and a compatibility layer would be a second legacy UI rather than a replacement.
3. **The new protocol needs sanctioned hooks for the common hacks**, or authors will hack the new UI the same way. Judging by the tail, that means:
   - hiding and reordering panes
   - adding custom panes
   - styling the command bar
   - turning autoscroll off
   - click-to-continue on waits
   - fading or disabling old output
   - turning map interaction off
   - custom status and command panes

   That's the same list as the UI options on `<game>`, plus about five more.
4. **`request()` already looks like a protocol.** Author uses of it (`Hide`/`Show` Command or Panes) are intent-level and would map directly onto messages.

## Caveats

- **Static extraction only.** Selectors built at runtime (`"#" + id`) are invisible, as is anything inside bundled `.js` files (1.3% of games).
- **The stock/author split is a heuristic.** It's based on current and Quest 5-final Core text plus frequency thresholds. Rare Core versions or pre-release builds can leak into "author" (judging by spot checks, a few dozen games at most). A shared library used by fewer than 20 games counts as "author".
- **Published games only.** Unpublished works in progress and private games aren't in the corpus. About 115 uploads share a `<gameid>` with another (re-uploads), which barely affects the percentages.
- **No game content is quoted.** Snippets above are short UI one-liners, with no game names or text.

To reproduce, using any working directory for the output:

```bash
python3 -I docs/research/player-ui-surface-survey/extract.py ~/aslx_files /tmp/all.jsonl
python3 -I docs/research/player-ui-surface-survey/aggregate.py /tmp/all.jsonl . ~/Code/quest5 /tmp/result.json
```
