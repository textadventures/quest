---
layout: index
title: ProcessScopeCommand
---

    ProcessScopeCommand (string function name, objectlist scope, string text)

**New in Quest 5.7**    

The purpose of this function is to handle commands that reference objects outside the normal scope. It will attempt to match the `text` to the objects in the `scope` list, using the usual Quest procedure (so will check against partial matches and alternatives). If it finds a match, then it will pass the matched object to the named function, and will return a Boolean, the value returned by the function. If there is no match, then `ProcessScopeCommand` will return false.

-  The named function must return a Boolean - true if it handled the command, false otherwise, and it must have a single parameter, the object.

-  The scope must be a list of objects that the command can potentially be applied to. This could be spells in a spell book, or items for sale in a shop. In both cases these would be in a room separate to the player.

-  The text is what the player typed to refer to the object.