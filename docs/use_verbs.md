---
layout: index
title: Use verbs
---

The Verb element
----------------

The verb element adds a new verb to the game's vocabulary. In XML, a verb might look like this:

      <verb>
        <property>zing</property>
        <pattern>zing; ping; ling; ring ring</pattern>
        <defaulttext>You can't zing that.</defaulttext>
      </verb>

You can create a new verb in quest by right clicking on Obhects and selecting "Add verb"; all the above are then filled in a text boxes.

The verb is called "zing", and it has various synonyms, in the pattern element, each separated by a semi-colon. Any attempt to use this verb (or its synonyms) with an object that is not set up for it will produce the given default text.

Many standard verbs are already defined in the language libraries.


Adding a verb to an element
---------------------------

To associate the verb with an object, go to the verb tab for the object, and click on the plus sign. All the built-in verbs plus your own will be listed in the drop down list. At the bottom, set up what the response will be.

In the XML, it looks like this:

        <object name="test_object">
          <inherit name="editor_object" />
          <zing>It goes zing</zing>
          <alias>obj</alias>
        </object>

In this case, the output is just some text, but you can put in all the usual script stuff.

Special considerations for users of the web editor
--------------------------------------------------

Note that in the web version, verb elements are not accessible. Quest creates them for you just the same; they are just not visible. This means that you cannot edit or delete them. Quest will check that you are not adding a verb that will mess up your game (for example, if you add LOOK AT, that would potentially affect the LOOK for _everything_ in your game), however, it can only do that for single words or phrases. That is, it will stop you adding "look at" and it will stop you adding "examine", however, it will not stop you adding multiple synonyms like "look at;examine".

For users of the web editor, it is therefore safer to add each synonym as a new verb (unless using a built-in verb that already has synonyms). The best way to do that is to have one as the master verb, with the script that does all the work, and have the synonyms use that with the "Run an object's script" command. In the example below, "burn" is the master for the "stool" object, with an admittedly trivial script. Then there is "incinerate", and that has a script that runs the "burn" script on the "stool" object. You can, of course, add as many synonyms as you like.

![](images/verbs-web.png "verbs-web.png")