---
title: Using scripts
sidebar:
  order: 5
---

We'll now start to play with the real power behind Quest Viva – scripts. Scripts let you do things within the game, change the game world, show pictures and more. With a script, you can print different messages or run other actions depending on the state of any object in the game.

## What a script is

A script is a list of commands, run in order. Each command does one thing — print a message, move an object, change an attribute, ask a question — and you build up whatever you need by stacking them.

You never have to type a script out. The editor gives you a list of every command available, you pick one, and it shows you boxes to fill in. (If you would rather write code directly, you can — see [Editing in full code view](/howto/scripting/codeview) — but everything in this tutorial is done by clicking.)

## Adding a script to a verb

In this example, we'll use a script to customise the "watch" verb we added to the TV in the previous section. We want to update it to provide a sensible response depending on whether the TV is switched on or not.

Select the TV object and go to the _Verbs_ tab. If you've been following all the steps in this tutorial, you should already have a "watch" verb which prints a message. If you've already got a "watch" verb, change it from "Print a message" to "Run a script" (or add a new "watch" verb if you don't already have one).

Click "Add script" and you'll see a list of all the commands you can add to a script. The commands are in broad categories - Output, Objects, Variables and so on - but you can also find a command by typing in the filter box, if you don't know the category.

## Using the "if" command

Go to the Scripts category and add the "If" command (select it and click OK, or just double-click the command).

![](/images/Addif.png)

The "if" command is hugely powerful, because it lets us choose which script to run depending on a condition that we set.

After adding the command, you'll see the following editor:

![](/images/Addif2.png)

First, we need to add a condition. If you click the "expression" dropdown list next to the "If" label, you'll see a list of conditions that you can add. Select "object is switched on".

The editor template will then change, and next to the condition you will now see two more drop-down lists. Leave the first one set to "object", and you'll be able to choose an object from the second list. Select "TV".

![](/images/Addif3.png)

That's our condition added - now we just need to say what happens when the condition is met. Click the "Then" header and you'll see that you can add script commands here too. These script commands will *only* be run *if* the TV is switched on. Add a "Print a message" command.

This will be the text that will appear when the player types `WATCH TV` while the TV is switched on, so enter a message like "You watch for a few minutes. As your will to live slowly ebbs away, you remember that you’ve always hated watching westerns."

We're not done yet - what if the TV is *not* switched on? Fortunately we don't need to add a whole other condition - we can just add an "Else" script to the one we're working on. Click the "Add Else" button, then expand the "Else" header that appears. Add a "Print a message" command again, and this time add a message like "You watch for a few minutes, thinking that the latest episode of ‘Big Brother’ is even more boring than usual. You then realise that the TV is in fact switched off."

Your screen should now look like this:

![](/images/Addif4.png)

Now would be a good time to play the game to test that it works properly. Switch the TV on and off, and verify that you get a sensible response when you type `WATCH TV`.

## Working with a script

A few things are worth knowing once your scripts get longer than two commands:

- Commands run **in order, from top to bottom**. Use the arrows beside a command to move it up or down, and the delete button to remove it.
- The **filter box** at the top of the command list is usually quicker than hunting through the categories, once you know roughly what a command is called.
- A script can be **nested** inside another, as you have just seen — the "Then" and "Else" parts of an `if` are scripts in their own right, and can hold `if` commands of their own.
- Anywhere the editor offers "Run a script" instead of a plain message, you get exactly this editor. Verbs, commands, "after taking the object", a room's "after entering" script — they all work the same way.

## Where else scripts go

The verb you have just written runs when the player does something specific. Scripts can also be attached to moments in the game rather than to a command: the game starting, the player entering a room, the end of every turn. Those live on the _Scripts_ tab of the game object and of each room — see [When scripts run](/howto/scripting/when-scripts-run).

The next few sections of this tutorial use scripts steadily more, so this is the pattern to get comfortable with.
