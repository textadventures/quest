Quests for Quest!

_NOTE:_ This requires Quest 5.7, which is currently only at beta-testing.

Here is a way to track quests. A quest is a multi-step task for the player, and is a common feature of RPG-style games. The player might have several quests active at at a time, so we need to be able to track where she is in each one. A quest can be:

- "Inactive" The quest has not been given yet
- "Active" An ongoing quest
- "Failed" This quest is over, the player failed to complete it
- "Successful" This quest was completed successfully

The library can be downloaded here:

[QuestLib](https://github.com/ThePix/quest/blob/master/QuestLib.aslx)


### Some Commands

The player has a number of commands available. These three will list the quests in the respective state:

> QUESTS or Q or CURRENT QUESTS or CQ

> SUCCESSFUL QUESTS or SQ

> FAILED QUESTS or FQ

For example:

> \> quests
>
> Current quests:
>
>     <i>An apple for Mary</i>: Give the apple to Mary



The player can also get a summary for a specific quest with:

> QUEST \<quest name>

For example

> \> quest apple
>
> Quest: <i>An apple for Mary</i>
>
>     Get an apple for Mary
>
>     Give the apple to Mary
>
>     Apple given

### Some Functions

You have a small number of functions to handle quests.

```
QuestStart
QuestNext
QuestEnd
QuestFail
```

One of these should be used each time the quest progresses. Each function takes two parameters, the quest object and a string describing the current state of the quest. When the quest starts, call `QuestStart`, when it ends use `QuestEnd`, if it is failed, use `QuestFail` and otherwise `QuestNext`. You can have as many steps as you like, and the system does worry about how one step progresses to the next, so you can jump around inside a quest however you like.

Finally there is `QuestState`, which takes a single parameter, the quest object, and returns the current state, "Active", "Successful", "Failed" or (if it has yet to be started) "Inactive".


### A Quest

Now we need to create a quest. We are going to get an apple for Mary, so first create Mary, and then create an apple. Then create an object called "apple_for_mary" somewhere the player cannot get to, this will be the quest. Give `apple_for_mary` an alias; this is what will be displayed, and is required.

The quest will have three steps:

1. Mary asks for apple
1. Get an apple
1. Give the apple to Mary

It is important to consider what will happen if the player does things out of order or does them several times, and to make allowances for that where necessary. With that in mind, let us start at the end, as Mary can only be given the apple once.

Set this up on the _Use/Give_ tab of Mary (make it visible on the _Features_ tab). Go to the "Give (other object) to this" section, set the action to "Handle objects individually", then add the apple.

For the script, paste this in:

```
msg ("You give the apple to Mary. She gives you a little peck on the check.")
QuestEnd (apple_for_mary, "Apple given")
```

Now go to the apple. Tick that it can be taken, then for the "After taking the object" script, paste this in:

```
QuestNext (apple_for_mary, "Give the apple to Mary")
```

What if the player eats the apple? The quest will be failed (but only if it is active). Go to the _Attributes_ tab, and find "eat" (it will be grey). Click "Make editable copy", and paste in this script. Note that it checks if getting an apple for Mary is an active quest.

```
msg ("Yummy apple!")
if (QuestState(apple_for_mary) = "Active") {
  apple_for_mary.parent = null
  msg ("So what if Mary wants the apple, hey?")
  QuestFail (apple_for_mary, "Eat the apple")
}
this.parent = null
```

Now back to Mary. We want Mary to give the quest to the player when the player talks to her, but only the first time, and not if the player has already eaten the apple. Give Mary a "speak to " verb (_Verbs_ tab), and add this script:

```
if (apple.parent = null) {
  msg ("Well I was going to ask you for an apple...")
}
else if (QuestState(apple_for_mary) = "Successful") {
  msg ("Thanks for the apple.")
}
else if (QuestState(apple_for_mary) = "Inactive") {
  msg ("Can you get me an apple please?")
  QuestStart (apple_for_mary, "Get an apple for Mary")
}
else {
  msg ("Where's that apple?")
}
```

We better go through that. First it checks if the apple has been eaten (if it has, apple.parent will be null). Then it checks if the quest has already been completed. Then it checks if the quest has yet to be given. Note that it gives a message in every situation - the player will expect a response of some sort.

Now play the game and see if you can complete the quest.