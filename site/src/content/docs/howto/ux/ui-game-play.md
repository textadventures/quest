---
title: The player interface
description: The ways players can interact with your game - typing, links, panes, menus and pages - how they work on a phone, and where to go to customise each one
---

Players can interact with a Quest Viva text adventure in several ways, and you decide which ones your game offers. This page describes each of them, how they behave on a phone, and where to go to change them.

| Way to play | What the player does | Turn it on or off |
|---|---|---|
| Command bar | Types commands such as `take lamp` | _Interface_ tab: **Show command bar** |
| Links in the text | Clicks an object for a menu of verbs, an exit to go that way, or a link you've written | _Display_ tab: **Hyperlinks** |
| Panes | Clicks objects in the Inventory and Places and Objects lists, or a direction on the compass | _Interface_ tab: **Show panes** |
| Menus and pages | Chooses from numbered options in the text | Your scripts and pages |

All of these are on the "game" element; see [Look and feel](/howto/ux/ui-style) for every option.

## The command bar

Typing gives the player the most freedom, and asks the most of you: if a room description mentions a painting, players will expect to `look at painting`, and they'll try synonyms you haven't thought of. It's also the only way to reach commands such as `put ball in sack` unless you provide a link for them.

## Links in the text

With hyperlinks on, the objects and exits in room descriptions are links. Clicking an object shows a menu of its [display verbs](/howto/ux/display-verbs), such as "Look at" and "Take"; clicking an exit goes that way. You can add your own links anywhere the text processor works, such as a room description or a message:

```quest
msg ("The guard looks bored. Perhaps you could {command:wait:wait for him to leave}.")
```

A `{command:...}` link runs the command as if the player had typed it. See [Links](/howto/world/text-processor#links) for the other kinds.

## Panes

The panes list what the player is carrying and what's in the room, with buttons for each object's verbs, and a compass for moving around. A game can also show a Status pane, a command pane of one-click commands and a custom status pane - see [Panes](/howto/ux/custom-panes).

## Menus and pages

When a script asks the player to choose - "Which do you mean?", or a `ShowMenu` in your own script - the options appear as numbered links in the text. The player can click one or type its number. See [Asking the player](/howto/scripting/asking-the-player).

For conversations, [Pages](/tutorial/using-pages) show some text and a list of options leading to further pages. Every choice is a complete turn, so the player can save or undo at any point.

## Choosing a combination

The default - command bar, links and panes all on - suits most games. Before changing it, consider:

- **Links and panes can only do so much.** Without the command bar, the player can only move, use each object's verbs and click links you've written. Anything else - `wait`, `look`, two-object commands - needs a `{command:...}` link, a command pane or a page. Play the whole game by clicking alone before you turn the command bar off.
- **Players may assume one method is enough.** Some players will expect to finish the game using only links. If a puzzle needs a typed command, make sure your hints point to it.
- **Links give things away.** A list of verbs tells the player what's possible. Typing avoids that, at the risk of "guess the verb" frustration. You can hide or add verbs on each object's [Object tab](/howto/ux/display-verbs).

## On a phone

The player adapts to narrow screens:

- The text fills the width of the screen, with no margins.
- The panes move into a drawer. The player opens it with the ☰ button at the top right, next to Save / Load, and closes it with the same button. If you set a custom display width, this happens whenever the window is narrower than that width.
- Verb menus, the pane lists and the buttons get larger, so they're easy to tap. Links in the text stay the size of the text around them.

A phone player won't see the status pane or command pane unless they open the drawer, so keep anything important in the text too. Typing on a phone is slow, so links in the text are especially welcome - and link text such as "the rusty key" is easier to tap than "key".

## Accessibility

- **The command bar suits screen reader users.** New text is announced as it appears, and typing commands is often quicker than moving between links. Avoid turning it off.
- **Links work from the keyboard.** Links in the text and the compass buttons can be reached with Tab and activated with Enter, and verb menus can be used with the arrow keys. Menu options can also be chosen by typing their number. The object lists in the Inventory and Places and Objects panes can't be reached from the keyboard, so don't make them the only way to do something.
- **Check your colours.** Choose text, link and background colours with strong contrast, and don't rely on colour alone to convey meaning - some players can't tell red from green.
- **Choose readable fonts.** Keep decorative web fonts for headings or the odd message.
- **Describe your pictures.** A player who can't see an image misses anything that is only in the image, so put the important details in the text too.

## Customising the interface

| To change | Read |
|---|---|
| The theme, fonts, colours, panes, command bar and room descriptions | [Look and feel](/howto/ux/ui-style) |
| Which verbs appear for an object, in menus and panes | [Object verbs](/howto/ux/display-verbs) |
| The command pane and custom status pane | [Panes](/howto/ux/custom-panes) |
| Anything the options don't cover, with your own CSS | [Styling the player](/howto/ux/customising-the-ui) |
| The interface's behaviour, with JavaScript | [Calling the game from JavaScript](/howto/ux/ui-callback) |
