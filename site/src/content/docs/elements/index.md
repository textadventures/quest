---
title: XML Elements
sidebar:
  order: 25
---

Note that this is about XML elements in the ASLX file, which is not quite the same as the elements in the game.

The following elements may appear in an ASLX file:

-   [asl](/elements/asl) or [library](/elements/library) as the top level element

Underneath this, the following may appear:

-   [include](/elements/include)
-   [template](/elements/template)
-   [dynamictemplate](/elements/dynamictemplate)
-   [verbtemplate](/elements/verbtemplate)
-   [function](/elements/function)
-   [command](/elements/command)
-   [verb](/elements/verb)
-   [type](/elements/type)
-   [game](/elements/game)
-   [object](/elements/object)
-   [exit](/elements/exit)
-   [walkthrough](/elements/walkthrough)
-   [timer](/elements/timer)
-   [turnscript](/elements/turnscript)
-   [implied](/elements/implied)
-   [delegate](/elements/delegate)
-   [javascript](/elements/javascript)
-   [editor](/elements/editor)
-   [tab](/elements/tab)
-   [control](/elements/control)
-   [resource](/elements/resource)

Within a [type](/elements/type), [object](/elements/object), [exit](/elements/exit) or [command](/elements/command) tag:

-   [inherit](/elements/inherit)
-   [command](/elements/command)
-   [verb](/elements/verb)
-   in an object element only, nested [object](/elements/object) or [exit](/elements/exit) elements may appear. Their "parent" attribute will be set to the parent object
-   any other XML element will set an attribute of that name on the parent object/type/exit/command.