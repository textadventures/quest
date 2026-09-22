---
title: Changing the game's messages
description: Find and change Quest Viva's built-in responses, vary them at random, and rename the compass directions
---

When the player types something Quest Viva doesn't recognise, it says "I don't understand your command." If they mistype an object's name, they get "I can't see that." Messages like these can break the flow of your game if they don't match the rest of your writing. You can change every one of them.

| You want to | Do this |
|---|---|
| Change a built-in message everywhere in your game | [Copy its template into your game](#changing-a-template) and edit it |
| Change what one object says for one verb | Set that verb on the object instead - see [Using verbs](/howto/commands/verbs) |
| Pick a response at random each time | [Vary the template's text](#varying-default-responses) |
| Use different direction names, like port and starboard | [Change the compass templates](#custom-directions) |

## Where the messages come from

None of Quest Viva's game text is built into the engine. Every standard response is a **template** in the language library you chose when you created the game - `English.aslx` for an English game, which holds about 250 of them. That's what lets you [write games in other languages](/customise/other-languages).

When you copy a template into your game and change it, your copy replaces the library's version for your game only. The library itself isn't changed.

There are two kinds of template:

- A **template** is plain text, such as `UnrecognisedCommand`: "I don't understand your command."
- A **dynamic template** is an [expression](/howto/scripting/writing-code#expressions-and-operators), for messages that depend on an object. The object is in a variable called `object`. For example, `TakeSuccessful` is used when the player picks something up:

  ```quest
  WriteVerb(game.pov, "pick") + " " + object.article + " up."
  ```

  That prints "You pick it up." for one object and "You pick them up." for a plural object. The `WriteVerb` and `article` parts are explained in [Using neutral language](/howto/text/neutral-language). A few dynamic templates that involve two objects, like `DefaultGive`, use `object1` and `object2` instead.

Both kinds go through the [text processor](/howto/text/text-processor) when they're printed, and several of the built-in ones use it already.

## Finding the template for a message

Templates are named after what they're for, not what they say, so you'll usually need to search for the message text:

1. Click "Raw XML code view" on the toolbar (on a narrow screen it's in the "More" menu).
2. In the "File:" list, choose `English.aslx` under "Included libraries". It opens read-only.
3. Click in the text and press Ctrl+F (Cmd+F on a Mac), then search for part of the message, such as `understand your`.

The match is inside a line like this, which tells you the template's name:

```xml
<template name="UnrecognisedCommand">I don't understand your command.</template>
```

Close the code view when you're done.

## Changing a template

1. Click the "Tree view options" button next to the "Filter..." box at the top of the tree, and select "Show Library Elements".
2. In the tree, expand "Advanced", then "Templates" or "Dynamic Templates". Type part of the name in the "Filter..." box to find it quickly.
3. Select the template. A banner says it comes from a library and can't be edited directly. Click "Copy into your game".
4. Change the "Text" field.

You can turn "Show Library Elements" off again afterwards - your copy stays in the tree under "Templates" or "Dynamic Templates".

In code view, a changed template is just a `template` or `dynamictemplate` element in your game file, with the same name as the one it replaces:

```xml
<template name="UnrecognisedCommand">Sorry, I don't know how to do that.</template>
<dynamictemplate name="DefaultKiss">"You'd rather not kiss " + object.article + "."</dynamictemplate>
```

Adding those lines by hand does the same job as copying the template in the tree, which is handy when you want to change several at once. It's also the only way in a gamebook: the gamebook editor's tree has no Templates section.

You can use a template in your own scripts too. `Template("UnresolvedObject")` returns a template's text, and `DynamicTemplate("TakeSuccessful", lamp)` returns a dynamic template's text for the `lamp` object.

## The ones most games change

| Name | Kind | Default text |
|---|---|---|
| `UnrecognisedCommand` | template | "I don't understand your command." |
| `UnresolvedObject` | template | "I can't see that." |
| `UnresolvedLocation` | template | "You can't go there." |
| `DefaultObjectDescription` | template | "Nothing out of the ordinary." |
| `DefaultSelfDescription` | template | "Looking good." (what LOOK AT ME says) |
| `NotCarryingAnything` | template | "You are not carrying anything." |
| `NoKey` | template | "You do not have the key." |
| `DefaultHelp` | template | the whole text of the HELP command |
| `TakeUnsuccessful` | dynamic | "You can't take it." |
| `DefaultSpeakTo` | dynamic | "He says nothing." |
| `LookAtDarkness` | dynamic | "It is too dark to make anything out." |

`DefaultObjectDescription` is worth changing early: it's what every object you haven't described yet falls back on, and a more characterful line makes an unfinished game feel less unfinished.

## Varying default responses

Hearing "I don't understand your command." every time gets dull. The simplest way to vary one is the text processor's `{random:...}` directive. Set the `UnrecognisedCommand` template's text to:

```
{random:Eh?:Come again?:Try something else.}
```

Each time, one of the three is picked at random.

For a dynamic template, put the same directive in the expression's text. This `DefaultHit` still names the object in one of its responses:

```quest
"{random:You'd rather not.:" + WriteVerb(game.pov, "can't") + " hit " + object.article + ".}"
```

That prints either "You'd rather not." or "You can't hit it." Because the options are separated by colons, none of them can contain a colon. If you need one, pick from a list with `PickOneString` instead:

```quest
PickOneString(Split("That would not be nice.;No: you won't do that.", ";"))
```

## Custom directions

On a ship, north and south don't make much sense. This example replaces them with forward, aft, port and starboard. You'll need to change the templates for the direction names, their short forms, and the commands that recognise them. That's easier to paste into code view than to copy one at a time.

1. Click "Raw XML code view" on the toolbar.
2. Paste these lines on a new line after `<include ref="Core.aslx" />`:

   ```xml
   <template name="CompassN">forward</template>
   <template name="CompassS">aft</template>
   <template name="CompassW">port</template>
   <template name="CompassE">starboard</template>
   <template name="CompassNShort">f</template>
   <template name="CompassSShort">a</template>
   <template name="CompassWShort">p</template>
   <template name="CompassEShort">s</template>
   <template templatetype="command" name="go"><![CDATA[^go to (?<exit>.*)$|^go (?<exit>.*)$|^(?<exit>forward|aft|port|starboard|in|out|up|down|f|a|p|s|o|u|d)$]]></template>
   <template templatetype="command" name="lookdir"><![CDATA[^look (?<exit>forward|aft|port|starboard|out|up|down|f|a|p|s|o|u|d)$]]></template>
   ```

3. Click "Apply", and confirm. This reloads the game from the text you edited, so the editor picks up the new names straight away - and discards your undo history.

The `go` template is the pattern for moving: it lets the player type a direction on its own, like `forward` or `f`, as well as `go forward`. `lookdir` does the same for `look forward`. Both are [regular expressions](/howto/commands/regular-expressions), so if you add or rename directions, add them to both lists.

Now the compass on each room's Exits tab shows the new names, and exits you create with it use them. In the game, "You can go forward or aft." appears in room descriptions, and the compass buttons on the player's screen show the new names too.

**Exits you created before changing the templates keep their old names.** Each exit stores its own direction name as its alias. Either change the "Alias" of each one (under "Advanced" on the Exit tab), or in code view replace `alias="north"` with `alias="forward"` and so on.

With only four directions, the diagonal ones have nothing sensible to be called. Leave them out of your map.

Players will still try `north`, so it helps to explain. Add a command to your game with this pattern:

```
n;e;w;ne;nw;se;sw;north;south;east;west;northeast;northwest;southeast;southwest
```

and a "Print a message" script, such as:

```quest
msg ("On board ship, directions are forward, aft, port and starboard. Port is on the left when you face forward.")
```

`s` isn't in the list, because it's now short for starboard.

You may also want to change the `DefaultHelp` template, the text of the HELP command, which tells players to type GO NORTH.

## Printing square brackets

When Quest Viva loads a game, any template name in square brackets, like `[CompassN]`, is replaced with the template's text. That happens everywhere - in descriptions, in messages, even in scripts. Square brackets around anything else, like "[here]", are left alone.

If you really want to print a template's name in square brackets, type the HTML code `&#91;` instead of the opening bracket:

```
&#91;CompassN]
```

That prints "[CompassN]". In code view, the `&` itself has to be written as `&amp;`, so it looks like `&amp;#91;CompassN]`.

## What happens when you publish

Publishing copies the whole language library, including every template you didn't change, into the published game file. A published game is a snapshot: it carries its own copy of the messages as they were when you published, and keeps working the same way on later versions of Quest Viva even if the standard wording changes.

That also means a template change only reaches players when you publish again. It has no effect on a game file they already have.

## See also

- [Text processor](/howto/text/text-processor) - the directives you can use in a template's text
- [Using neutral language](/howto/text/neutral-language) for writing messages that work for any object
- [Writing a game in another language](/customise/other-languages)
- [template](/reference/elements#template) and [dynamictemplate](/reference/elements#dynamictemplate) in the XML elements reference
- [Template](/reference/functions/string#template) and [DynamicTemplate](/reference/functions/string#dynamictemplate) functions
