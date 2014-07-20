---
layout: index
title: Pattern Matching
---

When the player types something into Quest, Quest will attempt to match that against the commands in the game. Quest has a long list of commands, in the order that they appear in the code. As libraries are declared right at the top, library commands will be at the top of the list. Quest starts at the *bottom* of the list, so your own commands will override those in the libraries, as long as there is a match.

So how are matches mades?

There are two types of matches, the simple pattern and the regex.

Simple Pattern
--------------

The simple pattern matches exact text and simple substitutions. Here are three examples.

      <command name="log">
        <pattern type="simplepattern">log #text#</pattern>
        ...
      </command>

      <command name="examine">
        <pattern type="simplepattern">examine #object#;x #object#</pattern>
        ...
      </command>

      <command name="give">
        <pattern type="simplepattern">give #object1# to #object2#</pattern>
        ...
      </command>

The first will match any input that starts "log " and if it gets a match the rest of the input goes into a local variable called text. If the player types "log found a cup", then we get a match and the command might add "found a cup" to the game log.

The second form uses the \#object\# substitution, and Quest will try to match this against an object that is present. If there is no such object in scope then the match will fail. If there is a match then the local variable object will contain the matched object.

Notice that two matches are supplied, separated by a semi-colon. The player can use either "examine" or "x" to invoke this command.

In the third example, two objects are required. Both must be present to make a match, and if they are, they can be referenced as object1 and object2.

Simple patterns get automatically converted to regex expressions by Quest.

Regex
-----

A regex is rather more complicated, but a lot more powerful. A regex (also called a regexp, or regular expression) is a sort of string that can be used to match against another string. You could think of it as a template or a set of rules that a string can be compared to.

Quest uses .NET regex rules, and and a quick reference for .NET regex rules can be found here: <http://msdn.microsoft.com/en-us/library/az24scfc.aspx>

Here is the "put" command, using a regex.

      <command name="put">
        <pattern><![CDATA[^put (?<object1>.*) (on|in) (?<object2>.*)$]]></pattern>
        ...
      </command>

First, note that regex matching is the default; compare to simple pattern matching, where you have to specify it with type="simplepattern".

The second point is that the pattern is CDATA. This tells Quest that any less than symbols ("\<") are part of the pattern, and not the start of an XML tag. The CDATA part starts <![CDATA[ and ends ]]>. If you are doing this through the GUI, Quest will do that for you.

This, then, is the pattern:

      ^put (?<object1>.*) (on|in) (?<object2>.*)$

-   \^ and \$ have to match the start and end of the text respectively.
-   The curved brackets group a part of the text.
-   The (?\< ... \> ... ) indicates a capture group, that is, a set of characters that we want to use. The part inside the angle brackets, object1, is the name of the variable we can use to access the captured characters, the bit between the \> and the ) is a pattern to match.
-   The . indicates any character (except newline).
-   The \* indicates that the previous thing can be matched any number of times.
-   The vertical bar indicates either/or, so given the brackets, this will match both "on" and "in".

Quest will attempt to match object1 and object2 to objects that are present.

A Note About Language Support
-----------------------------

The core quest commands were built with language support. The command is given a pattern inside square brackets, and this tells Quest to use a template. All the templates go into a language specific file. This is the actual give-to command:

      <command name="give" pattern="[give]">
        HandleGiveTo (object1, object2)
      </command>

This is the template for English.

      <template templatetype="command" name="give"><![CDATA[^give (?<object1>.*) to (?<object2>.*)$]]></template>

A Note About Verb Templates
---------------------------

Verb templates seem to offer another simple method for matching. Instead of defining a pattern, you specify a template (might have been better called a verbtemplate?), and in the language file you specify your verb templates.

I would guess that Quest attempts to match against each verbtemplate with the objects present, and if it finds a match, it links that back to the command. You can have several verbtempates for one command. Here is an example from the Quest library.

      <command name="lookat" template="lookat">
        ...
      </command>

      <verbtemplate name="lookat">look at</verbtemplate>
      <verbtemplate name="lookat">x</verbtemplate>
      <verbtemplate name="lookat">examine</verbtemplate>
      <verbtemplate name="lookat">exam</verbtemplate>
      <verbtemplate name="lookat">ex</verbtemplate>
