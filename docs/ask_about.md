---
layout: index
title: Handling ask ... about
---

Interactive fiction has broadly three ways for the player to talk to NPCs.

> SAY ...

> TALK TO ...

> ASK ... ABOUT ...

This page is about using ask ... about.

This is something built in to Quest, but it does need to be turned on. On the _Features_ tab of the game object, tick the "Ask/Tell" box. You should now find there is an _Ask/Tell_ tab for the objects in your game.

I am just going to focus on ASK on this page, but you will see there is also the capability to do TELL too. It works just the same, so everything here can also be applied to TELL.

Ask ... about
-------------

The way ASK works is that you give a list of topics and corresponding scripts. Suppose we have a character, Mary, and we want to ask her about the murder of Dr Black. In the ASK section, click _Add_, and then type in the topic, "dr black". Click _Okay_, and then add the script that will run when the player asks about Dr Black. For now, just have it print a simple message.

Now go in game, and type ASK MARY ABOUT DR BLACK, and you should see your message. Quest will do its best to match topics, so you should also see the message if you type ASK MARY ABOUT DR or ASK MARY ABOUT BLACK. Ah, but what if the player types ASK MARY ABOUT DOCTOR BLACK? Go back to the _Ask/Tell_ tab for Mary, make sure this topic is selected and click on "Edit Key". Replace "dr black" with "dr doctor black".

What Quest does is attempt to match the topic against each word in the topic's key, rather than exactly matching the whole thing, so you can put a list of keywords here, separated by spaces.

Obviously you can change the script that runs by selecting the topic and clicking "Edit Script". However, I am not going to worry about that in this tutorial.


More advanced...
----------------

We can make this system more sophisticated in a number of ways. However, we will be using code to do that. Why? Well, it is easier for me to show you, but it is easier for you to put in your game, as you can just copy-and-paste the code. We will keep it simple, and let you know what you need to change for your game, and what can be pasted in without any changes.

For more on how to copy-and-paste code, see [here](copy_and_paste_code.html).


Default replies
---------------

Underneath the list of ASK topics, you can put a script to run when there is no topic for this character. Perhaps for Mary we could print a message that says "She looks at you, wondering what you are talking about."

As of Quest 5.7, you can use a special variable, "text", which will contain the subject the player was asking about. You could have something like this:

    msg("Mary shrugs, and says, 'I know nothing about " + text + ".'")


Topics
------

It can be frustrating for the player to have to guess what topics are available, so an option is to provide a TOPICS command, which simply lists the topics the player can ask about. The best way to do this is to set up a string with some initial entries, and then to add to it as the game progresses. In the investigation of the murder of Dr Black, new topics could be added as new evidence comes to light, for instance.

Note that this assumes all NPCs will have the same topics available. I think it likely that the player will assume that if she can ask Mary about Dr Black, she can ask any other character too. This does mean you will need to add all these topics to all the NPCs.

The first step, then, is to create our string list. If using the off-line editor, go to the _Attributes_ tab of the game object, and create a new attribute called "topics". Set it to be a string list, and add names of any topics that will be available from the start. 

If using the on-line editor, you will have to add the attribute in a script. Go to the _Scripts_ tab of the game object, and add this to the start script (this will set "Job" and "Alibi" as topics from the start):
```
game.topics = Split("Job;Alibi", ";")
```
This will be shown to the player, so capitalise it nicely and bear in mind it needs to fit the ASK ... ABOUT ... format. Also, it makes the script for the topics command easier if there are at least two topics from the start.

Now we need to create a new command. Go to _Commands_ in the left pane, then click "Add" in the right pane. For the command pattern, type in "topics", and paste in this code:
```
msg ("Topics you might want to ask about include:")
foreach (s, game.topics) {
  msg (s)
}
```
You can add to the list of topics at any time during the game, as the plot develops. For example:
```
list add(game.topics, "Forensic results")
```


Ask about ...
-------------

If there is only one NPC in the room, we can save the player some typing by creating an ASK ABOUT ... command. Go to _Commands_ in the left pane, then click "Add" in the right pane. For the command pattern, type in "ask about #text#", and paste in this code:
```
npcs = NewObjectList()
opts = NewStringDictionary()
foreach (o, GetDirectChildren(player.parent)) {
  if (HasAttribute(o, "ask")) {
    list add (npcs, o)
    dictionary add (opts, o.name, GetDisplayAlias(o))
  }
}
if (ListCount(npcs) = 0) {
  msg ("You can ask, but no one is here to tell you anything.")
}
else if (ListCount(npcs) = 1) {
  DoAskTell (ObjectListItem(npcs, 0), text, "ask", "askdefault", "DefaultAsk")
}
else {
  game.askabouttext = text
  ShowMenu ("Ask who?", opts, true) {
    if (not result = null) {
      o = GetObject(result)
      DoAskTell (o, game.askabouttext, "ask", "askdefault", "DefaultAsk")
    }
  }
}
```
What the code does is firstly go through all the objects in the current room (which it gets using `GetDirectChildren(player.parent)`), and collects up all those with an "ask" attribute (this is where Quest stores the topics we set earlier, so this is a good test of whether the object is a character).

Then it looks at how many NPCs it found. If none, an error message; if one, then it calls the same function that the built-in ask/tell system uses, `DoAskTell`, using the one NPC it found.

If it found more than one, it will show a menu, asking the player to select one, and then again call `DoAskTell`.


Ask ...
-------

A further option we could give the player is to ask a character, and then offer a list of topics to ask about. We already have the list of topics from the TOPICS command, so half the work is done.

Go to _Commands_ in the left pane, the click "Add" in the right pane. For the command pattern, type in "ask #object#", and paste in this code:
```
game.askaboutobject = object
ShowMenu ("Ask about?", game.topics, true) {
  if (not result = null) {
    DoAskTell (game.askaboutobject, result, "ask", "askdefault", "DefaultAsk")
  }
}
```
It is even more important that every NPC have a response to all the topics now.