---
layout: index
title: ASLX Elements
---

Element Types
-------------

The following elements may appear in an ASLX file:

-   [asl](elements/asl.html) or [library](elements/library.html) as the top level element

Underneath this, the following may appear:

-   [include](elements/include.html)
-   [template](elements/template.html)
-   [dynamictemplate](elements/dynamictemplate.html)
-   [verbtemplate](elements/verbtemplate.html)
-   [function](elements/function.html)
-   [command](elements/command.html)
-   [verb](elements/verb.html)
-   [type](elements/type.html)
-   [game](elements/game.html)
-   [object](elements/object.html)
-   [exit](elements/exit.html)
-   [walkthrough](elements/walkthrough.html)
-   [timer](elements/timer.html)
-   [turnscript](elements/turnscript.html)
-   [implied](elements/implied.html)
-   [delegate](elements/delegate.html)
-   [javascript](elements/javascript.html)
-   [editor](elements/editor.html)
-   [tab](elements/tab.html)
-   [control](elements/control.html)
-   [resource](elements/resource.html)

Within a [type](elements/type.html), [object](elements/object.html), [exit](elements/exit.html) or [command](elements/command.html) tag:

-   [inherit](elements/inherit.html)
-   [command](elements/command.html)
-   [verb](elements/verb.html)
-   in an object element only, nested [object](elements/object.html) or [exit](elements/exit.html) elements may appear. Their "parent" attribute will be set to the parent object
-   any other XML element will set an attribute of that name on the parent object/type/exit/command.

XML file format
---------------

Quest uses an XML-based file format, and files have an .aslx extension.

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

The available types are listed on the [Attribute Types](attribute_types.html) page.

The type of an attribute can determine the behaviour of an object. In the above example, the sofa's "take" attribute is a script, so that will run when the player types "take sofa". If the "take" attribute is a string, the object will be taken and the string will be printed. This behaviour is defined in Core.aslx.

Attributes can change type while the game is running, by simply setting them to a new value.

Additional attributes
---------------------

When Quest loads the game, it will set the following additional attributes on objects:

-   name – string, from the "name" attribute specified for the \<object\> tag
-   parent – reference to the containing object, or null if the object has no parent

