---
layout: index
title: ASLX Elements
---

Element Types
-------------

The following elements may appear in an ASLX file:

-   [asl](elements/asl_element.html) or [library](elements/library_element.html) as the top level element

Underneath this, the following may appear:

-   [include](elements/include_element.html)
-   [template](elements/template_element.html)
-   [dynamictemplate](elements/dynamictemplate_element.html)
-   [verbtemplate](elements/verbtemplate_element.html)
-   [function](elements/function_element.html)
-   [command](elements/command_element.html)
-   [verb](elements/verb_element.html)
-   [type](elements/type_element.html)
-   [game](elements/game_element.html)
-   [object](elements/object_element.html)
-   [exit](elements/exit_element.html)
-   [walkthrough](elements/walkthrough_element.html)
-   [timer](elements/timer_element.html)
-   [turnscript](elements/turnscript_element.html)
-   [implied](elements/implied_element.html)
-   [delegate](elements/delegate_element.html)
-   [javascript](elements/javascript_element.html)
-   [editor](elements/editor_element.html)
-   [tab](elements/tab_element.html)
-   [control](elements/control_element.html)
-   [resource](elements/resource_element.html)

Within a [type](elements/type_element.html), [object](elements/object_element.html), [exit](elements/exit_element.html) or [command](elements/command_element.html) tag:

-   [inherit](elements/inherit_element.html)
-   [command](elements/command_element.html)
-   [verb](elements/verb_element.html)
-   in an object element only, nested [object](elements/object_element.html) or [exit](elements/exit_element.html) elements may appear. Their "parent" attribute will be set to the parent object
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

