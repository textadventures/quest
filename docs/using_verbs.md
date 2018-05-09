---
layout: index
title: How to use verbs
---


Verbs are an alternative to commands that can be simpler to use, but do seem to lead to some confusion. We did look at them in the [tutorial](tutorial/using_scripts.html), but it is worth looking at them in more depth.

Verbs are always used in conjunction an object, so ROTATE KNOB could be set up as a verb, but STAND UP or JUMP will require commands. You could use a command for ROTATE KNOB, but using a verb is probably simpler. If the player will only try to do this with one or two objects, use a verb. If she could potentially do it with anything, and the outcome will be broadly similar (such as a SELL command - the player could try to sell any object, and you would handle them all the same), a command might be better; however there is no hard and fast rule.

To create a verb, select the appropriate object, and go to the _Verbs_ tab. Click add, and type in your verb. Your verb will appear in the upper box, and the response can be set in the section below, either "Print a message" or "Run a script". Let us suppose we set this up with a script to make things happen when the dial is turned (how to do scripts is not something I will be dealing with here).



Verbs are just script attributes
--------------------------------

When you create a verb for an object, it is just a script attribute of that object, and scripts can be called whenever you like. Let us suppose you have an object that is a chair, and you create a "sit on" verb, with an appropriate script. That works fine if the player types SIT ON CHAIR, but what if she just types SIT? You need a command to handle that, but you can still use your verb here. Create your command, then, in the script, have it first test that sitting is appropriate (that the chair is in the current room), and if it is, invoke the script on the chair object.

Here is the script for the command:

    if (chair.parent = player.parent) {
      do (chair, "sit")
    }
    else {
      msg ("Nothing to sit on here!")
    }

The second line is where the "sit on" verb is invoked.

By the way, if your verb is multiple words (such as "sit on") Quest will run them together into one long word, "siton". However, some built-in verbs have been set up differently, so in this case the attribute name is just "sit". You can check what name Quest is using by looking on the Attributes tab if you are using the desktop version.

This trick is also useful when you have a verb that can mean different things. You might want your game to handle these:

> LIGHT MATCH

> STRIKE MATCH

> PUNCH MAN

> STRIKE MAN

What you can do is set up match with a "light" verb, and man with a "punch" verb. Then add a second verb to each for "strike". All the second verb does is invoke the other verb. So on the match, the "strike" verb does this:

    do(this, "light")

On the man, the "strike" verb does this:

    do(this, "punch")

In case you are wondering, Quest understands "this" to mean the object to which the script is attached. It is good practice to use "this" rather than the name of the object; for one thing, you may later rename an object, perhaps giving the man a proper name.


Multiple Objects
----------------

You can also set up verbs to handle multiple objects, so we could use them for ATTACK GOBLIN WITH KNIFE. Whether this is preferable to using a command is debatable. Again it depends how specific it is. If there are several things you can attack the goblin with, all doing pretty much the same thing, use a command. If there are specific combinations that apply then a verb is probably easier.

The first step is to add the verb to the object, in this case the goblin. However, instead of setting "attack" to use a script, set it to "Require another object". this will then put up a list of objects (currently empty) to which you can add the knife by clicking Add. You will then get a new dialogue box, into which you can put your script.


Notes
-----

Some verbs are already implemented, such as "speak to" and "sit on". If you start to type in the Add Verb box, you will see these appear as options. If this is the case, check what the attribute name is. If you add "talk to" as a verb, it will get added as an attribute called "speak", for example.

```
lieon             lie on #object#; lie upon #object#; lie down on #object#; lie down upon #object#
listento          listen to #object#
siton             sit on #object#; sit upon #object#; sit down on #object#; sit down upon #object#
speak             speak to #object#; speak #object#; talk to #object#; talk #object#
```

A few verbs cannot be implements, as they already mean something in Quest. "Open" and "close", and "switch/turn on/off" and "enter" are the main examples. Quest should warn you if you try to do this, as it can have far-reaching consequences in your game.


The Verb Element
---------------

<div class="alert alert-info">
Note: Verb elements can currently only be edited with the Windows desktop version, though they are present in all games.
</div>

The text Quest uses to match against goes into the verb element, and as Quest quietly creates these for you it is easy to miss they even exist (especially on the web version, where they are not accessible!). Look for them under the game object. Here is one for our ROTATE verb.

![](images/verb_element.png "verb_element.png")

The first bit is the text that Quest will match against, just as with a command. You can change this, to allow for synonyms, with each word separated by semi-colons, like this:

> rotate; turn; twist

The "Attribute" is the name of the attribute on the object, it tells Quest to use the "rotate" script attribute in this case.

The third part ("Default" and the text box below) is what Quest will use if the player tries this verb on something you have not implemented it for (and Quest will even generate this default text for you, so the above is the default default!). You can, of course, change this to your liking.

The fourth part is for handling multiple objects for your verb. Remember the ATTACK GOBLIN WITH KNIFE verb?

The "Object separator" defaults to "with; using". This is a list, separated by semi-colons, of words that will go between the two objects, i.e., between GOBLIN and KNIFE. In this case the default is what we want.

If the player just types ATTACK GOBLIN, she will be presented with a menu of appropriate objects, and the "Menu caption" will be the caption for that menu. If there are no such objects around, the "If no objects available..." text is shown.


### Complex verbs

Verbs are a simple way to add commands to your game, but they only handle commands of the form `VERB OBJECT`, such as THROW BALL. Or so you might think. In fact, in the desktop version you can edit the verb element to cover a lot of possible commands.

Say we have a HUG command. We can implement HUG MARY very easily, but what about GIVE MARY A HUG? Sure, just use this as the pattern:

```
hug;give #object# a hug
```

Each option is separated by a semi-colon. The second option includes `#object#` - just as commands do - as a stand-in for the object name (note that it has to be called "object"; with commands it can be anything that starts "object"). The above will handle HUG MARY and GIVE MARY A HUG, but not GIVE MARY HUG, but we can add that too:

```
hug;give #object# a hug;give #object# hug
```

Note that the order is important here. If you use this:

```
hug;give #object# hug;give #object# a hug
```

... Quest will get a match with the second option, then complain it cannot find a "mary a".


As with commands, you can also use a Regex to match against (change "Pattern" to "Regular expression"). 

```
^(hug (?<object>.*)|give (?<object>.*?) (a hug|hug))$
```

By default, Regex matching is "greedy", and will try to grab as much as it can, so again will attempt to grab "mary a" as the object. The question mark after the asterisk makes that non-greedy so it takes the minimum, leaving the "a" out of the object name.

For verbs that use two objects, Quest will append the option to include the second object in the command pattern, which will just confuse it if you are using a regular expression. That appears to be no way to successfully use a regular expression with multiple objects for a verb. There also seems to be no way to reverse the order (to allow for ATTACK GOBLIN WITH KNIFE and USE KNIFE TO ATTACK GOBLIN) using the command pattern. In both cases you will need to use commands.
