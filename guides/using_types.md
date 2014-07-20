---
layout: index
title: Using Types
---

Using Object Types
------------------

Object types in Quest offer a way to give a group of things the same properties. Quest has several object types built in, such as the *editor\_object* type and the *editor\_room type*; it uses these two to flag whether something is an object or a room (incidentally, these types are only there when you are editing the game; when a game is published editor types are not included).

You can see the type of an object by going to the "Attributes" tab; the top pane is a list of types. Here is an example for an openable container:

![](inherited_types.png "inherited_types.png")

As you can see, something in quest can be more than one type. As well as being an *editor\_object* type, this is also a *container\_open* type. Also, in grey, it is a *container\_base;* this is an inherited type, my object has this because *container\_open* inherits from *container\_base*. It is also a *defaultobject*, which is in grey because everything in Quest has the *defaultobject* type.

My container gets all the properties of the types it has inherited, which is how it will work as a container with minimal effort from me. Note that *container\_base* sets up a closed container, but that that value is overridden by *container\_open*. I can further change any of the setting to suit my needs.

You can do all that without knowing what types are, but sometimes we want to do more, and hopefully this tutorial will show you how.

Creating new types
------------------

Creating your own types is a great way to extend Quest for your own needs. Any time you have a bunch of things that are all pretty similar in what they do, but a bit different to anything already in Quest, consider creating a new type.

As an example, we are going to create a *spell* type. So let us do that now. Right click in the Quest right pane, and select "Add Object Type", in the box type "spell", and click "Okay".

So far so good, but it does not do anything use. There are certain properties that spells have, so let us assign them to the spell type, then all spells will be like that.

Firstly, you cannot drop a spell, and you cannot pick one up. So we can set both drop and take to false. Make sure you have your object type open, and look at the bottom half of the "Attributes" tab. This lists the attributes. Click on "Add", and type "drop", then set this to be a Boolean, and have the checkbox not ticked. Do the same again for a second attribute "take". It should look something like this:

![](attributes2.png "attributes2.png")

Testing for Types
-----------------

You can test if an object is of a certain type in a script or function using the DoesInherit function. In code, it might look like this:

      if (DoesInherit (fireball_spell, "spell")) {
        // do stuff
      }

In the GUI, it looks like this:

![](doesinherit.png "doesinherit.png")

Adding Scripts
--------------

Something else we can do with spells is to learn them, so next we will add a "learn" script to the object type. We will do this in code, so click on the "code" icon at the right end of the tool bar, and look for some code like this:

      <type name="spell">
        <drop type="boolean">false</drop>
        <take type="boolean">false</take>
      </type>

The code behind Quest can look a bit intimidating, but is worth getting to know. The first line here is saying that this is an object type that we are dealing with, and its name is "spell". The second line sets up an attribute, "drop", as a Boolean with the value false, and the third line does the same for the take attribute. The last line closes off this thing.

We want to change it, to this, inserting 9 new lines:

      <type name="spell">
        '''<learn type="script">'''
          '''if (not this.parent = game.pov) {'''
            '''this.parent = player'''
            '''msg ("How about that? You can now cast " + this.alias + ".")'''
          '''}'''
          '''else {'''
            '''msg ("Er, you already know that one!")'''
          '''}
        '''</learn>'''
        <drop type="boolean">false</drop>
        <take type="boolean">false</take>
      </type>

This is adding a third attribute, this time called "learn", but this attribute is a script, and the next 7 lines are the code of the script.

Briefly: The first line of the script checks that the player does not already hold the spell ("this" indicates the object the script is attached too, i.e., the spell; "this.parent" is the object or room holding the spell). The next line moves this spell to the player (we use game.pov for the player, in case you want to change the player object ever), and the next line lets the player know this happened.

Does it Work?
-------------

So let us see this work! Go back into the GUI, and create an object called spellbook. Set it to be a container that can be opened and closed, and will list children when opened or looked at. Then put some spells inside the spell book. Remember that on the "Attributes" tab, you must set each spell to be of the "spell" type.

We also need to set up a couple of verbs. Still in the GUI, right click in the right pane and select Add verb; type in "learn". Do the same for "cast", ready for later.

Now when you go in game you should be able to open the spell book and learn the spells, but not take or drop them.

It would be worth while at this point setting up the cast verb for each spell. Each spell will be different, so you are on your own here, but to get you started: select the spell, and go to the "Verbs" tab; click "Add", and type in "cast". Now below this pane, select "Script", and put in what you want it to do. In the trivial example below, it just prints a message, without actually affecting the world at all.

![](cast.png "cast.png")

Display and Inventory Verbs
---------------------------

Now we are going to change the type so that *Cast* and *Learn* appear as options in the right pane when the player clicks on the object. It is going to look like this:

![](verbs.png "verbs.png")

First, I suggest that you untick "Automatically generate object display verbs list" on the Room Descriptions tab of the game object, so other verbs are not there. We do not want the play clicking on "Drop" for our fireball spell.

Go to your type object, and the attributes section. Add a new attribute "displayverbs" (you need to get that exactly right, all lower case, no spaces); set it to be a string list, and add "Learn" to it. Then add another attribute "inventoryverbs" (again, needs to be exact); set it to be a string list, and add "Cast" to it. Should look like this:

![](verbs_for_type.png "verbs_for_type.png")

In Quest, an inventory verb is a command for an object the player is holding (in our example, *cast*), while a display verb is a command for an object the player can see, but is not holding (such as *learn*).

This is what the code for our type will now look like:

      <type name="spell">
        <displayverbs type="stringlist">
          <value>Learn</value>
        </displayverbs>
        <inventoryverbs type="stringlist">
          <value>Cast</value>
        </inventoryverbs>
        <learn type="script">
          if (not this.parent = game.pov) {
            this.parent = game.pov
            msg ("How about that? You can now cast " + this.alias + ".")
          }
          else {
            msg ("Er, you already know that one!")
          }
        </learn>
      </type>

By the way, if you pick up the spell book, the verb for the spells becomes *Cast*, even though you still have to learn them, which is a problem for a release game, but good enough here where I am just showing how these things work. See the downloads at the end of the [Using Types and Tabs (Advanced)](using_types_and_tabs__advanced_.html) to see this resolved.

A Note About Attributes
-----------------------

Generally attributes that can be set on an object can be set just the same on a type. However, there are a couple of exceptions to this.

It is also worth noting that attributes on types are not mutable - they cannot be changed. You might never notice this, because if you have an object and you attempt to assign a value to an attribute that is set in the object's type, it works fine, because when you assign it the attribute stops being on the type, and is not on the object. The problem only arises if you try to change a list or dictionary (see also [Notes](../notes.html)).

Quest sometimes uses "implied" values to determine the type of an attribute. This works fine for objects but not for types. This seems not to be used in Quest 5.4, but in earlier versions "displayverbs" and "inventoryverbs" were like this.

The Complete Example
--------------------

This is the finished code from the above, which you can paste into Quest (in code mode), to see it all in action. Now updated for Quest 5.4.

      <asl version="540">
      <include ref="English.aslx" />
      <include ref="Core.aslx" />
      <game name="SpellDemo">
        <gameid>9b1b2320-14a1-4ae7-903c-5dd29b2c66ff</gameid>
        <version>1.1</version>
        <autodisplayverbs type="boolean">false</autodisplayverbs>
      </game>
      <object name="room">
        <inherit name="editor_room" />
        <object name="player">
          <inherit name="defaultplayer" />
          <object name="aura_spell">
            <inherit name="editor_object" />
            <inherit name="spell" />
            <alias>Aura</alias>
            <cast type="script">
              msg ("You say '" + this.alias + ".'")
            </cast>
          </object>
        </object>
        <object name="spellbook">
          <inherit name="editor_object" />
          <inherit name="container_open" />
          <open />
          <close />
          <transparent type="boolean">false</transparent>
          <hidechildren />
          <isopen type="boolean">false</isopen>
          <listchildren />
          <listchildrenprefix>It contains</listchildrenprefix>
          <object name="fireball_spell">
            <inherit name="editor_object" />
            <inherit name="spell" />
            <alias>Fireball</alias>
            <cast type="script">
              msg ("You say '" + this.alias + " and a ball of fire shoots off.. somewhere.'")
            </cast>
          </object>
        </object>
      </object>
      <verb>
        <property>learn</property>
        <pattern>learn</pattern>
        <defaultexpression>"You can't learn " + object.article + "."</defaultexpression>
      </verb>
      <verb>
        <property>cast</property>
        <pattern>cast</pattern>
        <defaultexpression>"You can't cast " + object.article + "."</defaultexpression>
      </verb>
      <type name="spell">
        <displayverbs type="stringlist">
          <value>Learn</value>
        </displayverbs>
        <inventoryverbs type="stringlist">
          <value>Cast</value>
        </inventoryverbs>
        <learn type="script">
          if (not this.parent = game.pov) {
            this.parent = game.pov
            msg ("How about that? You can now cast " + this.alias + ".")
          }
          else {
            msg ("Er, you already know that one!")
          }
        </learn>
      </type>
      <function name="TestFunction" parameters="obj" type="boolean">
        return (DoesInherit (obj, "spell"))
      </function>
      </asl>

If you are feeling brave, you might like to see this extended on [this page](simple_combat_system__advanced_.html)
