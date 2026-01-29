---
layout: index
title: Translating Quest
---

Introduction
------------

Quest has been designed to be language neutral, so you can write games that can be played in any language.

There are numerous translations that are built in, donated by the community over the years. This does mean that some of them are out of date, and do not include the more recent additions. 

You can even use the editor in your own language, but that will obviously involve more translations. As of March 2018, the editor can only be used in German and, to a degree, Spanish.


Using Non-Latin Alphabets
-------------------------

Quests scripts cannot cope with letters outside the standard Latin alphabet.

Let us suppose you have an object, a rock, in a game you are writing in Greek. Quest will not object to you calling it πέτρα, and the player will be able to interact with it as normal. However, if you try to do anything in a script using that name, you will get an error:

```
πέτρα.parent = player.parent
```

The solution is to name in in the Latin alphabet, and give it an alias "πέτρα".


Verbs
-----

In English, we can just put a verb with a noun to get a command, and the verbs in Quest employ this to great effect. That may not work in your language. If you are using the desktop version, you can edit the verb object. In the pattern bit, you can set a pattern just as you do with commands, so instead of "wear", you could use "put #object# on", and the verb will match PUT HAT ON.



Making a Translation
--------------------

To translate Quest, make a copy of English.aslx and rename it for your language. Open the file in a text editor, we recommend [NotePad++](https://notepad-plus-plus.org/).

At the top you will see this:

```
<library>
  <include ref="EditorEnglish.aslx"/>
  <template name="LanguageId">en</template>
```

The first step is to modify that so it uses "English.aslx", rather than "EditorEnglish.aslx", and change the language ID. This example is for Icelandic:

```
<library>
  <include ref="English.aslx"/>
  <template name="LanguageId">is</template>
```

This will include the English template within yours. This means that if English.aslx is updated, your translation won't cause errors due to missing template entries.


Translating default text
------------------------

Default text appears within [template](elements/template.html) and [dynamictemplate](elements/dynamictemplate.html) tags.

You can translate "template" tags directly, as they are simply static text. Note that the name must not be changed, just the bit between the tags. Here are some examples from the Russian.aslx:

```
  <template name="LookAt">Посмотреть на</template>
  <template name="Take">Взять</template>
  <template name="SpeakTo">Поговорить с</template>
  <template name="Use">Использовать</template>
```

Dynamic templates are expressions - usually these are templates that include some object attribute, for example:

```
  <dynamictemplate name="TakeSuccessful">"You pick " + object.article + " up."</dynamictemplate>
```

Your translation should also be an expression, but you're not forced to use the same attributes. If it makes more sense for your language, for example, you could use the [gender](attributes/gender.html) instead of the [article](attributes/article.html) to create your sentence. Again, you just change the bit between the tags, as these examples from Russian show.

```
  <dynamictemplate name="DropSuccessful">"Ты оставляешь " + object.article + " здесь."</dynamictemplate>
  <dynamictemplate name="DropUnsuccessful">"Ты не можешь " + object.article + "оставить."</dynamictemplate>
```

Some functions that appear within dynamic templates are defined in English.aslx - for example the [GetDefaultPrefix](functions/corelibrary/getdefaultprefix.html) and [Conjugate](functions/corelibrary/conjugate.html) functions. You can add, edit, or remove these functions in your template as required.

If you want to know where a template is used, search through the Core library files. Notepad++ has a good "Find in Files" feature that lets you search across all files in a folder.


Translating commands
--------------------

Some commands are defined using a [verbtemplate](elements/verbtemplate.html) - you can have as many of these as you wish for any particular command, so feel free to add more if there are more alternatives. Likewise, feel free to remove any additional ones you don't need - there are 4 "speakto" verbtemplates in English.aslx, but it's fine for there to be fewer in your translation.

Some commands are defined like this:

     <template templatetype="command" name="put"><![CDATA[^put (?<object1>.*) (on|in) (?<object2>.*)$]]></template>

That may look a bit off-putting at first glance, but it's fairly simple. Let's break it down:

-   you don't need to worry about the templatetype - just leave it in. This is used so the Editor knows this is a template for a command, and that it doesn't need to display this template separately.
-   the CDATA is simply XML formatting. Within the template, we're using "\<" and "\>" - so for our XML to be valid, we *must* enclose the template within a [CDATA](http://en.wikipedia.org/wiki/CDATA) section - starting with "\<![CDATA[" and ending with "]]\>"

So the only bit we need to worry about is inside the CDATA, which is this:

     ^put (?<object1>.*) (on|in) (?<object2>.*)$

This is a [regular expression](http://en.wikipedia.org/wiki/Regular_expression) ("regex") and is simply a more advanced form of command pattern, and is discussed in some detail [here](pattern_matching.html). This [cheat sheet](http://regexlib.com/CheatSheet.aspx) is a handy syntax reference.

The "^" at the beginning and the "$" at the end simply mean that this regex must match the *entire* player input. We don't want to match only a small fragment of what the player typed in - we want to understand the entire command. So leave those in.

That means you only need to worry about the bit in the middle:

     put (?<object1>.*) (on|in) (?<object2>.*)

The brackets are there for grouping. There are three groups in the regex above. The first one is named using the ?\<name\> syntax as "object1". It matches ".\*" which is the regex way of saying "any number of any character". The second group matches "on" or "in". The third group is named "object2".

If we didn't have the "(on|in)" in there, this would be equivalent to this command pattern:

     put #object1# in #object2#

To translate it, you only need to worry about the "put" and "on|in" parts.

For example, in Deutsch.aslx the translation of this is:

     <template templatetype="command" name="put"><![CDATA[^lege (1?<object>.*) (auf|in) (?<object2>.*)$]]></template>

	 
	 
Language-specific object types
------------------------------

English doesn't have the concept of "gender" for inanimate objects, but most other languages do. To handle this, you can define "masculine" and "feminine" [types](elements/type.html) in your language file.

For example, in French:

     <type name="masculine">
       <gender>il</gender>
       <article>le</article>
     </type>
     
     <type name="feminine">
       <gender>elle</gender>
       <article>la</article>
     </type>

Then the LanguageSpecificObjectTypes template should look like this:

     <template name="LanguageSpecificObjectTypes">masculine=Inanimate object (masculine); feminine=Inanimate object (feminine); </template>

The type names in the template must match the type names you define. If they don't, you'll see errors in the Editor.

This will add two entries in the object "Type" dropdown, allowing the game author to choose masculine or feminine inanimate object types.

If your language has more than two genders, you can add more types and add them to the same LanguageSpecificObjectTypes template.



Adding the translation to your game
-----------------------------------

These are standard library files so can be added as such.

To add a library, go the bottom of the left pane in the GUI, and expand Advanced, then click on Included Libraries. Click Add, and navigate to the file. Quest will copy the file to your game folder, and add a line of code to your game so the library is part of it. Quest will then tell you to save and re-load your game.

More on using libraries [here](using_libraries.html).


Display Verbs
-------------

Quest uses a very simple method for handling display verbs (the verbs that are shown when you click on an object): The verb followed by the object name are sent to be handled as a command. In English this works fine; "wear" plus "hat" gives "wear hat", and Quest will understand that. That may not be the case in your language. If not, them just add that as an alternative.

For example, in German, the verb to display to allow the player to wear something is "Anziehen", but the full phrase for "wear hat" would be "ziehe hut an". The solution is to add "anziehen #object#" as an alternative, even though it is bad German.


Releasing your translation
--------------------------

When you have finished your translation - and checked it works in your game - if you'd like it to be included with Quest so that other game authors can use it, please submit a new issue to the list at Github, and attach your translation. We would also be grateful for updates to existing translations.

[Github Issue Tracker](https://github.com/textadventures/quest/issues)

Note that Quest is only updated once or twice a year, so your changes may not become official straightaway.

Keeping the translation up to date
----------------------------------

Quest is continually improving, and as new features are added, new templates are added to English.aslx. This means that your language library will need to be updated to reflect new changes.

If you include English.aslx in your language file, as recommended, you won't see errors, but it does mean that there is a chance players will see some English text. To avoid this, you will need to keep your language file up to date to reflect changes made in English.aslx.

The easiest way to do this is to see what changes have been made to English.aslx by [browsing the source code](https://github.com/textadventures/quest). Navigate to WorldModel/WorldModel/Core/Languages/English.aslx, then you can use the "Compare to other versions" list in the top right. If you select "Older versions" from the list, you'll be able to see the changes made since a particular date.
