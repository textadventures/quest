---
layout: index
title: Complex commands
---

So you want to use THIS with THAT? what is the best way to handle it?

> TIE CORD TO HOOK
>
> CUT ROPE WITH KNIFE
>
> ATTACK GOBLIN WITH KNIFE
>
> GET HOT COAL WITH TONGS
>
> IGNITE FIREWORK WITH MATCH

First, remember that GIVE and USE are already built in; if you want to use them, tick the feature on one of the items, and go to the Use/give tab. What about the others? 

As an example, I am going to implement TIE CORD TO HOOK.


Command Pattern
---------------

First you need a command pattern. We could do this:
```
  tie cord to hook
```
If you do that then the player has to type that exact phrase or Quest will not recognise it. We can concatenate alternatives by separating them with semi-colons (and optionally spaces too).
```
  tie cord to hook; tie thread to hook; tie string to hook
```
However, we can do even better, and let Quest handle the synonyms, by using #object#.
```
  tie #object# to hook
```
As long as you have set up the cord with these alternative names (the " Other names" list on the Object tab), Quest will match #object# against any of them. An important benefit here is that your game will have the same set of synonyms for an object whether the player is trying to pick it up, look at it or tie it to a hook.

And you can have as many objects as you like in the pattern; just make sure they start object so Quest will match them against objects that are present (in practice, any more than two will confuse the player). We need two here, the cord and the hook:
```
  tie #object1# to #object2#
```
You might now want to include alternative verbs.
```
  tie #object1# to #object2#; attach #object1# to #object2#; fasten #object1# to #object2#
```


Regular Expression (optional!)
------------------------------

If you are feeling brave, you could use a regular expression here (remember to set Regular expression from the drop down list).
```
  ^(tie|attach|fasten) (?<object1>.*) to (?<object2>.*)$
```
* The ^ at the start says Quest must match this to the start of the command, whilst the $ at the end says this must be the end of the command.

* The (tie\|attach\|fasten) tells Quest it has to match to one of these. One has to match exactly, but it does not matter which.

* (?\<object1\>.*) is equivalent to #object1#; in a regex it is called a "capture group, because it groups some characters together, and captures them for use elsewhere.

* Plain text, like "to", has to be matched exactly.

* If you need to match special characters, you can escape them with a backslash. Backslashes have a special meaning in strings, so you then need to escape the backslash as well! To match a `*`, you therefore need to use `\\*`

Quest uses .NET regex rules, and and a quick reference for .NET regex rules can be found [here](http://msdn.microsoft.com/en-us/library/az24scfc.aspx).

May be not much point in this example, but if you have variations in the joining word to handle too, you could be looking at a lot of combinations, so this way may be easier. For example:
```
  ^(get|pick up|take) (?<object1>.*) (using|holding|with) (the |)(?<object2>.*)$
```
Or even:
```
  ^((get|pick up|take) (the |)(?<object1>.*) (using|holding|with) (the |)(?<object2>.*)|(using|holding|with) (the |)(?<object2>.*) (get|pick up|take) (the |)(?<object1>.*))$
```
That will handle any of these:

> get hat with hook
>
> pick up hat using hook
>
> take the hat with the hook
>
> using the hook take the hat

There is more on regular expressions [here](pattern-matching.html).


Script
------

So we have a command pattern or regular expression that Quest will use to match this command, now we need to do something, so we need a script. Because we used this as the command pattern:
```
  tie #object1# to #object2#
```
... Quest will already have assigned values to two special variables, in this case called object1 and object2. We do not know specifically what they are, but they will be objects that are present in the current room or in the player's inventory.

The way to write the code here is going to be the same for most commands. Think of a checklist; what do we need to check before allowing the command to work?

1. The player has the first object

1. The first object is the cord

1. The second object is the hook

1. The cord is not already tied to the hook

As Quest will only match an object if it is present, so we do not need to check if the hook is present, it must be if Quest found it (and you may choose not to check number 1; does the player need the cord in his inventory or should the command work if the cord is lying on the ground?).

For these four conditions, we convert them to a if/else if/else cascade, at each step testing if it is not so (lines starting with two slashes are comments, by the way):

```
  // 1. The player has the first object
  if (not object1.parent = player) {
    msg("You are not holding " + GetDisplayName(object1) + ".")
  }
  // 2. The first object is the cord
  else if (not object1 = cord) {
    msg("You cannot tie the " + GetDisplayName(object1) + " to anytthing.")
  }
  // 3. The second object is the hook
  else if (not object2 = hook) {
    msg("You cannot tie anything to the " + GetDisplayName(object2) + ".")
  }
  // 4. The cord is not already tied to the hook
  else if (GetBoolean(object1, "tiedtohook")) {
    msg("You've already done that.")
  }
  // Everything checked, so do it
  else {
    msg("You tie the cord to the hook.")
    cord.tiedtohook = true
    code.take = false
    cord.parent = player.parent
  }
```

More General
------------

Suppose there are several objects the cord might be tied to, what is the best way to handle that? What we want is a command that can handle tying the cord to any such object, so the first thing to do is to flag an object as attachable. If you are using the desktop version, go to the _Attributes_ tab of each object, and add a new attribute, "attachable", set it to be a Boolean, and tick it. Now our command can check if the object has that set, and if it does, the cord can be tied to it.

The code here has two changes. Condition number 3 now checks the attachable flag, instead of checking the object in the hook. Also, at the end, an attribute on the cord gets set to the object it is attached to, so you can test what that was if necessary.

```
  // 1. The player has the first object
  if (not object1.parent = player) {
    msg("You are not holding " + GetDisplayName(object1) + ".")
  }
  // 2. The first object is the cord
  else if (not object1 = cord) {
    msg("You cannot tie the " + GetDisplayName(object1) + " to anytthing.")
  }
  // 3. The second object is the attachable
  else if (not GetBoolean(object2, "attachable")) {
    msg("You cannot tie anything to the " + GetDisplayName(object2) + ".")
  }
  // 4. The cord is not already tied to the hook
  else if (GetBoolean(object1, "tiedtohook")) {
    msg("You've already done that.")
  }
  // Everything checked, so do it
  else {
    msg("You tie the cord to the hook.")
    cord.tiedtohook = true
    code.take = false
    cord.parent = player.parent
    cord.attachedto = object2
  }
```
