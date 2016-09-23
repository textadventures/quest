---
layout: index
title: Hyperlinks
---

Quest uses a kind of HTML (the language of web pages) to display text, and displaying a link is done through that. In fact, there are three different links, and if you set up a room that will print this (set the description to run a script, and have the script print the following as a message) you can see them:

      Here is some text with an <a href="http://textadventures.co.uk/forum/quest">anchor link</a>,
      a link for <object verbs="Look at/Examine">me</object>
      and a link for a <command input="wait">command</command>.

When the room description is displayed, there are three links. Clicking on the first will open up that web page in your browser, and is exactly the same as a link on a web page (so, yes, the HTML tag that lets you move to another web page is a for anchor, a nautical device for keeping you from moving somewhere else).

Click on the third, and Quest gives a choice of verbs, and when one is selected, it is sent as a command. Specifically, the command sent is: <verbselected> <textinlink>

In fact, you can put any command together like this; say there is a command that recognises "jump up and down", this string will give a link to that: Here is some text with a link for <object verbs="Jump up">and down</object>.

However, a better way is using the third type of link. The command link just sends the input attribute to the parser.

*Note:* Quest 5.3 is expected to add extra functionality to hyperlinks.

Related functions
-----------------

*GetTaggedName (object, type, verbs)* Gets the name/alias of the object. If type is not the empty string and game.enablehyperlinks then this is wrapped up as a link, using object tags, and the verbs given. This is an internal command, and you are not expected to use it.

*GetDisplayNameLink (object, type, verbs)* Gets the name/alias of the object. If type is not the empty string and game.enablehyperlinks then this is wrapped up as a link, using object tags, and the verbs given, unless the type is "exit", in which case command tags are used. Prefixes and suffixes are also added as required outside the tags.

*ObjectLink (object)* Uses GetTaggedName for the given object.

*CommandLink (cmd, text)* Creates the text for a command link

*DisplayMailtoLink (text, email)* Creates an email link.

*DisplayHttpLink (text, url, https)* Creates a link to another web page.
