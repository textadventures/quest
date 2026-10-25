# The 610 UI contract

Phase 0 of [#2584](https://github.com/textadventures/quest/issues/2584). See [ui-v2-plan.md](ui-v2-plan.md) for the plan this belongs to.

**Status: agreed, not yet frozen.** The open questions were settled on 2026-10-23 (see Decisions). Members can still be added or adjusted while the Core rewrite is built. The contract freezes with the first 6.1 release, the first point at which a 610 game can be published (see "When it freezes" below). From then on it only ever gains additions.

The contract is what 610 Core and 610 authors may rely on. It has three parts:

1. **`QuestUI`**, a named JS API.
2. **CSS custom properties** (`--qv-*`).
3. **Part names** (`data-qv-part`), plus the output markup attributes that links carry.

Everything else in a UI's DOM, CSS and JS is internal and may change. That includes the host adapter that maps `IPlayer` and the engine's JS imports onto a UI: games never call it directly, so it isn't part of the contract.

## Findings that change the plan

Surveying current Core turned up three things the plan doesn't yet account for.

### 1. No 6.1 release before the 610 Core rewrite

`v610` is already the editor's save version, but no 6.1 release has shipped (6.1.0-beta.1 is pending as #2446). Today's `InitInterface`:

- calls `JS.setCss` on legacy IDs (`#compassAccordion`, `#qv-status`, `#txtCommandDiv`, …)
- calls jQuery directly through `JS.eval` (`$('#commandPane').insertBefore('#inventoryLabel')`, `$('.ui-widget-header').css(...)`)
- injects `<style>` rules for `#txtCommand` through `JS.addScript`

A 610 `.quest` package inlines whatever Core it was published with. If any 6.1 release shipped before the rewrite, packages published from it would carry these calls. Under decision 1 (v2 for 610 or later), they'd load v2 and none of their interface options would apply. The same goes for every intermediate state of a rewrite done in stages: each published snapshot would add its own calls to what v2 must support for good. (Plain `.aslx` source isn't affected, because it references Core live.)

**So decision 1 stands as written, with one release rule: no 6.1 release, beta included, ships until the whole 610 Core rewrite has landed.** Until then nobody can publish a 610 game, so 610 Core can change freely. This fits the plan to hold the first beta until most of 6.1's features are in anyway. The open release PR (#2446) must not be merged before then.

### 2. Legacy needs a `QuestUI` implementation too

610 games play on legacy until Phase 5, and after it any game with `classicinterface` still plays on legacy. Once 610 Core goes through `QuestUI`, legacy has to implement it: a thin adapter over its existing functions (`setPaneVisible("compass", false)` → `$("#compassLabel, #compassAccordion").hide()`, and so on). Phase 1 should include it.

So "legacy is frozen" means its *existing* behaviour is frozen. Legacy still gains the `QuestUI` adapter, and that adapter follows the contract's additions. Where an addition has no sensible legacy equivalent (e.g. a layout preset), the legacy adapter accepts the call and does nothing. That's allowed, because contract calls describe intent, not guaranteed visuals.

This settles a question the plan leaves open: a future Core version (say 620) can add calls, and a 620 game with `classicinterface` still works on legacy.

### 3. Most link markup is already delegated

The plan originally proposed new `data-qv-object` / `data-qv-command` attributes. Current Core already emits delegated markup with no inline handlers for object, exit and command links:

```html
<a class="cmdlink elementmenu" data-elementid="lamp">lamp</a>
<a class="cmdlink exitlink" data-elementid="north" data-command="go north">north</a>
<a class="cmdlink commandlink" data-elementid="…" data-command="…" data-deactivateonclick="true">…</a>
```

**Decided:** adopt these existing names (`data-elementid`, `data-command`, `data-deactivateonclick`, plus the `elementmenu`/`exitlink`/`commandlink` classes) as the contract's link markup rather than inventing a parallel `data-qv-*` set. Legacy already handles them, and they're already in every 6.0 game. What's left is removing the inline handlers that remain:

| Source | Today | 610 |
|---|---|---|
| `ShowMenu` non-inline fallback (`CoreFunctions.aslx`, `GamebookCore.aslx`) | `onclick="ASLEvent('ShowMenuResponse','…')"` | `data-qv-event="ShowMenuResponse" data-qv-param="…"` |
| `{popup:}` text processor | jQuery in inline `onclick`, plus an inline-styled `position:fixed` div | `<button data-qv-popup="id">` + a `<div data-qv-popup-content id>` that the UI shows as a popover |

`data-qv-event` and `data-qv-popup` are the only new attribute names. Both UIs handle all of these through delegated listeners.

`PlayerHelper` also writes inline handlers (`onclick="sendCommand('…')"` and `goUrl('…')`) for the links in its XML output. That path isn't part of 610: `WorldModel.PrintAsync` sends every game of version 540 or later that has Core's `OutputText` through the text processor instead, so only pre-540 and Quest 4 games reach `PlayerHelper`'s links, and they stay on the legacy UI.

## Today's surface (current Core)

This is the checklist the contract has to cover. It's everything current Core (Core, CoreOutput, CoreFunctions, CoreGrid, CoreEffects, CoreTimers, CoreScopes, CoreStatusAttributes, CoreCommands, CoreParser, CoreDescriptions, CoreTypes and GamebookCore) sends to the UI. Editor-only libraries are excluded. The survey's numbers (about 40 functions, 15 IDs, 20 request types) cover every Core version ever published. Current Core alone uses 73 distinct `JS.*` names (the Grid and transcript families account for most of the difference), 7 `request` types and 15 IDs.

### `JS.*` calls, by area

| Area | Calls | From |
|---|---|---|
| Output | `addText`, `clearScreen`, `createNewDiv`, `scrollToEnd`, `StartOutputSection`, `EndOutputSection`, `HideOutputSection`, `disableMenuOutputSection`, `disableAllCommandLinks`, `getDivCount` | OutputTextRaw, ClearScreen, SetAlignment, SetTimeoutID, StartGame, output-section functions, gamebook HandleCommand |
| Link state | `updateObjectLinks`, `updateExitLinks`, `updateCommandLinks` | UpdateObjectLinks |
| Layout and panes | `panesVisible`, `uiShow`, `uiHide`, `setCss`, `hideBorder`, `setGameWidth`, `setGamePadding` | InitInterface, gamebook GetInput |
| Theming | `setBackground`, `colourBlend`, `setPanes`, `SetMenuBackground`/`Foreground`/`HoverBackground`/`HoverForeground`/`FontName`/`FontSize`, `TurnOffHyperlinksUnderline`, `SetBackgroundImage`, `SetBackgroundOpacity`, `addExternalStylesheet`, `addScript` (only to inject `<style>`) | InitInterface, CoreOutput setters |
| Command bar | `setCommandBarStyle` | ResetCommandBarFormat |
| Status, location, compass | `updateStatus`, `updateLocation`, `setCompassDirections`, `setInterfaceString` | UpdateStatusAttributes, OnEnterRoom, InitInterface |
| Picture frame | `setPanelContents` | SetFramePicture, ClearFramePicture |
| Media | `AddYouTube` | ShowYouTube, gamebook DoPage |
| Text effects | `TextFX.Typewriter`, `TextFX.Unscramble` | TextFX_* |
| Map | `ShowGrid`, `respondToGridClicks`, `Grid_SetScale`, `Grid_SetCentre`, `Grid_DrawBox`, `Grid_DrawLabel`, `Grid_DrawPlayer`, `Grid_DrawLine`, `Grid_DrawArrow`, `Grid_DrawSquare`, `Grid_DrawShape`, `Grid_AddNewShapePoint`, `Grid_DrawImage`, `Grid_DrawSvg`, `Grid_LoadSvg`, `Grid_DrawGridLines`, `Grid_ShowCustomLayer`, `Grid_ClearCustomLayer`, `Grid_ClearAllLayers` | CoreGrid, InitInterface, ChangePOV |
| Transcript | `writeToTranscript`, `SaveTranscript`, `enableTranscript`, `disableTranscript`, `killTranscript`, `reviveTranscript`, `printTranscriptDateAndTime`, `replaceTranscriptString`, `loadTranscriptNameInputVal`, `showTranscript` | parser, transcript commands and functions, defaultgame |
| Raw `JS.eval` | `var gameName…; var transcriptName…`, `var savingTranscript = true`, `transcriptEnabled = false`, `transcriptName = '…'`, six jQuery `insertBefore`s, `.ui-widget-header` background image, `#txtCommandPrompt` HTML, and restart's `RestartGame()`/`location.reload()` | InitInterface, StartGame, InitiateTranscript, restart |

### Element IDs

All from `InitInterface`, except `txtCommandDiv`, which gamebook `GetInput` also uses: `txtCommand`, `txtCommandDiv`, `txtCommandPrompt`, `gameBorder`, `location`, `qv-status`, `commandPane`, `customStatusPane`, and the label/accordion pairs `inventory*`, `placesObjects*`, `statusVars*`, `compass*`. One class, `.ui-widget-header`.

### `request` types

Core itself uses 7: `Show`, `Foreground`, `LinkForeground`, `Log`, `Pause`, `Wait`, `RequestSave`. But `request` is an ASL script command that authors use directly, so v2 has to handle the engine's whole `Request` enum (23 types, `src/Engine/Scripts/RequestScript.cs`). Most already map onto typed `IPlayer` members. The exceptions are `RunScript` (arbitrary JS, so not part of the contract) and `Show`/`Hide`, whose element names (`Panes`, `Location`, `Command`) become part names.

### `IPlayer` and the host adapter

`IPlayer` (30 members, `src/Common/IGame.cs`) plus the `UpdateList` event are the engine's typed channel. WasmPlayer turns them into about 30 JS imports (`addTextAndScroll`, `showMenu`, `updateList`, `updateCompass`, `bindMenu`, `beginWait`, `setTurnPending`, …). Each UI implements the same adapter interface. It's internal: games can't call it, so it can change freely, and none of it is in the contract below.

### Engine-bound calls from the UI

Author JS calls two player globals to talk to the engine: `ASLEvent(name, param)` and `sendCommand(text)`. They're the most-used player functions in author code, so they belong in the contract (as `QuestUI` members, with the globals kept as aliases in both UIs).

## `QuestUI`

A global object, defined by both UIs before any game script runs. 610 Core calls it as `JS.QuestUI.<member>(…)`, which `RunScriptAsync` already supports for dotted names (Core uses `JS.TextFX.Typewriter` today).

Arguments are strings, numbers, booleans, and ASL lists and dictionaries, which arrive as JSON arrays and objects. Lists and dictionaries may hold further lists and dictionaries; an object reference inside one arrives as the object's name. (WasmPlayer's bridge serialises these recursively for `QuestUI.*` calls. Any other `JS.*` call still gets only flat string lists and string dictionaries as JSON, and anything else as its `ToString()`, which is what published games' own JS functions were written against.)

Rules that apply to every member:
- **Unknown names are ignored, not errors.** An unknown pane, part, token or option does nothing. That lets a newer Core pass something an older adapter doesn't know, and lets legacy no-op what it can't do.
- **Calls describe intent.** `setVisible("compass", false)` means "the player shouldn't see the compass", not "set `display:none` on a node". A layout may satisfy it differently (e.g. by hiding the exits strip on a phone too).
- **Additions only.** Members, parameters (trailing and optional only), part names, token names and event names can be added. Nothing is removed or changes meaning.

### Engine

| Member | Replaces |
|---|---|
| `sendCommand(text)` | the `sendCommand` global (kept as an alias) |
| `sendEvent(name, param)` | the `ASLEvent` global (kept as an alias) |
| `restart()` | restart's `JS.eval` and the `RestartGame` global |

### Output

| Member | Replaces |
|---|---|
| `addText(html)` | `addText` |
| `clearScreen()` | `clearScreen` |
| `setAlignment(align)` | `createNewDiv` |
| `startSection(name)`, `endSection(name)`, `hideSection(name)` | `StartOutputSection`, `EndOutputSection`, `HideOutputSection` |
| `disableSection(name)` | `disableMenuOutputSection` |
| `disableCommandLinks()` | `disableAllCommandLinks` |
| `updateLinks(kind, data)` with `kind` = `object`, `exit` or `command` | `updateObjectLinks`, `updateExitLinks`, `updateCommandLinks` |
| `scrollToEnd()` | `scrollToEnd` |
| `addYouTube(id)`, `addVimeo(id)` | `AddYouTube`, `AddVimeo` |
| `textEffect(kind, …)` with `kind` = `typewriter` or `unscramble` | `TextFX.Typewriter`, `TextFX.Unscramble` |

`textEffect`'s arguments after `kind` are TextFX's own: `typewriter` takes `text, speed, font, color, size` and `unscramble` takes `text, speed, reveal, font, color, size`, with `speed` in milliseconds and `size` in points. An empty `font`, `color` or `size` leaves it as the rest of the game's text has it, and 610 Core passes the game's starting ones that way (#2627, decision 4). Markup in `text` is dropped.

`getDivCount` is dropped: it exists only to keep legacy's div IDs in step after a restore, which the adapter can do itself.

`addText` inserts its HTML as markup. A `<script>` element in it doesn't run on v2, though legacy runs it (jQuery's `append` does). A 610 game runs JavaScript through `JS.eval` or its own script files.

### Layout and panes

| Member | Replaces |
|---|---|
| `setVisible(part, visible)` | `uiShow`/`uiHide`, `panesVisible`, `hideBorder`, and the `setCss(…, "display:…")` calls |
| `setPaneOrder(parts)` | the six `insertBefore` evals (`statusandcompassattop`) |
| `setLayoutPreset(name, narrow)` with `sidepanes`, `nopanes` or `bottombar` (see "Layout presets" below), or `gamebook` | GamebookCore's `panesVisible(false)` and its `uiHide`s of the location and command bar. Legacy hides those parts, as today. v2 gives gamebooks their own layout |
| `setPictureFrame(html)` | `setPanelContents` |
| `setInterfaceString(name, text)` | `setInterfaceString` (same names: `InventoryLabel`, `StatusLabel`, `PlacesObjectsLabel`, `CompassLabel`, `InButtonLabel`, `OutButtonLabel`, `EmptyListLabel`, `NothingSelectedLabel`, `TypeHereLabel`, `ContinueLabel`) |
| `setCompassDirections(list)` | `setCompassDirections` |
| `setLocation(text)` | `updateLocation` |
| `setStatusAttributes(items)` | `updateStatus` (see below) |

**Status attributes become structured.** Before 610, `UpdateStatusAttributes` joined pre-formatted strings with `<br/>`. For #2337 item 3 (a pinned phone status bar with `value/max` bars), 610 Core passes items instead: `[{ "name": "health", "value": 7, "max": 10, "pinned": true, "text": "Health: 7%" }]`.

- `text` is the formatted string as before, so a UI that doesn't draw bars just shows it.
- `value` is sent only when the attribute is a number.
- `max` and `pinned` come from two game-wide settings, keyed by attribute name, on the editor's Attributes tab:
  - `game.statusattributemaximums` (a string dictionary) gives an attribute a maximum: a number, or the name of another attribute of the same element (`maxhealth`). `max` is sent only when that resolves to a number.
  - `game.pinnedstatusattributes` (a string list) keeps an attribute in view on a small screen: `pinned` is `true` for those, and absent otherwise.

v2 draws an attribute with a `value` and a positive `max` as a bar, and shows the pinned ones in the `status-bar` part on a narrow layout. Legacy shows `text` only.

### Layout presets

`setLayoutPreset(name, narrow)` chooses where the panes go. `name` is the preset for every width, unless `narrow` gives another for a narrow player (under 900px wide):

| Preset | Wide | Narrow |
|---|---|---|
| `sidepanes`, the default | the panes in a column beside the game | the panes in a drawer opened from the top bar, with the exits strip under the output and the pinned status bar under the top bar standing in for the compass and status panes |
| `nopanes` | no panes, and no exits strip or pinned status bar | the same |
| `bottombar` | the panes in a sheet opened from a button beside the command bar, with the exits strip and command bar kept in view at the bottom, and the pinned status bar under the top bar | the same |

610 Core's `InitInterface` passes two `game` attributes: `layoutpreset`, and `narrowlayoutpreset` for a narrow player when it differs. A game with neither gets `sidepanes` at both widths. `gamebook` (GamebookCore's own) ignores `narrow`.

The presets place the panes region in one of the layout's named regions: `header` (the top bar, picture frame and pinned status bar), `output`, `below-output` (the exits strip and command bar), `side` or `drawer` (`bottombar`'s sheet is a drawer from the bottom). Region names aren't part of the contract yet: placing individual panes in them can be added later.

Legacy has one layout for every width, so it hides the panes for `nopanes` at both widths and otherwise ignores the call.

### Theming

| Member | Replaces |
|---|---|
| `setTokens({ "--qv-…": value, … })` | every styling call in `InitInterface`: `setCss` on `body`/`#gameBorder`/`#qv-status`, `setGameWidth`, `setGamePadding`, `setPanes`, `SetMenu*`, `TurnOffHyperlinksUnderline`, `colourBlend`, `setBackground`, `SetBackgroundImage`, `SetBackgroundOpacity`, the `.ui-widget-header` eval |
| `setCommandBarStyle(css)` | `setCommandBarStyle` (now applied to the `command-bar` part) |
| `setCommandBarPreset(name)` with `borderless` or `shadow` | the `addScript("<style>#txtCommand …")` injections |
| `setCommandPrompt(text)` | the `#txtCommandPrompt` HTML eval |
| `addStylesheet(url)` | `addExternalStylesheet` |

`setTokens` sets each name with `style.setProperty` on the UI root, so values can't inject markup. Names must start with `--qv-` or be an author's own custom property (`--` prefix). One call carries all of `InitInterface`'s options, so they apply in a single style recalculation.

### Map

`QuestUI.map` mirrors today's Grid functions one for one, with the same arguments, so 610 CoreGrid changes only in what it calls: `show(size)`, `respondToClicks(on)` (`respondToGridClicks`: clicking a square sends `JS_GridSquareClick`), `setInteractive(on)` (zooming and dragging the map, on by default), `setScale`, `setCentre`, `drawBox`, `drawLabel`, `drawPlayer`, `drawLine`, `drawArrow`, `drawSquare`, `drawShape`, `addShapePoint`, `drawImage`, `drawSvg`, `loadSvg`, `drawGridLines`, `showCustomLayer`, `clearCustomLayer`, `clearAllLayers`. v2 draws these with SVG (#2437). The arguments are frozen exactly as today's (grid coordinate units, same order, same colour and border strings), so #2437 implements them in SVG rather than redesigning them. A cleaner, SVG-native API can be added alongside later.

On v2 (#2627, decisions 1–3), the map is the `map` part, under the picture frame and `show`'s `size` pixels high (at most 40% of the height on a narrow layout, where the player can collapse it). It keeps what 610 CoreGrid relies on: z-levels with every level but the current one faded to 0.2, drawing calls going to the current level or to the custom layer while that's shown, reused ids replacing their item, and the map centring on the first room and following the player's dot. Three things differ from legacy, which has bugs there: the custom layer is an overlay above the map rather than a replacement for it (drawing a room doesn't hide it), the whole map pans together, and positions and sizes aren't truncated to whole squares. An SVG passed to `loadSvg` has its scripts and event handler attributes removed.

**Room pictures (#2199).** `drawBox` takes a tenth argument, `image`: the URL of the room's picture, from its `grid_image` attribute through `GetFileURL`, or `""` for none. 610 CoreGrid always passes it. v2 draws it over the room's fill, cropped to the room, under its border and label. Legacy ignores it.

### Transcript

The transcript belongs to the host (the start screen and transcript viewer are shared by both UIs), but today it's driven from `playercore.js`'s globals and seeded through `JS.eval`'d variables. 610 Core calls `QuestUI.transcript.*` instead: `write(html)`, `enable(name)` (`name` optional), `disable()`, `kill()`, `revive()`, `setName(name)`, `setPrefix(text)` (from `replaceTranscriptString`, which has never done anything since Quest 5.8, so neither UI does anything with it), `stampDateAndTime()`, `promptForName()` (offers the transcript's name in the command bar), and `show(message, linkText)` (a link to the transcript viewer, with Core's translated message and link text). The game name no longer needs seeding: the host already gets it from `IPlayer.UpdateGameName`.

The host keeps the transcripts (`WebPlayer.transcript` in `wasm-player.js`, #2627 decision 5), in the same `questtranscript-<name>` localStorage keys, or Electron's own store, as before. v2's `QuestUI.transcript.*` forward to it, and v2 copies its output into it while recording, as legacy does. Legacy keeps its own on/off state and text conversion, and stores through the host.

### Author hooks

These have no Core caller today. They're the supported versions of the survey's common hacks, so they're part of the first contract:

| Member | Instead of |
|---|---|
| `addPane(id, title, html)`, `setPaneContent(id, html)` | inserting accordion sections by hand; `setCustomStatus` (custom status pane) |
| `setCommands(list)` | `setCommands` (command pane). Each entry is `Label`, or `Label:command` when the command differs from its label; the command is sent in lower case. A `;`-separated string, as legacy took, also works |
| `setAutoScroll(on)` | `scrollToEnd = function(){}` |
| `setContinueOnClick(on)` | a window handler that checks `_waitMode` and calls `endWait()` |
| `fadePreviousOutput(on)` | fading `#divOutput` children |
| `setCommandPlaceholder(text)` | setting `#txtCommand`'s placeholder |
| `setMapInteractive(on)` | `gridApi.zoomIn = function(){}` and turning off paper.js drag (same as `map.setInteractive`) |
| `on(event, fn)` / `off(event, fn)` with `turnStarted`, `turnEnded`, `outputAdded` (`{ html }`), `locationChanged` (`{ location }`) | wrapping player functions to observe them |

A **turn** is one engine round trip the player starts: a command, an event (`sendEvent`, a `data-qv-event` link), or answering a wait, pause, menu or question. `turnStarted` fires before the input goes to the engine and `turnEnded` once the engine has finished with it and its output is on screen. Timer ticks aren't turns.

Each gets a 610 Core wrapper (`SetPaneVisible`, `AddPane`, `SetAutoScroll`, …) in Phase 4, so most authors never write JS.

## CSS custom properties

Every game option that `InitInterface` applies today maps onto a token. 610 `InitInterface` reads the game's attributes and passes them in one `setTokens` call (a string dictionary). Themes (`theme_novella`, …) are types that set those attributes, so they reach the UI as token sets the same way, and authors override any of them in the game stylesheet.

Values are CSS values, as a stylesheet would write them: colours as any CSS colour, lengths with units (`700px`, `12pt`), images as `url("…")`, and `--qv-bg-opacity` as a number from 0 to 1. A bare number for a length is read as pixels. An empty value unsets the token.

| Token | From |
|---|---|
| `--qv-font-body`, `--qv-font-size` | `defaultfont` / `defaultwebfont`, `defaultfontsize` |
| `--qv-text`, `--qv-bg`, `--qv-link` | `defaultforeground`, `defaultbackground`, `defaultlinkforeground`, only once one differs from the editor's defaults (see "Light and dark") |
| `--qv-link-decoration` | `underlinehyperlinks` |
| `--qv-page-bg` | `marginscolour` |
| `--qv-bg-image`, `--qv-bg-opacity` | `backgroundimage`, `backgroundopacity` |
| `--qv-bg-blend-top`, `--qv-bg-blend-bottom` | `colourblendtop`, `colourblendbottom` |
| `--qv-output-width` | `customwidth` |
| `--qv-padding-top`, `-bottom`, `-left`, `-right` | `custompadding*` |
| `--qv-border-colour` | `bordercolour`. `showborder = false` is `setVisible("border", false)`, not a token |
| `--qv-pane-fg`, `--qv-pane-bg`, `--qv-pane-heading-fg`, `--qv-pane-heading-bg`, `--qv-pane-highlight` | `panestheme` (the seven named themes become token sets) |
| `--qv-status-fg`, `--qv-status-bg`, `--qv-status-border`, `--qv-status-image` | `customlocationtextcolour`, `customlocationcolour`, `customlocationbordercolour`, `locationbarimage` |
| `--qv-menu-fg`, `--qv-menu-bg`, `--qv-menu-hover-fg`, `--qv-menu-hover-bg`, `--qv-menu-font`, `--qv-menu-font-size` | `menuforeground`, … `menufontsize`. The four colours only once one differs from the editor's defaults |
| `--qv-command-fg`, `--qv-command-bg` | command bar format |

v2 also defines tokens with no game attribute behind them yet (`--qv-surface`, `--qv-radius`, `--qv-space`, `--qv-font-ui`, focus-ring colours). Those are in the contract too, and the editor gains options for them later.

### Light and dark

A game that keeps the editor's default colours (black text, blue links, a white background) gets v2's own colours, which follow the player's `prefers-color-scheme`. So 610 Core sends colour tokens only once a game has changed one (#2613, decision 5):

- **Text, links and background go together.** Once any of `defaultforeground`, `defaultlinkforeground` or `defaultbackground` differs from its default (text and links as the game started, the background as it is now), or the game sets a background opacity, Core sends all three. A game never gets its own text colour on the scheme's background. A mid-game `SetBackgroundColour` sends the text colours with it for the same reason.
- **So do the menu colours:** all four, once one differs.
- **The command bar** leaves its colour and background out while the game's text and background colours are the defaults.

Any other colour an author sets (location bar, margins, border, pane theme) is sent as before. Legacy's own defaults match the editor's, so leaving the tokens out changes nothing there.

v2's defaults, light and dark (`--qv-surface`, `--qv-page-bg` and `--qv-border-colour` are mixed from the text and background, so they also suit a game's own colours):

| Token | Light | Dark |
|---|---|---|
| `--qv-text` | `#1a1a1a` | `#e4e4e7` |
| `--qv-bg` | `#ffffff` | `#18181b` |
| `--qv-link` | `#1d4ed8` | `#93c5fd` |
| `--qv-surface`, `--qv-page-bg` | 5% text over the background | the same |
| `--qv-border-colour` | 20% text over the background | the same |
| `--qv-status-fg` / `--qv-status-bg` | `#ffffff` / `#3f3f46` | `#ffffff` / `#27272a` |
| `--qv-pane-fg`, `--qv-pane-heading-fg`, `--qv-menu-fg` | `--qv-text` | the same |
| `--qv-pane-bg`, `--qv-pane-heading-bg` | `--qv-surface` | the same |
| `--qv-menu-bg` / `--qv-menu-hover-bg` / `--qv-menu-hover-fg` | `--qv-bg` / `--qv-link` / `--qv-bg` | the same |
| `--qv-pane-highlight` | `#f59e0b` | the same |
| `--qv-focus-ring` | `#2563eb` | `#60a5fa` |

The built-in themes and pane themes keep WCAG AA contrast, except `theme_hotdogstand`'s yellow on red, which is the point of it. 610 Core's Nature pane theme draws its headings in `#254117` rather than legacy's `#306754`, which was too faint on its heading colour.

### Inline output styles

`OutputTextRaw` wraps every line in `<span style="font-family:…;color:…;font-size:…">`, taken from `defaultfont`, `defaultforeground` and `defaultfontsize`. Inline styles beat any token, so a theme, a light/dark variant or an author stylesheet couldn't change the text colour or font of output that's already there.

**610 Core leaves the defaults out.** The first time `InitInterface` runs, it records the game's starting font, size, text colour and link colour in `game.uidefaults`, which is kept in saves. Those values become the `--qv-font-body`, `--qv-font-size`, `--qv-text` and `--qv-link` tokens (the colours only as "Light and dark" describes). `GetCurrentTextFormat` and the link formats leave a property out when it equals the recorded value, so ordinary output and links inherit from the tokens.

- A mid-game `SetForegroundColour`, `SetLinkForegroundColour`, `SetFontName` or `SetFontSize` differs from the recorded value, so it's written inline exactly as before, and only affects text from then on.
- After a restore, the recorded values still come from the save, so earlier text keeps inheriting the starting colour.
- The command bar's style is always written in full, because a text input doesn't inherit the page's font, except for the default colours.

## Game stylesheet

A 610 game's own CSS is its `stylesheet` attribute, a string on `game`, edited as CSS under Advanced on the game's Interface tab (#2613, decision 6). It's saved and packaged like any other attribute, and like any other a script can change it, though it's only reapplied on the next start or restore.

The host applies it, not the UI and not Core, so it needs no `QuestUI` call:

- On every `Initialise` (start, restart and restoring a save), the host sets it as one `<style id="qv-game-stylesheet">` at the end of `<head>`, after the player UI's own styles, and removes it for a game without one.
- It isn't in a cascade layer, so on v2 (whose CSS is all in `qv.*` layers) the author's rules win over v2's whatever their specificity, without `!important`. On legacy, whose CSS isn't layered, it wins on order at equal specificity.
- It's read only from 610, since an older game may use the attribute name for something of its own (`IGame.GetGameStylesheet`).

What it may rely on is the rest of this contract: the `--qv-*` tokens and the `data-qv-part` names. Anything else in a UI's DOM is internal.

## Part names

Elements carry `data-qv-part`. Authors may select on part names (and on `data-qv-pane` for a specific pane), and `setVisible`/`setPaneOrder` take them. IDs and classes stay internal. Both UIs carry them. Legacy has no element wrapping a whole pane, so there the part name is on the pane's content, not its heading.

| Part | What | Today's IDs |
|---|---|---|
| `output` | the transcript | `divOutput` |
| `command-bar` | input row, including prompt | `txtCommandDiv`, `txtCommand`, `txtCommandPrompt` |
| `location` | the location bar | `location` inside `qv-status` |
| `border` | the frame around the game | `gameBorder` |
| `panes` | the whole pane region | `gamePanes` |
| `pane` + `data-qv-pane=` `inventory`, `objects`, `status`, `compass`, `commands`, `customstatus`, or an author pane's ID | one pane | `inventory*`, `placesObjects*`, `statusVars*`, `compass*`, `commandPane`, `customStatusPane` |
| `exits` | the exits strip (#2581) | `qv-exits` |
| `picture-frame` | the picture frame | `gamePanel` |
| `map` | the map | `gridPanel` |
| `menu` | an object verb menu or `ShowMenu` dialog | jjmenu, jQuery UI dialog |
| `status-bar` | the pinned phone status bar | (new) |

`Show`/`Hide` requests map `Panes` → `panes`, `Location` → `location`, `Command` → `command-bar`.

## When it freezes

The contract freezes with the first 6.1 release (finding 1). After that, a published package can depend on it. Until then, changing anything here costs nothing.

That gives the order of work:

1. Review and settle this draft.
2. Phase 1 builds `QuestUI` in both UIs: the legacy adapter, plus v2 for what a text game needs.
3. The 610 Core rewrite (`InitInterface`, output, status, transcript, grid, link markup) lands once both UIs implement every member it calls. It can be split into several PRs, since nothing ships in between, but all of it must land before the first 6.1 release. The map members are frozen with today's Grid arguments, so #2437 doesn't block it.
4. The first 6.1 release ships. From then on, additions only.

v2 still only becomes the default for 610 at Phase 5, which can come after the first beta: until then 610 games play on legacy through its `QuestUI` adapter.

## Decisions

Settled on 2026-10-23 in #2590:

1. **Link markup:** keep the existing delegated names (`data-elementid`, `data-command`, `data-deactivateonclick`, and the `elementmenu`/`exitlink`/`commandlink` classes). The only new attributes are `data-qv-event`/`data-qv-param` and `data-qv-popup` (finding 3). The plan's `data-qv-object`/`data-qv-command` aren't used.
2. **`QuestUI` only:** 610 Core reaches the UI only through `JS.QuestUI.*`, with no new typed `IPlayer` members. Each addition is JS-only in both UIs. If #1780 (non-HTML players) needs typed calls later, the engine can intercept known `QuestUI` calls without breaking anything.
3. **Map arguments:** `QuestUI.map` freezes today's Grid arguments, so the Core rewrite doesn't wait on #2437.
4. **Gamebooks:** `GamebookCore`'s `InitInterface` calls `setLayoutPreset("gamebook")` instead of hiding parts one by one.
