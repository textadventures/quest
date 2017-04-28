---
layout: index
title: How to use verbs
---

<div class="alert alert-info">
Note: While verbs can be used by all users of Quest, verb elements can currently only be edited with the Windows desktop version, and most of the discussion is only relevant to thet.
</div>


Verbs are an alternative to commands that can be simpler to use, but do seem to lead to some confusion. We did look at them in the [tutorial](tutorial/using_scripts.html), but it is worth looking at them in more depth.

Verbs are always used in conjunction an object, so ROTATE KNOB could be set up as a verb, but STAND UP or JUMP will require commands. You could use a command for ROTATE KNOB, but using a verb is probably simpler.

To create a verb, select the appropriate object, and go to the _Verbs_ tab. Click add, and type in your verb. Your verb will appear in the upper box, and the response can be set in the section below, either "Print a message" or "Run a script". Let us suppose we set this up with a script to make things happen when the dial is turned (how to do scripts is not something I will be dealing with here).

There are actually two parts to a verb. The verb object and the script on our object. The script on our object is just an attribute, which is called "rotate" in this example, but that is really just a name for Quest; the player never sees it.


The Verb Element
---------------

The text Quest uses to match against goes into the verb element, and as Quest quietly creates these for you it is easy to miss they even exist. Look for them under the game object. Here is one for our ROTATE verb.

verb_element.png

This is the underlying XML:

    <verb>
      <property>rotate</property>
      <pattern>rotate</pattern>
      <defaultexpression>"You can't rotate " + object.article + "."</defaultexpression>
    </verb>


There are three parts to it. The first is the property element (if you are looking in the GUI, it is called "Attribute", which makes more sense), this is the name of the attribute on the object. The second is the text that Quest will match against. You can change this, to allow for synonyms, with each word separated by semi-colons, like this:

    <verb>
      <property>rotate</property>
      <pattern>rotate; turn; twist</pattern>
      <defaultexpression>"You can't rotate " + object.article + "."</defaultexpression>
    </verb>


The third part is what Quest will use if the player tries this verb on something you have not implemented it for (and Quest will even generate this default text for you, so the above is the default default!). You can, of course, change this to your liking.

Once you have the ROTATE verb set up, Quest will use that for all objects that have that verb.

Remember that you can also set up synonyms for all your objects on the _Object_ tab, so we could have "dial" and "tuner" for our knob object. The player can TURN DIAL, TWIST TUNER, or any combination. Plus, those synonyms for rotate are there for every object, and those synonyms for the dial are there for every verb. This may not seem a big deal as you start to create your masterpiece, but if you get to beta-testing, and someone wants to use REVOLVE, it is trivial to add that to the ROTATE verb. It could be a real pain to have to update every combination of synonym for every turnable object.

If you start to use types, verbs become even more useful, but that is beyond this discussion...


Verbs are just script
---------------------

When you create a verb for an object, it is just a script attribute of that object, and scripts can be called whenever you like. Let us suppose you have an object that is a chair, and you create a "sit on" verb, with an appropriate script. That works fine if the player types SIT ON CHAIR, but what if she just types SIT? You need a command to handle that, but you can still use your verb here. Create your command, then, in the script, have it first test that sitting is appropriate (that the chair is in the current room), and if it is, invoke the script on the chair object.

Here is the script for the command:

    if (chair.parent = player.parent) {
      do (chair, "sit")
    }
    else {
      msg ("Nothing to sit on here!")
    }

The second line is where the "sit on" verb is invoked.

By the way, if your verb is multiple words (such as "sit on") Quest will run them together into one long word, "siton". However, some built-in verbs have been set up differently, so in this case the attribute name is just "sit". You can check what name Quest is using by looking on the Attributes tab.

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

You can also set up verbs to handle multiple objects, so we could use them for ATTACK GOBLIN WITH KNIFE. Whether this is preferable to using a command is debatable (if there are several things you can attack the goblin with, all doing pretty much the same thing, use a command), but we will look at how it is done.

The first step is to add the verb to the object, in this case the goblin. However, instead of setting "attack" to use a script, set it to "Require another object". this will then put up a list of objects (currently empty) to which you can add the knife by clicking Add. You will then get a new dialogue box, into which you can put your script.

Now go to the verb object. In the lower half of the data, you will see the "Multiple Objects" section. Perhaps the most important is the "Object separator", which defaults to "with; using". This is a list, separated by semi-colons, of words that will go between the two objects, i.e., between GOBLIN and KNIFE. In this case the default is what we want.

If the player just types ATTACK GOBLIN, she will be presented with a menu of appropriate objects, and the "Menu caption" will be the caption for that menu. If there are no such objects around, the "If no objects available..." text is shown.

If there are several monsters that can be attacked with the knife, you would do well to create a new type, say "monster", and have the verb set up on that, and then set your goblin and other foes to be on the "monster" type. For more on types, see here:
http://docs.textadventures.co.uk/quest/guides/using_types.html


Notes
-----

Some verbs are already implemented, such as "speak to" and "sit on". If you start to type in the Add Verb box, you will see these appear as options. If this is the case, check what the attribute name is. If you add "talk to" as a verb, it will get added as an attribute called "speak", for example.

A few verbs cannot be implements, as they already mean something in Quest. Open and close, and switch/turn on/off are the main examples.



Complex verbs
-------------

Verbs are a simple way to add commands to your game, but they only handle commands of the form `VERB OBJECT`, such as THROW BALL. Or so you might think. In fact, you can edit the verb element to cover a lot of possible commands.

Say we have a HUG command. We can implement HUG MARY very easily, but what about GIVE MARY A HUG? Sure, just use this as the pattern:

```
hug;give #object# a hug
```

Each option is separated by a semi-colon. The second option includes `#object#` - just as commands do - as a stand in for the object name. The above will handle HUG MARY and GIVE MARY A HUG, but not GIVE MARY HUG, but we can add that too:

```
hug;give #object# a hug;give #object# hug
```

Note that the order is important here. If you use this:

```
hug;give #object# hug;give #object# a hug
```

... Quest will get a match with the second option, then complain it cannot find a "mary a".


As with commands, you can also use a Regex to match against. 

```
^(hug (?<object>.*)|give (?<object>.*?) (a hug|hug))$
```

By default, Regex matching is "greedy", and will try to grab as much as it can, so again will attempt to grab "mary a" as the object. The question mark after the asterisk makes that non-greedy so it takes the minimum, leaving the "a" out of the object name.

