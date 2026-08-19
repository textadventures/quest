---
title: Hyperlinks
---

Quest lets you embed clickable links directly in text, using the same `{...}` syntax as the rest of the [text processor](/howto/world/text_processor). There are three different kinds, and if you set up a room description to run a script, and have it print the following as a message, you can see all three:

```xml
Here is some text with an <a href="https://textadventures.co.uk/forum/quest">anchor link</a>,
a link for {object:torch:the torch}
and a link for a {command:wait:wait a moment}.
```

When the room description is displayed, there are three links. Clicking on the first will open up that web page in your browser, and is exactly the same as a link on a web page (so, yes, the HTML tag that lets you move to another web page is `a` for anchor, a nautical device for keeping you from moving somewhere else).

Click on the second (the `{object:...}` link), and Quest gives a choice of verbs for the named object - whichever verbs apply to it (Look at, Take, and so on) - and when one is selected, it is sent as a command.

Click on the third (the `{command:...}` link), and Quest sends the given command straight to the parser - there is no choice of verbs, it always sends the same thing.

In fact, you can put any command together like this; say there is a command that recognises "jump up and down", this will give a link to that: Here is some text with a link for {command:jump up and down:jump about}.

## Related functions

*GetDisplayNameLink (object, type)* Gets the name/alias of the object. If type is not the empty string and game.enablehyperlinks then this is wrapped up as a link, using the `{object:...}` syntax above, unless type is "exit" (and the object has exactly one verb), in which case the `{exit:...}` syntax is used. Prefixes and suffixes are also added as required, outside the link.

*ObjectLink (object)* Returns `{object:objectname}` - the link text for the given object, as used above.

*CommandLink (cmd, text)* Creates the text for a command link, using the `{command:...}` syntax above.

*DisplayMailtoLink (text, email)* Creates an email link.

*DisplayHttpLink (text, url, https)* Creates a link to another web page.
