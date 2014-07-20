---
layout: index
title: verb element
---

    <verboptional name="name"optional pattern="pattern"optional unresolved="unresolved text"optional property="attribute name"optional response="default response text"optional template="template name">script</verb>

or

    <verboptional name="name">attributes</verb>

Creates a verb, which is a specialised type of [command element](command_element.html) - so everything that applies to a command also applies to a verb. Underneath, verbs are just commands - if you look at them in the Debugger, they are the same thing. But they are designed to be easier to use than commands for the vast majority of commands which are of the form "command object", such as "look at thing", "eat food", "sit on bench" etc.

In addition to any "defaultcommand" type, verbs also inherit "defaultverb". In Core.aslx this provides the standard verb implementation. We take the object the player entered, and look for the attribute as specified by "property". Then:

-   if the attribute is a script, run it;
-   if the attribute is a string, print it;
-   if the attribute is not set, print the default verb response (e.g. "You can't eat it")
-   if the attribute is some other type, raise an error.

