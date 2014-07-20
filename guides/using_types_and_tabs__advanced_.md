---
layout: index
title: Using Types and Tabs (Advanced)
---

So you know all about types, from the [Using Types](using_types.html) page, right? Let us do some more. By the way, we will be using code, rather than the GUI, mostly.

Use a library
-------------

If we are going to get serious, it is better to put your types in another file (and personally, I would put verbs, commands, functions, turnscripts and templates all in there too), so we will do that first.

One reason to use a library is that Quest can actualy loose some code when going into the GUI, as mentioned on the "Using Types" page, but for things like functions you are (in my opinion) better just working with the code anyway, plus you can add XML comments (inside \<!-- and --\>) and put in line breaks to make it easier to read. Finally, you cannot create and edit tabs through the GUI.

Quest cannot edit library files, so open up in a text editor like Notepad. Your basic library has a start tag and an end tag. I also put in an XML directive first, so I can use an online XML validator on it (such as [1](http://validator.w3.org/#validate_by_input)), in case I have messed up the code so much Quest cannot handle it. So it looks like this:

      <?xml version="1.0"?>
      <library>
      </library>

Go into the code in Quest, and cut the type from there, and paste it into here (if you are following this as a tutorial from the previous page, you will see that I have improved the text here a little).

      <?xml version="1.0"?>
      <library>
        <type name="spell">
          <inventoryverbs type="list">Cast</inventoryverbs>
          <displayverbs type="list">Learn</displayverbs>
          <drop type="boolean">false</drop>
          <take type="boolean">false</take>
          <usedefaultprefix type="boolean">false</usedefaultprefix>
          <learn type="script"><![CDATA[
            if (not this.parent = player) {
              this.parent = player
              msg ("In a process that seems at once unfathomable, and yet familiar, the spell fades away, and you realise you are now able to cast the <i>" + this.alias + "</i> spell.")
            }
            else {
              msg ("Er, you already know that one!")
            }
          ]]></learn>
        </type>
      </library>

It is vital that you cut-and-paste everything from the start tag to the end tag, including both tags. Save this file as "library.aslx" in the same directory as your game.

In your game file in Quest, you need an extra line at the top of the code, telling Quest to include your library. The top five lines will be like this, with your library after the two standard libraries.

      <!--Saved by Quest 5.2.4515.34846-->
      <asl version="520">
        <include ref="English.aslx" />
        <include ref="Core.aslx" />
        <include ref="Library.aslx" />
        ...
     

Now go into the game, and check it still works.

More Types
----------

We will now set up a new type, attackspell. The attackspell is a particular type of spell, so it needs all the properties of an ordinary spell, but perhaps some new ones too. So it wants to *inherit* those properties from spell. Here, then, is our new type:

      <type name="attackspell">
        <inherit name="spell"/>
      </type>

New Tabs
--------

For simplicity, an attack spell will affect everyone in the room (except the player character, of course), but different spells will use different elements and have different power ratings. The way to do that is to have attributes on each spell, powerrating and element. The cool way to do *that*, is to create new tabs in the editor.

Here is the basic code (paste this in before the </library> tag).

      <tab>
        <parent>_ObjectEditor</parent>
        &lt;caption&gt;Spell&lt;/caption&gt;
        <mustnotinherit>editor_room; defaultplayer</mustnotinherit>
        <control>
          <controltype>dropdowntypes</controltype>
          &lt;caption&gt;Spell type&lt;/caption&gt;
          <types>*=Not a spell; spell=Non-attack spell; attackspell=Attack spell</types>
          <width>150</width>
        </control>
      </tab>

So what do we see here? It starts and ends with <tab> and </tab>, so Quest knows this is a tab, and *parent* tells Quest this is an editor for an object (I think they all are). The *caption* is the name on the tab, and *mustnotinherit* stops this tab appearing for rooms and the player.

Then there is our control, and you can have several of these. This one gives a dropdown menu, allowing the user to select whether this is not spell, is a non-attack spell, or is an attack spell. The asterisk indicates the null choice, by the way.

Quest will not realise you have changed your library file; to see a difference, you will need to go into code, make a trivial change (say to the version), then go back into the GUI, to force Quest to reload the editors. Hopefully, if you click on an object you will see something like this:

![](spelltab.png "spelltab.png")

More Controls
-------------

Now we will put in a powerrating control. Try this (remember, it has to go before the </tag> line, because that marks the end of the tag):

        <control>
          <controltype>number</controltype>
          &lt;caption&gt;Power of attack (1-10)&lt;/caption&gt;
          <attribute>powerrating</attribute>
          <width>100</width>
          <mustinherit>attackspell</mustinherit>
          <minimum>0</minimum>
          <maximum>10</maximum>
        </control>

The *controltype* says this is for an integer number, the *caption* is a label the user will see, the *attribute* is what is being set, and *width*, of course, is the size. By setting *mustinherit*, this is only visible for attackspells. Also, *minimum* and *maximum* values are set (but these are only for this tab, the values can change outside the tab by editing the code, or during play, so do not rely on them being in that range).

Now here is a text box, which is pretty straightforward. We can use this for descriptions of what happens.

        <control>
          <controltype>textbox</controltype>
          &lt;caption&gt;Description&lt;/caption&gt;
          <attribute>description</attribute>
          <mustinherit>attackspell</mustinherit>
        </control>

We also want to be able to select an element (fire for fireball, etc). This is a little more complicated, if we want a dropdown list, as these only work with types, but we can create some new types quickly enough (these need to be outside the "tab" tags, but inside the "library" tags).

      <type name="fire_type">
      </type>

      <type name="frost_type">
      </type>

      <type name="storm_type">
      </type>

Now we can create the dropdown list just as we did before (inside the "tab" tags):

        <control>
          <controltype>dropdowntypes</controltype>
          &lt;caption&gt;Element&lt;/caption&gt;
          <types>*=None; fire_type=Fire; frost_type=Frost; storm_type=Storm</types>
          <width>150</width>
          <mustinherit>attackspell</mustinherit>
        </control>

Note that Quest will complain if there is no default value (with \*).

![](spelltab2.png "spelltab2.png")

Other control types available include title, script, checkbox and objects. For more details see this page:

[More on Tabs (Advanced)](more_on_tabs__advanced_.html)

What About the Monsters?
------------------------

So we want monsters in our game too, and they need to interact with the spells. We will need a new type, monster, and add that to the list of options on the Spell" tab (which we can rename as "Magic"). The tab can be set up to set hits, etc. for monsters.

Now the attack spells can be set to only affect monsters, and to check if the monster is vulnerable to this element. This can be done on the *attackspell* type, so any new attack spell gets this automatically. The principles are all the same, so I am not going through them, but here is the start of a full game incorporating these features. I have also modified the types so spells can be learnt from items that are held.
