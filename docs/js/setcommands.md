---
title: setCommands
parent: "JS functions"
grand_parent: "Reference"
nav_order: 6
---

    JS.setCommands(string commands, string colour)

Sets the commands to be displayed on the command pane (turn the command pane on on the _Interface_ script of the game object). Commands should be sent as a string, separated by semi-colons. The colour of the text can be specified, but is optional.

```
JS.setCommands("Wait;Look")
JS.setCommands("Wait;Look;Get apple", "red")
```