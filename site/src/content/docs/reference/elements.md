---
title: XML elements
sidebar:
  order: 25
---

Note that this is about XML elements in the ASLX file, which is not quite the same as the elements in the game.

## asl

```xml
<asl version="580">all game content</asl>
```

To load any game, the top-level element must be an \<asl\> element as shown above. All other XML elements in the file must appear within this tag.

## library

```xml
<library>all library content</library>
```

The top-level element of any library must be a \<library\> element as shown above. All other XML elements in the file must appear within this tag.

## include

```xml
<include ref="filename"/>
```

Loads the specified library.

## template

```xml
<template name="name">text</template>
```

Creates a template of the specified name. You can print the template's text using the [Template](/reference/functions/string#template) function.

Within a language library, a template may define a **templatetype** of "command", for example:

```xml
<template templatetype="command" name="undo">^undo$</template>
```

This simply is a flag to the Editor to prevent it from showing the template in the list of templates (as the way to edit it would be to edit the associated command pattern).

Note that it is important to have templates defined in the right place in the code. If your template is to override an existing template, then it has to come *after* the language file include. However, it has to come *before* the template is used in the code, which should be before the core library file include. As of version 5.2 Quest Viva does not do this, so you will need to manually move the templates to the right place. Your game file should start something like this:

```xml
<!--Saved by Quest 5.2.4515.34846-->
<asl version="520">
  <include ref="English.aslx"/>
  <template name="SeeListHeader">There's</template>
  <template name="GoListHeader"> Go to </template>
  <template name="UnrecognisedCommand">Unknown command.</template>
  <template name="YouAreIn"></template>
  <template name="PlacesObjectsLabel">Places / Objects</template>
  <include ref="Core.aslx" />
  <game name="Test_1">
  ...
```

## dynamictemplate

```xml
<dynamictemplate name="name">expression</template>
```

A dynamictemplate is used in a similar way as [template](#template), except that its value is an expression, not a static string. The expression will have access to an object called "object", which you can use to craft a response.

You can print a dynamic template using the [DynamicTemplate](/reference/functions/string#dynamictemplate) function. This takes an object or text parameter, which is then passed in to the template expression.

## verbtemplate

```xml
<verbtemplate name="name">text</template>
```

Creates or adds to a verb template of the specified name. Specifying multiple verb templates with the same name lets you handle multiple verbs with one template.

You can refer to verbtemplates within a [verb element](#verb), or using the "template" attribute of a [command element](#command).

The text can optionally include `#object#` as a stand-in for the object name; if it is omitted, the object name is assumed to be at the end. For example:

```xml
<verbtemplate name="wear">wear</verbtemplate>
<verbtemplate name="wear">put on</verbtemplate>
<verbtemplate name="wear">put #object# on</verbtemplate>
<verbtemplate name="wear">don</verbtemplate>
```

## function

```xml
<function name="name"optional type="type"optional parameters="parameters">script</function>
```

Creates a function.

If no type is specified, the function does not return a value.

If the function does return a value, the type should be one of the valid [Attribute Types](/reference/attributes/types/). Return a value within the function using the [return](/reference/script-commands#return) command.

If the function takes parameters, the parameters should be specified as a comma-delimited list.

For example:

```xml
<function name="FormatObjectList" type="string" parameters="preList, parent, preFinal, postList">
...
</function>
```

### The attributes of a function

**name:** This is the name of the function. Every function must have name, and that is the name you use to invoke the function in some other script.

**parameters:** These are the values (if any) passed into the function. You give them names, and when the function is called, those parameters must be set by giving values in the function call (e.g. MyFunction(a, b) ). The values are mapped to the parameters in the order they are given. If your function does not take input parameters, then you can omit this or leave it as an empty string.

**type:** This is the return type of the function (the value passed out), if the function returns a value. Some functions do, and some don't. If you use a "[return](/reference/script-commands#return)" statement in your function to send a value back to the caller, then you need to specify the return type, so that Quest Viva knows what type the function is expected to return. If your function does not return a value, then you can omit this or leave it an empty string.

Quest Viva will object if there is a return statement, but no type specified; or if there is a type specified, but no return statement.

### A Working Example

Here is a trivial example. It's a function to concatenate two strings and return the result. Clearly, you don't need this function (since you can just use the "+" yourself), but hopefully it illustrates how functions are set up.

```xml
<function name="ConcatStrings" parameters="s1, s2" type="string">
  return (s1 + s2)
</function>
```

This basically says, "We have a function called 'ConcatStrings', it takes two input parameters, which we will call 's1' and 's2' inside the function, and the function returns a string value."

The function would be invoked as:

```quest
s = ConcatStrings("Mama ", "Mia")
```

The resulting "s" would be "Mama Mia"

## command

```xml
<command name="name" pattern="pattern" unresolved="unresolved text" template="template name">script</command>
```

or

```xml
<command name="name">attributes</command>
```

All XML attributes are optional.

Creates a command. There are two syntaxes - one syntax lets you specify a pattern, some text to display when an object is unresolved, and the script to run. The second syntax is more open and flexible, and lets you specify everything by directly setting the attributes of the command object. The second syntax is preferred, although the first may be more concise.

All commands automatically inherit a "defaultcommand" type if it exists.

### Name

If a name is not specified, a unique name will be created. Using the first syntax allows Quest Viva to try and create a user-friendly name by taking the first word(s) of the specified pattern; otherwise the name will be something like "k1". It is best to always specify a name, as it will make debugging easier - the Debugger will show you a sensible name for your command. It will also let you easily change the behaviour of the command by setting its attributes when the game is in progress.

### Pattern

The "pattern" attribute of a command is a string - the regular expression that triggers the command. You can use friendlier syntax with type="simplepattern", which in Core.aslx is set as the implied type for a command "pattern" attribute, so you don't need to specify it. This will convert friendly syntax such as "look at \#object\#" into a regular expression. If you want to specify a regex yourself, you need to explicitly set type="string".

### Unresolved

The "unresolved" attribute is the text to print if the user enters the name of an object which is not in the current visible scope.

### Template

The "template" attribute specifies the command pattern to use, if the command pattern is defined by a [verbtemplate](#verbtemplate).

### Allow all

To handle "take all" and "drop all", the "take" and "drop" commands, for example, have "allow_all" set to true. When this is set to `true`, the script attribute will be sent an object list as "object" instead of a single object. In addition, it will be sent "multiple" which will be true to indicate the player used "all", and so the items need a prefix saying what they are.

### Scope

The scope attribute tells Quest Viva where to look first for objects for this command. See the "Alternative scope" section of [this page](/howto/commands/scope) for details.

## verb

```xml
<verboptional name="name"optional pattern="pattern"optional unresolved="unresolved text"optional property="attribute name"optional response="default response text"optional template="template name">script</verb>
```

or

```xml
<verboptional name="name">attributes</verb>
```

Creates a verb, which is a specialised type of [command element](#command) - so everything that applies to a command also applies to a verb. Underneath, verbs are just commands - if you look at them in the Debugger, they are the same thing. But they are designed to be easier to use than commands for the vast majority of commands which are of the form "command object", such as "look at thing", "eat food", "sit on bench" etc.

In addition to any "defaultcommand" type, verbs also inherit "defaultverb". In Core.aslx this provides the standard verb implementation. We take the object the player entered, and look for the attribute as specified by "property". Then:

-   if the attribute is a script, run it;
-   if the attribute is a string, print it;
-   if the attribute is not set, print the default verb response (e.g. "You can't eat it")
-   if the attribute is some other type, raise an error.

## type

```xml
<type name="name">properties</type>
```

Creates a type. The type element can contain properties and [\<inherit\> tags](#inherit).

Use an [\<inherit\> tag](#inherit) in an object definition to include all the type's properties in that object.

See [Types](/understanding/attributes-and-types).

## game

```xml
<game name="name">properties</game>
```

Defines the game and its global properties. Every ASLX file has exactly one `<game>` element.

### Bibliographic metadata

These fields describe the game for players and catalogues. Most are optional; new games created in the editor are given a `gameid`, `version`, `versioncode`, and `firstpublished` automatically. You can edit them on the game's **Setup** tab.

```xml
<game name="Cloak of Darkness">
  <subtitle>A basic IF sample</subtitle>
  <author>The Pixie</author>
  <version>1.0</version>
  <versioncode type="int">1</versioncode>
  <gameid>18ad63b5-78e2-4846-872b-9177d78cc5e6</gameid>
  <category>Fantasy</category>
  <firstpublished>2018</firstpublished>
  <cover>cover.png</cover>
  <description>From the specification here:...</description>
</game>
```

**name** (XML attribute)  
The title of the game. Written as the `name` attribute on the `<game>` tag, but stored internally as `gamename` (`game.gamename`). It appears on the title screen (when `showtitle` is enabled), in the `version` command output, and as the default transcript name.

**subtitle**  
An optional secondary title, shown under the game name on the title screen.

**author**  
The author's name. Shown on the title screen (when `showtitle` is enabled) and by the `version` command.

**version**  
A free-form version string for display (for example `"1.0"` or `"1.2-beta"`). Shown by the `version` command.

**versioncode**  
A non-negative integer version number (`type="int"`). Use `version` for the human-readable label and `versioncode` for a number that tools and catalogues can compare. Bump `versioncode` whenever you publish a new release.

**gameid**  
A unique identifier for the game, also known as an "IFID" under the [Treaty of Babel](https://babel.ifarchive.org/). Stored as a UUID string (for example `18ad63b5-78e2-4846-872b-9177d78cc5e6`). The editor creates one when you start a new game. Keep the same `gameid` across updates of the same game; only generate a new one if you have copied a game to create a different game. The `version` command displays this as the IFID.

**category**  
A genre or category string used when listing the game (for example `Fantasy`, `Mystery`, `Puzzle`). The editor offers a dropdown of common values, but any string is allowed.

**firstpublished**  
The year the game was first released, typically a four-digit year such as `2018`.

**cover**  
The filename of the cover image, relative to the game folder (for example `cover.png`). Recommended format is a 512×512 PNG.

**description**  
A plain text blurb describing the game for catalogues and listings. This is not the same as an object's [description](/reference/attributes/all#description) attribute (which describes a room or item in play).

**difficulty** (legacy)  
A difficulty rating string. Typical values were `Easy`, `Medium`, `Hard`, and `Very Hard`. Removed from the editor in Quest 5.6.2; still present in some older games.

**cruelty** (legacy)  
A [Zarfian cruelty scale](https://www.ifwiki.org/Cruelty_scale) rating. Typical values were `Merciful`, `Polite`, `Tough`, `Nasty`, and `Cruel`. Removed from the editor in Quest 5.6.2; still present in some older games.

### Game attributes handled by Core.aslx:

-   [allobjects](/reference/attributes/all#allobjects)
-   [appendobjectdescription](/reference/attributes/all#appendobjectdescription)
-   [autodescription](/reference/attributes/all#autodescription)
-   [autodescription\_description](/reference/attributes/all#autodescription)
-   [autodescription\_description\_newline](/reference/attributes/all#autodescription)
-   [autodescription\_youarein](/reference/attributes/all#autodescription)
-   [autodescription\_youarein\_useprefix](/reference/attributes/all#autodescription)
-   [autodescription\_youarein\_newline](/reference/attributes/all#autodescription)
-   [autodescription\_youcango](/reference/attributes/all#autodescription)
-   [autodescription\_youcango\_newline](/reference/attributes/all#autodescription)
-   [autodescription\_youcansee](/reference/attributes/all#autodescription)
-   [autodescription\_youcansee\_newline](/reference/attributes/all#autodescription)
-   [autodisplayverbs](/reference/attributes/all#autodisplayverbs)
-   [backgroundimage](/reference/attributes/all#backgroundimage)
-   [backgroundopacity](/reference/attributes/all#backgroundopacity)
-   [clearframe](/reference/attributes/all#clearframe)
-   [compassdirections](/reference/attributes/all#compassdirections)
-   [defaultbackground](/reference/attributes/all#defaultbackground)
-   [defaultfont](/reference/attributes/all#defaultfont)
-   [defaultfontsize](/reference/attributes/all#defaultfontsize)
-   [defaultforeground](/reference/attributes/all#defaultforeground)
-   [defaultlinkforeground](/reference/attributes/all#defaultlinkforeground)
-   [defaultwebfont](/reference/attributes/all#defaultwebfont)
-   [displayroomdescriptiononstart](/reference/attributes/all#displayroomdescriptiononstart)
-   [echohyperlinks](/reference/attributes/all#echohyperlinks)
-   [enablehyperlinks](/reference/attributes/all#enablehyperlinks)
-   [gridmap](/reference/attributes/all#gridmap)
-   [languageid](/reference/attributes/all#languageid)
-   [mapscale](/reference/attributes/all#mapscale)
-   [mapsize](/reference/attributes/all#mapsize)
-   [menubackground](/reference/attributes/all#menubackground)
-   [menuforeground](/reference/attributes/all#menuforeground)
-   [menufont](/reference/attributes/all#menufont)
-   [menufontsize](/reference/attributes/all#menufontsize)
-   [menuhoverbackground](/reference/attributes/all#menuhoverbackground)
-   [menuhoverforeground](/reference/attributes/all#menuhoverforeground)
-   [parserignoreprefixes](/reference/attributes/all#parserignoreprefixes)
-   [setbackgroundopacity](/reference/attributes/all#setbackgroundopacity)
-   [showdescriptiononenter](/reference/attributes/all#showdescriptiononenter)
-   [showhealth](/reference/attributes/all#showhealth)
-   [showpanes](/reference/attributes/all#showpanes)
-   [showscore](/reference/attributes/all#showscore)
-   [start](/reference/attributes/all#start)
-   [statusattributes](/reference/attributes/all#statusattributes)
-   [useframe](/reference/attributes/all#useframe)
-   [underlinehyperlinks](/reference/attributes/all#underlinehyperlinks)

## object

```xml
<object name="name">attributes</object>
```

Creates an object.

Objects can contain nested object definitions. In that case, all sub-objects are children of the parent object. This is how rooms work - rooms are just objects which contain other objects.

Object attributes handled by Core.aslx:

-   [alt](/reference/attributes/all#alt)
-   [alias](/reference/attributes/all#alias)
-   [article](/reference/attributes/all#article)
-   [ask](/reference/attributes/all#ask)
-   [askdefault](/reference/attributes/all#askdefault)
-   [autoopen](/reference/attributes/all#autoopen)
-   [autounlock](/reference/attributes/all#autounlock)
-   [beforefirstenter](/reference/attributes/all#beforefirstenter)
-   [canlockopen](/reference/attributes/all#canlockopen)
-   [close](/reference/attributes/all#close)
-   [closescript](/reference/attributes/all#closescript)
-   [containerfullmessage](/reference/attributes/all#containerfullmessage)
-   [contentsprefix](/reference/attributes/all#contentsprefix)
-   [dark](/reference/attributes/all#dark)
-   [darklevel](/reference/attributes/all#darklevel)
-   [descprefix](/reference/attributes/all#descprefix)
-   [description](/reference/attributes/all#description)
-   [displayverbs](/reference/attributes/all#displayverbs)
-   [drop](/reference/attributes/all#drop)
-   [dropmsg](/reference/attributes/all#dropmsg)
-   [enter](/reference/attributes/all#enter)
-   [exitslistprefix](/reference/attributes/all#exitslistprefix)
-   [firstenter](/reference/attributes/all#firstenter)
-   [gender](/reference/attributes/all#gender)
-   [give](/reference/attributes/all#give)
-   [giveanything](/reference/attributes/all#giveanything)
-   [givesingle](/reference/attributes/all#givesingle)
-   [giveto](/reference/attributes/all#giveto)
-   [givetoanything](/reference/attributes/all#givetoanything)
-   [grid\_border](/reference/attributes/all#grid_border)
-   [grid\_bordersides](/reference/attributes/all#grid_bordersides)
-   [grid\_borderwidth](/reference/attributes/all#grid_borderwidth)
-   [grid\_fill](/reference/attributes/all#grid_fill)
-   [grid\_label](/reference/attributes/all#grid_label)
-   [grid\_length](/reference/attributes/all#grid_length)
-   [grid\_parent\_offset\_auto](/reference/attributes/all#grid_parent_offset_auto)
-   [grid\_parent\_offset\_x](/reference/attributes/all#grid_parent_offset_x)
-   [grid\_parent\_offset\_y](/reference/attributes/all#grid_parent_offset_y)
-   [grid\_render](/reference/attributes/all#grid_render)
-   [grid\_width](/reference/attributes/all#grid_width)
-   [hidechildren](/reference/attributes/all#hidechildren)
-   [inventoryverbs](/reference/attributes/all#inventoryverbs)
-   [isopen](/reference/attributes/all#isopen)
-   [key](/reference/attributes/all#key)
-   [lightstrength](/reference/attributes/all#lightstrength)
-   [locked](/reference/attributes/all#locked)
-   [lockmessage](/reference/attributes/all#lockmessage)
-   [listchildren](/reference/attributes/all#listchildren)
-   [listchildrenprefix](/reference/attributes/all#listchildrenprefix)
-   [look](/reference/attributes/all#look)
-   [maxobjects](/reference/attributes/all#maxobjects)
-   [nokeymessage](/reference/attributes/all#nokeymessage)
-   [objectslistprefix](/reference/attributes/all#objectslistprefix)
-   [onclose](/reference/attributes/all#onclose)
-   [ondrop](/reference/attributes/all#ondrop)
-   [onlock](/reference/attributes/all#onlock)
-   [onopen](/reference/attributes/all#onopen)
-   [onswitchoff](/reference/attributes/all#onswitchoff)
-   [onswitchon](/reference/attributes/all#onswitchon)
-   [ontake](/reference/attributes/all#ontake)
-   [onunlock](/reference/attributes/all#onunlock)
-   [open](/reference/attributes/all#open)
-   [openscript](/reference/attributes/all#openscript)
-   [parent](/reference/attributes/all#parent)
-   [picture](/reference/attributes/all#picture-attribute)
-   [pov\_alias](/reference/attributes/all#pov_alias)
-   [pov\_alt](/reference/attributes/all#pov_alt)
-   [pov\_article](/reference/attributes/all#pov_article)
-   [pov\_gender](/reference/attributes/all#pov_gender)
-   [pov\_look](/reference/attributes/all#pov_look)
-   [prefix](/reference/attributes/all#prefix)
-   [scenery](/reference/attributes/all#scenery)
-   [selfuseanything](/reference/attributes/all#selfuseanything)
-   [selfuseon](/reference/attributes/all#selfuseon)
-   [statusattributes](/reference/attributes/all#statusattributes)
-   [suffix](/reference/attributes/all#suffix)
-   [switchedoffdesc](/reference/attributes/all#switchedoffdesc)
-   [switchedon](/reference/attributes/all#switchedon)
-   [switchedondesc](/reference/attributes/all#switchedondesc)
-   [switchoffmsg](/reference/attributes/all#switchoffmsg)
-   [switchonmsg](/reference/attributes/all#switchonmsg)
-   [take](/reference/attributes/all#take)
-   [takemsg](/reference/attributes/all#takemsg)
-   [transparent](/reference/attributes/all#transparent)
-   [tell](/reference/attributes/all#tell)
-   [telldefault](/reference/attributes/all#telldefault)
-   [unlockmessage](/reference/attributes/all#unlockmessage)
-   [use](/reference/attributes/all#use)
-   [useanything](/reference/attributes/all#useanything)
-   [usedefaultprefix](/reference/attributes/all#usedefaultprefix)
-   [useon](/reference/attributes/all#useon)
-   [visible](/reference/attributes/all#visible)
-   [visited](/reference/attributes/all#visited)
-   [volume](/reference/attributes/all#volume)

Object types defined by Core.aslx:

-   [container](/reference/attributes/all#container)
-   [container\_base](/reference/attributes/all#container_base)
-   [container\_closed](/reference/attributes/all#container_closed)
-   [container\_limited](/reference/attributes/all#container_limited)
-   [container\_lockable](/reference/attributes/all#container_lockable)
-   [container\_open](/reference/attributes/all#container_open)
-   [defaultobject](/reference/attributes/all#defaultobject)
-   [edible](/reference/attributes/all#edible)
-   [editor\_object](/reference/attributes/all#editor_object)
-   [editor\_room](/reference/attributes/all#editor_room)
-   [female](/reference/attributes/all#female)
-   [femaleplural](/reference/attributes/all#femaleplural)
-   [male](/reference/attributes/all#male)
-   [maleplural](/reference/attributes/all#maleplural)
-   [namedfemale](/reference/attributes/all#namedfemale)
-   [namedmale](/reference/attributes/all#namedmale)
-   [openable](/reference/attributes/all#openable)
-   [plural](/reference/attributes/all#plural)
-   [surface](/reference/attributes/all#surface)
-   [switchable](/reference/attributes/all#switchable)

## exit

```xml
<exit alias="direction or displayed exit name" name="name" to="to room">attributes</exit>
```

Creates an exit from the exit's parent room to the specified room.

The alias might be something like "east", "north", or the name of a room that the player can go to.

The name is optional. If no name is specified, Quest Viva will generate a name for the exit.

Attributes:

alias  
[string](/reference/attributes/types#string) exit alias

grid\_length  
[int](/reference/attributes/types#int) length of exit line on map in grid units

grid\_offset\_x  
X offset of exit position on grid

grid\_offset\_y  
Y offset of exit position on grid

grid\_render  
see [grid\_render](/reference/attributes/all#grid_render) object attribute

lightstrength  
see [lightstrength](/reference/attributes/all#lightstrength) object attribute

locked  
[boolean](/reference/attributes/types#boolean) specifying if exit is locked

lockmessage  
[string](/reference/attributes/types#string) to display when exit is locked

look  
[string](/reference/attributes/types#string) description to print when the player looks in this direction, or [script](/reference/attributes/types#script) to run

lookonly  
[boolean](/reference/attributes/types#boolean) - if true, the player can't move in this direction, only look

prefix  
[string](/reference/attributes/types#string) to print before exit name in room descriptions

script  
[script](/reference/attributes/types#script) to run instead of moving the player

suffix  
[string](/reference/attributes/types#string) to print after exit name in room descriptions

visible  
[boolean](/reference/attributes/types#boolean) - if false, exit is not available (as if the exit's parent was null)

## walkthrough

```xml
<walkthrough name="name" > <steps>steps</steps> </walkthrough>
```

Defines a walkthrough with a list of steps. Each step should be on its own line.

Walkthrough elements can be nested within each other to create a hierarchy.

See [Walkthroughs](/howto/testing/walkthroughs).

## timer

```xml
<timer name="name">attributes</timer>
```

Timer attributes:

enabled  
[boolean](/reference/attributes/types#boolean) specifying whether timer is ticking

interval  
[int](/reference/attributes/types#int) specifying number of seconds between tick events

script  
[script](/reference/attributes/types#script) specifying what to do when timer ticks

## turnscript

```xml
<turnscript name="name">attributes</turnscript>
```

Turnscript attributes:

enabled  
[boolean](/reference/attributes/types#boolean) specifying whether turnscript is active

script  
[script](/reference/attributes/types#script) specifying what to do after each turn

Note that as of 5.7.2, turnscripts run in alphabetic order (in earlier versions the order could change unexpectedly). To have turnscripts in a certain order, prefix them "ts01_", "ts02_", ... .

## implied

```xml
<implied element="element" property="attribute name" type="type"/>
```

Specifies an implied type. For example, the "alt" attribute on an object is usually a list, so to save having to specify the type each time we can use this:

```xml
<implied element="object" property="alt" type="list">
```

This means we can specify an alt attribute without specifying the type:

```xml
<alt>telly; television</alt>
```

## delegate

```xml
<delegate name="name"optional type="type"optional parameters="parameters">properties</delegate>
```

Creates a delegate type. Delegates are script properties that can be called like functions. The delegate tag defines the function signature (the parameters passed to the function and its return type, if any), and then an object can provide its own implementation of the delegate function.

You can run delegate functions on objects using the [rundelegate](/reference/script-commands#rundelegate) command (if the delegate does not return a value) or using the [RunDelegateFunction](/reference/functions/general#rundelegatefunction) function (for delegates that do return a value).

See [Using delegates](/customise/delegates)

## javascript

```xml
<javascript src="filename"/>
```

Adds the specified Javascript file to the player interface.

## editor

```xml
<editor name="name">attributes</editor>
```

This defines the Editor tabs and controls for a particular element type or script command.

It should have nested [tab](#tab) elements and [control](#control) elements. "Name" is optional, but if specified it means the nested tab controls can set their [parent](/reference/attributes/all#parent) attribute without having to be nested in the parent editor XML definition.

Attributes:

appliesto  
[string](/reference/attributes/types#string) specifying which element type or script command this editor definition applies to

## tab

```xml
<tab>attributes</tab>
```

This defines a tab within an [editor element](#editor).

It should have nested [control](#control) elements.

Attributes:

caption  
[string](/reference/attributes/types#string) specifying the caption for the tab

## control

```xml
<control>nameattributes</control>
```

This defines the controls within a [tab element](#tab).

Attributes:

attribute  
[string](/reference/attributes/types#string) specifying the attribute name that this control applies to

caption  
[string](/reference/attributes/types#string) specifying the label for the control

controltype  
[string](/reference/attributes/types#string) specifying the control type

See [Script commands for your functions](/customise/editor-tabs#script-commands-for-your-functions)

## resource

```xml
<resource src="filename"/>
```

Specifies that a particular file should be included when building a .quest package.

**This is usually not required** - the Packager will pick up all supported files in the same directory as the game. The only time this is required is when an additional file in the library directory is required - so this element is only intended to be used by the Core library.

## inherit

```xml
<inherit name="name"/>
```

Within an object, type, command or exit definition, inherits properties from the specified type.

See [Types](/understanding/attributes-and-types).
