---
layout: index
title: When the Player Saves a Game
---

When a player saves a game, she saves _everything_.

Everything in Quest is an object with attributes, and potentially all those attributes could change. So Quest saves the lot. In effect, playing the game is like editing and saving your own version of it.

When the player later loads a saved game, Quest does not do anything with the original. It does not need to know anything about the original, because all the data, the entire game (in its modified state) was saved.

That works fine, until you update your game.

When the player now loads a saved game, Quest does not bother to look at the game itself. The player saved the old version (in whatever game state), and so that is what is loaded, and so the player is still using the old version.

Generally, this is not much of a problem; hopefully your original game was in a decent staste before you released it (you did beta-test, right?). For longer games, players may be disappinted that they need to start again to see new content.

Possibly the biggest problem is when you upload your latest version, you may be confused why the changes are not there. It may be because you are played a saved game; you need to restart the game to see the changes.


Are There Any Alternatives?
---------------------------

If you feel you absolutely must have player's saved games updated too, you have two approaches, neither of which will be easy to implement.

### Patching

One approach would be to upload a text file to a website with all your changes in it, and have a command, PATCH, that fetches the file, checks if the patch has already been installed, and if not, apply the changes.

The "apply the changes" bit is the difficult bit. You would need to devise a set of instructions for the text file that the PATCH command could process to create objects and exits and to update existing ones.

Your PATCH command would need to be in the original game.

You would need to think about how a player could do several patches.

As far as I know, you cannot convert a string to a script, so all your scripts would need to be in your original game (but you can add them to new objects).

I am not aware of anyone attempting this. Test well before release.


### Alternative Saving

An alternative approach is to have your own save system. To get this to work you would need to flag every attribute that can potentially change, and just save them. Then the player would restart the game, and use a new LOAD command, apply her saved file, and all those attributes get set.

There is a library available [here](https://github.com/ThePix/quest/wiki/Library:-Save-and-Load). 
