---
layout: index
title: ASLX File Format
---

Quest uses an XML-based file format, and files have an .aslx extension. If you go to _Tools - Code view_ you can see the XML code that is your game. 

Generally you have no need to look at the full code view, but just occasionally it is useful, for example if you are creating or editing a library, or when spell checking, and some idea of XML is useful.

Here is a simple example:

     <asl version="500">
       <include ref="English.aslx"/>
       <include ref="Core.aslx"/>
     
       <game name="Test ASLX Game"/>
     
       <object name="lounge">
     
         <object name="player">
           <inherit name="defaultplayer" />
         </object>
     
         <object name="sofa">
           <prefix>a</prefix>
           <look>Just a sofa.</look>
           <take type="script">
              msg ("Example script attribute")
           </take>
         </object>
     
         <exit name="east" to="hall"/>
       </object>
     
       <object name="hall">
         <exit name="east" to="kitchen"/>
         <exit name="west" to="lounge"/>
       </object>
     
       <object name="kitchen">
         <object name="sink">
           <look>Just an ordinary sink</look>
         </object>
     
         <exit name="west" to="hall"/>
       </object>
     
     </asl>

This example defines three "rooms" – a lounge, a hall and a kitchen. These "rooms" are defined as objects, and they themselves contain the objects "sofa" and "sink". The lounge also contains the "player" object.

By nesting \<object\> elements, you can define further objects inside objects.

Libraries
---------

There are two libraries included in this example:

-   Core.aslx provides the default Quest functionality, including: showing room descriptions, implementing default verbs such as "take", "drop" and "use", opening and closing containers, and deciding which objects are currently available to the player.
-   [English.aslx](guides/translating_quest.html) provides the English text for the built-in default responses, and the names of the verbs whose behaviour is defined in Core.aslx. This means Core.aslx is language-neutral – if you wanted to make a game in German or Spanish, just translate English.aslx and plug it in to your game.

Attributes
----------

Each object's attributes are defined in the XML. Attributes define all the behaviour of an object. The XML "type" attribute used is to specify the type of the attribute. If no type is specified, the [string](types/string.html) type is assumed, as with the sink's "look" attribute in the above example. An exception is if there is no data in the XML tag, in which case a [boolean](types/boolean.html) "true" is assumed instead.

The available types are listed on the [Attribute Types](types/) page.

The type of an attribute can determine the behaviour of an object. In the above example, the sofa's "take" attribute is a script, so that will run when the player types "take sofa". If the "take" attribute is a string, the object will be taken and the string will be printed. This behaviour is defined in Core.aslx.

Attributes can change type while the game is running, by simply setting them to a new value.

Additional attributes
---------------------

When Quest loads the game, it will set the following additional attributes on objects:

-   name – string, from the "name" attribute specified for the \<object\> tag
-   parent – reference to the containing object, or null if the object has no parent




More on XML
-----------

A chunk of XML - called an element - typically consists of a start tag, possible with attributes, the content, and the end tag. Tags are delineated by angle brackets, with a slash before the name in the end tag:
```
<tag name="value">The content</tag>
```
Elements can nest, but they cannot overlap, so this is allowed because the `inner` element is entirely nested inside the `outer` element:
```
<outer name="value">The content <inner>Some inner content</inner></outer>
```
This is not, because the two elements overlap:
```
<left name="value">The content <right>Some inner content</left></right>
```
If an element has no content, a reduced form can be used:
```
<tag name="value"/>
```
Valid XML should include a link at the start to a document type definition, and this will state exactly what elements are allowed where, and with what attributes. Quest has no such link, but it still has a set of rules.

### XML and Quest

The outer most element of a Quest document is the `asl` element; everything goes inside there. Inside that are the various parts of a Quest game: include (references to libraries), game, verb, command, object, function, turnscript, walkthrough. Every game has one game object, but can have any number of the other objects. All the attributes (in Quest terms) are elements inside those elements, except the `name` attribute, which is a XML attribute.

Looking again at the blank game, you can see the `game` object has a name attribute as an XML attribute, but `gameid`, `version` and `firstpublished` are all XML elements.
```
  <game name="blank">
    <gameid>35ccfb71-ef3a-4edc-aba6-7c556231626b</gameid>
    <version>1.0</version>
    <firstpublished>2016</firstpublished>
  </game>
```
By default elements that hold Quest attributes are strings, but the type attribute can state otherwise. Here is some XML that defines an integer attribute called "temp" and a string dictionary called "statusattributes", and gives the latter a single name-value pair.
```
        <temp type="int">0</temp>
        <statusattributes type="stringdictionary">
          <item>
            <key>temp</key>
            <value>Temperure: !°C</value>
          </item>
```


What About HTML?
----------------

HTML is the mark-up language used on web pages to control how a browser will display the page.

Like XML, HTML is derived from SGML, a markup language developed 30 years ago, and uses the same scheme of tags. There are differences, but recent versions of HTML have become more XML like, and the differences are not worth discussing here. If you follow the rules for XML when writing HTML you will a step ahead of the game.


### Simple Formatting

HTML has some tags to display text in bold, underline or italic:
```
HTML has some tags to display text in <b>bold</b>, <u>underline</u> or <i>italic</i>.
Also <strike>strike-through</strike>.
And <b><i>combinations</i></b> too, but remember they have to nest!
```

### Line breaks

HTML ignores line breaks and collapses all white space (spaces, tabs and returns) into a single space. If you want to have a line break, use the `<br/>` element. As with XML, the slash indicates this is an empty element (no content, no end tag). In fact HTML is not as strict as XML, and `<br>` will work too.

That said, where possible I would recommend breaking paragraphs into separate `msg` statement in your code, and let Quest add the line breaks for you.

### More style options

For more involved styling, you are better using CSS. This can be associatedwith a section of HTML using `span` and `div` elements. Use `span` for a section within a single line, and use `div` for a section that includes several sections.

Whichever you use, give it a `style` attribute, and use CSS as the value. Here is an example that sets both the foreground and background colour. Note that CSS attributes take a colon between the name and the value, and each pair is separated by a semi-colon (and the US spelling of "color").
```
How to do <span style="color:white;background-color:black">reverse video</span>.
```
CSS offers a huge range of options, see [here](http://www.w3schools.com/cssref/) for example. It can be quite fussy in the values allowed.

### CDATA

HTML is not compatible with XML. If you have HTML in your strings or scripts, Quest will get confused when opening your file, will try to interpret the HTML as XML, and throw an error. The solution is to put the HTML (and any test with a `<` in it) inside a CDATA section. A CDATA section is just something tagged as not XML.

Generally Quest does this for you. If you are coding directly in the XML, perhaps in a library, you need to start and end the text with `<![CDATA[` and `]]>` respectively. For example:
```
  <take type="script"><![CDATA[
    msg("You can't take <i>that</i>!")
  ]]></take>
```