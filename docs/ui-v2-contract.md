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
| `PlayerHelper` command links (`<a cmd>` in `msg`) | `onclick="sendCommand('…')"` | `data-command="…"` (the version branch lives in `PlayerHelper`, which isn't frozen) |
| `PlayerHelper` external links | `onclick="goUrl('…')"` | a real `href` with `target="_blank" rel="noopener"` |

`data-qv-event` and `data-qv-popup` are the only new attribute names. Both UIs handle all of these through delegated listeners.

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

`getDivCount` is dropped: it exists only to keep legacy's div IDs in step after a restore, which the adapter can do itself.

### Layout and panes

| Member | Replaces |
|---|---|
| `setVisible(part, visible)` | `uiShow`/`uiHide`, `panesVisible`, `hideBorder`, and the `setCss(…, "display:…")` calls |
| `setPaneOrder(parts)` | the six `insertBefore` evals (`statusandcompassattop`) |
| `setLayoutPreset(name)` with `gamebook` | GamebookCore's `panesVisible(false)` and its `uiHide`s of the location and command bar. Legacy hides those parts, as today. v2 gives gamebooks their own layout |
| `setPictureFrame(html)` | `setPanelContents` |
| `setInterfaceString(name, text)` | `setInterfaceString` (same names: `InventoryLabel`, `StatusLabel`, `PlacesObjectsLabel`, `CompassLabel`, `InButtonLabel`, `OutButtonLabel`, `EmptyListLabel`, `NothingSelectedLabel`, `TypeHereLabel`, `ContinueLabel`) |
| `setCompassDirections(list)` | `setCompassDirections` |
| `setLocation(text)` | `updateLocation` |
| `setStatusAttributes(items)` | `updateStatus` (see below) |

**Status attributes become structured.** Today `UpdateStatusAttributes` joins pre-formatted strings with `<br/>`. For #2337 item 3 (a pinned phone status bar with `value/max` bars), 610 Core passes items instead: `[{ "name": "health", "label": "Health", "value": 7, "max": 10, "text": "Health: 7/10" }]`. `text` is the formatted string as today, so a UI that doesn't draw bars just shows it.

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

### Transcript

The transcript belongs to the host (the start screen and transcript viewer are shared by both UIs), but today it's driven from `playercore.js`'s globals and seeded through `JS.eval`'d variables. 610 Core calls `QuestUI.transcript.*` instead, and both UIs forward to the host: `write(html)`, `enable(name)` (`name` optional), `disable()`, `kill()`, `revive()`, `setName(name)`, `setPrefix(text)` (from `replaceTranscriptString`), `stampDateAndTime()`, `promptForName()`, `show()`. The game name no longer needs seeding: the host already gets it from `IPlayer.UpdateGameName`.

### Author hooks

These have no Core caller today. They're the supported versions of the survey's common hacks, so they're part of the first contract:

| Member | Instead of |
|---|---|
| `addPane(id, title, html)`, `setPaneContent(id, html)` | inserting accordion sections by hand; `setCustomStatus` (custom status pane) |
| `setCommands(list)` | `setCommands` (command pane) |
| `setAutoScroll(on)` | `scrollToEnd = function(){}` |
| `setContinueOnClick(on)` | a window handler that checks `_waitMode` and calls `endWait()` |
| `fadePreviousOutput(on)` | fading `#divOutput` children |
| `setCommandPlaceholder(text)` | setting `#txtCommand`'s placeholder |
| `setMapInteractive(on)` | `gridApi.zoomIn = function(){}` and turning off paper.js drag (same as `map.setInteractive`) |
| `on(event, fn)` / `off(event, fn)` with `turnStarted`, `turnEnded`, `outputAdded` (`{ html }`), `locationChanged` (`{ location }`) | wrapping player functions to observe them |

A **turn** is one engine round trip the player starts: a command, an event (`sendEvent`, a `data-qv-event` link), or answering a wait, pause, menu or question. `turnStarted` fires before the input goes to the engine and `turnEnded` once the engine has finished with it and its output is on screen. Timer ticks aren't turns.

Each gets a 610 Core wrapper (`SetPaneVisible`, `AddPane`, `SetAutoScroll`, …) in Phase 4, so most authors never write JS.

## CSS custom properties

Every game option that `InitInterface` applies today maps onto a token. 610 `InitInterface` reads the game's attributes and passes them in one `setTokens` call (a string dictionary). Themes (`theme_novella`, …) become token sets, and authors override any of them in the game stylesheet.

Values are CSS values, as a stylesheet would write them: colours as any CSS colour, lengths with units (`700px`, `12pt`), images as `url("…")`, and `--qv-bg-opacity` as a number from 0 to 1. A bare number for a length is read as pixels. An empty value unsets the token.

| Token | From |
|---|---|
| `--qv-font-body`, `--qv-font-size` | `defaultfont` / `defaultwebfont`, `defaultfontsize` |
| `--qv-text`, `--qv-bg`, `--qv-link` | `defaultforeground`, `defaultbackground`, `defaultlinkforeground` |
| `--qv-link-decoration` | `underlinehyperlinks` |
| `--qv-page-bg` | `marginscolour` |
| `--qv-bg-image`, `--qv-bg-opacity` | `backgroundimage`, `backgroundopacity` |
| `--qv-bg-blend-top`, `--qv-bg-blend-bottom` | `colourblendtop`, `colourblendbottom` |
| `--qv-output-width` | `customwidth` |
| `--qv-padding-top`, `-bottom`, `-left`, `-right` | `custompadding*` |
| `--qv-border-colour` | `bordercolour`. `showborder = false` is `setVisible("border", false)`, not a token |
| `--qv-pane-fg`, `--qv-pane-bg`, `--qv-pane-heading-fg`, `--qv-pane-heading-bg`, `--qv-pane-highlight` | `panestheme` (the seven named themes become token sets) |
| `--qv-status-fg`, `--qv-status-bg`, `--qv-status-border`, `--qv-status-image` | `customlocationtextcolour`, `customlocationcolour`, `customlocationbordercolour`, `locationbarimage` |
| `--qv-menu-fg`, `--qv-menu-bg`, `--qv-menu-hover-fg`, `--qv-menu-hover-bg`, `--qv-menu-font`, `--qv-menu-font-size` | `menuforeground`, … `menufontsize` |
| `--qv-command-fg`, `--qv-command-bg` | command bar format |

v2 also defines tokens with no game attribute behind them yet (`--qv-surface`, `--qv-radius`, `--qv-space`, `--qv-font-ui`, focus-ring colours). Those are in the contract too, and the editor gains options for them later.

### Inline output styles

`OutputTextRaw` wraps every line in `<span style="font-family:…;color:…;font-size:…">`, taken from `defaultfont`, `defaultforeground` and `defaultfontsize`. Inline styles beat any token, so a theme, a light/dark variant or an author stylesheet couldn't change the text colour or font of output that's already there.

**Recommendation:** 610 `GetCurrentTextFormat` omits a property when it equals the game's default, so ordinary output inherits from the tokens. A mid-game `SetForegroundColour`/`SetFontName` still works: the new value differs from the default, so it's written inline, exactly as today. `SetForegroundColour` etc. also update the matching token when called from `InitInterface`.

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
