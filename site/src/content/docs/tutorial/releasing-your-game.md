---
title: Releasing your game
sidebar:
  order: 15
---

There are five stages to releasing a Quest Viva game.

1.  Before release testing
2.  Upload as an unlisted game
3.  Upload testing
4.  Public upload
5.  Announcement


## Before release testing

Before you even think about releasing your game, you need to thoroughly check to make sure it works properly, and that it has sensible responses for things that a player might reasonably type while playing it. To create a good game is a lot of hard work, and while it might be tempting to release your first efforts after a minimal amount of testing, your players won’t thank you for it.

And no matter how tempting it might be, do not even *think* about releasing the game you’ve created while working your way through this tutorial!

Here are some things to think about before unleashing your game on an unsuspecting public:

-   Think about all the objects a player might refer to – make sure everything you refer to in your descriptions is at least set up as a scenery object.
-   Think about all the different things a player might reasonably try to do with an object – set up verbs, even if they just tell the player that they can't do that.
-   Think about all the different ways a player might type a command, and make sure you have enough verb alternatives set up.
-   Think about the different ways a player might refer to the same thing, and set up alternative names.
-   Make sure you **test** your game thoroughly.
-   Spell check!




## Upload as an unlisted game

In the editor, open the **File** menu in the toolbar and choose **Publish…**, then pick **.quest file**. This builds a `.quest` package (your game file plus its assets) and downloads it.

On textadventures.co.uk, click on _Create_ at the top, then _Submit_ below that. Then follow the instructions. When it asks "Who can access this game?", choose "Only people I give the link to" for now. This makes the game unlisted, so only people you send the link to can find it.

For more on the Publish tool, including size limitations and what gets included in the .quest file, see [Publishing](/publishing/publishing).


## Upload testing

Now play the uploaded version of your game. It is a good idea to save, and to make sure a saved game can be loaded and looks okay, as saving and loading tend to be especially sensitive to errors in scripts!

Now get some other people to test it – you'll be surprised at all the things they pick up that you would never have thought of. This is called beta-testing, and while it can be a pain, especially as you are keen to get your game out there fast, it is well worth it in the long run. A couple of bugs in your game will quickly lead to bad reviews.


## Public upload

Once all the bugs are sorted, publish a fresh `.quest` file, go to your game's page on textadventures.co.uk, and use "Upload an updated game file" in the _Edit_ menu to upload it. Then choose "Edit this listing" from the same menu, check the game listing text is fine, and set who can access the game to "Everybody". Congratulations, your game is now live!


## Announcement

Now all you have to do is tell people about it! See [Publishing](/publishing/publishing) for a list of places you can announce your game.


## What next?

That's the end of the tutorial - you now know enough to write a complete game. Here are some things you'll probably want to learn about next, as your game needs them:

- [The text processor](/howto/world/text-processor) - vary descriptions with `{if}`, `{once}` and `{random}`, and add links to objects and commands, without writing any script.
- [Exits](/howto/world/exits) - including locked exits, and exits that run a script.
- [Asking the player](/howto/scripting/asking-the-player) - ask for typed input, a yes or no, or a choice from a menu.
- [Talking to characters](/howto/npcs/conversations) - Ask/Tell topics and [Pages](/tutorial/using-pages) for conversations.
- [Score, health and money](/howto/world/score-health-money) - the built-in versions of what we built by hand with status attributes.
- [Handling light and dark](/howto/world/handling-light-and-dark) - rooms the player needs a light source to see in.
- [Debugging your game](/howto/scripting/debugging-your-game) and [walkthroughs](/howto/scripting/using-walkthroughs) - check your game still works as it grows.

If you get stuck, ask on [Discord](https://textadventures.co.uk/community/discord) or in [GitHub Discussions](https://github.com/textadventures/quest/discussions).
