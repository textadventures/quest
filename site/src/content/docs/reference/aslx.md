---
title: ASLX file format
sidebar:
  order: 29
---

Quest Viva uses an XML-based file format, and files have an .aslx extension. If you open the raw XML code view in the editor toolbar, you can see the XML code that is your game. 

Generally you have no need to look at the full code view, but just occasionally it is useful, for example if you are creating or editing a library, or when spell checking, and some idea of XML is useful.

Here is a simple game, written the way the current editor saves it:

```quest
<!--Saved by Quest Viva 6.0.0-->
<asl version="600">
  <include ref="English.aslx" />
  <include ref="Core.aslx" />
  <game name="Test ASLX Game">
    <gameid>5d6e2a4c-6f0b-4c2e-9a51-3b7f1c8d2e90</gameid>
    <version>1.0</version>
    <firstpublished>2026</firstpublished>
  </game>
  <object name="lounge">
    <inherit name="editor_room" />
    <description>A comfortable lounge.</description>
    <object name="player">
      <inherit name="editor_object" />
      <inherit name="editor_player" />
    </object>
    <object name="sofa">
      <inherit name="editor_object" />
      <look>Just a sofa.</look>
      <take type="script">
        msg ("Example script attribute")
      </take>
      <attr name="feature_usegive" type="boolean">true</attr>
      <use type="script">
        msg ("You sit on the sofa for a while.")
      </use>
    </object>
    <exit alias="east" to="hall">
      <inherit name="eastdirection" />
    </exit>
  </object>
  <object name="hall">
    <inherit name="editor_room" />
    <exit alias="east" to="kitchen">
      <inherit name="eastdirection" />
    </exit>
    <exit alias="west" to="lounge">
      <inherit name="westdirection" />
    </exit>
  </object>
  <object name="kitchen">
    <inherit name="editor_room" />
    <object name="sink">
      <inherit name="editor_object" />
      <look>Just an ordinary sink.</look>
    </object>
    <exit alias="west" to="hall">
      <inherit name="westdirection" />
    </exit>
  </object>
</asl>
```

This example defines three "rooms" – a lounge, a hall and a kitchen. These "rooms" are defined as objects, and they themselves contain the objects "sofa" and "sink". The lounge also contains the "player" object.

By nesting \<object\> elements, you can define further objects inside objects.

## The asl element and its version

Everything in the file goes inside the `<asl>` element. Its `version` says which version of the file format the game was written for, and so which [behaviour fixes](/reference/asl-versions) apply to it. The Quest Viva editor saves version 600. A file without a version, or with a number Quest Viva doesn't know, won't load.

The `<!--Saved by Quest Viva ...-->` comment on the first line is written by the editor so you can tell which version saved the file. Quest Viva ignores it when loading, so you can leave it out.

## What's required, and what's there for the editor

A lot of what the editor saves is only there for the editor. When you write a file by hand, here's what each part does:

-   `<include ref="English.aslx" />` and `<include ref="Core.aslx" />` – required for any normal game. See [Libraries](#libraries).
-   `<game name="...">` – required, exactly once.
-   `gameid`, `version` and `firstpublished` – the editor fills these in for a new game. They don't change how the game plays, but `gameid` identifies the game: keep it the same across updates, and give a different game its own. Any [UUID](https://en.wikipedia.org/wiki/Universally_unique_identifier) will do. See [game](/reference/elements#game) for these and the other details you can set.
-   `<inherit name="editor_room" />` and `<inherit name="editor_object" />` – tell the editor whether an object is a room or an object. A room gets the room tabs (Room, Exits) and a room icon in the tree; an object gets the object tabs (Object, Inventory, Features and so on). An object with neither has "Object type not set" on its Setup tab, and gets both sets of tabs. These types are defined in Core.aslx, so they work in the player too, where all they do is set the `isroom` attribute: `true` for `editor_room`, `false` for `editor_object`. Nothing in the Core library reads `isroom` except the [AllRooms](/reference/functions/scope#allrooms) function. A room that is also an object, such as a vehicle, uses `editor_room_object`. Some older saved games also have `<isroom />` written directly on a room, which means the same thing.
-   `<inherit name="editor_player" />` – gives the player object the Player tab in the editor. It's defined in an editor-only part of Core, and does nothing in the player.
-   An object called `player` – the game starts with the player as this object. If there's no such object, Quest Viva creates one inside the first object in the file. To start as a different object, set the game's `pov` attribute.
-   `<inherit name="eastdirection" />` and so on – the editor adds a direction type to each compass exit. The [map](/howto/rooms/map) uses it to work out where the room on the other side goes, and the editor uses it to put the exit on the Exits tab's compass. The exit works without it, so it only matters if the game shows a map or will be edited.
-   `feature_*` attributes – choose which tabs the editor shows. See [Feature flags](#feature-flags).

So a game written by hand will load and play without any of the `editor_*` types or feature flags. Put them in anyway if you, or someone else, will open the game in the editor later: without them, rooms are shown as plain objects with every tab, and the tabs for the features you used are shown with a warning that the feature is turned off.

## Libraries

There are two libraries included in this example:

-   Core.aslx provides the default Quest Viva functionality, including: showing room descriptions, implementing default verbs such as "take", "drop" and "use", opening and closing containers, and deciding which objects are currently available to the player.
-   [English.aslx](/customise/other-languages) provides the English text for the built-in default responses, and the names of the verbs whose behaviour is defined in Core.aslx. This means Core.aslx is language-neutral – if you wanted to make a game in German or Spanish, just translate English.aslx and plug it in to your game.

The game file doesn't contain a copy of either library: Quest Viva loads them from the version that's playing the game. When the game is published as a `.quest` file, the library code it uses is copied into it.

## Attributes

Each object's attributes are defined in the XML. Attributes define all the behaviour of an object. The XML "type" attribute is used to specify the type of the attribute. If no type is specified, the [string](/reference/attributes/types#string) type is assumed, as with the sink's "look" attribute in the above example. An exception is if there is no data in the XML tag, in which case a [boolean](/reference/attributes/types#boolean) "true" is assumed instead. So these two lines mean the same thing:

```xml
<take />
<take type="boolean">true</take>
```

A boolean that is `false` needs its type, `<take type="boolean">false</take>`, because without it `false` would be read as a string.

Some attributes have an implied type, set up with an [implied](/reference/elements#implied) element in a library. For example, an object's `alt` attribute is a list, so `<alt>telly; television</alt>` is read as a list of two names without needing a type.

The available types are listed on the [Attribute Types](/reference/attributes/types/) page.

The type of an attribute can determine the behaviour of an object. In the above example, the sofa's "take" attribute is a script, so that will run when the player types "take sofa". If the "take" attribute is a string, the object will be taken and the string will be printed. This behaviour is defined in Core.aslx.

Attributes can change type while the game is running, by simply setting them to a new value.

### The attr form

An attribute is usually written as an element with the attribute's name, as in `<look>Just a sofa.</look>`. That only works when the name is a valid XML element name, so there's a second form, where the name goes in a `name` attribute on an `<attr>` element:

```xml
<attr name="look">Just a sofa.</attr>
<attr name="feature_usegive" type="boolean">true</attr>
<attr name="_initialise_" type="script">
  this.worn = true
</attr>
```

Both forms mean exactly the same thing, and you can use either for any attribute. The editor uses the `<attr>` form when the name contains anything other than letters and numbers – so every name with an underscore, like `feature_usegive` or `autodescription_youcango`, and names with spaces or accented letters. It also uses it when the name is the same as one of the element names on the [XML elements](/reference/elements) page, like `object` or `command`, which would otherwise be read as that element. When it writes a `true` boolean this way, it writes the type and value in full, so you'll see both `<take />` and `<attr name="feature_usegive" type="boolean">true</attr>` in a saved file. Games saved by older versions of Quest also use the short form, `<feature_usegive />`, for names with underscores. That still loads.

## Feature flags

Attributes whose names start with `feature_` turn tabs on and off in the editor. Each one is a checkbox: on the game's Features tab for the game-wide ones, and on an object's Features tab for the others. They only affect the editor. What an object actually does in the game comes from its other attributes and types, so leaving the flags out of a file you write by hand changes nothing about how the game plays.

On the game:

| Attribute | Shows |
| --- | --- |
| `feature_limitinventory` | volume and capacity settings on objects, containers and the player |
| `feature_lightdark` | the Light/Dark tab on rooms, the light options on exits, and the Light/Dark checkbox on objects' Features tab |
| `feature_asktell` | the Ask/Tell tab on objects |
| `feature_annotations` | the Notes tab on rooms, for notes that players don't see |
| `feature_advancedwearables` | the advanced options on the Wearable tab |
| `feature_advancedscripts` | the Advanced scripts tab on the game |
| `feature_pictureframe` | the picture frame options, including a picture for each room |

On an object:

| Attribute | Shows |
| --- | --- |
| `feature_usegive` | the Use/Give tab |
| `feature_container` | the Container tab |
| `feature_switchable` | the Switchable tab |
| `feature_edible` | the Edible tab |
| `feature_wearable` | the Wearable tab |
| `feature_lightdark` | the Light/Dark tab (only when it's also turned on for the game) |
| `feature_player` | the Player tab, for an object the player can become. `editor_player` sets this |
| `feature_startscript` | the Initialisation script tab, which sets the `_initialise_` script |

If an object has settings from a feature tab but its flag is off, the editor still shows the tab, with a warning that the settings take effect in the game anyway. See [Features](/howto/objects/features) for what each one is for.

## Additional attributes

When Quest Viva loads the game, it will set the following additional attributes on objects:

-   name – string, from the "name" attribute specified for the \<object\> tag
-   parent – reference to the containing object, or null if the object has no parent

## More on XML

A chunk of XML - called an element - typically consists of a start tag, possibly with attributes, the content, and the end tag. Tags are delineated by angle brackets, with a slash before the name in the end tag:
```xml
<tag name="value">The content</tag>
```
Elements can nest, but they cannot overlap, so this is allowed because the `inner` element is entirely nested inside the `outer` element:
```xml
<outer name="value">The content <inner>Some inner content</inner></outer>
```
This is not, because the two elements overlap:
```xml
<left name="value">The content <right>Some inner content</left></right>
```
If an element has no content, a reduced form can be used:
```xml
<tag name="value"/>
```
Valid XML should include a link at the start to a document type definition, and this will state exactly what elements are allowed where, and with what attributes. Quest Viva has no such link, but it still has a set of rules.

### XML and Quest Viva

The outermost element of a Quest Viva document is the `asl` element; everything goes inside there. Inside that are the various parts of a Quest Viva game: include (references to libraries), game, verb, command, object, function, turnscript, walkthrough. Every game has one game object, but can have any number of the other objects. All the attributes (in Quest Viva terms) are elements inside those elements, except the `name` attribute, which is a XML attribute.

Looking again at the blank game, you can see the `game` object has a name attribute as an XML attribute, but `gameid`, `version` and `firstpublished` are all XML elements.
```xml
<game name="blank">
  <gameid>35ccfb71-ef3a-4edc-aba6-7c556231626b</gameid>
  <version>1.0</version>
  <firstpublished>2016</firstpublished>
</game>
```
By default elements that hold Quest Viva attributes are strings, but the type attribute can state otherwise. Here is some XML that defines an integer attribute called "temp" and a string dictionary called "statusattributes", and gives the latter a single name-value pair.
```xml
<temp type="int">0</temp>
<statusattributes type="stringdictionary">
  <item>
    <key>temp</key>
    <value>Temperature: !°C</value>
  </item>
</statusattributes>
```


## What about HTML?

HTML is the mark-up language used on web pages to control how a browser will display the page.

Like XML, HTML is derived from SGML, a markup language developed 30 years ago, and uses the same scheme of tags. There are differences, but recent versions of HTML have become more XML like, and the differences are not worth discussing here. If you follow the rules for XML when writing HTML you will be a step ahead of the game.


### Simple formatting

HTML has some tags to display text in bold, underline or italic:
```xml
HTML has some tags to display text in <b>bold</b>, <u>underline</u> or <i>italic</i>.
Also <strike>strike-through</strike>.
And <b><i>combinations</i></b> too, but remember they have to nest!
```

### Line breaks

HTML ignores line breaks and collapses all white space (spaces, tabs and returns) into a single space. If you want to have a line break, use the `<br/>` element. As with XML, the slash indicates this is an empty element (no content, no end tag). In fact HTML is not as strict as XML, and `<br>` will work too.

That said, where possible it is better to break paragraphs into separate `msg` statements in your code, and let Quest Viva add the line breaks for you.

### More style options

For more involved styling, you are better using CSS. This can be associated with a section of HTML using `span` and `div` elements. Use `span` for a section within a single line, and use `div` for a section that includes several sections.

Whichever you use, give it a `style` attribute, and use CSS as the value. Here is an example that sets both the foreground and background colour. Note that CSS attributes take a colon between the name and the value, and each pair is separated by a semi-colon (and the US spelling of "color").
```xml
How to do <span style="color:white;background-color:black">reverse video</span>.
```
CSS offers a huge range of options, see [here](https://www.w3schools.com/cssref/) for example. It can be quite fussy in the values allowed.

### CDATA

HTML is not compatible with XML. If you have HTML in your strings or scripts, Quest Viva will get confused when opening your file, will try to interpret the HTML as XML, and throw an error. The solution is to put the HTML (and any text with a `<` in it) inside a CDATA section. A CDATA section is just something tagged as not XML.

Generally Quest Viva does this for you. If you are coding directly in the XML, perhaps in a library, you need to start and end the text with `<![CDATA[` and `]]>` respectively. For example:
```xml
<take type="script"><![CDATA[
  msg("You can't take <i>that</i>!")
]]></take>
```