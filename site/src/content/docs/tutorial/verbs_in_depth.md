---
title: Verbs in depth
sidebar:
  order: 8
---

We've been using verbs since the very first section - "watch" on the TV, "sit on" on the sofa, "read" on the newspaper. Each is a "doing word" attached to one object, giving a single, specific response. Now that we also know how to build commands, it's worth looking at how the two fit together, and going a bit deeper on what a verb actually is.

## Verbs are just script attributes

When you add a verb to an object, Quest stores its response as a script attribute on that object, named after the verb. You can run that same script from anywhere - not just when the player types the verb directly - using `do (object, "attributename")`.

If your verb is more than one word, Quest usually squashes it into one word for the attribute name - "look under" would become `lookunder`, for example. Built-in verbs sometimes use a shorter name instead: our sofa's "sit on" verb is one of these. Even though we typed "sit on" into the Add Verb box, the script is actually stored in an attribute called `sit`. If you're ever not sure what a verb's real attribute is called, check the object's Attributes tab.

## Combining verbs and commands

Right now, if the player types `SIT ON SOFA`, they get our custom response - but if they just type `SIT`, they get Quest's own generic reply, even with the sofa right there in the room. Let's use what we learned about commands to fix that.

Select "Commands" in the tree (underneath "game"), click "+ Add", and choose "Add Command". For the command pattern, enter:

    sit

Switch to Code View for the script, and enter:

```quest
if (sofa.parent = player.parent) {
  do (sofa, "sit")
}
else {
  msg ("There's nothing to sit on here.")
}
```

`sofa.parent = player.parent` checks whether the sofa is in the same room as the player - if so, we run the sofa's own "sit" verb script directly, giving exactly the same response as `SIT ON SOFA`. Quest already has a generic built-in response for a plain `SIT`, but a command you add yourself takes priority over one built into Quest, so ours is the one that runs.

Launch the game, go to the lounge, and try both `SIT` and `SIT ON SOFA` - you should get an identical response either way. Try `SIT` from the kitchen too, and check you get the "nothing to sit on" message instead.

## Going further

Verbs can also involve a second object - for example, handling `ATTACK GOBLIN WITH KNIFE` - and the pattern text a verb matches against can be edited directly, with semicolon-separated synonyms or even a regular expression, in exactly the same way as the command patterns from the last section. See [How to use verbs](/howto/commands/using_verbs) for both of these in depth.
