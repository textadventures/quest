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
