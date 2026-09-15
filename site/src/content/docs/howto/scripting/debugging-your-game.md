---
title: Debugging your game
sidebar:
  order: 21
---

As you develop your games, there will be times when things happen which you didn't expect – usually because you’ve forgotten to set something up, or you've made a mistake in one of your script commands.

Fortunately, Quest Viva provides you with the Debugger, which lets you keep an eye on what's going on inside your game while you're testing it, and change things to try out a fix without editing the game.

## Opening the Debugger

Click "Preview" in the editor to run your game, then click the "Debug" button at the top of the player. It's also available if you open an `.aslx` file directly in the player, but not when playing a published `.quest` file.

![](/images/Debugger.png)

The Debugger doesn't block the game, so you can leave it open while you play. It updates after every turn, so you can watch attributes change as you type commands. You can move it by dragging its title bar, resize it from the bottom-right corner, and drag the divider between the list and the attributes to make either side wider.

Next to "Debug" there's a "Restart" button, which starts the game again from the beginning without closing the preview.

## Viewing attributes

The tabs along the top choose what to look at - objects, exits, commands, the game itself, turn scripts and timers. Select an item from the list on the left to see its current attributes.

Any attributes inherited from a type are shown in grey - there will usually be quite a few of these, as every object implicitly inherits the "defaultobject" type, which includes many default attributes. The Source column shows where each attribute comes from: the object itself, or the name of the type it was inherited from.

There can be a lot of attributes, so to find the one you want:

- type in the search box to show only matching attributes
- click a column heading to sort by that column, and click it again to reverse the order
- click "Name" above the list on the left to reverse its order

You can also drag the edges of the column headings to resize the columns.

## Changing attributes

To change a value, click its row in the attribute table. A box appears at the bottom with the current value in it - edit it and click "Apply".

What you type is treated the same as the right-hand side of a script that sets the attribute, so:

- strings need quotes, e.g. `"a rusty key"`
- booleans are `true` or `false`
- numbers are just the number, e.g. `42`
- objects are the object's name, without quotes, e.g. `kitchen`

You can also type an expression, such as `player.parent` or `game.score + 10`. If what you type isn't valid, the error is shown under the box and nothing changes.

Setting an attribute this way behaves exactly as if a script had done it, so if the object has a change script for that attribute it runs too. For example, setting `player.parent` to another room runs the player's `changedparent` script, which describes the new room just as if the player had walked there.

Only strings, numbers, booleans and object references can be changed. Lists, dictionaries and scripts can be viewed but not edited.

Changes only last until the game restarts - they aren't saved to your game file. Once you've found a fix that works, make the change in the editor.

## Running walkthroughs

The Walkthrough tab lists your game's [walkthroughs](/howto/scripting/using-walkthroughs). Select one to see its steps, then click "Run" to play them. You can click "Cancel" to stop a walkthrough that's running.
