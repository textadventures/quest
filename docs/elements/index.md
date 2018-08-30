---
layout: index
title: XML Elements
---

Note that this is about XML elements in the ASLX file, which is not quite the same as the elements in the game.

The following elements may appear in an ASLX file:

-   [asl](asl.html) or [library](library.html) as the top level element

Underneath this, the following may appear:

-   [include](include.html)
-   [template](template.html)
-   [dynamictemplate](dynamictemplate.html)
-   [verbtemplate](verbtemplate.html)
-   [function](function.html)
-   [command](command.html)
-   [verb](verb.html)
-   [type](type.html)
-   [game](game.html)
-   [object](object.html)
-   [exit](exit.html)
-   [walkthrough](walkthrough.html)
-   [timer](timer.html)
-   [turnscript](turnscript.html)
-   [implied](implied.html)
-   [delegate](delegate.html)
-   [javascript](javascript.html)
-   [editor](editor.html)
-   [tab](tab.html)
-   [control](control.html)
-   [resource](resource.html)

Within a [type](type.html), [object](object.html), [exit](exit.html) or [command](command.html) tag:

-   [inherit](inherit.html)
-   [command](command.html)
-   [verb](verb.html)
-   in an object element only, nested [object](object.html) or [exit](exit.html) elements may appear. Their "parent" attribute will be set to the parent object
-   any other XML element will set an attribute of that name on the parent object/type/exit/command.