---
title: Character creation
sidebar:
  order: 1
---

Some text adventures leave the protagonist an empty slate, with no background or even a specific gender. In others, the protagonist is a certain person, and for the course of the game the player assumes the role of someone devised by the author. The third option is to let the player choose - after all, that is the nature of the genre.

To do that in Quest Viva involves setting up a start script on the game object and asking the player a series of questions.

We will look at various ways of doing that, starting with the most basic.


## Just a couple of questions

Go to the "Scripts" tab of the "game" object. The start script is at the top. Set the script to print a message prompting the player, then add a "Set a variable or attribute" action, and pick "player's typed input" from the list of value templates (this is `GetInput()` under the hood - you'll see that if you switch to code view). Set it up like this:

![](/images/Creation1.png)

In code view it will look like this:

```quest
<start type="script">
  msg ("Let's generate a character...")
  msg ("First, what is your name?")
  player.alias = GetInput()
  msg ("Hi, " + player.alias)
</start>
```

The important part is the `GetInput()` function, which suspends the game until the player types something, then returns what they typed as a string. There's no need to wrap the rest of the script in a block waiting for a callback - execution just continues on the next line once the player has answered, same as any other function call.

You can keep adding more questions the same way. When you want to limit the player to a set of choices, use a menu instead - add another "Set a variable or attribute" action and pick "player's choice from a menu" (`ShowMenu` in code view). Like `GetInput()`, it waits for the player to choose, then carries on with the next line:

![](/images/Creation2.png)

In code view it will look like this:

```quest
<start type="script">
  msg ("Let's generate a character...")
  msg ("First, what is your name?")
  player.alias = GetInput()
  msg ("Hi, " + player.alias)
  classes = Split("Warrior;Wizard;Priest;Thief", ";")
  player.class = ShowMenu("Your character class?", classes, false)
  msg (player.alias + " is a " + LCase(player.class) + ".")
  msg ("Now press a key to begin...")
  WaitForKeyPress
  ClearScreen
</start>
```

A menu needs a list of options, and `Split` gives a quick way to create one from a string - here we put it in a variable called `classes` first. The last parameter says whether the player can ignore the menu, which we don't want here. `WaitForKeyPress` pauses until the player presses a key, so they can read the summary before the screen is cleared.

See [Asking the player](/howto/scripting/asking-the-player) for more about `GetInput`, `ShowMenu` and the other ways to ask questions.


## Setting up the character

Once you know the answers, you can set up the player. A `switch` is a tidy way to do something different for each class:

```quest
switch (player.class) {
  case ("Warrior") {
    player.strength = 4
    player.agility = 1
    player.magic = 0
    sword.parent = player
  }
  case ("Wizard") {
    player.strength = 0
    player.agility = 1
    player.magic = 4
    black_robes.parent = player
  }
  case ("Priest") {
    player.strength = 2
    player.agility = 0
    player.magic = 2
    white_robes.parent = player
  }
  case ("Thief") {
    player.strength = 1
    player.agility = 4
    player.magic = 0
    black_catsuit.parent = player
  }
}
```

Give every class a value for every attribute you plan to use, even if it's zero. An attribute you never set doesn't default to zero - it doesn't exist at all, and doing arithmetic with it causes an error.

Because each question simply waits for its answer, a later question can depend on an earlier one - just ask it inside an `if`:

```quest
if (player.class = "Wizard") {
  schools = Split("Fire;Ice;Illusion", ";")
  player.school = ShowMenu("Which school of magic?", schools, false)
}
```

If the start script gets long, you can move parts of it into functions - say, `CharacterCreationClass` - and call them one after another from the start script.

You should consider carefully if you want the player to know what bonuses they will get for each choice. If you decide to do so, they are likely to pick choices that maximise one attribute. On the other hand, if you choose not, they may end up with a mediocre character that is not good at anything.



## A creation room

A useful trick is to start the player in a blank room, with no exits, description or objects. This will prevent the player doing anything until the character creation is over and nothing will be displayed in the panes on the right.

At the end of the creation process, move the player to the start room. This will conveniently trigger the description for that room, setting the scene.


## A note about random stats

You may be tempted to generate stats randomly. This is more in keeping with traditional tabletop RPGs, though many have moved away from that nowadays, and in any case they still offered a way to reject the worst values or to assign them to attributes as you choose.

There are two big problems with random stats. The first is the player may end up with a terrible character who dies at the first encounter, barely able to swing a sword. The second is the player may end up with an incredible character, able to sweep past any hurdle without breaking a sweat. Then there is the very real temptation for a player to keep generating new characters until she gets one that is great at everything.