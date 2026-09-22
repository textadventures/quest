---
title: Styling the player with CSS
description: Add your own CSS to change how the player looks, which parts of the page are safe to target, and how to test the result on a phone and after loading a saved game
---

The Display and Interface tabs of the `game` object cover most of what you'll want to change about how your game looks - see [Look and feel](/customise/look-and-feel). When they don't go far enough, you can add your own CSS, because the player is a web page. This page shows where to put your CSS, which parts of the page you can target, and some recipes that work on a phone as well as on a wide screen.

| You want to | Use |
|---|---|
| Change the theme, fonts, text colours, links or location bar colours | The Display and Interface tabs - see [Look and feel](/customise/look-and-feel) |
| Change a few properties of one element | `JS.setCss` |
| Add a handful of CSS rules | `JS.addScript` with a `<style>` block |
| Add a longer stylesheet | A Javascript element |

All three code approaches go in the same place: the user interface initialisation script.

## Where your CSS goes

Select the `game` object, go to the _Features_ tab and tick "Show advanced scripts for the game object". Then, on the _Advanced Scripts_ tab, use the "User interface initialisation script (run at start and also when loading a saved game)".

That script runs after Quest Viva has set up the interface from your Display and Interface tab options, so your changes are applied on top of them. More importantly, it runs again whenever the player loads a saved game. The page is rebuilt from scratch when a game is loaded, so anything you change from any other script - the start script, a command, a room's script - is lost after a load. Keep all your styling in this one script.

Don't print anything from this script. At the start of the game it runs before the title is shown, so any text appears above the title.

### A few properties: JS.setCss

`JS.setCss` takes a CSS selector and a list of properties, each written `name:value` and separated by semicolons:

```quest
JS.setCss ("#qv-status", "background:#3b2f2f;color:wheat;border:none")
```

It can't handle a value that contains a colon, such as a full `https://` address in `url(...)`. Use a `<style>` block for those.

### A handful of rules: JS.addScript

`JS.addScript` adds HTML to the page. Give it a `<style>` block to add ordinary CSS rules, including ones `JS.setCss` can't express, like `:hover` or rules for elements that don't exist yet:

```quest
JS.addScript ("<style>#lstInventory li:hover { background: gold; } #commandPane a:hover { text-decoration: underline; }</style>")
```

### A longer stylesheet: a Javascript element

A stylesheet written inside a Quest string soon gets hard to read. For more than a few rules, add a Javascript element (select **Advanced** in the tree, then **Add JavaScript** - see [Calling the game from JavaScript](/customise/javascript#adding-your-own-javascript)) and put the CSS in its file:

```js
if (!document.getElementById("game-css")) {
  document.head.insertAdjacentHTML("beforeend", `<style id="game-css">
    #lstInventory li:hover { background: gold; }
    #commandPane a:hover { text-decoration: underline; }
    .qv-narrow #sidebar { background: black !important; }
  </style>`);
}
```

The file is loaded every time the game starts or a saved game is loaded, before the interface is set up, so you don't need to call anything from the initialisation script. The `if` stops the rules being added twice when the player loads a game without leaving the page.

A separate `.css` file in your game folder, loaded with `JS.addExternalStylesheet(GetFileURL("style.css"))`, works while you test from the editor but not in a published `.quest` file, so use a Javascript element instead.

### When a rule has no effect

Quest Viva sets some styles directly on elements, and these beat an ordinary rule in a stylesheet:

- The game text carries its font and colour on every line, so change those on the Display tab, or during play with functions like `SetFontName` and `SetForegroundColour`.
- Links in the text carry the link colour from the Display tab.
- The page font and colour, the location bar colours from the Interface tab, and the colours from "Colour scheme for panes" are set on their elements.

`JS.setCss` sets styles the same way, and because the initialisation script runs last, it wins. In a stylesheet, add `!important` to a rule that needs to override one of these.

## Elements

These are the parts of the player page you can target. Quest Viva's own libraries use them too, and they are unlikely to change. Avoid classes beginning `ui-` other than `.ui-selected`: they come from the jQuery UI library the player uses for its buttons and panes, and restyling them tends to break more than you intended - `.ui-icon`, for example, is both the pane arrows and the compass arrows.

| Selector | What it is |
|---|---|
| `body` | The whole page. Its background is the margin colour. |
| `.qv-narrow` | A class on `body` whenever the window is narrower than the game's width - on a phone, for instance - and the panes have moved into the ☰ drawer. Start a rule with `.qv-narrow` to apply it only then. |
| `#gameBorder` | The column holding the game, centred on a wide screen. |
| `#qv-status` | The location bar across the top. It also holds the Save/Load and ☰ buttons, and grows taller on a phone. |
| `#location` | The room name, inside the location bar. Quest Viva replaces its contents whenever the player moves. |
| `#controlButtons` | The buttons at the right of the location bar. |
| `#divOutput` | The game text. |
| `#txtCommandDiv`, `#txtCommand` | The command bar, and the box the player types in. |
| `#endWaitLink` | The "Continue..." link shown while the game waits for a key press. |
| `#sidebar` | The container for the panes. On a phone, it is the drawer. |
| `#gamePanes` | The panes. |
| `#inventoryLabel`, `#inventoryAccordion` | The Inventory pane's heading and body. The list inside is `#lstInventory`. |
| `#statusVarsLabel`, `#statusVarsAccordion` | The Status pane's heading and body. The text inside is `#statusVars`. |
| `#placesObjectsLabel`, `#placesObjectsAccordion` | The Places and Objects pane's heading and body. The list inside is `#lstPlacesObjects`. |
| `#compassLabel`, `#compassAccordion` | The Compass pane's heading and body. |
| `#commandPane`, `#customStatusPane` | The command pane and custom status pane - see [Panes](/customise/panes). |
| `#gamePanesFinished` | The "Game Over" message that replaces the panes when the game finishes. |
| `.accordion-header-text` | The text of each pane heading. |
| `.elementList li`, `.ui-selected` | An entry in the Inventory or Places and Objects list, and the selected entry. |
| `a.cmdlink` | A link in the game text. Object links also have the class `elementmenu`, exit links `exitlink` and command links `commandlink`. |
| `#gamePanel`, `#gridPanel` | The picture frame and the map. |

The player sizes and positions the location bar, panes, drawer and text column itself, differently on a wide screen and on a phone. Never set a `width`, `height`, `position`, `left` or `top` on `#gameBorder`, `#qv-status`, `#sidebar`, `#gamePanes` or `#gameContent`, or fix the command bar in place: it looks fine on your screen and breaks the phone layout. To change the width of the game, use "Set a custom display width" on the Interface tab, which the player takes into account.

## Recipes

Each of these goes in the user interface initialisation script unless it says otherwise.

### Colouring the location bar

For plain colours, untick "Classic location style" on the Interface tab and set "Location bar colour", "Location text colour" and "Location border colour". For anything more, use CSS:

```quest
JS.setCss ("#qv-status", "background:#3b2f2f;color:wheat;border:none")
```

Setting `background` rather than `background-color` also removes the classic style's background image.

### Adding the score and turns to the location bar

This shows "Score: 5 | Turns: 12" at the right of the location bar, next to the Save/Load button. On a phone it moves onto a second line of the bar.

Tick "Score" on the game's _Features_ tab, and on the game's _Attributes_ tab add an attribute `turns`, of type integer, set to 0. Add a Javascript element with this function, which adds a `status-extra` element to the location bar the first time it's called and then updates it:

```js
function setStatusExtra(html) {
  var el = document.getElementById("status-extra");
  if (!el) {
    el = document.createElement("span");
    el.id = "status-extra";
    document.getElementById("controlButtons").after(el);
  }
  el.innerHTML = html;
}
```

Add a function called `UpdateStatusExtra`, with this script:

```quest
JS.setStatusExtra ("Score: " + GetInt(game, "score") + " | Turns: " + game.turns)
```

`GetInt` gives 0 if the score hasn't been set up yet, which is the case when the initialisation script first runs. Add a turn script, tick "Enabled when the game begins", and give it this script:

```quest
game.turns = game.turns + 1
UpdateStatusExtra
```

Finally, add this to the user interface initialisation script, so the bar is filled in at the start and after a load:

```quest
JS.addScript ("<style>#status-extra { float: right; padding: 4px 8px; } .qv-narrow #status-extra { padding-top: 12px; }</style>")
UpdateStatusExtra
```

### Hiding a pane during the game

To hide a pane for the whole game, use the "Turn off" options on the Interface tab. To hide one partway through, set the same attribute and hide the pane's heading and body - here, from a script such as a command or a room's "After entering the room":

```quest
game.turnoffcompass = true
JS.uiHide ("#compassLabel, #compassAccordion")
```

Setting `game.turnoffcompass` is what keeps the compass hidden after the player loads a saved game, because Quest Viva reads it when it sets up the interface. To bring it back, set it to `false` and call `JS.uiShow` with the same selectors. The inventory and the places and objects pane work the same way, with `game.turnoffinventory` and `game.turnoffplacesandobjects`.

### Dark panes on a phone

With a dark "Colour scheme for panes", such as Black or Midnight, the panes are dark but the ☰ drawer they sit in on a phone stays white. Colour the drawer to match:

```quest
JS.addScript ("<style>.qv-narrow #sidebar { background: black !important; }</style>")
```

The `.qv-narrow` means the rule only applies when the panes are in the drawer; on a wide screen the panes sit on the page as usual. The `!important` is needed because the player sets the drawer's background itself.

## Testing

Before you publish, check each change:

- **At phone width.** Make your browser window narrow, or use your browser's developer tools to emulate a phone about 375 pixels wide. Check the location bar, the text, and the panes in the ☰ drawer.
- **After loading a saved game.** Play a few turns, save, then load the game again. Anything that only appears at the start of the game belongs in the user interface initialisation script.
- **With your theme's colours.** The player doesn't switch to dark colours when the player's device is in dark mode - it always uses your game's colours. But if you use a dark Theme, such as Retro, or a dark colour scheme for the panes, check that everything you've styled is still readable against it. Set a text colour whenever you set a background colour, and vice versa.

To see what's on the page and which styles apply, right-click while playing and choose "Inspect" (or similar) to open your browser's developer tools.

## See also

- [JS functions](/reference/js/) - all the `JS.` functions, including `setPanes` and `setInterfaceString`
- [Panes](/customise/panes) - the command pane and custom status pane
- [Calling the game from JavaScript](/customise/javascript)
