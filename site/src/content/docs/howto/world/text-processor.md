---
title: Text processor
description: Show text conditionally, vary it at random, format it, and add links the player can click
sidebar:
  order: 1
---

The text processor gives an easy way to conditionally print text, show object links, show text only once, and more.

To use the text processor, you can simply add a directive in curly braces in any text that gets displayed. In this simple example, a room description is set to say that room smells only the first time the text is printed:

![](/images/text_processor_text.png)

The more important text areas have shortcut buttons for some text processor commands; these are the buttons on the right in the image above. However, you can use text processor commands in almost any text, for example, in an [msg](/scripts#msg) command:

```quest
msg ("Would you like some {command:help}?")
```

You can use as many sections as you like within the same text, and even nest them:

```quest
msg ("You can {command:go to shop:go into the shop}. {if player.coins>10:You have {player.coins} coins, which is more than enough.}")

```
Supported processor commands are:

## Text adventure mode and gamebook mode

{once:**text**}  
Displays the text only once. The text will not be printed on subsequent occasions.

{notfirst:**text**}  
Does not displays the text the first time it is printed; the text will only be printed on subsequent occasions.

{random:**text 1:text 2:text 3**}  
Choose text at random (you can have as many sections as you like). This is a great way to add some movement to a character.

```quest
You can see Mary {random:paddling in the sea:building a sand castle:running in the sand}.
```

{img:**filename.png**}  
Insert the specified image.

{**object.attribute**}  
Displays the value of an object's attribute. A great example of this is where the player can set the name of the main character, you can use `{player.alias}` as a stand-in for the character's name.

```quest
'Hi, {player.alias},' says Mary, 'I've not seen you in a while!'
```
{if **object.attribute**:**text**}  
Display text only if object attribute is true (so requires a flag, otherwise known as a Boolean attribute). Containers have a flag called "isopen", and you could use that to modify the description, for instance.

```quest
The chest is old, and almost falling apart. {if chest.isopen:The lid is open.}
```

{if not **object.attribute**:**text**}  
Display text only if object attribute is false.

{if **object.attribute=value**:**text**}  
Display text only if an object attribute equals a certain value. Note that there should be no spaces either side of the `=`.

{if **object.attribute\<\>value**:**text**}  
Display text only if an object attribute does not equal a certain value. Note that there should be no spaces either side of the `\<\>`.

{if **object.attribute\>value**:**text**}  
Display text only if an object attribute is greater than a certain value. Note that there should be no spaces either side of the `\>`.

{if **object.attribute\>=value**:**text**}  
Display text only if an object attribute is greater than or equal to a certain value. Note that there should be no spaces either side of the `\>=`.

{if **object.attribute\<value**:**text**}  
Display text only if an object attribute is less than a certain value. Note that there should be no spaces either side of the `\<`.

{if **object.attribute\<=value**:**text**}  
Display text only if an object attribute is less than or equal to a certain value. Note that there should be no spaces either side of the `\<=`.



{command:**command**}  
{command:**command**:**text**}  
Displays a link that runs a command when clicked. See [Links](#links).

{page:**page**}  
{page:**page**:**text**}  
Displays a link to a page - a gamebook page, or a [Pages](/tutorial/using-pages) dialogue in a text adventure. See [Links](#links).

## Additional text adventure commands

{object:**name**}  
{object:**name**:**link text**}  
{exit:**name**}  
Display a link to an object or an exit. See [Links](#links).

{rndalt:**object**}  
Display a randomly chosen name from an object's [alt](/attributes#alt) list.

{if **attribute**:**text**}  
Display text only if game attribute is true

{if not **attribute**:**text**}  
Display text only if game attribute is false

{select:**object.attribute**:**text 0:text 1:text 2**}
Selects one text to display, based on the value of the object attribute (you can have as many sections as you like). Note that the attribute must be an integer (whole number), and the sections number from zero.



## More text adventure commands

{i:**text**}
Displays the given text in italic.

{b:**text**}
Displays the given text in bold. To do bold and italic, nest the commands, like this: {b:{i:very important}}.

{u:**text**}
Displays the given text in underline.

{s:**text**}
Displays the given text in strike-through.


{colour:**colour**:**text**}
Displays the given text in the colour specified (you can also use "color", by the way). The colour can be a named colour, a list of which can be found [here](https://developer.mozilla.org/en-US/docs/Web/CSS/color_value), or a hexadecimal value such as `#dedede`.

{back:**colour**:**text**}
Displays the given text with the colour specified as the background. To show text as white on black, you can combine these like this: {colour:white:{back:black:some highlighted text}}.

{here **object**:**text**}
Displays the text only if the given object is in the current room (but not if in the player's inventory or in a container in the room).

```quest
The beach is long, and the sand almost white. {here mary:You can see Mary, building a sand castle.}
```

{nothere **object**:**text**}
Displays the text only if the given object is NOT in the current room.

```quest
The beach is long, and the sand almost white. {nothere mary:You wonder where Mary could be.}
```

{popup:**text**:**long text**}
Displays a link, with the first text (which cannot have text processor directives nested in it). When the player clicks on the link, a pop-up will be displayed, containing the long text. The pop-up will disappear when the long text is clicked on. This can be used with the img command to have an image pop-up.


{either **condition**:**text**}  
{either **condition**:**text if true**|**text if false**}  
This works like `if`, with three differences. The condition can be any Quest Viva expression that results in true or false, so you can use functions, `and`, `or` and `not`. Strings being compared need double quotes, as in normal code. And you can give a second text, after a `|`, to show when the condition is false.

```quest
An old chest. {either chest.isopen:The lid is open.|The lid is shut.}
```

{eval:**code**}
The code is evaluated, just as normal Quest Viva code is, and the result displayed.

{=**code**}
This is a short cut for eval, and works just the same. The samples below show the potential, though by its nature this is rather less forgiving that the other commands available.
```quest
"You are in the {eval:player.parent.name}"
 -> "You are in the kitchen"
"You are in the {=player.parent.name}"
 -> "You are in the kitchen"
"You are in the {=CapFirst(player.parent.name)}"
 -> "You are in the Kitchen"
"There are {=ListCount(AllObjects())} objects"
-> "There are 6 objects"
"You look out the window: {=LookOutWindow}"
 -> "You look out the window: A figure is moving by the bushes"
```
 



## Additional gamebook commands

{counter:**countername**}  
Displays the value of an counter

{if **flag**:**text**}  
Display text only if flag is set

{if not **flag**:**text**}  
Display text only if flag is not set

{if **countername=value**:**text**}  
Display text only if a counter equals a certain value.

{if **countername\>value**:**text**}  
Display text only if a counter is greater than a certain value.

{if **countername\>=value**:**text**}  
Display text only if a counter is greater than or equal to a certain value.

{if **countername\<value**:**text**}  
Display text only if a counter is less than a certain value.

{if **countername\<=value**:**text**}  
Display text only if a counter is less than or equal to a certain value.


## Links

Four directives turn text into something the player can click. Like every other directive, they work anywhere the text processor does: descriptions, `msg`, page text and so on.

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
- **Page links** work in both kinds of game. In a gamebook, `{page:...}` and `{command:...}` do the same thing: go to the named page. In a text adventure, `{page:...}` links to a [Pages](/tutorial/using-pages) dialogue, and clicking it starts the conversation at that page, as the "Show page" script command does. A sign could say `{page:guard_intro:talk to the guard}`, for example.

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

## Errors

If the text processor cannot understand your directive, it will generally leave the text as is. This should make it easier to identify issue. For example, for the "select" directive, if the value of the object attribute is outside the range (a negative number or a number higher or equal to the number of options), no processing is done, and the text will appear as written.



## Curly braces

Should you want to use curly braces to actually display curly braces, Quest Viva will usually work out that that is what you want. If you find it is trying to display it as a text processor command (or is throwing an error because it has failed to), you can use `@@@open@@@` and `@@@close@@@` to tell Quest Viva to display curly braces.
```quest
"player.count = @@@open@@@player.count@@@close@@@"
 -> "player.count = {player.count}"
 ```

 
## Using text processor with object aliases

You cannot use text processor commands in an object's name, as only a limited set of characters is allowed (letters, numbers, space and underscore). You can for the object's alias, however, so you could set an alias to "{i:big} settee". You will find that the alias as it appears in the pane on the right has not been processed; the player will see the raw "{i:big} settee". To get around that, give the object a list alias on the _Object_ tab.
 
 
## Support for "this"

In a script, `this` means the object the script belongs to. In the text processor, `{this.attribute}` means the object of the command the player has just typed: the teapot in X TEAPOT, the lamp in SWITCH ON LAMP, the first object in PUT BALL IN BOX. That makes it useful in an object's description, and in the messages it prints for its verbs:

```quest
The {this.alias} is {if this.switchedon:on}{if not this.switchedon:off}.
```

`{this.alias}` shows nothing if the object has no alias. `{=GetDisplayAlias(this)}` always shows the name the player sees.

`this` isn't reset after a command, so don't rely on it anywhere else. In a room description, or in the output of a command without an object, it still refers to the object from the last command that had one.

To choose what `this` means yourself, set `game.text_processor_this` before printing the text:

```quest
game.text_processor_this = teapot
msg ("The {this.alias} is {if this.capacity<5:not }big enough.")
```


## Local variables

You can give the text processor other names for objects too. Put them in a dictionary in `game.text_processor_variables`, with the name as the key and the object as the value:

```quest
game.text_processor_variables = NewDictionary()
dictionary add (game.text_processor_variables, "animal", tiger)
msg ("You can see a {animal.alias}.")
```

The names last until you change the dictionary, and you can add as many as you like. A `this` entry in the dictionary is ignored while `game.text_processor_this` is set.


## Extending

You can add your own text processor directives. This should be done in the "start" script of the game object (top of the _Scripts_ tab on the game object).

Here is a very simple example that will replace `{test}` with `Some Text`:

```quest
game.textprocessorcommands = game.textprocessorcommands
scr => {
  game.textprocessorcommandresult = "Some Text"
}
dictionary add(game.textprocessorcommands, "test", scr)
```

The first step is to clone the script dictionary to the game object, which might look as if it is not actually doing anything, but behind the scenes is vital (you only ever need to do this once; if you forget you will get an error saying "Cannot modify the contents of this dictionary..."). The next three lines create a script, whilst the last line adds that script to the dictionary, using the key "test", which is then the name of the directive.

The script is where the action happens. In this case it just sets the result (a special attribute on the game object).

The script has access to a local variable called "section", which contains the text inside the curly braces (including the name of the directive). For the example above, that would just be "test".

Let us add another directive to see how that can be used:

```quest
scr => {
  s = Mid(section, 6)
  game.textprocessorcommandresult = "<span style=\"color:blue\">" + s + "</span>"
  }
dictionary add(game.textprocessorcommands, "blue", scr)
```

This will print the text in blue. 

```quest
msg("Here is the {test}, now with some in {blue:a different colour!}")
```

Inside the script, scr, there are two lines. The first gets the actual text. The word "blue" is four characters, then there is the colon, so the bit we want starts at the sixth character.

The second line then sets the return value, using HTML and CSS to change the text colour to blue.

## HTML tags

You can also use HTML tags directly in any text output. For example:

```xml
This text is <b>bold</b>. This text is <i>italic</i>. This text is <u>underlined</u>.
```

For more complex styling, use `<span>` tags with inline CSS, for example `<span style="color:red">this is red</span>`. The text processor `{colour:}` and `{back:}` commands above are generally more convenient for this.