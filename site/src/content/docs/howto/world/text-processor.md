---
title: Text processor
description: The full list of curly-brace directives, and where in your game they work
sidebar:
  order: 1
---

The text processor lets you put instructions inside your game's text, in curly braces. Text that would otherwise be fixed can then show something only once, vary at random, change with an object's state, or become a link the player can click.

```quest
msg ("You can {command:go to shop:go into the shop}. {if player.coins>10:You have {player.coins} coins, which is more than enough.}")
```

This page is the complete reference. Every directive below is one that the engine actually registers, and each works in both text adventures and gamebooks unless the table says otherwise.

## Where it runs

The text processor runs on everything the game **prints**. That includes:

- room, object and in-room descriptions
- anything a script prints with "Print a message" (`msg`)
- [templates and dynamic templates](/howto/world/changing-templates) - the built-in messages use directives themselves
- an object's alias, when it appears in printed text such as "You can see a big settee"
- page text and page option captions, in a gamebook and in text adventure [Pages](/tutorial/using-pages)
- the captions and options of `ShowMenu` and `Ask`

It does **not** run on:

- names and aliases in the _Places and Objects_ and _Inventory_ panes - the player sees the raw `{i:big} settee`. A list alias is no different; it isn't processed either
- the status bar - `game.statusattributes` formats are printed as written
- the echo of the command the player typed
- an object's **name**, which can only contain letters, numbers, spaces and underscores anyway
- anything you send to the browser yourself with a `JS.` function

## Using a directive in the editor

Every large text box in the editor - a description, a verb's message, a page's text - has a toolbar above it:

- **Bold**, **Italic** and **Underline** wrap the selected text. These insert the HTML tags `<b>`, `<i>` and `<u>` rather than the `{b:}` directives; both work.
- **Page** inserts a link to a page, picked from a list.
- **Insert** opens a menu of the rest, grouped as _Links_ (Object Link, Command Link, Exit Link), _Conditions_ (If..., If not..., If in room, If not in room) and _Other_ (Once, Random text, Image). The link and image ones ask you to pick the target from a list. A gamebook, which has no objects or exits, shows only Once, If..., Random text and Image.

Anything the menu doesn't cover, you type by hand. Nothing else is needed: a directive is just text.

## Showing text conditionally

| Directive | What it does |
|---|---|
| `{if object.attribute:text}` | Shows the text if the attribute is true |
| `{if not object.attribute:text}` | Shows the text if the attribute is false or unset |
| `{if object.attribute=value:text}`<br/>`{if object.attribute<>value:text}` | Compares the attribute with the value as text, so it works for a string or a number. No spaces around the operator, and no quotes around the value |
| `{if object.attribute>value:text}` | Compares as numbers. Also `>=`, `<` and `<=` |
| `{if attribute:text}`<br/>`{if not attribute:text}` | The same, for an attribute on the **game** object. `{if brave:...}` and `{if game.brave:...}` are the same thing |
| `{if counter>value:text}` | A bare name on the left of a comparison works only if the game has an **integer** attribute of that name |
| `{either condition:text}`<br/>`{either condition:text if true\|text if false}`<br/>`{either condition:text if true:text if false}` | Like `{if}`, but the condition is a full [expression](/howto/scripting/expressions), so it can use functions, `and`, `or` and `not`, and compare strings with double quotes |
| `{select:object.attribute:text 0:text 1:text 2}` | Picks one option by number. The attribute must be a whole number, and options count from zero |
| `{here object:text}` | Shows the text if the object is directly in the player's current room - not if it's carried, and not if it's inside something |
| `{nothere object:text}` | Shows the text if it isn't |
| `{once:text}` | Shows the text the first time only |
| `{notfirst:text}` | Shows the text every time except the first |
| `{random:text 1:text 2:text 3}` | Picks one at random. As many options as you like |

```quest
An old chest. {either chest.isopen:The lid is open.|The lid is shut.}
The beach is long, and the sand almost white. {here mary:You can see Mary {random:paddling in the sea:building a sand castle}.}
```

`{if}` and `{either}` overlap. Reach for `{if}` for a simple flag or number, and `{either}` when you want an else branch or anything an expression can do:

```quest
{either player.coins > 10 and not shop.closed:You could afford this.|Better keep walking.}
```

`{select}` is for a value that steps through a few states. The `{select:jacket.multistate:open:buttoned:zipped}` form is the usual one; if an option needs to contain a colon, separate the options with `|` instead and the whole directive switches to `|` as its separator.

`{once}` and `{notfirst}` remember what they have shown against **the exact text they appear in**, not against the object. Two objects whose descriptions are character-for-character identical share the same "already shown" record, so a `{once:}` in both will only fire once between them. Vary the wording if that matters.

## Showing values

| Directive | What it does |
|---|---|
| `{object.attribute}` | Prints the value of an attribute |
| `{counter:name}` | Prints an integer attribute of the game object, or `0` if there isn't one |
| `{rndalt:object}` | Prints a name picked at random from the object's [alt](/attributes#alt) list |
| `{eval:expression}` | Works out the expression and prints the result |
| `{=expression}` | The same thing, written shorter |

`{object.attribute}` takes one dot only: the part before it is an object name, the part after it is the attribute. `{game.pov.alias}` will not work, because "game.pov" isn't an object name - use `{=game.pov.alias}` for that. An attribute the object doesn't have prints nothing; an object that doesn't exist leaves the directive on screen as written. A true/false attribute prints as "true" or "false", and a list or dictionary prints as "(stringlist)", which is rarely what you want.

```quest
'Hi, {player.alias},' says Mary, 'I've not seen you in a while!'
```

That is the usual way to use the player's chosen name, but note that until you set it, `player.alias` is "me" - the player object swaps in its own point-of-view alias while it's the one being played. Set it, typically from [`GetInput()`](/howto/scripting/asking-the-player), and `{player.alias}` shows what you set from then on.

`{=...}` is the escape hatch, and is less forgiving than everything else here - a mistake in it is a runtime error rather than text left as written. If the expression contains nothing but letters, digits and spaces, it's treated as a function call, so `{=LookOutWindow}` calls your `LookOutWindow` function.

```quest
You are in the {=CapFirst(game.pov.parent.name)}. There are {=ListCount(AllObjects())} objects.
```

## Formatting

| Directive | What it does |
|---|---|
| `{i:text}` | Italic |
| `{b:text}` | Bold |
| `{u:text}` | Underlined |
| `{s:text}` | Struck through |
| `{colour:colour:text}` | Coloured text. `{color:...}` is accepted too |
| `{back:colour:text}` | Coloured background |
| `{img:filename.png}` | Inserts an image |
| `{popup:link text:pop-up text}` | A link that shows the longer text in a pop-up when clicked. Clicking the pop-up closes it |

The colour can be a [named colour](https://developer.mozilla.org/en-US/docs/Web/CSS/color_value) or a hex value such as `#dedede`. Combine the two for highlighted text:

```quest
msg ("{colour:white:{back:black:Some highlighted text}}")
```

You can also use HTML tags directly in any text - `<b>bold</b>`, `<i>italic</i>`, or a `<span style="color:red">` for anything the directives don't cover. The editor's Bold, Italic and Underline buttons insert HTML tags for exactly this reason.

## Links

Four directives turn text into something the player can click.

| Directive | What clicking it does | Example |
|---|---|---|
| `{object:name}`<br/>`{object:name:text}` | Shows a menu of the object's verbs (Look at, Take and so on), and runs the one the player picks | `{object:torch:the old torch}` |
| `{command:command}`<br/>`{command:command:text}` | Runs the command, just as if the player had typed it | `{command:wait:wait a moment}` |
| `{exit:name}` | Goes through the exit | `{exit:north_exit}` |
| `{page:page}`<br/>`{page:page:text}` | Goes to the page | `{page:guard_intro:talk to the guard}` |

In each case the optional last part is the text the player sees. Leave it out and the link shows the object's display alias, the command itself, the exit's alias ("north") or the page name.

- **Object links** take the object's name, not its alias. The menu offers the same verbs as the _Places and Objects_ pane.
- **Command links** go through the parser like anything typed, so they can run any command your game understands, including your own: `{command:jump up and down:jump about}`.
- **Exit links** need the exit's name. Exits don't have one by default, so give it one in the _Name_ box on the exit's _Exit_ tab. Clicking sends "go" and the exit's alias, for example "go north".
- **Page links** work in both kinds of game. In a gamebook, `{page:...}` and `{command:...}` do the same thing: go to the named page. In a text adventure, `{page:...}` links to a [Pages](/tutorial/using-pages) dialogue, and clicking it starts the conversation at that page, as the "Show page" script command does.

To link to a web page, use an ordinary HTML link:

```quest
msg ("Read more on <a href=\"https://questviva.com\">the Quest Viva website</a>.")
```

### Hyperlink settings

The game object's _Display_ tab has a _Hyperlinks_ section:

- **Hyperlinks: players can access object verbs by clicking object names** - on by default. Untick it and `{object:...}` shows plain text, as do the objects and exits listed in room descriptions. `{command:...}`, `{exit:...}` and `{page:...}` links still appear.
- **Link colour** and **Underline hyperlinks** - how links look.
- **After using a command hyperlink, deactivate it** - each command link stops working once the player has clicked it.

In code, these are `game.enablehyperlinks`, `game.defaultlinkforeground`, `game.underlinehyperlinks` and `game.deactivatecommandlinks`.

### Building links in code

These functions build the directive text for you, so you can join it into a message:

- [`ObjectLink(object)`](/reference/functions/internal-core#objectlink) returns `{object:name}`.
- [`CommandLink(command, text)`](/reference/functions/internal-core#commandlink) returns `{command:command:text}`.
- [`GetDisplayNameLink(object, type)`](/reference/functions/core#getdisplaynamelink) returns the object's name with its article, as a link when `type` is `"object"`: "a torch", with "torch" as the link.

```quest
msg ("You could " + CommandLink("wait", "wait here") + ", or pick up " + GetDisplayNameLink(torch, "object") + ".")
```

Two more print a link straight away. [`DisplayHttpLink(text, url, https)`](/reference/functions/internal-core#displayhttplink) prints a link to a web page, using `https://` when the third parameter is `true`. [`DisplayMailtoLink(text, email)`](/reference/functions/user-interface#displaymailtolink) prints an email link.

```quest
DisplayHttpLink ("Quest Viva", "questviva.com", true)
DisplayMailtoLink ("Email the author", "author@example.com")
```

## Nesting, colons and braces

Directives nest, as deep as you like, and the inner ones are worked out after the outer one has chosen its text:

```quest
{if chest.isopen:The lid is open, {random:and the chest is {i:empty}:and something glints inside}.}
```

The colon separates a directive's parts, so a colon in your own text can split an option in two. `{random:}` and `{select:}` are the ones that bite - `{random:He said: hello:She said hi}` is three options, not two. Rewrite the text, or use the `|` separator that `{select:}` and `{either}` accept. `{colour:}`, `{command:}`, `{object:}`, `{page:}` and `{popup:}` only split at their first colons, so the last part can contain as many as it likes.

To print a real curly brace, Quest Viva usually works it out for itself: anything it doesn't recognise as a directive, like `{brace}` or an unclosed `{`, is left exactly as written. When it guesses wrong, use `@@@open@@@` and `@@@close@@@`:

```quest
msg ("player.count = @@@open@@@player.count@@@close@@@")
```

That prints `player.count = {player.count}`.

## Errors

A directive the text processor can't make sense of is generally left on screen as written, rather than throwing an error. If `{select:chest.mood:a:b:c}` is showing up verbatim, `chest.mood` is either missing, not a whole number, or outside the range of the options. The same goes for an `{if}` comparison against an attribute the object doesn't have, and for `{object:}`, `{exit:}` and `{rndalt:}` with a name that doesn't exist. Seeing your own directive in the game text is the signal to check the name.

## What `this` means

In a script, `this` is the object the script belongs to. In the text processor, `{this.attribute}` is the object of the command the player has just typed: the teapot in X TEAPOT, the lamp in SWITCH ON LAMP, the first object in PUT BALL IN BOX. That makes it useful in an object's description, and in the messages its verbs print:

```quest
The {this.alias} is {if this.switchedon:on}{if not this.switchedon:off}.
```

`{this.alias}` shows nothing if the object has no alias; `{=GetDisplayAlias(this)}` always shows the name the player sees. `this` isn't cleared at the end of a turn, so don't rely on it anywhere else - in a room description, or after a command with no object, it still refers to the object from the last command that had one.

To choose what `this` means yourself, set `game.text_processor_this` before printing:

```quest
game.text_processor_this = teapot
msg ("The {this.alias} is {if this.capacity<5:not }big enough.")
```

:::caution[Known bug]
While `this` is in force, `{if}` also replaces the letters "this" inside longer names in its condition, so `{if thistle.prickly:...}` silently fails ([#2351](https://github.com/textadventures/quest/issues/2351)). Avoid object and attribute names that contain "this", or use `{either thistle.prickly:...}`, which isn't affected.
:::

## Your own names for objects

You can give the text processor other names for objects. Put them in a dictionary in `game.text_processor_variables`, with the name as the key and the object as the value:

```quest
game.text_processor_variables = NewDictionary()
dictionary add (game.text_processor_variables, "animal", tiger)
msg ("You can see {object:animal}. {if animal.asleep:It is asleep.}")
```

The names last until you change the dictionary, and work in `{object.attribute}`, `{if}`, `{either}`, `{object:}`, `{select:}`, `{rndalt:}`, `{here}` and `{nothere}`. A `this` entry in the dictionary is ignored while `game.text_processor_this` is set.

## Gamebooks

Everything above works in a gamebook except the directives that need a world of objects and rooms. `{object:}`, `{exit:}`, `{rndalt:}`, `{here}`, `{nothere}` and `{popup:}` throw an error in a gamebook, because the functions they rely on are part of the text adventure library. Use `{page:}` and `{command:}` for links instead - in a gamebook they are the same thing.

`{counter:}` and the bare `{if flag:...}` form are the ones you'll use most there, since a gamebook's counters and flags are simply attributes on the game object: `SetCounter("gold", 7)` is what `{counter:gold}` reads.

## Adding your own directives

You can add directives of your own. Do it in the "start" script of the game object (top of the _Scripts_ tab), and clone the script dictionary onto the game object first - that line looks as if it does nothing, but without it you get "Cannot modify the contents of this dictionary...".

```quest
game.textprocessorcommands = game.textprocessorcommands
scr => {
  game.textprocessorcommandresult = "<span style=\"color:blue\">" + Mid(section, 6) + "</span>"
}
dictionary add (game.textprocessorcommands, "blue:", scr)
```

`{blue:like this}` now prints in blue. The script gets a local variable `section` holding everything inside the braces, directive name included, so `Mid(section, 6)` skips past "blue:" to the text. Set `game.textprocessorcommandresult` to whatever should replace the directive.

The key you add is matched against the start of the section, which is why the built-in ones end in a colon - or, for `{if }`, `{either }`, `{here }` and `{nothere }`, a space.

## See also

- [Changing the game's messages](/howto/world/changing-templates) - the built-in text, which is written with these directives
- [Expressions](/howto/scripting/expressions) - what you can put in `{either}` and `{=}`
- [Pictures](/howto/multimedia/images) - more on `{img:}` and the picture frame
