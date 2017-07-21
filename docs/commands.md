---
layout: index
title: How to use commands
---


When the player types something (or clicks a link), Quest will try to match the command string against all the commands it knows. If it gets a match, then it will process that command.

Let us start with a HELP command. There is already a HELP command in Quest, but we will add our own (somewhat less helpful) version.

Quest searches the commands starting at the bottom, so any command we add will be found first, so our new command will get a match and used, and the built-in one will not.

To add a command:

- In the Windows desktop version, select “game” in the tree. Now, you can right-click on the tree and choose “Add Command”, or use the Add menu and choose “Command”.

- In the web version, select “Commands” in the tree (underneath “game”). Then click the “Add” button.

The pattern we will match against is "help", and we just want to print something, so it will just look like this:

![](CommandHelp.png "CommandHelp.png")

Now if you play the game and type HELP, you will see the new text.

There is a convention in interactive fiction that a question mark can be used as a synonym for HELP, so we better add that. You can add as many synonyms as you like (and should try to think of as many as possible), just separate them with semi-colons. So now the pattern Quest will use is:

> help;?

Commands and objects
--------------------

Often you will want a command to involve an object. To handle that, Quest has a special system (in fact it has two, but verbs are a discussion for another time). Let us say we want to have a command for attacking a zombie. And we want to allow STRIKE and HIT.

We could use this as the pattern:

> attack zombie;strike zombie;hit zombie

That would work... but what if the player does ATTACK UNDEAD or ATTACK FOE? We could just add these synonyms too, but you can quickly get a dozen combinations. Also, we would have to check what zombie is present. Are we in the street with the ragged zombie or the back alley with the decrepit zombie or the cellar with the hat-wearing zombie?

A far better way is to use a place-holder, and to let Quest dynamically match that against any object present.

> attack #object#;strike #object#;hit #object#

All the different words for "zombie" you give to the zombie (on the _Object_ tab), and that will be good for any command.

So now the player can type HIT UNDEAD, and Quest will match this command pattern, and then it will check the objects present, and see if it can find a match for the object. If we are in the cellar, it will match it to the hat-wearing zombie, for example.

### What if there is no matching object?

If Quest cannot match the object, then it will print the "Unresolved object text".

### What if there are several matching objects?

If there are two or more zombies here, Quest will ask the player which one she meant, and then will proceed with that one matched object.

### What if there is one matching object

So we have a single object, called `object`, which we know matches the name the player typed, and we know is present. Your script can now do what it likes with that object as though it is the real thing.

![](CommandAttack.png "CommandAttack.png")

