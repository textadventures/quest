---
layout: index
title: Translating Quest
---

Introduction
------------

To translate Quest, make a copy of English.aslx and rename it for your language.

It is recommended that you include this line in your translated file:

     <include ref="English.aslx"/>

This will include the English template within yours. This means that if English.aslx is updated, your translation won't cause errors due to missing template entries.

Translating default text
------------------------

Default text appears within [template](../elements/template.html) and [dynamictemplate](../elements/dynamictemplate.html) tags.

You can translate "template" tags directly, as they are simply static text.

Dynamic templates are expressions - usually these are templates that include some object attribute, for example:

     <dynamictemplate name="TakeSuccessful">"You pick " + object.article + " up."</dynamictemplate>

Your translation should also be an expression, but you're not forced to use the same attributes. If it makes more sense for your language, for example, you could use the [gender](../attributes/gender.html) instead of the [article](../attributes/article.html) to create your sentence.

Some functions that appear within dynamic templates are defined in English.aslx - for example the [GetDefaultPrefix](../functions/corelibrary/getdefaultprefix.html) and [Conjugate](../functions/corelibrary/conjugate.html) functions. You're free to add, edit, or remove these functions in your template - of course, as long as you define any functions that you use in your dynamic templates.

If you want to know where a template is used, search through the Core library files. [TextPad](http://www.textpad.com) for example has a good "Find in Files" feature.

Translating commands
--------------------

Some commands are defined using a [verbtemplate](../elements/verbtemplate.html) - you can have as many of these as you wish for any particular command, so feel free to add more if there are more alternatives. Likewise, feel free to remove any additional ones you don't need - there are 4 "speakto" verbtemplates in English.aslx, but it's fine for there to be fewer in your translation.

Some commands are defined like this:

     <template templatetype="command" name="put"><![CDATA[^put (?<object1>.*) (on|in) (?<object2>.*)$]]></template>

That may look a bit off-putting at first glance, but it's fairly simple. Let's break it down:

-   you don't need to worry about the templatetype - just leave it in. This is used so the Editor knows this is a template for a command, and that it doesn't need to display this template separately.
-   the CDATA is simply XML formatting. Within the template, we're using "\<" and "\>" - so for our XML to be valid, we *must* enclose the template within a [CDATA](http://en.wikipedia.org/wiki/CDATA) section - starting with "\<![CDATA[" and ending with "]]\>"

So the only bit we need to worry about is inside the CDATA, which is this:

     ^put (?<object1>.*) (on|in) (?<object2>.*)$

This is a [regular expression](http://en.wikipedia.org/wiki/Regular_expression) ("regex") and is simply a more advanced form of command pattern. This [cheat sheet](http://regexlib.com/CheatSheet.aspx) is a handy syntax reference.

The "\^" at the beginning and the "\$" at the end simply mean that this regex must match the *entire* player input. We don't want to match only a small fragment of what the player typed in - we want to understand the entire command. So leave those in.

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

English doesn't have the concept of "gender" for inanimate objects, but most other languages do. To handle this, you can define "masculine" and "feminine" [types](../elements/type.html) in your language file.

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

Releasing your translation
--------------------------

When you have finished your translation, if you'd like it to be included with Quest so that other game authors can use it, please email it to <alex@axeuk.com> for it to be included in the next release.

Keeping the translation up to date
----------------------------------

Quest is continually improving, and as new features are added, new templates are added to English.aslx. This means that your language library will need to be updated to reflect new changes.

If you include English.aslx in your language file, as recommended, you won't see errors, but it does mean that there is a chance players will see some English text. To avoid this, you will need to keep your language file up to date to reflect changes made in English.aslx.

The easiest way to do this is to see what changes have been made to English.aslx by [browsing the source code](https://github.com/textadventures/quest). Navigate to WorldModel/WorldModel/Core/Languages/English.aslx, then you can use the "Compare to other versions" list in the top right. If you select "Older versions" from the list, you'll be able to see the changes made since a particular date.
