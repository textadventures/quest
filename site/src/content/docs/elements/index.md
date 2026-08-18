---
title: XML elements
sidebar:
  order: 25
---

Note that this is about XML elements in the ASLX file, which is not quite the same as the elements in the game.

## asl

    <asl version="580">all game content</asl>

To load any game, the top-level element must be an \<asl\> element as shown above. All other XML elements in the file must appear within this tag.

## library

    <library>all library content</library>

The top-level element of any library must be a \<library\> element as shown above. All other XML elements in the file must appear within this tag.

## include

    <include ref="filename"/>

Loads the specified library.

## template

    <template name="name">text</template>

Creates a template of the specified name. You can print the template's text using the [Template](/functions/string#template) function.

Within a language library, a template may define a **templatetype** of "command", for example:

     <template templatetype="command" name="undo">^undo$</template>

This simply is a flag to the Editor to prevent it from showing the template in the list of templates (as the way to edit it would be to edit the associated command pattern).

Note that it is important to have templates defined in the right place in the code. If your template is to override an existing template, then it has to come *after* the language file include. However, it has to come *before* the template is used in the code, which should be before the core library file include. As of version 5.2 Quest does not do this, so you will need to manually move the templates to the right place. Your game file should start something like this:

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

## dynamictemplate

    <dynamictemplate name="name">expression</template>

A dynamictemplate is used in a similar way as [template](#template), except that its value is an expression, not a static string. The expression will have access to an object called "object", which you can use to craft a response.

You can print a dynamic template using the [DynamicTemplate](/functions/string#dynamictemplate) function. This takes an object or text parameter, which is then passed in to the template expression.

## verbtemplate

    <verbtemplate name="name">text</template>

Creates or adds to a verb template of the specified name. Specifying multiple verb templates with the same name lets you handle multiple verbs with one template.

You can refer to verbtemplates within a [verb element](#verb), or using the "template" attribute of a [command element](#command).

The text can optionally include `#object#` as a stand-in for the object name; if it is omitted, the object name is assumed to be at the end. For example:

```
<verbtemplate name="wear">wear</verbtemplate>
<verbtemplate name="wear">put on</verbtemplate>
<verbtemplate name="wear">put #object# on</verbtemplate>
<verbtemplate name="wear">don</verbtemplate>
```

## function

    <function name="name"optional type="type"optional parameters="parameters">script</function>

Creates a function.

If no type is specified, the function does not return a value.

If the function does return a value, the type should be one of the valid [Attribute Types](/types/). Return a value within the function using the [return](/scripts#return) command.

If the function takes parameters, the parameters should be specified as a comma-delimited list.

For example:

    <function name="FormatObjectList" type="string" parameters="preList, parent, preFinal, postList">
    ...
	</function>

### The attributes of a function

**name:** This is the name of the function. Every function must have name, and that is the name you use to invoke the function in some other script.

**parameters:** These are the values (if any) passed into the function. You give them names, and when the function is called, those parameters must be set by giving values in the function call (e.g. MyFunction(a, b) ). The values are mapped to the parameters in the order they are given. If your function does not take input parameters, then you can omit this or leave it as an empty string.

**type:** This is the return type of the function (the value passed out), if the function returns a value. Some functions do, and some don't. If you use a "[return](/scripts#return)" statement in your function to send a value back to the caller, then you need to specify the return type, so that Quest knows what type the function is expected to return. If your function does not return a value, then you can omit this or leave it an empty string.

Quest will object if there is a return statement, but no type specified; or if there is a type specified, but no return statement.

### A Working Example

Here is a trivial example. It's a function to concatenate two strings and return the result. Clearly, you don't need this function (since you can just use the "+" yourself), but hopefully it illustrates how functions are set up.

        <function name="ConcatStrings" parameters="s1, s2" type="string">
          return (s1 + s2)
        </function>

This basically says, "We have a function called 'ConcatStrings', it takes two input parameters, which we will call 's1' and 's2' inside the function, and the function returns a string value."

The function would be invoked as:

        s = ConcatStrings("Mama ", "Mia")

The resulting "s" would be "Mama Mia"

## command

    <command name="name" pattern="pattern" unresolved="unresolved text" template="template name">script</command>

or

    <command name="name">attributes</command>

All XML attributes are optional.

Creates a command. There are two syntaxes - one syntax lets you specify a pattern, some text to display when an object is unresolved, and the script to run. The second syntax is more open and flexible, and lets you specify everything by directly setting the attributes of the command object. The second syntax is preferred, although the first may be more concise.

All commands automatically inherit a "defaultcommand" type if it exists.

### Name

If a name is not specified, a unique name will be created. Using the first syntax allows Quest to try and create a user-friendly name by taking the first word(s) of the specified pattern; otherwise the name will be something like "k1". I recommend you always specify a name, as it will make debugging easier - the Debugger will show you a sensible name for your command. It will also let you easily change the behaviour of the command by setting its attributes when the game is in progress.

### Pattern

The "pattern" attribute of a command is a string - the regular expression that triggers the command. You can use friendlier syntax with type="simplepattern", which in Core.aslx is set as the implied type for a command "pattern" attribute, so you don't need to specify it. This will convert friendly syntax such as "look at \#object\#" into a regular expression. If you want to specify a regex yourself, you need to explicitly set type="string".

### Unresolved

The "unresolved" attribute is the text to print if the user enters the name of an object which is not in the current visible scope.

### Template

The "template" attribute specifies the command pattern to use, if the command pattern is defined by a [verbtemplate](#verbtemplate).

### Allow all

To handle "take all" and "drop all", the "take" and "drop" commands, for example, have "allow_all" set to true. When this is set to `true`, the script attribute will be sent an object list as "object" instead of a single object. In addition, it will be sent "multiple" which will be true to indicate the player used "all", and so the items need a prefix saying what they are.

### Scope

The scope attribute tells Quest where to look first for objects for this command. See the "Alternative scope" section of [this page](/howto/commands/advanced_scope) for details.

## verb

    <verboptional name="name"optional pattern="pattern"optional unresolved="unresolved text"optional property="attribute name"optional response="default response text"optional template="template name">script</verb>

or

    <verboptional name="name">attributes</verb>

Creates a verb, which is a specialised type of [command element](#command) - so everything that applies to a command also applies to a verb. Underneath, verbs are just commands - if you look at them in the Debugger, they are the same thing. But they are designed to be easier to use than commands for the vast majority of commands which are of the form "command object", such as "look at thing", "eat food", "sit on bench" etc.

In addition to any "defaultcommand" type, verbs also inherit "defaultverb". In Core.aslx this provides the standard verb implementation. We take the object the player entered, and look for the attribute as specified by "property". Then:

-   if the attribute is a script, run it;
-   if the attribute is a string, print it;
-   if the attribute is not set, print the default verb response (e.g. "You can't eat it")
-   if the attribute is some other type, raise an error.

## type

    <type name="name">properties</type>

Creates a type. The type element can contain properties and [\<inherit\> tags](#inherit).

Use an [\<inherit\> tag](#inherit) in an object definition to include all the type's properties in that object.

See [Types](/advanced-topics/about_types).

## game

    <game name="name">properties</game>

Defines the name of the game and any global properties.

Game attributes handled by Core.aslx:

-   [allobjects](/attributes#allobjects)
-   [appendobjectdescription](/attributes#appendobjectdescription)
-   [autodescription](/attributes#autodescription)
-   [autodescription\_description](/attributes#autodescription)
-   [autodescription\_description\_newline](/attributes#autodescription)
-   [autodescription\_youarein](/attributes#autodescription)
-   [autodescription\_youarein\_useprefix](/attributes#autodescription)
-   [autodescription\_youarein\_newline](/attributes#autodescription)
-   [autodescription\_youcango](/attributes#autodescription)
-   [autodescription\_youcango\_newline](/attributes#autodescription)
-   [autodescription\_youcansee](/attributes#autodescription)
-   [autodescription\_youcansee\_newline](/attributes#autodescription)
-   [autodisplayverbs](/attributes#autodisplayverbs)
-   [backgroundimage](/attributes#backgroundimage)
-   [backgroundopacity](/attributes#backgroundopacity)
-   [clearframe](/attributes#clearframe)
-   [compassdirections](/attributes#compassdirections)
-   [defaultbackground](/attributes#defaultbackground)
-   [defaultfont](/attributes#defaultfont)
-   [defaultfontsize](/attributes#defaultfontsize)
-   [defaultforeground](/attributes#defaultforeground)
-   [defaultlinkforeground](/attributes#defaultlinkforeground)
-   [defaultwebfont](/attributes#defaultwebfont)
-   [description](/attributes#description)
-   [displayroomdescriptiononstart](/attributes#displayroomdescriptiononstart)
-   [echohyperlinks](/attributes#echohyperlinks)
-   [enablehyperlinks](/attributes#enablehyperlinks)
-   [gridmap](/attributes#gridmap)
-   [languageid](/attributes#languageid)
-   [mapscale](/attributes#mapscale)
-   [mapsize](/attributes#mapsize)
-   [menubackground](/attributes#menubackground)
-   [menuforeground](/attributes#menuforeground)
-   [menufont](/attributes#menufont)
-   [menufontsize](/attributes#menufontsize)
-   [menuhoverbackground](/attributes#menuhoverbackground)
-   [menuhoverforeground](/attributes#menuhoverforeground)
-   [parserignoreprefixes](/attributes#parserignoreprefixes)
-   [setbackgroundopacity](/attributes#setbackgroundopacity)
-   [showdescriptiononenter](/attributes#showdescriptiononenter)
-   [showhealth](/attributes#showhealth)
-   [showpanes](/attributes#showpanes)
-   [showscore](/attributes#showscore)
-   [start](/attributes#start)
-   [statusattributes](/attributes#statusattributes)
-   [useframe](/attributes#useframe)
-   [underlinehyperlinks](/attributes#underlinehyperlinks)

## object

    <object name="name">attributes</object>

Creates an object.

Objects can contain nested object definitions. In that case, all sub-objects are children of the parent object. This is how rooms work - rooms are just objects which contain other objects.

Object attributes handled by Core.aslx:

-   [alt](/attributes#alt)
-   [alias](/attributes#alias)
-   [article](/attributes#article)
-   [ask](/attributes#ask)
-   [askdefault](/attributes#askdefault)
-   [autoopen](/attributes#autoopen)
-   [autounlock](/attributes#autounlock)
-   [beforefirstenter](/attributes#beforefirstenter)
-   [canlockopen](/attributes#canlockopen)
-   [close](/attributes#close)
-   [closescript](/attributes#closescript)
-   [containerfullmessage](/attributes#containerfullmessage)
-   [contentsprefix](/attributes#contentsprefix)
-   [dark](/attributes#dark)
-   [darklevel](/attributes#darklevel)
-   [descprefix](/attributes#descprefix)
-   [description](/attributes#description)
-   [displayverbs](/attributes#displayverbs)
-   [drop](/attributes#drop)
-   [dropmsg](/attributes#dropmsg)
-   [enter](/attributes#enter)
-   [exitslistprefix](/attributes#exitslistprefix)
-   [firstenter](/attributes#firstenter)
-   [gender](/attributes#gender)
-   [give](/attributes#give)
-   [giveanything](/attributes#giveanything)
-   [givesingle](/attributes#givesingle)
-   [giveto](/attributes#giveto)
-   [givetoanything](/attributes#givetoanything)
-   [grid\_border](/attributes#grid_border)
-   [grid\_bordersides](/attributes#grid_bordersides)
-   [grid\_borderwidth](/attributes#grid_borderwidth)
-   [grid\_fill](/attributes#grid_fill)
-   [grid\_label](/attributes#grid_label)
-   [grid\_length](/attributes#grid_length)
-   [grid\_parent\_offset\_auto](/attributes#grid_parent_offset_auto)
-   [grid\_parent\_offset\_x](/attributes#grid_parent_offset_x)
-   [grid\_parent\_offset\_y](/attributes#grid_parent_offset_y)
-   [grid\_render](/attributes#grid_render)
-   [grid\_width](/attributes#grid_width)
-   [hidechildren](/attributes#hidechildren)
-   [inventoryverbs](/attributes#inventoryverbs)
-   [isopen](/attributes#isopen)
-   [key](/attributes#key)
-   [lightstrength](/attributes#lightstrength)
-   [locked](/attributes#locked)
-   [lockmessage](/attributes#lockmessage)
-   [listchildren](/attributes#listchildren)
-   [listchildrenprefix](/attributes#listchildrenprefix)
-   [look](/attributes#look)
-   [maxobjects](/attributes#maxobjects)
-   [nokeymessage](/attributes#nokeymessage)
-   [objectslistprefix](/attributes#objectslistprefix)
-   [onclose](/attributes#onclose)
-   [ondrop](/attributes#ondrop)
-   [onlock](/attributes#onlock)
-   [onopen](/attributes#onopen)
-   [onswitchoff](/attributes#onswitchoff)
-   [onswitchon](/attributes#onswitchon)
-   [ontake](/attributes#ontake)
-   [onunlock](/attributes#onunlock)
-   [open](/attributes#open)
-   [openscript](/attributes#openscript)
-   [parent](/attributes#parent)
-   [picture](/attributes#picture-attribute)
-   [pov\_alias](/attributes#pov_alias)
-   [pov\_alt](/attributes#pov_alt)
-   [pov\_article](/attributes#pov_article)
-   [pov\_gender](/attributes#pov_gender)
-   [pov\_look](/attributes#pov_look)
-   [prefix](/attributes#prefix)
-   [scenery](/attributes#scenery)
-   [selfuseanything](/attributes#selfuseanything)
-   [selfuseon](/attributes#selfuseon)
-   [statusattributes](/attributes#statusattributes)
-   [suffix](/attributes#suffix)
-   [switchedoffdesc](/attributes#switchedoffdesc)
-   [switchedon](/attributes#switchedon)
-   [switchedondesc](/attributes#switchedondesc)
-   [switchoffmsg](/attributes#switchoffmsg)
-   [switchonmsg](/attributes#switchonmsg)
-   [take](/attributes#take)
-   [takemsg](/attributes#takemsg)
-   [transparent](/attributes#transparent)
-   [tell](/attributes#tell)
-   [telldefault](/attributes#telldefault)
-   [unlockmessage](/attributes#unlockmessage)
-   [use](/attributes#use)
-   [useanything](/attributes#useanything)
-   [usedefaultprefix](/attributes#usedefaultprefix)
-   [useon](/attributes#useon)
-   [visible](/attributes#visible)
-   [visited](/attributes#visited)
-   [volume](/attributes#volume)

Object types defined by Core.aslx:

-   [container](/attributes#container)
-   [container\_base](/attributes#container_base)
-   [container\_closed](/attributes#container_closed)
-   [container\_limited](/attributes#container_limited)
-   [container\_lockable](/attributes#container_lockable)
-   [container\_open](/attributes#container_open)
-   [defaultobject](/attributes#defaultobject)
-   [edible](/attributes#edible)
-   [editor\_object](/attributes#editor_object)
-   [editor\_room](/attributes#editor_room)
-   [female](/attributes#female)
-   [femaleplural](/attributes#femaleplural)
-   [male](/attributes#male)
-   [maleplural](/attributes#maleplural)
-   [namedfemale](/attributes#namedfemale)
-   [namedmale](/attributes#namedmale)
-   [openable](/attributes#openable)
-   [plural](/attributes#plural)
-   [surface](/attributes#surface)
-   [switchable](/attributes#switchable)

## exit

    <exit alias="direction or displayed exit name" name="name" to="to room">attributes</exit>

Creates an exit from the exit's parent room to the specified room.

The alias might be something like "east", "north", or the name of a room that the player can go to.

The name is optional. If no name is specified, Quest will generate a name for the exit.

Attributes:

alias  
[string](/types#string) exit alias

grid\_length  
[int](/types#int) length of exit line on map in grid units

grid\_offset\_x  
X offset of exit position on grid

grid\_offset\_y  
Y offset of exit position on grid

grid\_render  
see [grid\_render](/attributes#grid_render) object attribute

lightstrength  
see [lightstrength](/attributes#lightstrength) object attribute

locked  
[boolean](/types#boolean) specifying if exit is locked

lockmessage  
[string](/types#string) to display when exit is locked

look  
[string](/types#string) description to print when the player looks in this direction, or [script](/types#script) to run

lookonly  
[boolean](/types#boolean) - if true, the player can't move in this direction, only look

prefix  
[string](/types#string) to print before exit name in room descriptions

script  
[script](/types#script) to run instead of moving the player

suffix  
[string](/types#string) to print after exit name in room descriptions

visible  
[boolean](/types#boolean) - if false, exit is not available (as if the exit's parent was null)

## walkthrough

    <walkthrough name="name" > <steps>steps</steps> </walkthrough>

Defines a walkthrough with a list of steps. Each step should be on its own line.

Walkthrough elements can be nested within each other to create a hierarchy.

See [Walkthroughs](/howto/scripting/using_walkthroughs).

## timer

    <timer name="name">attributes</timer>

Timer attributes:

enabled  
[boolean](/types#boolean) specifying whether timer is ticking

interval  
[int](/types#int) specifying number of seconds between tick events

script  
[script](/types#script) specifying what to do when timer ticks

## turnscript

    <turnscript name="name">attributes</turnscript>

Turnscript attributes:

enabled  
[boolean](/types#boolean) specifying whether turnscript is active

script  
[script](/types#script) specifying what to do after each turn

Note that as of 5.7.2, turnscripts run in alphabetic order (in earlier versions the order could change unexpectedly). To have turnscripts in a certain order, prefix them "ts01_", "ts02_", ... .

## implied

    <implied element="element" property="attribute name" type="type"/>

Specifies an implied type. For example, the "alt" attribute on an object is usually a list, so to save having to specify the type each time we can use this:

     <implied element="object" property="alt" type="list">

This means we can specify an alt attribute without specifying the type:

     <alt>telly; television</alt>

## delegate

    <delegate name="name"optional type="type"optional parameters="parameters">properties</delegate>

Creates a delegate type. Delegates are script properties that can be called like functions. The delegate tag defines the function signature (the parameters passed to the function and its return type, if any), and then an object can provide its own implementation of the delegate function.

You can run delegate functions on objects using the [rundelegate](/scripts#rundelegate) command (if the delegate does not return a value) or using the [RunDelegateFunction](/functions/general#rundelegatefunction) function (for delegates that do return a value).

See [Using delegates](/advanced-topics/using_delegates)

## javascript

    <javascript src="filename"/>

Adds the specified Javascript file to the player interface.

## editor

    <editor name="name">attributes</editor>

This defines the Editor tabs and controls for a particular element type or script command.

It should have nested [tab](#tab) elements and [control](#control) elements. "Name" is optional, but if specified it means the nested tab controls can set their [parent](/attributes#parent) attribute without having to be nested in the parent editor XML definition.

Attributes:

appliesto  
[string](/types#string) specifying which element type or script command this editor definition applies to

## tab

    <tab>attributes</tab>

This defines a tab within an [editor element](#editor).

It should have nested [control](#control) elements.

Attributes:

caption  
[string](/types#string) specifying the caption for the tab

## control

    <control>nameattributes</control>

This defines the controls within a [tab element](#tab).

Attributes:

attribute  
[string](/types#string) specifying the attribute name that this control applies to

caption  
[string](/types#string) specifying the label for the control

controltype  
[string](/types#string) specifying the control type

See [Editor user interface elements](/advanced-topics/editor_user_interface_elements)

## resource

    <resource src="filename"/>

Specifies that a particular file should be included when building a .quest package.

**This is usually not required** - the Packager will pick up all supported files in the same directory as the game. The only time this is required is when an additional file in the library directory is required - so this element is only intended to be used by the Core library.

## inherit

    <inherit name="name"/>

Within an object, type, command or exit definition, inherits properties from the specified type.

See [Types](/advanced-topics/about_types).
