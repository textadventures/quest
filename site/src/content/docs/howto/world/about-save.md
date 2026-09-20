---
title: Updating a released game
description: What happens to players' saved games when you publish a new version, and how to release an update without breaking them
---

When you publish a fix or new content for a game people are already playing, anyone part-way through carries on with the version they started. This page explains why, what you can and can't change safely, and what to do about it.

## What a saved game contains

A saved game is not a list of changes since the start - it is a complete copy of your game as it stood at that moment. Everything in Quest Viva is an object with attributes, and any of them might have changed during play, so saving writes the lot: every room, object, command, function and library function, with its current values, plus the text already on screen. Even a one-room test game produces a save of around 350 KB, because the whole of the Core library goes into it too.

That means a save needs nothing from your published game to run. Loading one replaces the running game entirely.

The one exception is media. Pictures, sounds and other uploaded files aren't copied into the save - they're read from the `.quest` file that's open now. A `.quest` package is a zip containing `game.aslx` (your game with the Core library inlined), a `metadata.iFiction` file, and every media file you uploaded.

## What this means for an update

Publish version 2, and a player who resumes a version 1 save is still playing version 1. Your corrected typo, your new room, your fixed script: none of it is there, and there is no warning. They see the old version until they start a new game.

This catches authors out more often than players. If you upload a change, play your own game and can't see it, check whether you resumed a save rather than starting afresh.

What does reach an existing save is media, because that comes from the current package. Replacing `hall.jpg` with a better picture updates it for everyone, including saved games. The reverse is also true: deleting or renaming a media file breaks old saves that still ask for it, so leave retired files in place.

## Keep the game ID the same

Every game has an IFID - the "Game ID", under "Advanced" on the game's _Setup_ tab. Saved games are matched to it. If a player downloads a `.quest-save` and loads it into a build with a different IFID, the player refuses it: "This save is for a different game - open that game first, then try again."

So when you release an update, edit and re-publish the existing game file. Don't start a new game in the editor and paste your work into it - that generates a fresh IFID, and every save your players have made stops working.

Saves kept inside the player - the ones behind "Continue your game?" when a player reopens the game - are a separate matter: they're filed under where the game is served from, not its IFID. Publishing an update at the same address keeps them; renaming the file or moving the game to a new URL hides them, even though the game is the same.

## Releasing an update

- **Bump the version.** On the _Setup_ tab, update "Version" (the label players see) and "Version Code" (a number that only goes up). Both appear when the player types `VERSION`, and at the head of a [transcript](/howto/world/transcript), so a bug report can be tied to a build. See [Game details](/publishing/game-details).
- **Test before you release, not after.** A bug that ships is frozen into every save made against that build. Play through with the [debugger](/howto/scripting/debugging-your-game), and run a [walkthrough](/howto/scripting/using-walkthroughs) to check the main path still works.
- **Fix early.** The sooner after release a fix goes out, the fewer saves exist that will never see it.
- **Say so in your release notes.** Tell players a new game is needed to see what changed. Some will happily restart a short game; for a long one, they may prefer to finish first.
- **Don't try to patch a save from inside the game.** A save only ever runs the code it was saved with, so any patching mechanism would have to be complete and correct in the version the player already has - which is exactly the thing you can't rely on.

## See also

- [Publishing your game](/publishing/publishing) - creating the `.quest` file and putting it online
- [Game details](/publishing/game-details) - the version, version code and IFID
