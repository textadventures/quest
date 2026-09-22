---
title: Verbs in depth
sidebar:
  order: 8
---

We've been using verbs since the very first section - "watch" on the TV, "sit on" on the sofa, "read" on the newspaper. Each is a "doing word" attached to one object, giving a single, specific response. Now that we also know how to build commands, it's worth looking at how the two fit together, and going a bit deeper on what a verb actually is.

## Verbs are attributes

When you add a verb to an object, Quest Viva stores its response as an attribute on that object, named after the verb. What kind of attribute depends on the verb's Behaviour setting on the Verbs tab:

- **Print a message** stores the text as a string attribute.
- **Run a script** stores a script attribute.

A script attribute can be run from anywhere - not just when the player types the verb directly - using `do (object, "attributename")`. `do` only runs scripts, though. Use it on a verb that prints a message and you'll get an error saying the object "has no action" with that name.

If your verb is more than one word, Quest Viva usually squashes it into one word for the attribute name - "look under" would become `lookunder`, for example. Built-in verbs sometimes use a shorter name instead: our sofa's "sit on" verb is one of these. Even though we typed "sit on" into the Add Verb box, it's actually stored in an attribute called `sit`. If you're ever not sure what a verb's real attribute is called, check the object's Attributes tab.

## Combining verbs and commands

Right now, if the player types `SIT ON SOFA`, they get our custom response - but if they just type `SIT`, they get Quest Viva's own generic reply, even with the sofa right there in the room. Let's use what we learned about commands to fix that.

First, we need the sofa's "sit on" verb to be a script, so that we can run it from our command. Select the sofa, go to the Verbs tab and select "sit on". Change its Behaviour from "Print a message" to "Run a script", then add a "Print a message" command to the script with the same text as before - "There's no time for lounging about now." The game behaves exactly the same as it did, but the response is now a script we can call.

Next, select "Commands" in the tree (underneath "game"), click "+ Add", and choose "Add Command". For the command pattern, enter:

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

`sofa.parent = player.parent` checks whether the sofa is in the same room as the player - if so, we run the sofa's own "sit" verb script directly, giving exactly the same response as `SIT ON SOFA`. Quest Viva already has a generic built-in response for a plain `SIT`, but a command you add yourself takes priority over one built into Quest Viva, so ours is the one that runs.

Launch the game, go to the lounge, and try both `SIT` and `SIT ON SOFA` - you should get an identical response either way. Try `SIT` from the kitchen too, and check you get the "nothing to sit on" message instead.

## Going further

Verbs can also involve a second object - for example, handling `ATTACK GOBLIN WITH KNIFE` - and the pattern text a verb matches against can be edited directly, with semicolon-separated synonyms or even a regular expression, in exactly the same way as the command patterns from the last section. See [How to use verbs](/howto/commands/verbs) for both of these in depth.
