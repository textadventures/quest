---
layout: index
title: Using inherited types
---

<div class="alert alert-info">
Note: Inherited types can currently only be edited in the Windows desktop version of Quest.
</div>

Object types in Quest offer a way to give a group of things the same properties. Quest has several object types built in, such as the editor_object type and the editor_room type; it uses these two to flag whether something is an object or a room (incidentally, these types are only there when you are editing the game; when a game is published editor types are not included).

You can see the type of an object by going to the _Attributes_ tab; the top pane is a list of types. Here is an example for an openable container:

![](type_attributes.png "type_attributes.png")

As you can see, something in Quest can be more than one type. As well as being an editor_object type, this is also a container_open type. Also, in grey, it is a container_base; this is an inherited type, my object has this because container_open inherits from container_base. It is also a defaultobject, which is in grey too, because everything in Quest has the defaultobject type.

My container gets all the properties of the types it has inherited, which is how it will work as a container with minimal effort from me. Note that container_base sets up a closed container, but that that value is overridden by container_open. I can further change any of the setting to suit my needs.

You can do all that without knowing what types are, but sometimes we want to do more, and hopefully this tutorial will show you how.


Testing for Types
-----------------

You can test if an object is of a certain type in a script or function using the `DoesInherit` function. In code, it might look like this:
```
  if (DoesInherit (fireball_spell, "spell")) {
    // do stuff
  }
```
Here, "fireball_spell" is the thing we are testing, and we want to know if it is of the spell type. I am guessing it is.


Creating new types
------------------

Creating your own types is a great way to extend Quest for your own needs. Any time you have a bunch of things that are all pretty similar in what they do, but a bit different to anything already in Quest, consider creating a new type.

As an example, we are going to create a spell type. The easiest way to do that is to first do it for an actual example of the type, a prototype, and then to move the code from the object to the type (unfortately, you can only do it this way with the off-line editor).


### Create a prototype

So the first thing to do is to create an actual spell, let us call it "fireball spell". You cannot drop a spell, and you cannot pick one up, so on the Inventory tab, untick "Object can be dropped".

Something else we can do with spells is to learn them, so next we will add a “learn” script to it. Go to the Verbs tab, and add a "learn" verb. Set it to run a script, and paste this code in:
```
  if (not this.parent = game.pov) {
    this.parent = player
    msg ("How about that? You can now cast " + GetDisplayName(this) + ".")
  }
  else {
    msg ("Er, you already know that one!")
  }
```

Briefly then: The first line of the script checks that the player does not already hold the spell. The next line moves this spell to the player, and the next line lets the player know this happened.


### "this"

The important point to notice about the code is that it uses "this". In Quest, "this" refers to the thing the script is attached to. In this case, that will be the spell. Later on this code will be used by the type, and it could be any spell, so we need to keep it generic; nothing there referring to this specific spell.

Go in game, and you should be able to learn the spell, and it will appear in your inventory.


### Create a type

So now we are ready to create a new type. Right click in the Quest right pane, and select “Add Object Type”, in the box type “spelltype”, and click “Okay”.

So far so good, but it does not do anything yet. We will change that by copying code. It may look scary, but if you are careful, it will be pretty easy.

On the menus, go to _Tools - Code_ view. This will show you the code that is your game. If you are not familiar with XML, it will not make much sense, but do not worry about that. Somewhere there will be a bit like this (if you do [CTRL]-F, you can search for "fireball" to find it quickly):
```
    <object name="fireball spell">
      <inherit name="editor_object" />
      <drop type="boolean">false</drop>
      <learn type="script">
        if (not this.parent = game.pov) {
          this.parent = player
          msg ("How about that? You can now cast " + GetDisplayName(this) + ".")
        }
        else {
          msg ("Er, you already know that one!")
        }
      </learn>
    </object>
```
Somewhere else (probably at the bottom) you should find this:
```
  <type name="spelltype" />
```
Step 1. Expand the type XML. The type is using a condensed form of XML because there is nothing in it. Change it to this:
```
  <type name="spelltype">
  </type>
```
Step 2. Cut the attributes from the fireball spell (to leave just this)...
```
    <object name="fireball spell">
      <inherit name="editor_object" />
    </object>
```
Step 3. ... And paste them into the type:
```
  <type name="spelltype">
      <drop type="boolean">false</drop>
      <learn type="script">
        if (not this.parent = game.pov) {
          this.parent = player
          msg ("How about that? You can now cast " + GetDisplayName(this) + ".")
        }
        else {
          msg ("Er, you already know that one!")
        }
      </learn>
  </type>
```
Now go _Tools - Code view_ again, to get back to the GUI.

Now if you look at the spelltype, you should see it has a whole load of attributes now.

The last thing to do is to go back to the fireball spell, and on the attributes tab, to add "spelltype" to its list of inherited types.

Now go into the game, and see if you can still learn that spell.


### More on Creating Types

When you are creating types yourself, you may not want all the attributes in your prototype to be in your type. The best approach then is to copy the code from the prototype, rather than cutting it. Then, when you are in the GUI again, go to the type, and delete the attributes you do not want in the type. Then go to the prototype, and delete from there the attributes you do want in your type there.


### A Note About Lists and Dictionaries

It is worth noting that attributes on types are not mutable - they cannot be changed. You might never notice this, because if you have an object and you attempt to assign a value to an attribute that is set in the object’s type, it works fine - behind the scenes when you assign it, the attribute stops being on the type, and is now on the object, and as far as the game is concerned the attribute value has changed. The problem only arises if you try to change the content of a list or dictionary, so unless you are sure a list or dictionary will not change during a game, I would advise against having them in types.
